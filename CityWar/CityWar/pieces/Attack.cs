using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MattUtil;

namespace CityWar
{
    [Serializable]
    public partial class Attack
    {
        #region fields and constructors

        //changing requires rebalance of units
        private const double DamMultPercent = .39;

        //unit cost : death when killed
        public const double DeathDivide = 7.8;
        //unit cost : death for disband
        internal const double DisbandDivide = 3.9;
        //unit cost : relic for wounding
        internal const double RelicDivide = 5.2;

        //percentage of unused attacks that adds to work
        internal const double OverkillPercent = 1 / 1.3;

        //only used during a battle
        [NonSerialized]
        private bool used;

        public readonly string Name;
        public readonly EnumFlags<TargetType> Target;
        public readonly int Length, Pierce;

        private Unit owner;
        private int damage;
        private bool randed;

        //balance constructor
        public Attack(EnumFlags<TargetType> target, int length, int damage, int divide)
            : this(null, target, length, damage, divide)
        {
        }

        //in-game constructor
        internal Attack(string name, EnumFlags<TargetType> target, int length, int damage, int pierce)
        {
            this.Name = name;
            this.Target = target;
            this.Length = length;
            this.Pierce = pierce;

            this.owner = null;
            this.damage = damage;
            this.randed = false;
        }

        #endregion //fields and constructors

        #region public methods and properties

        public Unit Owner
        {
            get
            {
                return owner;
            }
        }

        public bool Used
        {
            get
            {
                return used;
            }
            internal set
            {
                used = value;
            }
        }

        public int Damage
        {
            get
            {
                return damage;
            }
        }

        public bool CanAttack(Unit u)
        {
            return CanAttack(u, u.Length);
        }
        public bool CanAttack(Unit u, int length)
        {
            if (this.Used || this.Length < length)
                return false;

            //Immobile units protect on defense only
            if (u.Owner != u.Owner.Game.CurrentPlayer && u.Type != UnitType.Immobile)
                foreach (Unit u2 in u.Tile.GetAllUnits())
                    if (u2.Type == UnitType.Immobile)
                        return false;

            return CanTarget(u);
        }
        internal bool CanTarget(Unit u)
        {
            if (owner.Owner == u.Owner || !owner.Tile.IsNeighbor(u.Tile) || owner.Dead || u.Dead)
                return false;

            if (u.Type == UnitType.Immobile)
                return true;

            TargetType enemy;
            if (u.Type == UnitType.Air)
                enemy = TargetType.Air;
            else if (u.Type == UnitType.Ground)
                enemy = TargetType.Ground;
            else if (u.Type == UnitType.Water)
                enemy = TargetType.Water;
            else if (u.Type == UnitType.Amphibious)
                if (u.Tile.Terrain == Terrain.Water)
                    enemy = TargetType.Water;
                else
                    enemy = TargetType.Ground;
            else
                throw new Exception();

            return Target.Contains(enemy);
        }

        public int GetMinDamage(Unit target)
        {
            return GetMinDamage(damage, Pierce, target.Armor, target.Shield);
        }
        public static int GetMinDamage(double damage, double divide, double armor, int shield)
        {
            int minDamage = (int)((damage * (1 - DamMultPercent) - armor / divide) * getShieldMult(shield));
            return (minDamage > 0 ? minDamage : 0);
        }

        public double GetAverageDamage(Unit enemy, out double killPct, out double avgRelic)
        {
            double averageDamage = DamageThrows(damage, Pierce, enemy.Armor, enemy.Shield, enemy.hits, out killPct, out avgRelic, true);
            killPct *= 100;
            avgRelic *= enemy.RandedCost / RelicDivide / enemy.MaxHits;
            return averageDamage;
        }

        public static double GetAverageDamage(double damage, double divide, double targetArmor, int shield, int targetHits)
        {
            return GetAverageDamage(damage, divide, targetArmor, shield, targetHits, out _);
        }
        public static double GetAverageDamage(double damage, double divide, double targetArmor, int shield, int targetHits, out double kill)
        {
            return DamageThrows(damage, divide, targetArmor, shield, targetHits, out kill, out _, false);
        }

        public string GetTargetString()
        {
            string res = "";
            foreach (TargetType t in Target)
                res += t.ToString()[0].ToString();
            return res;
        }

        public string GetLogString()
        {
            return string.Format(Name + " ({0}, {1})", damage, Pierce);
        }

        public static string GetString(string name, int damage, int divide, string targets, int length)
        {
            return string.Format(name + "({0}, {2}, {3}) - {1}", damage, targets, divide, length);
        }

        public override string ToString()
        {
            return GetString(Name, damage, Pierce, GetTargetString(), Length);
        }

        #endregion //public methods and properties

        #region internal methods

        internal int AttackUnit(Unit unit, out double relicValue)
        {
            relicValue = 0;
            if (!CanAttack(unit))
                return -1;

            Used = true;
            owner.Attacked(unit.Type == UnitType.Immobile ? int.MaxValue : Length);

            int hits = unit.Hits, armor = unit.Armor;
            int damage = DoDamage(armor, unit.Shield), retVal = damage;
            double overkill = 0;
            if (damage < 0)
            {
                damage = retVal = 0;
            }
            else if (damage > hits)
            {
                overkill = (damage - hits) / (double)damage;
                damage = hits;
            }

            //attacking player gets work back for overkill, defender pays upkeep to retalliate
            if (owner.Owner == owner.Owner.Game.CurrentPlayer)
            {
                double work = owner.WorkRegen * overkill * OverkillPercent / owner.Attacks.Length;
                owner.Owner.AddWork(work);
            }
            else
            {
                double upkeep = .39 * Player.GetUpkeep(owner) * (1 - overkill) / owner.Attacks.Length;
                owner.Owner.AddUpkeep(upkeep, .21);
            }

            relicValue = (GetAverageDamage(this.damage, this.Pierce, armor, unit.Shield, hits) - damage) / RelicDivide / unit.MaxHits;
            if (relicValue > 0)
            {
                relicValue *= unit.RandedCost;
                owner.Owner.AddRelic(relicValue);
            }
            else
            {
                relicValue *= unit.InverseCost;
                unit.Owner.AddRelic(-relicValue);
            }

            unit.Wound(damage);

            return retVal;
        }

        internal void SetOwner(Unit unit)
        {
            this.owner = unit;
        }

        internal void RandStats()
        {
            if (!randed)
            {
                this.damage = Unit.RandStat(this.damage, true);
                this.randed = true;
            }
        }

        #endregion //internal methods

        #region damage 

        private int DoDamage(int armor, int shield)
        {
            return DoDamage(this.damage, this.Pierce, armor, shield);
        }
        public static int DoDamage(int damage, int pierce, int armor, int shield)
        {
            double damMult = damage * DamMultPercent;
            int d = Game.Random.Round(damage - damMult - armor / (double)pierce) + Game.Random.OEInt(damMult);
            if (shield > 0)
                d = Game.Random.Round(d * getShieldMult(shield));
            return d;
        }
        private static double getShieldMult(int shield)
        {
            return (100 - shield) / 100.0;
        }
        private static double DamageThrows(double damage, double divide, double targetArmor, int shield, int targetHits, out double killPct, out double avgRelic, bool doRelic)
        {
            killPct = 0;
            avgRelic = 0;
            double avgDamage = 0, total = 0;
            double shieldMult = getShieldMult(shield);

            double damMult = damage * DamMultPercent;
            double damStatic = damage - damMult - targetArmor / divide;

            int oeLimit = MTRandom.GetOEIntMax(damMult);

            if ((int)Math.Floor(damStatic * shieldMult) >= targetHits)
            {
                killPct = 1;
                return targetHits;
            }
            if (!doRelic && damStatic >= 0 && (int)Math.Ceiling((damStatic + oeLimit) * shieldMult) < targetHits)
                return (damage - targetArmor / divide) * shieldMult;

            double avgDmgForRelic = -1;
            if (doRelic)
                avgDmgForRelic = GetAverageDamage(damage, divide, targetArmor, shield, targetHits);

            int baseDmg = (int)Math.Floor(damStatic);
            double roundChance = damStatic - baseDmg;
            damMult /= (damMult + 1);
            double oeChance = damMult;
            for (int oe = 0; oe <= oeLimit; ++oe)
            {
                for (int round = 0; round < 2; ++round)
                {
                    double chance = oeChance * (round == 0 ? 1 - roundChance : roundChance);
                    int totDamage = Math.Max(baseDmg + (round == 0 ? 0 : 1) + oe, 0);

                    for (int shieldRound = 0; shieldRound < 2; ++shieldRound)
                    {
                        double c = chance;
                        int td = totDamage;
                        if (shield > 0)
                        {
                            double sd = totDamage * shieldMult;
                            int sbd = (int)Math.Floor(sd);
                            double src = sd - sbd;
                            c *= shieldRound == 0 ? 1 - src : src;
                            td = Math.Max(sbd + (shieldRound == 0 ? 0 : 1), 0);
                        }

                        if (td > targetHits)
                            td = targetHits;
                        if (td == targetHits)
                            killPct += c;
                        if (doRelic)
                            avgRelic += c * Math.Max(avgDmgForRelic - td, 0);
                        avgDamage += c * td;

                        total += c;

                        if (shield == 0)
                            break;
                    }
                }
                oeChance *= damMult;
            }

            killPct /= total;
            avgRelic /= total;
            return avgDamage / total;
        }

        #endregion //damage
    }

    [Flags]
    [Serializable]
    public enum TargetType
    {
        Ground = 0x1,
        Water = 0x2,
        Air = 0x4
    }
}
