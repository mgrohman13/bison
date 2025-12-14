using game2.game;
using game2.map;
using game2.pieces.behavior;
using game2.pieces.behavior.attributes;
using MattUtil;

namespace game2.pieces.player
{
    public class Core : PlayerPiece
    {
        private Resources _income;
        private float _curIncMult = 1f, _rounding = Game.Rand.NextFloat();

        private Core(Tile tile) : base(tile, game.Game.Rand.GaussianOEInt(65f, .26f, .26f, 2)) //2)//
        {
            _income = new(Tile.Map.Game.Consts.CoreIncome);

            SetBehavior(
                new Movable(this, Movable.MoveType.Ground, 3, 1, 3),
                new Combatant(this, 1, 10, 20),
                new Harvester(this, 0));

            ReduceIncome();
        }
        internal static Core NewCore(Tile tile)
        {
            Core core = new(tile);
            tile.Map.Game.AddPiece(core);
            return core;
        }

        //needs to reduce over time
        public override Resources GetTurnEnd() =>
            Income() + base.GetTurnEnd();
        private void ReduceIncome()
        {
            float[] exponent = [0.6f, 1.0f, 1.8f, 0.8f]; //TODO: randomize
            _curIncMult *= Tile.Map.Game.Consts.CoreIncReduction;

            if (_income != GetInc())
            {
                _rounding = Game.Rand.NextFloat();
                _income = GetInc();
            }
            _income.Special++;
            _income.Research = 1;

            Resources GetInc()
            {
                Resources newInc = new();
                for (int a = 0; a < Resources.NumMapResources; a++)
                    newInc[a] = MTRandom.Round(Tile.Map.Game.Consts.CoreIncome[a] * Math.Pow(_curIncMult, exponent[a]), _rounding);
                return newInc;
            }
        }
        internal override void EndTurn(ref Resources resources)
        {
            resources += Income();
            ReduceIncome();
            base.EndTurn(ref resources);
        }

        private Resources Income()
        {
            return _income;
        }

        public override string ToString() => "Core";
    }
}
