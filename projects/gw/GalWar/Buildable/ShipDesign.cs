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
            const double speedAdd = 2.1, attDiv = 3.9;
            double speedValue = speed + speedAdd;
            if (speed < 1)
                speedValue = attDiv;

            double statMult = statResearchMult * hp;
            double attValue = GetStatValue(att) * statMult * speedValue / attDiv;
            double defValue = GetStatValue(def) * statMult;

            if (speed < 1)
            {
                speed = 1;
                speedValue = speedAdd;
            }

            return Consts.CostMult * totalResearchMult * (
                (
                    ( attValue + defValue )
                    +
                    ( colony ? 520.0 : 0.0 )
                    +
                    (
                        ( Math.Pow(trans, 1.0 + Consts.AttackNumbersPower) )
                        +
                        ( 13.0 * bombardDamage )
                    )
                    *
                    ( speed )
                    *
                    ( ( attValue / 5.2 + defValue + 65000.0 ) / 16900.0 )
                )
                *
                ( speedValue )
                +
                ( 78.0 * Math.Pow(speed - 1.0, 1.69) )
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
        private static bool unused;

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
                ShipDesign design = new ShipDesign(player, research[++idx], retVal, FocusStat.None,
                        ( type == 0 ), ( type == 1 ), ( type == 2 ), double.NaN, double.NaN, null, out unused);
                retVal.Add(design);
            }

            return retVal;
        }

        internal static ShipDesign TryUpgradeDesign(Player player, IEnumerable<int> tries, ShipDesign upgradeDesign)
        {
            foreach (int research in tries)
            {
                bool named;
                ShipDesign design = new ShipDesign(player, research, null, FocusStat.None,
                        false, false, false, double.NaN, double.NaN, upgradeDesign, out named);
                if (named)
                    return design;
            }
            return null;
        }

        internal ShipDesign(Player player, int research)
            : this(player, research, double.NaN, double.NaN)
        {
            checked
            {
            }
        }

        internal ShipDesign(Player player, int research, double minCost, double maxCost)
            : this(player, research, player.GetDesigns(), player.GetResearchFocus(), false, false, false, minCost, maxCost, null, out unused)
        {
            checked
            {
            }
        }

        private ShipDesign(Player player, int research, ICollection<ShipDesign> designs, FocusStat focus,
                bool forceColony, bool forceTrans, bool forceNeither, double minCost, double maxCost, ShipDesign nameIf, out bool named)
        {
            checked
            {
                int mapSize = player.Game.MapSize;

                this._bombardDamage = 0;
                this._research = (uint)research;

                //existing designs change probabilities for new research
                double colonyPct, upkeepPct, attPct, speedPct, transPct, dsPct;
                GetPcts(designs, mapSize, research, out colonyPct, out upkeepPct, out attPct, out speedPct, out transPct, out dsPct);

                //  ------  Colony/Trans/DS   ------
                double transStr = GetTransStr(research), bombardDamageMult;
                bool colony;
                int trans;
                DoColonyTransDS(forceColony, forceTrans, forceNeither, research, colonyPct, transPct, dsPct,
                        ref transStr, out colony, out trans, out bombardDamageMult, focus);
                bool deathStar = ( bombardDamageMult > 0 );
                this._colony = colony;
                this._trans = (ushort)trans;

                //  ------  Att/Def           ------
                //being a colony ship/transport/death star makes att and def lower
                double strMult = GetAttDefStrMult(transStr, bombardDamageMult, this.Colony, this.Trans);
                double str = GetAttDefStr(research, strMult, focus);
                int att, def;
                DoAttDef(transStr, str, attPct, out att, out def, focus);
                this._att = (byte)att;
                this._def = (byte)def;

                //  ------  HP                ------
                //being a colony ship/transport/death star makes hp higher
                double hpMult = GetHPMult(strMult, deathStar, this.Colony);
                //hp is relative to actual randomized stats
                this._hp = (ushort)MakeStat(GetHPStr(this.Att, this.Def, hpMult));

                //  ------  Speed             ------
                double speedStr = MultStr(ModSpeedStr(GetSpeedStr(research), transStr, deathStar, this.Colony, this.Trans, true),
                        GetSpeedMult(str, hpMult, speedPct, this.Att, this.Def, this.HP, focus));
                this._speed = (byte)MakeStat(speedStr);

                //  ------  BombardDamage     ------
                //modify bombard mult based on speed and att
                if (deathStar)
                    this._bombardDamage = (ushort)SetBombardDamage(Game.Random.Round(bombardDamageMult * this.BombardDamage
                            * Math.Sqrt(speedStr / (double)this.Speed * Math.Sqrt(str * this.Def) / (double)this.Att)), this.Att, true);

                //  ------  Cost/Upkeep       ------
                double cost = -1, upkRnd = double.NaN;
                int upkeep = -1;
                GetCost(mapSize, upkeepPct, out cost, out upkeep, ref upkRnd, focus);
                bool anomalyShip = !double.IsNaN(minCost);
                if (!anomalyShip)
                {
                    minCost = GetMinCost(mapSize);
                    maxCost = GetMaxCost(research, minCost);
                }
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

                //  ------  Name   
                this._name = byte.MaxValue;
                this._mark = byte.MaxValue;
                named = ( nameIf == null || ( !StatsIdentical(this, nameIf) && this.MakesObsolete(mapSize, nameIf) ) );
                if (named)
                {
                    this._name = (byte)player.Game.ShipNames.GetName(this, GetAttDefStr(Research), GetTransStr(Research), GetSpeedStr(Research), anomalyShip);
                    this._mark = (byte)player.Game.ShipNames.GetMark(player, this.Name);
                }
            }
        }

        public bool Colony
        {
            get
            {
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

        public int Upkeep
        {
            get
            {
                return this._upkeep;
            }
        }

        public override int Cost
        {
            get
            {
                return this._cost;
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

        public double BombardDamage
        {
            get
            {
                return GetBombardDamage(this._bombardDamage, this.Att);
            }
        }
        public bool DeathStar
        {
            get
            {
                return ( this._bombardDamage > 0 );
            }
        }

        public int Research
        {
            get
            {

                return (int)this._research;
            }
        }

        private static void GetPcts(ICollection<ShipDesign> designs, int mapSize, int research,
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

        private void DoAttDef(double transStr, double str, double attPct, out int att, out int def, FocusStat focus)
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
            double chance = ( ( this.Colony || this.Trans > Game.Random.Gaussian(transStr * .52, .39) ) ? .26 : .65 );
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
        private void CheckFocusStat(double str, ref int s1, ref int s2)
        {
            if (s1 >= s2)
                while (s1 < Game.Random.Gaussian(s2 * FocusAttMult, .39))
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

        private double GetMinCost(int mapSize)
        {
            double minCost = this.GetUpkeepPayoff(mapSize) * Consts.MinCostMult + 1 / Consts.RepairCostMult;
            return Game.Random.GaussianOE(minCost * 1.3, 0.026, 0.021, minCost);
        }

        private static double GetMaxCost(int research, double minCost)
        {
            double maxCost = Math.Pow(research, Consts.MaxCostPower) * Consts.MaxCostMult;
            //max is more of a guideline than actual rule
            if (maxCost > minCost)
                maxCost = Game.Random.GaussianOE(maxCost, .21, .039, minCost);
            else
                maxCost = minCost;
            return maxCost;
        }

        private void GetCost(int mapSize, double upkeepPct, out double cost, out int upkeep, ref double upkRnd, FocusStat focus)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(mapSize);
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
                    upkeep = Game.Random.GaussianCappedInt(avgUpk, 1, 1);
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
            int minDamage = GetDeathStarMin(att);
            if (bombardDamage > 0)
                if (bombardDamage < minDamage)
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
                int speed, int trans, bool colony, double bombardDamage, double research)
        {
            if (!colony)
                throw new Exception();
            if (curHP > maxHP)
                curHP = maxHP;
            return cost * curHP / (double)maxHP * Consts.GetNonColonyPct(att, def, maxHP, speed, trans, colony, bombardDamage, research, false);
        }

        public double GetUpkeepPayoff(int mapSize)
        {
            return Consts.GetUpkeepPayoff(mapSize, GetNonColonyPct(), GetNonTransPct(), this.Speed);
        }

        internal HashSet<ShipDesign> GetObsolete(int mapSize, IEnumerable<ShipDesign> designs)
        {
            HashSet<ShipDesign> retVal = new HashSet<ShipDesign>();
            foreach (ShipDesign design in designs)
                if (MakesObsolete(mapSize, design))
                    retVal.Add(design);
            return retVal;
        }
        internal bool MakesObsolete(int mapSize, ShipDesign oldDesign)
        {
            if (this.Research <= oldDesign.Research)
                return false;

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
                ObsoleteCost(this.Cost, oldDesign.Cost, this.Upkeep, oldDesign.Upkeep) ) )
            );
        }
        private bool CompareForObsolete(double s1, double s2)
        {
            return ( s2 == 0 || ( s2 - s1 ) / ( s2 + s1 ) < Game.Random.Weighted(.26, .21) );
        }
        private bool ObsoleteCost(double c1, double c2, double u1, double u2)
        {
            double c = Math.Min(c1, c2) / Math.Max(c1, c2);
            double u = Math.Min(u1, u2) / Math.Max(u1, u2);
            return Game.Random.Bool(Math.Pow(c * c * c * c * c * u * u * u, .3));
        }

        internal bool StatsIdentical(ShipDesign d1, ShipDesign d2)
        {
            return ( d1.Colony == d2.Colony && d1.Att == d2.Att && d1.Def == d2.Def && d1.Speed == d2.Speed
                    && d1.Upkeep == d2.Upkeep && d1.Cost == d2.Cost
                    && d1.HP == d2.HP && d1.Trans == d2.Trans && d1.BombardDamage == d2.BombardDamage );
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
            return Consts.BaseDesignHPMult / Math.Pow(strMult, deathStar || colony ? 1.69 : 2.6);
        }

        public static double GetHPStr(double s1, double s2)
        {
            return GetHPStr(s1, s2, Consts.BaseDesignHPMult);
        }
        private static double GetHPStr(double s1, double s2, double hpMult)
        {
            return MultStr(( s1 + s2 ) * ( s1 + s2 ), hpMult);
        }

        public double GetColonizationValue(int mapSize, int research)
        {
            return GetColonizationValue(AdjustCost(mapSize), this.Att, this.Def, this.HP, this.HP,
                    this.Speed, this.Trans, this.Colony, this.BombardDamage, research);
        }

        internal double AdjustCost(int mapSize)
        {
            double upkeepPayoff = this.GetUpkeepPayoff(mapSize);
            double cost = GetTotCost() - this.Upkeep * upkeepPayoff;
            cost += ( cost - this.Cost ) / Consts.ScalePct(1, Consts.RepairCostMult, GetNonColonyPct());
            if (cost < upkeepPayoff * Consts.MinCostMult)
                throw new Exception();
            return cost;
        }

        private double GetNonColonyPct()
        {
            return Consts.GetNonColonyPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, this.Research, true);
        }
        private double GetNonTransPct()
        {
            return Consts.GetNonTransPct(this.Att, this.Def, this.HP, this.Speed, this.Trans, this.Colony, this.BombardDamage, this.Research);
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
