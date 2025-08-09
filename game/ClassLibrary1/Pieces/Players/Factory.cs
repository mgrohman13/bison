using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using IRepairable = ClassLibrary1.Pieces.Behavior.Combat.IKillable.IRepairable;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Factory : FoundationPiece, IRepairable
    {
        private readonly double _rangeMult, _rounding;

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

            this.Vision = values.Vision;
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
            if (!HasBehavior<IMissileSilo>() && research.HasType(Research.Type.Missile))
                SetBehavior(new MissileSilo(this));
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
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.FactoryAutoRepair);
        public bool CanRepair() => Consts.CanRepair(this);

        public override string ToString()
        {
            return "Factory " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .5;

            private int energy, mass;
            private double vision, rounding, repairRate;
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
            public double Rounding => rounding;
            public IRepair.Values GetRepair(Game game, double rangeMult, double rounding)
            {
                IRepair.Values repair = this.repair;
                int rate = Math.Max(1, MTRandom.Round(repairRate / rangeMult, rounding));
                if (!game.Player.Research.HasType(Research.Type.FactoryRepair))
                    rate = 1;
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
                double costMult = ResearchUpgValues.Calc(UpgType.FactoryCost, researchMult);
                rounding = Game.Rand.NextDouble();
                this.energy = MTRandom.Round(1700 * costMult, 1 - rounding);
                this.mass = MTRandom.Round(550 * costMult, rounding);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                double defAvg = ResearchUpgValues.Calc(UpgType.FactoryDefense, researchMult);
                int defense = Game.Rand.Round(defAvg);
                this.vision = ResearchUpgValues.Calc(UpgType.FactoryVision, researchMult);
                this.killable = new(DefenseType.Hits, defense);
            }
            private void UpgradeFactoryRepair(double researchMult)
            {
                double repairMult = ResearchUpgValues.Calc(UpgType.FactoryRepair, researchMult);
                double repairRange = 7.8 * Math.Sqrt(repairMult);
                this.repairRate = repairMult;
                this.repair = new(new(repairRange), 1);
            }
        }
    }
}
