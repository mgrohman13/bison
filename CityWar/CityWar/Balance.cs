using System;
using System.Collections.Generic;
using MattUtil;

namespace CityWar
{
    public static class Balance
    {
        private const double AirMoveDiv = 2.6;
        private const double AttackTargetsPower = .39;

        public static double GetCost(UnitTypes unitTypes, UnitType type, bool isThree, Abilities ability, int maxHits, int baseArmor, int baseRegen, int maxMove, Attack[] attacks)
        {
            double gc;
            return GetCost(unitTypes, type, isThree, ability, maxHits, baseArmor, baseRegen, maxMove, attacks, out gc);
        }
        public static double GetCost(UnitTypes unitTypes, UnitType type, bool isThree, Abilities ability, int maxHits, int baseArmor, int baseRegen, int maxMove, Attack[] attacks, out double gc)
        {
            double costMult = unitTypes.GetCostMult();
            double weaponDiv = GetWeaponDiv(unitTypes);

            if ((baseRegen < 1 || maxHits < 1) ||
                (type == UnitType.Immobile ? (maxMove > 0 || ability == Abilities.Aircraft || attacks.Length > 0 || isThree)
                : (maxMove < 1 || attacks.Length < 1)))
                throw new Exception();

            double move = maxMove;
            double regen = baseRegen;
            bool air = ability == Abilities.Aircraft;
            double armor = baseArmor;

            if (move > 0)
                regen *= move;
            else
                // 3 =div for immobile regen since it costs resources
                regen /= 3;

            double typeVal, addArmor, movMult;
            GetValues(type, out typeVal, out addArmor, out movMult);

            armor = ModArmor(armor, addArmor);
            move = ModMove(move, air, movMult);

            EnumFlags<TargetType>[] targets = new EnumFlags<TargetType>[] { null, null, null };
            double[] length = new double[] { 0, 0, 0 };
            double[] damage = new double[] { 0, 0, 0 };
            double[] divide = new double[] { 1, 1, 1 };

            if (type == UnitType.Immobile)
            {
                if (attacks.Length > 0 || move > 0)
                    throw new Exception();
            }
            else
                for (int i = 0; i < attacks.Length; ++i)
                {
                    targets[i] = attacks[i].Target;
                    length[i] = attacks[i].Length;
                    damage[i] = attacks[i].Damage;
                    divide[i] = attacks[i].Pierce;
                }

            //do the actual cost calculation
            return CaulculateCost(unitTypes, type, costMult, weaponDiv, typeVal, maxHits, armor, regen, move,
                targets[0], length[0], damage[0], divide[0],
                targets[1], length[1], damage[1], divide[1],
                targets[2], length[2], damage[2], divide[2],
                isThree, ability == Abilities.AircraftCarrier, air, type == UnitType.Immobile, out gc);
        }

        public static double GetArmor(UnitType type, double armor)
        {
            double typeVal, addArmor, movMult;
            GetValues(type, out typeVal, out addArmor, out movMult);
            return ModArmor(armor, addArmor);
        }

        public static double GetMove(UnitType type, double move, bool air)
        {
            double typeVal, addArmor, movMult;
            GetValues(type, out typeVal, out addArmor, out movMult);
            return ModMove(move, air, movMult);
        }

        public static void GetValues(UnitType type, out double typeVal, out double addArmor, out double movMult)
        {
            double terrArm, cityArm;
            switch (type)
            {
                //typeVal is based on map domain (but not strictly a percentage; raw multiplyer to cost)
                //terrArm and cityArm are based on armor bonuses
                //movMult on terrain move cost (but not too low; also effects attack length cost calculation)
                case UnitType.Air:
                case UnitType.Immobile:
                    typeVal = 1.0;
                    terrArm = 0.0;
                    cityArm = 1.0;
                    movMult = 1.0;
                    break;
                case UnitType.Ground:
                    typeVal = 0.9;
                    //expected to spend more time in defensive terrain
                    terrArm = 1.3;
                    cityArm = 1.0;
                    movMult = 0.7;
                    break;
                case UnitType.Amphibious:
                    //extra typeVal for ability to switch between ground and water targeted
                    typeVal = 1.1;
                    //compared to ground, +1 armor in water (for no move cost) vs. -1; considered a wash when average is +1.3
                    terrArm = 1.3;
                    cityArm = 1.0;
                    movMult = 0.8;
                    break;
                case UnitType.Water:
                    typeVal = 0.7;
                    terrArm = -.1;
                    //-.75% rounded up; expected to spend more time in water cities
                    cityArm = -.7;
                    movMult = 1.0;
                    break;
                default:
                    throw new Exception();
            }
            //addArmor starts with a value for wizard armor bonus
            addArmor = 0.3;
            // 3 ... / 4 =percent of time spent outside city
            addArmor += (3 * terrArm + cityArm) / 4;
        }

        private static double ModArmor(double armor, double add)
        {
            armor += add;
            return armor;
        }

        private static double ModMove(double move, bool air, double movMult)
        {
            if (air)
                move /= AirMoveDiv;
            move *= movMult;
            return move;
        }

        private static double CaulculateCost(UnitTypes unitTypes, UnitType type, double costMult, double weaponDiv, double unitType, double health, double armor, double regeneration, double movement,
            EnumFlags<TargetType> a1Type, double a1Length, double a1Damage, double a1Divide,
            EnumFlags<TargetType> a2Type, double a2Length, double a2Damage, double a2Divide,
            EnumFlags<TargetType> a3Type, double a3Length, double a3Damage, double a3Divide,
            bool isThree, bool carry, bool air, bool immobile, out double gc)
        {
            //hits
            double avgDmg = GetAverageDamage(unitTypes.GetAverageDamage(type), unitTypes.GetAverageAP(type), armor);
            double hitWorth = HitWorth(health, avgDmg);

            //damage
            double weapon1, weapon2, weapon3;
            if (immobile)
            {
                // 9.0 =immobile cost
                weapon1 = 9.0;
                weapon2 = 0;
                weapon3 = 0;
            }
            else
            {
                weapon1 = Weapon(unitTypes, type, a1Type, a1Damage, a1Divide, a1Length, movement, weaponDiv, air, isThree, 1);
                weapon2 = Weapon(unitTypes, type, a2Type, a2Damage, a2Divide, a2Length, movement, weaponDiv, air, isThree, 2);
                weapon3 = Weapon(unitTypes, type, a3Type, a3Damage, a3Divide, a3Length, movement, weaponDiv, air, isThree, 3);
            }

            if (immobile)
                gc = double.NaN;
            else
                gc = (hitWorth + Regen(unitTypes, regeneration, avgDmg)) / (weapon1 + weapon2 + weapon3) * unitTypes.GetAverageDamage(type);

            //total
            double result = Unit(unitTypes, hitWorth, regeneration, weapon1, weapon2, weapon3, avgDmg);
            result = Final(unitTypes, result, movement, unitType);

            //carrier
            result = ForCarry(unitTypes, result, carry, unitType, movement, hitWorth);

            //overall unit costs
            result *= costMult;

            if (!immobile)
            {
                // 10 =turns for a unit's regen to pay for itself
                result += 10 * regeneration / Player.WorkMult;
            }

            return result;
        }

        public static double HitWorth(UnitTypes unitTypes, UnitType type, double health, double armor)
        {
            double avgDmg = GetAverageDamage(unitTypes.GetAverageDamage(type), unitTypes.GetAverageAP(type), armor);
            return HitWorth(health, avgDmg);
        }
        public static double HitWorth(double health, double avgDmg)
        {
            return health / avgDmg;
        }

        public static double Weapon(UnitTypes unitTypes, UnitType? type, EnumFlags<TargetType> targets, double damage, double divide, double length, double move, bool air, bool isThree, int num)
        {
            return Weapon(unitTypes, type, targets, damage, divide, length, move, GetWeaponDiv(unitTypes), air, isThree, num);
        }
        public static double Weapon(UnitTypes unitTypes, UnitType? type, EnumFlags<TargetType> targets, double damage, double divide, double length, double move, double weaponDiv, bool air, bool isThree, int num)
        {
            double result = 0;
            if (damage > 0)
            {
                //damage
                result = GetAverageDamage(damage, divide, unitTypes.GetAverageArmor(targets));
                //length
                //TODO: schema-based length handing
                double MaxLength = unitTypes.GetMaxLength();
                result *= Math.Pow(move * (air ? AirMoveDiv : 1) + unitTypes.GetAverageMove(), length / MaxLength)
                    / Math.Pow(unitTypes.GetAverageMove(), length / MaxLength);
                //target
                double count = targets == null ? unitTypes.GetAverageDamageType() : targets.Count;
                result *= Math.Pow(count, AttackTargetsPower);
                //isThree
                result *= IsThreeMult(isThree, num);
            }
            return result / weaponDiv;
        }
        public static double IsThreeMult(bool isThree, int num)
        {
            if (isThree && num > 1)
                // 2.3  ... / 3 =percent of time unit has hp>1/3
                // 1.69 ... / 3 =percent of time unit has hp>2/3
                return (num == 2 ? 2.3 : 1.69) / 3;
            return 1;
        }

        private static double Unit(UnitTypes unitTypes, double hits, double regeneration, double a1Worth, double a2Worth, double a3Worth, double avgDmg)
        {
            return Math.Sqrt(hits * (a1Worth + a2Worth + a3Worth + Regen(unitTypes, regeneration, avgDmg)));
        }

        private static double Regen(UnitTypes unitTypes, double regeneration, double avgDmg)
        {
            //TODO: remove AverageDamage?, rebalance
            // 3.9 =div for surviving attacks versus having attacks
            return regeneration / avgDmg * unitTypes.GetAverageDamage() / 3.9;
        }

        private static double Final(UnitTypes unitTypes, double unit, double move, double type)
        {
            double result = 0;
            result = (unit) * (move + unitTypes.GetAverageMove()) * type;
            return result;
        }

        private static double ForCarry(UnitTypes unitTypes, double cost, bool carry, double typeWorth, double move, double hitWorth)
        {
            if (carry)
            {
                double factor = 3.9 + 9 * move / unitTypes.GetAverageMove() + hitWorth / 9;
                factor *= typeWorth / 30;
                factor += 1;

                cost += Math.Sqrt(cost);
                cost *= factor;
            }

            return cost;
        }

        public static double GetAverageDamage(double damage, double divide, double armor)
        {
            return Attack.GetAverageDamage(damage, divide, armor, int.MaxValue);
        }

        private static double GetWeaponDiv(UnitTypes unitTypes)
        {
            double averageDamage = unitTypes.GetAverageDamage();
            return Weapon(unitTypes, null, null, averageDamage, unitTypes.GetAverageAP(null), unitTypes.GetAverageLength(), unitTypes.GetAverageMove(), 1, false, false, -1) / averageDamage;
        }
    }
}
