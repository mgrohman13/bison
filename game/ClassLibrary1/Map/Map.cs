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
        internal static readonly Stopwatch _watch = new();
        private static int _evalCount = 0;

        private static Func<Map, Point, Func<Tile, ITerrain>, Tile> NewTile;

        private const double TWO_PI = Math.PI * 2;
        private const double HALF_PI = Math.PI / 2.0;

        public readonly Game Game;

        private readonly double _featureDist;
        private readonly Noise _noise;
        private readonly Path[] _paths;
        private readonly Cave[] _caves;
        private readonly List<Elevation> _elevation;
        private readonly HashSet<Point> _clearTerrain;

        //private double _maxExplore;
        private double _nextElevation;

        private readonly Dictionary<Point, Piece> _pieces;
        private readonly HashSet<Point> _explored;
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

            const double dev = .21, oe = .13;
            double max = Game.Rand.GaussianOE(game.Consts.NoiseDistance, dev, oe, game.Consts.FeatureMin);
            double min = Game.Rand.GaussianOE(13, dev, oe, Game.Rand.Range(2, 4));
            int steps = Game.Rand.GaussianOEInt(5.2, dev, oe, Game.Rand.RangeInt(2, 5));
            double weightScale = Game.Rand.Weighted(.78) + Game.Rand.OE(.13);
            _featureDist = Game.Rand.GaussianOE(game.Consts.FeatureDist, dev, oe, game.Consts.FeatureMin);
            _noise = new Noise(Game.Rand, min, max, steps, .065, weightScale);

            _clearTerrain = [];
            _pieces = [];
            _explored = [];

            _resourcePool = new() { { ResourceType.Foundation, 1 },
                { ResourceType.Biomass, 3 }, { ResourceType.Artifact, 3 }, { ResourceType.Metal, 6 }, };

            int numPaths = Game.Rand.GaussianOEInt(Math.PI, game.Consts.PathDev / 2.0, .039, 2);
            double separation = game.Consts.PathMinSeparation;
            separation = Game.Rand.GaussianCapped(separation, .104, Math.Max(0, 2 * separation - TWO_PI)) / numPaths;
            _paths = GeneratePaths(numPaths, separation, out double[] pathAngles);
            _caves = GenerateCaves(numPaths, separation, pathAngles);

            _elevation = [];
            //GeneratePlateaus(0);

            LogEvalTime();
        }

        private Path[] GeneratePaths(int numPaths, double separation, out double[] angles)
        {
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

            Path[] paths = new Path[numPaths];
            for (int d = 0; d < numPaths; d++)
                paths[d] = new Path(Game.Consts, angles[d]);
            return paths;
        }
        private Cave[] GenerateCaves(int numPaths, double separation, double[] pathAngles)
        {
            separation /= 2.6;
            double caveMult = Math.PI / numPaths;
            int numCaves = Game.Rand.GaussianOEInt(2 + (Math.PI - 2) * caveMult * caveMult, Game.Consts.CaveDev, Game.Consts.CaveOE / 2.0, 2);
            List<Cave> caves = [];
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
                while (caves.Select(c => GetAngle(c.Center)).Concat(pathAngles).Any(a => GetAngleDiff(caveDir, a) < separation));

                PointD cave = GetPoint(caveDir, Game.Rand.GaussianOE(Game.Consts.CaveDistance * distMult,
                    Game.Consts.CaveDev / distMult, Game.Consts.CaveOE * 2.0, Game.Consts.CaveMinDist));
                PointD connect = Game.Rand.SelectValue(caves.Select(c => c.Center).Concat(_paths.Select(p => p.GetClosestPoint(cave.X, cave.Y)))
                     .OrderBy(p => GetDistSqr(cave, p)).Take(2));
                caves.Add(new(Game.Consts, cave, connect));//, connectCave));
            }
            if (caves.Count < 2)
                throw new Exception();
            return [.. caves];
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
        internal void Clear(Point center, double range)
        {
            ClearTerrain(Tile.GetPointsInRange(center, range).SelectMany(e => Tile.GetPointsInRange(e, Rand())));
            double Rand() => Game.Rand.DoubleHalf(range);
        }
        internal void CheckStart()
        {
            Core core = Game.Player.Core;
            foreach (Point p in Game.Rand.Iterate(_explored.Concat(core.Tile.GetPointsInRange(core.GetBehavior<IBuilder>().Range))))
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

        private void GeneratePlateaus(Point point)
        {
            double dist = Tile.GetDistance(point, new Point(0, 0));
            IEnumerable<Elevation> elevation = Elevation.GeneratePlateaus(Game.Consts, dist, ref _nextElevation);
            if (elevation.Any())
            {
                _elevation.AddRange(elevation);
                evaluateCache.Clear();
                heightCache.Clear();
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
                explore.Explore(this, Game.Consts.PathWidth);
        }
        private void SpawnHives()
        {
            double spawnHives = Math.PI - 1 + _caves.Length;
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
                Tile tile = SpawnTile(cave.Center, Game.Consts.CaveSize, true, false);
                cave.AddHive(Hive.NewHive(tile, f, cave.Spawner));

                counts.TryGetValue(cave, out int count);
                counts[cave] = count + 1;
            }

            int cavesLeft = _caves.Length, resources = Game.Rand.GaussianCappedInt(cavesLeft + 3, .13);
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
        //            if (Tile.GetAllPointsInRange(this, p, Attack.MELEE_RANGE).Any(n => GetTile(n) != null))
        //                eval = 1;
        //        }
        //        else
        //            eval = float.NaN;
        //    }
        //    return eval;
        //}

        [NonSerialized]
        private Dictionary<Point, Tuple<float, float>> evaluateCache = [];
        private Tuple<float, float> Evaluate(Point point)
        {
            int x = point.X, y = point.Y;
            evaluateCache ??= [];
            if (evaluateCache.TryGetValue(point, out var t))
                return t;

            _watch.Start();
            _evalCount++;

            GeneratePlateaus(point);

            double mult = 0;
            mult += _paths.Sum(p => p.Evaluate(this, point));
            mult += _caves.Sum(c => c.Evaluate(this, x, y));

            double eval = _noise.Evaluate(x, y);
            double dist = Tile.GetDistance(point, new(0, 0)) + 1;// + Math.Sqrt(Consts.FeatureDist);

            double offset = Math.Pow(float.Epsilon, 1.0 / 3);
            mult += (_featureDist / dist / dist / (offset + Math.Abs(eval - .5)));

            float value1 = (float)(eval);
            float value2 = (float)(eval * mult);
            if (double.IsInfinity(value2))
                ;
            Tuple<float, float> retVal = Tuple.Create(value1, value2);
            evaluateCache.Add(point, retVal);

            _watch.Stop();
            return retVal;
        }

        private static double GetDistSqr(PointD v, PointD w) => GetDistSqr(v.X, v.Y, w);
        private static double GetDistSqr(double x, double y, PointD point) => GetDistSqr(x, y, point.X, point.Y);
        private static double GetDistSqr(double x1, double y1, double x2, double y2)
        {
            double distX = x1 - x2, distY = y1 - y2;
            return distX * distX + distY * distY;
        }

        public IEnumerable<Piece> GetVisiblePieces() => _pieces.Values.Where(p => p.Tile.Visible);

        public Tile GetVisibleTile(int x, int y) => GetVisibleTile(new(x, y));
        public Tile GetVisibleTile(Point p) => Visible(p) ? GetTile(p) : null;
        internal Tile GetTile(Point p, bool visibleOnly) => visibleOnly ? GetVisibleTile(p) : this.GetTile(p);
        internal Tile GetTile(int x, int y) => GetTile(new(x, y));
        internal Tile GetTile(Point p)
        {
            Func<Tile, ITerrain> GetTerrain = t => null;
            if (!_clearTerrain.Contains(p))
            {
                bool block = false;
                double height = 0;

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

                if (!block)
                    height = GetHeight(p, evaluate);

                GetTerrain = t => block ? new Block(t, terrain) : height > 0 ? new Island(t, height) : null;
            }

            Piece piece = GetPiece(p);
            return piece == null ? NewTile(this, p, GetTerrain) : piece.Tile;

            //ITerrain GetTerrain(Tile t) =>
            //    block ? new Block(t, terrain) : island ? new Island(t, vision) : null;
        }

        [NonSerialized]
        private Dictionary<Point, float> heightCache = [];
        private float GetHeight(Point p, Tuple<float, float> evaluate)
        {
            heightCache ??= [];
            if (heightCache.TryGetValue(p, out float value))
                return value;

            List<Tuple<Elevation, double>> hills = [.. _elevation.Select(e => Tuple.Create(e, e.Dist(Game.Consts, p, evaluate)))];
            double minDist = hills.Min(h => h.Item2);
            double m1 = Elevation.Evaluate(Game.Consts, minDist);

            double m2 = evaluate.Item1 / .5;
            m2 *= m2;

            double m3 = evaluate.Item2 / .75;
            if (m3 > 1)
                m3 = Math.Pow(m3, .05);
            else
                m3 *= m3;

            double height = Math.Sqrt(m1 * m2 * m3);

            const double cutoff = 1;
            bool island = height > cutoff;
            if (island)
            {
                height -= cutoff;
                height *= Game.Consts.ElevationHeight;

                Elevation elevation = hills.Where(e => e.Item2 == minDist).Select(e => e.Item1).First();
                height = elevation.Round(Game.Consts, height, _elevation);
                island = height > .05;
            }
            if (!island)
                height = 0;

            heightCache.Add(p, (float)height);
            return (float)height;
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
        internal bool UpdateVision(Point point, double vision)
        {
            LogEvalTime();

            bool found = false;
            foreach (Point p in Tile.GetAllVision(this, point, vision))
                if (_explored.Add(p))
                {
                    if (Game.Rand.Next(Game.Consts.ExploreForResearch) == 0)
                        Game.Player.Research.AddBackground();

                    Tile explored = GetTile(p);
                    CreateTreasure(explored);
                    found |= explored != null && explored.Piece != null && explored.Piece is not ITerrain;
                }

            vision += Tile.Height(GetTile(point));
            Explore(point, vision);

            int bounds = (int)(2 + vision);
            int x = Math.Min(_gameBounds.X, point.X - bounds);
            int y = Math.Min(_gameBounds.Y, point.Y - bounds);
            int right = Math.Max(_gameBounds.Right, point.X + bounds + 1);
            int bottom = Math.Max(_gameBounds.Bottom, point.Y + bounds + 1);
            _gameBounds = new Rectangle(x, y, right - x, bottom - y);

            LogEvalTime();

            return found;
        }

        private HashSet<PointD> _treasures = [];
        private void CreateTreasure(Tile tile)
        {
            static bool Clear(Tile t) => t != null && (t.Piece == null || t.Piece.HasBehavior<IMovable>());
            if (Clear(tile) && tile.Piece == null && tile.GetAdjacentTiles().Where(Clear).Skip(1).Any())
            {
                int x = tile.X, y = tile.Y;

                Tile core = Game.Player.Core?.Tile;
                if (core != null)
                {
                    var dist = _treasures.Concat([core.LocationD])
                        .Concat(_caves.Select(c => c.Center))
                        .Concat(_paths.Select(p => p.GetClosestPoint(x, y)))
                        .Select(p => GetDistSqr(p, new(x, y))).Concat(_caves.Select(c => c.ConnectionDistSqr(x, y)))
                        .Min() + 1;
                    dist = Math.Sqrt(dist) / Game.Consts.PathWidth / 2;

                    double chance;
                    if (dist > 1.5)
                        chance = .21 - 1 / (dist - .5) / 5;
                    else
                        chance = .01 * dist / 1.5;
                    chance /= Game.Consts.TreasureDiv;

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
            if (_evalCount > 0)
            {
                //float evalTime = 1000f * watch.ElapsedTicks / Stopwatch.Frequency;
                //Debug.WriteLine($"Evaluate ({evalCount}): {evalTime}");
                _watch.Reset();
                _evalCount = 0;
            }
        }

        internal Tile StartTile() => StartTile(new(0, 0));
        internal Tile StartTile(Point center) => SpawnTile(new(center.X, center.Y), Game.Consts.PathWidth + Game.Consts.ResourceAvgDist, false);
        private Tile SpawnTile(PointD center, double dev, bool isEnemy, bool checkBounds = true, Func<Tile, bool> Valid = null) =>
            RandTile(center, dev, checkBounds, tile => (Valid == null || Valid(tile)) && !InvalidStartTile(tile, isEnemy));
        internal Tile RandTile(PointD center, double dev, bool checkBounds = true, Func<Tile, bool> Valid = null)
        {
            double mapSize = GetMapSize();
            Tile tile;
            do
            {
                tile = GetTile(RandCoord(center.X), RandCoord(center.Y));

                dev += Game.Rand.NextDouble();
                if (!Game.Rand.Bool(Game.Consts.MapDistMult(dev, mapSize)))
                    dev = Math.Sqrt(dev);
            }
            while (tile == null || tile.Piece != null || (Valid != null && !Valid(tile))
                || (checkBounds && !Game.Rand.Bool(Game.Consts.MapDistMult(tile, mapSize))));
            int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(dev));
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

        internal double GetMapSize() => Math.Max(Game.Consts.CaveDistance,
            Math.Sqrt(_explored.Select(p => new PointD(p.X, p.Y)) //PointD add
                .Concat(_pieces.Values.OfType<Hive>().Select(h => h.Tile.LocationD))
                //.Append(add)
                .Max(p => (double?)GetDistSqr(new(p.X, p.Y), new(0, 0))) ?? 0));

        internal void Explore(Point point, double vision)
        {
            foreach (Path p in Game.Rand.Iterate(_paths))
                p.Explore(this, point, vision);
            foreach (Cave c in Game.Rand.Iterate(_caves))
                c.Explore(point, vision);
            //GeneratePlateaus(point, vision);
        }
        internal void GenResources(Func<Tile> GetTile, double foundationMult, int numResources = 1)
        {
            for (int a = 0; a < numResources; a++)
            {
                Tile tile = GetTile();

                bool island = tile.Terrain is Island;
                double islandMult = tile.Terrain is Island i ? Math.Sqrt(i.Height / Game.Consts.ElevationHeight) : 1;

                double fMult = 0;
                if (Game.TEST_MAP_GEN.HasValue || !tile.Visible)
                {
                    fMult = 1 + _caves.Select(c => c.Center).Concat(AllFoundations)
                        .Concat(_clearTerrain.Append(Game.Player.Core.Tile.Location).Select(p => new PointD(p.X, p.Y)))
                        .Min(p => Tile.GetDistanceD(tile.X, tile.Y, p.X, p.Y));
                    fMult /= (fMult + (Game.Consts.CaveSize + Game.Consts.PathWidth) / 2.1);
                    fMult *= fMult * fMult * foundationMult;
                    const double baseMult = .26;
                    if (island)
                        fMult = Math.Sqrt(fMult) * (baseMult + islandMult * 2.1);
                    else
                        fMult *= baseMult;
                    fMult *= 2.6;
                }

                // TODO: Consts
                if (_resourcePool.Values.Any(v => v <= 0))
                {
                    _resourcePool[ResourceType.Artifact] += 2;
                    _resourcePool[ResourceType.Foundation] += 4;
                    _resourcePool[ResourceType.Biomass] += 5;
                    _resourcePool[ResourceType.Metal] += 6;
                }

                islandMult = .78 + (island ? islandMult : 0);
                double Mult(ResourceType r) => r switch
                {
                    ResourceType.Artifact => Math.Sqrt(islandMult),
                    ResourceType.Foundation => fMult,
                    ResourceType.Metal => islandMult,
                    _ => 1
                };
                ResourceType type = Game.Rand.SelectValue(_resourcePool.Keys, r =>
                    Game.Rand.Round(_resourcePool[r] * Mult(r)));

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
                        int size = Game.Rand.GaussianOEInt(Game.Consts.FoundationAmt, 0, 1, 1);
                        while (true)
                        {
                            count++;
                            Foundation.NewFoundation(tile);
                            if (count >= size)
                                break;

                            Dictionary<Tile, int> neighbors = [];
                            while (neighbors.Count == 0 && Game.Rand.Next(169) > 0)
                                neighbors = tile.GetPointsInRange(1 + Game.Rand.OE())
                                    .Select(this.GetTile)
                                    .Where(t => t != null && t.Piece == null && (Game.TEST_MAP_GEN.HasValue || !t.Visible))
                                    .ToDictionary(t => t, CountAdjacent);
                            if (neighbors.Count == 0)
                                break;
                            tile = Game.Rand.SelectValue(neighbors);
                        }

                        //- 1 to account for the first one already removed from the pool
                        _resourcePool[type] -= Game.Rand.Round(Math.Sqrt(count / Game.Consts.FoundationAmt)) - 1;
                        break;
                }
            }
            static int CountAdjacent(Tile tile)
            {
                static double Weight(Tile t) => t.Piece is Foundation ? 1 : t.Terrain is Island i ? .5 + .5 * i.Height / t.Map.Game.Consts.ElevationHeight : 0;
                double count = tile.GetAdjacentTiles().Sum(Weight) + Weight(tile) * 2;
                return Game.Rand.Round(1 + (1 + count) * count);
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
                    new Tuple<IEnemySpawn, PointD>(p.Spawn, p.Tile.LocationD)).Where(t => t.Item1 != null));


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
        internal List<Point> PathFindCore(Tile from, double move, Func<HashSet<Point>, bool> Accept)
        {
            if (corePaths.TryGetValue(from.Location, out FoundPath found) && found.Movement <= move)
                return [.. found.CompletePath(from.Location)];

            HashSet<Point> known = [.. corePaths.Keys.Where(k => corePaths[k].Movement <= move)];

            Tile to = Game.Enemy.PiecesOfType<Portal>().Where(p => !p.Exit).Select(p => p.Tile)
                .Append(Game.Player.Core.Tile).OrderBy(t => from.MoveDistTo(t)).First();
            var path = PathFind(from, to, move, move, true, false, p2 =>
                {
                    //the map is infinite, so to avoid pathfinding forever we impose a penalty on blocked terrain instead of blocking tiles entirely
                    double penalty = 0;
                    Tile tile = GetTile(p2);
                    if (tile == null)
                    {
                        penalty = Game.Rand.GaussianCapped((Game.Consts.PathWidth + move) * 2.25 * Game.Consts.PathWidth, .065);
                    }
                    else if (tile.Piece is Block block)
                    {
                        double mult = .5 + block.Value; //ranges from 0.5 - 1.5
                        mult *= mult; //ranges from 0.25 - 2.25
                        penalty = Game.Rand.GaussianCapped((Game.Consts.PathWidth + move) * mult, .065);
                    }
                    if (penalty > 0 && !Game.GameOver && !Game.TEST_MAP_GEN.HasValue && Visible(p2))
                        penalty *= Game.Consts.PathWidth;
                    return penalty;
                }, known.Contains, out var blocked);

            if (Accept(blocked))
            {
                //clear any blocked terrain we pathed through 
                ClearTerrain(blocked.SelectMany(p =>
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
                FoundPath foundPath = new(path, target, move);
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

        private void ClearTerrain(IEnumerable<Point> clear)
        {
            foreach (var point in clear)
                if (_clearTerrain.Add(point))
                {
                    Piece piece = GetPiece(point);
                    if (piece != null)
                    {
                        RemovePiece(piece);
                        Tile tile = GetTile(point);
                        piece.SetTile(tile);
                    }
                }
        }

        //internal Tile GetRetreatTo(Tile tile)
        //{
        //    throw new NotImplementedException();
        //}

        private IEnumerable<Tile> FindRetreatTiles(Tile tile, Func<Tile, bool> ValidRetreat)
        {
            Dictionary<PointD, double> dists = [];
            return _paths.Select(p => p.ExploredPoint(Game.Consts.PathWidth))
                .Concat(_caves.Where(c => !c.Explored).Select(c => c.Center))
                .OrderBy(p =>
                {
                    if (!dists.TryAdd(p, Math.Sqrt(GetDistSqr(tile.X, tile.Y, p)) + Game.Rand.OE(Game.Consts.CavePathWidth)))
                        ; //if never hit can remove dists dict
                    return dists[p];
                }).Select(point => SpawnTile(point, Game.Consts.PathWidth + Game.Consts.CaveSize, false, Valid: ValidRetreat));
        }
        internal List<Point> PathFindRetreat(Tile from, IEnumerable<Tile> targets, double move, double defense, Dictionary<Tile, double> playerAttacks, Func<Tile, bool> ValidRetreat)
        {
            var options = FindRetreatTiles(from, ValidRetreat);
            if (targets != null)
                options = options.Concat(targets);
            options = [.. options.OrderBy(t => from.GetDistance(t))];
            foreach (Tile tile in options)
            {
                // Game.Rand.Bool();
                var path = PathFind(from, tile, move, move, false, false, p =>
                {
                    double att = 0;
                    Tile key = GetTile(p);
                    if (key != null)
                        playerAttacks.TryGetValue(key, out att);
                    if (att == 0)
                        return 0;
                    return Math.Sqrt((att + 1) / (defense + 1)) * Game.Consts.PathWidth;
                },
                p => ValidRetreat(GetTile(p)),
                out var blocked);
                if (blocked.Count == 0)
                    return path;
            }
            return null;
        }

        //double? minFirstMove
        private List<Point> PathFind(Tile fromTile, Tile toTile, double firstMove, double move, bool includeBlocked, bool visibleOnly,
            Func<Point, double> Penalty, Func<Point, bool> Stop, out HashSet<Point> blocked)
        {
            blocked = [];

            Point from = fromTile.Location;
            Point to = toTile.Location;
            if (from == to)
                return [from, to,];

            //cache tile penalties at each point so they are consistent 
            Dictionary<Point, double> cache = [];

            //double moveMin = firstMove;
            //if (limitMove)
            //    firstMove += Math.Sqrt(2);
            //else
            //    moveMin = 0;
            if (move < 1)
                move = 1;

            bool first = firstMove >= 1 && firstMove != move;// && minFirstMove + 1 < movement;
            var path = TBSUtil.PathFind(Game.Rand, from, to, Stop, p1 =>
                {
                    IEnumerable<Point> points = Tile.GetAllMovement(this, p1, first ? firstMove : move, visibleOnly);
                    if (first && !points.Any())
                    {
                        points = Tile.GetAllMovement(this, p1, move, visibleOnly);
                        first = false;
                    }
                    var result = points.Where(p =>
                    {
                        if (first)
                        {
                            Tile t1 = GetTile(p1, visibleOnly);
                            Tile t2 = GetTile(p, visibleOnly);
                            if (t1 != null && t2 != null && !t1.MoveDistTo(t2, firstMove))
                                return false;
                        }
                        var tile = GetTile(p, visibleOnly);
                        if (visibleOnly && !Visible(p))
                            return false;
                        var piece = tile?.Piece;
                        if (tile == null || piece is ITerrain)
                            return includeBlocked;
                        return p == from || p == to || piece == null || piece.HasBehavior<IMovable>();
                    }).Select(p2 =>
                    {
                        if (!cache.TryGetValue(p2, out double penalty))
                        {
                            penalty = Penalty(p2);
                            cache.Add(p2, penalty);
                        }
                        Tile t1 = GetTile(p1, visibleOnly);
                        Tile t2 = GetTile(p2, visibleOnly);
                        double dist;
                        if (t1 != null && t2 != null)
                            dist = t1.MoveDistTo(t2);
                        else
                            dist = Tile.GetDistance(p1, p2);
                        return Tuple.Create(p2, dist + penalty);
                    }).ToList();
                    first = false;
                    return result;
                }, Tile.GetDistance);

            if (path != null)
                foreach (var p in path)
                {
                    Tile tile = GetTile(p, visibleOnly);
                    if (tile == null || tile.Piece is ITerrain)
                        blocked.Add(p);
                }

            //foreach (var p in blocked)
            //    if (!Game.TEST_MAP_GEN.HasValue && Visible(p))
            //        Debug.WriteLine($"Path through visible tile {p}");

            return path;
        }

        public List<Point> PathFind(Tile from, Tile to, double firstMove, double move, Func<Point, bool> Stop)
        {
            //double[] moves = new[] {
            //    Math.Min(movable.MoveCur - 1, movable.MoveCur + movable.MoveInc - movable.MoveMax) + 1,
            //    movable.MoveCur,
            //    movable.MoveMax,
            //    //(movable.MoveInc + movable.MoveMax) / 2.0,
            //    //movable.MoveInc,
            //    (movable.MoveMax + movable.MoveLimit) / 2.0,
            //    movable.MoveLimit, };

            return PathFind(from, to, firstMove, move, false, true,
                _ => 0,
                p => !_gameBounds.Contains(p.X, p.Y) || Stop(p),
                out _);
        }
    }
}
