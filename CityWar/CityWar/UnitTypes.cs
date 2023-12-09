using MattUtil;
using System;
using System.Linq;

namespace CityWar
{
    public class UnitTypes
    {
        private readonly UnitSchema schema;

        public UnitTypes()
        {
            schema = new UnitSchema();
            schema.ReadXml(Game.ResourcePath + "Units.xml");
        }

        public UnitSchema GetSchema()
        {
            return schema;
        }

        public void SetCostMult(double costMult)
        {
            schema.Balance.Clear();
            schema.Balance.AddBalanceRow(costMult);
        }
        public double GetCostMult()
        {
            return ((UnitSchema.BalanceRow)schema.Balance.Rows[0]).CostMult;
        }

        public static UnitType GetType(string typeStr)
        {
            return typeStr switch
            {
                "W" => UnitType.Water,
                "G" => UnitType.Ground,
                "A" => UnitType.Air,
                "GW" => UnitType.Amphibious,
                "GWA" => UnitType.Immobile,
                _ => throw new Exception(),
            };
        }
        public static EnumFlags<TargetType> GetAttackTargets(string targetType)
        {
            EnumFlags<TargetType> targets = new();
            if (targetType.Contains("G"))
                targets.Add(TargetType.Ground);
            if (targetType.Contains("W"))
                targets.Add(TargetType.Water);
            if (targetType.Contains("A"))
                targets.Add(TargetType.Air);
            return targets;
        }

        public static Attack.SpecialType GetAttackSpecial(string special)
        {
            return special switch
            {
                "S" => Attack.SpecialType.Splash,
                _ => Attack.SpecialType.None,
            };
        }

        internal double GetAverageCost()
        {
            return schema.Unit.Average(u => u.Cost + u.People);
        }

        public double GetAverageArmor()
        {
            return GetAverageArmor(null, null);
        }
        public double GetAverageArmor(string race, EnumFlags<TargetType> targets)
        {
            double armor = 0, count = 0;
            CheckUnits(race, targets, (weight, unit) =>
            {
                Balance.GetValues(GetType(unit.Type), out _, out double addArmor, out _);
                armor += (unit.Armor + addArmor) * weight;
                count += weight;
            });
            return armor / count;
        }

        public double GetAverageMove()
        {
            double move = 0, count = 0;
            CheckUnits(null, null, (weight, unit) =>
            {
                move += unit.Move * weight;
                count += weight;
            });
            return move / count;
        }

        public double GetAverageTargets()
        {
            double type = 0, count = 0;
            CheckUnits(null, null, (weight, unit) =>
            {
                type += unit.GetAttackRows().Length * weight;
                count += weight;
            });
            return type / count;
        }

        public double GetAverageDamage()
        {
            double damage = 0, count = 0;
            CheckAttacks(null, null, (weight, attack) =>
            {
                damage += weight * attack.Damage;
                count += weight;
            });
            return damage / count;
        }

        public double GetAverageDamage(string race, UnitType type, double armor, int shield, EnumFlags<Ability> abilities)
        {
            double damage = 0, count = 0;
            CheckAttacks(race, type, (weight, attack) =>
            {
                if (!abilities.Contains(Ability.Shield))
                    shield = GetSubmergedShield(GetType(attack.UnitRow.Type), type, abilities);
                damage += weight * Attack.GetAverageDamage(attack.Damage, attack.Divide_By, armor, shield, int.MaxValue);
                count += weight;
            });
            return damage / count;
        }
        public double GetAverageDamage(string race, UnitType? type, EnumFlags<TargetType> targets, double damage, double divide)
        {
            double tot = 0, count = 0;
            CheckUnits(race, targets, (weight, unit) =>
            {
                int submerge = type.HasValue ? GetSubmergedShield(type.Value, GetType(unit.Type), Unit.GetAbilities(unit, out _, out _)) : 0;
                Balance.GetValues(GetType(unit.Type), out _, out double addArmor, out _);
                tot += weight * Attack.GetAverageDamage(damage, divide, unit.Armor + addArmor, submerge, int.MaxValue);
                count += weight;
            });
            return tot / count;
        }
        private int GetSubmergedShield(UnitType attackerType, UnitType defenderType, EnumFlags<Ability> defenderAbilities)
        {
            if (Unit.IsSubmerged(attackerType, defenderAbilities))
                return (defenderType == UnitType.Amphibious ? 15 : 45);
            return 0;
        }

        internal double GetAverageRegen()
        {
            double regen = 0, count = 0;
            CheckUnits(null, null, (weight, unit) =>
            {
                regen += weight * Balance.ModRegen(Unit.GetAbilities(unit, out _, out _), unit.Move, unit.Regen);
                count += weight;
            });
            return regen / count;
        }

        public double GetAverageAP()
        {
            return GetAverageAP(null, null);
        }
        public double GetAverageAP(string race, UnitType? type)
        {
            double pierce = 0, count = 0;
            CheckAttacks(race, type, (weight, attack) =>
            {
                pierce += weight * attack.Divide_By;
                count += weight;
            });
            return pierce / count;
        }

        public double GetAverageLength()
        {
            double length = 0, count = 0;
            CheckAttacks(null, null, (weight, attack) =>
            {
                length += weight * attack.Length;
                count += weight;
            });
            return length / count;

        }
        public double GetLengthPct(string race, UnitType? type, double length)
        {
            double low = GetLengthPct(race, type, (int)length);
            if ((int)length == length)
                return low;
            double high = GetLengthPct(race, type, (int)length + 1);
            double offset = length - (int)length;
            return low * (1 - offset) + high * offset;
        }
        public double GetLengthPct(string race, UnitType? type, int length)
        {
            double pct = 0, count = 0;
            CheckAttacks(race, type, (weight, attack) =>
            {
                double add = 0;
                if (attack.Length < length)
                    add = 1;
                else if (attack.Length == length)
                    add = .5;
                pct += weight * add;
                count += weight;
            });
            return pct / count;
        }

        private void CheckUnits(string race, EnumFlags<TargetType> targets, Action<double, UnitSchema.UnitRow> Callback)
        {
            foreach (UnitSchema.UnitRow unit in schema.Unit)
            {
                UnitType unitType = GetType(unit.Type);
                double weight = 1;
                if (Target(race, unit, unitType, targets, ref weight))
                    Callback(weight, unit);
            }
        }
        private void CheckAttacks(string race, UnitType? type, Action<double, UnitSchema.AttackRow> Callback)
        {
            foreach (UnitSchema.UnitRow unit in schema.Unit)
            {
                UnitSchema.AttackRow[] attacks = unit.GetAttackRows();
                for (int attNum = 0; attNum < attacks.Length; attNum++)
                {
                    UnitSchema.AttackRow attack = attacks[attNum];
                    EnumFlags<TargetType> targets = GetAttackTargets(attack.Target_Type);
                    double weight = Balance.IsThreeMult(unit.IsThree, attNum + 1);
                    if (Target(race, unit, type, targets, ref weight))
                        Callback(weight, attack);
                }
            }
        }
        private static bool Target(string race, UnitSchema.UnitRow unit, UnitType? unitType, EnumFlags<TargetType> targets, ref double weight)
        {
            if (race == unit.Race)
                weight /= 2;
            if (!unitType.HasValue || targets == null)
                return true;
            switch (unitType)
            {
                case UnitType.Air:
                    return targets.Contains(TargetType.Air);
                case UnitType.Ground:
                    return targets.Contains(TargetType.Ground);
                case UnitType.Water:
                    return targets.Contains(TargetType.Water);
                case UnitType.Amphibious:
                    double g = targets.Contains(TargetType.Ground) ? 1 : 0;
                    double w = targets.Contains(TargetType.Water) ? 1 : 0;
                    weight *= (g + w) / 2.0;
                    return (weight > 0);
                case UnitType.Immobile:
                    return true;
                default:
                    throw new Exception();
            }
        }
    }
}
