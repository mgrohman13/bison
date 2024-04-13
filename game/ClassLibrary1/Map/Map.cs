﻿using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Point = MattUtil.Point;

namespace ClassLibrary1.Map
{
    [Serializable]
    public partial class Map // : IDeserializationCallback
    {
        internal static readonly Stopwatch watch = new();
        private static int evalCount = 0;

        private const double TWO_PI = Math.PI * 2;
        private const double HALF_PI = Math.PI / 2.0;

        public readonly Game Game;

        private readonly Noise noise;
        private readonly Path[] _paths;
        private readonly List<Cave> _caves;
        private readonly HashSet<Point> clearTerrain;

        private readonly Dictionary<Point, Piece> _pieces;
        private readonly HashSet<Point> _explored;
        private Rectangle _gameBounds;

        private readonly Dictionary<ResourceType, int> resourcePool;

        static Map()
        {
            //static init to set NewTile
            Tile.GetDistance(0, 0, 0, 0);
        }

        internal Map(Game game)
        {
            LogEvalTime();

            Game = game;
            clearTerrain = new();

            double max = Game.Rand.GaussianOE(130, .091, .13, 65);
            double min = Game.Rand.GaussianCapped(5.2, .078, 2.6);
            int steps = Game.Rand.GaussianOEInt(6.5, .13, .13, Game.Rand.RangeInt(2, 5));
            double weightScale = Game.Rand.Weighted(.78) + Game.Rand.OE(.13);
            noise = new Noise(Game.Rand, min, max, steps, .065, weightScale);

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

            //double[] enemyPows = angles.Select(x => 1 + Game.Rand.OE(1 / 13.0)).ToArray();
            //static double Increase(double x) => 1.3 + (x - 1) * 1.69;
            //enemyPows[0] = Increase(enemyPows[0]);
            //enemyPows[1] = Increase(enemyPows[1]);
            //enemyPows = enemyPows.Select(x => x > 2 ? 1 + Math.Sqrt(x - 1) : x).ToArray();
            //enemyPows[1] = 1 / enemyPows[1];
            //for (int c = 2; c < numPaths; c++)
            //    if (Game.Rand.Bool())
            //        enemyPows[c] = 1 / enemyPows[c];
            //Game.Rand.Shuffle(enemyPows);

            _paths = new Path[numPaths];
            for (int d = 0; d < numPaths; d++)
            {
                //double enemyPow = enemyPows[d];
                //double enemyMult = 1 + Game.Rand.OE(1 / (enemyPow > 1 ? enemyPow : 1 / enemyPow));
                //if (Game.Rand.Bool())
                //    enemyMult = 1 / enemyMult;
                _paths[d] = new Path(angles[d]);//, enemyMult / enemyPow, enemyPow);
            }

            separation /= 2.6;
            double caveMult = Math.PI / numPaths;
            int numCaves = Game.Rand.GaussianOEInt(2 + (Math.PI - 2) * caveMult * caveMult, .091, .039, 2);
            _caves = new();
            for (int e = 0; e < numCaves; e++)
            {
                int t = 0, tries = numCaves * numCaves * 13 + 169;
                double caveDir, distMult = 1;
                do
                {
                    caveDir = Game.Rand.NextDouble() * TWO_PI;
                    if (t++ > tries)
                    {
                        distMult = 1.3;
                        if (Game.Rand.Bool())
                            e = numCaves;
                        break;
                    }
                }
                while (_caves.Select(c => GetAngle(c.Center)).Concat(angles).Any(a => GetAngleDiff(caveDir, a) < separation));

                PointD cave = GetPoint(caveDir, Game.Rand.GaussianOE(Consts.CaveDistance * distMult, Consts.CaveDistanceDev / distMult, Consts.CaveDistanceOE, Consts.CaveMinDist));
                PointD connect = Game.Rand.SelectValue(_caves.Select(c => c.Center).Concat(_paths.Select(p => p.GetClosestPoint(cave.X, cave.Y)))
                     .OrderBy(p => GetDistSqr(cave, p)).Take(2));
                _caves.Add(new(cave, connect));//, connectCave));
            }
            if (_caves.Count < 2) throw new Exception();

            _pieces = new();
            _explored = new();

            resourcePool = new() { { ResourceType.Foundation, 1 },
                { ResourceType.Biomass, 2 }, { ResourceType.Artifact, 3 }, { ResourceType.Metal, 5 }, };

            LogEvalTime();
        }

        internal void NewGame()
        {
            SpawnHives();
            GenerateStartResources();

            foreach (var cave in Game.Rand.Iterate(_caves))
                cave.PathFind(this);//, Game.Player.Core.Tile);
        }
        private void GenerateStartResources()
        {
            for (int a = 0; a < 8; a++)
            {
                Tile tile = StartTile();
                switch (GenResourceType())
                {
                    case ResourceType.Artifact:
                        Artifact.NewArtifact(tile);
                        break;
                    case ResourceType.Biomass:
                        Biomass.NewBiomass(tile);
                        break;
                    case ResourceType.Metal:
                        Metal.NewMetal(tile);
                        break;
                    case ResourceType.Foundation:
                        Foundation.NewFoundation(tile);
                        break;
                }
            }

            foreach (var explore in Game.Rand.Iterate(_paths))
                GenResources(explore.Explore(this, Consts.PathWidth));
        }
        private void SpawnHives()
        {
            int hives = Game.Rand.GaussianOEInt(Math.PI - 1 + _caves.Count, .091, .039, Game.Rand.Round(3.9));
            Dictionary<Cave, int> chances = new(), counts = new();
            foreach (Cave c in _caves)
                chances[c] = 2;
            while (chances.Values.Sum() < hives)
                chances[Game.Rand.SelectValue(chances)]++;
            for (int f = 0; f < hives; f++)
            {
                Cave cave = Game.Rand.SelectValue(chances);
                if (Game.Rand.Next(13) > 0)
                    chances[cave]--;
                Tile tile = SpawnTile(cave.Center, Consts.CaveSize, true);
                cave.AddHive(Hive.NewHive(tile, f));
                counts.TryGetValue(cave, out int count);
                counts[cave] = count + 1;
            }

            int cavesLeft = _caves.Count, resources = cavesLeft + 2;
            double avgHives = hives / (double)cavesLeft + 1;
            foreach (var cave in Game.Rand.Iterate(_caves))
            {
                counts.TryGetValue(cave, out int caveHives);
                int spawn = resources;
                if (cavesLeft > 1)
                {
                    double avg = resources / (double)cavesLeft * avgHives / (caveHives + 1.0);
                    int cap = (int)Math.Ceiling(Math.Max(2 * avg - resources, 0));
                    spawn = Math.Min(resources, avg > cap ? Game.Rand.GaussianCappedInt(avg, 1, cap) : Game.Rand.RangeInt(0, resources));
                }

                cavesLeft--;
                resources -= spawn;
                GenResources(Enumerable.Repeat((object)null, spawn).Select(_ => cave.SpawnTile(this, false)), true);
            }
        }

        internal void PlayTurn(int turn)
        {
            foreach (var p in _paths)
                p.Turn(turn);
            foreach (var c in _caves)
                c.Turn(turn);
        }

        [NonSerialized]
        private Dictionary<Point, float> evaluateCache;//Tuple<float, float>> evaluateCache;
        private float Evaluate(Point p)//, out float lineDist)
        {
            int x = p.X, y = p.Y;
            evaluateCache ??= new();
            if (evaluateCache.TryGetValue(p, out var t))
            {
                //lineDist = t.Item2;
                return t;//.Item1;
            }

            watch.Start();
            evalCount++;

            double minLineDist = double.MaxValue;

            double mult = 0;
            foreach (var path in _paths)
            {
                double Gradient(PointD point, int sign)
                {
                    double backMult = 1;
                    double angle = GetAngle(x - point.X, y - point.Y);
                    if (GetAngleDiff(path.Angle, angle) > HALF_PI)
                    {
                        double distSqr = Math.Min(GetDistSqr(x, y, path.Left), GetDistSqr(x, y, path.Right));
                        backMult = Math.Min(1, Consts.PathWidth * Consts.PathWidth / distSqr);
                    }

                    double dist = PointLineDistanceSigned(path, point, p) * sign;
                    minLineDist = Math.Min(minLineDist, Math.Abs(dist));
                    return 2 / (1 + Math.Pow(Math.E, -.065 * dist)) * backMult;
                }

                double m = Math.Min(Gradient(path.Left, 1), Gradient(path.Right, -1));
                mult += m * m;
            }
            mult += _caves.Max(c => c.GetMult(x, y));

            //lineDist = (float)minLineDist;
            float value = (float)(noise.Evaluate(x, y) * mult);
            evaluateCache.Add(p, value);// new(value, lineDist));

            watch.Stop();
            return value;
        }
        private static double GetDistSqr(PointD v, PointD w) => GetDistSqr(v.X, v.Y, w);
        private static double GetDistSqr(double x, double y, PointD point)
        {
            double distX = point.X - x, distY = point.Y - y;
            return distX * distX + distY * distY;
        }

        public Tile GetVisibleTile(int x, int y)
        {
            return GetVisibleTile(new(x, y));
        }
        public Tile GetVisibleTile(Point p)
        {
            return Visible(p) ? GetTile(p) : null;
        }
        public IEnumerable<Piece> GetVisiblePieces()
        {
            return _pieces.Values.Where(p => p.Tile.Visible);
        }

        internal Tile GetTile(int x, int y)
        {
            return GetTile(new(x, y));
        }
        internal Tile GetTile(Point p)
        {
            double terrain = 1;
            bool chasm = false;
            if (!clearTerrain.Contains(p))
            {
                terrain = Evaluate(p);//, out float lineDist);
                //also use dist from center?
                // (5 * terrain * Consts.PathWidth + lineDist) / 2.0 % Consts.PathWidth < 1;
                if (!chasm && terrain < 1 / 4.0)
                    return null;
                chasm |= terrain < 1 / 2.0;
            }
            else
                ;

            Piece piece = GetPiece(p);
            return piece == null ? NewTile(this, p, t => chasm ? new Block(t, terrain) : null) : piece.Tile;
        }
        private Piece GetPiece(Point p)
        {
            _pieces.TryGetValue(p, out Piece piece);
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
        internal void GameOver()
        {
            int Inflate(double dir) => Game.Rand.GaussianOEInt(39 * Math.Sqrt(_gameBounds.Width * _gameBounds.Height) / dir, .13, .13) + 1;
            _gameBounds.Inflate(Inflate(_gameBounds.Width), Inflate(_gameBounds.Height));
        }

        public bool Visible(int x, int y)
        {
            return Visible(new(x, y));
        }
        public bool Visible(Point tile)
        {
            bool visible = _explored.Contains(tile);
            //if (!visible)
            //    visible = _pieces.ContainsKey(new Point(x, y));
            return Game.TEST_MAP_GEN.HasValue || Game.GameOver || visible;
        }

        internal void AddPiece(Piece piece)
        {
            _pieces.Add(piece.Tile.Location, piece);

            if (piece is PlayerPiece playerPiece)
                UpdateVision(playerPiece);
        }
        internal void RemovePiece(Piece piece)
        {
            _pieces.Remove(piece.Tile.Location);
        }
        private void UpdateVision(PlayerPiece piece) => UpdateVision(piece.Tile, piece.Vision, piece is not Core);
        internal void UpdateVision(Tile tile, double range, bool explore = true)
        {
            LogEvalTime();

            foreach (Point p in tile.GetPointsInRangeUnblocked(range))
                _explored.Add(p);

            int vision = (int)range + 1;
            int x = Math.Min(_gameBounds.X, tile.X - vision);
            int y = Math.Min(_gameBounds.Y, tile.Y - vision);
            int right = Math.Max(_gameBounds.Right, tile.X + vision + 1);
            int bottom = Math.Max(_gameBounds.Bottom, tile.Y + vision + 1);

            _gameBounds = new Rectangle(x, y, right - x, bottom - y);

            if (explore)
                Explore(tile, range);

            LogEvalTime();
        }
        public static void LogEvalTime()
        {
            if (evalCount > 0)
            {
                float evalTime = 1000f * watch.ElapsedTicks / Stopwatch.Frequency;
                Debug.WriteLine($"Evaluate ({evalCount}): {evalTime}");
                watch.Reset();
                evalCount = 0;
            }
        }

        internal Tile StartTile() => SpawnTile(new(0, 0), Consts.PathWidth + Consts.ResourceAvgDist, false);
        private Tile SpawnTile(PointD spawnCenter, double deviation, bool isEnemy)
        {
            int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(deviation));
            Tile tile;
            do
                tile = GetTile(RandCoord(spawnCenter.X), RandCoord(spawnCenter.Y));
            while (InvalidStartTile(tile, isEnemy));

            //Debug.WriteLine($"SpawnTile ({Angle:0.00}) {distance:0.0}: {spawnCenter} -> {tile}");

            return tile;
        }
        internal static bool InvalidStartTile(Tile tile, bool isEnemy)
        {
            if (tile == null) return true;

            bool visible = tile.Visible && !Game.TEST_MAP_GEN.HasValue;
            bool hiveRange = isEnemy && tile.Map._pieces.OfType<Hive>().Any(h => tile.GetDistance(h.Tile) <= h.MaxRange);
            Core core = tile.Map.Game.Player.Core;
            bool coreRange = core != null && tile.GetDistance(core.Tile) <= core.GetBehavior<IRepair>().Range;
            bool invalid = (visible && !hiveRange) || tile.Piece != null || coreRange;
            if (!invalid)
                Debug.WriteLine("InvalidStartTile: " + tile);
            return invalid;
        }

        internal void Explore(Tile tile, double vision)
        {
            if (tile.X != 0 || tile.Y != 0)
            {
                double angle = GetAngle(tile.X, tile.Y);
                Path explore = Game.Rand.Iterate(_paths).OrderBy(path => GetAngleDiff(path.Angle, angle)).First();
                GenResources(explore.Explore(tile, vision));
                foreach (Cave c in _caves)
                    c.Explore(tile, vision);
            }
        }
        internal void GenResources(IEnumerable<Tile> tiles, bool noFoundation = false)
        {
            foreach (Tile resource in Game.Rand.Iterate(tiles))
            {
                switch (GenResourceType(noFoundation))
                {
                    case ResourceType.Artifact:
                        Artifact.NewArtifact(resource);
                        break;
                    case ResourceType.Biomass:
                        Biomass.NewBiomass(resource);
                        break;
                    case ResourceType.Metal:
                        Metal.NewMetal(resource);
                        break;
                    case ResourceType.Foundation:
                        Foundation.NewFoundation(resource);
                        break;
                }
            }
        }
        private ResourceType GenResourceType(bool noFoundation = false)
        {
            if (resourcePool.Values.Any(v => v == 0))
            {
                resourcePool[ResourceType.Artifact] += 2;
                resourcePool[ResourceType.Foundation] += 3;
                resourcePool[ResourceType.Biomass] += 4;
                resourcePool[ResourceType.Metal] += 5;
            }
            ResourceType type = Game.Rand.SelectValue(resourcePool);
            if (noFoundation && type == ResourceType.Foundation)
                return GenResourceType(noFoundation);
            resourcePool[type]--;
            return type;
        }

        internal double GetMinSpawnMove(Tile tile)
        {
            Cave cave = _caves.OrderBy(c => GetDistSqr(new(tile.X, tile.Y), c.Center)).First();
            return cave.MinSpawnMove;
        }
        internal Tile GetEnemyTile(double enemyMove)
        {
            var choices = _paths.Concat<IEnemySpawn>(_caves).ToDictionary(k => k, v => v.SpawnChance(Game.Turn, enemyMove));
            foreach (var choice in choices)
                Debug.WriteLine($"choice - {choice.Key}: {choice.Value}");
            IEnemySpawn spawn = Game.Rand.SelectValue(choices);
            Debug.WriteLine($"GetEnemyTile: {spawn}");
            return spawn.SpawnTile(this, true, 1.69);
        }

        private static double GetAngle(PointD point) => GetAngle(point.X, point.Y);
        private static double GetAngle(double x, double y) => Math.Atan2(y, x);
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
        //private static double PointLineDistanceAbs(double a, double b, double c, Point point) =>
        //    Math.Abs(PointLineDistance(a, b, c, point));
        private static double PointLineDistance(double a, double b, double c, Point point) =>
            (a * point.X + b * point.Y + c) / Math.Sqrt(a * a + b * b);

        public readonly Dictionary<Point, FoundPath> FoundPaths = new();
        internal List<Point> PathFind(Tile from, double movement, Func<HashSet<Point>, bool> Accept)//, Tile to
        {
            Tile to = Game.Player.Core.Tile;

            //soo much overhead...

            Point fromP = from.Location;
            Point toP = to.Location;
            if (from.Equals(to))
                return new() { fromP, toP, };
            if (FoundPaths.TryGetValue(fromP, out FoundPath found) && found.Movement <= movement)
                return found.CompletePath(fromP).ToList();

            //cache tiles and penalties at each point so we maintain reference equality and consistent penalties
            Dictionary<Point, Tuple<Tile, double>> cache = new() {
                { fromP, Tuple.Create(from, 0.0) },
                { toP, Tuple.Create(to, 0.0) },
            };

            HashSet<Tile> known = FoundPaths.Keys.Where(k => FoundPaths[k].Movement <= movement).Select(GetTile).ToHashSet();
            foreach (Tile tile in known)
                cache[tile.Location] = Tuple.Create(tile, 0.0);

            var path = TBSUtil.PathFind(Game.Rand, from, to, known, p1 =>
                    p1.GetAllPointsInRange(movement).Where(p =>
                    {
                        var tile = GetTile(p);
                        var piece = tile?.Piece;
                        return tile == from || tile == to || piece is null || piece is Terrain || piece.HasBehavior<IMovable>();
                    }).Select(p2 =>
                    {
                        double dist = p1.GetDistance(p2);
                        if (!cache.TryGetValue(p2, out Tuple<Tile, double> tuple))
                        {
                            Tile tile = GetTile(p2);

                            //the map is infinite, so to avoid pathfinding forever we impose a penalty on blocked terrain instead of blocking tiles entirely
                            double penalty = 0;
                            if (tile == null)
                            {
                                penalty = Game.Rand.Gaussian((Consts.PathWidth + movement) * Consts.PathWidth, .039);
                            }
                            else if (tile.Piece is Block block)
                            {
                                double mult = .5 + 4 * (.5 - block.Value); //ranges from 0.5 - 1.5
                                mult *= mult; //ranges from 0.25 - 2.25
                                penalty = Game.Rand.Gaussian((Consts.PathWidth + movement) * mult, .039);
                            }
                            if (penalty > 0 && !Game.TEST_MAP_GEN.HasValue && Visible(p2))
                                penalty *= Consts.PathWidth;

                            tile ??= NewTile(this, p2, t => null);
                            tuple = Tuple.Create(tile, penalty);
                            cache.Add(p2, tuple);
                        }
                        return Tuple.Create(tuple.Item1, dist + tuple.Item2);
                    }),
                    (p1, p2) => p1.GetDistance(p2))
                .Select(t => t.Location).ToList();

            HashSet<Point> blocked = new();
            foreach (var p in path)
            {
                Tile tile = GetTile(p);
                if (tile == null || tile.Piece is Terrain)
                    blocked.Add(p);
            }

            foreach (var p in blocked)
                if (!Game.TEST_MAP_GEN.HasValue && Visible(p))
                    Debug.WriteLine($"Path through visible tile {p}");

            if (Accept(blocked))
            {
                //clear any blocked terrain we pathed through 
                clearTerrain.UnionWith(blocked.SelectMany(p =>
                {
                    List<Point> list = new() { p };
                    int extra = Game.Rand.OEInt();
                    for (int a = 0; a < extra; a++)
                    {
                        Tile tile = Game.Map.GetTile(p.X + Game.Rand.GaussianInt(), p.Y + Game.Rand.GaussianInt());
                        if (tile != null && tile.Piece is Terrain && (Game.TEST_MAP_GEN.HasValue || !tile.Visible))
                            list.Add(tile.Location);
                    }
                    return list;
                }));

                FoundPath target = null;
                Point final = path[^1];
                if (final != to.Location)
                    target = FoundPaths[final];
                FoundPath foundPath = new(path, target, movement);
                for (int a = 0; a < path.Count - 1; a++)
                {
                    FoundPaths.TryGetValue(path[a], out FoundPath old);
                    if (foundPath.Movement < (old?.Movement ?? double.MaxValue))
                        FoundPaths[path[a]] = foundPath;//should join together so that faster aliens can switch over to faster path
                }
                if (target != null)
                    path.AddRange(target.CompletePath(final));

                return path;
            }

            return null;
        }

        private static Func<Map, Point, Func<Tile, Terrain>, Tile> NewTile;
    }
}
