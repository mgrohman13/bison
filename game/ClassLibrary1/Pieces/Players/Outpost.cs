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

        public Factory ReplaceFactory(bool doReplace, out int energy, out int mass, out bool canReplace)
        {
            canReplace = HasBehavior<IBuilder.IBuildFactory>();
            Factory.Cost(Game, out int energyCost, out int massCost);
            return Replace(doReplace, out energy, out mass, ref canReplace,
                    energyCost, massCost, f => Factory.NewFactory(f));
        }
        public Turret ReplaceTurret(bool doReplace, out int energy, out int mass, out bool canReplace)
        {
            canReplace = HasBehavior<IBuilder.IBuildTurret>();
            Turret.Cost(Game, out int energyCost, out int massCost);
            return Replace(doReplace, out energy, out mass, ref canReplace,
                energyCost, massCost, f => Turret.NewTurret(f));
        }
        private T Replace<T>(bool doReplace, out int energy, out int mass, ref bool canReplace,
            double energyCost, double massCost, Func<Foundation, T> NewPiece) where T : FoundationPiece
        {
            T newPiece = null;

            DisbandValue(out double e, out double m);
            static void Mult(ref double v) => v *= Consts.UpgRefundValue / Consts.DisbandValue;
            Mult(ref e);
            Mult(ref m);

            GetValues(Game).Round(energyCost - e, massCost - m, out energy, out mass);

            canReplace &= Game.Player.Has(energy, mass);
            if (doReplace && canReplace)
            {
                this.Die(out Tile tile, out double treasure);
                Game.Enemy.AddResources(-treasure);
                if (tile.Piece is Foundation f && Game.Player.Spend(energy, mass))
                    newPiece = NewPiece(f);
                else
                    ;
            }
            return newPiece;
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

            if (!HasBehavior<IBuilder.IBuildFactory>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, new()));
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, new()));

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

            public Values()
            {
                UpgradeTurretAttack(1);
                UpgradeBuildingCost(1);
                UpgradeBuildingDefense(1);
                UpgradeFactoryRepair(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double Range => repair;

            public void Round(double e, double m, out int energy, out int mass)
            {
                energy = MTRandom.Round(e, rounding);
                mass = MTRandom.Round(m, Consts.MAX_ROUND - rounding);
            }

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

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingDefense(researchMult);
                else if (type == Research.Type.FactoryRepair)
                    UpgradeFactoryRepair(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                this.rounding = Game.Rand.NextDouble();
                double costMult = ResearchUpgValues.Calc(UpgType.OutpostCost, researchMult);
                Round(800 * costMult, 350 * costMult, out this.energy, out this.mass);
            }
            private void UpgradeTurretAttack(double researchMult)
            {
                this.att = ResearchUpgValues.Calc(UpgType.OutpostAttack, researchMult);
            }
            private void UpgradeBuildingDefense(double researchMult)
            {
                this.def = ResearchUpgValues.Calc(UpgType.OutpostDefense, researchMult);
                this.vision = ResearchUpgValues.Calc(UpgType.OutpostVision, researchMult);
            }
            private void UpgradeFactoryRepair(double researchMult)
            {
                this.repair = ResearchUpgValues.Calc(UpgType.OutpostRepair, researchMult);
            }
        }
    }
}
