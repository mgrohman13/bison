using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using Point = MattUtil.Point;

namespace ClassLibrary1.Map
{
    [Serializable]
    [DataContract(IsReference = true)]
    public partial class Map // : IDeserializationCallback
    {
        internal static readonly Stopwatch watch = new();
        private static int evalCount = 0;

        private static Func<Map, Point, Func<Tile, ITerrain>, Tile> NewTile;

        private const double TWO_PI = Math.PI * 2;
        private const double HALF_PI = Math.PI / 2.0;

        public readonly Game Game;

        private readonly double _featureDist;
        private readonly Noise _noise;
        private readonly Path[] _paths;
        private readonly List<Cave> _caves;
        private readonly HashSet<Point> clearTerrain;

        private readonly Dictionary<Point, Piece> _pieces;
        private readonly HashSet<Point> _explored;
        //private readonly HashSet<PointD> _plateaus;
        //private readonly double _plateauDist;
        private readonly double _hillInc, _hillRounding;
        private Rectangle _gameBounds;

        private readonly Dictionary<ResourceType, int> _resourcePool;

        internal IEnumerable<Piece> AllPieces => _pieces.Values;
        internal IEnumerable<PointD> AllFoundations =>
            AllPieces.Where(p => p is Foundation || p is FoundationPiece).Select(p => p.Tile.LocationD);

        static Map()
        {
            //static init to set NewTile
            Tile.GetDistance(0, 0, 0, 0);
        }

        internal Map(Game game)
        {
            LogEvalTime();

            Game = game;
            clearTerrain = [];

            const double dev = .21, oe = .13;
            _featureDist = Game.Rand.GaussianOE(Consts.FeatureDist, dev, oe, Consts.FeatureMin);
            double max = Game.Rand.GaussianOE(Consts.NoiseDistance, dev, oe, Consts.FeatureMin);
            double min = Game.Rand.GaussianOE(13, dev, oe, Game.Rand.Range(2, 4));
            int steps = Game.Rand.GaussianOEInt(5.2, dev, oe, Game.Rand.RangeInt(2, 5));
            double weightScale = Game.Rand.Weighted(.78) + Game.Rand.OE(.13);
            _noise = new Noise(Game.Rand, min, max, steps, .065, weightScale);

            int numPaths = Game.Rand.GaussianOEInt(Math.PI, .091, .039, 2);
            double separation = Consts.PathMinSeparation;
            separation = Game.Rand.GaussianCapped(separation, .104, Math.Max(0, 2 * separation - TWO_PI)) / numPaths;
            double[] angles;
            bool valid;
            do
            {
                angles = [.. Enumerable.Repeat(0, numPaths).Select(x => Game.Rand.NextDouble() * TWO_PI)];
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
            _caves = [];
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

            _pieces = [];
            _explored = [];

            _resourcePool = new() { { ResourceType.Foundation, 1 },
                { ResourceType.Biomass, 3 }, { ResourceType.Artifact, 3 }, { ResourceType.Metal, 6 }, };

            //GeneratePlateaus(0);

            // Game.Rand.GaussianOEInt(1.0 / Island.MAX_VISION, 1, .13);
            const double maxHill = Island.MAX_VISION / 2.0;
            _hillInc = Game.Rand.Weighted(.91) + Game.Rand.OE(1 / maxHill)
                + Game.Rand.Weighted(maxHill, 1 / maxHill);// + Game.Rand.Weighted(maxHill, Math.Sqrt(1.0 / maxHill));
            _hillRounding = Game.Rand.NextDouble();

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

            clearTerrain.UnionWith(_explored.SelectMany(e => Tile.GetAllPointsInRange(this, e, Rand())));
            static double Rand() => Game.Rand.GaussianCapped(Constructor.BASE_VISION / 2.0, 1);
        }
        internal void CheckStart()
        {
            Core core = Game.Player.Core;
            foreach (Point p in Game.Rand.Iterate(_explored.Concat(core.Tile.GetAllPointsInRange(core.GetBehavior<IBuilder>().Range))))
            {
                Piece piece = GetTile(p).Piece;
                if (piece is ITerrain)
                {
                    if (!Game.TEST_MAP_GEN.HasValue && piece.Tile.Visible)
                        throw new Exception();
                }
                else if (piece != null && !piece.IsPlayer)
                {
                    piece.SetTile(StartTile(p));
                }
            }
        }

        private void GenerateStartResources()
        {
            const int startResources = 8;
            GenResources(StartTile, 1.3 / startResources, startResources);
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

            Dictionary<Cave, int> chances = [], counts = [];
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
                GenResources(() => cave.SpawnTile(this, false), foundationMult, spawn);
            }
        }

        internal void PlayTurn(int turn)
        {
            foreach (var p in Game.Rand.Iterate(_paths))
                p.Spawner.Turn(turn);
            foreach (var c in Game.Rand.Iterate(_caves))
                c.Spawner.Turn(turn);
        }

        //public float EvalNull(Point p)
        //{
        //    float eval = float.NaN;
        //    if (Visible(p))
        //    {
        //        eval = Evaluate(p);
        //        if (eval < .25)
        //        {
        //            eval *= 4;
        //            if (Tile.GetAllPointsInRange(this, p, Attack.MELEE_RANGE).Any(n => GetTile(n) is not null))
        //                eval = 1;
        //        }
        //        else
        //            eval = float.NaN;
        //    }
        //    return eval;
        //}

        [NonSerialized]
        private Dictionary<Point, Tuple<float, float>> evaluateCache;
        private Tuple<float, float> Evaluate(Point point)
        {
            int x = point.X, y = point.Y;
            evaluateCache ??= [];
            if (evaluateCache.TryGetValue(point, out var t))
                return t;

            watch.Start();
            evalCount++;

            double mult = 0;
            mult += _paths.Sum(p => p.Evaluate(point));
            mult += _caves.Sum(c => c.Evaluate(x, y));

            double eval = _noise.Evaluate(x, y);
            double dist = Tile.GetDistance(point, new(0, 0)) + 1;

            double offset = Math.Pow(float.Epsilon, 1.0 / 3);
            mult += (_featureDist / dist / dist / (offset + Math.Abs(eval - .5)));

            float value1 = (float)(eval);
            float value2 = (float)(eval * mult);
            if (double.IsInfinity(value2))
                ;
            Tuple<float, float> retVal = Tuple.Create(value1, value2);
            evaluateCache.Add(point, retVal);

            watch.Stop();
            return retVal;
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
            Func<Tile, ITerrain> GetTerrain = t => null;
            if (!clearTerrain.Contains(p))
            {
                bool block = false, island = false;
                double vision = 0;

                Tuple<float, float> evaluate = Evaluate(p);
                double terrain = evaluate.Item2;
                //, out float lineDist);
                //also use dist from center?
                // (5 * terrain * Consts.PathWidth + lineDist) / 2.0 % Consts.PathWidth < 1;
                //double dist = Tile.GetDistance(p, new(0, 0));
                //bool clear = false;// Math.Abs(noise.Evaluate(p.X, p.Y) - .5) < Consts.CaveDistance / dist / dist;
                if (!block && terrain < 1 / 4.0)//&& !clear
                    return null;
                block |= terrain < 1 / 2.0;//!clear &&  

                if (!block)//&& AllFoundations.Any())
                {
                    double m = evaluate.Item2 / .5;
                    m = Math.Sqrt(m);
                    if (m > 1)
                        m = .013 + Math.Pow(m, .13);
                    vision = Math.Pow(evaluate.Item1, 2.1) * m;
                    const double cutoff = .39;
                    island = vision > cutoff;
                    double max = Island.MAX_VISION;
                    vision -= cutoff;
                    vision /= 1 - cutoff;
                    vision = max * vision;
                    max /= 2;
                    if (vision > max)
                        vision = max + max * (vision - max) / vision;

                    //use some kind of offset into a lookup table???
                    vision = 1 + MTRandom.Round(vision / _hillInc, _hillRounding) * _hillInc;

                    //vision *= Island.MAX_VISION;
                    ////double mult = AllFoundations.Select(f => GetDistSqr(p.X, p.Y, f)).Min();
                    ////if (mult > 0)
                    ////{
                    ////    mult = Consts.PathWidth * Consts.PathWidth / mult;  //const double l1 = .21;
                    ////const double l1 = .39; 
                    ////if (mult < l1) //go back to Math.Min so only affects close by
                    ////    mult = Math.Pow(mult / l1, 1.0 / 65) * l1;
                    //////mult = Math.Max(mult, .39);
                    ////const double l2 = 1.3;
                    ////if (mult > l2)
                    ////    mult = l2 + Math.Pow(1 + mult - l2, 1.0 / 13) - 1;
                    //double m2 = evaluate.Item1 / .5;
                    ////m2 *= m2 * m2;
                    //island = m2 > 1;//* mult
                    //                //vision = Math.Max(0, m2 * Math.Sqrt(mult) - 1);
                    //                //    vision *= Consts.IslandVisionMult;
                    //                //    vision++;
                    //                //}

                }
                if (double.IsInfinity(vision))
                    ;
                GetTerrain = t => block ? new Block(t, terrain) : island ? new Island(t, vision) : null;
            }

            Piece piece = GetPiece(p);
            return piece == null ? NewTile(this, p, GetTerrain) : piece.Tile;

            //ITerrain GetTerrain(Tile t) =>
            //    block ? new Block(t, terrain) : island ? new Island(t, vision) : null;
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
        internal bool UpdateVision(PlayerPiece playerPiece)
        {
            double vision = playerPiece.Vision;
            if (vision > 0)
                return UpdateVision(playerPiece.Tile.Location, vision);
            return false;
        }
        internal void UpdateVision(IEnumerable<Tile> tiles)
        {
            foreach (var t in Game.Rand.Iterate(tiles))
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
                    //GeneratePlateaus(Tile.GetDistance(p, new Point(0, 0)));

                    if (Game.Rand.Next(Consts.ExploreForResearch) == 0)
                        Game.Player.Research.AddBackground();

                    Tile explored = GetTile(p);
                    CreateTreasure(explored);
                    found |= explored != null && explored.Piece != null && explored.Piece is not ITerrain;
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

        //private void GeneratePlateaus(double v)
        //{
        //    //generate constant density
        //    //set generation buffer accordingly
        //    //use for constant heights
        //    //_plateausl
        //}

        private HashSet<PointD> _treasures = [];
        private void CreateTreasure(Tile tile)
        {
            static bool Clear(Tile t) => t != null && (t.Piece == null || t.Piece.HasBehavior<IMovable>());
            if (Clear(tile) && tile.Piece == null && tile.GetAdjacentTiles().Where(Clear).Skip(1).Any())
            {
                int x = tile.X, y = tile.Y;

                Tile core = Game.Player.Core?.Tile;
                if (core is not null)
                {
                    var dist = _treasures.Concat([core.LocationD])
                        .Concat(_caves.Select(c => c.Center))
                        .Concat(_paths.Select(p => p.GetClosestPoint(x, y)))
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
                    {
                        Treasure.NewTreasure(tile);
                        if (Game.Rand.Bool())//Consts.TreasureSpacingChance
                            _treasures.Add(tile.LocationD);
                    }
                }
                else
                    ;
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

        internal Tile StartTile() => StartTile(new(0, 0));
        internal Tile StartTile(Point center) => SpawnTile(new(center.X, center.Y), Consts.PathWidth + Consts.ResourceAvgDist, false);
        private Tile SpawnTile(PointD center, double deviation, bool isEnemy, Func<Tile, bool> Valid = null)
        {
            int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(deviation));
            Tile tile;
            do
            {
                tile = GetTile(RandCoord(center.X), RandCoord(center.Y));
                deviation += Game.Rand.DoubleFull(Consts.CavePathWidth);
            }
            while ((Valid != null && !Valid(tile)) || InvalidStartTile(tile, isEnemy));

            //Debug.WriteLine($"SpawnTile ({Angle:0.00}) {distance:0.0}: {spawnCenter} -> {tile}");

            return tile;
        }
        internal static bool InvalidStartTile(Tile tile, bool isEnemy)
        {
            if (tile == null)
                return true;

            bool visible = tile.Visible && !tile.Map.Game.GameOver && !Game.TEST_MAP_GEN.HasValue;
            bool hiveRange = isEnemy && tile.Map._pieces.Values.OfType<Hive>().Any(h => tile.GetDistance(h.Tile) <= h.MaxRange);
            Core core = tile.Map.Game.Player.Core; //
            bool coreRange = core != null && tile.GetDistance(core.Tile) <= core.GetBehavior<IRepair>().Range;
            bool invalid = (visible && !hiveRange) || tile.Piece != null || coreRange;
            //if (!invalid)
            //    Debug.WriteLine("InvalidStartTile: " + tile);
            return invalid;
        }

        internal void Explore(Point point, double vision)
        {
            foreach (Path p in Game.Rand.Iterate(_paths))
                p.Explore(this, point, vision);
            foreach (Cave c in Game.Rand.Iterate(_caves))
                c.Explore(point, vision);
        }
        internal void GenResources(Func<Tile> GetTile, double foundationMult, int numResources = 1)
        {
            for (int a = 0; a < numResources; a++)
            {
                Tile tile = GetTile();
                double distMult = _caves.Select(c => c.Center).Concat(AllFoundations)
                    .Concat(clearTerrain.Concat(Game.Player.Core.Tile.GetAllPointsInRange(Constructor.BASE_VISION))
                        .Select(p => new PointD(p.X, p.Y)))
                    .Min(p => Tile.GetDistanceD(tile.X, tile.Y, p.X, p.Y));
                distMult /= (distMult + (Consts.CaveSize + Consts.PathWidth) / 6.5);
                distMult *= distMult * distMult;
                if (!Game.TEST_MAP_GEN.HasValue && tile.Visible)
                    foundationMult = 0;

                if (_resourcePool.Values.Any(v => v <= 0))
                {
                    _resourcePool[ResourceType.Artifact] += 2;
                    _resourcePool[ResourceType.Foundation] += 4;
                    _resourcePool[ResourceType.Biomass] += 5;//swap?
                    _resourcePool[ResourceType.Metal] += 6;//swap? - inc start metal further?
                }

                ResourceType type;
                do
                    type = Game.Rand.SelectValue(_resourcePool);
                while (type == ResourceType.Foundation && !Game.Rand.Bool(foundationMult * distMult));
                _resourcePool[type]--;

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

                        int count = 0;
                        double avg = (Math.E + Math.PI) / 2.0; //~2.930
                        int size = Game.Rand.GaussianOEInt(avg, .21, .26, 1);
                        while (true)
                        {
                            count++;
                            Foundation.NewFoundation(tile);
                            if (count >= size)
                                break;

                            Dictionary<Tile, int> neighbors = [];
                            while (neighbors.Count == 0 && Game.Rand.Next(169) > 0)
                                neighbors = tile.GetPointsInRangeUnblocked(1 + Game.Rand.OE())
                                    .Select(this.GetTile)
                                    .Where(t => t != null && t.Piece == null && (Game.TEST_MAP_GEN.HasValue || !t.Visible))
                                    .ToDictionary(t => t, CountAdjacent);
                            if (neighbors.Count == 0)
                                break;
                            tile = Game.Rand.SelectValue(neighbors);
                        }

                        _resourcePool[type] -= Game.Rand.Round(count / avg) - 1; //- 1 to account for the first one already removed from the pool
                        break;
                }
            }
            static int CountAdjacent(Tile tile)
            {
                int count = tile.GetAdjacentTiles().Where(t => t.Piece is Foundation).Count();
                return 1 + (1 + count) * count;// * count;
            }
        }

        internal double GetMinSpawnMove(Tile tile)
        {
            Cave cave = _caves.OrderBy(c => GetDistSqr(tile.LocationD, c.Center)).First();
            return cave.MinSpawnMove;
        }
        internal Tile GetEnemyTile(double enemyMove)
        {
            var choices = GetSpawners()
                .ToDictionary(k => k.Item1, v => v.Item1.SpawnChance(Game.Turn, enemyMove));
            foreach (var choice in choices)
                Debug.WriteLine($"choice - {choice.Key}: {choice.Value}");
            IEnemySpawn spawn = Game.Rand.SelectValue(choices);
            spawn.Spawner.Spawned();
            Debug.WriteLine($"GetEnemyTile: {spawn}");
            return spawn.SpawnTile(this);
        }
        internal IEnemySpawn GetClosestSpawner(Point location)
        {
            var spawns = GetSpawners().Select(t =>
                {
                    double xDiff = location.X - t.Item2.X;
                    double yDiff = location.Y - t.Item2.Y;
                    int chance = Game.Rand.Round(int.MaxValue / (13 + xDiff * xDiff + yDiff * yDiff));
                    return new Tuple<IEnemySpawn, int>(t.Item1, chance);
                }).ToDictionary(t => t.Item1, t => t.Item2);
            return Game.Rand.SelectValue(spawns);
        }
        private IEnumerable<Tuple<IEnemySpawn, PointD>> GetSpawners() =>
            _paths.Select(p => new Tuple<IEnemySpawn, PointD>(p, p.ExploredPoint()))
                .Concat(_caves.Select(c => new Tuple<IEnemySpawn, PointD>(c, c.Center)))
                .Concat(Game.Enemy.PiecesOfType<EnemyPiece>().Select(p =>
                    new Tuple<IEnemySpawn, PointD>(p.Spawn, p.Tile.LocationD)).Where(t => t.Item1 is not null));


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
        private static double PointLineDistanceSigned(PointD linePoint, double angle, Point point)
        {
            Path.CalcLine(linePoint, angle, out double a, out double b, out double c);

            double dist = PointLineDistance(a, b, c, point);
            if (GetAngleDiff(angle, Math.PI) < HALF_PI)
                dist *= -1;
            return dist;
        }
        //line equation in the format ax + by + c = 0 
        internal static double PointLineDistanceAbs(double a, double b, double c, Point point) =>
            Math.Abs(PointLineDistance(a, b, c, point));
        private static double PointLineDistance(double a, double b, double c, Point point) =>
            (a * point.X + b * point.Y + c) / Math.Sqrt(a * a + b * b);

        private readonly Dictionary<Point, FoundPath> corePaths = [];
        public Dictionary<Point, FoundPath> EnemyPaths => Game.TEST_MAP_GEN.HasValue ? corePaths : null; //|| Game.GameOver 
        internal List<Point> PathFindCore(Tile from, double movement, Func<HashSet<Point>, bool> Accept)
        {
            if (corePaths.TryGetValue(from.Location, out FoundPath found) && found.Movement <= movement)
                return [.. found.CompletePath(from.Location)];

            HashSet<Point> known = [.. corePaths.Keys.Where(k => corePaths[k].Movement <= movement)];

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

                    List<Point> list = [p];
                    int extra = Game.Rand.OEInt();
                    for (int a = 0; a < extra; a++)
                    {
                        Tile tile = Game.Map.GetTile(p.X + Game.Rand.GaussianInt(), p.Y + Game.Rand.GaussianInt());
                        if (tile != null && tile.Piece is ITerrain && (Game.TEST_MAP_GEN.HasValue || Game.GameOver || !tile.Visible))
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
            Dictionary<PointD, double> dists = [];
            return _paths.Select(p => p.ExploredPoint(Consts.PathWidth))
                .Concat(_caves.Where(c => !c.Explored).Select(c => c.Center))
                .OrderBy(p =>
                {
                    if (dists.TryAdd(p, Math.Sqrt(GetDistSqr(tile.X, tile.Y, p)) + Game.Rand.OE(Consts.CavePathWidth)))
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
            options = [.. options.OrderBy(t => from.GetDistance(t))];
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
                if (blocked.Count == 0)
                    return path;
            }
            return null;
        }

        //double? minFirstMove
        private List<Point> PathFind(Tile fromTile, Tile toTile, double firstMove, bool limitMove, double movement, bool includeBlocked, bool visibleOnly,
            Func<Point, double> Penalty, Func<Point, bool> Stop, out HashSet<Point> blocked)
        {
            blocked = [];

            Point from = fromTile.Location;
            Point to = toTile.Location;
            if (from == to)
                return [from, to,];

            //cache tile penalties at each point so they are consistent 
            Dictionary<Point, double> cache = [];

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
                            if (tile == null || piece is ITerrain)
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
                    if (tile == null || tile.Piece is ITerrain)
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
