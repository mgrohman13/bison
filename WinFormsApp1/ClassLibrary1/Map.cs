using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1
{
    [Serializable]
    public class Map
    {
        private readonly Dictionary<Point, Piece> _pieces;

        public readonly Game Game;
        public readonly int left, right, down, up;

        private readonly int[] _exploredLeft, _exploredRight, _exploredDown, _exploredUp;

        public int Width => right - left + 1;
        public int Height => up - down + 1;

        static Map()
        {
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
        }

        internal void OnDeserialization()
        {
            foreach (KeyValuePair<Point, Piece> pair in _pieces)
                pair.Value.SetTile(NewTile(this, pair.Key.X, pair.Key.Y));
        }

        public Tile GetTile(int x, int y)
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
            return new System.Drawing.Rectangle(x - 13, y - 13, w - x + 27, h - y + 27);
        }
        public bool Visible(int x, int y)
        {
            bool xIn = CheckX(x);
            bool yIn = CheckY(y);

            if (!xIn && !yIn)
                return false;

            bool expX = yIn && (x >= _exploredLeft[y - down] && x <= _exploredRight[y - down]);
            bool expY = xIn && (y >= _exploredDown[x - left] && y <= _exploredUp[x - left]);

            if (xIn && yIn)
                return expX || expY;
            return (!yIn || expX) && (!xIn || expY);
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
