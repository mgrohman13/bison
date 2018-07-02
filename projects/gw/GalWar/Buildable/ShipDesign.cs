using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class ShipDesign : IShipStats
    {
        #region cost

        public static double GetStrength(int att, int def, int hp, int speed)
        {
            return GetTotCost(att, def, hp, speed, 0, false, 0, 0);
        }

        public static double GetPlanetDefenseStrength(double stat1, double stat2)
        {
            return GetPlanetDefenseCost(stat1, stat2, 0);
        }

        public static double GetValue(int att, int def, int hp, int speed, int trans, bool colony, double bombardDamage, Game game)
        {
            double researchMult = GetResearchMult(game.AvgResearch);
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
            const double SpeedPow = 1.3, AttDiv = 5.2;

            double speedValue = Math.Pow(speed, SpeedPow) + SpeedAvg;
            if (speed < 1)
                speedValue = AttDiv;

            double statMult = statResearchMult * hp;
            double attValue = GetStatValue(att) * statMult * speedValue / AttDiv;
            double defValue = GetStatValue(def) * statMult;

            if (speed < 1)
            {
                speed = 1;
                speedValue = SpeedAvg;
            }

            return Consts.CostMult * totalResearchMult * (
                (
                    ( attValue + defValue )
                    +
                    ( colony ? 520 : 0 )
                    +
                    (
                        ( GetTransValue(trans) )
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

        private static double GetTransValue(double trans)
        {
            //expect 169% attacking soldiers on average
            const double AvgAttSoldiers = 1 + 1.69;
            return Math.Pow(trans * AvgAttSoldiers, 1 + Consts.InvadeNumbersPower) / AvgAttSoldiers;
        }

        public static double GetStatValue(double stat)
        {
            return ( 1.0 * stat * stat * stat + 6.0 * stat * stat + 2.0 * stat ) / 9.0;
        }

        private static double GetResearchMult(double research)
        {
            return Consts.ResearchFactor / ( research + Consts.ResearchFactor );
        }

        #endregion //cost

        #region fields and constructors

        public const double DeathStarAvg = 91, DeathStarMin = 7.8, SpeedAvg = 2.1;
        private const double FocusCostMult = 1.69, FocusUpkeepMult = 1.3, FocusAttMult = 2.1, FocusSpeedMult = 1.3, FocusTypeMult = 2.6;

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
                ICollection<ShipDesign> designs = null;
                FocusStat focus = FocusStat.None;
                ShipDesign design = null;
                if (useFocus)
                {
                    design = player.ResearchFocusDesign;
                    if (design == null)
                    {
                        designs = player.GetDesigns();
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
                        NewDesign(player.Game, research, designs, focus, forceColony, forceTrans, forceNeither,
                                out upkeepPct, out hpMult, out att, out def, out hp, out speed, out colony, out trans, out bombardDamage);
                    else
                        UpgradeDesign(player.Game, research, design,
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
                        minCost = GetMinCost(player.Game);
                        maxCost = GetMaxCost(research, minCost);
                    }
                    else
                    {
                        minCost = Math.Max(minCost, GetMinCost(player.Game));
                    }

                    //  ------  Cost/Upkeep       ------
                    double cost = -1, upkRnd = double.NaN;
                    int upkeep = -1;
                    GetCost(player.Game, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                    while (cost > maxCost)
                    {
                        ModifyStat ms = GetReduce(cost, hpMult, forceColony, forceTrans);
                        Console.WriteLine(string.Format("Reduce: {0} {1} {2}", ms, cost, maxCost));
                        switch (ms)
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
                        GetCost(player.Game, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                    }
                    while (cost < minCost)
                    {
                        ModifyStat ms = GetIncrease(hpMult);
                        Console.WriteLine(string.Format("Increase: {0} {1} {2}", ms, cost, minCost));
                        switch (ms)
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
                        GetCost(player.Game, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                    }
                    this._cost = (ushort)Game.Random.Round(cost);
                    this._upkeep = (byte)upkeep;
                    this._costNotInit = false;
                } while (design != null && !this.MakesObsolete(player.Game, design));

                //  ------  Name   
                this._name = (byte)player.Game.ShipNames.GetName(player.Game, this, GetAttDefStr(Research), GetTransStr(Research), GetSpeedStr(Research), anomalyShip);
                this._mark = (byte)player.Game.ShipNames.GetMark(player, this.Name);
            }
        }

        private static void NewDesign(Game game, int research, ICollection<ShipDesign> designs, FocusStat focus, bool forceColony, bool forceTrans, bool forceNeither,
                out double upkeepPct, out double hpMult, out int att, out int def, out int hp,
                out int speed, out bool colony, out int trans, out int bombardDamage)
        {
            //existing designs change probabilities for new research
            double colonyPct, attPct, speedPct, transPct, dsPct;
            GetPcts(designs, game, research, out upkeepPct, out speedPct, out attPct, out colonyPct, out transPct, out dsPct);

            //  ------  Colony/Trans/DS   ------
            double transStr = GetTransStr(research), bombardDamageMult;
            DoColonyTransDS(forceColony, forceTrans, forceNeither, research, colonyPct, transPct, dsPct,
                    ref transStr, out colony, out trans, out bombardDamageMult, focus);
            bool deathStar = ( bombardDamageMult > 0 );

            //  ------  Att/Def           ------
            //being a colony ship/transport/death star makes att and def lower
            double strMult = GetAttDefStrMult(transStr, bombardDamageMult, colony, trans);
            double str = GetAttDefStr(research, strMult, focus);
            DoAttDef(str, attPct, ( colony || trans > Game.Random.GaussianOEInt(transStr * .52, .52, .26) ), out att, out def, focus);

            //  ------  HP                ------
            //being a colony ship/transport/death star makes hp higher
            hpMult = GetHPMult(strMult, deathStar, colony);
            //hp is relative to actual randomized stats
            hp = MakeStat(GetHPStr(att, def, hpMult));

            //  ------  Speed             ------
            double speedStr = MultStr(ModSpeedStr(GetSpeedStr(research), transStr, deathStar, colony, trans),
                    GetSpeedMult(str, hpMult, speedPct, att, def, hp, focus));
            speed = MakeStat(speedStr);

            //  ------  BombardDamage     ------
            //modify bombard mult based on speed and att
            if (deathStar)
                bombardDamage = SetBombardDamage(Game.Random.Round(bombardDamageMult * Consts.GetBombardDamage(att)
                        * Math.Sqrt(speedStr / (double)speed * Math.Sqrt(str * def) / (double)att)), att, true);
            else
                bombardDamage = 0;
        }

        private static void UpgradeDesign(Game game, int research, ShipDesign design,
                out double upkeepPct, out double hpMult, out int att, out int def, out int hp,
                out int speed, out bool colony, out int trans, out int bombardDamage)
        {
            upkeepPct = ( design.Cost / (double)design.Upkeep / design.GetUpkeepPayoff(game) + 1 ) * Consts.CostUpkeepPct;
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
                    attStr += Game.Random.Range(0, attDefStr);
                if (Game.Random.Bool())
                    defStr += Game.Random.Range(0, attDefStr);
                if (Game.Random.Bool())
                    hpStr += Game.Random.Range(1, GetHPStr(attDefStr, attDefStr));
                if (Game.Random.Bool())
                {
                    if (Game.Random.Bool())
                        speedStr += Game.Random.Range(0, GetSpeedStr(research) - 1);
                    if (Game.Random.Bool() && ( deathStarStr || transStr > 0 || Game.Random.Bool() ))
                    {
                        if (!deathStarStr)
                            if (transStr > 0 || Game.Random.Bool())
                                transStr += Game.Random.Range(1, GetTransStr(research));
                            else
                                deathStarStr = true;
                        bombardDamageStr += Game.Random.Range(1, GetDsStr(attDefStr));
                        if (!colonyStr)
                            colonyStr = ( transStr > 0 && Game.Random.Bool() );
                    }
                }
            }

            att = MakeStat(attStr);
            def = MakeStat(defStr);
            hp = MakeStat(hpStr);
            speed = MakeStat(speedStr);
            colony = colonyStr;
            trans = ( transStr > 0 ? MakeStat(transStr) : 0 );
            bombardDamage = ( deathStarStr ? SetBombardDamage(MakeStat(bombardDamageStr), att) : 0 );
        }

        private static double GetDsResearchStr(int research)
        {
            return GetDsStr(GetAttDefStr(research));
        }

        private static double GetDsStr(double attDefStr)
        {
            return DeathStarAvg * Consts.GetBombardDamage(attDefStr);
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

        public int Cost
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

        private static void GetPcts(ICollection<ShipDesign> designs, Game game, int research,
                out double upkeep, out double speed, out double att, out double colony, out double trans, out double ds)
        {
            if (designs != null && designs.Count > 0)
            {
                upkeep = speed = att = colony = trans = ds = 0;
                double speedStr = GetSpeedStr(research), transStr = GetTransStr(research), dsStr = GetDsResearchStr(research);
                double avgCost = designs.Average(sd => sd.GetTotCost());
                double baseMultTot = 0, strMultTot = 0;
                foreach (ShipDesign design in designs)
                {
                    double totalCost = design.GetTotCost();
                    double speedMult = design.Speed / speedStr;

                    double baseMult = design.Research + 260;
                    baseMult = ( baseMult + ( Math.Pow(baseMult, .169) - 2.6 ) * 1300 ) / (double)research;
                    baseMultTot += baseMult;

                    upkeep += design.Upkeep / ( totalCost / design.GetUpkeepPayoff(game) * Consts.CostUpkeepPct ) * baseMult;
                    speed += ( design.Speed + SpeedAvg ) / ( speedStr + SpeedAvg ) * baseMult;

                    double costMult = Math.Sqrt(avgCost / totalCost);
                    double nonDSPct = 1 - Consts.LimitPct(design.Speed / GetSpeedStr(design.Research) * design.BombardDamage / GetDsResearchStr(design.Research) * costMult);
                    double strMult = design.GetNonColonyPct(game) * design.GetNonTransPct(game) * nonDSPct * baseMult;
                    strMultTot += strMult;

                    const double strAdd = 1.3;
                    att += ( design.Att + strAdd ) / ( (double)design.Def + strAdd ) * strMult;

                    costMult *= baseMult;

                    if (design.Colony)
                        colony += costMult;
                    trans += speedMult * design.Trans / transStr * costMult;
                    ds += speedMult * design.BombardDamage / dsStr * costMult;
                }

                upkeep /= baseMultTot;
                speed /= baseMultTot;

                att /= strMultTot;

                colony /= baseMultTot;
                trans /= baseMultTot;
                ds /= baseMultTot;
            }
            else
            {
                upkeep = speed = att = 1;
                colony = trans = ds = double.NaN;
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
            const double dsMin = .0078, dsMax = 0.21, dsResearch = 3900, dsPow = Math.PI;
            double dsTrg = dsMin + Math.Pow(research / ( dsResearch + research ), dsPow) * ( dsMax - dsMin );

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
            double strChance = ( 1 - focus ) * ( 1 - oth1 ) * ( 1 - oth2 );
            focus = Consts.LimitPct(focus * FocusTypeMult);
            bool swap = Game.Random.Bool();
            if (swap)
            {
                double temp = oth1;
                oth1 = oth2;
                oth2 = temp;
            }
            oth1 = 1 - ( strChance / Math.Sqrt(FocusTypeMult) / ( 1 - focus ) / ( 1 - oth2 ) );
            if (swap)
            {
                double temp = oth1;
                oth1 = oth2;
                oth2 = temp;
            }
        }
        private static bool CreateType(double target, double actual)
        {
            if (!double.IsNaN(actual))
            {
                //chance is higher when target > actual and lower when target < actual
                const double add = .021;
                double mult = Math.Sqrt(( actual + add ) / ( target + add ));
                if (mult < 1)
                    target = ( 1 - ( ( 1 - target ) * mult ) );
                else
                    target = target / mult;
            }
            return Game.Random.Bool(target);
        }

        private static double GetColTransStr(double transStr)
        {
            return MultStr(transStr, .39);
        }

        #endregion //Colony/Trans

        #region Att/Def

        private static double GetAttDefStrMult(double transStr, double bombardDamageMult, bool colony, double trans)
        {
            double strMultOffset = SpeedAvg * transStr;
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
                while (s1 < Game.Random.GaussianOEInt(s2 * FocusAttMult, .65 / FocusAttMult, .078))
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

        private static double ModSpeedStr(double speedStr, double transStr, bool deathStar, bool colony, double trans)
        {
            if (colony)
                speedStr = MultStr(speedStr, .91);
            else if (deathStar)
                speedStr = MultStr(speedStr, .65);
            if (trans > 0)
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
            return Math.Pow(offenseFactor * strengthFactor, .21) / Math.Sqrt(speedPct) * focusFactor;
        }

        #endregion //Speed

        #region Cost/Upkeep

        private double GetMinCost(Game game)
        {
            //random increase to absolute minimum
            double upkPayoff = GetUpkeepPayoff(game);
            return Game.Random.GaussianOE(upkPayoff * Consts.MinCostMult, ( Consts.MinCostMult - 1 ) / 1.69, .052, upkPayoff) + Consts.MinCostBuffer;
        }
        private static double GetMaxCost(int research, double minCost)
        {
            //max cost is more of a guideline than actual rule
            double maxCost = Math.Pow(research, Consts.MaxCostPower) * Consts.MaxCostMult;
            if (maxCost > minCost)
                return minCost + Game.Random.OE(maxCost - minCost);
            return minCost;
        }

        private void GetCost(Game game, double upkeepPct, out double cost, out int upkeep, ref double upkRnd, FocusStat focus)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(game);
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

        internal double GetTotCost()
        {
            return GetTotCost(this.Research);
        }
        internal double GetTotCost(int targetResearch)
        {
            return GetTotCost(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, targetResearch);
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
                stats.Add(ModifyStat.DS, Game.Random.Round(this.BombardDamage * SpeedAvg));

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

        internal static int SetBombardDamage(double bombardDamage, int att)
        {
            return SetBombardDamage(bombardDamage, att, true);
        }
        private static int SetBombardDamage(double bombardDamage, int att, bool keepDeathStar)
        {
            int minDamage = GetDeathStarMin(att);
            if (bombardDamage < minDamage)
                if (keepDeathStar)
                    bombardDamage = minDamage;
                else
                    bombardDamage = 0;
            if (bombardDamage != (int)bombardDamage)
                throw new Exception();
            return (int)bombardDamage;
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
        public static int GetDeathStarMin(int att)
        {
            return (int)Math.Ceiling(DeathStarMin * Consts.GetBombardDamage(att));
        }

        #endregion //Cost/Upkeep

        #region internal

        internal static double GetColonizationValue(double cost, int att, int def, double curHP, int maxHP,
                int speed, int trans, bool colony, double bombardDamage, Game game)
        {
            if (!colony)
                throw new Exception();
            if (curHP > maxHP)
                curHP = maxHP;
            return cost * curHP / (double)maxHP * Consts.GetNonColonyPct(att, def, maxHP, speed, trans, colony, bombardDamage, game, false);
        }

        public double GetUpkeepPayoff(Game game)
        {
            return Consts.GetUpkeepPayoff(game.MapSize, GetNonColonyPct(game), GetNonTransPct(game), this.Speed);
        }

        internal HashSet<ShipDesign> GetObsolete(Game game, IEnumerable<ShipDesign> designs)
        {
            HashSet<ShipDesign> retVal = new HashSet<ShipDesign>();
            foreach (ShipDesign design in designs)
                if (MakesObsolete(game, design))
                    retVal.Add(design);
            return retVal;
        }
        internal bool MakesObsolete(Game game, ShipDesign oldDesign)
        {
            if (this.Research <= oldDesign.Research)
                return false;

            double totCost = this.Cost + this.Upkeep * this.GetUpkeepPayoff(game);
            double oldTotCost = oldDesign.Cost + oldDesign.Upkeep * oldDesign.GetUpkeepPayoff(game);

            double attStr = GetStatValue(this.Att) * this.HP;
            double oldAttStr = GetStatValue(oldDesign.Att) * oldDesign.HP;

            double defStr = GetStatValue(this.Def) * this.HP;
            double oldDefStr = GetStatValue(oldDesign.Def) * oldDesign.HP;

            double transStr = GetTransValue(this.Trans) * this.Speed;
            double oldTransStr = GetTransValue(oldDesign.Trans) * oldDesign.Speed;

            double colonyStr = ( this.Colony ? 1 : 0 );
            double oldColonyStr = ( oldDesign.Colony ? 1 : 0 );

            double deathStr = this.BombardDamage * this.Speed;
            double oldDeathStr = oldDesign.BombardDamage * oldDesign.Speed;

            //must be at least as fast, and either
            bool faster = CompareForObsolete(this.Speed, oldDesign.Speed);
            //be better in each stat category and have a lower cost and upkeep, or
            bool objectively = ( CompareForObsolete(attStr, oldAttStr) && CompareForObsolete(defStr, oldDefStr)
                    && CompareForObsolete(transStr, oldTransStr) && CompareForObsolete(colonyStr, oldColonyStr) && CompareForObsolete(deathStr, oldDeathStr)
                    && CompareForObsolete(oldDesign.Cost, this.Cost) && CompareForObsolete(oldDesign.Upkeep, this.Upkeep) );
            //have a better value per total cost in each category and a similar cost and upkeep
            bool similar = ( CompareForObsolete(attStr / totCost, oldAttStr / oldTotCost) && CompareForObsolete(defStr / totCost, oldDefStr / oldTotCost)
                    && CompareForObsolete(transStr / totCost, oldTransStr / oldTotCost) && CompareForObsolete(colonyStr / totCost, oldColonyStr / oldTotCost)
                    && CompareForObsolete(deathStr / totCost, oldDeathStr / oldTotCost)
                    && ObsoleteCost(this.Cost, oldDesign.Cost, this.Upkeep, oldDesign.Upkeep, this.Research, oldDesign.Research) );

            ;
            ;
            ;
            ;
            ;
            ;
            ;
            ;

            return ( faster && ( objectively || similar ) );
        }
        private bool CompareForObsolete(double s1, double s2)
        {
            return ( s2 <= s1 || ( s2 - s1 ) / ( s2 + s1 ) < Game.Random.Weighted(.21, .26) );
        }
        private bool ObsoleteCost(double c1, double c2, double u1, double u2, double r1, double r2)
        {
            double c = Math.Min(c1, c2) / Math.Max(c1, c2);
            double u = Math.Min(u1, u2) / Math.Max(u1, u2);
            return Game.Random.Bool(Math.Pow(c * c * c * c * c * u * u * u, 780 / ( 260 + r1 - r2 )));
        }

        #endregion //internal

        #region public

        public override string ToString()
        {
            return ShipNames.GetName(this.Name, this.Mark);
        }

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

        public double GetColonizationValue(Game game)
        {
            return GetColonizationValue(AdjustCost(game), this.Att, this.Def, this.HP, this.HP,
                    this.Speed, this.Trans, this.Colony, this.BombardDamage, game);
        }

        internal double AdjustCost(Game game)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(game);
            double cost = GetTotCost() - this.Upkeep * upkeepPayoff;
            cost += ( cost - this.Cost ) / Consts.ScalePct(1, Consts.RepairCostMult, GetNonColonyPct(game));
            if (cost < 1 / Consts.RepairCostMult)
                throw new Exception();
            return cost;
        }

        private double GetNonColonyPct(Game game)
        {
            return Consts.GetNonColonyPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, game, true);
        }
        private double GetNonTransPct(Game game)
        {
            return Consts.GetNonTransPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, game, true);
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
            None = 0x00,    //00000000
            Trans = 0x04,   //00000100
            DS = 0x06,      //00000110
            Colony = 0x07,  //00000111
            Def = 0x10,     //00010000
            Att = 0x18,     //00011000
            Speed = 0x20,   //00100000
            Cost = 0x80,    //10000000
            Upkeep = 0xC0,  //11000000
        }

        #endregion //enum

        #region test

        public static void DoCostTable()
        {
            Console.WriteLine("research\t\ta/d\th\ts\tt\td\t\tmax\t\treg\tcol\ttrans\tds\t\tRS\tCS\tTS\tDS");

            int research = 0;
            int max = Game.Random.GaussianOEInt(130000, .13, .13);
            while (( research = Game.Random.GaussianCappedInt(( research + 13 ) * 1.3, .13, research + 13) ) < max)
            {
                double ad = GetAttDefStr(research);
                double h = GetHPStr(ad, ad);
                double s = GetSpeedStr(research);
                double tr = GetTransStr(research);
                double d = Consts.GetBombardDamage(ad);
                double rMult = GetResearchMult(research);

                double reg = GetTotCost(ad, ad, h, s, 0, false, d, rMult, rMult);
                double regS = GetTotCost(ad, ad, h, s, 0, false, 0, rMult, rMult);

                double strMult = GetAttDefStrMult(tr, 0, false, tr);
                double str = GetAttDefStr(research, strMult, FocusStat.None);
                double hpMult = GetHPMult(strMult, false, false);
                double hp = GetHPStr(str, str, hpMult);

                double t = GetTotCost(str, str, hp, s, tr, false, d, rMult, rMult);
                double tS = GetTotCost(str, str, hp, s, 0, false, 0, rMult, rMult);

                double trans = GetColTransStr(t);
                strMult = GetAttDefStrMult(trans, 0, true, trans);
                str = GetAttDefStr(research, strMult, FocusStat.None);
                hpMult = GetHPMult(strMult, false, true);
                hp = GetHPStr(str, str, hpMult);
                double speed = ModSpeedStr(GetSpeedStr(research), trans, false, true, trans);

                double col = GetTotCost(str, str, hp, speed, trans, true, d, rMult, rMult);
                double colS = GetTotCost(str, str, hp, speed, 0, false, 0, rMult, rMult);

                trans = GetTransStr(research);
                strMult = GetAttDefStrMult(trans, DeathStarAvg, false, 0);
                str = GetAttDefStr(research, strMult, FocusStat.None);
                hpMult = GetHPMult(strMult, true, false);
                hp = GetHPStr(str, str, hpMult);
                speed = ModSpeedStr(GetSpeedStr(research), trans, true, false, 0);
                d *= DeathStarAvg;

                double ds = GetTotCost(str, str, hp, speed, 0, false, d, rMult, rMult);
                double dsS = GetTotCost(str, str, hp, speed, 0, false, 0, rMult, rMult);

                double maxCost = Math.Pow(research, Consts.MaxCostPower) * Consts.MaxCostMult;

                Console.WriteLine(research + "\t\t" + ad + "\t" + h + "\t" + s + "\t" + tr + "\t" + d + "\t\t"
                        + maxCost + "\t\t" + reg + "\t" + col + "\t" + t + "\t" + ds
                        + "\t\t" + regS + "\t" + colS + "\t" + tS + "\t" + dsS);
            }
        }

        #endregion //test

        #region IShipStats Members

        double IShipStats.Cost
        {
            get
            {
                return Cost;
            }
        }
        int IShipStats.Upkeep
        {
            get
            {
                return Upkeep;
            }
        }
        bool IShipStats.Colony
        {
            get
            {
                return Colony;
            }
        }
        int IShipStats.CurTrans
        {
            get
            {
                return Trans;
            }
        }
        int IShipStats.MaxTrans
        {
            get
            {
                return Trans;
            }
        }
        double IShipStats.BombardDamage
        {
            get
            {
                return BombardDamage;
            }
        }
        int IShipStats.CurSpeed
        {
            get
            {
                return Speed;
            }
        }
        int IShipStats.MaxSpeed
        {
            get
            {
                return Speed;
            }
        }
        int IShipStats.Att
        {
            get
            {
                return Att;
            }
        }
        int IShipStats.Def
        {
            get
            {
                return Def;
            }
        }
        int IShipStats.CurHP
        {
            get
            {
                return HP;
            }
        }
        int IShipStats.MaxHP
        {
            get
            {
                return HP;
            }
        }
        double IShipStats.GetUpkeepPayoff(Game game)
        {
            return GetUpkeepPayoff(game);
        }

        #endregion
    }
}
