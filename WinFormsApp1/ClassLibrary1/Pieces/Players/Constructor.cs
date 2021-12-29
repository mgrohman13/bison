using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MattUtil;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Constructor : PlayerPiece, IKillable.IRepairable
    {
        private bool _canUpgrade;
        private readonly bool _defenseType;
        private readonly double _rangeMult, _rounding;
        public Piece Piece => this;

        private Constructor(Map.Tile tile, Values values, bool starter)
            : base(tile, values.Vision)
        {
            this._canUpgrade = false;

            this._defenseType = Game.Rand.Bool();
            this._rangeMult = 1;
            this._rounding = Game.Rand.NextDouble();
            if (!starter)
                this._rangeMult = Game.Rand.GaussianOE(values.BuilderRange, .21, .26, 1) / values.BuilderRange;

            SetBehavior(
                new Killable(this, values.GetKillable(Game.Player.Research, _defenseType)),
                new Movable(this, values.GetMovable(_rangeMult, _rounding)),
                new Builder.BuildExtractor(this, values.GetRepair(_rangeMult).Builder));
            Unlock(Game.Player.Research);
        }
        internal static Constructor NewConstructor(Map.Tile tile, bool starter)
        {
            Constructor obj = new(tile, GetValues(tile.Map.Game), starter);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        public bool CanUpgrade => _canUpgrade;
        internal override void OnResearch(Research.Type type)
        {
            switch (type)
            {
                case Research.Type.ConstructorDefense:
                case Research.Type.ConstructorMove:
                case Research.Type.ConstructorRepair:
                    _canUpgrade = true;
                    Upgrade();
                    break;
                case Research.Type.Turret:
                case Research.Type.Factory:
                    UnlockBuilds(Game.Player.Research, GetValues(Game));
                    break;
            }
        }
        private bool Upgrade()
        {
            if (CanUpgrade && Side.PiecesOfType<IBuilder.IBuildConstructor>().Any(b => Tile.GetDistance(b.Piece.Tile) <= b.Range))
            {
                Unlock(Game.Player.Research);
                Values values = GetValues(Game);

                this._vision = values.Vision;
                GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _defenseType));
                GetBehavior<IMovable>().Upgrade(values.GetMovable(_rangeMult, _rounding));
                if (HasBehavior<IRepair>())
                    GetBehavior<IRepair>().Upgrade(values.GetRepair(_rangeMult));
                Builder.UpgradeAll(this, values.GetRepair(_rangeMult).Builder);

                _canUpgrade = false;
                return true;
            }
            return false;
        }
        private void Unlock(Research research)
        {
            Values values = GetValues(Game);
            UnlockBuilds(research, values);
            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.ConstructorRepair))
                SetBehavior(new Repair(this, values.GetRepair(_rangeMult)));
        }
        private void UnlockBuilds(Research research, Values values)
        {
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, values.GetRepair(_rangeMult).Builder));
            if (!HasBehavior<IBuilder.IBuildFactory>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, values.GetRepair(_rangeMult).Builder));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out int energy, out int mass);
                return Consts.GetRepairCost(energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => false;

        internal override void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            base.GetUpkeep(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
            Upgrade();
        }

        public override string ToString()
        {
            return "Constructor " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private int energy, mass;
            private double vision;
            private IKillable.Values killable;
            private IMovable.Values movable;
            private IRepair.Values repair;
            public Values()
            {
                UpgradeConstructorCost(1);
                UpgradeConstructorDefense(1);
                UpgradeConstructorMove(1);
                UpgradeConstructorRepair(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double BuilderRange => repair.Builder.Range;
            public IKillable.Values GetKillable(Research research, bool defenseType)
            {
                double armor = research.HasType(Research.Type.ConstructorDefense) && !defenseType ? killable.Armor : 0;
                bool shields = research.HasType(Research.Type.ConstructorDefense) && defenseType;
                double shieldInc = shields ? killable.ShieldInc : 0;
                int shieldMax = shields ? killable.ShieldMax : 0;
                int shieldLimit = shields ? killable.ShieldLimit : 0;
                return new IKillable.Values(killable.HitsMax, killable.Resilience, armor, shieldInc, shieldMax, shieldLimit);
            }
            internal IMovable.Values GetMovable(double rangeMult, double rounding)
            {
                double mult = 1 / Math.Pow(rangeMult, .26);
                double inc = movable.MoveInc * mult;
                int max = 1 + MTRandom.Round((movable.MoveMax - 1) * mult, rounding);
                int limit = 1 + MTRandom.Round((movable.MoveLimit - 1) * mult, rounding);
                return new IMovable.Values(inc, max, limit);
            }
            public IRepair.Values GetRepair(double rangeMult)
            {
                IRepair.Values repair = this.repair;
                double range = repair.Builder.Range * rangeMult;
                double rate = Consts.GetPct(repair.Rate, 1 / Math.Pow(rangeMult, .52));
                return new(new(range), rate);
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.ConstructorCost)
                    UpgradeConstructorCost(researchMult);
                else if (type == Research.Type.ConstructorDefense)
                    UpgradeConstructorDefense(researchMult);
                else if (type == Research.Type.ConstructorMove)
                    UpgradeConstructorMove(researchMult);
                else if (type == Research.Type.ConstructorRepair)
                    UpgradeConstructorRepair(researchMult);
            }
            private void UpgradeConstructorCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .5);
                this.energy = this.mass = Game.Rand.Round(750 / researchMult);
            }
            private void UpgradeConstructorDefense(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .8);
                double armor = Consts.GetPct(1 / 3.0, researchMult);
                double shieldInc = 1.69 * researchMult;
                int shieldMax = Game.Rand.Round(26 * researchMult);
                int shieldLimit = Game.Rand.Round(52 * researchMult);
                this.killable = new(50, .35, armor, shieldInc, shieldMax, shieldLimit);
            }
            private void UpgradeConstructorMove(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .4);
                double moveInc = 3 * researchMult;
                int moveMax = Game.Rand.Round(7 * researchMult);
                int moveLimit = Game.Rand.Round(12 * researchMult);
                this.vision = 4.5 * researchMult;
                this.movable = new(moveInc, moveMax, moveLimit);
            }
            private void UpgradeConstructorRepair(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .5);
                double repairRange = 3.5 * researchMult;
                double repairRate = Consts.GetPct(.052, researchMult);
                this.repair = new(new(repairRange), repairRate);
            }
        }
    }
}
