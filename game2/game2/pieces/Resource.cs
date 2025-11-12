using game2.game;
using game2.map;

namespace game2.pieces
{
    public class Resource : Piece
    {
        private Resources _total;
        private Resources _generate;

        private readonly float _rate;
        private bool _mined = false;

        public Resources Generate => _generate;

        private Resource(Tile tile, int primary) //, int secondary)
            : base(null, tile)
        {
            int[] resources = new int[Resources.NumResources];

            //float mult = tile.Map.Game.Consts.ResourcePrimarySecondaryRatio;
            //resources[primary] += Rand(primary, mult / (mult + 1f));
            //resources[secondary] += Rand(secondary, 1f / (mult + 1f));
            resources[primary] = Rand(primary, 1f);

            int Rand(int r, float m) => Game.Rand.GaussianOEInt(
                m * tile.Map.Game.Consts.ResourceTotal[r] / tile.Map.Game.Consts.ResourceValue[r],
                tile.Map.Game.Consts.ResourceDev, tile.Map.Game.Consts.ResourceOE, 1);

            _total = new Resources(resources);
            _generate = _total;
            _rate = 1f / Game.Rand.GaussianOE(tile.Map.Game.Consts.ResourceRateDiv[primary],
                Game.Consts.ResourceRateDev, Game.Consts.ResourceRateOE, 1f);

            SetGenerate();
        }
        private void SetGenerate()
        {
            for (int a = 0; a < Resources.NumResources; a++)
                _generate[a] = Math.Min(Math.Min(_generate[a], _total[a]), Math.Max(1,
                    Game.Rand.Round((_total[a] + Game.Rand.OEFloat()) * _rate))); 
        }

        internal static Resource NewResource(Tile tile, int primary)//, int secondary)
        {
            Resource resource = new(tile, primary);//, secondary);
            tile.Map.Game.AddPiece(resource);
            return resource;
        }

        internal Resources GetGenerate()//int bonus)
        {
            if (_mined)
                return new Resources();

            Resources r = new(_generate);
            //for (int a = 0; a < Resources.NumResources; a++)
            //    if (r[a] > 0)
            //    {
            //        r[a] += bonus;
            //        if (r[a] > _total[a])
            //            r[a] = _total[a];
            //    }
            return r;
        }
        internal void DoGenerate(ref Resources resources)//, int bonus)
        {
            Resources r = GetGenerate();// bonus);
            for (int a = 0; a < Resources.NumResources; a++)
                _total[a] -= r[a];
            SetGenerate();

            if (_total.Sum() <= 0)
                Die();
            _mined = true;

            resources += r;
        }

        internal override void StartTurn() => _mined = false;
    }
}
