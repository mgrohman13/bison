using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace CityWar
{
    [Serializable]
    public class Player : IDeserializationCallback
    {
        #region fields and constructors
        public const int WizardCost = 1300, RelicCost = 300;
        internal const int TradeDown = 9, TradeUp = 30;
        internal const double WorkMult = ( TradeDown + ( TradeUp - TradeDown ) / 3.0 ) / 10.0;
        internal const double UpkeepMult = ( TradeUp - ( TradeUp - TradeDown ) / 3.0 ) / 10.0;

        private Game game;

        public readonly Color Color;
        public readonly string Name;

        private static float zoom = -1;

        public readonly string Race;

        private int death, air, earth, nature, water, population, production, _relic, magic, upkeep, work;
        private double healRound;

        private List<Piece> pieces = new List<Piece>();

        [NonSerialized]
        private List<string> trades;
        [NonSerialized]
        private Dictionary<string, Bitmap> pics, picsConst;

        public Player(string Race, Color color, string Name)
        {
            this.Race = Race;
            this.Color = color;
            Color c = this.InverseColor;
            this.Name = Name;
            this.healRound = Game.Random.NextDouble();

            this.OnDeserialization(null);
        }
        #endregion //fields and constructors

        #region public methods and properties
        public Game Game
        {
            get
            {
                return game;
            }
        }

        public ReadOnlyCollection<Piece> GetPieces()
        {
            return pieces.AsReadOnly();
        }

        public Piece NextPiece(int currentGroup)
        {
            int count = pieces.Count;

            pieces.Sort(delegate(Piece p1, Piece p2)
            {
                return p1.Group - p2.Group;
            });

            //find the last index of a piece that is selected and start there
            int index;
            for (index = count ; --index > -1 ; )
                if (pieces[index].Group == currentGroup)
                    break;

            bool stop = false;
            while (true)
            {
                if (++index < count)
                {
                    if (( pieces[index] ).Movement > 0 && pieces[index].Group != currentGroup)
                        return pieces[index];
                }
                else
                {
                    if (stop)
                        break;
                    //loop around to the beginning since we started somewhere in the middle
                    stop = true;
                    index = -1;
                }
            }

            //finally, just return any piece with movement left
            foreach (Piece p in pieces)
                if (p.Movement > 0)
                    return p;

            return null;
        }

        public bool HasMovesLeft()
        {
            foreach (Piece p in pieces)
                if (p.Movement > 0)
                    return true;

            return false;
        }

        public void GetCounts(out int wizards, out int portals, out int cities, out int relics, out int units)
        {
            wizards = 0;
            portals = 0;
            cities = 0;
            relics = 0;
            units = 0;
            foreach (Piece p in pieces)
            {
                if (p is Wizard)
                    ++wizards;
                else if (p is Portal)
                    ++portals;
                else if (p is City)
                    ++cities;
                else if (p is Relic)
                    ++relics;
                else if (p is Unit)
                    ++units;
                else
                    throw new Exception();
            }
        }

        public Tile GetCenter()
        {
            return Tile.FindCenterPiece(pieces).Tile;
        }

        public Color InverseColor
        {
            get
            {
                const double inverseColorMidVal = 255 * 3 / 2.0;
                int total = Color.R + Color.G + Color.B;
                return total < inverseColorMidVal ? Color.White : Color.Black;
            }
        }

        public int Production
        {
            get
            {
                return production;
            }
        }
        public int Population
        {
            get
            {
                return population;
            }
        }
        public int Air
        {
            get
            {
                return air;
            }
        }
        public int Earth
        {
            get
            {
                return earth;
            }
        }
        public int Nature
        {
            get
            {
                return nature;
            }
        }
        public int Water
        {
            get
            {
                return water;
            }
        }
        public int Death
        {
            get
            {
                return death;
            }
        }
        public int Magic
        {
            get
            {
                return magic;
            }
        }
        public int Relic
        {
            get
            {
                return relic;
            }
        }
        public int Work
        {
            get
            {
                return work;
            }
        }

        internal double HealRound
        {
            get
            {
                return healRound;
            }
        }

        public double GetTurnUpkeep()
        {
            return upkeep * .13;
        }

        public int GetResource(string resource)
        {
            switch (resource)
            {
            case "Air":
                return air;
            case "Death":
                return death;
            case "Earth":
                return earth;
            case "Magic":
                return magic;
            case "Nature":
                return nature;
            case "Work":
                return work;
            case "Production":
                return production;
            case "Water":
                return water;
            case "Relic":
                return relic;
            case "Population":
                return population;
            default:
                throw new Exception();
            }
        }

        public double GetTotalResources()
        {
            double result = 0;

            result += air;
            result += death;
            result += earth;
            result += nature;
            result += production;
            result += water;
            result += magic;
            result += relic;
            result += population;
            result += work / WorkMult;
            result -= upkeep / UpkeepMult;

            return result;
        }
        public double GetArmyStrength()
        {
            double retVal = 0;
            foreach (Piece p in pieces)
            {
                Unit u = p as Unit;
                if (u != null)
                    retVal += u.RandedCost * u.GetHealthPct();
            }
            return retVal;
        }

        public override string ToString()
        {
            return Name;
        }
        #endregion //public methods and properties

        #region internal methods
        internal void NewPlayer(Game game, bool city, string[] startUnits, double totalStartCost)
        {
            this.game = game;

            pieces.Clear();
            trades.Clear();

            air = 0;
            earth = 0;
            nature = 0;
            death = 0;
            water = 0;
            production = 0;
            population = 0;
            magic = 0;
            relic = 0;
            work = 0;
            upkeep = 0;

            double thisTotal = 0;
            if (city)
            {
                int[] prodPop = AddStartResources(666, ref magic, 300, 599, 2);
                production += prodPop[0];
                population += prodPop[1];

                for (int a = -1 ; ++a < 3 ; )
                {
                    Tile t = RandomStartTile();
                    for (int b = 0 ; b < 6 ; ++b)
                        if (t.GetNeighbor(b) == null)
                        {
                            t = RandomStartTile();
                            b = -1;
                            continue;
                        }

                    new City(this, t);
                    thisTotal += Unit.NewUnit(startUnits[a], t, this).BaseCost;
                }
            }
            else
            {
                int[] elementals = AddStartResources(390, ref _relic, 100, 199, 5);
                air += elementals[0];
                earth += elementals[1];
                nature += elementals[2];
                death += elementals[3];
                water += elementals[4];

                Tile t = RandomStartTile();

                bool unused;
                new Wizard(this, t, out unused);
                new Relic(this, t);

                for (int i = -1 ; ++i < startUnits.Length ; )
                    thisTotal += Unit.NewUnit(startUnits[i], t, this).BaseCost;
            }

            BalanceForUnit(totalStartCost, thisTotal);
        }

        public void BalanceForUnit(double avg, double mine)
        {
            avg -= mine;
            if (avg > 0)
                AddWork(avg * WorkMult);
            else
                AddUpkeep(avg * -UpkeepMult);
        }

        private int[] AddStartResources(int totalAmt, ref int mainType, int minMain, int maxMain, int numOthers)
        {
            int addAmt = Game.Random.RangeInt(minMain, maxMain);
            mainType += addAmt;
            totalAmt -= addAmt;

            int[] retVal = new int[numOthers];
            foreach (int val in Game.Random.Iterate(numOthers))
            {
                if (numOthers > 1)
                    addAmt = Game.Random.RangeInt(0, Game.Random.Round(2f * totalAmt / numOthers--));
                else
                    addAmt = totalAmt;
                retVal[val] = addAmt;
                totalAmt -= addAmt;
            }
            return retVal;
        }

        private Tile RandomStartTile()
        {
            while (true)
            {
                Tile t = game.RandomTile();
                if (t.Occupied())
                    continue;

                //cannot start adjacent to another player, or even yourself
                bool can = true;
                for (int b = 0 ; b < 6 ; ++b)
                    if (t.GetNeighbor(b) != null && t.GetNeighbor(b).Occupied())
                    {
                        can = false;
                        break;
                    }
                if (can)
                    return t;
            }
        }

        internal void Add(Piece p)
        {
            pieces.Add(p);
            if (p is Capturable)
                GetRelic();
        }
        internal void Remove(Piece p, bool turnIncrement)
        {
            pieces.Remove(p);
            //actually losing the game is handled separately inside IncrementTurn
            if (!turnIncrement && pieces.Count < 1)
                KillPlayer();
        }

        internal void LostCapt(Capturable c, Player p)
        {
            //the capturing player gets stuck with some of the other players upkeep
            int wizards, portals, cities, relics, units;
            this.GetCounts(out wizards, out portals, out cities, out relics, out units);
            double transferAmt = this.upkeep / ( wizards + portals + cities + relics - .13 ) * .65;
            this.AddUpkeep(-Game.Random.GaussianCappedInt(transferAmt, .091));
            p.AddUpkeep(Game.Random.GaussianCappedInt(transferAmt, .091));

            Remove(c, false);
        }

        internal void Spend(int amt, CostType type, int population)
        {
            if (type == CostType.Air)
                air -= amt;
            else if (type == CostType.Death)
                death -= amt;
            else if (type == CostType.Earth)
                earth -= amt;
            else if (type == CostType.Nature)
                nature -= amt;
            else if (type == CostType.Production)
                production -= amt;
            else if (type == CostType.Water)
                water -= amt;
            else
                throw new Exception();

            this.population -= population;
        }

        internal void SpendMagic(int amt)
        {
            magic -= amt;
        }

        internal void AddRelic(double amt)
        {
            relic += Game.Random.Round(amt);
        }
        internal void AddDeath(double amt)
        {
            death += Game.Random.Round(amt);
        }
        internal void AddWork(double amt)
        {
            work += Game.Random.Round(amt);
        }
        internal void AddUpkeep(double amount)
        {
            upkeep += Game.Random.Round(amount);
        }

        internal static void SubtractCommonUpkeep(Player[] players)
        {
            int min = int.MaxValue;
            foreach (Player p in players)
                min = Math.Min(min, p.upkeep);
            min = Game.Random.WeightedInt(min, .78f);
            foreach (Player p in players)
                p.upkeep -= min;
        }

        internal Unit FreeUnit(string name, double avgCost)
        {
            Unit unit = FreeUnit(name, RandomCapturable());
            BalanceForUnit(avgCost, unit.BaseCost);
            return unit;
        }
        internal Unit FreeUnit(string unit, Capturable cur)
        {
            return Unit.NewUnit(unit, cur.Tile, this);
        }

        internal void CollectWizardPts(double amount)
        {
            int amt = (int)amount;
            if (amt == amount)
            {
                amount = 1;
            }
            else
            {
                amount -= amt;
                ++amt;
            }

            while (--amt > -1)
            {
                double avg = 50 * ( amt == 0 ? amount : 1 );
                int addAmt = Game.Random.GaussianCappedInt(avg, .26, Game.Random.Round(avg * .6));

                if (Game.Random.Bool(.01f))
                    population += addAmt;		// 01.00%
                else if (Game.Random.Bool(.013f))
                    production += addAmt;		// 01.29%
                else if (Game.Random.Bool(.03f))
                    magic += addAmt;			// 02.93%
                else if (Game.Random.Bool(.06f))
                    relic += addAmt;			// 05.69%
                else if (Game.Random.Bool(.169f))
                    death += addAmt;			// 15.06%
                else
                    switch (Game.Random.NextBits(2))
                    {
                    case 0:
                        water += addAmt;	// 18.51%
                        break;
                    case 1:
                        nature += addAmt;	// 18.51%
                        break;
                    case 2:
                        earth += addAmt;	// 18.51%
                        break;
                    case 3:
                        air += addAmt;		// 18.51%
                        break;
                    default:
                        throw new Exception();
                    }
            }
        }

        internal void EndTurn()
        {
            int oldUpkeep = upkeep;
            foreach (Piece piece in pieces.ToArray())
                piece.ResetMove();
            int addUpkeep = upkeep - oldUpkeep;

            upkeep = oldUpkeep;
            PayUpkeep();
            upkeep += addUpkeep;

            healRound = Game.Random.NextDouble();
        }

        internal void AddMagic(Terrain terrain, int amount)
        {
            if (terrain == Terrain.Forest)
                nature += amount;
            else if (terrain == Terrain.Mountain)
                earth += amount;
            else if (terrain == Terrain.Plains)
                air += amount;
            else if (terrain == Terrain.Water)
                water += amount;
            else
                throw new Exception();
        }

        internal void KillPlayer()
        {
            if (pieces.Count > 0)
                throw new Exception();

            game.DefeatPlayer(this);

            trades.Clear();
            picsConst.Clear();
            pics.Clear();
        }
        #endregion //internal methods

        #region portal cost
        public static int TotalPortalCost(string race, CostType costType)
        {
            int[] cost = Player.SplitPortalCost(race)[costType];
            return cost[0] + cost[1];
        }
        public static void SplitPortalCost(string race, CostType costType, out int magic, out int element)
        {
            int[] retVal = SplitPortalCost(race)[costType];
            magic = retVal[0];
            element = retVal[1];
        }
        public static Dictionary<CostType, int[]> SplitPortalCost(string race)
        {
            Dictionary<CostType, int[]> portalCosts = new Dictionary<CostType, int[]>();

            double[] elmDbl = new double[5];
            int[] elmInt = new int[5];
            double[] other = new double[5];
            double[] ppl = new double[5];

            foreach (string name in Game.Races[race])
            {
                Unit unit = Unit.CreateTempUnit(name);
                int idx = getCTIdx(unit.costType);
                if (idx > -1)
                {
                    elmDbl[idx] += unit.BaseCost + unit.BasePplCost / 2.0;
                    ++elmInt[idx];

                    double div = Math.Sqrt(unit.BaseCost);
                    other[idx] += unit.BaseOtherCost / div;
                    ppl[idx] += unit.BasePplCost / div;
                }
            }

            double total = 0;
            for (int idx = 0 ; idx < 5 ; ++idx)
            {
                elmDbl[idx] = GetTotalPortalCost(elmDbl[idx], elmInt[idx]);
                total += elmDbl[idx];
            }

            int totInt = 0;
            for (int idx = 0 ; idx < 4 ; ++idx)
            {
                elmInt[idx] = (int)Math.Round(elmDbl[idx] * Portal.AvgPortalCost * 5.0 / total);
                totInt += elmInt[idx];
            }
            elmInt[4] = Portal.AvgPortalCost * 5 - totInt;

            for (int idx = 0 ; idx < 5 ; ++idx)
            {
                int totalCost = elmInt[idx];
                int element = GetPortalElementCost(other[idx] / ( other[idx] + ppl[idx] ), totalCost);
                int magic = totalCost - element;
                portalCosts.Add(getIdxCT(idx), new int[] { magic, element });
            }

            return portalCosts;
        }
        private static double GetTotalPortalCost(double totCost, int numUnits)
        {
            //the greater the number of units and their cost, the greater the total cost of the portal
            return Math.Pow(totCost * ( numUnits + 1 ), .39);
        }
        public static int GetPortalElementCost(double percent, double totalCost)
        {
            //the more population the units cost, the less magic the portal costs
            return (int)Math.Ceiling(( 1 - ( percent * percent * .666 + .21 ) ) * totalCost);
        }
        private static int getCTIdx(CostType costType)
        {
            switch (costType)
            {
            case CostType.Air:
                return 0;
            case CostType.Earth:
                return 1;
            case CostType.Nature:
                return 2;
            case CostType.Water:
                return 3;
            case CostType.Death:
                return 4;
            default:
                return -1;
            }
        }
        private static CostType getIdxCT(int idx)
        {
            switch (idx)
            {
            case 0:
                return CostType.Air;
            case 1:
                return CostType.Earth;
            case 2:
                return CostType.Nature;
            case 3:
                return CostType.Water;
            case 4:
                return CostType.Death;
            default:
                throw new Exception();
            }
        }
        #endregion //portal cost

        #region relic
        private int relic
        {
            get
            {
                return _relic;
            }
            set
            {
                _relic = value;
                GetRelic();
            }
        }
        private void GetRelic()
        {
            if (relic >= RelicCost)
            {
                Capturable cur = RandomCapturable();
                if (cur != null)
                {
                    relic -= RelicCost;
                    new Relic(this, cur.Tile);
                }
            }
        }
        private Capturable RandomCapturable()
        {
            foreach (Piece p in Game.Random.Iterate(pieces))
            {
                Capturable piece = ( p as Capturable );
                if (piece != null)
                    return piece;
            }
            return null;
        }
        #endregion //relic

        #region piece images
        public Bitmap GetPic(string name)
        {
            return GetPic(name, false);
        }
        public Bitmap GetConstPic(string name)
        {
            return GetPic(name, true);
        }
        private Bitmap GetPic(string name, bool constPic)
        {
            Dictionary<string, Bitmap> pics = constPic ? this.picsConst : this.pics;
            if (!pics.ContainsKey(name))
                CreatePic(name, constPic);
            return pics[name];
        }
        private void CreatePic(string name, bool constPic)
        {
            string loadName = name.Replace(" (1)", "1").Replace(" (2)", "2").Replace(" (3)", "3");

            Color portalColor = Color.FromArgb(200, 0, 0);
            if (name.EndsWith(" Portal") || name.EndsWith(" PortalUnit"))
            {
                string[] split = name.Split(' ');
                switch (split[0])
                {
                case "Air":
                    portalColor = Color.Gray;
                    break;
                case "Death":
                    portalColor = Color.Black;
                    break;
                case "Earth":
                    portalColor = Color.Gold;
                    break;
                case "Nature":
                    portalColor = Color.Green;
                    break;
                case "Water":
                    portalColor = Color.Blue;
                    break;
                }
                loadName = split[1];
            }

            Bitmap pic;
            //dont bother saving a const pic if the name ends with unit; it will never be drawn in a panel
            if (constPic || name.EndsWith("Unit"))
                pic = LoadPicFromFile(loadName, portalColor);
            else
                pic = GetConstPic(name);

            if (constPic)
                picsConst[name] = pic;
            else
                pics[name] = ResizePic(pic);
        }
        private Bitmap LoadPicFromFile(string name, Color portalColor)
        {
            Bitmap basePic;
            try
            {
                basePic = new Bitmap(Game.Path + "pics\\" + name + ".bmp");
            }
            catch
            {
                basePic = new Bitmap(Game.Path + "pics\\notFound.bmp");
            }
            //white is transparent
            basePic.MakeTransparent(Color.FromArgb(255, 255, 255));
            //change the gray to the player color and the red to the poral color
            ImageAttributes colorRemapping = new ImageAttributes();
            ColorMap playerMap = new ColorMap();
            playerMap.OldColor = Color.FromArgb(100, 100, 100);
            playerMap.NewColor = Color;
            ColorMap portalMap = new ColorMap();
            portalMap.OldColor = Color.FromArgb(200, 0, 0);
            portalMap.NewColor = portalColor;
            colorRemapping.SetRemapTable(new ColorMap[] { playerMap, portalMap });
            Bitmap pic = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(pic);
            //draw it to a new image to remap the colors
            g.DrawImage(basePic, new Rectangle(0, 0, 100, 100), 0, 0, 100, 100, GraphicsUnit.Pixel, colorRemapping);
            g.Dispose();
            basePic.Dispose();
            colorRemapping.Dispose();
            //return the new image
            return pic;

        }
        private Bitmap ResizePic(Bitmap pic)
        {
            return new Bitmap(pic, Game.Random.Round(zoom * 5f / 6f),
                Game.Random.Round(zoom * 5f / 6f));
        }
        internal static void ResetPics(Player[] players, float zoom)
        {
            if (Math.Abs(Player.zoom - zoom) > 1)
            {
                Player.zoom = zoom;
                foreach (Player p in players)
                {
                    foreach (Bitmap image in p.pics.Values)
                        image.Dispose();
                    p.pics.Clear();
                }
            }
        }
        #endregion //piece images

        #region trading
        public void GambleWork(int low, int high)
        {
            work = Gamble(work, low, high);
        }
        public void GambleDeath(int low, int high)
        {
            death = Gamble(death, low, high);
        }
        public void GambleProduction(int low, int high)
        {
            production = Gamble(production, low, high);
        }
        public void GambleNature(int low, int high)
        {
            nature = Gamble(nature, low, high);
        }
        public void GambleEarth(int low, int high)
        {
            earth = Gamble(earth, low, high);
        }
        public void GambleWater(int low, int high)
        {
            water = Gamble(water, low, high);
        }
        public void GambleAir(int low, int high)
        {
            air = Gamble(air, low, high);
        }
        public void GamblePopulation(int low, int high)
        {
            population = Gamble(population, low, high);
        }
        private int Gamble(int value, int low, int high)
        {
            if (low >= 0 && low < high)
                while (value > low && value < high)
                    value += ( Game.Random.Bool() ? -1 : 1 );
            return value;
        }

        private void TradeMagic(bool up)
        {
            magic = Trade(up, magic, "ma");
        }
        private void TradeRelic(bool up)
        {
            relic = Trade(up, relic, "re");
        }
        public void TradeDeath(bool up)
        {
            death = Trade(up, death, "de");
        }
        public void TradeProduction(bool up)
        {
            production = Trade(up, production, "pr");
        }
        public void TradeNature(bool up)
        {
            nature = Trade(up, nature, "na");
        }
        public void TradeEarth(bool up)
        {
            earth = Trade(up, earth, "ea");
        }
        public void TradeWater(bool up)
        {
            water = Trade(up, water, "wa");
        }
        public void TradeAir(bool up)
        {
            air = Trade(up, air, "ai");
        }
        public void TradePopulation(bool up)
        {
            population = Trade(up, population, "po");
        }
        private int Trade(bool up, int amt, string id)
        {
            if (this != game.CurrentPlayer || ( up && work < 1 ) || ( !up && amt < 1 ))
                return amt;

            if (up)
            {
                if (trades.Contains(id + "d"))
                {
                    if (work < TradeDown)
                    {
                        amt += Game.Random.Round(10f * work / TradeDown);
                        work = 0;
                    }
                    else
                    {
                        work -= TradeDown;
                        amt += 10;
                    }

                    trades.Remove(id + "d");
                }
                else
                {
                    if (work < TradeUp)
                    {
                        amt += Game.Random.Round(10f * work / TradeUp);
                        work = 0;
                    }
                    else
                    {
                        work -= TradeUp;
                        amt += 10;

                        trades.Add(id + "u");
                    }
                }
            }
            else
            {
                if (trades.Contains(id + "u"))
                {
                    if (amt < 10)
                    {
                        work += Game.Random.Round(TradeUp * amt / 10f);
                        amt = 0;
                    }
                    else
                    {
                        work += TradeUp;
                        amt -= 10;
                    }

                    trades.Remove(id + "u");
                }
                else
                {
                    if (amt < 10)
                    {
                        work += Game.Random.Round(TradeDown * amt / 10f);
                        amt = 0;
                    }
                    else
                    {
                        work += TradeDown;
                        amt -= 10;

                        trades.Add(id + "d");
                    }
                }
            }

            return amt;
        }
        #endregion //trading

        #region income and upkeep
        private void PayUpkeep()
        {
            double payment = GetTurnUpkeep();
            PayUpkeep(payment);

            //unit upkeep
            double total = 0;
            foreach (Piece p in pieces)
            {
                Unit u = p as Unit;
                if (u != null && u.Type != UnitType.Immobile)
                    total += u.RandedCost / 210.0;
            }
            AddUpkeep(total * UpkeepMult / WorkMult);
        }

        private void PayUpkeep(double payment)
        {
            AddWork(-payment);
            AddUpkeep(-payment);

            //when your work is negative, there is a chance you will lose some actual resources
            int amt = 10;
            while (Game.Random.OE(39 * GetRandVal(amt)) < -work)
            {
                switch (Game.Random.NextBits(4))
                {
                case 0:
                case 1:
                    amt = death;
                    TradeDeath(false);
                    break;
                case 2:
                    amt = air;
                    TradeAir(false);
                    break;
                case 3:
                    amt = earth;
                    TradeEarth(false);
                    break;
                case 4:
                    amt = nature;
                    TradeNature(false);
                    break;
                case 5:
                    amt = water;
                    TradeWater(false);
                    break;
                case 6:
                case 7:
                case 8:
                case 9:
                    amt = production;
                    TradeProduction(false);
                    break;
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    amt = population;
                    TradePopulation(false);
                    break;
                default:
                    throw new Exception();
                }
                //if you did not have enough of the chosen standard resource, you may lose some magic or relic
                while (Game.Random.Bool(1 - GetRandVal(amt)))
                    if (magic > 0 || relic > 0)
                    {
                        switch (Game.Random.NextBits(3))
                        {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            amt = magic;
                            TradeMagic(false);
                            break;
                        case 5:
                        case 6:
                        case 7:
                            amt = relic;
                            TradeRelic(false);
                            break;
                        default:
                            throw new Exception();
                        }
                    }
                    else
                    {
                        amt = 0;
                        break;
                    }
            }
        }
        private static double GetRandVal(int amt)
        {
            double retVal = amt / 10.0;
            if (retVal > 1)
                retVal = 1;
            else if (retVal < 0)
                retVal = 0;
            return retVal;
        }

        internal int StartTurn()
        {
            GenerateIncome(ref air, ref death, ref earth, ref nature, ref production, ref water, ref work, ref magic, ref _relic, ref population, true);
            CheckNegativeResources();
            GetRelic();
            trades.Clear();
            return Math.Min(work, 0);
        }

        private void CheckNegativeResources()
        {
            CheckNegativeResource(ref air);
            CheckNegativeResource(ref death);
            CheckNegativeResource(ref earth);
            CheckNegativeResource(ref nature);
            CheckNegativeResource(ref production);
            CheckNegativeResource(ref water);
            CheckNegativeResource(ref magic);
            CheckNegativeResource(ref _relic);
            CheckNegativeResource(ref population);
        }

        private void CheckNegativeResource(ref int amt)
        {
            if (amt < 0)
            {
                AddUpkeep(amt * -UpkeepMult);
                amt = 0;
            }
        }

        //this doesnt actually add the income to the player so it can be used to simply view the income
        public void GenerateIncome(ref int airP, ref int deathP, ref int earthP, ref int natureP, ref int productionP, ref int waterP, ref int workP, ref int magicP, ref int relicP, ref int populationP, bool randRound)
        {
            populationP += 15;
            magicP += 10;
            productionP += 5;

            foreach (Piece p in pieces)
            {
                if (!( p is Capturable ))
                    continue;

                int elemental = 0;

                Portal portal;
                if (p is Wizard)
                {
                    //cost 1300
                    //roi 16.25-43.33
                    elemental += 30;
                    //rest: +10 (roi 32.50)
                    //find: +50
                    //88.10% collection needed for average portal roi (52.67% for relic)
                }
                else if (p is City)
                {
                    productionP += 13;
                    magicP += 7;
                    elemental += 6;
                    populationP += 3;
                    deathP += 1;
                }
                else if (p is Relic)
                {
                    //cost 300
                    //roi 23.08
                    magicP += 6;
                    elemental += 5;
                    populationP += 2;
                }
                else if (( portal = p as Portal ) != null)
                {
                    //avg cost 1000 (700-1502)
                    //avg roi 17.56 (16.03-19.97)
                    int amt = portal.income;

                    int type = 0, position = 0;
                    for ( ; amt > 0 ; --amt)
                        switch (++position % 6)
                        {
                        case 1:
                            ++type;
                            break;
                        case 2:
                            ++magicP;
                            break;
                        case 3:
                            ++elemental;
                            break;
                        case 4:
                            ++type;
                            break;
                        case 5:
                            ++magicP;
                            break;
                        case 0:
                            ++type;
                            break;
                        }

                    switch (( (Portal)p ).PortalType)
                    {
                    case CostType.Air:
                        airP += type;
                        break;
                    case CostType.Death:
                        deathP += type;
                        break;
                    case CostType.Earth:
                        earthP += type;
                        break;
                    case CostType.Nature:
                        natureP += type;
                        break;
                    case CostType.Water:
                        waterP += type;
                        break;
                    default:
                        throw new Exception();
                    }
                }
                else
                    throw new Exception();

                //the actual resource for the element is based off of the terrain type
                switch (p.Tile.Terrain)
                {
                case Terrain.Forest:
                    natureP += elemental;
                    break;
                case Terrain.Mountain:
                    earthP += elemental;
                    break;
                case Terrain.Plains:
                    airP += elemental;
                    break;
                case Terrain.Water:
                    waterP += elemental;
                    break;
                default:
                    throw new Exception();
                }
            }
        }
        #endregion //income and upkeep

        #region removing pieces between turn rounds
        internal void RemoveCapturable(Type type, double portalAvg)
        {
            //pick a random piece of the right type
            Piece remove = null;
            foreach (Piece p in Game.Random.Iterate(pieces))
                if (type.IsInstanceOfType(p))
                {
                    remove = p;
                    break;
                }

            if (type == typeof(Portal))
            {
                //if its a portal, receive or lose some compensation for the cost difference
                Portal portal = (Portal)remove;
                portal.PayUpkeep();
                int m, e;
                Player.SplitPortalCost(portal.Owner.Race, portal.PortalType, out m, out e);
                double magicPct = m / (double)( m + e ), elementPct = e / (double)( m + e );
                double diff = portal.GetPortalValue() - portalAvg;
                const double mult = .78;
                magic += Game.Random.Round(diff * mult * magicPct);
                Spend(-Game.Random.Round(diff * mult * elementPct), portal.PortalType, 0);
            }

            remove.Tile.Remove(remove);
            Remove(remove, true);
        }

        internal int RemoveUnit()
        {
            //if you have enough resources, you will lose those instead of a unit
            int loseUnits = RemoveResources();
            for (int i = 0 ; i < loseUnits ; ++i)
            {
                int count = pieces.Count;
                if (count > 0)
                {
                    //all of the players pieces will be units if this method is called
                    Unit unit = (Unit)pieces[Game.Random.Next(count)];
                    double reimburse = GetReimbursement(i == 0, unit.InverseCost, unit.GetHealthPct());

                    int costReimbursement = Game.Random.Round(( unit.BaseOtherCost * reimburse ) / unit.BaseCost);
                    int pplReimbursement = Game.Random.Round(( unit.BasePplCost * reimburse ) / unit.BaseCost);
                    Spend(-costReimbursement, unit.costType, -pplReimbursement);

                    unit.Tile.Remove(unit);
                    this.Remove(unit, true);
                }
            }
            return loseUnits;
        }
        private double GetReimbursement(bool first, double cost, double healthPct)
        {
            return ( cost * healthPct - ( first ? 250 : 0 ) );
        }
        private int RemoveResources()
        {
            const double LoseAmt = 250;
            const double LoseWork = LoseAmt * WorkMult;
            while (work < LoseWork)
            {
                double totalResources = GetTotalResources();
                //check if you could get enough by trading; every loop in case of unlucky rounding
                if (totalResources * WorkMult < LoseWork)
                {
                    int loseUnits = Game.Random.Round(1 - totalResources / LoseAmt);
                    if (loseUnits == 0)
                        AddUpkeep(LoseAmt * UpkeepMult);
                    return loseUnits;
                }
                else
                {
                    //trade a percantage of what you have of each resource
                    air = TradePctAtAvgTrade(air);
                    earth = TradePctAtAvgTrade(earth);
                    nature = TradePctAtAvgTrade(nature);
                    water = TradePctAtAvgTrade(water);
                    death = TradePctAtAvgTrade(death);
                    production = TradePctAtAvgTrade(production);
                    population = TradePctAtAvgTrade(population);
                    magic = TradePctAtRate(magic, 1, .091, 1.3);
                    relic = TradePctAtRate(relic, 1, .091, 1.3);
                    //pass a negative rate so it decreases work
                    upkeep = TradePctAtRate(upkeep, 1 / -UpkeepMult);
                }
            }
            AddWork(-LoseWork);
            return 0;
        }
        private int TradePctAtAvgTrade(int amt)
        {
            return TradePctAtRate(amt, 1);
        }
        private int TradePctAtRate(int amt, double rate)
        {
            return TradePctAtRate(amt, rate, .169, 13);
        }
        private int TradePctAtRate(int amt, double rate, double pct, double add)
        {
            //add a little bit to the percent so itll trade away the dregs
            double tradeAmt = pct * amt + add;
            if (tradeAmt > amt)
                tradeAmt = amt;
            AddWork(tradeAmt * WorkMult * rate);
            return Game.Random.Round(amt - tradeAmt);
        }
        #endregion //removing pieces between turn rounds

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            this.trades = new List<string>();
            this.pics = new Dictionary<string, Bitmap>();
            this.picsConst = new Dictionary<string, Bitmap>();
        }

        #endregion
    }
}
