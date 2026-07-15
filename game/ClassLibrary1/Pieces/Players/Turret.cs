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
            Upgrade();
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

        internal override void OnResearch(Research.Type type)
        {
            Upgrade();
        }
        private void Upgrade()
        {
            Values values = GetValues(Game);
            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _shieldMult, _armorMult, _rounding), Values.Resilience);
            GetBehavior<IAttacker>().Upgrade(values.GetAttacks(Game.Player.Research, _attMult, _rounding));
            this.Vision = values.Vision;
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
            private double vision;

            private readonly IKillable.Values[] defenses;
            private readonly IAttacker.Values[] attacks;

            public Values()
            {
                this.defenses = new IKillable.Values[MAX_DEFENSES];
                for (int a = 0; a < MAX_DEFENSES; a++)
                    defenses[a] = new(DefenseType.Hits, 1);

                this.attacks = new IAttacker.Values[MAX_ATTACKS];
                for (int a = 0; a < MAX_ATTACKS; a++)
                    attacks[a] = new(AttackType.Kinetic, 1, Attack.MELEE_RANGE);

                UpgradeBuildingCost(1);
                //UpgradeBuildingHits(1);
                UpgradeTurretDefense(1);
                UpgradeTurretAttack(1);
                UpgradeTurretRange(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double[] AttackRange => [.. attacks.Select(v => v.Range)];

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
                    bool has = ((a == 1 && research.HasType(Research.Type.TurretShields))
                         || (a == 2 && research.HasType(Research.Type.TurretArmor)));
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

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.TurretDefense)
                    UpgradeTurretDefense(researchMult);
                else if (type == Research.Type.TurretRange)
                    UpgradeTurretRange(researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                double costMult = ResearchUpgValues.Calc(UpgType.TurretCost, researchMult);
                this.energy = Game.Rand.Round(1150 * costMult);
                this.mass = Game.Rand.Round(1550 * costMult);
            }
            private void UpgradeTurretDefense(double researchMult)
            {
                this.vision = ResearchUpgValues.Calc(UpgType.TurretVision, researchMult);

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

                    int defense = Game.Rand.Round(ResearchUpgValues.Calc(upgType, researchMult));
                    this.defenses[a] = new(type, defense);
                }
            }
            private void UpgradeTurretRange(double researchMult)
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

                    double range = ResearchUpgValues.Calc(upgType, researchMult);
                    this.attacks[a] = new(attacks[a].Type, attacks[a].Attack, range, 1);
                }
            }
            private void UpgradeTurretAttack(double researchMult)
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

                    int attack = Game.Rand.Round(ResearchUpgValues.Calc(upgType, researchMult));
                    this.attacks[a] = new(type, attack, attacks[a].Range, 1);
                }
            }
        }
    }
}
