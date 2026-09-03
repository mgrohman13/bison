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
            public static double Height(Tile tile) => tile?.Terrain is Island i ? i.Height : 0;

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

            public static IEnumerable<Point> GetAdjacentPoints(Point point) => GetPointsInRange(point, Attack.MELEE_RANGE);
            public IEnumerable<Point> GetAdjacentPoints() => GetAdjacentPoints(Location);
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
            //performance optimization for comparisons - MoveDistTo is O(n)
            public bool MoveDistTo(Tile other, double curMove) => GetDistance(other) <= curMove && MoveDistTo(other, true) <= curMove;
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
                GetAllVision(map, point, vision, true);
            internal static IEnumerable<Point> GetAllVision(Map map, Point point, double vision) =>
                GetAllVision(map, point, vision, false);
            private static IEnumerable<Point> GetAllVision(Map map, Point from, double vision, bool visibleOnly)
            {
                double BlockRadius = (1 + Math.Sqrt(2)) / 4.0;

                if (!map.Visible(from))
                    yield return from;
                HashSet<Point> returned = [from];

                if (vision >= 1)
                {
                    double height = 0;
                    bool Visible(Point p) => !visibleOnly || map.Visible(p);
                    bool NullTile(Point p) => Visible(p) && map.GetTile(p, visibleOnly) == null;
                    double GetHeight(Point p) => Visible(p) ? Height(map.GetTile(p, visibleOnly)) : 0;

                    height = GetHeight(from);
                    vision += height;

                    List<Point> enumerable = [.. GetPointsInRange(from, vision).Where(p => p != from)];
                    List<Point> blocks = [.. enumerable.Where(NullTile).OrderBy(Dist)];
                    List<Point> heights = [.. enumerable.Where(Visible).Where(p => GetHeight(p) >= height)
                        .OrderByDescending(GetHeight).OrderBy(Dist)];


                    ////can make this check smarter based on how alt alg is implemented
                    //if (blocks.Count + heights.Count > vision)
                    //{
                    //    //use alt algorithm that doesnt loop through all blocks 
                    //    //need to get all points within the block angle for each to?
                    //    //ideally use GetLinePoints, but that will return a different result 
                    //    //instead, could change both algorithms to do something similar to GetLinePoints but different
                    //    //GetLinePoints is not ideal anyways because it wont show inset corners
                    //}


                    foreach (Point to in Game.Rand.Iterate(enumerable))
                        if (!map.Visible(to))
                        {
                            double a = (to.Y - from.Y) / (double)(to.X - from.X);
                            double c = from.Y - a * from.X;

                            double distance = Dist(to);
                            double check = vision - distance;
                            var h = Select(heights.Where(h => GetHeight(h) > check));
                            var b = Select(blocks);
                            Point retVal = ((Point?[])[h, b]).Where(p => p.HasValue).OrderBy(DistN).FirstOrDefault()
                                ?? to;
                            if (!returned.Contains(retVal) && !map.Visible(retVal))
                            {
                                yield return retVal;
                                returned.Add(retVal);
                            }

                            Point? Select(IEnumerable<Point> points) => points.Where(Blocks).Select(p => (Point?)p).FirstOrDefault();
                            bool Blocks(Point block) => Dist(block) < distance && LineDist(from, a, c, block) < BlockRadius
                                && (GetAngleDiff(GetAngle(block.X - from.X, block.Y - from.Y),
                                    GetAngle(to.X - from.X, to.Y - from.Y)) < HALF_PI);
                        }

                    double DistN(Point? other) => other.HasValue ? Dist(other.Value) : double.MaxValue;
                    double Dist(Point other) => GetDistance(from, other);
                    static double LineDist(Point from, double a, double c, Point point) =>
                        double.IsFinite(a) ? PointLineDistanceAbs(a, -1, c, point) : Math.Abs(point.X - from.X);
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

                double dist = Math.Ceiling(GetDistance(from, to) + .5);
                double dx = (to.X - from.X) / dist;
                double dy = (to.Y - from.Y) / dist;

                double curX = from.X;
                double curY = from.Y;
                Point prev = from;
                while (prev != to)
                {
                    curX += dx;
                    curY += dy;
                    Point next = new((int)Math.Round(curX), (int)Math.Round(curY));
                    if (prev != next)
                    {
                        yield return next;
                        prev = next;
                    }
                }
            }

            public static IEnumerable<Tuple<Point, double>> GetAttacks(IAttacker attacker) => GetAttacks(attacker, true);
            internal static IEnumerable<Tuple<Point, double>> GetAttacks(IAttacker attacker, bool visibleOnly)
            {
                Piece piece = attacker?.Piece;
                if (piece != null && !(piece.GetBehavior<IKillable>()?.Dead ?? false))
                {
                    Tile start = piece.Tile;
                    IEnumerable<Attack> attacks = attacker.Attacks;
                    if (visibleOnly)
                        attacks = attacks.Where(a => a.CanAttack());
                    IMovable movable = piece.GetBehavior<IMovable>();
                    bool melee = movable != null && attacks.Any(a => a.Range == Attack.MELEE_RANGE);

                    Dictionary<Point, double> meleeHeights = [];
                    if (melee)
                        foreach (Point moveP in start.GetPointsInRange(movable).Where(CanMove))
                            foreach (Point attP in GetAdjacentPoints(moveP).Where(CanMove))
                                if (moveP != attP)
                                {
                                    meleeHeights.TryGetValue(attP, out double height);
                                    height = Math.Max(height, Height(GetTile(moveP)));
                                    meleeHeights[attP] = height;
                                }

                    foreach (var attack in attacks)
                    {
                        Dictionary<Point, int> tileMods = [];
                        if (melee && attack.Range == Attack.MELEE_RANGE)
                            foreach (var p in meleeHeights)
                                Add(p.Key, Attack.TerrainAttMod(p.Value, Height(GetTile(p.Key))));
                        else
                            foreach (var p in start.GetPointsInRange(attack).Where(CanMove))
                                Add(p, Attack.TerrainAttMod(start, GetTile(p)));
                        void Add(Point p, int mod) => tileMods[p] = Math.Max(mod, tileMods.GetValueOrDefault(p, int.MinValue));
                        foreach (var pair in tileMods)
                            yield return Tuple.Create(pair.Key, Consts.StatValue(Consts.ModAtt(attack.AttackCur, pair.Value)));
                    }

                    bool CanMove(Point p)
                    {
                        Tile tile = GetTile(p);
                        if (tile == null)
                            return visibleOnly && !start.Map.Visible(p);
                        if (tile.Piece == null)
                            return true;
                        if (visibleOnly)
                            return tile.Piece.HasBehavior<IMovable>() || (tile.Piece.HasBehavior<IKillable>() && tile.Piece.Side.IsPlayer)
                                || tile.Piece is Resource || tile.Piece is Foundation;
                        else
                            return tile.Piece.HasBehavior<IKillable>() && tile.Piece is not FoundationPiece;
                    }
                    Tile GetTile(Point p) => start.Map.GetTile(p, visibleOnly);
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
