using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Point = MattUtil.Point;

namespace ClassLibrary1
{
    [Serializable]
    public class Map
    {
        private const double TWO_PI = Math.PI * 2;
        private const double HALF_PI = Math.PI / 2.0;

        public readonly Game Game;

        private readonly Noise noise;
        private readonly Path[] _paths;

        private readonly Dictionary<Point, Piece> _pieces;
        private readonly HashSet<Point> _explored;
        private Rectangle _gameBounds;

        private readonly RandBooleans[] _resourceRands;
        private readonly RandBooleans _foundationRand;

        static Map()
        {
            //static init to set NewTile
            Tile.GetDistance(0, 0, 0, 0);
        }

        internal Map(Game game)
        {
            this.Game = game;

            double max = Game.Rand.GaussianOE(130, .091, .13, 65);
            double min = Game.Rand.GaussianCapped(5.2, .065, 2.6);
            int steps = Game.Rand.GaussianOEInt(6.5, .13, .13, 4);
            double weightScale = Game.Rand.Weighted(.78) + Game.Rand.OE(.13);
            //double weightPower = Game.Rand.GaussianOE(6.5, .13, .13, 3.9);
            this.noise = new Noise(Game.Rand, min, max, steps, .052, weightScale);//, weightPower);

            //Tuple<double, double>[] enemyDirs = Game.Rand.Iterate(new Tuple<double, double>[] {
            //    new(1, 1),
            //    new(Game.Rand.GaussianCapped(1.69, .13,     1   ), Game.Rand.GaussianCapped( .65, .13 ,  .52)),
            //    new(Game.Rand.GaussianCapped(1.13, .13,      .52), Game.Rand.GaussianCapped(1.69, .13 , 1.3 )),
            //    new(Game.Rand.GaussianOE    (1   , .21, .26, .13), Game.Rand.GaussianCapped(1   , .169,  .65)),
            //}).ToArray();

            int numPaths = Game.Rand.GaussianOEInt(Math.PI, .091, .039, 2);
            double separation = Consts.PathMinSeparation;
            separation = Game.Rand.GaussianCapped(separation, .104, Math.Max(0, 2 * separation - TWO_PI)) / numPaths;
            double[] angles;
            bool valid;
            do
            {
                angles = Enumerable.Repeat(0, numPaths).Select(x => Game.Rand.NextDouble() * TWO_PI).ToArray();
                valid = true;
                for (int a = 0; valid && a < numPaths - 1; a++)
                    for (int b = a; valid && ++b < numPaths;)
                    {
                        double check = GetAngleDiff(angles[a], angles[b]);
                        if (check < separation)
                            valid = false;
                    }
            } while (!valid);

            double[] enemyPowers = angles.Select(x => 1 + Game.Rand.OE(1 / 13.0)).ToArray();
            static double Increase(double x) => 1.3 + (x - 1) * 1.69;
            enemyPowers[0] = Increase(enemyPowers[0]);
            enemyPowers[1] = Increase(enemyPowers[1]);
            enemyPowers = enemyPowers.Select(x => x > 2 ? 1 + Math.Sqrt(x - 1) : x).ToArray();
            enemyPowers[1] = 1 / enemyPowers[1];
            for (int a = 2; a < numPaths; a++)
                if (Game.Rand.Bool())
                    enemyPowers[a] = 1 / enemyPowers[a];
            Game.Rand.Shuffle(enemyPowers);

            this._paths = new Path[numPaths];
            for (int a = 0; a < numPaths; a++)
            {
                double enemyPow = enemyPowers[a];
                double enemyMult = 1 + Game.Rand.OE(1 / (enemyPow > 1 ? enemyPow : 1 / enemyPow));
                if (Game.Rand.Bool())
                    enemyMult = 1 / enemyMult;
                this._paths[a] = new Path(angles[a], enemyMult / enemyPow, enemyPow);
            }

            this._pieces = new();
            this._explored = new();

            _resourceRands = new[] {
                new RandBooleans(Game.Rand, .26), //artifact
                new RandBooleans(Game.Rand, .52), //biomass
                new RandBooleans(Game.Rand, .65), //metal
            };
            _foundationRand = new(.39);
        }

        internal void GenerateStartResources()
        {
            for (int a = 0; a < 1; a++)
                Biomass.NewBiomass(StartTile());
            for (int b = 0; b < 2; b++)
                Artifact.NewArtifact(StartTile());
            for (int c = 0; c < 3; c++)
                Metal.NewMetal(StartTile());

            foreach (var explore in Game.Rand.Iterate(_paths))
                GenResources(explore.Explore(this, Consts.PathWidth));
        }

        [NonSerialized]
        private Dictionary<Point, double> evaluateCache;
        private double Evaluate(int x, int y)
        {
            Point p = new(x, y);
            evaluateCache ??= new Dictionary<Point, double>();
            if (evaluateCache.TryGetValue(p, out double v))
                return v;

            double mult = 0;
            foreach (var path in _paths)
            {
                double Gradient(PointD point, int sign)
                {
                    double backMult = 1;
                    double angle = GetAngle(x - point.X, y - point.Y);
                    if (GetAngleDiff(path.Angle, angle) > HALF_PI)
                    {
                        static double GetDistSqr(int x, int y, PointD point)
                        {
                            double distX = point.X - x, distY = point.Y - y;
                            return distX * distX + distY * distY;
                        }
                        double distSqr = Math.Min(GetDistSqr(x, y, path.Left), GetDistSqr(x, y, path.Right));
                        backMult = Math.Min(1, Consts.PathWidth * Consts.PathWidth / distSqr);
                    }

                    double lineDist = PointLineDistanceSigned(path, point, new Point(x, y)) * sign;

                    return 2 / (1 + Math.Pow(Math.E, -.065 * lineDist)) * backMult;
                }

                double m = Math.Min(Gradient(path.Left, 1), Gradient(path.Right, -1));
                mult += m * m;
            }

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

        public Rectangle GameRect()
        {
            if (Game.TEST_MAP_GEN.HasValue)
            {
                int v = Game.TEST_MAP_GEN.Value;
                return new Rectangle(-v, -v, v * 2, v * 2);
            }

            return _gameBounds;
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
            return Game.TEST_MAP_GEN.HasValue || visible;
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
            Tile tile = piece.Tile;
            foreach (Point p in tile.GetPointsInRangeUnblocked(piece.Vision))
                this._explored.Add(p);

            int vision = (int)piece.Vision;
            int x = Math.Min(_gameBounds.X, piece.Tile.X - vision - 1);
            int y = Math.Min(_gameBounds.Y, piece.Tile.Y - vision - 1);
            int right = Math.Max(_gameBounds.Right, piece.Tile.X + vision + 2);
            int bottom = Math.Max(_gameBounds.Bottom, piece.Tile.Y + vision + 2);

            _gameBounds = new Rectangle(x, y, right - x, bottom - y);

            if (piece is not Core)
                Explore(tile, piece.Vision);
        }

        internal Tile StartTile()
        {
            Tile tile;
            do
                tile = GetTile(Game.Rand.GaussianInt(Consts.PathWidth + Consts.ResourceAvgDist), Game.Rand.GaussianInt(Consts.PathWidth + Consts.ResourceAvgDist));
            while (InvalidStartTile(tile));
            return tile;
        }
        internal static bool InvalidStartTile(Tile tile)
        {
            if (tile == null) return true;

            bool visible = tile.Visible && !Game.TEST_MAP_GEN.HasValue;
            Core core = tile.Map.Game.Player.Core;
            bool inCoreRange = core != null && tile.GetDistance(core.Tile) <= core.GetBehavior<IRepair>().Range;
            return (visible || tile.Piece != null || inCoreRange);
        }

        private void Explore(Tile tile, double vision)
        {
            if (tile.X != 0 || tile.Y != 0)
            {
                double angle = GetAngle(tile.X, tile.Y);
                Path explore = Game.Rand.Iterate(_paths).OrderBy(path => GetAngleDiff(path.Angle, angle)).First();
                GenResources(explore.Explore(tile, vision));
            }
        }
        private void GenResources(IEnumerable<Tile> tiles)
        {
            foreach (Tile resource in Game.Rand.Iterate(tiles))
            {
                GenResourceType(resource);

                if (_foundationRand.GetResult())
                {
                    Tile foundation;
                    do
                        foundation = GetTile(resource.X + Game.Rand.GaussianInt(Consts.ResourceAvgDist), resource.Y + Game.Rand.GaussianInt(Consts.ResourceAvgDist));
                    while (InvalidStartTile(foundation));
                    Foundation.NewFoundation(foundation);
                }
            }
        }
        private void GenResourceType(Tile tile)
        {
            bool[] resources;
            int count;
            do
            {
                resources = _resourceRands.Select(r => r.GetResult()).ToArray();
                count = resources.Count(b => b);
            }
            while (count == 0);

            int select;
            if (count == 1)
                select = Array.IndexOf(resources, true);
            else
                do
                    select = Game.Rand.Next(3);
                while (!resources[select]);

            switch (select)
            {
                case 0:
                    Artifact.NewArtifact(tile);
                    break;
                case 1:
                    Biomass.NewBiomass(tile);
                    break;
                case 2:
                    Metal.NewMetal(tile);
                    break;
                default:
                    throw new Exception();
            }
        }

        internal Tile GetEnemyTile()
        {
            int GetChance(Path path)
            {
                double main = path.EnemyMult * path.EnemyMult * (13 + Math.Pow(Game.Turn / 2.1, path.EnemyPow));
                double exp = Math.Abs(path.ExploredDist) / 1.3;
                return Game.Rand.Round(Math.Sqrt(39 + main + exp));
            };
            Path path = Game.Rand.SelectValue(_paths, GetChance);

            return path.SpawnTile(this, path.ExploredDist, Consts.PathWidth * 1.69);
        }

        private static double GetAngle(double x, double y)
        {
            return Math.Atan2(y, x);
        }
        private static PointD GetPoint(double angle, double dist)
        {
            return new(Math.Cos(angle) * dist, Math.Sin(angle) * dist);
        }

        private static double GetAngleDiff(double a, double b)
        {
            double check = Math.Abs(a - b) % TWO_PI;
            if (check > Math.PI)
                check = TWO_PI - check;
            return check;
        }

        //the sign indicates which side of the line the point is on
        private static double PointLineDistanceSigned(Path path, PointD linePoint, Point point)
        {
            path.CalcLine(linePoint, out double a, out double b, out double c);

            double dist = PointLineDistance(a, b, c, point);
            if (GetAngleDiff(path.Angle, Math.PI) < HALF_PI)
                dist *= -1;
            return dist;
        }
        private static double PointLineDistanceAbs(Point segment1, Point segment2, Point point)
        {
            if (segment2.X == segment1.X)
                return Math.Abs(point.X - segment1.X);

            //merge with CalcLine?
            double a = (segment2.Y - segment1.Y) / (segment2.X - segment1.X);
            double b = -1;
            double c = segment1.Y - a * segment1.X;
            return PointLineDistanceAbs(a, b, c, point);
        }
        private static double PointLineDistanceAbs(double a, double b, double c, Point point) =>
            Math.Abs(PointLineDistance(a, b, c, point));
        private static double PointLineDistance(double a, double b, double c, Point point) =>
            (a * point.X + b * point.Y + c) / Math.Sqrt(a * a + b * b);

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

            //support blocking
            public double GetDistance(Point other) => GetDistance(other.X, other.Y);
            public double GetDistance(Tile other) => GetDistance(other.X, other.Y);
            public double GetDistance(int x, int y) => GetDistance(this.X, this.Y, x, y);
            public static double GetDistance(Point p1, Point p2) => GetDistance(p1.X, p1.Y, p2.X, p2.Y);
            public static double GetDistance(int x1, int y1, int x2, int y2)
            {
                double xDiff = x1 - x2;
                double yDiff = y1 - y2;
                //if (1 == y1 - y2 % 2)
                //    xDiff = (xDiff) + .5;
                //yDiff *= Math.Sqrt(3) / 2.0;
                return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            }

            public IEnumerable<Tile> GetVisibleTilesInRange(IBuilder builder) => GetVisibleTilesInRange(builder.Range, true, null);
            public IEnumerable<Tile> GetVisibleTilesInRange(Attack attack) => GetVisibleTilesInRange(attack.Range, true, attack.Piece);
            private IEnumerable<Tile> GetVisibleTilesInRange(double range, bool blockMap, Piece blockFor) => GetPointsInRange(range, blockMap, blockFor)
                .Where(Map.Visible).Select(Map.GetTile).Where(t => t != null);

            internal IEnumerable<Tile> GetTilesInRange(IMovable movable) => GetTilesInRange(movable.MoveCur, false, movable.Piece);
            internal IEnumerable<Tile> GetTilesInRange(double range, bool blockMap, Piece blockFor) => GetPointsInRange(range, blockMap, blockFor)
                .Select(Map.GetTile).Where(t => t != null);

            internal IEnumerable<Point> GetPointsInRangeUnblocked(double vision) => GetPointsInRange(vision, false, null);
            internal static IEnumerable<Point> GetPointsInRangeUnblocked(Map map, Point point, double range) => GetPointsInRange(map, point, range, false, null);

            public IEnumerable<Point> GetPointsInRange(IMovable movable) => GetPointsInRange(movable, movable.MoveCur);
            public IEnumerable<Point> GetPointsInRange(IMovable movable, double move) => GetPointsInRange(move, false, movable.Piece);
            public IEnumerable<Point> GetPointsInRange(IBuilder builder) => GetPointsInRange(builder.Range, true, null);
            public IEnumerable<Point> GetPointsInRange(Attack attack) => GetPointsInRange(attack.Range, true, attack.Piece);
            private IEnumerable<Point> GetPointsInRange(double range, bool blockMap, Piece blockFor) => GetPointsInRange(Map, new Point(X, Y), range, blockMap, blockFor);
            private static IEnumerable<Point> GetPointsInRange(Map map, Point point, double range, bool blockMap, Piece blockFor)
            {
                //Dictionary<Point, double> block = new();

                //double sqrtTwo = Math.Sqrt(2);
                //double baseBlock = .5 + (sqrtTwo / 2.0 - .5) / 2.0;
                //double enemyBlock = 1 + (sqrtTwo - 1) / 2.0;
                //void AddBlock(Point b, double blockRange)
                //{
                //    block.TryGetValue(b, out double range);
                //    range = Math.Max(range, blockRange);
                //    block[b] = range;
                //}
                //if (blockMap)
                //    foreach (var p in GetPointsInRangeUnblocked(map, point, range).Where(p => map.GetTile(p) == null))
                //        AddBlock(p, baseBlock);
                //if (blockFor != null)
                //    foreach (var pair in map._pieces.Where(p => p.Value != blockFor
                //            && (p.Value.Side != blockFor.Side || !p.Value.HasBehavior<IMovable>())
                //            && GetDistance(point, p.Key) <= range))
                //        AddBlock(pair.Key, pair.Value.Side != null && pair.Value.Side != blockFor.Side ? enemyBlock : baseBlock);

                int max = (int)range + 1;
                foreach (Point p in Game.Rand.Iterate(-max, max, -max, max))
                {
                    int x = point.X + p.X;
                    int y = point.Y + p.Y;
                    double distance = GetDistance(point.X, point.Y, x, y);
                    if (distance <= range)
                    {
                        //if (!block.Any(p => GetDistance(point, p.Key) < distance
                        //       && PointLineDistanceAbs(point, new(x, y), p.Key) < p.Value
                        //       && (GetAngleDiff(GetAngle(p.Key.X - point.X, p.Key.Y - point.Y), GetAngle(x - point.X, y - point.Y)) < HALF_PI)))
                        yield return new(x, y);
                    }
                }
            }

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
        private class Path
        {
            public readonly double Angle;
            public readonly PointD Left, Right;

            public readonly double EnemyMult, EnemyPow;

            public int ResourceNum { get; private set; }
            public double ExploredDist { get; private set; }
            public double NextResourceDist { get; private set; }

            public Path(double angle, double enemyMult, double enemyPow)
            {
                this.Angle = angle;
                this.EnemyMult = enemyMult;
                this.EnemyPow = enemyPow;

                static double Dist() => Game.Rand.GaussianCapped(Consts.PathWidth, Consts.PathWidthDev, Consts.PathWidthMin);
                PointD GetOrigin(int sign)
                {
                    double dist = Dist();
                    double dir = angle + HALF_PI * sign;
                    return GetPoint(dir, dist);
                }
                this.Left = GetOrigin(1);
                this.Right = GetOrigin(-1);

                //Debug.WriteLine(Left);
                //Debug.WriteLine(Right);

                this.ResourceNum = 0;
                this.ExploredDist = 0;
                this.NextResourceDist = 0;
                GetNextDist();
            }
            private void GetNextDist()
            {
                double avg = ResourceNum * Consts.ResourceAvgDist + Consts.PathWidth;
                double inc = Math.Max(avg - NextResourceDist, 0) + Consts.ResourceAvgDist / 2.0;
                double oe = Math.Min(inc, Consts.ResourceAvgDist);
                inc -= oe;
                NextResourceDist += Game.Rand.DoubleFull(inc) + Game.Rand.OE(oe);
            }

            public HashSet<Tile> Explore(Tile tile, double vision)
            {
                return Explore(tile.Map, GetExploredDist(tile, vision));
            }
            public HashSet<Tile> Explore(Map map, double dist)
            {
                ExploredDist = Math.Max(ExploredDist, dist);

                HashSet<Tile> tiles = new();
                foreach (double distance in Game.Rand.Iterate(CreateResources()))
                    tiles.Add(SpawnTile(map, distance, Consts.PathWidth));
                return tiles;
            }
            private double GetExploredDist(Tile tile, double vision)
            {
                double x = tile.X, y = tile.Y;
                CalcLine(new PointD(0, 0), out double a, out double b, out double c);

                double div = a * a + b * b;
                double lineX = (b * (+b * x - a * y) - a * c) / div;
                double lineY = (a * (-b * x + a * y) - b * c) / div;
                double path = GetAngle(lineX, lineY);
                //check can be againsts any arbitrarily small value - angle will either be equal or opposite
                if (GetAngleDiff(path, Angle) < HALF_PI)
                    return Math.Sqrt(lineX * lineX + lineY * lineY) + vision;
                return -1;
            }
            private IEnumerable<double> CreateResources()
            {
                const double generationBuffer = 2.1 * (Consts.PathWidth + Consts.ResourceAvgDist);

                List<double> create = new();
                while (ExploredDist + generationBuffer > NextResourceDist)
                {
                    create.Add(NextResourceDist);
                    ResourceNum++;
                    GetNextDist();
                }
                return create;
            }

            public Tile SpawnTile(Map map, double distance, double deviation)
            {
                PointD spawnCenter = GetPoint(Angle, distance);
                int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(deviation));
                Tile tile;
                do
                    tile = map.GetTile(RandCoord(spawnCenter.X), RandCoord(spawnCenter.Y));
                while (InvalidStartTile(tile));

                Debug.WriteLine($"SpawnTile ({Angle:0.00}) {distance:0.0}: {spawnCenter} -> {tile}");

                return tile;
            }

            //calculates the line equation in the format ax + by + c = 0 
            public void CalcLine(PointD start, out double a, out double b, out double c)
            {
                a = Math.Tan(Angle);
                b = -1;
                c = start.Y - (a * start.X);
            }
        }
    }
}
