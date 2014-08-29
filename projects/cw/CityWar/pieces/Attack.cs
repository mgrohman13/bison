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
        public const double DeathDivide = 7;
        //unit cost : death for disband
        internal const double DisbandDivide = 3;
        //unit cost : relic for wounding
        internal const double RelicDivide = 5;

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
        //private bool randed;

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
            //this.randed = false;
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
            return GetMinDamage(damage, Pierce, target.Armor);
        }
        public static int GetMinDamage(double damage, double divide, double armor)
        {
            int minDamage = (int)( damage * ( 1 - DamMultPercent ) - armor / divide );
            return ( minDamage > 0 ? minDamage : 0 );
        }

        public double GetAverageDamage(Unit enemy, out double killPct, out double avgRelic)
        {
            double averageDamage = DamageThrows(damage, Pierce, enemy.Armor, enemy.hits, out killPct, out avgRelic, true);
            killPct *= 100;
            avgRelic *= enemy.RandedCost / RelicDivide / enemy.MaxHits;
            return averageDamage;
        }

        public static double GetAverageDamage(double damage, double divide, double targetArmor, int targetHits)
        {
            double kill;
            return GetAverageDamage(damage, divide, targetArmor, targetHits, out kill);
        }
        public static double GetAverageDamage(double damage, double divide, double targetArmor, int targetHits, out double kill)
        {
            double relic;
            return DamageThrows(damage, divide, targetArmor, targetHits, out kill, out relic, false);
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

        internal int AttackUnit(Unit unit)
        {
            if (!CanAttack(unit))
                return -1;

            Used = true;
            owner.Attacked(Length);

            int hits = unit.Hits, armor = unit.Armor;
            int damage = DoDamage(armor), retVal = damage;
            double overkill = 0;
            if (damage < 0)
            {
                damage = retVal = 0;
            }
            else if (damage > hits)
            {
                overkill = ( damage - hits ) / (double)damage;
                damage = hits;
            }

            //attacking player gets work back for overkill, defender pays upkeep to retalliate
            if (owner.Owner == owner.Owner.Game.CurrentPlayer)
                owner.Owner.AddWork(owner.WorkRegen * overkill * OverkillPercent / owner.Attacks.Length);
            else
                owner.Owner.AddUpkeep(.39 * Player.GetUpkeep(owner) * ( 1 - overkill ) / owner.Attacks.Length, .169);

            double relicValue = ( GetAverageDamage(this.damage, this.Pierce, armor, hits) - damage ) / RelicDivide / unit.MaxHits;
            if (relicValue > 0)
                owner.Owner.AddRelic(unit.RandedCost * relicValue);
            else
                unit.Owner.AddRelic(unit.InverseCost * -relicValue);

            unit.Wound(damage);

            return retVal;
        }

        internal void SetOwner(Unit unit)
        {
            this.owner = unit;
        }

        internal void RandStats()
        {
            //if (!randed)
            this.damage = Unit.RandStat(this.damage, true);
        }

        #endregion //internal methods

        #region damage

        private double damMult
        {
            get
            {
                return damage * DamMultPercent;
            }
        }
        private double damStatic
        {
            get
            {
                return damage - damMult;
            }
        }

        private int DoDamage(int armor)
        {
            return Game.Random.Round(damStatic - armor / (double)Pierce) + Game.Random.OEInt(damMult);
        }

        private static double DamageThrows(double damage, double divide, double targetArmor, int targetHits, out double killPct, out double avgRelic, bool doRelic)
        {
            killPct = 0;
            avgRelic = 0;
            double avgDamage = 0, total = 0;

            double avgDmgForRelic = -1;
            if (doRelic)
                avgDmgForRelic = GetAverageDamage(damage, divide, targetArmor, targetHits);

            double damMult = damage * DamMultPercent;
            double damStatic = damage - damMult - targetArmor / divide;

            int oeLimit = MattUtil.MTRandom.GetOEIntMax(damMult);

            if (!doRelic && damStatic >= 0 && (int)Math.Ceiling(damStatic) + oeLimit < targetHits)
                return damage - targetArmor / divide;

            int baseDmg = (int)Math.Floor(damStatic);
            double roundChance = damStatic - baseDmg;
            damMult /= ( damMult + 1 );
            double oeChance = damMult;
            for (int oe = 0 ; oe <= oeLimit ; ++oe)
            {
                for (int round = 0 ; round < 2 ; ++round)
                {
                    double chance = oeChance * ( round == 0 ? 1 - roundChance : roundChance );
                    int totDamage = Math.Max(Math.Min(baseDmg + ( round == 0 ? 0 : 1 ) + oe, targetHits), 0);

                    if (totDamage == targetHits)
                        killPct += chance;
                    if (doRelic)
                        avgRelic += chance * Math.Max(avgDmgForRelic - totDamage, 0);
                    avgDamage += chance * totDamage;

                    total += chance;
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
