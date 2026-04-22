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
            this._rangeMult = Game.Rand.GaussianOE(values.BuilderRange, .169, .13, 1) / values.BuilderRange;
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(new Killable(this, values.GetKillable(Game, _rounding), Values.Resilience));
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
            Values values = GetValues(Game);

            this.Vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game, _rounding), Values.Resilience);
            if (HasBehavior(out IRepair repair))
                repair.Upgrade(values.GetRepair(Game, _rangeMult, _rounding));
            Builder.UpgradeAll(this, values.GetRepair(Game, _rangeMult, _rounding).Builder);
        }
        private void Unlock()
        {
            Research research = Game.Player.Research;
            Values values = GetValues(Game);

            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.FactoryRepair))
                SetBehavior(new Repair(this, values.GetRepair(Game, _rangeMult, _rounding)));
            if (!HasBehavior<IMissileSilo>() && research.HasType(Research.Type.Missile))
                SetBehavior(new MissileSilo(this));

            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this, values.GetRepair(Game, _rangeMult, _rounding).Builder));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.FactoryConstructor))
                SetBehavior(new Builder.BuildConstructor(this, values.GetRepair(Game, _rangeMult, _rounding).Builder));
            if (!HasBehavior<IBuilder.IBuildOutpost>() && research.HasType(Research.Type.Outpost))
                SetBehavior(new Builder.BuildOutpost(this, new(.5)));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }
        internal static double GetRounding(Game game)
        {
            return GetValues(game).CostRounding;
        }

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

            private int _energy, _mass, _def;
            private double _vision, _rounding, _repairRate;

            //private IKillable.Values killable;
            private IRepair.Values repair;

            public Values()
            {
                UpgradeBuildingCost(1);
                UpgradeBuildingDefense(1);
                UpgradeFactoryRepair(1);
            }

            public int Energy => _energy;
            public int Mass => _mass;
            public double Vision => _vision;
            //public IKillable.Values Killable => killable;
            public double BuilderRange => repair.Builder.Range;
            public double CostRounding => _rounding;

            public IKillable.Values[] GetKillable(Game game, double rounding)
            {
                List<IKillable.Values> defenses = [new(DefenseType.Hits, _def)];
                if (game.Player.Research.HasType(Research.Type.FactoryShields))
                    defenses.Add(new IKillable.Values(DefenseType.Armor, MTRandom.Round(_def / Math.PI, 1 - rounding)));
                return [.. defenses];
            }
            public IRepair.Values GetRepair(Game game, double rangeMult, double rounding)
            {
                IRepair.Values repair = this.repair;
                int rate = Math.Max(1, MTRandom.Round(_repairRate / rangeMult, rounding));
                if (!game.Player.Research.HasType(Research.Type.FactoryRepair))
                    rate = 1;
                double range = repair.Builder.Range * _repairRate / rate;
                return new(new(range), rate);
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
                _rounding = Game.Rand.NextDouble();
                this._energy = MTRandom.Round(1700 * costMult, 1 - _rounding);
                this._mass = MTRandom.Round(550 * costMult, _rounding);
            }
            private void UpgradeBuildingDefense(double researchMult)
            {
                double defAvg = ResearchUpgValues.Calc(UpgType.FactoryDefense, researchMult);
                this._def = Game.Rand.Round(defAvg);
                this._vision = ResearchUpgValues.Calc(UpgType.FactoryVision, researchMult);
                //this.killable = new(DefenseType.Hits, defense);
            }
            private void UpgradeFactoryRepair(double researchMult)
            {
                double repairMult = ResearchUpgValues.Calc(UpgType.FactoryRepair, researchMult);
                double repairRange = 7.50 * Math.Sqrt(repairMult);
                this._repairRate = repairMult;
                this.repair = new(new(repairRange), 1);
            }
        }
    }
}
