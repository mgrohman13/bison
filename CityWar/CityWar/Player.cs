using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace CityWar
{
    [Serializable]
    public class Player : IDeserializationCallback
    {
        #region fields and constructors

        public const int WizardCost = 1300, RelicCost = 300, NoCaptRelicPenalty = 30;
        public const int TradeDown = 9, TradeUp = 30;
        public const double WorkMult = (TradeDown + (TradeUp - TradeDown) / 3.0) / 10.0;
        public const double UpkeepMult = (TradeUp - (TradeUp - TradeDown) / 3.0) / 10.0;
        public const double TurnUpkPct = .13;

        [NonSerialized]
        private Dictionary<string, Bitmap> pics = new(), picsConst = new();

        public readonly string Name;
        public readonly Color Color;
        public readonly string Race;

        public int StartOrder { get; private set; }
        public bool StartCity { get; private set; }

        public double LastRelicScore { get; private set; }

        private Game game;
        private List<Piece> pieces;
        private List<string> trades;
        private int death, air, earth, nature, water, population, production, relicOffset, magic, work;
        private double upkeep, healRound, score;

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
            StartCity = city;
            LastRelicScore = 0;

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
            this.score = 0;

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
                    thisTotal += Unit.NewUnit(startUnits[a], t, this).RandedCost;
                }
            }
            else
            {
                int maxRelic = Math.Min(260, RelicCost + relicOffset - Relic - 1);
                int numTypes = Game.Random.GaussianCappedInt(3.5, .21, 2);
                int[] elementals = AddStartResources(390, ref _relic, Game.Random.RangeInt(78, 130), maxRelic, numTypes);

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
                    thisTotal += Unit.NewUnit(startUnits[i], t, this).RandedCost;
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
        internal void SetStartOrder(int currentPlayer)
        {
            StartOrder = currentPlayer;
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

            pieces.Sort((p1, p2) => Math.Sign((long)p1.Group - (long)p2.Group));

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

        public double Score
        {
            get
            {
                return score;
            }
            internal set
            {
                score = value;
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
        public int RelicProgress
        {
            get
            {
                return Relic + RelicCost - relicOffset;
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
            return resource switch
            {
                "Air" => air,
                "Death" => death,
                "Earth" => earth,
                "Nature" => nature,
                "Production" => production,
                "Water" => water,
                _ => throw new Exception(),
            };
        }

        public double CountTradeableResources()
        {
            return CountResources(true, false, false);
        }
        internal double CountTotalResources()
        {
            return CountResources(true, true, true);
        }
        private double CountResources(bool workUpk, bool inclWiz, bool inclRel)
        {
            double result = 0;

            result += death;
            result += air;
            result += earth;
            result += nature;
            result += water;
            result += production;
            result += population;

            if (workUpk)
            {
                result += work / WorkMult;
                result -= upkeep / UpkeepMult;
            }
            if (inclWiz)
                result += magic;
            if (inclRel)
                result += Relic;

            return result;
        }
        public double GetArmyStrength()
        {
            double retVal = 0;
            foreach (Piece p in pieces)
            {
                if (p is Unit u)
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
                AddUpkeep(-avg * UpkeepMult, .052);
        }

        internal void Add(Piece p)
        {
            pieces.Add(p);
            if (p is Capturable)
                GetRelic();
        }
        internal void Remove(Piece p)
        {
            pieces.Remove(p);
            if (Dead)
                KillPlayer();
        }

        internal void LostCapt(Capturable c, Player p)
        {
            //the capturing player gets stuck with some of the other players upkeep 
            this.GetCounts(out int wizards, out int portals, out int cities, out int relics, out _);
            double transferAmt = this.upkeep / (wizards + portals + cities + relics) / 2.1;
            this.AddUpkeep(-transferAmt);
            p.AddUpkeep(transferAmt);

            Remove(c);
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

        internal void AddScore(double amt)
        {
            score += amt;
            AddRelic(amt, .26);
        }
        internal void AddRelic(double amt, double dev, double avg)
        {
            LastRelicScore -= avg;
            AddRelic(amt, dev);
        }
        private void AddRelic(double amt, double dev)
        {
            Relic += Game.Random.GaussianInt(amt, dev);
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
            BalanceForUnit(avgCost, unit.RandedCost);
            return unit;
        }
        internal Unit FreeUnit(string unit, Capturable cur)
        {
            return Unit.NewUnit(unit, cur.Tile, this);
        }

        internal void CollectResources(bool wizardTreasure, int points, Terrain? terrain = null)
        {
            Action<int> population = (amt => this.population += amt);
            Action<int> production = (amt => this.production += amt);
            Action<int> magic = (amt => this.magic += amt);
            Action<int> relic = (amt => this.Relic += amt);
            Action<int> death = (amt => this.death += amt);
            Action<int> air = (amt => this.air += amt);
            Action<int> earth = (amt => this.earth += amt);
            Action<int> nature = (amt => this.nature += amt);
            Action<int> water = (amt => this.water += amt);

            Dictionary<Action<int>, int> typeFuncs;
            if (!wizardTreasure)
            {
                // for turn order balancing
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
                //for wizard points (collectible treasure)
                //total: 117
                typeFuncs = new Dictionary<Action<int>, int>() {
                    { death, 13 },
                    //these will really be 21 or 41, based on terrain
                    { air, 26 },
                    { earth, 26 },
                    { nature, 26 },
                    { water, 26 },
                };
                const int mod = 5;
                Action<int> terrainFunc = terrain.Value switch
                {
                    Terrain.Forest => nature,
                    Terrain.Mountain => earth,
                    Terrain.Plains => air,
                    Terrain.Water => water,
                    _ => throw new Exception(),
                };
                typeFuncs[terrainFunc] += mod * 4;
                typeFuncs[air] -= mod;
                typeFuncs[earth] -= mod;
                typeFuncs[nature] -= mod;
                typeFuncs[water] -= mod;
            }

            for (int a = 0; a < points; a++)
                Game.Random.SelectValue(typeFuncs)(Game.Random.GaussianCappedInt(50, .21, 26));
        }

        internal void EndTurn()
        {
            GenerateIncome(ref air, ref death, ref earth, ref nature, ref production, ref water, ref magic, ref population);
            foreach (Piece piece in pieces.ToArray())
                piece.ResetMove();
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

        #region relic

        internal int Relic
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

            if (Relic - relicOffset >= RelicCost)
            {
                Piece cur = RandomCapturable();
                bool noCapts = (cur == null);
                if (noCapts && Relic - relicOffset >= RelicCost + NoCaptRelicPenalty)
                    cur = Game.Random.SelectValue(pieces);

                if (cur != null)
                {
                    LastRelicScore = this.score;
                    relicOffset = int.MinValue;
                    _relic -= RelicCost + (noCapts ? NoCaptRelicPenalty : 0);
                    new Relic(this, cur.Tile);

                    bool another = GetRelic();
                    if (another)
                        ;
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
            return ImageUtil.GetPic(this, name, pics, picsConst, false);
        }
        public Bitmap GetConstPic(string name)
        {
            return ImageUtil.GetPic(this, name, pics, picsConst, true);
        }
        public static void ResetPics(Player[] players)
        {
            foreach (Player p in players)
            {
                foreach (Bitmap image in p.pics.Values)
                    image.Dispose();
                p.pics.Clear();
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
            Relic = Trade(false, Relic, "re");
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
            bool inclWizRel = pieces.OfType<Capturable>().Any();
            //when your work is negative, you will lose some actual resources
            while (Game.Random.Round(work / 10.0) < 0 && CountResources(false, inclWizRel, inclWizRel) > 0)
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
                if (inclWizRel)
                    while (Game.Random.Bool(1 - GetRandVal(amt)))
                        if (magic > 0 || Relic > 0)
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
                                    amt = Relic;
                                    TradeRelic();
                                    break;
                                default:
                                    throw new Exception();
                            }
                        }
                        else
                        {
                            break;
                        }
                else
                    ;
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
                if (!capturable.EarnedIncome || capturable.Owner.game.CurrentPlayer != capturable.Owner)
                {
                    int elemental = 0;

                    if (capturable is Wizard)
                    {
                        //cost 1300
                        //roi 43.33
                        elemental += 30;
                        //+30
                        //rest: +40 (roi 32.50)
                        //collect: +80 (roi 16.25)
                        //89% collection needed for average portal roi (53% for relic)
                    }
                    else if (capturable is City)
                    {
                        //cost 169.6  
                        //roi 5.65  
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
                    else if (capturable is Portal portal)
                    {
                        //avg cost 1000 
                        //avg roi 17.5 
                        int type = 0;
                        for (int a = 0; a < portal.Income; a++)
                            switch (a % 6)
                            {
                                case 0:
                                    ++type;
                                    break;
                                case 1:
                                    ++elemental;
                                    break;
                                case 2:
                                    ++type;
                                    break;
                                case 3:
                                    ++magic;
                                    break;
                                case 4:
                                    ++type;
                                    break;
                                case 5:
                                    ++elemental;
                                    break;
                                default: throw new Exception();
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
                            default: throw new Exception();
                        }
                    }
                    else throw new Exception();

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
                Portal.SplitPortalCost(portal.Owner.game, portal.Owner.Race, portal.Type, out int m, out int e);
                double magicPct = m / (double)(m + e), elementPct = e / (double)(m + e);
                double diff = portal.GetPortalValue() - portalAvg;
                const double mult = 1;
                magic += Game.Random.Round(diff * mult * magicPct);
                Spend(-Game.Random.Round(diff * mult * elementPct), portal.Type, 0);
            }

            remove.Tile.Remove(remove);
            Remove(remove);
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
                    this.Remove(unit);
                }
        }
        private const double noCapLoseAmt = 1000 / 3.0;
        private double GetReimbursement(bool first, double cost, double healthPct)
        {
            return (cost * healthPct - (first ? noCapLoseAmt : 0));
        }
        private int RemoveResources()
        {
            if (CountTradeableResources() > noCapLoseAmt)
            {
                //if you can afford to lose only tradeable resources, always do that first
                RemoveResources(false, false);
                return 0;
            }

            //the lower your army strength, the more likely you will lose wizard/relic resources
            double str = GetArmyStrength();
            const double strFactor = 3900;
            double c2 = strFactor / (str + strFactor);
            c2 *= c2;
            //the farther away you are from getting a wizard or relic, the more likely you will lose those resources
            bool include(int amt, int cost)
            {
                //if on your last piece, must lose resources (no other options)
                if (pieces.Count <= 1 || amt <= 0)
                    return true;
                //if can immediately get one, never lose it if you dont have to
                if (amt >= cost)
                    return false;
                double c1 = (cost - amt) / (double)cost;
                //since the metrics are multiplied together, you have to be both far away from getting one and have low army strength to have a high chance of losing the resource
                return Game.Random.Bool(c1 * c1 * c2);
            };
            bool inclWiz = include(magic, WizardCost);
            // use public 0-600 RelicProgress value (indicates how close you really are to a relic)
            bool inclRel = include(RelicProgress, 2 * RelicCost);

            if (inclWiz || inclRel)
            {
                //potentially lose units if not enough resources even including the wizard/relic resources
                double resources = CountResources(true, inclWiz, inclRel);
                int loseUnits = GetLoseUnits(resources);
                if (loseUnits > 0)
                    return loseUnits;

                if (resources > noCapLoseAmt || (inclWiz && inclRel))
                {
                    //if not losing units, lose resources
                    RemoveResources(inclWiz, inclRel);
                    return 0;
                }

                //if not losing either one of the two and not enough resources, always lose a unit (otherwise upkeep could force trade it)
                return 1;
            }

            //if not losing wizard/relic resources and not enough tradeable, always lose units
            return Math.Max(1, GetLoseUnits(CountTradeableResources()));
        }
        private static int GetLoseUnits(double resources)
        {
            //if upkeep is high enough for resources to be negative, can lose multiple units 
            return Game.Random.Round(1 - resources / noCapLoseAmt);
        }
        private void RemoveResources(bool inclWiz, bool inclRel)
        {
            double pct = noCapLoseAmt / CountResources(true, inclWiz, inclRel);

            TradePct(ref air, pct);
            TradePct(ref earth, pct);
            TradePct(ref nature, pct);
            TradePct(ref water, pct);
            TradePct(ref death, pct);
            TradePct(ref production, pct);
            TradePct(ref population, pct);
            if (inclWiz)
                TradePct(ref magic, pct);
            if (inclRel)
                TradePct(ref _relic, pct);

            const double loseWork = noCapLoseAmt * WorkMult;
            if (loseWork > work)
            {
                upkeep += Game.Random.Round((loseWork - work) / WorkMult * UpkeepMult);
                work = 0;
            }
            else
            {
                work -= Game.Random.Round(loseWork);
            }
        }
        private void TradePct(ref int amt, double pct)
        {
            if (amt > 0)
            {
                //add a small constant amount so itll trade away the dregs
                int tradeAmt = Game.Random.GaussianCappedInt(pct * amt + 13, .39);
                if (tradeAmt > amt)
                    tradeAmt = amt;

                work += Game.Random.Round(tradeAmt * WorkMult);
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
