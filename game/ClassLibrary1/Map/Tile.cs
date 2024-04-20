using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = MattUtil.Point;

namespace ClassLibrary1.Map
{
    public partial class Map
    {
        [Serializable]
        public class Tile
        {
            public readonly Map Map;
            public readonly int X, Y;
            private Terrain _terrain;
            public Terrain Terrain => _terrain;
            public Piece Piece => Map.GetPiece(Location) ?? Terrain;
            public bool Visible => Map.Visible(Location);
            public Point Location => new(X, Y);

            static Tile()
            {
                NewTile = (map, p, GetTerrain) =>
                {
                    Tile tile = new(map, p.X, p.Y);
                    tile._terrain = GetTerrain(tile);
                    return tile;
                };
            }
            private Tile(Map map, int x, int y)
            {
                Map = map;
                X = x;
                Y = y;
            }

            //support blocking
            public double GetDistance(Point other) => GetDistance(other.X, other.Y);
            public double GetDistance(Tile other) => GetDistance(other.X, other.Y);
            public double GetDistance(int x, int y) => GetDistance(X, Y, x, y);
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

            internal IEnumerable<Tile> GetAdjacentTiles() => GetTilesInRange(Attack.MELEE_RANGE, false, null);
            internal IEnumerable<Tile> GetTilesInRange(IMovable movable, double? cur = null) => GetTilesInRange(cur ?? movable.MoveCur, false, movable.Piece);
            internal IEnumerable<Tile> GetTilesInRange(IAttacker attacker, double? cur = null) => GetTilesInRange(cur ?? attacker.Attacks.Max(a => a.Range), true, attacker.Piece);
            internal IEnumerable<Tile> GetTilesInRange(Attack attack) => GetTilesInRange(attack.Range, true, attack.Piece);
            private IEnumerable<Tile> GetTilesInRange(double range, bool blockMap, Piece blockFor) => GetPointsInRange(range, blockMap, blockFor)
                .Select(Map.GetTile).Where(t => t != null);

            internal IEnumerable<Point> GetPointsInRangeUnblocked(double vision) => GetPointsInRange(vision, false, null);
            internal static IEnumerable<Point> GetPointsInRangeUnblocked(Map map, Point point, double range) => GetPointsInRange(map, point, range, false, null);

            public static IEnumerable<Point> GetAllPointsInRange(Map map, Point point, double range) => GetPointsInRange(map, point, range, false, null);
            public IEnumerable<Point> GetAllPointsInRange(double range) => GetPointsInRange(range, false, null);
            public IEnumerable<Point> GetPointsInRange(IMovable movable) => GetPointsInRange(movable, movable.MoveCur);
            public IEnumerable<Point> GetPointsInRange(IMovable movable, double move) => GetPointsInRange(move, false, movable.Piece);
            public IEnumerable<Point> GetPointsInRange(IBuilder builder) => GetPointsInRange(builder.Range, true, null);
            public IEnumerable<Point> GetPointsInRange(Attack attack) => GetPointsInRange(attack.Range, true, attack.Piece);
            private IEnumerable<Point> GetPointsInRange(double range, bool blockMap, Piece blockFor) => GetPointsInRange(Map, Location, range, blockMap, blockFor);
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
                ////more efficient implementation?
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
                        //       && Dist(point, new(x, y), p.Key) < p.Value
                        //       && (GetAngleDiff(GetAngle(p.Key.X - point.X, p.Key.Y - point.Y), GetAngle(x - point.X, y - point.Y)) < HALF_PI)))
                        yield return new(x, y);
                    }
                }
                //static double Dist(PointD segment1, PointD segment2, Point point)
                //{
                //    if (segment2.X == segment1.X)
                //        return Math.Abs(point.X - segment1.X);

                //    //merge with CalcLine?
                //    double a = (segment2.Y - segment1.Y) / (segment2.X - segment1.X);
                //    double b = -1;
                //    double c = segment1.Y - a * segment1.X;
                //    return PointLineDistanceAbs(a, b, c, point);
                //}
            }

            public static bool operator !=(Tile a, Tile b)
            {
                return !(a == b);
            }
            public static bool operator ==(Tile a, Tile b)
            {
                return a is null ? b is null : a.Equals(b);
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
    }
}
