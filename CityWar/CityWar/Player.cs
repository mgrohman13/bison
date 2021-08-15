using System;
using System.Collections.Generic;
using System.Linq;
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

        public const int WizardCost = 1300, RelicCost = 300, NoCaptRelicPenalty = 33;
        public const int TradeDown = 9, TradeUp = 30;
        public const double WorkMult = (TradeDown + (TradeUp - TradeDown) / 3.0) / 10.0;
        public const double UpkeepMult = (TradeUp - (TradeUp - TradeDown) / 3.0) / 10.0;
        public const double TurnUpkPct = .13;

        private static float zoom = -1;

        [NonSerialized]
        private Dictionary<string, Bitmap> pics = new Dictionary<string, Bitmap>(),
                picsConst = new Dictionary<string, Bitmap>();

        public readonly string Name;
        public readonly Color Color;
        public readonly string Race;

        private Game game;
        private List<Piece> pieces;
        private List<string> trades;
        private int death, air, earth, nature, water, population, production, relicOffset, magic, work;
        private double upkeep, healRound;

        private int _relic;

        //template player to pass into a new game
        public Player(string race, Color color, string name)
        {
            this.Name = name;
            this.Color = color;
            this.Race = race;
        }

        //real player for in-game use
        internal void NewPlayer(Game game, bool city, string[] startUnits, double totalStartCost)
        {
            this.game = game;
            this.pieces = new List<Piece>();
            this.trades = new List<string>();
            this.death = 0;
            this.air = 0;
            this.earth = 0;
            this.nature = 0;
            this.water = 0;
            this.population = 0;
            this.production = 0;
            this.relicOffset = Game.Random.RangeInt(1, RelicCost - 1);
            this.magic = 0;
            this.work = 0;
            this.upkeep = 0;
            this.healRound = Game.Random.NextDouble();

            this._relic = Game.Random.Round(RelicCost / 2.0);

            double thisTotal = 0;
            if (city)
            {
                //stronger starting units means the concentrated armies of wizard players is more of a threat
                double mult = (Math.Pow(totalStartCost, .39) * 10.4);
                int[] prodPop = AddStartResources(Game.Random.Round(5.2 * mult), ref magic,
                        Game.Random.Round(2.6 * mult), Game.Random.Round(3.9 * mult), 2);
                this.production += prodPop[0];
                this.population += prodPop[1];

                for (int a = -1; ++a < 3;)
                {
                    Tile t = RandomStartTile(false);

                    new City(this, t);
                    thisTotal += Unit.NewUnit(startUnits[a], t, this).BaseTotalCost;
                }
            }
            else
            {
                int maxRelic = Math.Min(Game.Random.Round(RelicCost * .78), RelicCost + relicOffset - relic - 1);
                int numTypes = Game.Random.GaussianCappedInt(3.5, .21, 2);
                int[] elementals = AddStartResources(390, ref _relic, RelicCost - maxRelic, maxRelic, numTypes);

                Action<int>[] typeFuncs = new Action<int>[] {
                    amt => this.air += amt,
                    amt => this.earth += amt,
                    amt => this.nature += amt,
                    amt => this.water += amt,
                };
                int idx = 0;
                foreach (int type in Game.Random.Iterate(typeFuncs.Length).Take(numTypes - 1))
                    typeFuncs[type](elementals[idx++]);

                int pop = Game.Random.WeightedInt(elementals[idx], .78);
                this.population += pop;
                this.death += elementals[idx] - pop;

                Tile t = RandomStartTile(true);

                new Wizard(this, t, out _);
                new Relic(this, t);

                for (int i = -1; ++i < startUnits.Length;)
                    thisTotal += Unit.NewUnit(startUnits[i], t, this).BaseTotalCost;
            }

            BalanceForUnit(totalStartCost, thisTotal);
        }
        private static int[] AddStartResources(int totalAmt, ref int mainType, int minMain, int maxMain, int numOthers)
        {
            int addAmt = Game.Random.RangeInt(minMain, maxMain);
            mainType += addAmt;
            totalAmt -= addAmt;

            int[] retVal = new int[numOthers];
            foreach (int val in Game.Random.Iterate(numOthers))
            {
                if (numOthers > 1)
                    addAmt = Game.Random.RangeInt(0, Game.Random.Round(2 * totalAmt / (double)(numOthers--)));
                else
                    addAmt = totalAmt;
                retVal[val] = addAmt;
                totalAmt -= addAmt;
            }
            return retVal;
        }
        private Tile RandomStartTile(bool canEdge)
        {
            return game.RandomTile(neighbor => (canEdge || neighbor != null) && (neighbor == null || !neighbor.Occupied()));
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

            pieces.Sort((p1, p2) => p1.Group - p2.Group);

            //find the last index of a piece that is selected and start there
            int index;
            for (index = count; --index > -1;)
                if (pieces[index].Group == currentGroup)
                    break;

            bool stop = false;
            while (true)
            {
                if (++index < count)
                {
                    if (pieces[index].Movement > 0 && pieces[index].Group != currentGroup)
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
                return relic + RelicCost - relicOffset;
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
            return GetTurnUpkeep(upkeep);
        }
        private static double GetTurnUpkeep(double upkeep)
        {
            return upkeep * TurnUpkPct;
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
                case "Nature":
                    return nature;
                case "Production":
                    return production;
                case "Water":
                    return water;
                default:
                    throw new Exception();
            }
        }

        public double GetTotalResources()
        {
            return GetTotalResources(true);
        }
        private double GetTotalResources(bool all)
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
            if (all)
            {
                result += work / WorkMult;
                result -= upkeep / UpkeepMult;
            }

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

        internal bool Dead
        {
            get
            {
                return (pieces.Count < 1);
            }
        }

        public void BalanceForUnit(double avg, double mine)
        {
            avg -= mine;
            if (avg > 0)
                AddWork(avg * WorkMult);
            else
                AddUpkeep(-avg * UpkeepMult, .039);
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
            if (!turnIncrement && Dead)
                KillPlayer();
        }

        internal void LostCapt(Capturable c, Player p)
        {
            //the capturing player gets stuck with some of the other players upkeep
            int wizards, portals, cities, relics;
            this.GetCounts(out wizards, out portals, out cities, out relics, out _);
            double transferAmt = this.upkeep / (wizards + portals + cities + relics) / 2.1;
            this.AddUpkeep(-transferAmt);
            p.AddUpkeep(transferAmt);

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
            relic += Game.Random.GaussianInt(amt, .26);
        }
        internal void AddDeath(double amt)
        {
            death += Game.Random.Round(amt);
        }
        internal void AddWork(double amt)
        {
            work += Game.Random.Round(amt);
        }
        internal void AddUpkeep(double amount, double devPct = .065)
        {
            upkeep += Game.Random.Gaussian(amount, devPct);
        }

        internal static void SubtractCommonUpkeep(Player[] players)
        {
            double minUpk = players.Min(player => player.upkeep);
            minUpk = Game.Random.Weighted(minUpk, .78);

            double minWork = players.Min(player => player.work - GetTurnUpkeep(player.upkeep - minUpk));
            if (minWork > 0)
                minUpk -= Game.Random.Weighted(minWork / TurnUpkPct / 2.1, .169);

            foreach (Player p in players)
                p.AddUpkeep(-minUpk);
        }

        internal Unit FreeUnit(string name, double avgCost)
        {
            Unit unit = FreeUnit(name, RandomCapturable());
            BalanceForUnit(avgCost, unit.BaseTotalCost);
            return unit;
        }
        internal Unit FreeUnit(string unit, Capturable cur)
        {
            return Unit.NewUnit(unit, cur.Tile, this);
        }

        internal void CollectWizardPts(double points, Terrain? terrain = null)
        {
            Action<int> population = (amt => this.population += amt);
            Action<int> production = (amt => this.production += amt);
            Action<int> magic = (amt => this.magic += amt);
            Action<int> relic = (amt => this.relic += amt);
            Action<int> death = (amt => this.death += amt);
            Action<int> air = (amt => this.air += amt);
            Action<int> earth = (amt => this.earth += amt);
            Action<int> nature = (amt => this.nature += amt);
            Action<int> water = (amt => this.water += amt);

            Dictionary<Action<int>, int> typeFuncs;
            if (terrain == null)
            {
                //not really wizard points, but for turn order balancing
                //total: 131
                typeFuncs = new Dictionary<Action<int>, int>() {
                    { relic, 3 },
                    { magic, 5 },
                    { air, 13 },
                    { earth, 13 },
                    { nature, 13 },
                    { water, 13 },
                    { death, 15 },
                    { population, 27 },
                    { production, 29 },
                };
            }
            else
            {
                //for actual wizard points
                //total: 137
                typeFuncs = new Dictionary<Action<int>, int>() {
                    { production, 1 },
                    { population, 2 },
                    { magic, 4 },
                    { relic, 9 },
                    { death, 17 },
                    //these will really be 22 or 38, based on terrain
                    { air, 26 },
                    { earth, 26 },
                    { nature, 26 },
                    { water, 26 },
                };
                const int mod = 4;
                Action<int> terrainFunc;
                switch (terrain.Value)
                {
                    case Terrain.Forest:
                        terrainFunc = nature;
                        break;
                    case Terrain.Mountain:
                        terrainFunc = earth;
                        break;
                    case Terrain.Plains:
                        terrainFunc = air;
                        break;
                    case Terrain.Water:
                        terrainFunc = water;
                        break;
                    default:
                        throw new Exception();
                }
                typeFuncs[terrainFunc] += mod * 4;
                typeFuncs[air] -= mod;
                typeFuncs[earth] -= mod;
                typeFuncs[nature] -= mod;
                typeFuncs[water] -= mod;
            }

            int whole = (int)points;
            double fraction = (whole == points ? 1 : points - whole++);
            while (--whole > -1)
            {
                double avg = 50 * (whole == 0 ? fraction : 1);
                Game.Random.SelectValue(typeFuncs)(Game.Random.GaussianCappedInt(avg, .21, avg > 2 ? Game.Random.Round(avg * .52) : 0));
            }
        }

        internal void EndTurn()
        {
            foreach (Piece piece in pieces.ToArray())
                piece.ResetMove();

            GenerateIncome(ref air, ref death, ref earth, ref nature, ref production, ref water, ref magic, ref population);
            PayUpkeep();

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
            game.DefeatPlayer(this);

            trades.Clear();

            foreach (Bitmap image in picsConst.Values)
                image.Dispose();
            foreach (Bitmap image in pics.Values)
                image.Dispose();
            picsConst.Clear();
            pics.Clear();
        }

        #endregion //internal methods

        #region portal cost

        public static int TotalPortalCost(Game game, string race, CostType costType)
        {
            int[] cost = Player.SplitPortalCost(game, race)[costType];
            return cost[0] + cost[1];
        }
        public static void SplitPortalCost(Game game, string race, CostType costType, out int magic, out int element)
        {
            int[] retVal = SplitPortalCost(game, race)[costType];
            magic = retVal[0];
            element = retVal[1];
        }
        public static Dictionary<CostType, int[]> SplitPortalCost(Game game, string race)
        {
            Dictionary<CostType, int[]> portalCosts = new Dictionary<CostType, int[]>();

            double[] elmDbl = new double[5];
            int[] elmInt = new int[5];
            double[] other = new double[5];
            double[] ppl = new double[5];

            foreach (string name in Game.Races[race])
            {
                Unit unit = Unit.CreateTempUnit(game, name);
                int idx = getCTIdx(unit.CostType);
                if (idx > -1)
                {
                    elmDbl[idx] += unit.BaseTotalCost + unit.BaseOtherCost / 2.1;
                    ++elmInt[idx];

                    double div = Math.Sqrt(unit.BaseTotalCost);
                    other[idx] += unit.BaseOtherCost / div;
                    ppl[idx] += unit.BasePplCost / div;
                }
            }

            double total = 0;
            for (int idx = 0; idx < 5; ++idx)
            {
                elmDbl[idx] = GetTotalPortalCost(elmDbl[idx], elmInt[idx]);
                total += elmDbl[idx];
            }

            int totInt = 0;
            for (int idx = 0; idx < 4; ++idx)
            {
                elmInt[idx] = (int)Math.Round(elmDbl[idx] * Portal.AvgPortalCost * 5.0 / total);
                totInt += elmInt[idx];
            }
            elmInt[4] = Portal.AvgPortalCost * 5 - totInt;

            for (int idx = 0; idx < 5; ++idx)
            {
                int totalCost = elmInt[idx];
                int element = GetPortalElementCost(other[idx] / (other[idx] + ppl[idx]), totalCost);
                int magic = totalCost - element;
                portalCosts.Add(getIdxCT(idx), new int[] { magic, element });
            }

            return portalCosts;
        }
        private static double GetTotalPortalCost(double totCost, int numUnits)
        {
            //the greater the number of units and their cost, the greater the total cost of the portal
            return Math.Pow(totCost * (numUnits + 1), .39);
        }
        public static int GetPortalElementCost(double elemPct, double totalCost)
        {
            ////the more population the units cost, the less magic the portal costs
            return (int)Math.Ceiling((1 - (elemPct * elemPct * .666 + .21)) * totalCost);

            ////the more population the units cost, the more magic the portal costs
            //elemPct *= elemPct;
            //if (elemPct <= .26)
            //    return 1;
            //elemPct = (0.65 * (elemPct - 0.26));
            //return (int)Math.Ceiling(elemPct * totalCost);
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

        internal int relic
        {
            get
            {
                return _relic;
            }
            private set
            {
                _relic = value;
                GetRelic();
            }
        }
        private bool GetRelic()
        {
            if (relicOffset < 0)
                relicOffset = Game.Random.RangeInt(0, RelicCost);

            if (relic - relicOffset >= RelicCost)
            {
                Piece cur = RandomCapturable();
                bool noCapts = (cur == null);
                if (noCapts && relic - relicOffset >= RelicCost + NoCaptRelicPenalty)
                    cur = Game.Random.SelectValue(pieces);

                if (cur != null)
                {
                    relicOffset = int.MinValue;
                    _relic -= RelicCost + (noCapts ? NoCaptRelicPenalty : 0);
                    new Relic(this, cur.Tile);

                    bool another = GetRelic();
                    if (another)
                    { }
                    return true;
                }
            }

            return false;
        }
        private Capturable RandomCapturable()
        {
            IEnumerable<Capturable> capturables = pieces.OfType<Capturable>();
            if (capturables.Any())
                return Game.Random.SelectValue(capturables);
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
            bool file = (constPic || name.EndsWith("Unit"));
            if (file)
                pic = LoadPicFromFile(loadName, portalColor);
            else
                pic = GetConstPic(name);

            if (constPic)
                picsConst.Add(name, pic);
            else
                pics.Add(name, ResizePic(pic, file));
        }
        private Bitmap LoadPicFromFile(string name, Color portalColor)
        {
            Bitmap basePic;
            try
            {
                basePic = new Bitmap(Game.ResourcePath + "pics\\" + name + ".bmp");
            }
            catch
            {
                basePic = new Bitmap(Game.ResourcePath + "pics\\notFound.bmp");
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
        private Bitmap ResizePic(Bitmap pic, bool dispose)
        {
            Bitmap newPic = new Bitmap(pic, Game.Random.Round(zoom * 5f / 6f), Game.Random.Round(zoom * 5f / 6f));
            if (dispose)
                pic.Dispose();
            return newPic;
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
                    value += (Game.Random.Bool() ? -1 : 1);
            return value;
        }

        private void TradeMagic()
        {
            magic = Trade(false, magic, "ma");
        }
        private void TradeRelic()
        {
            relic = Trade(false, relic, "re");
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
            if (this != game.CurrentPlayer || (up && work < 1) || (!up && amt < 1))
                return amt;

            if (up)
            {
                if (trades.Contains(id + "d"))
                {
                    if (work < TradeDown)
                    {
                        amt += Game.Random.Round(10 * work / (double)TradeDown);
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
                        amt += Game.Random.Round(10 * work / (double)TradeUp);
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
                        work += Game.Random.Round(TradeUp * amt / 10.0);
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
                        work += Game.Random.Round(TradeDown * amt / 10.0);
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
            double payment = -(GetTurnUpkeep());
            AddWork(payment);

            //unit upkeep
            double total = 0;
            foreach (Piece p in pieces)
            {
                Unit u = (p as Unit);
                if (u != null && u.Type != UnitType.Immobile)
                    total += GetUpkeep(u);
            }
            AddUpkeep(total + payment);
        }
        internal static double GetUpkeep(Unit u)
        {
            return (u.RandedCost * UpkeepMult / 210.0);
        }

        private void CheckNegativeWork()
        {
            //when your work is negative, you will lose some actual resources
            while (Game.Random.Round(work / 10.0) < 0 && GetTotalResources(false) > 0)
            {
                int amt;
                switch (Game.Random.Next(16))
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
                        switch (Game.Random.Next(8))
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                amt = magic;
                                TradeMagic();
                                break;
                            case 5:
                            case 6:
                            case 7:
                                amt = relic;
                                TradeRelic();
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
            return retVal;
        }

        internal void StartTurn()
        {
            CheckNegativeResources();
            trades.Clear();
        }

        internal void CheckNegativeResources()
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

            if (upkeep < 0)
            {
                AddWork(-upkeep / UpkeepMult * WorkMult);
                upkeep = 0;
            }
            else if (upkeep > 0)
            {
                CheckNegativeWork();
            }
            if (work < 0)
            {
                AddUpkeep(-work / WorkMult * UpkeepMult);
                work = 0;
            }
        }

        private void CheckNegativeResource(ref int amt)
        {
            if (amt < 0)
            {
                AddUpkeep(-amt * UpkeepMult);
                amt = 0;
            }
        }

        //this doesnt actually add the income to the player so it can be used to simply view the income
        public void GenerateIncome(ref int airInc, ref int deathInc, ref int earthInc, ref int natureInc,
                ref int prodInc, ref int waterInc, ref int magicInc, ref int popInc)
        {
            GenerateIncome(this.pieces.OfType<Capturable>(), ref airInc, ref deathInc,
                    ref earthInc, ref natureInc, ref prodInc, ref waterInc, ref magicInc, ref popInc);
        }
        private static void GenerateIncome(IEnumerable<Capturable> capturables, ref int air, ref int death,
                ref int earth, ref int nature, ref int prod, ref int water, ref int magic, ref int pop)
        {
            pop += 15;
            magic += 10;
            prod += 5;

            foreach (Capturable capturable in capturables)
                if (!capturable.EarnedIncome)
                {
                    int elemental = 0;

                    Portal portal;
                    if (capturable is Wizard)
                    {
                        //cost 1300
                        //roi 16.25-43.33
                        elemental += 30;
                        //rest: +10 (roi 32.50)
                        //find: +50

                        //88.01% collection needed for average portal roi (52.67% for relic)
                    }
                    else if (capturable is City)
                    {
                        prod += 13;
                        magic += 7;
                        elemental += 6;
                        pop += 3;
                        death += 1;
                        //+30
                    }
                    else if (capturable is Relic)
                    {
                        //cost 300
                        //roi 23.08
                        magic += 6;
                        elemental += 5;
                        pop += 2;
                        //+13
                    }
                    else if ((portal = capturable as Portal) != null)
                    {
                        //avg cost 1000 (673-1461)
                        //avg roi 17.57 (15.64-19.51)
                        int amt = portal.Income;

                        int type = 0, position = 0;
                        for (; amt > 0; --amt)
                            switch (++position % 6)
                            {
                                case 1:
                                    ++type;
                                    break;
                                case 2:
                                    ++elemental;
                                    break;
                                case 3:
                                    ++type;
                                    break;
                                case 4:
                                    ++magic;
                                    break;
                                case 5:
                                    ++type;
                                    break;
                                case 0:
                                    ++elemental;
                                    break;
                            }

                        switch (portal.Type)
                        {
                            case CostType.Air:
                                air += type;
                                break;
                            case CostType.Death:
                                death += type;
                                break;
                            case CostType.Earth:
                                earth += type;
                                break;
                            case CostType.Nature:
                                nature += type;
                                break;
                            case CostType.Water:
                                water += type;
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    else
                        throw new Exception();

                    //the actual resource for the element is based off of the terrain type
                    switch (capturable.Tile.Terrain)
                    {
                        case Terrain.Forest:
                            nature += elemental;
                            break;
                        case Terrain.Mountain:
                            earth += elemental;
                            break;
                        case Terrain.Plains:
                            air += elemental;
                            break;
                        case Terrain.Water:
                            water += elemental;
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
            Piece remove = Game.Random.SelectValue(pieces.Where(piece => type.IsInstanceOfType(piece)));

            if (type == typeof(Portal))
            {
                //if its a portal, receive or lose some compensation for the cost difference
                Portal portal = (Portal)remove;
                int m, e;
                Player.SplitPortalCost(portal.Owner.game, portal.Owner.Race, portal.Type, out m, out e);
                double magicPct = m / (double)(m + e), elementPct = e / (double)(m + e);
                double diff = portal.GetPortalValue() - portalAvg;
                const double mult = .87;
                magic += Game.Random.Round(diff * mult * magicPct);
                Spend(-Game.Random.Round(diff * mult * elementPct), portal.Type, 0);
            }

            remove.Tile.Remove(remove);
            Remove(remove, true);
        }

        internal void RemoveUnit()
        {
            //if you have enough resources, you will lose those instead of a unit
            int loseUnits = RemoveResources();
            for (int i = 0; i < loseUnits; ++i)
                if (pieces.Any())
                {
                    //all of the players pieces will be units if this method is called
                    Unit unit = (Unit)Game.Random.SelectValue(pieces);
                    double reimburse = GetReimbursement(i == 0, unit.InverseCost, unit.GetHealthPct());

                    int costReimbursement = Game.Random.Round((unit.BaseOtherCost * reimburse) / unit.BaseTotalCost);
                    int pplReimbursement = Game.Random.Round((unit.BasePplCost * reimburse) / unit.BaseTotalCost);
                    Spend(-costReimbursement, unit.CostType, -pplReimbursement);

                    unit.Tile.Remove(unit);
                    this.Remove(unit, true);
                }
        }
        private const double noCapLoseAmt = 1000 / 3.0;
        private double GetReimbursement(bool first, double cost, double healthPct)
        {
            return (cost * healthPct - (first ? noCapLoseAmt : 0));
        }
        private int RemoveResources()
        {
            int loseUnits = 0;
            const double LoseWork = noCapLoseAmt * WorkMult;
            while (work < LoseWork)
            {
                double totalResources = GetTotalResources();
                //check if you could get enough by trading; every loop in case of unlucky rounding
                if (totalResources < noCapLoseAmt)
                {
                    loseUnits = Game.Random.Round(1 - totalResources / noCapLoseAmt);
                    if (loseUnits == 0)
                    {
                        TradeAll(ref air);
                        TradeAll(ref earth);
                        TradeAll(ref nature);
                        TradeAll(ref water);
                        TradeAll(ref death);
                        TradeAll(ref production);
                        TradeAll(ref population);
                        TradeAll(ref magic);
                        TradeAll(ref _relic);
                        //TradePct(ref upkeep, -1 / UpkeepMult, 1, 0);
                    }
                    break;
                }
                else
                {
                    //trade a percentage of what you have of each resource
                    TradePct(ref air);
                    TradePct(ref earth);
                    TradePct(ref nature);
                    TradePct(ref water);
                    TradePct(ref death);
                    TradePct(ref production);
                    TradePct(ref population);
                    TradePct(ref magic, 1, .013, 1.3);
                    TradePct(ref _relic, 1, .013, 1.3);
                    ////pass a negative rate so it decreases work
                    //TradePct(ref upkeep, -1 / UpkeepMult, .078, 0);
                }
            }
            if (loseUnits == 0)
                AddWork(-LoseWork);
            return loseUnits;
        }
        private void TradeAll(ref int amt)
        {
            TradePct(ref amt, 1, 1, 0);
        }
        private void TradePct(ref int amt)
        {
            TradePct(ref amt, 1);
        }
        private void TradePct(ref int amt, double rate)
        {
            TradePct(ref amt, rate, .169, 13);
        }
        private void TradePct(ref int amt, double rate, double pct, double add)
        {
            if (amt > 0)
            {
                int tradeAmt = amt;
                if (pct < 1)
                {
                    //add a small constant amount so itll trade away the dregs
                    tradeAmt = Game.Random.GaussianOEInt(pct * amt + add, .52, .13);
                    if (tradeAmt > amt)
                        tradeAmt = amt;
                }
                AddWork(tradeAmt * WorkMult * rate);
                amt -= tradeAmt;
            }
        }

        #endregion //removing pieces between turn rounds

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            pics = new Dictionary<string, Bitmap>();
            picsConst = new Dictionary<string, Bitmap>();
        }

        #endregion
    }
}
