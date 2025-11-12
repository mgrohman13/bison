using game2.game;
using game2.pieces;
using System.Drawing;
using Point = MattUtil.Point;

namespace game2.map
{
    public class Tile
    {
        public static readonly float YRatio = (float)(Math.Sqrt(3.0) / 2.0);

        public readonly Map Map;
        public readonly Terrain Terrain;

        public readonly int XIndex, YIndex;

        internal Tile(Map map, Point p, Func<Tile, Terrain> GetTerrain)
        {
            Map = map;

            XIndex = p.X;
            YIndex = p.Y;

            Terrain = GetTerrain(this);
        }

        public PointF Point => GetPoint(XIndex, YIndex);
        public static PointF GetPoint(int x, int y) => new(x + .5f * (y & 1), y * YRatio);

        public Point GetMapIndex() => new(XIndex, YIndex);

        public Piece? Piece => Map.GetPiece(this);
        public bool Visible => Map.Visible(this);

        internal IEnumerable<Tile> GetNeighbors()
        {
            Point[] deltasEven =
            [
                new(1, 0),
                new(0, 1),
                new(-1, 1),
                new(-1, 0),
                new(-1, -1),
                new(0, -1)
            ];
            Point[] deltasOdd =
            [
                new(1, 0),
                new(1, 1),
                new(0, 1),
                new(-1, 0),
                new(0, -1),
                new(1, -1)
            ];
            Point[] deltas = (YIndex & 1) == 0 ? deltasEven : deltasOdd;

            return Game.Rand.Iterate(deltas).Select(delta => Map.GetTile(XIndex + delta.X, YIndex + delta.Y));
        }

        public static bool operator ==(Tile? a, Tile? b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            return a.Map == b.Map && a.XIndex == b.XIndex && a.YIndex == b.YIndex;
        }

        public static bool operator !=(Tile? a, Tile? b) => !(a == b);

        public override bool Equals(object? obj) => obj is Tile other && this == other;

        public override int GetHashCode() => HashCode.Combine(Map, XIndex, YIndex);

        public override string ToString()
        {
            return $"{Point} : {XIndex},{YIndex}";
        }
    }
}
