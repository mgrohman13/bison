﻿using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Turret : FoundationPiece, IKillable.IRepairable
    {
        public const int MAX_ATTACKS = 3;
        public const int MAX_DEFENSES = 3;

        private readonly double _shieldMult, _armorMult, _rounding;
        private readonly double[] _rangeMult = new double[MAX_ATTACKS];

        public Piece Piece => this;

        private Turret(Tile tile, Values values)
            : base(tile, values.Vision)
        {
            this._shieldMult = Game.Rand.GaussianCapped(1, .169, .21);
            this._armorMult = Game.Rand.GaussianCapped(1.0 / _shieldMult, .169, .21);
            this._rounding = Game.Rand.NextDouble();
            for (int a = 0; a < MAX_ATTACKS; a++)
                this._rangeMult[a] = Game.Rand.GaussianOE(values.AttackRange[a], .21, .26, 1) / values.AttackRange[a];

            SetBehavior(
                new Killable(Piece, values.GetKillable(Game.Player.Research, _shieldMult, _armorMult, _rounding), values.Resilience),
                new Attacker(this, values.GetAttacks(Game.Player.Research, _rangeMult, _rounding)));
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

            this._vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _shieldMult, _armorMult, _rounding), values.Resilience);
            GetBehavior<IAttacker>().Upgrade(values.GetAttacks(Game.Player.Research, _rangeMult, _rounding));
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
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.TurretAutoRepair);

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
                    defenses[a] = new(DefenseType.Hits, -1);

                this.attacks = new IAttacker.Values[MAX_ATTACKS];
                for (int a = 0; a < MAX_ATTACKS; a++)
                    attacks[a] = new(AttackType.Kinetic, -1);

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
                double hitsMult = Math.Sqrt(1.0 / shieldMult / armorMult);

                List<IKillable.Values> results = new();
                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    IKillable.Values defense = defenses[a];
                    double mult = a switch { 0 => hitsMult, 1 => shieldMult, 2 => armorMult, _ => throw new Exception() };
                    int def = Math.Max(1, MTRandom.Round(defense.Defense * mult, rounding));
                    results.Add(new(defense.Type, def));
                }

                if (!research.HasType(Research.Type.TurretArmor))
                    results.RemoveAt(2);
                if (!research.HasType(Research.Type.TurretShields))
                    results.RemoveAt(1);

                return results.ToArray();
            }
            public IAttacker.Values[] GetAttacks(Research research, double[] rangeMult, double rounding)
            {
                List<IAttacker.Values> results = new();
                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    IAttacker.Values attack = attacks[a];
                    int att = attack.Attack;
                    int damage = MTRandom.Round(attack.Attack / rangeMult[a], rounding);
                    if (damage < 1)
                        damage = 1;
                    double range = int.MaxValue;
                    do
                    {
                        if (range < Attack.MIN_RANGED && damage > 1)
                            --damage;
                        range = attack.Range * Math.Pow(attack.Attack / (double)damage, 1.3);
                    } while (range < Attack.MIN_RANGED && damage > 1);
                    if (range < Attack.MIN_RANGED)
                        range = Attack.MIN_RANGED;
                    results.Add(new(attack.Type, att, range));
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
                //else if (type == Research.Type.BuildingDefense)
                //    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.TurretDefense)
                    UpgradeTurretDefense(researchMult);
                else if (type == Research.Type.TurretRange)
                    UpgradeTurretRange(researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .4);
                rounding = Game.Rand.NextDouble();
                this.energy = MTRandom.Round(1000 / researchMult, rounding);
                this.mass = MTRandom.Round(1750 / researchMult, rounding);
            }
            private void UpgradeTurretDefense(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .6);
                this.vision = 20 * researchMult;

                for (int a = 0; a < MAX_DEFENSES; a++)
                {
                    DefenseType type = a switch
                    {
                        0 => DefenseType.Hits,
                        1 => DefenseType.Shield,
                        2 => DefenseType.Armor,
                        _ => throw new Exception(),
                    };

                    double devAvg = a switch { 0 => 13, 1 => 7.8, 2 => 16.9, _ => throw new Exception(), };
                    devAvg *= researchMult;
                    //dont pow researchMult first
                    //const double lowPenalty = 2.0;
                    //if (researchMult < lowPenalty)
                    //    attAvg *= researchMult / lowPenalty;

                    int defense = Game.Rand.Round(devAvg);
                    defenses[a] = new(type, defense);
                }
            }
            private void UpgradeTurretRange(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .7);
                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    double range = a switch { 0 => 16.9, 1 => 9.1, 2 => 10.4, _ => throw new Exception(), };
                    range *= researchMult;
                    IAttacker.Values attack = attacks[a];
                    attacks[a] = new(attack.Type, attack.Attack, range);
                }
            }
            private void UpgradeTurretAttack(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .5);
                for (int a = 0; a < MAX_ATTACKS; a++)
                {
                    AttackType type = a switch
                    {
                        0 => AttackType.Kinetic,
                        1 => AttackType.Energy,
                        2 => AttackType.Explosive,
                        _ => throw new Exception(),
                    };

                    double attAvg = a switch { 0 => 13, 1 => 10.4, 2 => 7.8, _ => throw new Exception(), };
                    attAvg *= researchMult;
                    //dont pow researchMult first
                    //const double lowPenalty = 2.0;
                    //if (researchMult < lowPenalty)
                    //    attAvg *= researchMult / lowPenalty;

                    int attack = Game.Rand.Round(attAvg);
                    attacks[a] = new(type, attack, attacks[a].Range);
                }
            }
        }
    }
}
