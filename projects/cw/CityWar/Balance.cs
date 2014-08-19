using System;
using System.Collections.Generic;

namespace CityWar
{
    public static class Balance
    {
        public const double AverageArmor = 5.0, AverageAP = 2.0, AverageDamage = 5.5,
            AverageDamageType = 2.0, AverageLength = 2.0, AverageMove = 2.1, MaxLength = 5;

        private const double AirMoveDiv = 2.6;
        private const double AttackTargetsPower = .39;

        private static double weaponDiv = GetWeaponDiv();

        public static double getCost(UnitTypes unitTypes, int MaxMove, int BaseRegen, Abilities Ability, int BaseArmor, UnitType Type, Attack[] Attacks, bool isThree, int maxHits)
        {
            double gc;
            return getCost(unitTypes, MaxMove, BaseRegen, Ability, BaseArmor, Type, Attacks, isThree, maxHits, out gc);
        }
        public static double getCost(UnitTypes unitTypes, int MaxMove, int BaseRegen, Abilities Ability, int BaseArmor, UnitType Type, Attack[] Attacks, bool isThree, int maxHits, out double gc)
        {
            double costMult = getCostMult(unitTypes);
            return getCost(MaxMove, BaseRegen, Ability, BaseArmor, Type, Attacks, isThree, maxHits, costMult, out gc);
        }
        public static double getCost(int MaxMove, int BaseRegen, Abilities Ability, int BaseArmor, UnitType Type, Attack[] Attacks, bool isThree, int maxHits, double costMult)
        {
            double gc;
            return getCost(MaxMove, BaseRegen, Ability, BaseArmor, Type, Attacks, isThree, maxHits, costMult, out gc);
        }
        public static double getCost(int MaxMove, int BaseRegen, Abilities Ability, int BaseArmor, UnitType Type, Attack[] Attacks, bool isThree, int maxHits, double costMult, out double gc)
        {
            if (( BaseRegen < 1 || maxHits < 1 ) ||
                ( Type == UnitType.Immobile ? ( MaxMove > 0 || Ability == Abilities.Aircraft || Attacks.Length > 0 || isThree )
                : ( MaxMove < 1 || Attacks.Length < 1 ) ))
                throw new Exception();

            double move = MaxMove;
            double regen = BaseRegen;
            bool air = Ability == Abilities.Aircraft;
            double armor = BaseArmor;

            if (move > 0)
                regen *= move;
            else
                // 3 =div for immobile regen since it costs resources
                regen /= 3;

            double typeVal, addArmor, movMult;
            getValues(Type, out typeVal, out addArmor, out movMult);

            armor = modArmor(armor, addArmor);
            move = modMove(move, air, movMult);

            double[] type = new double[] { 0, 0, 0 };
            double[] length = new double[] { 0, 0, 0 };
            double[] damage = new double[] { 0, 0, 0 };
            double[] divide = new double[] { 1, 1, 1 };

            if (Type == UnitType.Immobile)
            {
                if (Attacks.Length > 0 || move > 0)
                    throw new Exception();
            }
            else
                for (int i = 0 ; i < ( isThree ? 3 : Attacks.Length ) ; ++i)
                {
                    int attack = isThree ? 0 : i;
                    type[i] = Attacks[attack].Target.Count;
                    length[i] = Attacks[attack].Length;
                    damage[i] = Attacks[attack].Damage;
                    divide[i] = Attacks[attack].Pierce;
                }

            //do the actual cost calculation
            return caulculateCost(costMult, typeVal, maxHits, armor, regen, move,
                type[0], length[0], damage[0], divide[0],
                type[1], length[1], damage[1], divide[1],
                type[2], length[2], damage[2], divide[2],
                isThree, Ability == Abilities.AircraftCarrier, air, Type == UnitType.Immobile, out gc);
        }

        public static double getArmor(UnitType Type, double armor)
        {
            double typeVal, addArmor, movMult;
            getValues(Type, out typeVal, out addArmor, out movMult);
            return modArmor(armor, addArmor);
        }

        public static double getMove(UnitType Type, double move, bool air)
        {
            double typeVal, addArmor, movMult;
            getValues(Type, out typeVal, out addArmor, out movMult);
            return modMove(move, air, movMult);
        }

        private static void getValues(UnitType Type, out double typeVal, out double addArmor, out double movMult)
        {
            double terrArm, cityArm;
            switch (Type)
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
            addArmor += ( 3 * terrArm + cityArm ) / 4;
        }

        private static double getCostMult(UnitTypes unitTypes)
        {
            UnitSchema schema = unitTypes.GetSchema();
            return (double)schema.CostMult.Rows[0][0];
        }

        private static double modArmor(double armor, double add)
        {
            armor += add;
            return armor;
        }

        private static double modMove(double move, bool air, double movMult)
        {
            if (air)
                move /= AirMoveDiv;
            move *= movMult;
            return move;
        }

        private static double caulculateCost(double costMult, double unitType, double health, double armor, double regeneration, double movement,
            double a1Type, double a1Length, double a1Damage, double a1Divide,
            double a2Type, double a2Length, double a2Damage, double a2Divide,
            double a3Type, double a3Length, double a3Damage, double a3Divide,
            bool isThree, bool carry, bool air, bool immobile, out double gc)
        {
            double avgDmg = GetAverageDamage(AverageDamage, AverageAP, armor);

            //hits
            double hitWorth = hit(health, avgDmg);

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
                weapon1 = weapon(a1Type, a1Damage, a1Divide, a1Length, movement, air, isThree, 1);
                weapon2 = weapon(a2Type, a2Damage, a2Divide, a2Length, movement, air, isThree, 2);
                weapon3 = weapon(a3Type, a3Damage, a3Divide, a3Length, movement, air, isThree, 3);
            }

            if (immobile)
                gc = double.NaN;
            else
                gc = ( hitWorth + regen(regeneration, avgDmg) ) / ( weapon1 + weapon2 + weapon3 ) * AverageDamage;

            //total
            double result = unit(hitWorth, regeneration, weapon1, weapon2, weapon3, avgDmg);
            result = final(result, movement, unitType);

            //carrier
            result = forCarry(result, carry, unitType, movement, hitWorth);

            //overall unit costs
            result *= costMult;

            if (!immobile)
            {
                // 10 =turns for a unit's regen to pay for itself
                result += 10 * regeneration / Player.WorkMult;
            }

            return result;
        }

        public static double hitWorth(double hits, double armor)
        {
            return hit(hits, GetAverageDamage(AverageDamage, AverageAP, armor));
        }

        private static double hit(double hits, double avgDmg)
        {
            return hits / avgDmg;
        }

        public static double weapon(double type, double damage, double divide, double length, double move, bool air, bool isThree, int num)
        {
            return weapon(type, damage, divide, length, move, weaponDiv, air, isThree, num);
        }

        private static double weapon(double type, double damage, double divide, double length, double move, double weaponDiv, bool air, bool isThree, int num)
        {
            double result = 0;
            if (damage > 0)
            {
                //damage
                result = GetAverageDamage(damage, divide, AverageArmor);
                //length
                result *= Math.Pow(move * ( air ? AirMoveDiv : 1 ) + AverageMove, length / MaxLength)
                    / Math.Pow(AverageMove, length / MaxLength);
                //target
                result *= Math.Pow(type, AttackTargetsPower);
                //isThree
                if (isThree && num > 1)
                    // 2.3  ... / 3 =percent of time unit has hp>1/3
                    // 1.69 ... / 3 =percent of time unit has hp>2/3
                    result *= ( num == 2 ? 2.3 : 1.69 ) / 3;
            }
            return result / weaponDiv;
        }

        private static double unit(double hits, double regeneration, double a1Worth, double a2Worth, double a3Worth, double avgDmg)
        {
            // 3.9 =div for surviving attacks versus having attacks
            return Math.Sqrt(hits * ( a1Worth + a2Worth + a3Worth + regen(regeneration, avgDmg) ));
        }

        private static double regen(double regeneration, double avgDmg)
        {
            return regeneration / avgDmg * AverageDamage / 3.9;
        }

        private static double final(double unit, double move, double type)
        {
            double result = 0;
            result = ( unit ) * ( move + AverageMove ) * type;
            return result;
        }

        private static double forCarry(double cost, bool carry, double typeWorth, double move, double hitWorth)
        {
            if (carry)
            {
                double factor = 3.9 + 9 * move / AverageMove + hitWorth / 9;
                factor *= typeWorth / 30;
                factor += 1;

                cost += Math.Sqrt(cost);
                cost *= factor;
            }

            return cost;
        }

        private static double GetAverageDamage(double damage, double divide, double armor)
        {
            return Attack.GetAverageDamage(damage, divide, armor, int.MaxValue);
        }

        internal static void ResetWeapon()
        {
            weaponDiv = GetWeaponDiv();
        }

        internal static double GetWeaponDiv()
        {
            return weapon(AverageDamageType, AverageDamage, AverageAP, AverageLength, AverageMove, 1, false, false, -1) / AverageDamage;
        }
    }
}
