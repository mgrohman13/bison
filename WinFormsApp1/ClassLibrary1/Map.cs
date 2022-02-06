using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace ClassLibrary1
{
    [Serializable]
    public class Map
    {
        public readonly Game Game;

        private readonly Noise noise;
        private readonly Direction _left, _right, _down, _up;

        private readonly Dictionary<Point, Piece> _pieces;
        private readonly HashSet<Point> _explored;

        static Map()
        {
            //static init to set NewTile
            Tile.GetDistance(0, 0, 0, 0);
        }

        internal Map(Game game)
        {
            this.Game = game;

            double max = Game.Rand.GaussianOE(130, .13, .13, 91);
            double min = Game.Rand.GaussianCapped(5.2, .039, 3.9);
            int steps = Game.Rand.GaussianOEInt(13, .13, .13, 8);
            double weightScale = Game.Rand.Weighted(.91);
            double weightPower = Game.Rand.GaussianOE(6.5, .13, .13, 3.9);
            this.noise = new Noise(Game.Rand, min, max, steps, .052, weightScale, weightPower);

            Tuple<double, double>[] enemyDirs = Game.Rand.Iterate(new Tuple<double, double>[] {
                new(1, 1),
                new(Game.Rand.GaussianCapped(1.69, .13,     1   ), Game.Rand.GaussianCapped( .65, .13 ,  .52)),
                new(Game.Rand.GaussianCapped(1.13, .13,      .52), Game.Rand.GaussianCapped(1.69, .13 , 1.3 )),
                new(Game.Rand.GaussianOE    (1   , .21, .26, .13), Game.Rand.GaussianCapped(1   , .169,  .65)),
            }).ToArray();
            this._left = new Direction(true, true, enemyDirs[0]);
            this._right = new Direction(true, false, enemyDirs[1]);
            this._up = new Direction(false, true, enemyDirs[2]);
            this._down = new Direction(false, false, enemyDirs[3]);

            this._pieces = new();
            this._explored = new();
        }

        [NonSerialized]
        private Dictionary<Point, double> evaluateCache;
        private double Evaluate(int x, int y)
        {
            Point p = new Point(x, y);
            if (evaluateCache == null)
                evaluateCache = new Dictionary<Point, double>();
            if (evaluateCache.TryGetValue(p, out double v))
                return v;
            static double Gradient(double coord, double center) => 2 / (1 + Math.Pow(Math.E, -.052 * (center - coord)));
            double xMult = Math.Min(Gradient(x, _right.Boundary), Gradient(-x, -_left.Boundary));
            double yMult = Math.Min(Gradient(y, _down.Boundary), Gradient(-y, -_up.Boundary));
            double mult = xMult * xMult + yMult * yMult;
            double value = noise.Evaluate(x, y) * mult;
            evaluateCache.Add(p, value);
            return value;
        }

        public Tile GetVisibleTile(Point p)
        {
            return GetVisibleTile(p.X, p.Y);
        }
        public Tile GetVisibleTile(int x, int y)
        {
            return Visible(x, y) ? GetTile(x, y) : null;
        }
        public IEnumerable<Piece> GetVisiblePieces()
        {
            return _pieces.Values.Where(p => p.Tile.Visible);
        }

        internal Tile GetTile(Point p)
        {
            return GetTile(p.X, p.Y);
        }
        internal Tile GetTile(int x, int y)
        {
            if (Evaluate(x, y) < .5)
                return null;
            Piece piece = GetPiece(x, y);
            return piece == null ? NewTile(this, x, y) : piece.Tile;
        }
        private Piece GetPiece(int x, int y)
        {
            _pieces.TryGetValue(new Point(x, y), out Piece piece);
            return piece;
        }

        public System.Drawing.Rectangle GameRect()
        {
            int x = _left.Explored;
            int y = _up.Explored;
            int w = _right.Explored;
            int h = _down.Explored;
            return new System.Drawing.Rectangle(x, y, w - x + 1, h - y + 1);
        }

        public bool Visible(Point tile)
        {
            return Visible(tile.X, tile.Y);
        }
        public bool Visible(int x, int y)
        {
            bool visible = _explored.Contains(new(x, y));
            //if (!visible)
            //    visible = _pieces.ContainsKey(new Point(x, y));
            return visible;
        }

        internal void AddPiece(Piece piece)
        {
            this._pieces.Add(new Point(piece.Tile.X, piece.Tile.Y), piece);

            if (piece is PlayerPiece playerPiece)
                UpdateVision(playerPiece);
        }
        internal void RemovePiece(Piece piece)
        {
            this._pieces.Remove(new Point(piece.Tile.X, piece.Tile.Y));
        }
        private void UpdateVision(PlayerPiece piece)
        {
            foreach (Point p in piece.Tile.GetPointsInRange(piece.Vision))
                this._explored.Add(p);
            foreach (Direction dir in new Direction[] { _left, _right, _up, _down })
                dir.Explore(piece.Tile, (int)piece.Vision);
            GenResources(piece.Tile, (int)piece.Vision);
        }

        internal Map.Tile StartTile()
        {
            Map.Tile tile;
            do
                tile = GetTile(Game.Rand.GaussianInt(Consts.MapCoordSize), Game.Rand.GaussianInt(Consts.MapCoordSize));
            while (InvalidStartTile(tile));
            return tile;
        }
        internal bool InvalidStartTile(Map.Tile tile)
        {
            return (tile == null || tile.Piece != null || tile.Visible || tile.GetDistance(Game.Player.Core.Tile) <= Game.Player.Core.GetBehavior<IRepair>().Range);
        }

        private void GenResources(Tile explored, int vision)
        {
            IEnumerable<Tile> resources = this._pieces.Values.Where(p => p is Extractor || p is Resource).Select(r => r.Tile);
            if (resources.Any())
            {
                var funcs = new Func<bool>[] {
                    () => GenResource(true, true, _left.Boundary, explored.X - vision, resources.Min(t => t.X), ref _left.ResourceNum),
                    () => GenResource(true, false, _right.Boundary, explored.X + vision, resources.Max(t => t.X), ref _right.ResourceNum),
                    () => GenResource(false, true, _up.Boundary, explored.Y - vision, resources.Min(t => t.Y), ref _up.ResourceNum),
                    () => GenResource(false, false, _down.Boundary, explored.Y + vision, resources.Max(t => t.Y), ref _down.ResourceNum),
                };
                bool generated = false;
                foreach (var Func in Game.Rand.Iterate(funcs))
                    generated |= Func();
                if (generated)
                    GenResources(explored, vision);
            }
        }
        private bool GenResource(bool dir, bool neg, double start, int explored, int min, ref int resourceNum)
        {
            if (explored == min || (explored < min) == neg)
            {
                if (neg)
                {
                    start *= -1;
                    min *= -1;
                }

                double avg = start + (resourceNum + .5) * Consts.ResourceAvgDist;
                int x = avg > min ? Game.Rand.GaussianOEInt(avg, 1, 1 / Math.Sqrt(avg), min) : min;
                x += Game.Rand.OEInt();
                int y = Game.Rand.GaussianInt(Consts.MapCoordSize);

                if (neg)
                {
                    x *= -1;
                    // flip back in case we recurse
                    start *= -1;
                    min *= -1;
                }
                if (!dir)
                {
                    int t = y;
                    y = x;
                    x = t;
                }

                Tile tile = GetTile(x, y);
                if (tile == null || tile.Piece != null)
                {
                    return GenResource(dir, neg, start, explored, min, ref resourceNum);
                }
                else
                {
                    switch (Game.Rand.Next(11))
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            Biomass.NewBiomass(tile);
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            Metal.NewMetal(tile);
                            break;
                        case 9:
                        case 10:
                            Artifact.NewArtifact(tile);
                            break;
                    }

                    if (Game.Rand.Bool())
                    {
                        Tile t2;
                        do
                            t2 = GetTile(tile.X + Game.Rand.GaussianInt(Consts.ResourceAvgDist), tile.Y + Game.Rand.GaussianInt(Consts.ResourceAvgDist));
                        while (InvalidStartTile(t2));
                        Foundation.NewFoundation(t2);
                    }

                    resourceNum++;
                    return true;
                }
            }
            return false;
        }

        internal Tile GetEnemyTile()
        {
            double GetExplored(Direction dir)
            {
                int GetBound(double b) => Game.Rand.GaussianCappedInt(Math.Abs(b * 1.17), .13, 0);
                int a = -GetBound(dir.IsX ? _up.Boundary : _left.Boundary);
                int b = GetBound(dir.IsX ? _down.Boundary : _right.Boundary);
                double max = 0, avg = 0;
                for (int c = a; c <= b; c++)
                {
                    var e = _explored.Where(p => (dir.IsX ? p.Y : p.X) == c).Select(dir.GetCoord);
                    if (e.Any())
                    {
                        int f = dir.Neg ? e.Min() : e.Max();
                        max = ((Func<double, double, double>)(dir.Neg ? Math.Min : Math.Max))(max, f);
                        avg += f;
                    }
                    else
                        ;
                }
                avg /= (b - a + 1);
                return (max + avg) / 2;
            }
            int GetChance(Direction dir)
            {
                double main = dir.EnemyMult * dir.EnemyMult * (13 + Math.Pow(Game.Turn / 2.1, dir.EnemyPow));
                double exp = Math.Abs(GetExplored(dir)) / 1.3;
                return Game.Rand.Round(Math.Sqrt(39 + main + exp));
            };

            Direction dir = Game.Rand.SelectValue(new Direction[] { _left, _right, _up, _down }, GetChance);

            Map.Tile tile;
            do
            {
                int x = Game.Rand.GaussianInt(Consts.MapCoordSize);
                int y = Game.Rand.Round(GetExplored(dir)) + Game.Rand.GaussianOEInt(Game.Rand.Range(2.1, 16.9), .39, .26, 1) * dir.Sign();
                if (dir.IsX)
                {
                    int t = y;
                    y = x;
                    x = t;
                }
                tile = GetTile(x, y);
            }
            while (InvalidStartTile(tile));
            return tile;
        }

        private static Func<Map, int, int, Tile> NewTile;
        [Serializable]
        public class Tile
        {
            public readonly Map Map;
            public readonly int X, Y;
            public Piece Piece => Map.GetPiece(X, Y);
            public bool Visible => Map.Visible(X, Y);

            static Tile()
            {
                NewTile = (map, x, y) => new Tile(map, x, y);
            }
            private Tile(Map map, int x, int y)
            {
                this.Map = map;
                this.X = x;
                this.Y = y;
            }

            public double GetDistance(Point other)
            {
                return GetDistance(other.X, other.Y);
            }
            public double GetDistance(Tile other)
            {
                return GetDistance(other.X, other.Y);
            }
            public double GetDistance(int x, int y)
            {
                return GetDistance(this.X, this.Y, x, y);
            }
            public static double GetDistance(int x1, int y1, int x2, int y2)
            {
                double xDiff = x1 - x2;
                double yDiff = y1 - y2;
                //if (1 == y1 - y2 % 2)
                //    xDiff = (xDiff) + .5;
                //yDiff *= Math.Sqrt(3) / 2.0;
                return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            }

            public IEnumerable<Tile> GetVisibleTilesInRange(double range)
            {
                return GetVisibleTilesInRange(range, false, null);
            }
            internal IEnumerable<Tile> GetTilesInRange(double range)
            {
                return GetTilesInRange(range, false, null);
            }
            public IEnumerable<Point> GetPointsInRange(double range)
            {
                return GetPointsInRange(range, false, null);
            }
            public IEnumerable<Point> GetPointsInRange(IMovable movable)
            {
                return GetPointsInRange(movable.MoveCur, true, movable.Piece);
            }

            public IEnumerable<Tile> GetVisibleTilesInRange(double range, bool blockMap, Piece blockFor)
            {
                return GetPointsInRange(range, blockMap, blockFor).Where(Map.Visible).Select(Map.GetTile).Where(t => t != null);
            }
            internal IEnumerable<Tile> GetTilesInRange(double range, bool blockMap, Piece blockFor)
            {
                return GetPointsInRange(range, blockMap, blockFor).Select(Map.GetTile).Where(t => t != null);
            }
            public IEnumerable<Point> GetPointsInRange(double range, bool blockMap, Piece blockFor)
            {
                //double blockDist = Math.Sqrt(2) / 2;

                //IEnumerable<Point> block = Array.Empty<Point>();
                //if (blockMap)
                //    block = block.Concat(GetPointsInRange(range, false, null)).Where(p => Map.GetTile(p) == null);
                //if (blockFor != null)
                //    block = block.Concat(Map._pieces.Where(p => p.Value != blockFor &&
                //       (p.Value.Side != blockFor.Side || !p.Value.HasBehavior<IMovable>())).Select(p => p.Key))
                //        .Where(p => GetDistance(p) <= range);

                int max = (int)range + 1;
                for (int a = -max; a <= max; a++)
                {
                    int x = X + a;
                    for (int b = -max; b <= max; b++)
                    {
                        int y = Y + b;
                        if (GetDistance(x, y) <= range)
                        {
                            //if (!block.Any(p => (p.X != x || p.Y != y) && PointLineDist(this, x, y, p) <= blockDist))
                            yield return new(x, y);
                        }
                    }
                }
            }
            //private double PointLineDist(Tile tile, int x, int y, Point p)
            //{
            //    if (tile.X == x)
            //        return Math.Abs(p.Y - y);
            //    double a = (tile.Y - y) / (tile.X - x);
            //    double b = -1;
            //    double c = y - a * x;
            //    return Math.Abs(a * p.X + b * p.Y + c) / Math.Sqrt(a * a + b * b);
            //}

            public static bool operator !=(Tile a, Tile b)
            {
                return !(a == b);
            }
            public static bool operator ==(Tile a, Tile b)
            {
                return (a is null ? b is null : a.Equals(b));
            }

            public override bool Equals(object obj)
            {
                Tile other = obj as Tile;
                return other != null && this.X == other.X && this.Y == other.Y;
            }

            public override int GetHashCode()
            {
                return X * ushort.MaxValue + Y;
            }

            public override string ToString()
            {
                return string.Format("({0}, {1})", X, Y);
            }
        }

        [Serializable]
        private class Direction
        {
            public readonly bool IsX, Neg;
            public readonly double Boundary;

            public readonly double EnemyMult, EnemyPow;

            public int ResourceNum, Explored;

            public Direction(bool isX, bool neg, Tuple<double, double> enemies)
            {
                this.IsX = isX;
                this.Neg = neg;

                this.Boundary = Game.Rand.GaussianCapped(Consts.MapCoordSize, Consts.MapDev, Consts.MinMapCoord);
                this.Boundary *= Sign();

                this.EnemyMult = enemies.Item1;
                this.EnemyPow = Math.Sqrt(enemies.Item2);

                this.ResourceNum = 0;
                this.Explored = 0;
            }

            public int Sign()
            {
                return (Neg ? -1 : 1);
            }
            public int GetCoord(Tile tile)
            {
                return GetCoord(new Point(tile.X, tile.Y));
            }
            public int GetCoord(Point point)
            {
                return IsX ? point.X : point.Y;
            }

            public void Explore(Tile tile, int vision)
            {
                Func<int, int, int> F = Neg ? Math.Min : Math.Max;
                Explored = F(Explored, GetCoord(tile) + vision * Sign());
            }
        }
    }
}
