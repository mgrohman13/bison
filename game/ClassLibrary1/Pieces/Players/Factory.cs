using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using IRepairable = ClassLibrary1.Pieces.Behavior.Combat.IKillable.IRepairable;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Factory : FoundationPiece, IRepairable
    {
        public static double Resilience => Values.Resilience;

        private readonly double _rangeMult, _rounding;

        private Factory(Tile tile, Values values)
            : base(tile, values.Vision)
        {
            this._rangeMult = Game.Rand.GaussianOE(values.Range, .169, .13, Attack.MIN_RANGED) / values.Range;
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(
                new Killable(this, new IKillable.Values(), Values.Resilience),
                new Repair(this, new()));
            Unlock();
        }

        internal static Factory NewFactory(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Factory obj = new(tile, GetValues(foundation.Game));
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }
        internal override void Cost(out int energy, out int mass) =>
            Cost(Game, out energy, out mass);

        internal override void OnResearch(Research.Type type)
        {
            Unlock();
        }
        private void Upgrade()
        {
            Values values = GetValues(Game);

            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game, _rounding), Values.Resilience);

            IRepair.Values repair = values.GetRepair(Game, _rangeMult, _rounding);
            GetBehavior<IRepair>().Upgrade(repair);
            Builder.UpgradeAll(this, repair.Builder);

            this.Vision = values.Vision;
        }
        private void Unlock()
        {
            Research research = Game.Player.Research;

            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this, new()));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.Constructor))
                SetBehavior(new Builder.BuildConstructor(this, new()));
            if (!HasBehavior<IBuilder.IBuildFactory>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, new()));
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, new()));

            if (!HasBehavior<IMissileSilo>() && research.HasType(Research.Type.Missile))
                SetBehavior(new MissileSilo(this));

            Upgrade();
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass);
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
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            public const double Resilience = .5;

            private int energy, mass;
            private double def, vision, repairRate;
            private IRepair.Values repair;

            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingDefense(1);
                UpgradeFactoryRepair(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double Range => repair.Builder.Range;

            public List<IKillable.Values> GetKillable(Game game, double rounding)
            {
                List<IKillable.Values> defenses = [new(DefenseType.Hits, MTRandom.Round(this.def, Consts.MAX_ROUND - rounding))];
                if (game.Player.Research.HasType(Research.Type.FactoryShields))
                    defenses.Add(new IKillable.Values(DefenseType.Shield, MTRandom.Round(this.def / Math.PI, rounding)));
                return defenses;
            }
            public IRepair.Values GetRepair(Game game, double rangeMult, double rounding)
            {
                if (game.Player.Research.HasType(Research.Type.FactoryRepair))
                {
                    int rate = MTRandom.Round(this.repairRate / rangeMult, rounding);
                    rate = Math.Max(rate, 1);
                    double range = this.repair.Builder.Range * this.repairRate / rate;
                    range = Math.Max(range, Attack.MELEE_RANGE);
                    return new(new(range), rate);
                }
                else
                {
                    return Outpost.GetRepair(game, Math.Sqrt(2));
                }
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingDefense(researchMult);
                else if (type == Research.Type.FactoryRepair)
                    UpgradeFactoryRepair(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                double costMult = ResearchUpgValues.Calc(UpgType.FactoryCost, researchMult);
                this.energy = Game.Rand.Round(1700 * costMult);
                this.mass = Game.Rand.Round(550 * costMult);
            }
            private void UpgradeBuildingDefense(double researchMult)
            {
                this.def = ResearchUpgValues.Calc(UpgType.FactoryDefense, researchMult);
                this.vision = ResearchUpgValues.Calc(UpgType.FactoryVision, researchMult);
            }
            private void UpgradeFactoryRepair(double researchMult)
            {
                this.repairRate = ResearchUpgValues.Calc(UpgType.FactoryRepair, researchMult);
                this.repair = new(new(7.50 * Math.Sqrt(repairRate)), 1);
            }
        }
    }
}
