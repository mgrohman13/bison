using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace GalWar
{
    [Serializable]
    public class Player
    {
        #region fields and constructors

        public readonly Game Game;
        public readonly string Name;
        public readonly Color Color;
        public readonly IGalWarAI AI;

        private readonly List<ShipDesign> designs;
        private readonly List<Colony> colonies;
        private readonly List<Ship> ships;

        private readonly byte _id;

        private ShipDesign _researchFocusDesign;

        private bool _goldEmphasis, _researchEmphasis, _productionEmphasis;
        private byte _researchFocus, _pdAtt, _pdDef;
        private ushort _newResearch;
        private uint _research, _lastResearched, _researchGuess, _goldValue;
        private float _rKey, _rChance, _rMult, _rDisp, _rDispTrg, _rDispChange, _incomeTotal;
        private double _goldOffset;

        internal Player(int id, Game Game, StartingPlayer player, Planet planet,
                int population, double soldiers, double gold, List<int> research)
        {
            checked
            {
                this.Game = Game;
                this.Name = player.Name;
                this.Color = player.Color;
                this.AI = player.AI;

                this.colonies = new List<Colony>();
                this.ships = new List<Ship>();

                this._researchFocusDesign = null;

                this._id = (byte)id;

                this._goldEmphasis = false;
                this._researchEmphasis = false;
                this._productionEmphasis = false;

                this._researchFocus = (byte)ShipDesign.FocusStat.None;

                this._pdAtt = 1;
                this._pdDef = 1;

                this._newResearch = 0;
                //the highest research value is the actual starting research
                this._research = (uint)research[3];
                this._lastResearched = (uint)research[2];
                this._researchGuess = (uint)research[3];

                this._goldValue = 0;

                this._rKey = float.NaN;
                this._rChance = float.NaN;
                this._rMult = float.NaN;

                this._rDisp = 1;
                this._rDispTrg = 1;
                this._rDispChange = 1;
                //production is added in later
                this._incomeTotal = (float)( gold + this.Research );

                this._goldOffset = gold;

                this.designs = ShipDesign.GetStartDesigns(this, research);
                foreach (ShipDesign design in this.designs)
                    SetPlanetDefense(design);

                ResetResearchChance();
                //starting production is handled after all players have been created
                NewColony(null, planet, population, soldiers, 0);
            }
        }

        internal int ID
        {
            get
            {
                return this._id;
            }
        }

        public ShipDesign ResearchFocusDesign
        {
            get
            {
                TurnException.CheckTurn(this);

                return this._researchFocusDesign;
            }
            set
            {
                checked
                {
                    TurnException.CheckTurn(this);
                    if (value != null)
                        AssertException.Assert(this.researchFocus == ShipDesign.FocusStat.None);

                    this._researchFocusDesign = value;
                }
            }
        }

        public bool GoldEmphasis
        {
            get
            {
                TurnException.CheckTurn(this);

                return this._goldEmphasis;
            }
            set
            {
                checked
                {
                    TurnException.CheckTurn(this);

                    this._goldEmphasis = value;
                }
            }
        }
        public bool ResearchEmphasis
        {
            get
            {
                TurnException.CheckTurn(this);

                return _researchEmphasis;
            }
            set
            {
                checked
                {
                    TurnException.CheckTurn(this);

                    this._researchEmphasis = value;
                }
            }
        }
        public bool ProductionEmphasis
        {
            get
            {
                TurnException.CheckTurn(this);

                return _productionEmphasis;
            }
            set
            {
                checked
                {
                    TurnException.CheckTurn(this);

                    this._productionEmphasis = value;
                }
            }
        }

        private ShipDesign.FocusStat researchFocus
        {
            get
            {
                return (ShipDesign.FocusStat)this._researchFocus;
            }
            set
            {
                checked
                {
                    this._researchFocus = (byte)value;
                }
            }
        }

        internal int PDAtt
        {
            get
            {
                return this._pdAtt;
            }
            private set
            {
                checked
                {
                    this._pdAtt = (byte)value;
                }
            }
        }
        internal int PDDef
        {
            get
            {
                return this._pdDef;
            }
            private set
            {
                checked
                {
                    this._pdDef = (byte)value;
                }
            }
        }

        private int newResearch
        {
            get
            {
                return this._newResearch;
            }
            set
            {
                checked
                {
                    this._newResearch = (ushort)value;
                }
            }
        }
        internal int Research
        {
            get
            {
                return (int)this._research;
            }
            private set
            {
                checked
                {
                    this._research = (uint)value;
                }
            }
        }
        internal int LastResearched
        {
            get
            {
                return (int)this._lastResearched;
            }
            private set
            {
                checked
                {
                    this._lastResearched = (uint)value;
                }
            }
        }
        public int ResearchGuess
        {
            get
            {
                TurnException.CheckTurn(this);

                return (int)this._researchGuess;
            }
            private set
            {
                checked
                {
                    this._researchGuess = (uint)value;
                }
            }
        }

        private double goldValue
        {
            get
            {
                return this._goldValue / 10.0;
            }
            set
            {
                checked
                {
                    VerifyRounded(value);
                    this._goldValue = (uint)Math.Round(value * 10);
                }
            }
        }

        private double rKey
        {
            get
            {
                return this._rKey;
            }
            set
            {
                checked
                {
                    this._rKey = (float)value;
                }
            }
        }
        private double rChance
        {
            get
            {
                return this._rChance;
            }
            set
            {
                checked
                {
                    this._rChance = (float)value;
                }
            }
        }
        private double rMult
        {
            get
            {
                return this._rMult;
            }
            set
            {
                checked
                {
                    this._rMult = (float)value;
                }
            }
        }

        private double rDisp
        {
            get
            {
                return this._rDisp;
            }
            set
            {
                checked
                {
                    this._rDisp = (float)value;
                }
            }
        }
        private double rDispTrg
        {
            get
            {
                return this._rDispTrg;
            }
            set
            {
                checked
                {
                    this._rDispTrg = (float)value;
                }
            }
        }
        private double rDispChange
        {
            get
            {
                return this._rDispChange;
            }
            set
            {
                checked
                {
                    this._rDispChange = (float)value;
                }
            }
        }

        public double IncomeTotal
        {
            get
            {
                return this._incomeTotal;
            }
            internal set
            {
                checked
                {
                    this._incomeTotal = (float)value;
                }
            }
        }
        private double goldOffset
        {
            get
            {
                return this._goldOffset;
            }
            set
            {
                checked
                {
                    this._goldOffset = value;
                }
            }
        }

        private void SetPlanetDefense(ShipDesign design)
        {
            this.PDAtt = GetPDStat(this.PDAtt, design.Att);
            this.PDDef = GetPDStat(this.PDDef, design.Def);
        }
        private int GetPDStat(int cur, int add)
        {
            return Math.Max(GetPDStat(( cur + add * Consts.PlanetDefenseStatRndm ) / ( 1 + Consts.PlanetDefenseStatRndm )), GetPDStat(add));
        }
        private static int GetPDStat(double stat)
        {
            return Game.Random.GaussianOEInt(stat, Consts.PlanetDefenseStatRndm, Consts.PlanetDefenseStatRndm, 1);
        }

        public int PlanetDefenseAtt
        {
            get
            {
                TurnException.CheckTurn(this);

                return this.PDAtt;
            }
        }
        public int PlanetDefenseDef
        {
            get
            {
                TurnException.CheckTurn(this);

                return this.PDDef;
            }
        }
        public double PlanetDefenseCostPerHP
        {
            get
            {
                TurnException.CheckTurn(this);

                return ShipDesign.GetPlanetDefenseCost(PDAtt, PDDef, this.LastResearched);
            }
        }

        #endregion //fields and constructors

        #region internal

        internal void SetGame(Game game)
        {
            if (this.AI != null)
                this.AI.SetGame(game);
        }

        internal void StartTurn(IEventHandler handler)
        {
            //actual researching happens at turn start
            CheckResearch(handler);

            foreach (Colony colony in this.colonies)
                colony.StartTurn(handler);
            //gain any levels for exp acquired during enemy turns
            foreach (Ship ship in this.ships)
                ship.StartTurn(handler);

            //consolidate all gold to start the turn
            double gold = this.TotalGold;
            this.goldValue = 0;
            this.goldOffset = 0;
            AddGold(gold, true);
        }

        internal void NewRound()
        {
            if (this.newResearch > 0)
                this.Research += Game.Random.GaussianOEInt(this.newResearch, Consts.ResearchRndm, Consts.ResearchRndm, 1);
        }

        internal void FreeResearch(IEventHandler handler, int freeResearch, int designResearch)
        {
            this.ResearchGuess += freeResearch;
            this.Research += freeResearch;
            NewShipDesign(handler, designResearch, false);
        }

        private void CheckResearch(IEventHandler handler)
        {
            if (this.ResearchFocusDesign == null)
                ResearchWithFocus(handler);
            else
                ResearchWithDesign(handler);
            this.newResearch = 0;

            //re-randomize research chance and display skew
            ResetResearchChance();
            RandResearchDisplay();
        }

        private void ResearchWithFocus(IEventHandler handler)
        {
            if (Game.Random.Bool(GetResearchChance(this.newResearch)))
                NewShipDesign(handler, Game.Random.RangeInt(this.LastResearched, this.Research), true);
        }

        private void ResearchWithDesign(IEventHandler handler)
        {
            int chances = Game.Random.OEInt(GetResearchChance(this.newResearch) * 13);
            if (chances > 0)
            {
                SortedSet<int> tries = new SortedSet<int>();
                for (int a = 0 ; a < chances ; ++a)
                {
                    int designResearch = this.Research;
                    while (true)
                    {
                        designResearch = Game.Random.RangeInt(designResearch, ResearchFocusDesign.Research);
                        while (tries.Contains(designResearch))
                            --designResearch;
                        if (designResearch > ResearchFocusDesign.Research)
                            tries.Add(designResearch);
                        else
                            break;
                    }
                }

                ShipDesign tryDesign = ShipDesign.TryUpgradeDesign(this, tries.Reverse(), ResearchFocusDesign);
                if (tryDesign != null)
                    NewShipDesign(handler, tryDesign, true, true);
            }
        }

        private void NewShipDesign(IEventHandler handler, int designResearch, bool checkGuess)
        {
            NewShipDesign(handler, new ShipDesign(this, designResearch), checkGuess, false);
        }
        private void NewShipDesign(IEventHandler handler, ShipDesign newDesign, bool checkGuess, bool doObsolete)
        {
            HashSet<ShipDesign> obsoleteDesigns = newDesign.GetObsolete(Game.MapSize, this.designs);
            if (doObsolete)
                obsoleteDesigns.Add(ResearchFocusDesign);
            foreach (ShipDesign obsoleteDesign in obsoleteDesigns)
                this.designs.Remove(obsoleteDesign);
            //switch to the new production at AutomaticObsoleteLossPct
            foreach (Colony colony in this.colonies)
                if (obsoleteDesigns.Contains(colony.Buildable as ShipDesign))
                    colony.SetBuildable(newDesign, Consts.AutomaticObsoleteLossPct);

            this.designs.Add(newDesign);

            SetPlanetDefense(newDesign);
            this.LastResearched = Math.Max(LastResearched, newDesign.Research);
            if (checkGuess && newDesign.Research > ResearchGuess)
                ResearchGuess = newDesign.Research;

            if (doObsolete)
                ResearchFocusDesign = newDesign;
            handler.OnResearch(newDesign, obsoleteDesigns);
        }

        private void ResetResearchChance()
        {
            this.rKey = Game.Random.FloatHalf();
            this.rChance = Game.Random.NextFloat();
            this.rMult = Game.Random.FloatHalf();
        }

        private void RandResearchDisplay()
        {
            Player[] research = Game.GetResearchOrder();
            //the maximum possible skew change can plausibly be accounted for by economy emphasis choices
            double totalIncome = GetTotalIncome();
            double low = totalIncome * 1 / ( 1 + 2 * Consts.EmphasisValue );
            double high = totalIncome * Consts.EmphasisValue / ( Consts.EmphasisValue + 2 );
            double diff = ( high - low ) / research[0].ResearchDisplay;

            double add = Game.Random.Gaussian(rDispChange * diff, Consts.ResearchDisplayRndm);
            bool sign = ( rDisp > rDispTrg );
            if (sign)
                rDisp -= add;
            else
                rDisp += add;

            if (sign != ( rDisp > rDispTrg ) || rDisp == rDispTrg)
            {
                rDisp = rDispTrg;
                double cap = Math.Max(3 / Consts.ResearchVictoryMult - rDispTrg - 1, 3 + rDispTrg - 3 * Consts.ResearchVictoryMult);
                rDispTrg = ( rDispTrg + Game.Random.GaussianCapped(1, Consts.ResearchDisplayRndm, cap) + 1 ) / 3.0;

                //rate is based on distance to new value
                rDispChange = Consts.FLOAT_ERROR + Game.Random.Weighted(1 -
                        Consts.ResearchDisplayRndm / ( Consts.ResearchDisplayRndm + 3 * Math.Abs(rDisp - rDispTrg) ));
            }
        }

        internal void EndTurn(IEventHandler handler)
        {
            bool neg = MinGoldNegative();

            AutoRepairShips(handler, true);

            //income happens at turn end so that it always matches what was expected
            this.IncomeTotal += GetTotalIncome();

            foreach (Ship ship in this.ships)
                ship.EndTurn();

            double gold = 0;
            int research = 0;
            foreach (Colony colony in Game.Random.Iterate<Colony>(this.colonies))
                colony.EndTurn(handler, ref gold, ref research);
            this.AddGold(gold);
            this.newResearch += research;
            this.ResearchGuess += research;

            if (!neg && NegativeGold())
                throw new Exception();

            CheckGold(handler);
        }

        private void CheckGold(IEventHandler handler)
        {
            if (NegativeGold())
            {
                Dictionary<Colony, int> production = new Dictionary<Colony, int>();
                foreach (Colony colony in this.colonies)
                    if (colony.Production > 0)
                        production.Add(colony, colony.Production);

                //first any production is sold
                while (NegativeGold() && production.Count > 0)
                {
                    Colony colony = Game.Random.SelectValue<Colony>(production);

                    GoldIncome(Consts.GetProductionUpkeepMult(Game.MapSize));
                    colony.SellProduction(handler, 1);

                    if (colony.Production > 0)
                        production[colony] = colony.Production;
                    else
                        production.Remove(colony);
                }

                //then random ships are disbanded for gold
                while (NegativeGold() && this.ships.Count > 0)
                {
                    Ship ship = this.ships[Game.Random.Next(this.ships.Count)];

                    //the upkeep that was just paid for the ship this turn is re-added
                    GoldIncome(ship.Upkeep);
                    ship.Disband(handler, null);
                }
            }
        }

        public bool MinGoldNegative()
        {
            return NegativeGold(RoundGold(GetMinGold()));
        }
        private bool NegativeGold()
        {
            //fudge factor of half-rounding so that NegativeGold will never return true when MinGoldNegative returned false
            return NegativeGold(this.goldOffset + .05);
        }
        private bool NegativeGold(double add)
        {
            return ( this.goldValue + add < -Consts.FLOAT_ERROR );
        }

        internal Colony NewColony(IEventHandler handler, Planet planet, int population, double soldiers, int production)
        {
            Colony colony = new Colony(handler, this, planet, population, soldiers, production);
            this.colonies.Add(colony);
            return colony;
        }

        internal Ship NewShip(IEventHandler handler, Tile tile, ShipDesign design)
        {
            Ship ship = new Ship(handler, this, tile, design);
            AddShip(ship);
            return ship;
        }

        internal void RemoveColony(Colony colony)
        {
            colony.Planet.Colony = null;
            this.colonies.Remove(colony);
        }

        internal void RemoveShip(Ship ship)
        {
            this.ships.Remove(ship);
        }
        internal void AddShip(Ship ship)
        {
            this.ships.Add(ship);
        }

        internal void AddGold(double gold)
        {
            AddGold(gold, false);
        }
        internal void AddGold(double gold, bool random)
        {
            double rounded;
            AddGold(gold, random, out rounded);
        }
        internal void AddGold(double gold, out double rounded)
        {
            AddGold(gold, true, out rounded);
        }
        private void AddGold(double gold, bool random, out double rounded)
        {
            rounded = RoundGold(gold, random);
            AddGold(gold, rounded);
        }
        internal void AddGold(double gold, double rounded)
        {
            if (rounded < -this.goldValue)
                rounded = -this.goldValue;
            GoldIncome(gold - rounded);
            this.goldValue += rounded;
        }

        public static void VerifyRounded(double rounded)
        {
            if (Math.Abs(rounded - RoundGold(rounded)) > Consts.FLOAT_ERROR)
                throw new Exception();
        }

        internal void GoldIncome(double gold)
        {
            this.goldOffset += gold;
        }

        internal void SpendGold(double gold)
        {
            AddGold(-gold);
        }
        internal void SpendGold(double gold, double rounded)
        {
            AddGold(-gold, -rounded);
        }

        public static double RoundGold(double gold)
        {
            return RoundGold(gold, false);
        }
        public static double RoundGold(double gold, bool random)
        {
            if (random)
                return Game.Random.Round(gold * 10) / 10.0;
            else
                return Math.Round(gold, 1);
        }

        public static double FloorGold(double gold)
        {
            return Math.Floor(gold * 10 + Consts.FLOAT_ERROR) / 10.0;
        }

        public static double CeilGold(double gold)
        {
            return Math.Ceiling(gold * 10 - Consts.FLOAT_ERROR) / 10.0;
        }

        internal void DeathCheck()
        {
            if (this.colonies.Count == 0)
            {
                Destroy();
                Game.KillPlayer(this);
            }
        }

        internal void Destroy()
        {
            foreach (Colony colony in this.colonies.ToArray())
            {
                Planet planet = colony.Planet;
                this.RemoveColony(colony);

                //the player will only be destroyed while still having colonies if they won the game
                planet.DamageVictory();
            }
            foreach (Ship ship in this.ships.ToArray())
            {
                ship.Tile.SpaceObject = null;
                this.RemoveShip(ship);
            }
        }

        public int GetLastResearched()
        {
            TurnException.CheckTurn(this);

            return LastResearched;
        }

        #endregion //internal

        #region public

        public bool IsTurn
        {
            get
            {
                return ( this == Game.CurrentPlayer );
            }
        }

        public double Gold
        {
            get
            {
                TurnException.CheckTurn(this);

                return this.goldValue + .05 - Consts.FLOAT_ERROR;
            }
        }
        internal double TotalGold
        {
            get
            {
                return this.goldValue + this.goldOffset;
            }
        }

        internal double ResearchDisplay
        {
            get
            {
                return this.Research * this.rDisp;
            }
        }

        public ShipDesign.FocusStat ResearchFocus
        {
            get
            {
                TurnException.CheckTurn(this);

                return this.researchFocus;
            }
            set
            {
                checked
                {
                    TurnException.CheckTurn(this);
                    if (value != ShipDesign.FocusStat.None)
                        AssertException.Assert(this.ResearchFocusDesign == null);

                    this.researchFocus = value;
                }
            }
        }

        public bool IsFocusing(ShipDesign.FocusStat check)
        {
            TurnException.CheckTurn(this);

            return ShipDesign.IsFocusing(researchFocus, check);
        }
        internal ShipDesign.FocusStat GetResearchFocus()
        {
            return this.researchFocus;
        }

        public double GetArmadaStrength()
        {
            double retVal = 0;
            foreach (Ship ship in this.ships)
                retVal += ( ship.GetStrength() * ship.HP / (double)ship.MaxHP );
            return retVal;
        }

        public int GetTotalQuality()
        {
            int retVal = 0;
            foreach (Colony colony in this.colonies)
                retVal += colony.Planet.Quality;
            return retVal;
        }

        public int GetPopulation()
        {
            int retVal = 0;
            foreach (Colony colony in this.colonies)
                retVal += colony.Population;
            foreach (Ship ship in this.ships)
                retVal += ship.Population;
            return retVal;
        }

        public double GetPopulationGrowth()
        {
            double retVal = 0;
            foreach (Colony colony in this.colonies)
                retVal += colony.GetPopulationGrowth();
            return retVal;
        }

        public double GetTotalIncome()
        {
            double retVal = 0;
            foreach (Colony colony in this.colonies)
                retVal += colony.GetTotalIncome();
            return retVal;
        }

        public double GetMinGold()
        {
            return GetMinGold(true);
        }
        public double GetMinGold(bool countRepair)
        {
            int research;
            double population, production, gold;
            GetTurnIncome(out population, out research, out production, out gold, true, countRepair);
            return gold;
        }

        public void GetTurnIncome(out double population, out int research, out double production, out double gold)
        {
            GetTurnIncome(out population, out research, out production, out gold, true);
        }
        public void GetTurnIncome(out double population, out int research, out double production, out double gold, bool countRepair)
        {
            GetTurnIncome(out population, out research, out production, out gold, false, countRepair);
        }
        private void GetTurnIncome(out double population, out int research, out double production, out double gold, bool minGold, bool countRepair)
        {
            TurnException.CheckTurn(this);

            population = 0;
            production = 0;
            gold = 0;
            research = 0;

            foreach (Colony colony in this.colonies)
                colony.GetTurnIncome(ref population, ref production, ref gold, ref research, minGold);
            foreach (Ship ship in this.ships)
                gold -= ( ship.Upkeep - ship.GetUpkeepReturn() );

            gold += this.goldOffset;

            if (countRepair)
                AutoRepairIncome(ref gold, minGold);
        }

        public ReadOnlyCollection<Colony> GetColonies()
        {
            return colonies.AsReadOnly();
        }

        public ReadOnlyCollection<Ship> GetShips()
        {
            return ships.AsReadOnly();
        }

        public ReadOnlyCollection<ShipDesign> GetShipDesigns()
        {
            return designs.AsReadOnly();
        }

        public double GetResearchChance(int researchInc)
        {
            TurnException.CheckTurn(this);

            double newResearch = this.Research - this.LastResearched;
            if (researchInc > 0 && newResearch > 0)
            {
                double chance = RandResearch(newResearch / ( newResearch + Consts.NewResearchFactor ));

                //parameters that may be modified during a players turn are done after RandResearch
                //so that a change in them doesnt have an inverse or exaggerated effect
                double newResearchPct = Math.Pow(researchInc / ( researchInc + this.LastResearched / Consts.ResearchIncMult ), Consts.ResearchIncPower);
                double numDesignsPct = Math.Pow(Consts.NumDesignsFactor / ( Consts.NumDesignsFactor + this.designs.Count ), Consts.NumDesignsPower);

                return chance * newResearchPct * numDesignsPct;
            }
            return 0;
        }

        //mostly analogous to MTRandom.Weighted, but using constants for the random values
        private double RandResearch(double avg)
        {
            bool neg = avg > .5;
            if (neg)
                avg = 1 - avg;

            double key = rKey * avg;
            if (rChance < ( avg - .5 ) / ( key - .5 ))
                key *= 2;
            else
                key = 1;
            key *= rMult;

            if (neg)
                key = 1 - key;
            return key;
        }

        public void MarkObsolete(IEventHandler handler, ShipDesign obsoleteDesign)
        {
            handler = new HandlerWrapper(handler, this.Game, true, true);
            TurnException.CheckTurn(this);
            AssertException.Assert(obsoleteDesign != null);
            AssertException.Assert(this.designs.Contains(obsoleteDesign));
            AssertException.Assert(this.designs.Count > 1);

            this.designs.Remove(obsoleteDesign);
            //manualy marking a design as obsolete allows build switching at ManualObsoleteLossPct
            foreach (Colony colony in this.colonies)
                if (colony.Buildable == obsoleteDesign)
                {
                    colony.SetBuildable(Game.StoreProd, Consts.ManualObsoleteLossPct);
                    colony.StartBuilding(handler, handler.getNewBuild(colony));
                }

            if (this.ResearchFocusDesign == obsoleteDesign)
                this.ResearchFocusDesign = null;
        }

        public void AutoRepairShips(IEventHandler handler)
        {
            handler = new HandlerWrapper(handler, this.Game, false);
            TurnException.CheckTurn(this);

            AutoRepairShips(handler, false);
        }
        private void AutoRepairShips(IEventHandler handler, bool checkGoldLoss)
        {
            double goldLoss = 0;
            if (checkGoldLoss)
            {
                goldLoss = GetMinGold(false);
                if (goldLoss > 0)
                    goldLoss = 0;
            }

            foreach (Ship ship in Game.Random.Iterate(this.ships))
                if (ship.DoAutoRepair)
                {
                    double cost = ( this.goldValue + goldLoss ) / GetAutoRepairCost();
                    if (cost < 1)
                        ship.AutoRepair = ship.GetAutoRepairForHP(ship.GetHPForGold(cost * ship.GetGoldForHP(ship.GetAutoRepairHP())));

                    int hp = Game.Random.Round(ship.GetAutoRepairHP());
                    while (hp > 0 && ship.GetGoldForHP(hp) > this.Gold + FloorGold(goldLoss))
                        --hp;
                    if (hp > 0)
                        ship.GoldRepair(handler, hp);
                }
        }
        public double GetAutoRepairCost()
        {
            return GetAutoRepairCost(false);
        }
        public double GetAutoRepairCost(bool minGold)
        {
            double cost = 0;
            foreach (Ship ship in this.ships)
                if (ship.DoAutoRepair)
                {
                    double hp = ship.GetAutoRepairHP();
                    if (minGold)
                        hp = Math.Ceiling(hp);
                    double gold = ship.GetGoldForHP(hp);
                    if (Math.Abs(hp - Math.Round(hp)) < Consts.FLOAT_ERROR)
                        gold = Player.RoundGold(gold);
                    cost += gold;
                }
            return cost;
        }
        private void AutoRepairIncome(ref double gold, bool minGold)
        {
            double repairGold = gold;
            if (!minGold)
                repairGold = GetMinGold(false);
            if (repairGold > 0)
                repairGold = 0;
            repairGold += this.goldValue;
            if (repairGold > 0)
            {
                double repairCost = GetAutoRepairCost(minGold);
                if (repairCost > repairGold)
                    repairCost = repairGold;

                gold -= repairCost;
            }
        }

        internal void PlayTurn(IEventHandler handler)
        {
            if (AI != null)
            {
                try
                {
                    AI.PlayTurn(handler);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Game.EndTurn(handler, true);
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion //public

        public class StartingPlayer
        {
            public readonly string Name;
            public readonly Color Color;
            public readonly IGalWarAI AI;

            public StartingPlayer(string name, Color color, IGalWarAI AI)
            {
                this.Name = name;
                this.Color = color;
                this.AI = AI;
            }
        }
    }
}
