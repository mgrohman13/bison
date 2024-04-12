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

namespace ClassLibrary1
{
    [Serializable]
    public class Map // : IDeserializationCallback
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

            this.Game = game;
            this.clearTerrain = new();

            double max = Game.Rand.GaussianOE(130, .091, .13, 65);
            double min = Game.Rand.GaussianCapped(5.2, .078, 2.6);
            int steps = Game.Rand.GaussianOEInt(6.5, .13, .13, Game.Rand.RangeInt(2, 5));
            double weightScale = Game.Rand.Weighted(.78) + Game.Rand.OE(.13);
            this.noise = new Noise(Game.Rand, min, max, steps, .065, weightScale);

            int numPaths = Game.Rand.GaussianOEInt(Math.PI, .091, .039, 2);
            double separation = Consts.PathMinSeparation;
            separation = Game.Rand.GaussianCapped(separation, .104, Math.Max(0, 2 * separation - TWO_PI)) / (double)numPaths;
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

            this._paths = new Path[numPaths];
            for (int d = 0; d < numPaths; d++)
            {
                //double enemyPow = enemyPows[d];
                //double enemyMult = 1 + Game.Rand.OE(1 / (enemyPow > 1 ? enemyPow : 1 / enemyPow));
                //if (Game.Rand.Bool())
                //    enemyMult = 1 / enemyMult;
                this._paths[d] = new Path(angles[d]);//, enemyMult / enemyPow, enemyPow);
            }

            separation /= 2.6;
            double caveMult = Math.PI / numPaths;
            int numCaves = Game.Rand.GaussianOEInt(2 + (Math.PI - 2) * caveMult * caveMult, .091, .039, 2);
            this._caves = new();
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
                this._caves.Add(new(cave, connect));//, connectCave));
            }
            if (_caves.Count < 2) throw new Exception();

            this._pieces = new();
            this._explored = new();

            resourcePool = new() { { ResourceType.Foundation, 1 },
                { ResourceType.Biomass, 2 }, { ResourceType.Artifact, 3 }, { ResourceType.Metal, 5 }, };

            LogEvalTime();
        }

        internal void NewGame()
        {
            SpawnHives();
            GenerateStartResources();

            foreach (var cave in Game.Rand.Iterate(_caves))
                cave.PathFind(this, Game.Player.Core.Tile);
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
                else
                    ;
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
        private float Evaluate(int x, int y)//, out float lineDist)
        {
            Point p = new(x, y);
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

                    double dist = PointLineDistanceSigned(path, point, new Point(x, y)) * sign;
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

        public Tile GetVisibleTile(Point p)
        {
            return GetVisibleTile(p.X, p.Y);
        }
        public Tile GetVisibleTile(int x, int y)
        {
            return Visible(x, y) ? GetTile(x, y) : null;
        }
        public IEnumerable<Piece> GetVisiblePieces()
        {
            return _pieces.Values.Where(p => p.Tile.Visible);
        }

        internal Tile GetTile(Point p)
        {
            return GetTile(p.X, p.Y);
        }
        internal Tile GetTile(int x, int y)
        {
            double terrain = 1;
            bool chasm = false;
            if (!clearTerrain.Contains(new(x, y)))
            {
                terrain = Evaluate(x, y);//, out float lineDist);
                //also use dist from center?
                // (5 * terrain * Consts.PathWidth + lineDist) / 2.0 % Consts.PathWidth < 1;
                if (!chasm && terrain < 1 / 4.0)
                    return null;
                chasm |= terrain < 1 / 2.0;
            }
            else
                ;

            Piece piece = GetPiece(x, y);
            return piece == null ? NewTile(this, x, y, t => chasm ? new Block(t, terrain) : null) : piece.Tile;
        }
        private Piece GetPiece(int x, int y)
        {
            _pieces.TryGetValue(new Point(x, y), out Piece piece);
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

        public bool Visible(Point tile)
        {
            return Visible(tile.X, tile.Y);
        }
        public bool Visible(int x, int y)
        {
            bool visible = _explored.Contains(new(x, y));
            //if (!visible)
            //    visible = _pieces.ContainsKey(new Point(x, y));
            return Game.TEST_MAP_GEN.HasValue || Game.GameOver || visible;
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
        private void UpdateVision(PlayerPiece piece) => UpdateVision(piece.Tile, piece.Vision, piece is not Core);
        internal void UpdateVision(Tile tile, double range, bool explore = true)
        {
            LogEvalTime();

            foreach (Point p in tile.GetPointsInRangeUnblocked(range))
                this._explored.Add(p);

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
                float evalTime = 1000f * watch.ElapsedTicks / (float)Stopwatch.Frequency;
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
            bool invalid = ((visible && !hiveRange) || tile.Piece != null || coreRange);
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
        [Serializable]
        internal enum ResourceType
        {
            Artifact,
            Biomass,
            Metal,
            Foundation,
        }

        internal double GetMinSpawnMove(Tile tile)
        {
            Cave cave = _caves.OrderBy(c => GetDistSqr(new(tile.X, tile.Y), c.Center)).First();
            return cave.MinSpawnMove;
        }
        internal Tile GetEnemyTile()
        {
            IEnemySpawn spawn = Game.Rand.SelectValue(_paths.Concat<IEnemySpawn>(_caves).ToDictionary(k => k, v => v.SpawnChance(Game.Turn)));
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

        private void ForcePath(HashSet<Point> blocked)
        {
            if (blocked.Any())
                this.clearTerrain.UnionWith(blocked.SelectMany(p =>
                {
                    List<Point> list = new() { p };
                    int extra = Game.Rand.OEInt();
                    for (int a = 0; a < extra; a++)
                    {
                        Tile tile = Game.Map.GetTile(p.X + Game.Rand.GaussianInt(), p.Y + Game.Rand.GaussianInt());
                        if (tile != null && tile.Piece is Terrain)
                            list.Add(new(tile.X, tile.Y));
                    }
                    return list;
                }));
        }

        //internal List<Point> PathFindForce(Tile from, Tile to, double move)
        //{
        //    return PathFindForce(from, to, move, out _);
        //}
        //internal List<Point> PathFindForce(Tile from, Tile to, double move, out HashSet<Point> blocked)
        //{
        //    var path = PathFind(from, to, move, out blocked);
        //    ForcePath(blocked);
        //    return path;
        //}
        public List<List<Point>> savedPathing = new();
        public List<Point> PathFind(Tile from, Tile to, double move, out HashSet<Point> blocked)
        {
            blocked = new();
            Dictionary<Point, Tuple<Tile, double>> cache = new() {
                { new(from.X, from.Y), Tuple.Create(from, 0.0) },
                { new(to.X, to.Y), Tuple.Create(to, 0.0) },
            };

            var path = TBSUtil.PathFind<Tile>(Game.Rand, from, to, p1 =>
                    p1.GetAllPointsInRange(move).Select(p2 =>
                    {
                        double dist = p1.GetDistance(p2);
                        if (!cache.TryGetValue(p2, out Tuple<Tile, double> tuple))
                        {
                            Tile tile = GetTile(p2.X, p2.Y);

                            //the map is infinite, so to avoid pathfinding forever we impose a penalty on blocked terrain instead of blocking tiles entirely
                            double penalty = 0;
                            if (tile == null)
                                penalty = Game.Rand.Gaussian((Consts.PathWidth + move) * Consts.PathWidth, .039);
                            else if (tile.Piece is Block block)
                            {
                                double mult = .5 + 4 * (.5 - block.Value); //ranges from 0.5 - 1.5
                                penalty = Game.Rand.Gaussian((Consts.PathWidth + move) * mult * mult, .039);
                            }
                            if (penalty > 0 && !Game.TEST_MAP_GEN.HasValue && Visible(p2))
                                penalty *= Consts.PathWidth;

                            tile ??= NewTile(this, p2.X, p2.Y, t => null);
                            tuple = Tuple.Create(tile, penalty);
                            cache.Add(p2, tuple);
                        }
                        return Tuple.Create(tuple.Item1, dist + tuple.Item2);
                    }).Where(t =>
                    {
                        var tile = t.Item1;
                        var piece = tile?.Piece;
                        return tile == from || tile == to || piece is null || piece is Terrain || piece.HasBehavior<IMovable>();
                    }),
                    (p1, p2) => p1.GetDistance(p2))
                .Select(t => new Point(t.X, t.Y)).ToList();

            foreach (var p in path)
            {
                Tile tile = GetTile(p);
                if (tile == null || tile.Piece is Terrain)
                    blocked.Add(p);
            }

            savedPathing.Add(path);

            return path;
        }

        private static Func<Map, int, int, Func<Tile, Terrain>, Tile> NewTile;
        [Serializable]
        public class Tile
        {
            public readonly Map Map;
            public readonly int X, Y;
            private Terrain _terrain;
            public Terrain Terrain => _terrain;
            public Piece Piece => Map.GetPiece(X, Y) ?? Terrain;
            public bool Visible => Map.Visible(X, Y);

            static Tile()
            {
                NewTile = (map, x, y, GetTerrain) =>
                {
                    Tile tile = new(map, x, y);
                    tile._terrain = GetTerrain(tile);
                    return tile;
                };
            }
            private Tile(Map map, int x, int y)
            {
                this.Map = map;
                this.X = x;
                this.Y = y;
            }

            //support blocking
            public double GetDistance(Point other) => GetDistance(other.X, other.Y);
            public double GetDistance(Tile other) => GetDistance(other.X, other.Y);
            public double GetDistance(int x, int y) => GetDistance(this.X, this.Y, x, y);
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
            internal IEnumerable<Tile> GetTilesInRange(IMovable movable) => GetTilesInRange(movable.MoveCur, false, movable.Piece);
            internal IEnumerable<Tile> GetTilesInRange(IAttacker attacker) => GetTilesInRange(attacker.Attacks.Max(a => a.Range), true, attacker.Piece);
            internal IEnumerable<Tile> GetTilesInRange(double range, bool blockMap, Piece blockFor) => GetPointsInRange(range, blockMap, blockFor)
                .Select(Map.GetTile).Where(t => t != null);

            internal IEnumerable<Point> GetPointsInRangeUnblocked(double vision) => GetPointsInRange(vision, false, null);
            internal static IEnumerable<Point> GetPointsInRangeUnblocked(Map map, Point point, double range) => GetPointsInRange(map, point, range, false, null);

            public IEnumerable<Point> GetAllPointsInRange(double range) => GetPointsInRange(range, false, null);
            public IEnumerable<Point> GetPointsInRange(IMovable movable) => GetPointsInRange(movable, movable.MoveCur);
            public IEnumerable<Point> GetPointsInRange(IMovable movable, double move) => GetPointsInRange(move, false, movable.Piece);
            public IEnumerable<Point> GetPointsInRange(IBuilder builder) => GetPointsInRange(builder.Range, true, null);
            public IEnumerable<Point> GetPointsInRange(Attack attack) => GetPointsInRange(attack.Range, true, attack.Piece);
            private IEnumerable<Point> GetPointsInRange(double range, bool blockMap, Piece blockFor) => GetPointsInRange(Map, new Point(X, Y), range, blockMap, blockFor);
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

            public override string ToString()
            {
                return string.Format("({0}, {1})", X, Y);
            }
        }

        [Serializable]
        private class Path : IEnemySpawn
        {

            public readonly double Angle;
            public readonly PointD Left, Right;

            private readonly SpawnChance spawn = new();
            //public readonly double EnemyMult, EnemyPow;

            public int ResourceNum { get; private set; }
            public double ExploredDist { get; private set; }
            public double NextResourceDist { get; private set; }

            public Path(double angle)//, double enemyMult, double enemyPow)
            {
                this.Angle = angle;
                //this.EnemyMult = enemyMult;
                //this.EnemyPow = enemyPow;

                static double Dist() => Game.Rand.GaussianCapped(Consts.PathWidth, Consts.PathWidthDev, Consts.PathWidthMin);
                PointD GetOrigin(int sign)
                {
                    double dist = Dist();
                    double dir = angle + HALF_PI * sign;
                    return GetPoint(dir, dist);
                }
                this.Left = GetOrigin(1);
                this.Right = GetOrigin(-1);

                //Debug.WriteLine(Left);
                //Debug.WriteLine(Right);

                this.ResourceNum = 0;
                this.ExploredDist = 0;
                this.NextResourceDist = 0;
                GetNextDist();
            }
            private void GetNextDist()
            {
                double avg = ResourceNum * Consts.ResourceAvgDist + Consts.PathWidth;
                double inc = Math.Max(avg - NextResourceDist, 0) + Consts.ResourceAvgDist / 2.0;
                double oe = Math.Min(inc, Consts.ResourceAvgDist);
                inc -= oe;
                NextResourceDist += Game.Rand.DoubleFull(inc) + Game.Rand.OE(oe);
            }

            public HashSet<Tile> Explore(Tile tile, double vision)
            {
                return Explore(tile.Map, GetExploredDist(tile, vision));
            }
            public HashSet<Tile> Explore(Map map, double dist)
            {
                ExploredDist = Math.Max(ExploredDist, dist);

                HashSet<Tile> tiles = new();
                foreach (double distance in Game.Rand.Iterate(CreateResources()))
                    tiles.Add(map.SpawnTile(GetPoint(Angle, distance), Consts.PathWidth, false));
                return tiles;
            }
            private double GetExploredDist(Tile tile, double vision)
            {
                PointD closest = GetClosestPoint(tile.X, tile.Y);
                return Math.Sqrt(closest.X * closest.X + closest.Y * closest.Y) + vision;

            }
            public PointD GetClosestPoint(double x, double y)
            {
                CalcLine(new PointD(0, 0), out double a, out double b, out double c);

                double div = a * a + b * b;
                double lineX = (b * (+b * x - a * y) - a * c) / div;
                double lineY = (a * (-b * x + a * y) - b * c) / div;
                double path = GetAngle(lineX, lineY);
                //check can be againsts any arbitrarily small value - angle will either be equal or opposite
                if (GetAngleDiff(path, Angle) < HALF_PI)
                    return new(lineX, lineY);
                return new(0, 0);
            }
            private IEnumerable<double> CreateResources()
            {
                double generationBuffer = 2.1 * (Consts.PathWidth + Consts.ResourceAvgDist);
                if (Game.TEST_MAP_GEN.HasValue)
                    generationBuffer = Game.TEST_MAP_GEN.Value;

                List<double> create = new();
                while (ExploredDist + generationBuffer > NextResourceDist)
                {
                    create.Add(NextResourceDist);
                    ResourceNum++;
                    GetNextDist();
                }
                return create;
            }

            //calculates the line equation in the format ax + by + c = 0 
            public void CalcLine(PointD start, out double a, out double b, out double c)
            {
                a = Math.Tan(Angle);
                b = -1;
                c = start.Y - (a * start.X);
            }

            public void Turn(int turn)
            {
                spawn.Turn(turn);
            }
            public int SpawnChance(int turn, double? enemyMove = null)
            {
                return spawn.Chance * 3;
            }
            public Tile SpawnTile(Map map, bool isEnemy, double deviationMult = 1)
                => map.SpawnTile(GetPoint(Angle, ExploredDist), Consts.PathWidth * deviationMult, isEnemy);
        }
        [Serializable]
        private class Cave : IEnemySpawn
        {
            public readonly PointD Center;
            private readonly double[] shape;

            private readonly PointD seg1, seg2;
            private readonly double segSize;

            private readonly SpawnChance spawn = new();
            private readonly List<Hive> hives = new();

            private bool explored;//, spawned;

            private double minSpawnMove = double.NaN;
            private List<Point> pathToCore = new();

            //PathCenter should handle edge distances better (e.g. segment on other side of cave/path)
            public PointD PathCenter => new((seg1.X + seg2.X) / 2.0, (seg1.Y + seg2.Y) / 2.0);
            public double PathLength => Math.Sqrt(GetDistSqr(seg1, seg2));

            public double MinSpawnMove => minSpawnMove;
            public Point PathFindStart => pathToCore.First();
            public IReadOnlyList<Point> PathToCore => pathToCore.AsReadOnly();

            public Cave(PointD center, PointD connectTo, bool connectCave = false)
            {
                double off2 = connectCave ? Consts.CaveSize : 1.3 * Consts.PathWidth;
                static double Offset(double amt) => Game.Rand.Gaussian(amt / 2.1);

                this.Center = center;
                this.seg1 = new(center.X + Offset(Consts.CaveSize), center.Y + Offset(Consts.CaveSize));
                this.seg2 = new(connectTo.X + Offset(off2), connectTo.Y + Offset(off2));
                this.segSize = Game.Rand.GaussianOE(Consts.CavePathSize, .13, .13, 1.3);

                this.shape = new[] { Game.Rand.GaussianOE(1.69, .39, .13), 1 + Game.Rand.GaussianOEInt(1.3, .26, .26), Game.Rand.NextDouble() * TWO_PI,
                    Game.Rand.GaussianCapped(1.3, .26), Game.Rand.GaussianOE(6.5, .39, .13), Game.Rand.GaussianCapped(1, .13, .5) };
                if (shape[1] != (int)shape[1])
                    throw new Exception();
            }

            public void Explore(Tile tile, double vision)
            {
                explored |= GetDistSqr(new(tile.X, tile.Y), Center) < vision * vision;
            }

            public double GetMult(int x, int y)
            {
                double offset = 1.3 + shape[0];
                double s = offset + Math.Sin((GetAngle(Center.X - x, Center.Y - y) + Math.PI) * shape[1] + shape[2]);
                s *= s;
                double distance = GetDistSqr(x, y, Center) / s;
                double centerMult = GetMult(distance, shape[5] * Consts.CaveSize / offset, shape[3]);

                double connection = GetMult(PointLineDistSqr(seg1, seg2, new(x, y)), segSize, shape[4]);

                return centerMult + connection;

                static double GetMult(double distSqr, double size, double o) =>
                    (Math.Pow(size, Consts.CaveDistPow) + o) / (Math.Pow(distSqr, Consts.CaveDistPow / 2.0) + o);
            }
            private static double PointLineDistSqr(PointD v, PointD w, PointD p)
            {
                var l2 = GetDistSqr(v, w);
                if (l2 == 0) return GetDistSqr(p, v);
                var t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
                t = Math.Max(0, Math.Min(1, t));
                return GetDistSqr(p, new(v.X + t * (w.X - v.X), v.Y + t * (w.Y - v.Y)));
            }

            public void AddHive(Hive hive)
            {
                hives.Add(hive);
            }
            public void PathFind(Map map, Tile to)
            {
                this.minSpawnMove = 1;

                while (true)
                {
                    Tile from = map.SpawnTile(Center, Math.Sqrt(2), true);
                    this.pathToCore = map.PathFind(from, to, minSpawnMove, out HashSet<Point> blocked);

                    double penalty = 1;
                    foreach (var p in blocked)
                    {
                        Tile tile = map.GetTile(p);
                        double div = 1;
                        if (tile == null)
                            div = 1 + Consts.CaveSize * Consts.CaveSize;
                        else if (tile.Piece is Terrain)
                            div = 1 + Consts.CaveSize / minSpawnMove;
                        if (div > 1)
                        {
                            penalty += div;
                        }
                    }

                    if (Game.Rand.Bool(1 / penalty))
                    {
                        map.ForcePath(blocked);
                        break;
                    }

                    this.minSpawnMove += Math.Sqrt(1 + this.minSpawnMove) - 1 + Game.Rand.OE();
                }
            }

            public void Turn(int turn)
            {
                spawn.Turn(turn);
            }
            public int SpawnChance(int turn, double? enemyMove = null)
            {
                if (enemyMove.HasValue && enemyMove.Value > this.minSpawnMove)
                    return 0;
                if (hives.Any(h => !h.Dead) || !explored)// && spawned)
                    return Game.Rand.Round(spawn.Chance * Math.Sqrt(2.1 + hives.Count));
                return 0;
            }
            public Tile SpawnTile(Map map, bool isEnemy, double deviationMult = 1)
            {
                //this.spawned = true;

                bool inPath = !isEnemy && Game.Rand.Bool(.169);
                PointD spawnCenter = inPath ? this.PathCenter : this.Center;
                double deviation = deviationMult * (inPath ? this.PathLength / 6.5 : Consts.CaveSize);
                Tile tile = map.SpawnTile(spawnCenter, deviation, isEnemy);
                if (!isEnemy)
                    Debug.WriteLine($"Cave resource ({inPath}): {tile} ({Math.Sqrt(GetDistSqr(spawnCenter, new(tile.X, tile.Y)))})");
                return tile;
            }
        }
        private interface IEnemySpawn
        {
            public void Turn(int turn);
            int SpawnChance(int turn, double? enemyMove = null);
            Tile SpawnTile(Map map, bool isEnemy, double deviationMult = 1);
        }
        [Serializable]
        private class SpawnChance
        {
            private int chance, target;
            private double rate;

            public int Chance => chance;

            public SpawnChance()
            {
                this.chance = GenValue();
                this.rate = Game.Rand.Gaussian();
                SetTarget(-10);
            }

            public void Turn(int turn)
            {
                double avg = chance + rate;
                if (avg > 1)
                {
                    int sign = Math.Sign(target - chance);
                    this.chance = Game.Rand.GaussianCappedInt(avg, rate / avg / 2.6, 1);
                    if (sign != Math.Sign(target - chance) || Game.Rand.Next(52) == 0)
                        SetTarget(turn);
                    else
                        SetRate(turn, target, 65);
                }
                else
                {
                    this.chance = 1 + Game.Rand.OEInt(Math.Abs(avg));
                    SetTarget(turn);
                }
            }
            private void SetTarget(int turn)
            {
                this.target = GenValue();
                SetRate(turn, target, 1);

                int sign = Math.Sign(target - chance);
                if (sign != 0)
                    this.rate = sign * (1 + Math.Abs(rate));

                SetRate(turn, GenValue(), 4);
            }

            private void SetRate(int turn, int value, double prevWeight)
            {
                double diff = value - chance;
                double time = Game.Rand.GaussianOEInt((turn + 3900) / (double)(turn + 169), .13, .13, 1);
                double newRate = diff / time;

                int factor = GenValue() + GenValue();
                newRate *= factor / (factor + Math.Abs(diff));

                double newWeight = 1 + Game.Rand.OE();
                newRate = (newRate * newWeight + rate * prevWeight) / (newWeight + prevWeight);

                this.rate = newRate + Game.Rand.Gaussian();
            }

            private static int GenValue() => Game.Rand.GaussianOEInt(650, .39, .13, 1);
        }
    }
}
