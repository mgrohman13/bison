using System;
using System.Collections.Generic;
using MattUtil;

namespace CityWar
{
    public static class Balance
    {
        private const double AttackTargetsPower = .39;

        public static double GetCost(UnitTypes unitTypes, string race, UnitType type, bool isThree, EnumFlags<Ability> abilities, int shield, int fuel, int maxHits, int baseArmor, int baseRegen, int maxMove, Attack[] attacks, out double gc)
        {
            double costMult = unitTypes.GetCostMult();
            double weaponDiv = GetWeaponDiv(unitTypes);

            bool air = abilities.Contains(Ability.Aircraft);
            if (baseRegen < 1 || maxHits < 1)
                throw new Exception();
            if (air && abilities.Contains(Ability.AircraftCarrier))
                throw new Exception();
            if (abilities.Contains(Ability.Shield) ? shield <= 0 || shield >= 100 : shield != 0)
                throw new Exception();
            if (abilities.Contains(Ability.Shield) && abilities.Contains(Ability.Submerged))
                throw new Exception();
            if (type == UnitType.Immobile ? maxMove > 0 || air || attacks.Length > 0 || isThree
                    : maxMove < 1 || attacks.Length < 1)
                throw new Exception();

            double move = maxMove;
            double regen = baseRegen;
            double armor = baseArmor;

            regen = ModRegen(abilities, move, regen);

            GetValues(type, out double typeVal, out double addArmor, out double movMult);

            armor = ModArmor(armor, addArmor);
            move = ModMove(move, air, fuel, movMult);

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
            return CaulculateCost(unitTypes, race, type, costMult, weaponDiv, typeVal, maxHits, armor, regen, move, baseRegen, maxMove,
                targets[0], length[0], damage[0], divide[0],
                targets[1], length[1], damage[1], divide[1],
                targets[2], length[2], damage[2], divide[2],
                isThree, abilities, fuel, shield, type == UnitType.Immobile, out gc);
        }

        public static double ModRegen(EnumFlags<Ability> abilities, double move, double regen)
        {
            if (move > 0)
                regen *= move + (abilities.Contains(Ability.Regen) ? 1.3 : 0);
            else
                // 3 =div for immobile regen since it costs resources
                regen /= 3;
            if (abilities.Contains(Ability.Aircraft))
                // aircraft can only heal at a carrier
                regen /= 1.69;
            return regen;
        }

        public static double GetArmor(UnitType type, double armor)
        {
            GetValues(type, out _, out double addArmor, out _);
            return ModArmor(armor, addArmor);
        }

        public static double GetMove(UnitType type, double move, bool air, int fuel)
        {
            GetValues(type, out _, out _, out double movMult);
            return ModMove(move, air, fuel, movMult);
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
                    //compared to ground, +1 armor in water (for no move cost) vs. -1 
                    terrArm = 1.2;
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

        private static double ModMove(double move, bool air, int fuel, double movMult)
        {
            move *= movMult;
            if (air)
                move /= GetAirMoveDiv(move, fuel);
            return move;
        }

        private static double GetAirMoveDiv(double move, int fuel)
        {
            return 1 + 10.4 * (move + 1) / (move + 1 + Math.Pow(fuel, 1.69) / 2);
        }

        private static double CaulculateCost(UnitTypes unitTypes, string race, UnitType type, double costMult, double weaponDiv, double unitType, double health, double armor, double regeneration, double movement, int baseRegen, int maxMove,
            EnumFlags<TargetType> a1Type, double a1Length, double a1Damage, double a1Divide,
            EnumFlags<TargetType> a2Type, double a2Length, double a2Damage, double a2Divide,
            EnumFlags<TargetType> a3Type, double a3Length, double a3Damage, double a3Divide,
            bool isThree, EnumFlags<Ability> abilities, int fuel, int shield, bool immobile, out double gc)
        {
            bool carry = abilities.Contains(Ability.AircraftCarrier);
            bool air = abilities.Contains(Ability.Aircraft);

            //hits
            double avgDmg = unitTypes.GetAverageDamage(race, type, armor, shield, abilities);
            double hitWorth = HitWorth(health, avgDmg);

            //damage
            double weapon1, weapon2, weapon3;
            if (immobile)
            {
                // 7.8 =immobile cost
                weapon1 = 7.8;
                weapon2 = 0;
                weapon3 = 0;
            }
            else
            {
                weapon1 = Weapon(unitTypes, race, type, a1Type, a1Damage, a1Divide, a1Length, movement, weaponDiv, air, fuel, isThree, 1);
                weapon2 = Weapon(unitTypes, race, type, a2Type, a2Damage, a2Divide, a2Length, movement, weaponDiv, air, fuel, isThree, 2);
                weapon3 = Weapon(unitTypes, race, type, a3Type, a3Damage, a3Divide, a3Length, movement, weaponDiv, air, fuel, isThree, 3);
            }

            if (immobile)
                gc = double.NaN;
            else
                gc = HitWorth(unitTypes, hitWorth, regeneration, avgDmg) / (weapon1 + weapon2 + weapon3);

            //total
            double result = Unit(unitTypes, hitWorth, regeneration, weapon1, weapon2, weapon3, avgDmg);
            result = Final(unitTypes, result, movement, unitType);

            //carrier
            result = ForCarry(unitTypes, result, carry, unitType, movement, hitWorth);

            //overall unit costs
            result *= costMult;

            if (!immobile)
                // 7.8 =turns for a unit's regen to pay for itself
                result += 7.8 * baseRegen * (maxMove + (abilities.Contains(Ability.Regen) ? 1 : 0)) / Player.WorkMult;

            return result;
        }

        public static double HitWorth(UnitTypes unitTypes, double hitWorth, double regeneration, double avgDmg)
        {
            double fullAvgDmg = unitTypes.GetAverageDamage();
            double mult = Attack.GetAverageDamage(fullAvgDmg, unitTypes.GetAverageAP(), unitTypes.GetAverageArmor(), 0, int.MaxValue);
            return hitWorth * (3 * fullAvgDmg + Regen(unitTypes, regeneration, avgDmg)) / (3 * fullAvgDmg + unitTypes.GetAverageRegen()) * mult;
        }

        public static double HitWorth(double health, double avgDmg)
        {
            return health / avgDmg;
        }

        public static double Weapon(UnitTypes unitTypes, string race, UnitType type, EnumFlags<TargetType> targets, double damage, double divide, double length, double move, bool air, int fuel, bool isThree, int num)
        {
            return Weapon(unitTypes, race, type, targets, damage, divide, length, move, GetWeaponDiv(unitTypes), air, fuel, isThree, num);
        }
        public static double Weapon(UnitTypes unitTypes, string race, UnitType? type, EnumFlags<TargetType> targets, double damage, double divide, double length, double move, double weaponDiv, bool air, int fuel, bool isThree, int num)
        {
            double result = 0;
            if (damage > 0)
            {
                //damage
                result = unitTypes.GetAverageDamage(race, type, targets, damage, divide);
                //length
                double pct = unitTypes.GetLengthPct(race, type, length);
                result *= Math.Pow(move * (air ? Math.Sqrt(GetAirMoveDiv(move, fuel)) : 1) + unitTypes.GetAverageMove(), pct)
                    / Math.Pow(unitTypes.GetAverageMove(), pct);
                //target
                double count = targets == null ? unitTypes.GetAverageTargets() : targets.Count;
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

        public static double Regen(UnitTypes unitTypes, double regeneration, double avgDmg)
        {
            // 3.9 =div for surviving attacks versus having attacks
            return regeneration / avgDmg * unitTypes.GetAverageDamage() / 3.9;
        }

        private static double Final(UnitTypes unitTypes, double unit, double move, double type)
        {
            return (unit) * (move + unitTypes.GetAverageMove()) * type;
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

        private static double GetWeaponDiv(UnitTypes unitTypes)
        {
            double averageDamage = unitTypes.GetAverageDamage();
            return Weapon(unitTypes, null, null, null, averageDamage, unitTypes.GetAverageAP(), unitTypes.GetAverageLength(), unitTypes.GetAverageMove(), 1, false, int.MaxValue, false, -1) / averageDamage;
        }
    }
}
