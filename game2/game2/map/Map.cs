using game2.game;
using game2.pieces;
using game2.pieces.player;
using MattUtil;
using System.Drawing;
using Point = MattUtil.Point;

namespace game2.map
{
    [Serializable]
    public class Map
    {
        public readonly Game Game;

        private readonly Noise WaterNoise;
        private readonly Noise ForestNoise;
        private readonly Noise MountainNoise;
        private readonly HashSet<Point> ForceLand;

        private readonly Dictionary<Point, Piece> _pieces;

        private readonly HashSet<Point> _explored, _notTall;

        private int _resourceCounter;
        private int[] _resourceChances;

        public IEnumerable<Tile> VisibleTiles => _explored.Select(GetTile);
        public IEnumerable<Point> NotTallTiles => [.. _notTall];

        internal IEnumerable<Piece> Pieces => _pieces.Values;

        internal Map(Game game)
        {
            Game = game;

            ForceLand = [];
            _pieces = [];
            _explored = [];
            _notTall = [];

            //Noise = new(Game.Rand, 3, 3, 1, 0, 1);

            WaterNoise = CreateNoise(Game.Consts.WaterNoiseMult);
            ForestNoise = CreateNoise(Game.Consts.ForestNoiseMult);
            MountainNoise = CreateNoise(Game.Consts.MountainNoiseMult);

            _resourceCounter = Game.Rand.OEInt();
            _resourceChances = new int[Resources.NumMapResources];
        }
        private static Noise CreateNoise(float mult)
        {
            double dev = Game.Rand.GaussianCapped(Consts.GetDeviation() * 2.0, Consts.GetDeviation());
            double oe = Game.Rand.GaussianCapped(Consts.GetDeviation() * 1.25, Consts.GetDeviation() * 1.5);
            double minMin = Game.Rand.Range(1.0, 3.0);
            double min = Game.Rand.GaussianOE(minMin + Game.Rand.Range(2.0, 4.0) * mult, dev, oe / 1.5, minMin);
            double maxMin = min + Game.Rand.Range(2.0 + Game.Rand.OE(), 10.0);
            double max = Game.Rand.GaussianOE(maxMin + Game.Rand.Range(15.0, 20.0) * mult, dev, oe * 1.5, maxMin);
            int steps = Game.Rand.GaussianOEInt(2.0 + Game.Rand.Range(2.0, 4.0) * mult, dev * 1.25, oe, 2);
            double weightScale = Game.Rand.Weighted(Game.Rand.Range(0.5, 1.0)) + Game.Rand.OE(Game.Rand.Range(0.0, 0.5));
            double stepDev = Game.Rand.GaussianOE((dev + Consts.GetDeviation()) * .5, dev * 1.5, oe * 1.5);
            return new(Game.Rand, min, max, steps, stepDev, weightScale);
        }
        internal void StartGame()
        {
            Tile start = Game.Player.Core.Tile;
            UpdateVision(Game.Rand.SelectValue(start.GetNeighbors()), 4);

            Tile food;
            if (GetVisiblePieces().OfType<Resource>().Any())
                food = GetRandomTile(start.Point, t => t.Piece == null);
            else
                food = GetRandomVisibleTile(t => t.Piece == null);
            Resource.NewResource(food, 0);

            foreach (var n in Game.Rand.Iterate(start.GetNeighbors()))
            {
                if (n.Piece is Resource resource)
                {
                    Game.RemovePiece(resource);

                    int idx = Game.Rand.Iterate(Enumerable.Range(0, Resources.NumResources)
                           .Select(a => (a, resource.Generate[a]))).OrderByDescending(p => p.Item2).First().Item1;

                    Tile move = GetRandomVisibleTile(t => t.Piece == null && !start.GetNeighbors().Contains(t));
                    Resource.NewResource(move, idx);
                }
            }

            //ensure path from core to first food
            //pick a target enemy spot for another path to avoid island start?
            //if need to redo vision to create ForceLand path, decrement resource counter to avoid extra resources
        }

        internal void AddPiece(Piece piece)
        {
            _pieces.Add(piece.Tile.GetMapIndex(), piece);

            if (piece is PlayerPiece playerPiece)
            {
                UpdateVision(playerPiece);
                //Treasure.Collect(piece.Tile);
            }
        }
        internal void RemovePiece(Piece piece) => _pieces.Remove(piece.Tile.GetMapIndex());

        private void UpdateVision(PlayerPiece playerPiece) => UpdateVision(playerPiece.Tile, playerPiece.Vision);
        private void UpdateVision(Tile tile, int vision)
        {
            Dictionary<(Tile, bool, bool), int> closed = [];
            Dictionary<(Tile, bool, bool), int> queue = [];
            foreach (Tile neighbor in Game.Rand.Iterate(tile.GetNeighbors()))
                queue.Add((neighbor, false, false), vision);

            while (queue.Count > 0)
            {
                (Tile next, bool seenFlat, bool seenTall) = Game.Rand.SelectValue(queue.Keys);
                int curVision = queue[(next, seenFlat, seenTall)];
                queue.Remove((next, seenFlat, seenTall));
                closed[(next, seenFlat, seenTall)] = curVision;

                bool canSeeTall = !seenTall && curVision == 0;
                switch (next.Terrain.VisionType())
                {
                    case VisionType.Tall:
                        seenTall = true;
                        break;
                    case VisionType.Flat:
                        if (!seenFlat && (curVision > 1 || curVision == vision))
                            curVision++;
                        seenFlat = true;
                        goto case VisionType.Normal;
                    case VisionType.Normal:
                        if (seenTall)
                        {
                            curVision--;
                            canSeeTall |= curVision == 0;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                if (curVision > 0)
                {
                    curVision -= next.Terrain.VisionCost();
                    foreach (Tile neighbor in Game.Rand.Iterate(next.GetNeighbors()))
                        if (queue.TryGetValue((neighbor, seenFlat, seenTall), out int existingVision))
                            queue[(neighbor, seenFlat, seenTall)] = Math.Max(curVision, existingVision);
                        else if (!closed.TryGetValue((neighbor, seenFlat, seenTall), out int exploredVision)
                                || exploredVision < curVision)
                            queue.Add((neighbor, seenFlat, seenTall), curVision);

                    Explore(next);
                }
                else if (canSeeTall)
                {
                    if (next.Terrain.VisionType() == VisionType.Tall)
                        Explore(next);
                    else if (!_explored.Contains(next.GetMapIndex()))
                        _notTall.Add(next.GetMapIndex());
                }
            }
        }
        private bool Explore(Tile tile)
        {
            _notTall.Remove(tile.GetMapIndex());
            if (!_explored.Contains(tile.GetMapIndex()))
            {
                _resourceCounter--;
                if (_resourceCounter < 0 && Game.Rand.Bool())
                {
                    var dict = tile.GetNeighbors().Concat([tile]).Where(CanPlaceResource).ToDictionary(k => k, v =>
                        Game.Rand.Round(GetResourceChances(v.Terrain).Values.Sum() / (1f + v.GetNeighbors().Count(n => n.Piece is Resource))));
                    if (dict.Values.Any(v => v > 0))
                    {
                        Tile resource = Game.Rand.SelectValue(dict);

                        int primary = PickResources(resource.Terrain);//      (int primary, int secondary)
                        Resource.NewResource(resource, primary);//, secondary);

                        _resourceCounter += Game.Rand.OEInt(Game.Consts.TilesPerResource);
                    }
                }

                return _explored.Add(tile.GetMapIndex());
            }
            return false;
        }
        private static bool CanPlaceResource(Tile t) => t.Piece == null && !t.Visible
            && t.GetNeighbors().Any(n => n.Terrain != Terrain.Glacier && n.Piece == null);
        private int PickResources(Terrain terrain) //(int, int)
        {
            int Select()//float reduce)
            {
                Dictionary<int, int> dict = GetResourceChances(terrain);
                int pick = Game.Rand.SelectValue(dict);

                int[] updated = [.. _resourceChances];
                updated[pick] -= Game.Rand.Round(Game.Consts.ResourceTypeConsistency);//*reduce);
                _resourceChances = updated;

                return pick;
            }

            int primary = Select();// Game.Consts.ResourcePrimarySecondaryRatio);
            //int secondary = Select(1);

            return primary;//,  secondary);
        }
        private Dictionary<int, int> GetResourceChances(Terrain terrain)
        {
            while (_resourceChances.Any(v => v <= 0))
            {
                int[] refill = [.. _resourceChances];
                for (int a = 0; a < Resources.NumMapResources; a++)
                    refill[a] += Game.Rand.Round(Game.Consts.ResourceFreq[a]);
                _resourceChances = refill;
            }

            float[] mult = Game.Consts.TerrainResourceMults(terrain);
            return Enumerable.Range(0, Resources.NumMapResources).ToDictionary(k => k, v => Game.Rand.Round(_resourceChances[v] * mult[v]));
        }

        public Tile GetRandomVisibleTile(Func<Tile, bool> Predicate) => Game.Rand.SelectValue(VisibleTiles.Where(Predicate));
        public Tile GetRandomTile(PointF center, Func<Tile, bool> Predicate)
        {
            float deviation = 0;
            while (true)
            {
                deviation += Game.Rand.OEFloat();
                float x = center.X + Game.Rand.Gaussian(deviation);
                float y = center.Y + Game.Rand.Gaussian(deviation);
                int mapY = Game.Rand.Round(y / Tile.YRatio);
                int mapX = Game.Rand.Round(x - .5f * (mapY & 1));
                Tile tile = GetTile(mapX, mapY);
                if (Predicate(tile))
                    return tile;
            }
        }

        public Tile? GetVisibleTile(int x, int y) => GetVisibleTile(new(x, y));
        public Tile? GetVisibleTile(Point p)
        {
            Tile t = GetTile(p);
            return _explored.Contains(t.GetMapIndex()) ? t : null;
        }

        internal Tile GetTile(int x, int y) => GetTile(new(x, y));
        internal Tile GetTile(Point p)
        {
            Tile result = new(this, p, GetTerrain);
            if (_pieces.TryGetValue(result.GetMapIndex(), out Piece? piece))
                result = piece.Tile;
            return result;
        }

        private Terrain GetTerrain(Tile tile)
        {
            PointF p = tile.Point;

            double waterEval = WaterNoise.Evaluate(p.X, p.Y);
            double forestEval = ForestNoise.Evaluate(p.X, p.Y);
            double mountainEval = MountainNoise.Evaluate(p.X, p.Y);

            double depth = waterEval - .5;

            double mountains = Math.Abs(mountainEval - .5);
            bool hills = mountains < Game.Consts.HillsWidth;
            if (hills)
                depth -= mountains * Game.Consts.HillsDepthMod;

            bool forceLand = ForceLand.Contains(tile.GetMapIndex());
            bool land = forceLand;
            if (!land)
                land = depth < Game.Consts.LandWidth;

            Terrain terrain = land ? Terrain.Plains : Terrain.Sea;

            if (depth < -Game.Consts.LandWidth && !forceLand)
            {
                double waterDepth = depth + (.5 - forestEval) * Game.Consts.ForestGlacierSeaMod;
                if (waterDepth < -Game.Consts.LandWidth)
                    terrain = Terrain.Glacier;
                else
                    terrain = Terrain.Sea;
            }
            else if (land)
            {
                if (hills)
                {
                    terrain = Terrain.Hills;
                    double mountainDepth = depth + (forestEval - .5) * Game.Consts.ForestMountainMod;
                    if (mountainDepth < -Game.Consts.Mountains)
                        terrain = Terrain.Mountains;
                }
                else
                {
                    if (forestEval < Game.Consts.Forests)
                        terrain = Terrain.Forest;
                    else if (forestEval > Game.Consts.Sand)
                        terrain = Terrain.Sand;
                }
            }
            else
            {
                double glacierDepth = depth - (mountainEval - .5) * Game.Consts.MountainSeaGlacierMod;
                double kelp = forestEval + (waterEval - .5) * Game.Consts.DepthKelpMod;
                if (glacierDepth > Game.Consts.DepthGlacier)
                    terrain = Terrain.Glacier;
                else if (kelp < Game.Consts.Kelp)
                    terrain = Terrain.Kelp;
            }


            return terrain;
        }

        internal Piece? GetPiece(Tile tile) => _pieces.TryGetValue(tile.GetMapIndex(), out Piece? piece) ? piece : null;

        public bool Visible(Tile tile) => _explored.Contains(tile.GetMapIndex());
        public IEnumerable<Piece> GetVisiblePieces() => _pieces.Values.Where(p => p.Tile.Visible);
    }
}
