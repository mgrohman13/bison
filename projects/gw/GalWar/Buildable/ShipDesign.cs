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

        public static double GetPlanetDefenseStrength(int stat1, int stat2)
        {
            return GetPlanetDefenseCost(stat1, stat2, 0);
        }

        public static double GetValue(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, double research)
        {
            double researchMult = GetResearchMult(research);
            return GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, researchMult, 1 / researchMult);
        }

        public static double GetPlanetDefenseCost(double stat1, double stat2, double research)
        {
            double researchMult = GetResearchMult(research);
            return GetTotCost(stat1, stat2, 1, -1, 0, false, 0, researchMult, researchMult) * Consts.PlanetDefensesCostMult;
        }

        public static double GetTotCost(int att, int def, int hp, int speed, double trans, bool colony, double bombardDamage, double research)
        {
            double researchMult = GetResearchMult(research);
            return GetTotCost(att, def, hp, speed, trans, colony, bombardDamage, researchMult, researchMult);
        }

        private static double GetTotCost(double att, double def, double hp, double speed, double trans, bool colony, double bombardDamage, double statResearchMult, double totalResearchMult)
        {
            const double SpeedAdd = 2.1, SpeedPow = 1.3, AttDiv = 5.2;
            //expect 169% attacking soldiers on average
            const double AvgAttSoldiers = 1 + 1.69;

            double speedValue = Math.Pow(speed, SpeedPow) + SpeedAdd;
            if (speed < 1)
                speedValue = AttDiv;

            double statMult = statResearchMult * hp;
            double attValue = GetStatValue(att) * statMult * speedValue / AttDiv;
            double defValue = GetStatValue(def) * statMult;

            if (speed < 1)
            {
                speed = 1;
                speedValue = SpeedAdd;
            }

            return Consts.CostMult * totalResearchMult * (
                (
                    ( attValue + defValue )
                    +
                    ( colony ? 520 : 0 )
                    +
                    (
                        ( Math.Pow(trans * AvgAttSoldiers, 1 + Consts.AttackNumbersPower) / AvgAttSoldiers )
                        +
                        ( 13 * bombardDamage )
                    )
                    *
                    ( speed )
                    *
                    ( ( attValue / 5.2 + defValue + 65000 ) / 16900.0 )
                )
                *
                ( speedValue )
                +
                ( 104 * ( Math.Pow(1.69, speed - 1) - 1 ) )
            );
        }

        public static double GetStatValue(double stat)
        {
            double sqr = stat * stat;
            return ( sqr * stat + 6.0 * sqr + 2.0 * stat ) / 9.0;
        }

        private static double GetResearchMult(double research)
        {
            return Consts.ResearchFactor / ( research + Consts.ResearchFactor );
        }

        #endregion //cost

        #region fields and constructors

        public const double DeathStarAvg = 91;
        private const double DeathStarMin = 7.8, FocusCostMult = 1.69, FocusUpkeepMult = 1.3, FocusAttMult = 2.1, FocusSpeedMult = 1.3;
        //note - cannot go much higher than 2.1, due to current CreateType logic overflow 
        private const double FocusTypeMult = 2.1;

        [NonSerialized]
        private readonly bool _statsNotInit = true, _costNotInit = true;

        private readonly bool _colony;
        private readonly byte _name, _mark, _att, _def, _speed, _upkeep;
        private readonly ushort _cost, _hp, _trans, _bombardDamage;
        private readonly uint _research;

        internal static List<ShipDesign> GetStartDesigns(Player player, List<int> research)
        {
            List<ShipDesign> retVal = new List<ShipDesign>(3);

            //randomize which of the starting research values go to which design type
            int idx = -1;
            foreach (int type in Game.Random.Iterate(3))
            {
                ShipDesign design = new ShipDesign(player, false, research[++idx],
                        ( type == 0 ), ( type == 1 ), ( type == 2 ), double.NaN, double.NaN);
                retVal.Add(design);
            }

            return retVal;
        }

        internal ShipDesign(Player player, bool useFocus, int research)
            : this(player, useFocus, research, double.NaN, double.NaN)
        {
            checked
            {
            }
        }
        internal ShipDesign(Player player, int research, double minCost, double maxCost)
            : this(player, false, research, minCost, maxCost)
        {
            checked
            {
            }
        }

        private ShipDesign(Player player, bool useFocus, int research, double minCost, double maxCost)
            : this(player, useFocus, research, false, false, false, minCost, maxCost)
        {
            checked
            {
            }
        }
        private ShipDesign(Player player, bool useFocus, int research, bool forceColony, bool forceTrans, bool forceNeither, double minCost, double maxCost)
        {
            checked
            {
                double mapSize = player.Game.MapSize;

                ICollection<ShipDesign> designs = null;
                FocusStat focus = FocusStat.None;
                ShipDesign design = null;
                if (useFocus)
                {
                    design = player.ResearchFocusDesign;
                    if (design == null)
                    {
                        designs = player.GetShipDesigns();
                        focus = player.ResearchFocus;
                    }
                }

                bool anomalyShip = !double.IsNaN(minCost);

                this._research = (ushort)research;
                do
                {
                    this._statsNotInit = true;
                    this._costNotInit = true;

                    double upkeepPct, hpMult;

                    bool colony;
                    int att, def, hp, speed, trans, bombardDamage;

                    if (design == null)
                        NewDesign(mapSize, research, designs, focus, forceColony, forceTrans, forceNeither,
                                out upkeepPct, out hpMult, out att, out def, out hp, out speed, out colony, out trans, out bombardDamage);
                    else
                        UpgradeDesign(mapSize, research, design,
                                out upkeepPct, out hpMult, out att, out def, out hp, out speed, out colony, out trans, out bombardDamage);

                    this._colony = colony;
                    this._trans = (ushort)( trans );
                    this._att = (byte)att;
                    this._def = (byte)def;
                    this._hp = (ushort)hp;
                    this._speed = (byte)speed;
                    this._bombardDamage = (ushort)bombardDamage;
                    this._statsNotInit = false;

                    if (double.IsNaN(minCost))
                    {
                        minCost = GetMinCost(mapSize, research) + MinCostBuffer;
                        maxCost = GetMaxCost(research, minCost);
                    }
                    else
                    {
                        double absMin = GetAbsMinCost(mapSize, research) + MinCostBuffer;
                        if (minCost < absMin)
                            minCost = absMin;
                    }

                    //  ------  Cost/Upkeep       ------
                    double cost = -1, upkRnd = double.NaN;
                    int upkeep = -1;
                    GetCost(mapSize, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                    while (cost > maxCost)
                    {
                        switch (GetReduce(cost, hpMult, forceColony, forceTrans))
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
                        case ModifyStat.DS:
                            this._bombardDamage = (ushort)SetBombardDamage(this.BombardDamage - 1, this.Att, false);
                            break;
                        case ModifyStat.Colony:
                            this._colony = false;
                            break;
                        case ModifyStat.None:
                            maxCost = cost;
                            break;
                        default:
                            throw new Exception();
                        }
                        GetCost(mapSize, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                    }
                    while (cost < minCost)
                    {
                        switch (GetIncrease(hpMult))
                        {
                        case ModifyStat.Att:
                            ++this._att;
                            this._bombardDamage = (ushort)SetBombardDamage(this._bombardDamage, this.Att, this.DeathStar);
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
                        GetCost(mapSize, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                    }
                    this._cost = (ushort)Game.Random.Round(cost);
                    this._upkeep = (byte)upkeep;
                    this._costNotInit = false;
                } while (design != null && !this.MakesObsolete(mapSize, design));

                //  ------  Name   
                this._name = (byte)player.Game.ShipNames.GetName(this, GetAttDefStr(Research), GetTransStr(Research), GetSpeedStr(Research), anomalyShip);
                this._mark = (byte)player.Game.ShipNames.GetMark(player, this.Name);
            }
        }

        private static void NewDesign(double mapSize, int research, ICollection<ShipDesign> designs, FocusStat focus, bool forceColony, bool forceTrans, bool forceNeither,
                out double upkeepPct, out double hpMult, out int att, out int def, out int hp,
                out int speed, out bool colony, out int trans, out int bombardDamage)
        {
            //existing designs change probabilities for new research
            double colonyPct, attPct, speedPct, transPct, dsPct;
            GetPcts(designs, mapSize, research, out colonyPct, out upkeepPct, out attPct, out speedPct, out transPct, out dsPct);

            //  ------  Colony/Trans/DS   ------
            double transStr = GetTransStr(research), bombardDamageMult;
            DoColonyTransDS(forceColony, forceTrans, forceNeither, research, colonyPct, transPct, dsPct,
                    ref transStr, out colony, out trans, out bombardDamageMult, focus);
            bool deathStar = ( bombardDamageMult > 0 );
            trans = (ushort)trans;

            //  ------  Att/Def           ------
            //being a colony ship/transport/death star makes att and def lower
            double strMult = GetAttDefStrMult(transStr, bombardDamageMult, colony, trans);
            double str = GetAttDefStr(research, strMult, focus);
            DoAttDef(str, attPct, ( colony || trans > Game.Random.GaussianOEInt(transStr * .52, .52, .26) ), out att, out def, focus);
            att = (byte)att;
            def = (byte)def;

            //  ------  HP                ------
            //being a colony ship/transport/death star makes hp higher
            hpMult = GetHPMult(strMult, deathStar, colony);
            //hp is relative to actual randomized stats
            hp = (ushort)MakeStat(GetHPStr(att, def, hpMult));

            //  ------  Speed             ------
            double speedStr = MultStr(ModSpeedStr(GetSpeedStr(research), transStr, deathStar, colony, trans, true),
                    GetSpeedMult(str, hpMult, speedPct, att, def, hp, focus));
            speed = (byte)MakeStat(speedStr);

            //  ------  BombardDamage     ------
            //modify bombard mult based on speed and att
            if (deathStar)
                bombardDamage = (ushort)SetBombardDamage(Game.Random.Round(bombardDamageMult * Consts.GetBombardDamage(att)
                        * Math.Sqrt(speedStr / (double)speed * Math.Sqrt(str * def) / (double)att)), att, true);
            else
                bombardDamage = 0;
        }

        private static void UpgradeDesign(double mapSize, int research, ShipDesign design,
                out double upkeepPct, out double hpMult, out int att, out int def, out int hp,
                out int speed, out bool colony, out int trans, out int bombardDamage)
        {
            upkeepPct = ( design.Cost / (double)design.Upkeep / design.GetUpkeepPayoff(mapSize, research) + 1 ) * Consts.CostUpkeepPct;
            hpMult = design.HP / GetHPStr(design.Att, design.Def, 1);

            double attStr = design.Att;
            double defStr = design.Def;
            double hpStr = design.HP;
            double speedStr = design.Speed;
            bool colonyStr = design.Colony;
            double transStr = design.Trans;
            bool deathStarStr = design.DeathStar;
            double bombardDamageStr = design.BombardDamage;

            if (Game.Random.Bool())
            {
                double attDefStr = GetAttDefStr(research);
                if (Game.Random.Bool())
                    attStr += Game.Random.Range(0, 1 + attDefStr);
                if (Game.Random.Bool())
                    defStr += Game.Random.Range(0, 1 + attDefStr);
                if (Game.Random.Bool())
                    hpStr += Game.Random.Range(0, 1 + GetHPStr(attDefStr, attDefStr));
                if (Game.Random.Bool())
                {
                    if (Game.Random.Bool())
                        speedStr += Game.Random.Range(0, 1 + GetSpeedStr(research));
                    if (Game.Random.Bool() && ( deathStarStr || transStr > 0 || Game.Random.Bool() ))
                    {
                        if (!deathStarStr)
                            if (transStr > 0 || Game.Random.Bool())
                                transStr += Game.Random.Range(1, GetTransStr(research));
                            else
                                deathStarStr = true;
                        bombardDamageStr += Game.Random.Range(1, Consts.GetBombardDamage(attDefStr) * DeathStarAvg);
                    }
                    colonyStr |= ( transStr > 0 && Game.Random.Bool() );
                }
            }

            att = (byte)MakeStat(attStr);
            def = (byte)MakeStat(defStr);
            hp = (ushort)MakeStat(hpStr);
            speed = (byte)MakeStat(speedStr);
            colony = colonyStr;
            trans = (ushort)( transStr > 0 ? MakeStat(transStr) : 0 );
            bombardDamage = (ushort)( deathStarStr ? MakeStat(bombardDamageStr) : 0 );
        }

        public bool Colony
        {
            get
            {
                CheckInit();
                return this._colony;
            }
        }

        internal int Name
        {
            get
            {
                return this._name;
            }
        }
        internal int Mark
        {
            get
            {
                return this._mark;
            }
        }

        public int Att
        {
            get
            {
                CheckInit();
                return this._att;
            }
        }
        public int Def
        {
            get
            {
                CheckInit();
                return this._def;
            }
        }

        public int Speed
        {
            get
            {
                CheckInit();
                return this._speed;
            }
        }

        public int Upkeep
        {
            get
            {
                CheckInit(true);
                return this._upkeep;
            }
        }

        public override int Cost
        {
            get
            {
                CheckInit(true);
                return this._cost;
            }
        }

        public int HP
        {
            get
            {
                CheckInit();
                return this._hp;
            }
        }

        public int Trans
        {
            get
            {
                CheckInit();
                return this._trans;
            }
        }

        public double BombardDamage
        {
            get
            {
                CheckInit();
                return GetBombardDamage(this._bombardDamage, this.Att);
            }
        }
        public bool DeathStar
        {
            get
            {
                CheckInit();
                return ( this._bombardDamage > 0 );
            }
        }

        public int Research
        {
            get
            {
                CheckInit();
                return (int)this._research;
            }
        }

        private void CheckInit()
        {
            CheckInit(false);
        }
        private void CheckInit(bool cost)
        {
            if (cost ? this._costNotInit : this._statsNotInit)
                throw new Exception();
        }

        private static void GetPcts(ICollection<ShipDesign> designs, double mapSize, int research,
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
                    double upkeepPayoff = design.GetUpkeepPayoff(mapSize, research);
                    double totalCost = design.Cost + design.Upkeep * upkeepPayoff;

                    upkeep *= design.Upkeep / ( totalCost / upkeepPayoff * Consts.CostUpkeepPct );
                    att *= design.Att / (double)design.Def;
                    speed *= design.Speed / ModSpeedStr(GetSpeedStr(design.Research), GetTransStr(design.Research), design.DeathStar, design.Colony, design.Trans, false);

                    costMult += totalCost;
                    double mult = design.Research + 260;
                    mult = ( mult + ( Math.Pow(mult, .169) - 2.6 ) * 1300 ) / (double)research / totalCost;

                    if (design.Colony)
                        colony += mult;
                    trans += design.Speed * design.Trans * mult;
                    ds += design.Speed * design.BombardDamage * mult;
                }

                double pow = 1.0 / (double)numDesigns;
                upkeep = Math.Pow(upkeep, pow);
                att = Math.Pow(att, pow);
                speed = Math.Pow(speed, pow);

                costMult /= (double)numDesigns * numDesigns;
                colony *= costMult;
                trans *= costMult;
                ds *= costMult;
                double speedStr = GetSpeedStr(research);
                trans /= speedStr * GetTransStr(research);
                ds /= speedStr * Consts.GetBombardDamage(GetAttDefStr(research)) * DeathStarAvg;
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
            return Game.Random.GaussianOEInt(str, .91, .13, 1);
        }
        private static double MakeDeathStar()
        {
            return Game.Random.GaussianOE(DeathStarAvg, .91, .13, DeathStarMin);
        }

        #endregion //fields and constructors

        #region Colony/Trans

        public static double GetTransStr(double research)
        {
            return MakeStatStr(research, 16.9, .65);
        }

        public static void DoColonyTransDS(bool forceColony, bool forceTrans, bool forceNeither, int research,
                ref double transStr, out bool colony, out int trans, out double bombardDamageMult)
        {
            int t;
            DoColonyTransDS(forceColony, forceTrans, forceNeither, research, double.NaN, double.NaN, double.NaN,
                    ref transStr, out colony, out t, out bombardDamageMult, FocusStat.None);
            trans = t;
        }
        private static void DoColonyTransDS(bool forceColony, bool forceTrans, bool forceNeither, int research, double colonyPct, double transPct, double dsPct,
                ref double transStr, out bool colony, out int trans, out double bombardDamageMult, FocusStat focus)
        {
            double transTrg = .169;
            double colTrg = .104;
            //target pct of ships that should be death stars increases with research
            double dsTrg = research / ( 3000.0 + research );
            dsTrg *= dsTrg * dsTrg * .169;

            if (IsFocusing(focus, FocusStat.Colony))
                FocusPcts(ref colTrg, ref transTrg, ref dsTrg);
            else if (IsFocusing(focus, FocusStat.DS))
                FocusPcts(ref dsTrg, ref colTrg, ref transTrg);
            else if (IsFocusing(focus, FocusStat.Trans))
                FocusPcts(ref transTrg, ref colTrg, ref dsTrg);

            bool transport;
            if (forceTrans)
                transport = true;
            else if (forceNeither || forceColony)
                transport = false;
            else
                transport = CreateType(transTrg, transPct);

            if (forceColony)
                colony = true;
            else if (forceNeither || forceTrans)
                colony = false;
            else
                colony = CreateType(colTrg, colonyPct);

            //pure colony ships transport a reduced amount
            if (colony && !transport)
                transStr = GetColTransStr(transStr);

            bombardDamageMult = 0;
            if (colony || transport)
            {
                trans = MakeStat(transStr);
            }
            else
            {
                trans = 0;

                if (!forceNeither && CreateType(dsTrg, dsPct))
                    bombardDamageMult = MakeDeathStar();
            }
        }

        private static void FocusPcts(ref double focus, ref double oth1, ref double oth2)
        {
            focus *= FocusTypeMult;
            oth1 /= FocusTypeMult;
            oth2 /= FocusTypeMult;
        }
        private static bool CreateType(double target, double actual)
        {
            double chance;
            //chance is higher when target > actual and lower when target < actual
            if (double.IsNaN(actual))
                chance = target;
            else if (target > actual)
                chance = Math.Sqrt(target - actual) + target;
            else
                chance = ( 1 + ( target - actual ) / actual ) * target;
            return Game.Random.Bool(chance);
        }

        private static double GetColTransStr(double transStr)
        {
            return MultStr(transStr, .39);
        }

        #endregion //Colony/Trans

        #region Att/Def

        private static double GetAttDefStrMult(double transStr, double bombardDamageMult, bool colony, double trans)
        {
            double strMultOffset = 2.1 * transStr;
            return strMultOffset / ( strMultOffset + ( colony ? 65 : 0 ) + trans ) * 390 / ( 390 + bombardDamageMult );
        }

        public static double GetAttDefStr(double research)
        {
            return GetAttDefStr(research, 1, FocusStat.None);
        }

        private static double GetAttDefStr(double research, double strMult, FocusStat focus)
        {
            if (IsFocusing(focus, FocusStat.Cost))
                strMult *= FocusCostMult;
            return MakeStatStr(research, 2.1 * strMult, .585);
        }

        private static void DoAttDef(double str, double attPct, bool defensive, out int att, out int def, FocusStat focus)
        {
            if (IsFocusing(focus, FocusStat.Speed))
                str /= FocusSpeedMult;

            int s1;
            int s2;
            MakeAttDef(str, out s1, out s2);

            attPct = Math.Sqrt(attPct);
            if (IsFocusing(focus, FocusStat.Att))
                attPct /= FocusAttMult;
            else if (IsFocusing(focus, FocusStat.Def))
                attPct *= FocusAttMult;

            //colony ships and transports are more likely to be defensive
            double chance = ( defensive ? .26 : .65 );
            if (attPct < 1)
                chance = ( 1 - ( ( 1 - chance ) * attPct ) );
            else
                chance /= attPct;

            if (Game.Random.Bool(chance))
            {
                att = s1;
                def = s2;
            }
            else
            {
                att = s2;
                def = s1;
            }

            if (IsFocusing(focus, FocusStat.Att))
                CheckFocusStat(str, ref att, ref def);
            else if (IsFocusing(focus, FocusStat.Def))
                CheckFocusStat(str, ref def, ref att);
        }
        private static void CheckFocusStat(double str, ref int s1, ref int s2)
        {
            if (s1 >= s2)
                while (s1 < Game.Random.GaussianOEInt(s2 * FocusAttMult, .26, .078))
                    MakeAttDef(str, out s1, out s2);
        }
        private static void MakeAttDef(double str, out int s1, out int s2)
        {
            s1 = MakeStat(str);
            //second stat is adjusted to compensate for the first
            s2 = MakeStat(MultStr(str, Math.Sqrt(str / (double)s1)));
            if (s2 > s1)
            {
                int temp = s1;
                s1 = s2;
                s2 = temp;
            }
        }

        #endregion //Att/Def

        #region Speed

        public static double GetSpeedStr(int research)
        {
            return MakeStatStr(research, .65, .39);
        }

        private static double ModSpeedStr(double speedStr, double transStr, bool deathStar, bool colony, double trans, bool doColAndTrans)
        {
            if (colony)
                speedStr = MultStr(speedStr, .91);
            else if (deathStar)
                speedStr = MultStr(speedStr, .65);
            if (( doColAndTrans || !colony ) && trans > 0)
            {
                double transFactor = transStr / trans;
                if (transFactor < 1)
                    speedStr = MultStr(speedStr, Math.Pow(transFactor, .26));
            }
            return speedStr;
        }

        private static double GetSpeedMult(double str, double hpMult, double speedPct, double att, double def, double hp, FocusStat focus)
        {
            double focusFactor = 1;
            if (IsFocusing(focus, FocusStat.Speed))
                focusFactor = FocusSpeedMult;
            //speed is higher for more offensive and weaker ships
            double offenseFactor = att / def;
            double strengthFactor = 2 * GetStatValue(str) * MultStr(4 * str * str, hpMult)
                    / ( ( GetStatValue(att) + GetStatValue(def) ) * hp );
            return Math.Pow(offenseFactor * strengthFactor / speedPct, .21) * focusFactor;
        }

        #endregion //Speed

        #region Cost/Upkeep

        private double GetMinCost(double mapSize, int research)
        {
            //random increase to absolute minimum
            double minCost = GetAbsMinCost(mapSize, research);
            return Game.Random.GaussianOE(minCost * 1.3, .13, .039, minCost);
        }
        private const double MinCostBuffer = 2.1;
        private double GetAbsMinCost(double mapSize, int research)
        {
            return this.GetUpkeepPayoff(mapSize, research) * Consts.MinCostMult + MinCostBuffer;
        }
        private static double GetMaxCost(int research, double minCost)
        {
            //max cost is more of a guideline than actual rule
            double maxCost = Math.Pow(research, Consts.MaxCostPower) * Consts.MaxCostMult;
            double min = minCost * 1.3;
            if (maxCost > min)
                return Game.Random.GaussianOE(maxCost, .169, 1.3, min);
            return min;
        }

        private void GetCost(double mapSize, double upkeepPct, out double cost, out int upkeep, ref double upkRnd, FocusStat focus)
        {
            double upkeepPayoff = this.GetOnResearchUpkeepPayoff(mapSize);
            double totCost = GetTotCost();

            if (double.IsNaN(upkRnd))
            {
                //calculating for the first time, so randomize upkeep
                double avgUpk = totCost / upkeepPayoff * Consts.CostUpkeepPct / Math.Sqrt(upkeepPct);

                if (IsFocusing(focus, FocusStat.Upkeep))
                    avgUpk *= FocusUpkeepMult;
                else if (IsFocusing(focus, FocusStat.Cost))
                    avgUpk /= FocusUpkeepMult;

                if (avgUpk > 1)
                    upkeep = Game.Random.GaussianCappedInt(avgUpk, 1.3, 1);
                else
                    upkeep = 1;
            }
            else
            {
                //cost has been previously calculated, so maintain upkeep pct
                double avgUpk = totCost * upkRnd;
                if (avgUpk > 1)
                    upkeep = Game.Random.Round(avgUpk);
                else
                    upkeep = 1;
            }

            //upkeep should never account for more than half of the ship's cost
            while (upkeep > 1 && upkeep * upkeepPayoff > totCost / 2.0)
                --upkeep;

            cost = ( totCost - upkeep * upkeepPayoff );
            upkRnd = ( upkeep / totCost );
        }

        private double GetTotCost()
        {
            return GetTotCost(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, this.Research);
        }

        private ModifyStat GetReduce(double cost, double hpMult, bool forceColony, bool forceTrans)
        {
            Dictionary<ModifyStat, int> stats = new Dictionary<ModifyStat, int>();

            stats.Add(ModifyStat.Att, ReduceAttDef(this.Att, this.Def, hpMult));
            stats.Add(ModifyStat.Def, ReduceAttDef(this.Def, this.Att, hpMult));
            stats.Add(ModifyStat.HP, ( this.HP > 1 ? this.HP : 0 ));
            if (this.Speed > 1)
                stats.Add(ModifyStat.Speed, Game.Random.Round(this.Speed * .39));
            if (this.Trans > ( forceTrans || this.Colony ? 1 : 0 ))
                stats.Add(ModifyStat.Trans, Game.Random.Round(this.Trans * 1.3 + .52));
            if (this.DeathStar)
                stats.Add(ModifyStat.DS, Game.Random.Round(this.BombardDamage * 2.1));

            int total = 0;
            foreach (int value in stats.Values)
                total += value;
            if (this.Colony && !forceColony)
            {
                int colony = Game.Random.Round(Math.Sqrt(total) / 13.0);
                total += colony;
                stats.Add(ModifyStat.Colony, colony);
            }

            if (total == 0)
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
                    hpStr = GetStatChance(hpStr, stat, other);
                return Game.Random.Round(hpStr);
            }
            return 0;
        }

        internal static Dictionary<Ship.ExpType, int> IncreaseAttDef(int att, int def, int hp, out int total)
        {
            Dictionary<Ship.ExpType, int> stats = new Dictionary<Ship.ExpType, int>();
            total = 0;
            Dictionary<ModifyStat, int> modifyStats = BalanceAttDef(att, def, hp, Consts.BaseDesignHPMult);
            foreach (ModifyStat stat in modifyStats.Keys)
            {
                int amt = modifyStats[stat];
                stats[(Ship.ExpType)Enum.Parse(typeof(Ship.ExpType), stat.ToString())] = amt;
                total += amt;
            }
            return stats;
        }

        private ModifyStat GetIncrease(double hpMult)
        {
            Dictionary<ModifyStat, int> stats = BalanceAttDef(this.Att, this.Def, this.HP, hpMult);
            return Game.Random.SelectValue<ModifyStat>(stats);
        }

        private static Dictionary<ModifyStat, int> BalanceAttDef(int att, int def, int hp, double hpMult)
        {
            Dictionary<ModifyStat, int> stats = new Dictionary<ModifyStat, int>();
            stats.Add(ModifyStat.Att, IncreaseAttDef(att, def, hp, hpMult));
            stats.Add(ModifyStat.Def, IncreaseAttDef(def, att, hp, hpMult));
            stats.Add(ModifyStat.HP, Game.Random.Round(byte.MaxValue * GetHPStr(att, def, hpMult)));
            return stats;
        }

        private static int IncreaseAttDef(int stat, int other, int hp, double hpMult)
        {
            return Game.Random.Round(byte.MaxValue * GetStatChance(hp, stat, other) / ( GetHPStr(stat + 1, other, hpMult) - GetHPStr(stat, other, hpMult) ));
        }

        private static double GetStatChance(double mult, int stat, int other)
        {
            return mult * stat / (double)( stat + other );
        }

        internal static double SetBombardDamage(double bombardDamage, int att)
        {
            return SetBombardDamage(bombardDamage, att, true);
        }
        private static double SetBombardDamage(double bombardDamage, int att, bool keepDeathStar)
        {
            int minDamage = GetDeathStarMin(att);
            if (bombardDamage < minDamage)
                if (keepDeathStar)
                    bombardDamage = minDamage;
                else
                    bombardDamage = 0;
            if (bombardDamage != (ushort)bombardDamage)
                throw new Exception();
            return bombardDamage;
        }
        internal static double GetBombardDamage(ushort bombardDamage, int att)
        {
            if (bombardDamage > 0)
                if (bombardDamage < GetDeathStarMin(att))
                    throw new Exception();
                else
                    return bombardDamage;
            else
                return Consts.GetBombardDamage(att);
        }
        private static int GetDeathStarMin(int att)
        {
            return (int)Math.Ceiling(DeathStarMin * Consts.GetBombardDamage(att));
        }

        #endregion //Cost/Upkeep

        #region internal

        internal static double GetColonizationValue(double cost, int att, int def, double curHP, int maxHP,
                int speed, int trans, bool colony, double bombardDamage, int lastResearched)
        {
            if (!colony)
                throw new Exception();
            if (curHP > maxHP)
                curHP = maxHP;
            return cost * curHP / (double)maxHP * Consts.GetNonColonyPct(att, def, maxHP, speed, trans, colony, bombardDamage, lastResearched, false);
        }

        private double GetOnResearchUpkeepPayoff(double mapSize)
        {
            return GetUpkeepPayoff(mapSize, this.Research);
        }
        public double GetUpkeepPayoff(double mapSize, int lastResearched)
        {
            return Consts.GetUpkeepPayoff(mapSize, GetNonColonyPct(lastResearched), GetNonTransPct(lastResearched), this.Speed);
        }

        internal HashSet<ShipDesign> GetObsolete(double mapSize, IEnumerable<ShipDesign> designs)
        {
            HashSet<ShipDesign> retVal = new HashSet<ShipDesign>();
            foreach (ShipDesign design in designs)
                if (MakesObsolete(mapSize, design))
                    retVal.Add(design);
            return retVal;
        }
        internal bool MakesObsolete(double mapSize, ShipDesign oldDesign)
        {
            if (this.Research <= oldDesign.Research)
                return false;

            double totCost = this.Cost + this.Upkeep * this.GetOnResearchUpkeepPayoff(mapSize);
            double oldTotCost = oldDesign.Cost + oldDesign.Upkeep * oldDesign.GetUpkeepPayoff(mapSize, this.Research);

            double attStr = GetStatValue(this.Att) * this.HP;
            double oldAttStr = GetStatValue(oldDesign.Att) * oldDesign.HP;

            double defStr = GetStatValue(this.Def) * this.HP;
            double oldDefStr = GetStatValue(oldDesign.Def) * oldDesign.HP;

            double transStr = this.Trans * this.Speed;
            double oldTransStr = oldDesign.Trans * oldDesign.Speed;

            double colonyStr = ( this.Colony ? 1 : 0 );
            double oldColonyStr = ( oldDesign.Colony ? 1 : 0 );

            double deathStr = this.BombardDamage * this.Speed;
            double oldDeathStr = oldDesign.BombardDamage * oldDesign.Speed;

            return (
                //must be at least as fast, and either
                CompareForObsolete(this.Speed, oldDesign.Speed) &&
                //be better in each stat category and have a lower cost and upkeep, or
                ( ( CompareForObsolete(attStr, oldAttStr) && CompareForObsolete(defStr, oldDefStr) && CompareForObsolete(transStr, oldTransStr) &&
                CompareForObsolete(colonyStr, oldColonyStr) && CompareForObsolete(deathStr, oldDeathStr) &&
                CompareForObsolete(oldDesign.Cost, this.Cost) && CompareForObsolete(oldDesign.Upkeep, this.Upkeep) ) ||
                //have a better value per total cost in each category and a similar cost and upkeep
                ( CompareForObsolete(attStr / totCost, oldAttStr / oldTotCost) && CompareForObsolete(defStr / totCost, oldDefStr / oldTotCost) &&
                CompareForObsolete(transStr / totCost, oldTransStr / oldTotCost) && CompareForObsolete(colonyStr / totCost, oldColonyStr / oldTotCost) &&
                CompareForObsolete(deathStr / totCost, oldDeathStr / oldTotCost) &&
                ObsoleteCost(this.Cost, oldDesign.Cost, this.Upkeep, oldDesign.Upkeep, this.Research, oldDesign.Research) ) )
            );
        }
        private bool CompareForObsolete(double s1, double s2)
        {
            return ( s2 == 0 || ( s2 - s1 ) / ( s2 + s1 ) < Game.Random.Weighted(.21, .26) );
        }
        private bool ObsoleteCost(double c1, double c2, double u1, double u2, double r1, double r2)
        {
            double c = Math.Min(c1, c2) / Math.Max(c1, c2);
            double u = Math.Min(u1, u2) / Math.Max(u1, u2);
            return Game.Random.Bool(Math.Pow(c * c * c * c * c * u * u * u, 780 / ( 260 + r1 - r2 )));
        }

        #endregion //internal

        #region Buildable

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

        internal override void Build(IEventHandler handler, Colony colony, Tile tile)
        {
            Ship ship = colony.Player.NewShip(handler, tile, this);

            int max = Math.Min(colony.AvailablePop, ship.FreeSpace);
            if (max > 0)
            {
                max = handler.MoveTroops(colony, max, colony.Population, colony.Soldiers);
                if (max > 0)
                {
                    colony.MovePop(handler, max, ship);
                    //troops can be moved again next turn
                    ship.ResetMoved();
                }
            }
        }

        internal override bool CanBeBuiltBy(Colony colony)
        {
            return colony.Player.GetDesigns().Contains(this);
        }

        public override string GetProdText(string curProd)
        {
            return curProd + " / " + this.Cost.ToString();
        }

        public override string ToString()
        {
            return ShipNames.GetName(this.Name, this.Mark);
        }

        #endregion //Buildable

        #region public

        public double GetStrength()
        {
            return GetStrength(this.Att, this.Def, this.HP, this.Speed);
        }

        private static double GetHPMult(double strMult, bool deathStar, bool colony)
        {
            return Consts.BaseDesignHPMult / Math.Pow(strMult, deathStar ? 1.69 : ( colony ? 2.1 : Math.PI / 1.3 ));
        }

        public static double GetHPStr(double s1, double s2)
        {
            return GetHPStr(s1, s2, Consts.BaseDesignHPMult);
        }
        private static double GetHPStr(double s1, double s2, double hpMult)
        {
            return MultStr(( s1 + s2 ) * ( s1 + s2 ), hpMult);
        }

        public double GetColonizationValue(double mapSize, int lastResearched)
        {
            return GetColonizationValue(AdjustCost(mapSize, lastResearched), this.Att, this.Def, this.HP, this.HP,
                    this.Speed, this.Trans, this.Colony, this.BombardDamage, lastResearched);
        }

        internal double AdjustCost(double mapSize, int lastResearched)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(mapSize, lastResearched);
            double cost = GetTotCost() - this.Upkeep * upkeepPayoff;
            cost += ( cost - this.Cost ) / Consts.ScalePct(1, Consts.RepairCostMult, GetNonColonyPct(lastResearched));
            if (cost < GetAbsMinCost(mapSize, lastResearched))
                throw new Exception();
            return cost;
        }

        private double GetNonColonyPct(int lastResearched)
        {
            return Consts.GetNonColonyPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, lastResearched, true);
        }
        private double GetNonTransPct(int lastResearched)
        {
            return Consts.GetNonTransPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, lastResearched, true);
        }

        #endregion //public

        #region enum

        internal static bool IsFocusing(FocusStat focus, FocusStat check)
        {
            return ( ( focus & check ) == check );
        }

        private enum ModifyStat
        {
            Att,
            Def,
            HP,
            Speed,
            Colony,
            Trans,
            DS,
            None,
        }

        public enum FocusStat
        {
            None = 0x00,
            Trans = 0x04,
            DS = 0x06,
            Colony = 0x07,
            Def = 0x10,
            Att = 0x18,
            Speed = 0x20,
            Cost = 0x80,
            Upkeep = 0xC0,
        }

        #endregion //enum

        #region test

        public static void DoCostTable()
        {
            Console.WriteLine("research\t\tmax\t\treg\tcol\ttrans\tds\t\tRS\tCS\tTS\tDS");

            int research = 0;
            int max = Game.Random.GaussianOEInt(130000, .13, .13);
            while (( research = Game.Random.GaussianCappedInt(( research + 13 ) * 1.3, .13, research + 13) ) < max)
            {
                double str = GetAttDefStr(research);
                double hp = GetHPStr(str, str);
                double speed = GetSpeedStr(research);
                double trans = GetTransStr(research);
                double BD = Consts.GetBombardDamage(str);
                double rMult = GetResearchMult(research);

                double reg = GetTotCost(str, str, hp, speed, 0, false, BD, rMult, rMult);
                double regS = GetTotCost(str, str, hp, speed, 0, false, 0, rMult, rMult);

                double strMult = GetAttDefStrMult(trans, 0, false, trans);
                str = GetAttDefStr(research, strMult, FocusStat.None);
                double hpMult = GetHPMult(strMult, false, false);
                hp = GetHPStr(str, str, hpMult);

                double t = GetTotCost(str, str, hp, speed, trans, false, BD, rMult, rMult);
                double tS = GetTotCost(str, str, hp, speed, 0, false, 0, rMult, rMult);

                trans = GetColTransStr(trans);
                strMult = GetAttDefStrMult(trans, 0, true, trans);
                str = GetAttDefStr(research, strMult, FocusStat.None);
                hpMult = GetHPMult(strMult, false, true);
                hp = GetHPStr(str, str, hpMult);
                speed = ModSpeedStr(GetSpeedStr(research), trans, false, true, trans, true);

                double col = GetTotCost(str, str, hp, speed, trans, true, BD, rMult, rMult);
                double colS = GetTotCost(str, str, hp, speed, 0, false, 0, rMult, rMult);

                trans = GetTransStr(research);
                strMult = GetAttDefStrMult(trans, DeathStarAvg, false, 0);
                str = GetAttDefStr(research, strMult, FocusStat.None);
                hpMult = GetHPMult(strMult, true, false);
                hp = GetHPStr(str, str, hpMult);
                speed = ModSpeedStr(GetSpeedStr(research), trans, true, false, 0, true);
                BD *= DeathStarAvg;

                double ds = GetTotCost(str, str, hp, speed, 0, false, BD, rMult, rMult);
                double dsS = GetTotCost(str, str, hp, speed, 0, false, 0, rMult, rMult);

                double maxCost = Math.Pow(research, Consts.MaxCostPower) * Consts.MaxCostMult;

                Console.WriteLine(research + "\t\t" + maxCost + "\t\t" + reg + "\t" + col + "\t" + t + "\t" + ds
                        + "\t\t" + regS + "\t" + colS + "\t" + tS + "\t" + dsS);
            }
        }

        #endregion //test
    }
}
