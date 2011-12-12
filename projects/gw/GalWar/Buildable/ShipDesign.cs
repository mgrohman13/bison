using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class ShipDesign : Buildable
    {
        #region static

        internal static double GetStrength(int att, int def, int hp, int speed)
        {
            return GetTotCost(att, def, hp, speed, 0, false, 0, 0);
        }

        public static double GetPlanetDefenseStrength(int att, int def)
        {
            return GetPlanetDefenseCost(att, def, 0);
        }

        internal static double GetValue(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, double research)
        {
            double researchMult = GetResearchMult(research);
            return GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, researchMult, 1 / researchMult);
        }

        internal static double GetPlanetDefenseCost(int att, int def, int research)
        {
            //pass a speed of 2 so att cost=def cost and then adjust total cost as though speed were 0
            return GetTotCost(att, def, 1, 2, 0, false, 0, research) * 2.1 / 4.1 * Consts.PlanetDefensesCostMult;
        }

        internal static double GetTotCost(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, int research)
        {
            double researchMult = GetResearchMult(research);
            return GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, researchMult, researchMult);
        }

        private static double GetTotCost(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, double statResearchMult, double totalResearchMult)
        {
            double speedValue = speed + 2.1;
            double statMult = statResearchMult * hp;
            double attValue = GetStatValue(att) * statMult * speedValue / 3.9;
            double defValue = GetStatValue(def) * statMult;
            return Consts.CostMult * (
                (
                    ( attValue + defValue )
                    +
                    (
                        (
                            ( colony ? 30.0 * speedValue : 0.0 )
                            +
                            (
                                (
                                    ( Math.Pow(trans, 1 + Consts.AttackNumbersPower) )
                                    +
                                    ( 13.0 * GetBombardDamage(att, bombardDamageMult) )
                                )
                                *
                                speed
                            )
                        )
                        *
                        ( ( defValue + 9000.0 ) / 3000.0 )
                    )
                )
                *
                ( speedValue )
                *
                ( totalResearchMult )
            );
        }

        public static double GetStatValue(double stat)
        {
            double sqr = stat * stat;
            return ( sqr * stat + 6.0 * sqr + 2.0 * stat ) / 9.0;
        }

        internal static double GetHPStr(int s1, int s2)
        {
            return GetHPStr(s1, s2, Consts.BaseDesignHPMult);
        }

        private static double GetHPStr(int s1, int s2, double hpMult)
        {
            return MultStr(( s1 + s2 ) * ( s1 + s2 ), hpMult);
        }

        private static double GetResearchMult(double research)
        {
            return Consts.ResearchFactor / ( research + Consts.ResearchFactor );
        }

        internal static int GetStartDesigns(int mapSize, List<int> research, Player player, List<ShipDesign> designs, ShipNames shipNames)
        {
            int retVal = int.MaxValue;

            //randomize which of the starting research values go to which design type
            int rsrchIndx = -1;
            foreach (int i in Game.Random.Iterate(3))
            {
                bool forceColony, forceTrans, forceNeither;
                switch (i)
                {
                case 0:
                    forceColony = true;
                    forceTrans = false;
                    forceNeither = false;
                    break;
                case 1:
                    forceColony = false;
                    forceTrans = true;
                    forceNeither = false;
                    break;
                case 2:
                    forceColony = false;
                    forceTrans = false;
                    forceNeither = true;
                    break;
                default:
                    throw new Exception();
                }

                ShipDesign design = new ShipDesign(mapSize, research[++rsrchIndx], player, null, shipNames, forceColony, forceTrans, forceNeither);
                designs.Add(design);
                //the game will need to know the colony ship cost to determine starting production
                if (forceColony)
                    retVal = design._cost;
            }

            return retVal;
        }

        internal static double GetColonizationValue(int maxSpeed, double cost, int curHP, int maxHP)
        {
            //higher speed reduces bonus
            return GetDisbandValue(cost, curHP, maxHP) + Consts.ColonizationBonusPct / ( Consts.ColonizationBonusMoveFactor + maxSpeed )
                    * cost * Math.Pow(curHP / (double)maxHP, Consts.ColonizationHitPctPower);
        }

        internal static double GetDisbandValue(double cost, int curHP, int maxHP)
        {
            return Consts.DisbandPct * cost * Math.Pow(curHP / (double)maxHP, Consts.DisbandHitPctPower);
        }

        internal static double GetBombardDamage(int att, double bombardDamageMult)
        {
            return att * bombardDamageMult * Consts.BombardAttackMult;
        }

        #endregion //static

        #region fields and constructors

        public readonly bool Colony;

        internal readonly byte _name, _mark;

        private readonly byte _upkeep, _att, _def, _speed;
        private readonly ushort _cost, _research, _trans;
        private readonly int _hp;
        private readonly float _bombardDamageMult;

        internal ShipDesign(int mapSize, int research, Player player, List<ShipDesign> designs, ShipNames shipNames)
            : this(mapSize, research, player, designs, shipNames, false, false, false)
        {
        }

        private ShipDesign(int mapSize, int research, Player player, List<ShipDesign> designs, ShipNames shipNames, bool forceColony, bool forceTrans, bool forceNeither)
        {
            //  ------  Research      ------  
            checked
            {
                this._research = (ushort)research;
            }

            //  ------  Colony/Trans  ------  
            double transStr = MakeStatStr(research, 26, .666);
            DoColonyTrans(forceColony, forceTrans, forceNeither, research, designs, ref transStr, out this.Colony, out this._trans, out this._bombardDamageMult);
            //being a transport makes average att and def lower, but hp higher
            double strMult = 3 * transStr / ( 3 * transStr + ( this.Colony ? 60 : 0 ) + this._trans );
            strMult *= 600 / ( 599 + this._bombardDamageMult );

            //  ------  Att/Def       ------  
            double str = MakeStatStr(research, 1.69 * strMult, .666);
            DoAttDef(transStr, str, out this._att, out this._def);

            //  ------  HP            ------  
            double hpMult = Consts.BaseDesignHPMult / Math.Pow(strMult, this.DeathStar ? 1.3 : ( this.Colony ? 1.8 : 2.6 ));
            //average hp is relative to actual randomized stats
            this._hp = MakeStat(GetHPStr(this._att, this._def, hpMult));

            //  ------  Speed         ------  
            double speedStr = MakeStatStr(research, .666, .39);
            if (this.Colony)
                speedStr = MultStr(speedStr, .39);
            else if (this.DeathStar)
                speedStr = MultStr(speedStr, .666);
            //average speed is higher for more offensive and weaker ships
            checked
            {
                this._speed = (byte)MakeStat(MultStr(speedStr, GetSpeedMult(str, hpMult)));
            }

            this._bombardDamageMult = (float)MultStr(this._bombardDamageMult,
                    speedStr / this._speed * Math.Sqrt(str * this._def) / this._att);

            //  ------  cost/Upkeep   ------  
            double cost = -1;
            int upkeep = -1;
            GetCost(mapSize, ref cost, ref upkeep);
            double maxCost = GetMaxCost(research);
            double minCost = GetMinCost(mapSize, maxCost);
            maxCost = GetMaxCost(minCost, maxCost);
            while (cost > maxCost)
            {
                switch (GetReduce(cost, hpMult))
                {
                case ModifyStat.Att:
                    --this._att;
                    break;
                case ModifyStat.Def:
                    --this._def;
                    break;
                case ModifyStat.HP:
                    --this._hp;
                    break;
                case ModifyStat.Speed:
                    --this._speed;
                    break;
                case ModifyStat.Trans:
                    --this._trans;
                    break;
                case ModifyStat.BombardDamageMult:
                    this._bombardDamageMult -= ( 1 - Game.Random.NextFloat() );
                    if (this._bombardDamageMult < 1)
                        this._bombardDamageMult = 1;
                    break;
                case ModifyStat.None:
                    maxCost = cost;
                    break;
                default:
                    throw new Exception();
                }
                GetCost(mapSize, ref cost, ref upkeep);
            }
            while (cost < minCost)
            {
                switch (GetIncrease(hpMult))
                {
                case ModifyStat.Att:
                    ++this._att;
                    break;
                case ModifyStat.Def:
                    ++this._def;
                    break;
                case ModifyStat.HP:
                    ++this._hp;
                    break;
                default:
                    throw new Exception();
                }
                GetCost(mapSize, ref cost, ref upkeep);
            }
            checked
            {
                this._cost = (ushort)Game.Random.Round(cost);
                this._upkeep = (byte)upkeep;
            }
            //  ------  name          ------  
            this._name = shipNames.GetName(player, this, transStr, speedStr);
            this._mark = shipNames.GetMark(player, _name);
        }

        private void DoColonyTrans(bool forceColony, bool forceTrans, bool forceNeither, int research, List<ShipDesign> designs,
                ref double transStr, out bool colony, out ushort trans, out float bombardDamageMult)
        {
            bombardDamageMult = 1;

            bool transport;
            if (forceTrans)
                transport = true;
            else if (forceNeither || forceColony)
                transport = false;
            else
                transport = MakeTransport(designs, transStr);

            if (forceColony)
                colony = true;
            else if (forceNeither || forceTrans)
                colony = false;
            else
                colony = MakeColony(designs);

            //colony ships transport a reduced amount on average
            if (colony && !transport)
                transStr = MultStr(transStr, .39);

            if (colony || transport)
            {
                checked
                {
                    trans = (ushort)MakeStat(transStr);
                }
            }
            else
            {
                trans = 0;

                if (!forceNeither && MakeDeathStar(designs, research))
                    bombardDamageMult = MakeStat(130) + ( 1 - Game.Random.NextFloat() );
            }
        }

        private bool MakeTransport(List<ShipDesign> designs, double transStr)
        {
            if (designs.Count == 0)
                return false;

            double pct = 0;
            foreach (ShipDesign design in designs)
                pct += design.Speed * design.Trans * ( design.Colony ? .13 : 1 );
            pct /= designs.Count * 2.1;

            pct = pct / ( pct + transStr );

            return CreateType(.13, pct);
        }

        private bool MakeColony(List<ShipDesign> designs)
        {
            if (designs.Count == 0)
                return false;

            double pct = 0;
            foreach (ShipDesign design in designs)
                if (design.Colony)
                    ++pct;
            pct /= designs.Count;

            return CreateType(.078, pct);
        }

        private bool MakeDeathStar(List<ShipDesign> designs, int research)
        {
            if (designs.Count == 0)
                return false;

            double pct = 0;
            foreach (ShipDesign design in designs)
                pct += design.Speed * design.BombardDamage;
            pct /= designs.Count;

            pct = pct / ( pct + 30 );

            double target = research / ( 2.1 * Consts.ResearchFactor + research );
            target *= target * target * .21;

            return CreateType(target, pct);
        }

        private bool CreateType(double target, double actual)
        {
            double chance;
            //chance is higher when target > actual and lower when target < actual
            if (target > actual)
                chance = Math.Sqrt(target - actual) + target;
            else
                chance = ( 1 + ( target - actual ) / actual ) * target;
            return Game.Random.Bool(chance);
        }

        private void DoAttDef(double transStr, double str, out byte att, out byte def)
        {
            int s1 = MakeStat(str);
            //average second stat adjusted to compensate for first
            int s2 = MakeStat(MultStr(str, Math.Sqrt(str / (double)s1)));
            if (s2 > s1)
            {
                int temp = s1;
                s1 = s2;
                s2 = temp;
            }
            //colony, transports more likely to be defensive
            if (Game.Random.Bool(( this.Colony || this.Trans > Game.Random.Gaussian(transStr * .6, .39) ) ? .21f : .666f))
            {
                checked
                {
                    att = (byte)s1;
                    def = (byte)s2;
                }
            }
            else
            {
                checked
                {
                    att = (byte)s2;
                    def = (byte)s1;
                }
            }
        }

        private double GetSpeedMult(double str, double hpMult)
        {
            double offenseFactor = Math.Sqrt(this.Att / (double)this.Def);
            double strengthFactor = Math.Sqrt(2 * GetStatValue(str) * MultStr(4 * str * str, hpMult)
                    / (double)( ( GetStatValue(this.Att) + GetStatValue(this.Def) ) * this.HP ));
            return Math.Sqrt(offenseFactor * strengthFactor);
        }

        private double MakeStatStr(double research, double mult, double power)
        {
            //all stats based initially on research
            double str = ( Consts.ResearchFactor + research ) / Consts.ResearchFactor;
            str = Math.Pow(str, power) * mult;
            //always add 1
            return ++str;
        }

        private static double MultStr(double str, double mult)
        {
            //only multiply the portion that is greater than 1
            return 1 + ( str - 1 ) * mult;
        }

        private int MakeStat(double str)
        {
            //str will always be greater than 1
            --str;
            double oe = str * .13;
            double gaussian = str - oe;
            return Game.Random.GaussianCappedInt(gaussian + 1, 1, 1) + Game.Random.OEInt(oe);
        }

        private static double GetMaxCost(int research)
        {
            return Math.Pow(research, Consts.MaxCostPower) * Consts.MaxCostMult;
        }

        private double GetMinCost(int mapSize, double maxCost)
        {
            double minCost = this.GetUpkeepPayoff(mapSize) * Consts.MinCostMult + 1 / Consts.RepairCostMult;
            //randomized increase to absolute min
            return minCost + Game.Random.OE(maxCost / 21.0);
        }

        private static double GetMaxCost(double minCost, double maxCost)
        {
            //max is more of a guideline than actual rule
            if (maxCost > minCost)
                maxCost = Game.Random.GaussianCapped(maxCost, .26, minCost);
            else
                maxCost = minCost;
            return maxCost;
        }

        private void GetCost(int mapSize, ref double cost, ref int upkeep)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(mapSize);

            double totCost = GetTotCost();

            if (cost > 0)
            {
                double avgUpk = totCost * upkeep / ( cost + upkeep * upkeepPayoff );
                if (avgUpk > 1)
                    upkeep = Game.Random.Round(avgUpk);
                else
                    upkeep = 1;
            }
            else
            {
                double avgUpk = totCost / upkeepPayoff * Consts.CostUpkeepPct;
                if (avgUpk > 1)
                    upkeep = Game.Random.GaussianCappedInt(avgUpk, 1, 1);
                else
                    upkeep = 1;
            }

            while (upkeep > 1 && upkeep * upkeepPayoff > totCost / 2.0)
                --upkeep;

            cost = ( totCost - upkeep * upkeepPayoff );
        }

        public double GetColonizationValue(int mapSize)
        {
            return GetColonizationValue(this.Speed, AdjustCost(mapSize), this.HP, this.HP);
        }

        internal double AdjustCost(int mapSize)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(mapSize);
            double cost = GetTotCost() - this.Upkeep * upkeepPayoff;
            cost = cost + ( cost - this.Cost ) / Consts.RepairCostMult;
            if (cost < upkeepPayoff * Consts.MinCostMult)
                throw new Exception();
            return cost;
        }

        private double GetTotCost()
        {
            return GetTotCost(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this._bombardDamageMult, this.Research);
        }

        private ModifyStat GetReduce(double cost, double hpMult)
        {
            Dictionary<ModifyStat, int> stats = new Dictionary<ModifyStat, int>();

            stats.Add(ModifyStat.Att, ReduceAttDef(this.Att, this.Def, hpMult));
            stats.Add(ModifyStat.Def, ReduceAttDef(this.Def, this.Att, hpMult));
            stats.Add(ModifyStat.HP, ( this.HP > 1 ? this.HP : 0 ));
            if (this.Speed > 1)
                stats.Add(ModifyStat.Speed, Game.Random.Round(this.Speed * .39f));
            if (this.Trans > 1)
                stats.Add(ModifyStat.Trans, Game.Random.Round(this.Trans * 1.3f));
            if (this.DeathStar)
                stats.Add(ModifyStat.BombardDamageMult, Game.Random.Round(GetBombardDamage(this.Att, this._bombardDamageMult) * this.Speed * 13 + 78));

            bool none = true;
            foreach (int value in stats.Values)
                if (value > 0)
                {
                    none = false;
                    break;
                }
            if (none)
                return ModifyStat.None;

            return Game.Random.SelectValue<ModifyStat>(stats);
        }

        private int ReduceAttDef(int stat, int other, double hpMult)
        {
            if (stat > 1)
            {
                double hpStr = GetHPStr(stat, other, hpMult);
                hpStr /= hpStr - GetHPStr(stat - 1, other, hpMult);
                if (other > 1)
                    hpStr = GetStatChance(hpStr, stat);
                return Game.Random.Round(hpStr);
            }
            return 0;
        }

        private ModifyStat GetIncrease(double hpMult)
        {
            Dictionary<ModifyStat, int> stats = new Dictionary<ModifyStat, int>();

            stats.Add(ModifyStat.Att, IncreaseAttDef(this.Att, this.Def, hpMult));
            stats.Add(ModifyStat.Def, IncreaseAttDef(this.Def, this.Att, hpMult));
            stats.Add(ModifyStat.HP, Game.Random.Round(GetHPStr(this.Att, this.Def, hpMult)));

            ModifyStat retVal = Game.Random.SelectValue<ModifyStat>(stats);
            return retVal;
        }

        private int IncreaseAttDef(int stat, int other, double hpMult)
        {
            double chance = GetStatChance(this.HP, stat);
            chance /= GetHPStr(stat + 1, other, hpMult) - GetHPStr(stat, other, hpMult);
            return Game.Random.Round(chance);
        }

        private double GetStatChance(double mult, int stat)
        {
            return mult * stat / (double)( this.Att + this.Def );
        }

        #endregion //fields and constructors

        #region internal

        internal double GetUpkeepPayoff(int mapSize)
        {
            return Consts.GetUpkeepPayoff(mapSize, this.Colony, this._trans, this._speed);
        }

        internal float BombardDamageMult
        {
            get
            {
                return this._bombardDamageMult;
            }
        }

        internal override bool NeedsTile
        {
            get
            {
                return true;
            }
        }

        internal override bool Multiple
        {
            get
            {
                return true;
            }
        }

        internal override bool CanBeBuiltBy(Colony colony)
        {
            return colony.Player.GetShipDesigns().Contains(this);
        }

        public override string GetProdText(string curProd)
        {
            return curProd + " / " + this._cost.ToString();
        }

        internal HashSet<ShipDesign> GetObsolete(int mapSize, List<ShipDesign> designs)
        {
            HashSet<ShipDesign> retVal = new HashSet<ShipDesign>();
            foreach (ShipDesign design in designs)
                if (MakesObsolete(mapSize, design))
                    retVal.Add(design);
            return retVal;
        }

        private bool MakesObsolete(int mapSize, ShipDesign oldDesign)
        {
            double totCost = this._cost + this.Upkeep * this.GetUpkeepPayoff(mapSize);
            double oldTotCost = oldDesign._cost + oldDesign.Upkeep * oldDesign.GetUpkeepPayoff(mapSize);

            double attStr = GetStatValue(this.Att) * this.HP;
            double oldAttStr = GetStatValue(oldDesign.Att) * oldDesign.HP;

            double defStr = GetStatValue(this.Def) * this.HP;
            double oldDefStr = GetStatValue(oldDesign.Def) * oldDesign.HP;

            double transStr = this.Trans * this.Speed;
            double oldTransStr = oldDesign.Trans * oldDesign.Speed;

            double colonyStr = ( this.Colony ? 1 : 0 );
            double oldColonyStr = ( oldDesign.Colony ? 1 : 0 );

            double deathStr = this.Att * this.Speed * this._bombardDamageMult;
            double oldDeathStr = oldDesign.Att * oldDesign.Speed * oldDesign._bombardDamageMult;

            return (
                //must be at least as fast, and either
                this.Speed >= oldDesign.Speed &&
                //be better in each stat category and have a lower cost and upkeep, or
                ( ( attStr >= oldAttStr && defStr >= oldDefStr && transStr >= oldTransStr &&
                colonyStr >= oldColonyStr && deathStr >= oldDeathStr &&
                this._cost <= oldDesign._cost && this.Upkeep <= oldDesign.Upkeep ) ||
                //have a better value per total cost in each category and a similar cost and upkeep
                ( attStr / totCost >= oldAttStr / oldTotCost && defStr / totCost >= oldDefStr / oldTotCost &&
                transStr / totCost >= oldTransStr / oldTotCost && colonyStr / totCost >= oldColonyStr / oldTotCost &&
                deathStr / totCost >= oldDeathStr / oldTotCost &&
                ObsoleteCost(this._cost, oldDesign._cost, this.Upkeep, oldDesign.Upkeep) ) )
            );
        }

        private bool ObsoleteCost(double c1, double c2, double u1, double u2)
        {
            double c = Math.Min(c1, c2) / Math.Max(c1, c2);
            double u = Math.Min(u1, u2) / Math.Max(u1, u2);
            return Game.Random.Bool(Math.Pow(c * c * c * c * c * u * u * u, .36));
        }

        internal override void Build(Colony colony, Tile tile, IEventHandler handler)
        {
            Ship ship = colony.Player.NewShip(tile, this, handler);

            int max = Math.Min(colony.AvailablePop, ship.FreeSpace);
            if (max > 0)
            {
                double goldBonus = PopCarrier.GetGoldCost(max);
                if (colony.Player.Gold > goldBonus)
                    goldBonus = 0;
                else if (colony.Player.Gold < 0)
                    goldBonus -= colony.Player.Gold;
                colony.Player.AddGold(goldBonus);

                max = handler.MoveTroops(colony, max, 0, colony.Population, colony.GetSoldiers(colony.Population));
                if (max > 0)
                    colony.MovePop(max, ship);
                ship.ResetMoved();

                colony.Player.SpendGold(goldBonus);
            }
        }

        #endregion //internal

        #region public

        public int Research
        {
            get
            {
                return this._research;
            }
        }

        public int HP
        {
            get
            {
                return this._hp;
            }
        }

        public int Trans
        {
            get
            {
                return this._trans;
            }
        }

        public int Upkeep
        {
            get
            {
                return this._upkeep;
            }
        }

        public int Att
        {
            get
            {
                return this._att;
            }
        }

        public int Def
        {
            get
            {
                return this._def;
            }
        }

        public int Speed
        {
            get
            {
                return this._speed;
            }
        }

        public double BombardDamage
        {
            get
            {
                return GetBombardDamage(this._att, this._bombardDamageMult);
            }
        }

        public bool DeathStar
        {
            get
            {
                return ( this._bombardDamageMult > 1 );
            }
        }

        public override int Cost
        {
            get
            {
                return this._cost;
            }
        }

        public override string ToString()
        {
            return ShipNames.GetName(this._name, this._mark);
        }

        #endregion //public

        private enum ModifyStat
        {
            Att,
            Def,
            HP,
            Speed,
            Trans,
            BombardDamageMult,
            None,
        }
    }
}
