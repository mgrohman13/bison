using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static ClassLibrary1.Pieces.Behavior.Combat.CombatTypes;
using IRepairable = ClassLibrary1.Pieces.Behavior.Combat.IKillable.IRepairable;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Outpost : FoundationPiece, IRepairable
    {
        public static double Resilience => Values.Resilience;

        private readonly double _rangeMult, _rounding;

        private Outpost(Tile tile, Values values)
            : base(tile, values.Vision)
        {
            this._rounding = Game.Rand.NextDouble();
            this._rangeMult = Game.Rand.GaussianCapped(values.Range, .13, Attack.MELEE_RANGE) / values.Range;

            SetBehavior(new Killable(this, new IKillable.Values(), Values.Resilience));
            Unlock();
        }

        internal static Outpost NewOutpost(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Outpost obj = new(tile, GetValues(foundation.Game));
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

        protected override bool CanReplace<T>(out Tuple<double, double> rounding)
        {
            rounding = new(GetValues(Game).Rounding, _rounding);
            return typeof(T) != this.GetType();
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock();
        }
        private void Upgrade()
        {
            Values values = GetValues(Game);

            IKillable killable = GetBehavior<IKillable>();
            killable.Upgrade(values.GetKillable(Game, _rounding), Values.Resilience);

            if (HasBehavior(out IAttacker attacker))
                attacker.Upgrade([values.GetAttack(_rangeMult, _rounding)]);
            if (HasBehavior(out IRepair repair))
                repair.Upgrade(values.GetRepair(_rangeMult));

            Builder.UpgradeAll(this, new(values.Range));

            this.Vision = values.Vision;
        }
        private void Unlock()
        {
            Research research = Game.Player.Research;

            if (!HasBehavior<IBuilder.IBuildOutpost>() && research.HasType(Research.Type.Outpost))
                SetBehavior(new Builder.BuildOutpost(this, new()));
            if (!HasBehavior<IBuilder.IBuildFactory>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, new()));
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, new()));
            if (!HasBehavior<IBuilder.IBuildGenerator>() && research.HasType(Research.Type.AmbientGenerator))
                SetBehavior(new Builder.BuildGenerator(this, new()));

            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.OutpostRepair))
                SetBehavior(new Repair(this, new()));

            if (!HasBehavior<IAttacker>() && research.HasType(Research.Type.OutpostAttack))
                SetBehavior(new Attacker(this, []));

            Upgrade();
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();

        internal static IRepair.Values GetRepair(Game game, double mult) => GetValues(game).GetRepair(mult);

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass);
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.BuildingAutoRepair);
        public bool CanRepair() => Consts.CanRepair(this);

        public override string ToString() =>
            "Outpost " + PieceNum;

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            public const double Resilience = .55;

            private int energy, mass;
            private double rounding, att, def, vision, repair;

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double Range => repair;
            public double Rounding => rounding;

            public IKillable.Values[] GetKillable(Game game, double rounding)
            {
                List<IKillable.Values> defenses = [new(DefenseType.Hits, MTRandom.Round(this.def, rounding))];
                if (game.Player.Research.HasType(Research.Type.OutpostArmor))
                    defenses.Add(new IKillable.Values(DefenseType.Armor, MTRandom.Round(this.def / 1.69, Consts.MAX_ROUND - rounding)));
                return [.. defenses];
            }
            public IAttacker.Values GetAttack(double rangeMult, double rounding)
            {
                double att = this.att / Math.Sqrt(rangeMult);
                att = Math.Max(att, 1);
                return new(AttackType.Kinetic, MTRandom.Round(att, rounding), Attack.MELEE_RANGE, 1);
            }
            public IRepair.Values GetRepair(double rangeMult)
            {
                double range = this.repair * rangeMult;
                range = Math.Max(range, Attack.MELEE_RANGE);
                return new(new(range), 1);
            }

            public void Init(Game game)
            {
                UpgradeTurretAttack(game, 1);
                UpgradeBuildingCost(game, 1);
                UpgradeBuildingDefense(game, 1);
                UpgradeFactoryRepair(game, 1);
            }
            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(game, researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(game, researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingDefense(game, researchMult);
                else if (type == Research.Type.FactoryRepair)
                    UpgradeFactoryRepair(game, researchMult);
            }
            private void UpgradeBuildingCost(Game game, double researchMult)
            {
                this.rounding = Game.Rand.NextDouble();
                double costMult = game.ResearchUpgValues.Calc(UpgType.OutpostCost, researchMult);
                double e = 800 * costMult;
                double m = 350 * costMult;
                this.energy = MTRandom.Round(e, rounding);
                this.mass = MTRandom.Round(m, Consts.MAX_ROUND - rounding);
            }
            private void UpgradeTurretAttack(Game game, double researchMult)
            {
                this.att = game.ResearchUpgValues.Calc(UpgType.OutpostAttack, researchMult);
            }
            private void UpgradeBuildingDefense(Game game, double researchMult)
            {
                this.def = game.ResearchUpgValues.Calc(UpgType.OutpostDefense, researchMult);
                this.vision = game.ResearchUpgValues.Calc(UpgType.OutpostVision, researchMult);
            }
            private void UpgradeFactoryRepair(Game game, double researchMult)
            {
                this.repair = game.ResearchUpgValues.Calc(UpgType.OutpostRepair, researchMult);
            }
        }
    }
}
