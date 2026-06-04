using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Point = MattUtil.Point;

namespace ClassLibrary1.Map
{
    public partial class Map
    {
        [Serializable]
        [DataContract(IsReference = true)]
        public class Tile
        {
            public readonly Map Map;
            public readonly int X, Y;
            private ITerrain _terrain;
            public ITerrain Terrain => _terrain;
            public Piece Piece => Map.GetPiece(Location) ?? Terrain as Block;
            public bool Visible => Map.Visible(Location);
            public Point Location => new(X, Y);
            public PointD LocationD => new(X, Y);

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

            public double Height() => Height(this);
            public static double Height(Tile? tile) => tile?.Terrain is Island i ? i.Height : 0;

            public bool ShowMove() => Visible && GetAdjacentPoints().Where(Map.Visible).Skip(3).Any();

            //support blocking
            public double GetDistance(Point other) => GetDistance(other.X, other.Y);
            public double GetDistance(Tile other) => GetDistance(other.X, other.Y);
            public double GetDistance(int x, int y) => GetDistance(X, Y, x, y);
            public static double GetDistance(Point p1, Point p2) => GetDistance(p1.X, p1.Y, p2.X, p2.Y);
            public static double GetDistance(int x1, int y1, int x2, int y2) => GetDistanceD(x1, y1, x2, y2);
            public static double GetDistanceD(PointD p1, PointD p2) => GetDistanceD(p1.X, p1.Y, p2.X, p2.Y);
            public static double GetDistanceD(double x1, double y1, double x2, double y2)
            {
                double xDiff = x1 - x2;
                double yDiff = y1 - y2;
                //if (1 == y1 - y2 % 2)
                //    xDiff = (xDiff) + .5;
                //yDiff *= Math.Sqrt(3) / 2.0;
                return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            }

            public IEnumerable<Tile> GetVisibleAdjacentTiles() => GetVisibleTilesInRange(Attack.MELEE_RANGE);
            public IEnumerable<Tile> GetVisibleTilesInRange(IBuilder builder) => GetVisibleTilesInRange(builder.Range);
            public IEnumerable<Tile> GetVisibleTilesInRange(IAttacker attacker) => GetVisibleTilesInRange(attacker.Attacks.Max(a => a.Range));
            public IEnumerable<Tile> GetVisibleTilesInRange(Attack attack) => GetVisibleTilesInRange(attack.Range);
            private IEnumerable<Tile> GetVisibleTilesInRange(double range) => GetTilesInRange(range).Where(t => t.Visible);

            internal IEnumerable<Tile> GetAdjacentTiles() => GetTilesInRange(Attack.MELEE_RANGE);
            internal IEnumerable<Tile> GetTilesInRange(IBuilder builder) => GetTilesInRange(builder.Range);
            internal IEnumerable<Tile> GetTilesInRange(IAttacker attacker) => GetTilesInRange(attacker.Attacks.Max(a => a.Range));
            internal IEnumerable<Tile> GetTilesInRange(Attack attack) => GetTilesInRange(attack.Range);
            private IEnumerable<Tile> GetTilesInRange(double range) => GetPointsInRange(range)
                .Select(Map.GetTile).Where(t => t != null);

            public IEnumerable<Point> GetAdjacentPoints() => GetPointsInRange(Attack.MELEE_RANGE);
            public IEnumerable<Point> GetPointsInRange(IBuilder builder) => GetPointsInRange(builder.Range);
            public IEnumerable<Point> GetPointsInRange(IAttacker attacker) => GetPointsInRange(attacker.Attacks.Max(a => a.Range));
            public IEnumerable<Point> GetPointsInRange(Attack attack) => GetPointsInRange(attack.Range);
            public IEnumerable<Point> GetPointsInRange(double range) => GetPointsInRange(Location, range);
            public static IEnumerable<Point> GetPointsInRange(Point point, double range)
            {
                int max = (int)range + 1;
                foreach (Point p in Game.Rand.Iterate(-max, max, -max, max))
                {
                    int x = point.X + p.X;
                    int y = point.Y + p.Y;
                    double distance = GetDistance(point.X, point.Y, x, y);
                    if (distance <= range)
                        yield return new(x, y);
                }
            }

            //public IEnumerable<Tile> GetVisibleTilesInRange(IMovable movable) => GetVisibleTilesInRange(movable.MoveCur);
            public IEnumerable<Point> GetPointsInRange(IMovable movable) =>
                GetAllMovement(Map, movable.Piece.Tile.Location, movable.MoveCur, true);
            internal IEnumerable<Tile> GetTilesInRange(IMovable movable) =>
                GetAllMovement(Map, movable.Piece.Tile.Location, movable.MoveCur, false).Select(Map.GetTile).Where(t => t != null);
            internal static IEnumerable<Point> GetAllMovement(Map map, Point point, double move, bool visibleOnly = false)
            {
                Tile from = map.GetTile(point, visibleOnly);
                double height = Height(from);

                var all = GetPointsInRange(point, move);
                //List<Point> blocks = [.. all.Where(p => Height(GetTile(p, visibleOnly)) > height)];

                //List<Point> checkAlt = [];
                foreach (var to in all)
                {
                    // TODO performance 
                    double path = GetLinePoints(point, to).Max(p => Height(map.GetTile(p, visibleOnly)));
                    if (path <= height || GetDistance(point, to) + (path - height) <= move)
                        yield return to;

                    //if (!(blocks.Any(p => Blocks(point, to, p))))
                    //    yield return to;
                    //else
                    //    checkAlt.Add(to);
                    //Tile to = GetTile(p, visibleOnly);
                    //double dist = -1;
                    //if (from != null && to != null)
                    //    dist = from.MoveDistTo(to);
                    //return dist <= move;
                }

                //ALT
            }
            public double MoveDistTo(Tile other) => MoveDistTo(other, true);
            internal double MoveDistTo(Tile other, bool visibleOnly = false)
            {
                double dist = GetDistance(other);
                double from = Height();
                double height = GetLinePoints(this.Location, other.Location).Max(p => Height(other.Map.GetTile(p, visibleOnly))); 
                if (height > from)
                    dist += height - from;
                return dist;
            }

            public static IEnumerable<Point> GetVision(PlayerPiece piece, Point moveTo)
            {
                Map map = piece?.Tile.Map;
                Tile to = map?.GetVisibleTile(moveTo);
                if (map != null && to != null && piece.HasBehavior<IMovable>())
                {
                    IEnumerable<Point> enumerable = (piece.Tile == to ? [moveTo] : GetLinePoints(piece.Tile.Location, to.Location));
                    return enumerable.SelectMany(p => GetVision(map, p, piece.Vision));
                }
                return [];
            }
            public static IEnumerable<Point> GetVision(Map map, Point point, double vision) =>
                GetAllVision(map, point, vision, true).Distinct();
            internal static IEnumerable<Point> GetAllVision(Map map, Point point, double vision) =>
                GetAllVision(map, point, vision, false).Distinct();
            private static IEnumerable<Point> GetAllVision(Map map, Point from, double vision, bool visibleOnly)
            {
                yield return from;
                if (vision >= 1)
                {
                    double height = 0;
                    bool Visible(Point p) => !visibleOnly || map.Visible(p);
                    bool NullTile(Point p) => Visible(p) && map.GetTile(p, visibleOnly) == null;
                    double GetHeight(Point p) => Visible(p) ? Height(map.GetTile(p, visibleOnly)) : 0;

                    height = GetHeight(from);
                    vision += height;

                    List<Point> enumerable = [.. GetPointsInRange(from, vision).Where(p => p != from)];
                    List<Point> blocks = [.. enumerable.Where(NullTile).OrderBy(b => GetDistance(from, b))];
                    List<Point> heights = [.. enumerable.Where(Visible).Where(p => GetHeight(p) >= height)
                        .OrderByDescending(GetHeight).OrderBy(b => GetDistance(from, b))];

                    foreach (Point to in Game.Rand.Iterate(enumerable))
                    {
                        double distance = GetDistance(from.X, from.Y, to.X, to.Y);
                        //if (distance <= vision)
                        //{
                        Point? Select(IEnumerable<Point> points) => points.Select(p => (Point?)p).FirstOrDefault();

                        var plateau = Select(heights.Where(p => Blocks(from, to, p)).Where(h => distance + GetHeight(h) > vision));
                        //&& GetHeight(add) >= GetHeight(h)));// doesn't work, still gives away tile info even if you dont see it
                        if (plateau.HasValue && distance + GetHeight(plateau.Value) > vision)
                        {
                            yield return plateau.Value;
                        }
                        else
                        {
                            var block = Select(blocks.Where(p => Blocks(from, to, p)));
                            if (block.HasValue)
                                yield return block.Value;
                            else
                                yield return to;
                        }
                        //}
                    }
                }

            }

            private readonly static double BlockRadius = (1 + Math.Sqrt(2)) / 4.0;
            private static bool Blocks(Point from, Point to, Point block)
            {
                return GetDistance(from, block) < GetDistance(from, to)
                    && Dist(from, new(to.X, to.Y), block) < BlockRadius
                    && (GetAngleDiff(GetAngle(block.X - from.X, block.Y - from.Y), GetAngle(to.X - from.X, to.Y - from.Y)) < HALF_PI);

                static double Dist(Point segment1, PointD segment2, Point point)
                {
                    if (segment2.X == segment1.X) return Math.Abs(point.X - segment1.X);
                    //merge with CalcLine?
                    double a = (segment2.Y - segment1.Y) / (double)(segment2.X - segment1.X);
                    double b = -1;
                    double c = segment1.Y - a * segment1.X;
                    return PointLineDistanceAbs(a, b, c, point);
                }
            }
            //, bool blockMap, Piece blockFor)
            //double blockRadius = Math.Sqrt(2) / 2;//may use for different types of blocking
            ////SortedDictionary<Point, double> block = new(Comparer<Point>.Create(
            ////    (p1, p2) => GetDistance(point, p1).CompareTo(GetDistance(point, p2))));
            //double sqrtTwo = Math.Sqrt(2);
            //double baseBlock = .5 + (sqrtTwo / 2.0 - .5) / 2.0;
            ////double enemyBlock = 1 + (sqrtTwo - 1) / 2.0;
            //void AddBlock(Point b, double blockRange)
            //{
            //    block.TryGetValue(b, out double range);
            //    range = Math.Max(range, blockRange);
            //    block[b] = range;
            //}
            ////if (blockMap)
            //foreach (var p in GetPointsInRangeUnblocked(map, point, range).Where(p => map.GetTile(p, visibleOnly) == null))
            //    AddBlock(p, baseBlock);
            ////if (blockFor != null)
            //////more efficient implementation?
            ////    foreach (var pair in map._pieces.Where(p => p.Value != blockFor
            ////            && (p.Value.Side != blockFor.Side || !p.Value.HasBehavior<IMovable>())
            ////            && GetDistance(point, p.Key) <= range))
            ////        AddBlock(pair.Key, pair.Value.Side != null && pair.Value.Side != blockFor.Side ? enemyBlock : baseBlock);
            //public static IOrderedEnumerable<Point> GetLinePoints(Point from, Point to)
            //{
            //    bool isX = from.X == to.X;
            //    double a = isX ? 0 : (to.Y - from.Y) / (double)(to.X - from.X);
            //    double c = to.Y - a * to.X;
            //    double b = -1;
            //    return Game.Rand.Iterate(Math.Min(from.X, to.X), Math.Max(from.X, to.X), Math.Min(from.Y, to.Y), Math.Max(from.Y, to.Y))
            //        .Where(p => isX || PointLineDistanceAbs(a, b, c, p) < BlockRadius)
            //        .OrderBy(p => GetDistance(from, p));
            //}
            public static IEnumerable<Point> GetLinePoints(Point from, Point to)
            {
                yield return from;
                Point prev = from;

                double dx = (to.X - from.X);
                double dy = (to.Y - from.Y);
                double dist = GetDistance(from, to);
                double steps = (int)(Math.Ceiling(dist));
                double inc = dist / steps;

                PointD cur = new(from.X, from.Y);
                for (int a = 0; a < steps; a++)
                {
                    cur = new(cur.X + inc * dx / dist, cur.Y + inc * dy / dist);
                    Point next = new((int)Math.Round(cur.X), (int)Math.Round(cur.Y));
                    if (next != prev)
                    {
                        yield return next;
                        prev = next;
                    }
                }
            }

            public static bool operator !=(Tile a, Tile b) => !(a == b);
            public static bool operator ==(Tile a, Tile b) => a is null ? b is null : a.Equals(b);
            public override bool Equals(object obj)
            {
                Tile other = obj as Tile;
                return other != null && this.X == other.X && this.Y == other.Y;
            }

            public override int GetHashCode() => X * ushort.MaxValue + Y;

            public override string ToString() => string.Format("({0}, {1})", X, Y);
        }
    }
}
