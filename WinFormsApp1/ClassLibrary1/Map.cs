using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1
{
    [Serializable]
    public class Map
    {
        private readonly Dictionary<Point, Piece> _pieces;

        public readonly Game Game;
        public readonly int left, right, down, up;

        private readonly int[] _exploredLeft, _exploredRight, _exploredDown, _exploredUp;
        private int _resourceLeft, _resourceRight, _resourceDown, _resourceUp;

        public int Width => right - left + 1;
        public int Height => up - down + 1;

        static Map()
        {
            //static init to set NewTile
            Tile.GetDistance(0, 0, 0, 0);
        }

        internal Map(Game game)
        {
            this._pieces = new Dictionary<Point, Piece>();
            this.Game = game;

            static int GetCoord() => Game.Rand.GaussianCappedInt(Consts.MapCoordSize, Consts.MapDev, Consts.MinMapCoord);
            left = -GetCoord();
            right = GetCoord();
            down = -GetCoord();
            up = GetCoord();

            _exploredLeft = new int[Height];
            _exploredRight = new int[Height];
            _exploredDown = new int[Width];
            _exploredUp = new int[Width];
            for (int a = 0; a < Height; a++)
            {
                _exploredLeft[a] = 1;
                _exploredRight[a] = -1;
            }
            for (int b = 0; b < Width; b++)
            {
                _exploredDown[b] = 1;
                _exploredUp[b] = -1;
            }

            _resourceLeft = 1;
            _resourceRight = 1;
            _resourceDown = 1;
            _resourceUp = 1;
        }

        internal void OnDeserialization()
        {
            foreach (KeyValuePair<Point, Piece> pair in _pieces)
                pair.Value.SetTile(NewTile(this, pair.Key.X, pair.Key.Y));
        }

        public Tile GetVisibleTile(int x, int y)
        {
            Tile tile = GetTile(x, y);
            if (tile != null && tile.Visible)
                return tile;
            return null;
        }
        internal Tile GetTile(int x, int y)
        {
            if (!CheckX(x) && !CheckY(y))
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
            int x = _exploredLeft.Min();
            int y = _exploredDown.Min();
            int w = _exploredRight.Max();
            int h = _exploredUp.Max();
            for (int a = left; a <= right; a++)
                if (_exploredDown[a - left] <= 0 || _exploredUp[a - left] >= 0)
                {
                    x = Math.Min(x, a);
                    w = Math.Max(w, a);
                }
            for (int b = down; b <= up; b++)
                if (_exploredLeft[b - down] <= 0 || _exploredRight[b - down] >= 0)
                {
                    y = Math.Min(y, b);
                    h = Math.Max(h, b);
                }
            return new System.Drawing.Rectangle(x, y, w - x + 1, h - y + 1);
        }

        public bool Visible(Tile tile)
        {
            return Visible(tile.X, tile.Y);
        }
        public bool Visible(int x, int y)
        {
            bool visible = false;

            bool xIn = CheckX(x);
            bool yIn = CheckY(y);
            if (xIn || yIn)
            {
                bool expX = yIn && (x >= _exploredLeft[y - down] && x <= _exploredRight[y - down]);
                bool expY = xIn && (y >= _exploredDown[x - left] && y <= _exploredUp[x - left]);

                if (xIn && yIn)
                    visible = expX || expY;
                else
                    visible = (!yIn || expX) && (!xIn || expY);

                if (!visible)
                {
                    Tile tile = GetTile(x, y);
                    visible = (tile != null && tile.Piece is Resource);
                }
            }

            return visible;
        }

        private bool CheckX(int x)
        {
            return !(x < left || x > right);
        }
        private bool CheckY(int y)
        {
            return !(y < down || y > up);
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
            UpdateVision(left, right, piece.Tile.X, piece.Tile.Y, _exploredDown, _exploredUp, piece.Vision);
            UpdateVision(down, up, piece.Tile.Y, piece.Tile.X, _exploredLeft, _exploredRight, piece.Vision);
            GenResources();
        }
        private static void UpdateVision(int start, int end, int coord, int other, int[] expNeg, int[] expPos, double vision)
        {
            vision *= vision;
            for (int a = start; a <= end; a++)
            {
                int dist = a - coord;
                dist *= dist;
                if (vision >= dist)
                {
                    int range = (int)Math.Sqrt(vision - dist);
                    expNeg[a - start] = Math.Min(expNeg[a - start], other - range);
                    expPos[a - start] = Math.Max(expPos[a - start], other + range);
                }
            }
        }

        private void GenResources()
        {
            IEnumerable<Tile> resources = this._pieces.Values.Select(p => p is Extractor extractor ? extractor.Resource : p).OfType<Resource>().Select(r => r.Tile);
            if (resources.Any())
            {
                int minLeft = resources.Min(t => t.X);
                if (_exploredLeft.Min() < minLeft)
                    GenResource(true, true, left, minLeft, ref _resourceLeft, Game.Rand.RangeInt(down, up));
                int minRight = resources.Max(t => t.X);
                if (_exploredRight.Max() > minRight)
                    GenResource(true, false, right, minRight, ref _resourceRight, Game.Rand.RangeInt(down, up));
                int minDown = resources.Min(t => t.Y);
                if (_exploredDown.Min() < minDown)
                    GenResource(false, true, down, minDown, ref _resourceDown, Game.Rand.RangeInt(left, right));
                int minUp = resources.Max(t => t.Y);
                if (_exploredUp.Max() > minUp)
                    GenResource(false, false, up, minUp, ref _resourceUp, Game.Rand.RangeInt(left, right));
            }
        }
        private void GenResource(bool dir, bool neg, int start, int min, ref int resourceNum, int other)
        {
            if (neg)
            {
                start *= -1;
                min *= -1;
            }

            double avg = start + resourceNum * Consts.ResourceAvgDist;
            min++;
            int x = avg > min ? Game.Rand.GaussianOEInt(avg, 1, 1 / Math.Sqrt(avg), min) : min;
            x += Game.Rand.OEInt();
            int y = other;

            if (neg)
                x *= -1;
            if (!dir)
            {
                int t = y;
                y = x;
                x = t;
            }

            Tile tile = GetTile(x, y);
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
            resourceNum++;
        }

        internal Tile GetEnemyTile()
        {
            Map.Tile tile;
            do
            {
                int[] exp;
                bool dir, neg;
                int sel = Game.Rand.Next(4);
                switch (sel)
                {
                    case 0:
                        exp = _exploredDown;
                        dir = false;
                        neg = true;
                        break;
                    case 1:
                        exp = _exploredLeft;
                        dir = true;
                        neg = true;
                        break;
                    case 2:
                        exp = _exploredRight;
                        dir = true;
                        neg = false;
                        break;
                    case 3:
                        exp = _exploredUp;
                        dir = false;
                        neg = false;
                        break;
                    default: throw new Exception();
                }

                int x = Game.Rand.Next(exp.Length);
                int y = exp[x];
                x += dir ? down : left;
                int dist = Game.Rand.GaussianOEInt(Game.Rand.Range(1, 13), .39, .26, 1);
                if (neg)
                    dist = -dist;
                y += dist;
                if (dir)
                {
                    int t = y;
                    y = x;
                    x = t;
                }

                tile = GetTile(x, y);
            }
            while (tile == null || tile.Piece != null);
            return tile;
        }

        private static Func<Map, int, int, Tile> NewTile;
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
                return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            }

            public IEnumerable<Tile> GetTilesInRange(double range)
            {
                int max = (int)range + 1;
                for (int a = -max; a <= max; a++)
                {
                    int x = X + a;
                    for (int b = -max; b <= max; b++)
                    {
                        int y = Y + b;
                        if (GetDistance(x, y) <= range)
                        {
                            Tile tile = Map.GetTile(x, y);
                            if (tile != null)
                                yield return tile;
                        }
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
        }
    }
}
