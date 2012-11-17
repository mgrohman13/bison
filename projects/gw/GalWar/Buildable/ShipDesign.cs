using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class ShipDesign : Buildable
    {
        #region cost

        public static double GetStrength(int att, int def, int hp, int speed)
        {
            return GetTotCost(att, def, hp, speed, 0, false, 0, 0);
        }

        public static double GetPlanetDefenseStrength(int att, int def)
        {
            return GetPlanetDefenseCost(att, def, 0);
        }

        public static double GetValue(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, double research)
        {
            double researchMult = GetResearchMult(research);
            return GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, researchMult, 1 / researchMult);
        }

        internal static double GetPlanetDefenseCost(int att, int def, int research)
        {
            double researchMult = GetResearchMult(research);
            //pass a speed value such that att cost=def cost and then adjust total cost as though speed were 0
            return GetTotCost(att, def, 1, attDiv - speedAdd, 0, false, 0, researchMult, researchMult) * speedAdd / attDiv * Consts.PlanetDefensesCostMult;
        }

        public static double GetTotCost(int att, int def, int hp, int speed, int trans, bool colony, float bombardDamageMult, double research)
        {
            double researchMult = GetResearchMult(research);
            return GetTotCost(att, def, hp, speed, trans, colony, bombardDamageMult, researchMult, researchMult);
        }

        private const double speedAdd = 2.1, attDiv = 3.9;
        private static double GetTotCost(int att, int def, int hp, double speed, int trans, bool colony, float bombardDamageMult, double statResearchMult, double totalResearchMult)
        {
            double speedValue = speed + speedAdd;
            double statMult = statResearchMult * hp;
            double attValue = GetStatValue(att) * statMult * speedValue / attDiv;
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

        internal static double GetBombardDamage(double att, double bombardDamageMult)
        {
            return att * bombardDamageMult * Consts.BombardAttackMult;
        }

        private static double GetResearchMult(double research)
        {
            return Consts.ResearchFactor / ( research + Consts.ResearchFactor );
        }

        #endregion //cost

        #region fields and constructors

        public readonly bool Colony;

        internal readonly byte _name, _mark;

        private readonly byte _upkeep, _att, _def, _speed;
        private readonly ushort _cost, _research, _trans;
        private readonly ushort _hp;
        private readonly float _bombardDamageMult;

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
                    retVal = design.Cost;
            }

            return retVal;
        }

        internal ShipDesign(int mapSize, int research, Player player, List<ShipDesign> designs, ShipNames shipNames)
            : this(mapSize, research, player, designs, shipNames, false, false, false)
        {
        }

        private ShipDesign(int mapSize, int research, Player player, List<ShipDesign> designs, ShipNames shipNames, bool forceColony, bool forceTrans, bool forceNeither)
        {
            checked
            {
                //  ------  Research          ------
                this._research = (ushort)research;

                //get pcts for existing designs
                double colonyPct, upkeepPct, attPct, speedPct, transPct, dsPct;
                GetPcts(designs, mapSize, research, out colonyPct, out upkeepPct, out attPct, out speedPct, out transPct, out dsPct);

                //  ------  Colony/Trans      ------
                double transStr = GetTransStr(research);
                DoColonyTrans(forceColony, forceTrans, forceNeither, research, colonyPct, transPct, dsPct,
                        ref transStr, out this.Colony, out this._trans, out this._bombardDamageMult);

                //  ------  Att/Def           ------
                //being a colony ship/transport/death star makes att and def lower
                double strMultOffset = Math.PI * transStr;
                double strMult = strMultOffset / ( strMultOffset + ( this.Colony ? 65 : 0 ) + this.Trans ) * 601 / ( 600 + this.BombardDamageMult );
                double str = GetAttDefStr(research, strMult);
                DoAttDef(transStr, str, attPct, out this._att, out this._def);

                //  ------  HP                ------
                //being a colony ship/transport/death star makes hp higher
                double hpMult = Consts.BaseDesignHPMult / Math.Pow(strMult, this.DeathStar ? 1.3 : ( this.Colony ? 1.69 : Math.E ));
                //hp is relative to actual randomized stats
                this._hp = (ushort)MakeStat(GetHPStr(this.Att, this.Def, hpMult));

                //  ------  Speed             ------
                double speedStr = MultStr(ModSpeedStr(GetSpeedStr(research), transStr, true), GetSpeedMult(str, hpMult, speedPct));
                this._speed = (byte)MakeStat(speedStr);

                //  ------  BombardDamageMult ------
                //modify bombard mult based on speed and att
                if (this.DeathStar)
                {
                    this._bombardDamageMult *= (float)Math.Sqrt(speedStr / this.Speed * Math.Sqrt(str * this.Def) / this.Att);
                    this._bombardDamageMult = CheckBombardDamage(true);
                }

                //  ------  Cost/Upkeep       ------
                double cost = -1;
                int upkeep = -1;
                GetCost(mapSize, upkeepPct, ref cost, ref upkeep);
                double maxCost = GetMaxCost(research);
                maxCost = GetMaxCost(GetMinCost(mapSize, maxCost), maxCost);
                while (cost > maxCost)
                {
                    switch (GetReduce(cost, hpMult))
                    {
                    case ModifyStat.Att:
                        --this._att;
                        if (this.DeathStar)
                            this._bombardDamageMult = CheckBombardDamage(true);
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
                        this._bombardDamageMult = CheckBombardDamage(false);
                        break;
                    case ModifyStat.None:
                        maxCost = cost;
                        break;
                    default:
                        throw new Exception();
                    }
                    GetCost(mapSize, upkeepPct, ref cost, ref upkeep);
                }
                while (cost < GetMinCost(mapSize, maxCost))
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
                    GetCost(mapSize, upkeepPct, ref cost, ref upkeep);
                }
                this._cost = (ushort)Game.Random.Round(cost);
                this._upkeep = (byte)upkeep;

                //  ------  Name              ------
                this._name = shipNames.GetName(this, GetTransStr(research), GetSpeedStr(research));
                this._mark = shipNames.GetMark(player, this._name);
            }
        }

        private const float deathStarAvg = 130;
        private static void GetPcts(List<ShipDesign> designs, int mapSize, int research,
                out double colony, out double upkeep, out double att, out double speed, out double trans, out double ds)
        {
            upkeep = 1;
            att = 1;
            speed = 1;

            int numDesigns;
            if (designs != null && ( numDesigns = designs.Count ) > 0)
            {
                colony = 0;
                trans = 0;
                ds = 0;

                double costMult = 0;
                foreach (ShipDesign design in designs)
                {
                    double upkeepPayoff = design.GetUpkeepPayoff(mapSize);
                    double totalCost = design.Cost + design.Upkeep * upkeepPayoff;

                    upkeep *= design.Upkeep / ( totalCost / upkeepPayoff * Consts.CostUpkeepPct );
                    att *= design.Att / (double)design.Def;
                    speed *= design.Speed / design.ModSpeedStr(GetSpeedStr(design.Research), GetTransStr(design.Research), false);

                    costMult += totalCost;
                    double mult = design.Research + 260;
                    mult = ( mult + ( Math.Pow(mult, .169) - 2.6 ) * 1300 ) / research / totalCost;

                    if (design.Colony)
                        colony += mult;
                    trans += design.Speed * design.Trans * mult;
                    ds += design.Speed * design.BombardDamage * mult;
                }

                double pow = 1.0 / numDesigns;
                upkeep = Math.Pow(upkeep, pow);
                att = Math.Pow(att, pow);
                speed = Math.Pow(speed, pow);

                costMult /= numDesigns * numDesigns;
                colony *= costMult;
                trans *= costMult;
                ds *= costMult;
                double speedStr = GetSpeedStr(research);
                trans /= speedStr * GetTransStr(research);
                ds /= speedStr * GetBombardDamage(GetAttDefStr(research), deathStarAvg);
            }
            else
            {
                colony = 1;
                trans = 1;
                ds = 1;
            }
        }

        private static double MakeStatStr(double research, double mult, double power)
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

        public static int MakeStat(double str)
        {
            return Game.Random.GaussianOEInt(str, 1, .1, 1);
        }

        #endregion //fields and constructors

        #region Colony/Trans

        public static double GetTransStr(double research)
        {
            return MakeStatStr(research, 26, .65);
        }

        private void DoColonyTrans(bool forceColony, bool forceTrans, bool forceNeither, int research, double colonyPct, double transPct, double dsPct,
                ref double transStr, out bool colony, out ushort trans, out float bombardDamageMult)
        {
            bool transport;
            if (forceTrans)
                transport = true;
            else if (forceNeither || forceColony)
                transport = false;
            else
                transport = CreateType(.169, transPct);

            if (forceColony)
                colony = true;
            else if (forceNeither || forceTrans)
                colony = false;
            else
                colony = CreateType(.104, colonyPct);

            //pure colony ships transport a reduced amount
            if (colony && !transport)
                transStr = MultStr(transStr, .39);

            bombardDamageMult = 1;
            if (colony || transport)
            {
                trans = (ushort)MakeStat(transStr);
            }
            else
            {
                trans = 0;

                if (!forceNeither && CreateDeathStar(research, dsPct))
                {
                    float min = 1 / Consts.BombardAttackMult - .5f;
                    bombardDamageMult = MakeStat(deathStarAvg - min) + min + Game.Random.FloatHalf() - .5f;
                }
            }
        }

        private bool CreateDeathStar(int research, double actual)
        {
            //target pct of ships that should be death stars increases with research
            double target = research / ( Math.PI * 1040.0 + research );
            target *= target * target * .169;

            return CreateType(target, actual);
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

        #endregion //Colony/Trans

        #region Att/Def

        public static double GetAttDefStr(double research)
        {
            return GetAttDefStr(research, 1);
        }

        private static double GetAttDefStr(double research, double strMult)
        {
            return MakeStatStr(research, 1.69 * strMult, .65);
        }

        private void DoAttDef(double transStr, double str, double attPct, out byte att, out byte def)
        {
            int s1 = MakeStat(str);
            //second stat is adjusted to compensate for the first
            int s2 = MakeStat(MultStr(str, Math.Sqrt(str / s1)));
            if (s2 > s1)
            {
                int temp = s1;
                s1 = s2;
                s2 = temp;
            }

            //colony ships and transports are more likely to be defensive
            double chance = ( ( this.Colony || this.Trans > Game.Random.Gaussian(transStr * .52, .39) ) ? .26 : .65 );
            attPct = Math.Sqrt(attPct);
            if (attPct < 1)
                chance = ( 1 - ( ( 1 - chance ) * attPct ) );
            else
                chance /= attPct;

            if (Game.Random.Bool(chance))
            {
                att = (byte)s1;
                def = (byte)s2;
            }
            else
            {
                att = (byte)s2;
                def = (byte)s1;
            }
        }

        #endregion //Att/Def

        #region Speed

        private static double GetSpeedStr(int research)
        {
            return MakeStatStr(research, .65, .39);
        }

        private double ModSpeedStr(double speedStr, double transStr, bool doColAndTrans)
        {
            if (this.Colony)
                speedStr = MultStr(speedStr, .39);
            else if (this.DeathStar)
                speedStr = MultStr(speedStr, .65);
            if (( doColAndTrans || !this.Colony ) && this.Trans > 0)
            {
                double transFactor = transStr / this.Trans;
                if (transFactor < 1)
                    speedStr = MultStr(speedStr, Math.Pow(transFactor, .3));
            }
            return speedStr;
        }

        private double GetSpeedMult(double str, double hpMult, double speedPct)
        {
            //speed is higher for more offensive and weaker ships
            double offenseFactor = this.Att / (double)this.Def;
            double strengthFactor = 2 * GetStatValue(str) * MultStr(4 * str * str, hpMult)
                    / (double)( ( GetStatValue(this.Att) + GetStatValue(this.Def) ) * this.HP );
            return Math.Pow(offenseFactor * strengthFactor / speedPct, .21);
        }

        #endregion //Speed

        #region Cost/Upkeep

        private static double GetMaxCost(int research)
        {
            return Math.Pow(research, Consts.MaxCostPower) * Consts.MaxCostMult;
        }

        private double GetMinCost(int mapSize, double maxCost)
        {
            double minCost = this.GetUpkeepPayoff(mapSize) * Consts.MinCostMult + 1 / Consts.RepairCostMult;
            //randomized increase to absolute min
            return minCost + Game.Random.OE(maxCost / 39);
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

        private void GetCost(int mapSize, double upkeepPct, ref double cost, ref int upkeep)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(mapSize);
            double totCost = GetTotCost();

            if (cost > 0)
            {
                //cost has been previously calculated, so maintain upkeep pct
                double avgUpk = totCost * upkeep / ( cost + upkeep * upkeepPayoff );
                if (avgUpk > 1)
                    upkeep = Game.Random.Round(avgUpk);
                else
                    upkeep = 1;
            }
            else
            {
                //calculating for the first time, so randomize upkeep
                double avgUpk = totCost / upkeepPayoff * Consts.CostUpkeepPct / Math.Sqrt(upkeepPct);
                if (avgUpk > 1)
                    upkeep = Game.Random.GaussianCappedInt(avgUpk, 1, 1);
                else
                    upkeep = 1;
            }

            //upkeep should never account for more than half of the ship's cost
            while (upkeep > 1 && upkeep * upkeepPayoff > totCost / 2.0)
                --upkeep;

            cost = ( totCost - upkeep * upkeepPayoff );
        }

        private double GetTotCost()
        {
            return GetTotCost(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamageMult, this.Research);
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
                stats.Add(ModifyStat.BombardDamageMult, Game.Random.Round(this.BombardDamage * this.Speed * 13 + 78));

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

            return Game.Random.SelectValue<ModifyStat>(stats);
        }

        private int IncreaseAttDef(int stat, int other, double hpMult)
        {
            return Game.Random.Round(GetStatChance(this.HP, stat) / ( GetHPStr(stat + 1, other, hpMult) - GetHPStr(stat, other, hpMult) ));
        }

        private double GetStatChance(double mult, int stat)
        {
            return mult * stat / (double)( this.Att + this.Def );
        }

        private float CheckBombardDamage(bool keepDeathStar)
        {
            const float minDamage = 1f;
            if (this.BombardDamageMult < 1 || this.BombardDamage < minDamage)
                if (keepDeathStar)
                    return Math.Max(1, minDamage / Consts.BombardAttackMult / this.Att);
                else
                    return 1;
            return this.BombardDamageMult;
        }

        #endregion //Cost/Upkeep

        #region internal

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

        public double GetUpkeepPayoff(int mapSize)
        {
            return Consts.GetUpkeepPayoff(mapSize, GetNonColonyPct(), GetNonTransPct(), this.Speed);
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
            double totCost = this.Cost + this.Upkeep * this.GetUpkeepPayoff(mapSize);
            double oldTotCost = oldDesign.Cost + oldDesign.Upkeep * oldDesign.GetUpkeepPayoff(mapSize);

            double attStr = GetStatValue(this.Att) * this.HP;
            double oldAttStr = GetStatValue(oldDesign.Att) * oldDesign.HP;

            double defStr = GetStatValue(this.Def) * this.HP;
            double oldDefStr = GetStatValue(oldDesign.Def) * oldDesign.HP;

            double transStr = this.Trans * this.Speed;
            double oldTransStr = oldDesign.Trans * oldDesign.Speed;

            double colonyStr = ( this.Colony ? 1 : 0 );
            double oldColonyStr = ( oldDesign.Colony ? 1 : 0 );

            double deathStr = this.Att * this.Speed * this.BombardDamageMult;
            double oldDeathStr = oldDesign.Att * oldDesign.Speed * oldDesign.BombardDamageMult;

            return (
                //must be at least as fast, and either
                this.Speed >= oldDesign.Speed &&
                //be better in each stat category and have a lower cost and upkeep, or
                ( ( attStr >= oldAttStr && defStr >= oldDefStr && transStr >= oldTransStr &&
                colonyStr >= oldColonyStr && deathStr >= oldDeathStr &&
                this.Cost <= oldDesign.Cost && this.Upkeep <= oldDesign.Upkeep ) ||
                //have a better value per total cost in each category and a similar cost and upkeep
                ( attStr / totCost >= oldAttStr / oldTotCost && defStr / totCost >= oldDefStr / oldTotCost &&
                transStr / totCost >= oldTransStr / oldTotCost && colonyStr / totCost >= oldColonyStr / oldTotCost &&
                deathStr / totCost >= oldDeathStr / oldTotCost &&
                ObsoleteCost(this.Cost, oldDesign.Cost, this.Upkeep, oldDesign.Upkeep) ) )
            );
        }

        private bool ObsoleteCost(double c1, double c2, double u1, double u2)
        {
            double c = Math.Min(c1, c2) / Math.Max(c1, c2);
            double u = Math.Min(u1, u2) / Math.Max(u1, u2);
            return Game.Random.Bool(Math.Pow(c * c * c * c * c * u * u * u, Math.E * .13));
        }

        internal override void Build(IEventHandler handler, Colony colony, Tile tile)
        {
            Ship ship = colony.Player.NewShip(handler, tile, this);

            int max = Math.Min(colony.AvailablePop, ship.FreeSpace);
            if (max > 0)
            {
                //ensure the player has enough gold to move any number of troops
                double goldBonus = PopCarrier.GetGoldCost(max);
                if (colony.Player.Gold < goldBonus)
                    colony.Player.AddGold(0, goldBonus);

                max = handler.MoveTroops(colony, max, 0, colony.Population, colony.Soldiers);
                if (max > 0)
                {
                    colony.MovePop(handler, max, ship);
                    //troops can be moved again next turn
                    ship.ResetMoved();
                }
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
                return GetBombardDamage(this.Att, this.BombardDamageMult);
            }
        }

        public bool DeathStar
        {
            get
            {
                return ( this.BombardDamageMult > 1 );
            }
        }

        public override int Cost
        {
            get
            {
                return this._cost;
            }
        }

        public double GetStrength()
        {
            return GetStrength(this.Att, this.Def, this.HP, this.Speed);
        }

        public static double GetHPStr(int s1, int s2)
        {
            return GetHPStr(s1, s2, Consts.BaseDesignHPMult);
        }

        private static double GetHPStr(int s1, int s2, double hpMult)
        {
            return MultStr(( s1 + s2 ) * ( s1 + s2 ), hpMult);
        }

        public double GetColonizationValue(int mapSize)
        {
            return GetColonizationValue(this.Speed, AdjustCost(mapSize), this.HP, this.HP);
        }

        internal double AdjustCost(int mapSize)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(mapSize);
            double cost = GetTotCost() - this.Upkeep * upkeepPayoff;
            cost = cost + ( cost - this.Cost ) / Consts.ScalePct(1, Consts.RepairCostMult, GetNonColonyPct());
            if (cost < upkeepPayoff * Consts.MinCostMult)
                throw new Exception();
            return cost;
        }

        private double GetNonColonyPct()
        {
            return Consts.GetNonColonyPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamageMult, this.Research);
        }

        private double GetNonTransPct()
        {
            return Consts.GetNonTransPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamageMult, this.Research);
        }

        public override string GetProdText(string curProd)
        {
            return curProd + " / " + this.Cost.ToString();
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
