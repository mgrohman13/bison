using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace CityWar
{
    [Serializable]
    public class UnitTypes
    {
        private UnitSchema schema;
        private Dictionary<UnitType, UnitTypeCalc> unitTypes;

        public UnitTypes()
        {
            schema = new UnitSchema();
            schema.ReadXml(Game.ResourcePath + "Units.xml");
            CalculateTypes();
        }

        public UnitSchema GetSchema()
        {
            return schema;
        }

        public void CalculateTypes()
        {
            unitTypes = new Dictionary<UnitType, UnitTypeCalc>();
            foreach (UnitSchema.UnitRow row in schema.Unit)
            {
                UnitType type = GetType(row.Type);
                if (!unitTypes.ContainsKey(type))
                    unitTypes.Add(type, new UnitTypeCalc());
                UnitTypeCalc calc = unitTypes[type];

                calc.armor += row.Armor;
                calc.move += row.Move;
                calc.types += row.GetAttackRows().Length;
                calc.count++;

                for (int a = 0; a < row.GetAttackRows().Length; a++)
                {
                    UnitSchema.AttackRow att = row.GetAttackRows()[a];
                    EnumFlags<TargetType> targets = GetAttackTargets(att.Target_Type);
                    double weight = (1.0 / targets.Count);
                    weight *= Balance.IsThreeMult(row.IsThree, a + 1);
                    foreach (TargetType targType in targets)
                    {
                        if (!calc.targetTypes.ContainsKey(targType))
                            calc.targetTypes.Add(targType, new TargetTypeCalc());
                        TargetTypeCalc c = calc.targetTypes[targType];

                        c.damage += att.Damage * weight;
                        c.length += att.Length * weight;
                        c.pierce += att.Divide_By * weight;
                        c.count += weight;
                    }
                }
            }

            foreach (UnitTypeCalc u in unitTypes.Values)
            {
                u.armor /= u.count;
                u.move /= u.count;
                u.types /= u.count;
                foreach (TargetTypeCalc t in u.targetTypes.Values)
                {
                    t.damage /= t.count;
                    t.length /= t.count;
                    t.pierce /= t.count;
                }
            }
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

        public static CityWar.UnitType GetType(String typeStr)
        {
            switch (typeStr)
            {
                case "W":
                    return CityWar.UnitType.Water;
                case "G":
                    return CityWar.UnitType.Ground;
                case "A":
                    return CityWar.UnitType.Air;
                case "GW":
                    return CityWar.UnitType.Amphibious;
                case "GWA":
                    return CityWar.UnitType.Immobile;
                default:
                    throw new Exception();
            }
        }
        public static EnumFlags<TargetType> GetAttackTargets(String targetType)
        {
            EnumFlags<TargetType> targets = new EnumFlags<TargetType>();
            if (targetType.Contains("G"))
                targets.Add(TargetType.Ground);
            if (targetType.Contains("W"))
                targets.Add(TargetType.Water);
            if (targetType.Contains("A"))
                targets.Add(TargetType.Air);
            return targets;
        }
        public double GetAverageArmor(EnumFlags<TargetType> targets)
        {
            double armor = 0, count = 0;
            foreach (KeyValuePair<UnitType, UnitTypeCalc> calc in unitTypes)
            {
                double weight = 1;
                if (targets == null || Target(calc.Key, targets, out weight))
                {
                    double typeVal, addArmor, movMult;
                    Balance.GetValues(calc.Key, out typeVal, out addArmor, out movMult);
                    weight *= calc.Value.count;
                    armor += (calc.Value.armor + addArmor) * weight;
                    count += weight;
                }
            }
            return armor / count;
        }
        public double GetAverageAP(UnitType? type)
        {
            double pierce = 0, count = 0;
            foreach (KeyValuePair<UnitType, UnitTypeCalc> t in unitTypes)
                foreach (KeyValuePair<TargetType, TargetTypeCalc> calc in t.Value.targetTypes)
                {
                    double weight = 1;
                    if (!type.HasValue || Target(type.Value, new EnumFlags<TargetType>(calc.Key), out weight))
                    {
                        weight *= calc.Value.count;
                        pierce += calc.Value.pierce * weight;
                        count += weight;
                    }
                }
            return pierce / count;
        }
        public double GetAverageDamage()
        {
            return GetAverageDamage(null);
        }
        public double GetAverageDamage(UnitType? type)
        {
            double damage = 0, count = 0;
            foreach (KeyValuePair<UnitType, UnitTypeCalc> t in unitTypes)
                foreach (KeyValuePair<TargetType, TargetTypeCalc> calc in t.Value.targetTypes)
                {
                    double weight = 1;
                    if (!type.HasValue || Target(type.Value, new EnumFlags<TargetType>(calc.Key), out weight))
                    {
                        weight *= calc.Value.count;
                        damage += calc.Value.damage * weight;
                        count += weight;
                    }
                }
            return damage / count;
        }
        public double GetAverageLength()
        {
            double length = 0, count = 0;
            foreach (KeyValuePair<UnitType, UnitTypeCalc> t in unitTypes)
                foreach (KeyValuePair<TargetType, TargetTypeCalc> calc in t.Value.targetTypes)
                {
                    double weight = calc.Value.count;
                    length += calc.Value.length * weight;
                    count += weight;
                }
            return length / count;
        }
        public double GetAverageMove()
        {
            double move = 0, count = 0;
            foreach (KeyValuePair<UnitType, UnitTypeCalc> calc in unitTypes)
            {
                double weight = calc.Value.count;
                move += calc.Value.move * weight;
                count += weight;
            }
            return move / count;
        }
        public double GetAverageDamageType()
        {
            double type = 0, count = 0;
            foreach (KeyValuePair<UnitType, UnitTypeCalc> calc in unitTypes)
            {
                double weight = calc.Value.count;
                type += calc.Value.types * weight;
                count += weight;
            }
            return type / count;
        }
        public double GetMaxLength()
        {
            return schema.Attack.Max(a => a.Length);
        }

        private static bool Target(UnitType type, EnumFlags<TargetType> targets, out double weight)
        {
            weight = 1;
            switch (type)
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
                    weight = (g + w) / 2.0;
                    return (weight > 0);
                case UnitType.Immobile:
                    return true;
                default:
                    throw new Exception();
            }
        }

        private class UnitTypeCalc
        {
            public Dictionary<TargetType, TargetTypeCalc> targetTypes = new Dictionary<TargetType, TargetTypeCalc>();
            public double move = 0, armor = 0, types = 0, count = 0;
        }
        private class TargetTypeCalc
        {
            public double damage = 0, pierce = 0, length = 0, count = 0;
        }
    }
}
