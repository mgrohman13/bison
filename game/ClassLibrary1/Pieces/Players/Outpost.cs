using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Runtime.Serialization;
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

        private readonly double _rounding;

        private Outpost(Tile tile, Values values)
            : base(tile, 0)
        {
            this._rounding = Game.Rand.NextDouble();

            SetBehavior(new Killable(this, values.GetKillable(_rounding), Values.Resilience));
            Unlock();
        }

        internal static Outpost NewOutpost(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Outpost obj = new(tile, GetValues(tile.Map.Game));
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        internal override void OnResearch(Research.Type type) => Unlock();
        private void Unlock()
        {
            Research research = Game.Player.Research;
            Values values = GetValues(Game);

            if (!HasBehavior<IAttacker>() && research.HasType(Research.Type.OutpostAttack))
                SetBehavior(new Attacker(this, [values.GetAttack(_rounding)]));
            if (!HasBehavior<IRepair>() && research.HasType(Research.Type.OutpostRepair))
                SetBehavior(new Repair(this, values.GetRepair(GetBehavior<IKillable>(), GetBehavior<IAttacker>())));

            if (!HasBehavior<IBuilder.IBuildFactory>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, new(.5)));
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, new(.5)));

            Upgrade();
        }
        private void Upgrade()
        {
            Values values = GetValues(Game);
            IKillable killable = GetBehavior<IKillable>();
            killable.Upgrade([values.GetKillable(_rounding)], Values.Resilience);
            if (HasBehavior(out IAttacker attacker))
                attacker.Upgrade([values.GetAttack(_rounding)]);
            if (HasBehavior(out IRepair repair))
                repair.Upgrade(values.GetRepair(killable, attacker));
            this.Vision = values.GetVision(killable, attacker, repair);
        }
        private static Values GetValues(Game game) =>
            game.Player.GetUpgradeValues<Values>();
        internal static double GetRounding(Game game) =>
            GetValues(game).CostRounding;

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out int energy, out int mass);
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

            private int _energy, _mass;
            private double _rounding, _att, _def, _vision, _repair;
            public Values()
            {
                UpgradeTurretAttack(1);
                UpgradeBuildingCost(1);
                UpgradeBuildingDefense(1);
                UpgradeFactoryRepair(1);
            }

            public int Energy => _energy;
            public int Mass => _mass;
            public double CostRounding => _rounding;

            public IKillable.Values GetKillable(double rounding) =>
                new(CombatTypes.DefenseType.Hits, MTRandom.Round(_def, rounding));
            public IAttacker.Values GetAttack(double rounding) =>
                new(CombatTypes.AttackType.Kinetic, MTRandom.Round(_att, 1 - rounding), Attack.MELEE_RANGE, 1);
            public IRepair.Values GetRepair(IKillable killable, IAttacker attacker) =>
                new(new(Math.Max(Attack.MELEE_RANGE, GetRepairBase(killable, attacker))), 1);
            private double GetRepairBase(IKillable killable, IAttacker attacker) =>
                _repair * GetMult(killable, attacker);
            public double GetVision(IKillable killable, IAttacker attacker, IRepair repair)
            {
                double mult = repair == null ? GetMult(killable, attacker) : 1;
                if (repair != null)
                    mult *= GetRepairBase(killable, attacker) / repair.RangeBase;
                return _vision * mult * mult;
            }
            private double GetMult(IKillable killable, IAttacker attacker)
            {
                double numerator = Consts.StatValue(_def);
                double denominator = Consts.StatValue(killable.Hits.DefenseMax);
                if (attacker != null)
                {
                    numerator += Consts.StatValue(_att);
                    denominator += Consts.StatValue(attacker.Attacks[0].AttackMax);
                }
                return numerator / denominator;
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(researchMult);
                else if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingDefense(researchMult);
                else if (type == Research.Type.FactoryRepair)
                    UpgradeFactoryRepair(researchMult);
            }
            private void UpgradeTurretAttack(double researchMult) =>
                this._att = ResearchUpgValues.Calc(UpgType.OutpostAttack, researchMult);
            private void UpgradeBuildingCost(double researchMult)
            {
                double costMult = ResearchUpgValues.Calc(UpgType.OutpostCost, researchMult);
                _rounding = Game.Rand.NextDouble();
                this._energy = MTRandom.Round(450 * costMult, 1 - _rounding);
                this._mass = MTRandom.Round(350 * costMult, _rounding);
            }
            private void UpgradeBuildingDefense(double researchMult)
            {
                this._def = ResearchUpgValues.Calc(UpgType.OutpostDefense, researchMult);
                this._vision = ResearchUpgValues.Calc(UpgType.OutpostVision, researchMult);
            }
            private void UpgradeFactoryRepair(double researchMult) =>
                this._repair = ResearchUpgValues.Calc(UpgType.OutpostRepair, researchMult);
        }
    }
}
