using ClassLibrary1.Pieces;
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

        private static Func<Map, Point, Func<Tile, Terrain>, Tile> NewTile;

        private const double TWO_PI = Math.PI * 2;
        private const double HALF_PI = Math.PI / 2.0;

        public readonly Game Game;

        private readonly double featureDist;
        private readonly Noise noise;
        private readonly Path[] _paths;
        private readonly List<Cave> _caves;
        private readonly HashSet<Point> clearTerrain;

        private readonly Dictionary<Point, Piece> _pieces;
        private readonly HashSet<Point> _explored;
        private Rectangle _gameBounds;

        private readonly Dictionary<ResourceType, int> resourcePool;

        internal IEnumerable<Piece> AllPieces => _pieces.Values;

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

            const double dev = .21, oe = .13;
            featureDist = Game.Rand.GaussianOE(Consts.FeatureDist, dev, oe, Consts.FeatureMin);
            double max = Game.Rand.GaussianOE(Consts.NoiseDistance, dev, oe, Consts.FeatureMin);
            double min = Game.Rand.GaussianOE(13, dev, oe, Game.Rand.Range(2, 4));
            int steps = Game.Rand.GaussianOEInt(5.2, dev, oe, Game.Rand.RangeInt(2, 5));
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

            _paths = new Path[numPaths];
            for (int d = 0; d < numPaths; d++)
                _paths[d] = new Path(angles[d]);

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
                { ResourceType.Biomass, 3 }, { ResourceType.Artifact, 3 }, { ResourceType.Metal, 6 }, };

            LogEvalTime();
        }

        internal double ClosestCaveDistSqr(Tile tile) => _caves.Min(c => GetDistSqr(tile.X, tile.Y, c.Center));
        internal void NewGame()
        {
            //order is important - the starting resource distribution is different than the refils
            GenerateStartResources();
            InitExplorePaths();
            SpawnHives();

            foreach (var cave in Game.Rand.Iterate(_caves))
                cave.PathFind(this);//, Game.Player.Core.Tile);

            if (Game.TEST_MAP_GEN.HasValue)
            {
                int v = Game.TEST_MAP_GEN.Value;
                foreach (var p in Game.Rand.Iterate(-v, v, -v, v))
                    CreateTreasure(GetTile(p));
            }
        }

        private void GenerateStartResources()
        {
            const int startResources = 8;
            GenResources(_ => StartTile(), 1.3 / startResources, startResources);
        }
        private void InitExplorePaths()
        {
            foreach (var explore in Game.Rand.Iterate(_paths))
                explore.Explore(this, Consts.PathWidth);
        }
        private void SpawnHives()
        {
            double spawnHives = Math.PI - 1 + _caves.Count;
            int hives = Game.Rand.GaussianOEInt(spawnHives, .091, .039, Game.Rand.Round(3.9));
            spawnHives = (spawnHives + hives) / 2.0 + 1;

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
                cave.AddHive(Hive.NewHive(tile, f, cave.Spawner));

                counts.TryGetValue(cave, out int count);
                counts[cave] = count + 1;
            }

            int cavesLeft = _caves.Count, resources = Game.Rand.GaussianCappedInt(cavesLeft + 3, .13);
            double avgHives = hives / (double)cavesLeft + 1;
            foreach (var cave in Game.Rand.Iterate(_caves))
            {
                counts.TryGetValue(cave, out int caveHives);
                caveHives++;
                int spawn = resources;
                if (cavesLeft > 1)
                {
                    double avg = resources / (double)cavesLeft * avgHives / caveHives;
                    int cap = (int)Math.Ceiling(Math.Max(2 * avg - resources, 0));
                    spawn = Math.Min(resources, avg > cap ? Game.Rand.GaussianCappedInt(avg, 1, cap) : Game.Rand.RangeInt(0, resources));
                }

                cavesLeft--;
                resources -= spawn;
                double foundationMult = Math.Min(1, caveHives / spawnHives);
                GenResources(type => cave.SpawnTile(this, type), foundationMult, spawn);
            }
        }

        internal void PlayTurn(int turn)
        {
            foreach (var p in _paths)
                p.Spawner.Turn(turn);
            foreach (var c in _caves)
                c.Spawner.Turn(turn);
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

            double eval = noise.Evaluate(x, y);
            double dist = Tile.GetDistance(p, new(0, 0));
            mult += (featureDist / dist / dist / Math.Abs(eval - .5));
            //mult++;

            //lineDist = (float)minLineDist;
            float value = (float)(eval * mult);
            evaluateCache.Add(p, value);// new(value, lineDist));

            watch.Stop();
            return value;
        }
        private static double GetDistSqr(PointD v, PointD w) => GetDistSqr(v.X, v.Y, w);
        private static double GetDistSqr(double x, double y, PointD point) => GetDistSqr(x, y, point.X, point.Y);
        private static double GetDistSqr(double x1, double y1, double x2, double y2)
        {
            double distX = x1 - x2, distY = y1 - y2;
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
                //double dist = Tile.GetDistance(p, new(0, 0));
                bool clear = false;// Math.Abs(noise.Evaluate(p.X, p.Y) - .5) < Consts.CaveDistance / dist / dist;
                if (!chasm && !clear && terrain < 1 / 4.0)
                    return null;
                chasm |= !clear && terrain < 1 / 2.0;
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

            Rectangle bounds = _gameBounds;

            //int x = Math.Min(_pieces.Keys.Min(p => p.X), bounds.X);//
            //int y = Math.Min(_pieces.Keys.Min(p => p.Y), bounds.Y);//
            //int w = Math.Max(_pieces.Keys.Max(p => p.X), bounds.Right) - x + 1;//
            //int h = Math.Max(_pieces.Keys.Max(p => p.Y), bounds.Bottom) - y + 1;//
            //bounds = new(x, y, w, h);//

            return bounds;
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

            //if (!visible)//
            //    visible = _pieces.TryGetValue(tile, out var p) && p is Alien;//
            //visible |= _pieces.ContainsKey(tile);//

            return Game.TEST_MAP_GEN.HasValue || Game.GameOver || visible;
        }

        internal void AddPiece(Piece piece)
        {
            _pieces.Add(piece.Tile.Location, piece);

            if (piece is PlayerPiece playerPiece)
            {
                UpdateVision(playerPiece);
                Treasure.Collect(piece.Tile);
            }
        }
        internal void RemovePiece(Piece piece)
        {
            _pieces.Remove(piece.Tile.Location);
        }
        internal bool UpdateVision(PlayerPiece playerPiece) => UpdateVision(playerPiece.Tile.Location, playerPiece.Vision);
        internal void UpdateVision(IEnumerable<Tile> tiles)
        {
            foreach (var t in tiles)
                if (t != null)
                    UpdateVision(t.Location, 0);
        }
        internal bool UpdateVision(Point point, double range)
        {
            LogEvalTime();

            bool found = false;
            foreach (Point p in Tile.GetPointsInRangeBlocked(this, point, range))
                if (_explored.Add(p))
                {
                    if (Game.Rand.Next(Consts.ExploreForResearch) == 0)
                        Game.Player.Research.AddBackground();

                    Tile explored = GetTile(p);
                    CreateTreasure(explored);
                    found |= explored != null && explored.Piece != null && explored.Piece is not Terrain;
                }

            Explore(point, range);

            int vision = (int)range + 1;
            int x = Math.Min(_gameBounds.X, point.X - vision);
            int y = Math.Min(_gameBounds.Y, point.Y - vision);
            int right = Math.Max(_gameBounds.Right, point.X + vision + 1);
            int bottom = Math.Max(_gameBounds.Bottom, point.Y + vision + 1);
            _gameBounds = new Rectangle(x, y, right - x, bottom - y);

            LogEvalTime();

            return found;
        }
        private void CreateTreasure(Tile tile)
        {
            static bool Clear(Tile t) => t != null && (t.Piece == null || t.Piece.HasBehavior<IMovable>());
            if (Clear(tile) && tile.Piece == null && tile.GetAdjacentTiles().Where(Clear).Skip(1).Any())
            {
                int x = tile.X, y = tile.Y;

                var dist = _caves.Select(c => c.Center).Concat(_paths.Select(p => p.GetClosestPoint(x, y)))
                    .Select(p => GetDistSqr(p, new(x, y))).Concat(_caves.Select(c => c.ConnectionDistSqr(x, y)))
                    .Min() + 1;
                dist = Math.Sqrt(dist) / Consts.PathWidth / 2;

                double chance;
                if (dist > 1.5)
                    chance = .21 - 1 / (dist - .5) / 5;
                else
                    chance = .01 * dist / 1.5;
                chance /= Consts.TreasureDiv;

                if (Game.Rand.Bool(chance))
                    Treasure.NewTreasure(tile);
            }
        }

        public static void LogEvalTime()
        {
            if (evalCount > 0)
            {
                //float evalTime = 1000f * watch.ElapsedTicks / Stopwatch.Frequency;
                //Debug.WriteLine($"Evaluate ({evalCount}): {evalTime}");
                watch.Reset();
                evalCount = 0;
            }
        }

        internal Tile StartTile() => SpawnTile(new(0, 0), Consts.PathWidth + Consts.ResourceAvgDist, false);
        private Tile SpawnTile(PointD spawnCenter, double deviation, bool isEnemy, Func<Tile, bool> Valid = null)
        {
            int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(deviation));
            Tile tile;
            do
            {
                tile = GetTile(RandCoord(spawnCenter.X), RandCoord(spawnCenter.Y));
                deviation += Game.Rand.DoubleFull(Consts.CavePathSize);
            }
            while ((Valid != null && !Valid(tile)) || InvalidStartTile(tile, isEnemy));

            //Debug.WriteLine($"SpawnTile ({Angle:0.00}) {distance:0.0}: {spawnCenter} -> {tile}");

            return tile;
        }
        internal static bool InvalidStartTile(Tile tile, bool isEnemy)
        {
            if (tile == null) return true;

            bool visible = tile.Visible && !tile.Map.Game.GameOver && !Game.TEST_MAP_GEN.HasValue;
            bool hiveRange = isEnemy && tile.Map._pieces.OfType<Hive>().Any(h => tile.GetDistance(h.Tile) <= h.MaxRange);
            Core core = tile.Map.Game.Player.Core;
            bool coreRange = core != null && tile.GetDistance(core.Tile) <= core.GetBehavior<IRepair>().Range;
            bool invalid = (visible && !hiveRange) || tile.Piece != null || coreRange;
            //if (!invalid)
            //    Debug.WriteLine("InvalidStartTile: " + tile);
            return invalid;
        }

        internal void Explore(Point point, double vision)
        {
            if (point.X != 0 || point.Y != 0)
            {
                double angle = GetAngle(point.X, point.Y);
                Path explore = Game.Rand.Iterate(_paths).OrderBy(path => GetAngleDiff(path.Angle, angle)).First();
                explore.Explore(this, point, vision);
                foreach (Cave c in _caves)
                    c.Explore(point, vision);
            }
        }
        internal void GenResources(Func<ResourceType, Tile> GetTile, double foundationMult = 1, int numResources = 1)
        {
            for (int a = 0; a < numResources; a++)
            {
                if (resourcePool.Values.Any(v => v == 0))
                {
                    resourcePool[ResourceType.Artifact] += 2;
                    resourcePool[ResourceType.Foundation] += 4;
                    resourcePool[ResourceType.Biomass] += 5;
                    resourcePool[ResourceType.Metal] += 6;
                }

                ResourceType type;
                do
                    type = Game.Rand.SelectValue(resourcePool);
                while (type == ResourceType.Foundation && !Game.Rand.Bool(foundationMult));
                resourcePool[type]--;

                Tile tile = GetTile(type);
                switch (type)
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
        }

        internal double GetMinSpawnMove(Tile tile)
        {
            Cave cave = _caves.OrderBy(c => GetDistSqr(new(tile.X, tile.Y), c.Center)).First();
            return cave.MinSpawnMove;
        }
        internal Tile GetEnemyTile(double enemyMove)
        {
            var choices = _paths.Concat<IEnemySpawn>(_caves)
                    .Concat(Game.Enemy.PiecesOfType<EnemyPiece>().Select(p => p.Spawn).Where(s => s is not null))
                .ToDictionary(k => k, v => v.SpawnChance(Game.Turn, enemyMove));
            foreach (var choice in choices)
                Debug.WriteLine($"choice - {choice.Key}: {choice.Value}");
            IEnemySpawn spawn = Game.Rand.SelectValue(choices);
            spawn.Spawner.Spawned();
            Debug.WriteLine($"GetEnemyTile: {spawn}");
            return spawn.SpawnTile(this);
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
        //line equation in the format ax + by + c = 0 
        internal static double PointLineDistanceAbs(double a, double b, double c, Point point) =>
            Math.Abs(PointLineDistance(a, b, c, point));
        private static double PointLineDistance(double a, double b, double c, Point point) =>
            (a * point.X + b * point.Y + c) / Math.Sqrt(a * a + b * b);

        private readonly Dictionary<Point, FoundPath> corePaths = new();
        public Dictionary<Point, FoundPath> EnemyPaths => Game.TEST_MAP_GEN.HasValue ? corePaths : null; //|| Game.GameOver 
        internal List<Point> PathFindCore(Tile from, double movement, Func<HashSet<Point>, bool> Accept)
        {
            if (corePaths.TryGetValue(from.Location, out FoundPath found) && found.Movement <= movement)
                return found.CompletePath(from.Location).ToList();

            HashSet<Point> known = corePaths.Keys.Where(k => corePaths[k].Movement <= movement).ToHashSet();

            Tile to = Game.Enemy.PiecesOfType<Portal>().Where(p => !p.Exit).Select(p => p.Tile)
                .Append(Game.Player.Core.Tile).OrderBy(t => from.GetDistance(t)).First();
            var path = PathFind(from, to, movement, false, movement, true, false, p2 =>
                {
                    //the map is infinite, so to avoid pathfinding forever we impose a penalty on blocked terrain instead of blocking tiles entirely
                    double penalty = 0;
                    Tile tile = GetTile(p2);
                    if (tile == null)
                    {
                        penalty = Game.Rand.GaussianCapped((Consts.PathWidth + movement) * 2.25 * Consts.PathWidth, .065);
                    }
                    else if (tile.Piece is Block block)
                    {
                        double mult = .5 + 4 * (.5 - block.Value); //ranges from 0.5 - 1.5
                        mult *= mult; //ranges from 0.25 - 2.25
                        penalty = Game.Rand.GaussianCapped((Consts.PathWidth + movement) * mult, .065);
                    }
                    if (penalty > 0 && !Game.GameOver && !Game.TEST_MAP_GEN.HasValue && Visible(p2))
                        penalty *= Consts.PathWidth;
                    return penalty;
                }, known.Contains, out var blocked);

            if (Accept(blocked))
            {
                //clear any blocked terrain we pathed through 
                clearTerrain.UnionWith(blocked.SelectMany(p =>
                {
                    if (!Game.GameOver && !Game.TEST_MAP_GEN.HasValue && Visible(p))
                        Debug.WriteLine($"!!! Cleared terrain on visible tile! {p}");

                    List<Point> list = new() { p };
                    int extra = Game.Rand.OEInt();
                    for (int a = 0; a < extra; a++)
                    {
                        Tile tile = Game.Map.GetTile(p.X + Game.Rand.GaussianInt(), p.Y + Game.Rand.GaussianInt());
                        if (tile != null && tile.Piece is Terrain && (Game.TEST_MAP_GEN.HasValue || Game.GameOver || !tile.Visible))
                            list.Add(tile.Location);
                    }
                    return list;
                }));

                FoundPath target = null;
                Point final = path[^1];
                if (final != Game.Player.Core.Tile.Location && Game.Map.GetTile(final).Piece is not Portal)
                    target = corePaths[final];
                FoundPath foundPath = new(path, target, movement);
                for (int a = 0; a < path.Count - 1; a++)
                {
                    corePaths.TryGetValue(path[a], out FoundPath old);
                    if (foundPath.Movement < (old?.Movement ?? double.MaxValue) && to.Piece is Core)
                        corePaths[path[a]] = foundPath;//should join together so that faster aliens can switch over to faster path
                }
                if (target != null)
                    path.AddRange(target.CompletePath(final));
            }
            else
            {
                path = null;
            }

            return path;
        }

        //internal Tile GetRetreatTo(Tile tile)
        //{
        //    throw new NotImplementedException();
        //}

        private IEnumerable<Tile> FindRetreatTiles(Tile tile, Func<Tile, bool> ValidRetreat)
        {
            Dictionary<PointD, double> dists = new();
            return _paths.Select(p => p.ExploredPoint(Consts.PathWidth))
                .Concat(_caves.Where(c => !c.Explored).Select(c => c.Center))
                .OrderBy(p =>
                {
                    if (dists.TryAdd(p, Math.Sqrt(GetDistSqr(tile.X, tile.Y, p)) + Game.Rand.OE(Consts.CavePathSize)))
                        ;
                    else
                        ; //if never hit can remove dists dict
                    return dists[p];
                }).Select(point => SpawnTile(point, Consts.PathWidth + Consts.CaveSize, false, ValidRetreat));
        }
        internal List<Point> PathFindRetreat(Tile from, IEnumerable<Tile> targets, double movement, double defense, Dictionary<Tile, double> playerAttacks, Func<Tile, bool> ValidRetreat)
        {
            var options = FindRetreatTiles(from, ValidRetreat);
            if (targets != null)
                options = options.Concat(targets);
            options = options.OrderBy(t => from.GetDistance(t)).ToList();
            foreach (Tile tile in options)
            {
                // Game.Rand.Bool();
                var path = PathFind(from, tile, movement, false, movement, false, false, p =>
                {
                    double att = 0;
                    Tile key = GetTile(p);
                    if (key != null)
                        playerAttacks.TryGetValue(key, out att);
                    if (att == 0)
                        return 0;
                    return Math.Sqrt((att + 1) / (defense + 1)) * Consts.PathWidth;
                },
                p => ValidRetreat(GetTile(p)),
                out var blocked);
                if (!blocked.Any())
                    return path;
            }
            return null;
        }

        //double? minFirstMove
        private List<Point> PathFind(Tile fromTile, Tile toTile, double firstMove, bool limitMove, double movement, bool includeBlocked, bool visibleOnly,
            Func<Point, double> Penalty, Func<Point, bool> Stop, out HashSet<Point> blocked)
        {
            blocked = new();

            Point from = fromTile.Location;
            Point to = toTile.Location;
            if (from == to)
                return new() { from, to, };

            //cache tile penalties at each point so they are consistent 
            Dictionary<Point, double> cache = new();

            double moveMin = firstMove;
            if (limitMove)
                firstMove += Math.Sqrt(2);
            //else
            //    moveMin = 0;
            if (movement < 1)
                movement = 1;

            bool first = firstMove >= 1;// && minFirstMove + 1 < movement;
            var path = TBSUtil.PathFind(Game.Rand, from, to, Stop, p1 =>
                {
                    IEnumerable<Point> points = Tile.GetAllPointsInRange(this, p1, first ? firstMove : movement);
                    if (first && !points.Any())
                        points = Tile.GetAllPointsInRange(this, p1, movement);
                    var result = points.Where(p =>
                        {
                            if (first && limitMove && GetDistSqr(p1.X, p1.Y, p.X, p.Y) < moveMin * moveMin)
                                return false;
                            var tile = GetTile(p);
                            if (visibleOnly && !Visible(p))
                                return false;
                            var piece = tile?.Piece;
                            if (tile == null || piece is Terrain)
                                return includeBlocked;
                            return p == from || p == to || piece is null || piece.HasBehavior<IMovable>();
                        }).Select(p2 =>
                        {
                            if (!cache.TryGetValue(p2, out double penalty))
                            {
                                penalty = Penalty(p2);
                                cache.Add(p2, penalty);
                            }
                            double dist = Tile.GetDistance(p1, p2);
                            return Tuple.Create(p2, dist + penalty);
                        }).ToList();
                    first = false;
                    return result;
                }, Tile.GetDistance);

            if (path != null)
                foreach (var p in path)
                {
                    Tile tile = GetTile(p);
                    if (tile == null || tile.Piece is Terrain)
                        blocked.Add(p);
                }

            //foreach (var p in blocked)
            //    if (!Game.TEST_MAP_GEN.HasValue && Visible(p))
            //        Debug.WriteLine($"Path through visible tile {p}");

            return path;
        }

        public List<Point> PathFind(Tile from, Tile to, double firstMove, bool limitMove, double movement, Action DoEvents)
        {
            //double[] moves = new[] {
            //    Math.Min(movable.MoveCur - 1, movable.MoveCur + movable.MoveInc - movable.MoveMax) + 1,
            //    movable.MoveCur,
            //    movable.MoveMax,
            //    //(movable.MoveInc + movable.MoveMax) / 2.0,
            //    //movable.MoveInc,
            //    (movable.MoveMax + movable.MoveLimit) / 2.0,
            //    movable.MoveLimit, };

            return PathFind(from, to, firstMove, limitMove, movement, false, true,
                _ => { DoEvents(); return 0; },
                p => !_gameBounds.Contains(p.X, p.Y), out _);
        }
    }
}
