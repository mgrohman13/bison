using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Factory : FoundationPiece, IKillable.IRepairable
    {
        private readonly double _rangeMult, _rounding;
        public Piece Piece => this;

        private Factory(Tile tile, Values values)
            : base(tile, values.Vision)
        {
            this._rangeMult = Game.Rand.GaussianOE(values.BuilderRange, .169, .13, 1) / values.BuilderRange;
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(new Killable(this, values.Killable, values.Resilience));
            Unlock();
        }

        internal static Factory NewFactory(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Factory obj = new(tile, GetValues(tile.Map.Game));
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock();
            Values values = GetValues(Game);

            this._vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(new[] { values.Killable }, values.Resilience);
            if (HasBehavior(out IRepair repair))
                repair.Upgrade(values.GetRepair(Game, _rangeMult, _rounding));
            Builder.UpgradeAll(this, values.GetRepair(Game, _rangeMult, _rounding).Builder);
        }
        private void Unlock()
        {
            Research research = Game.Player.Research;
            Values values = GetValues(Game);
            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this, values.GetRepair(Game, _rangeMult, _rounding).Builder));
            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.FactoryRepair))
                SetBehavior(new Repair(this, values.GetRepair(Game, _rangeMult, _rounding)));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.FactoryConstructor))
                SetBehavior(new Builder.BuildConstructor(this, values.GetRepair(Game, _rangeMult, _rounding).Builder));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }
        internal static double GetRounding(Game game)
        {
            return GetValues(game).Rounding;
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out int energy, out int mass);
                return Consts.GetRepairCost(energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.FactoryAutoRepair);
        public bool CanRepair() => Consts.CanRepair(Piece);

        public override string ToString()
        {
            return "Factory " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .5;

            private int energy, mass;
            private double vision, hitRound, repairRate;
            private IKillable.Values killable;
            private IRepair.Values repair;
            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeFactoryRepair(1);
            }

            public double Resilience => resilience;
            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public IKillable.Values Killable => killable;
            public double BuilderRange => repair.Builder.Range;
            public double Rounding => hitRound;
            public IRepair.Values GetRepair(Game game, double rangeMult, double rounding)
            {
                IRepair.Values repair = this.repair;
                int rate = Math.Max(1, MTRandom.Round(repairRate / rangeMult, rounding));
                //if (!game.Player.Research.HasType(Research.Type.FactoryRepair))
                //    rate = 1;

                double range = repair.Builder.Range * repairRate / rate;
                return new(new(range), rate);
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.FactoryRepair)
                    UpgradeFactoryRepair(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .3);
                hitRound = Game.Rand.NextDouble();
                this.energy = MTRandom.Round(1750 / researchMult, hitRound);
                this.mass = MTRandom.Round(500 / researchMult, hitRound);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                double defAvg = 6 * Math.Pow(researchMult, .4);
                const double lowPenalty = 6 / 5.0;
                if (researchMult < lowPenalty)
                    defAvg *= researchMult / lowPenalty;
                int defense = Game.Rand.Round(defAvg);
                this.vision = 4 * Math.Pow(researchMult, .8);
                this.killable = new(DefenseType.Hits, defense);
            }
            private void UpgradeFactoryRepair(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .6);
                double repairRange = 6.5 * researchMult;
                repairRate = 1.13 * researchMult;// Consts.GetPct(..065, researchMult);
                this.repair = new(new(repairRange), 1);// repairRate);
            }
        }
    }
}
