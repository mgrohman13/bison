using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Turret : FoundationPiece, IKillable.IRepairable
    {
        public const int MAX_ATTACKS = 3;
        public const int MAX_DEFENSES = 3;

        private readonly double _shieldMult, _armorMult, _rounding;
        private readonly double[] _attMult = new double[MAX_ATTACKS];

        private Turret(Tile tile, Values values)
            : base(tile, values.Vision)
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
                new Killable(this, values.GetKillable(Game.Player.Research, _shieldMult, _armorMult, _rounding), values.Resilience),
                new Attacker(this, values.GetAttacks(Game.Player.Research, _attMult, _rounding)));
        }
        internal static Turret NewTurret(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Turret obj = new(tile, GetValues(foundation.Game));
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
            Values values = GetValues(Game);

            this.Vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _shieldMult, _armorMult, _rounding), values.Resilience);
            GetBehavior<IAttacker>().Upgrade(values.GetAttacks(Game.Player.Research, _attMult, _rounding));
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
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.TurretAutoRepair);
        public bool CanRepair() => Consts.CanRepair(this);

        public override string ToString()
        {
            return "Turret " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .6;

            private int energy, mass;
            private double vision, rounding;

            //private readonly IKillable.Values hits;
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

            public double Resilience => resilience;
            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double[] AttackRange => attacks.Select(v => v.Range).ToArray();
            public double Rounding => rounding;
            public IKillable.Values[] GetKillable(Research research, double shieldMult, double armorMult, double rounding)
            {
                double hitsMult = 1;

                List<IKillable.Values> results = new();
                for (int a = MAX_ATTACKS; --a >= 0;)
                {
                    IKillable.Values defense = defenses[a];
                    double mult = a switch { 0 => hitsMult, 1 => shieldMult, 2 => armorMult, _ => throw new Exception() };
                    int def = Math.Max(1, MTRandom.Round(Consts.StatValueInverse(Consts.StatValue(defense.Defense) * mult), 1 - rounding));
                    results.Add(new(defense.Type, def));

                    hitsMult *= Consts.StatValueInverse(Consts.StatValue(defense.Defense) / Consts.StatValue(def));
                }

                results.Reverse();
                if (!research.HasType(Research.Type.TurretArmor))
                    results.RemoveAt(2);
                if (!research.HasType(Research.Type.TurretShields))
                    results.RemoveAt(1);

                return results.ToArray();
            }
            public IAttacker.Values[] GetAttacks(Research research, double[] _attMult, double rounding)
            {
                List<IAttacker.Values> results = new();
                bool offset = false;
                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    IAttacker.Values attack = attacks[a];
                    AttackType type = attack.Type;

                    int baseAtt = attack.Attack - (offset ? 1 : 0);
                    int att = MTRandom.Round(baseAtt * _attMult[a], rounding);
                    if (att < 1)
                        att = 1;
                    double mult = Math.Sqrt(Consts.StatValue(baseAtt) / Consts.StatValue(att));

                    double baseReload = (1 + CombatTypes.ReloadAvg(type, baseAtt)) / 2.0;
                    int reload = MTRandom.Round(baseReload * mult, rounding);
                    if (reload < 1)
                        reload = 1;
                    else if (reload > att)
                        reload = att;
                    mult *= Math.Sqrt(baseReload / reload);

                    double range = attack.Range * mult;
                    if (range < Attack.MIN_RANGED)
                    {
                        if (!offset)
                        {
                            offset = true;
                            a--;
                            continue;
                        }
                        else
                            Debug.WriteLine("!!! turret min range " + range);
                        range = Attack.MIN_RANGED;
                    }
                    offset = false;

                    results.Add(new(type, att, range, reload));
                }

                if (!research.HasType(Research.Type.TurretExplosives))
                    results.RemoveAt(2);
                if (!research.HasType(Research.Type.TurretLasers))
                    results.RemoveAt(1);

                return results.ToArray();
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
                rounding = Game.Rand.NextDouble();
                this.energy = MTRandom.Round(1250 * costMult, 1 - rounding);
                this.mass = MTRandom.Round(1550 * costMult, rounding);
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

                    double defAvg = ResearchUpgValues.Calc(upgType, researchMult);
                    int defense = Game.Rand.Round(defAvg);
                    defenses[a] = new(type, defense);
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
                    IAttacker.Values attack = attacks[a];
                    attacks[a] = new(attack.Type, attack.Attack, range, 1);
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

                    double attAvg = ResearchUpgValues.Calc(upgType, researchMult);
                    int attack = Game.Rand.Round(attAvg);

                    //double range = attacks[a].Range;
                    //int reload = attacks[a].Reload;
                    //attacks[a] = new(type, attack, range);
                    //reload = Math.Max(reload, Game.Rand.Round(1 + (attacks[a].Reload - 1) / 2.0)); //use in GetAttacks
                    attacks[a] = new(type, attack, attacks[a].Range, 1);
                }
            }
        }
    }
}
