using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using AttackType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Turret : FoundationPiece, IKillable.IRepairable
    {
        public const int MAX_ATTACKS = 3;
        public const int MAX_DEFENSES = 3;
        public static double Resilience => Values.Resilience;

        private readonly double _shieldMult, _armorMult, _rounding;
        private readonly double[] _attMult = new double[MAX_ATTACKS];

        private Turret(Tile tile)
            : base(tile, 0)
        {
            this._shieldMult = Game.Rand.GaussianCapped(1, .13, .5);
            this._armorMult = Game.Rand.GaussianCapped(1 / _shieldMult, .13, .5 / _shieldMult);
            this._rounding = Game.Rand.NextDouble();
            double attMults = 1;
            for (int a = 0; a < MAX_ATTACKS; a++)
            {
                double devMult = (MAX_ATTACKS - a) / (double)MAX_ATTACKS;
                double mult = 1 / Game.Rand.GaussianCapped(attMults, .13 * devMult, attMults * (1 - .5 * devMult));
                this._attMult[a] = mult;
                attMults *= mult;
            }

            SetBehavior(
                new Killable(this, new IKillable.Values(), Values.Resilience),
                new Attacker(this, []));
            Unlock();
        }
        internal static Turret NewTurret(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Turret obj = new(tile);
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
            return typeof(T) == typeof(Generator);
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock();
        }
        private void Upgrade()
        {
            Values values = GetValues(Game);
            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _shieldMult, _armorMult, _rounding), Values.Resilience);
            GetBehavior<IAttacker>().Upgrade(values.GetAttacks(Game.Player.Research, _attMult, _rounding));
            Builder.UpgradeAll(this, new(.5));
            this.Vision = values.Vision;
        }
        private void Unlock()
        {
            Research research = Game.Player.Research;
            if (!HasBehavior<IBuilder.IBuildGenerator>() && research.HasType(Research.Type.AmbientGenerator))
                SetBehavior(new Builder.BuildGenerator(this, new()));
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
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.TurretAutoRepair);
        public bool CanRepair() => Consts.CanRepair(this);

        public override string ToString()
        {
            return "Turret " + PieceNum;
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            public const double Resilience = .6;

            private int energy, mass;
            private double rounding, vision;

            private readonly IKillable.Values[] defenses = new IKillable.Values[MAX_DEFENSES];
            private readonly IAttacker.Values[] attacks = new IAttacker.Values[MAX_ATTACKS];

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double[] AttackRange => [.. attacks.Select(v => v.Range)];
            public double Rounding => rounding;

            public List<IKillable.Values> GetKillable(Research research, double shieldMult, double armorMult, double rounding)
            {
                List<IKillable.Values> results = [];

                double hitsMult = 1;
                for (int a = MAX_ATTACKS; --a >= 0;)
                {
                    IKillable.Values defense = this.defenses[a];
                    double mult = a switch { 0 => hitsMult, 1 => shieldMult, 2 => armorMult, _ => throw new Exception() };
                    int def = MTRandom.Round(Consts.StatValueInverse(Consts.StatValue(defense.Defense) * mult), Consts.MAX_ROUND - rounding);
                    def = Math.Max(def, 1);
                    bool has = a == 0
                        || (a == 1 && research.HasType(Research.Type.TurretShields))
                        || (a == 2 && research.HasType(Research.Type.TurretArmor));
                    if (has)
                    {
                        results.Add(new(defense.Type, def));
                        hitsMult *= Consts.StatValueInverse(Consts.StatValue(defense.Defense) / Consts.StatValue(def));
                    }
                    else
                        ;
                }

                results.Reverse();
                return results;
            }
            public List<IAttacker.Values> GetAttacks(Research research, double[] attMult, double rounding)
            {
                List<IAttacker.Values> results = [];

                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    IAttacker.Values attack = this.attacks[a];
                    AttackType type = attack.Type;

                    int baseAtt = attack.Attack;
                    int att = MTRandom.Round(baseAtt * attMult[a], rounding);
                    if (att < 1)
                        att = 1;
                    double mult = Math.Sqrt(Consts.StatValue(baseAtt) / Consts.StatValue(att));

                    double baseReload = (1 + CombatTypes.ReloadAvg(type, baseAtt)) / 2.0;
                    int reload = MTRandom.Round(baseReload * mult, rounding);
                    reload = Math.Min(Math.Max(reload, 1), att);
                    mult *= baseReload / reload;

                    double range = attack.Range * Math.Sqrt(mult);
                    range = Math.Max(range, Attack.MIN_RANGED);

                    results.Add(new(type, att, range, reload));
                }

                if (!research.HasType(Research.Type.TurretExplosives))
                    results.RemoveAt(2);
                if (!research.HasType(Research.Type.TurretLasers))
                    results.RemoveAt(1);

                return results;
            }

            public void Init(Game game)
            {
                for (int a = 0; a < MAX_DEFENSES; a++)
                    defenses[a] = new(DefenseType.Hits, 1);
                for (int a = 0; a < MAX_ATTACKS; a++)
                    attacks[a] = new(AttackType.Kinetic, 1, Attack.MELEE_RANGE);

                UpgradeBuildingCost(game, 1);
                UpgradeTurretDefense(game, 1);
                UpgradeTurretAttack(game, 1);
                UpgradeTurretRange(game, 1);
            }
            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(game, researchMult);
                else if (type == Research.Type.TurretDefense)
                    UpgradeTurretDefense(game, researchMult);
                else if (type == Research.Type.TurretRange)
                    UpgradeTurretRange(game, researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(game, researchMult);
            }
            private void UpgradeBuildingCost(Game game, double researchMult)
            {
                this.rounding = Game.Rand.NextDouble();
                double costMult = game.ResearchUpgValues.Calc(UpgType.TurretCost, researchMult);
                double e = 1150 * costMult;
                double m = 1550 * costMult;
                this.energy = MTRandom.Round(e, rounding);
                this.mass = MTRandom.Round(m, Consts.MAX_ROUND - rounding);
            }
            private void UpgradeTurretDefense(Game game, double researchMult)
            {
                this.vision = game.ResearchUpgValues.Calc(UpgType.TurretVision, researchMult);

                for (int a = 0; a < MAX_DEFENSES; a++)
                {
                    UpgType upgType = a switch
                    {
                        0 => UpgType.TurretDefense,
                        1 => UpgType.TurretShieldDefense,
                        2 => UpgType.TurretArmorDefense,
                        _ => throw new Exception(),
                    };
                    DefenseType type = a switch
                    {
                        0 => DefenseType.Hits,
                        1 => DefenseType.Shield,
                        2 => DefenseType.Armor,
                        _ => throw new Exception(),
                    };

                    int defense = Game.Rand.Round(game.ResearchUpgValues.Calc(upgType, researchMult));
                    this.defenses[a] = new(type, defense);
                }
            }
            private void UpgradeTurretRange(Game game, double researchMult)
            {
                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    UpgType upgType = a switch
                    {
                        0 => UpgType.TurretRange,
                        1 => UpgType.TurretLaserRange,
                        2 => UpgType.TurretExplosivesRange,
                        _ => throw new Exception(),
                    };

                    double range = game.ResearchUpgValues.Calc(upgType, researchMult);
                    this.attacks[a] = new(attacks[a].Type, attacks[a].Attack, range, 1);
                }
            }
            private void UpgradeTurretAttack(Game game, double researchMult)
            {
                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    UpgType upgType = a switch
                    {
                        0 => UpgType.TurretAttack,
                        1 => UpgType.TurretLaserAttack,
                        2 => UpgType.TurretExplosivesAttack,
                        _ => throw new Exception(),
                    };
                    AttackType type = a switch
                    {
                        0 => AttackType.Kinetic,
                        1 => AttackType.Energy,
                        2 => AttackType.Explosive,
                        _ => throw new Exception(),
                    };

                    int attack = Game.Rand.Round(game.ResearchUpgValues.Calc(upgType, researchMult));
                    this.attacks[a] = new(type, attack, attacks[a].Range, 1);
                }
            }
        }
    }
}
