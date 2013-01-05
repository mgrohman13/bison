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

        private PlanetDefense planetDefense;

        private readonly List<ShipDesign> designs;
        private readonly List<Ship> ships;
        private readonly List<Colony> colonies;

        private bool _goldEmphasis, _researchEmphasis, _productionEmphasis;
        private byte _id;
        private ushort _newResearch;
        private uint _research, _lastResearched, _goldValue;
        private float _incomeTotal, _rKey, _rChance, _rMult, _rDisp, _rDispTrg, _rDispChange;
        private double _goldOffset;

        public Player(string name, Color color, IGalWarAI AI)
        {
            this.Name = name;
            this.Color = color;
            this.AI = AI;
        }

        internal Player(int id, Game Game, Player player, Planet planet,
                int population, double soldiers, double gold, List<int> research)
        {
            this.Game = Game;

            this.Name = player.Name;
            this.Color = player.Color;

            this.AI = player.AI;

            this.ships = new List<Ship>();

            this._goldEmphasis = false;
            this._researchEmphasis = false;
            this._productionEmphasis = false;
            checked
            {
                this._id = (byte)id;
            }
            this.goldValue = 0;
            this.goldOffset = gold;

            //the highest research value is the actual starting research
            this.Research = research[3];
            this.newResearch = 0;
            this.LastResearched = research[2];

            this.designs = new List<ShipDesign>();
            this.designs.AddRange(ShipDesign.GetStartDesigns(research, this));
            this.planetDefense = new PlanetDefense(this, this.designs);

            this.colonies = new List<Colony>();
            //starting production is handled after all players have been created
            NewColony(null, planet, population, soldiers, 0);

            //production is added in later
            this.IncomeTotal = gold + Consts.StartResearch;

            ResetResearchChance();
            _rDisp = 1;
            _rDispTrg = 1;
            _rDispChange = 1;
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

        private double goldOffset
        {
            get
            {
                return this._goldOffset;
            }
            set
            {
                this._goldOffset = value;
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
                VerifyRounded(value);
                checked
                {
                    this._goldValue = (uint)Math.Round(value * 10);
                }
            }
        }

        #endregion //fields and constructors

        #region internal

        internal byte ID
        {
            get
            {
                return _id;
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
            double gold = this.goldValue + this.goldOffset;
            this.goldValue = 0;
            this.goldOffset = 0;
            AddGold(gold, true);
        }

        internal void NewRound()
        {
            if (this.newResearch > 0)
                this.Research += Game.Random.GaussianCappedInt(this.newResearch, Consts.ResearchRndm, 1);
        }

        internal void FreeResearch(double research)
        {
            throw new NotImplementedException();
        }

        private void CheckResearch(IEventHandler handler)
        {
            if (this.newResearch > 0)
            {
                if (Game.Random.Bool(GetResearchChance(this.newResearch)))
                    NewShipDesign(handler);
                this.newResearch = 0;
            }

            //re-randomize research chance and display skew
            ResetResearchChance();
            RandResearchDisplay();
        }

        private void NewShipDesign(IEventHandler handler)
        {
            //only a random portion of total research can be used in the new design
            int designResearch = this.Research - this.LastResearched;
            if (designResearch > 1)
                designResearch = Game.Random.RangeInt(1, designResearch);
            designResearch += this.LastResearched;

            ShipDesign newDesign = new ShipDesign(designResearch, this, ShipDesign.FocusStat.None);

            HashSet<ShipDesign> obsoleteDesigns = newDesign.GetObsolete(Game.MapSize, this.designs);
            foreach (ShipDesign obsoleteDesign in obsoleteDesigns)
                this.designs.Remove(obsoleteDesign);
            //switch to the new production at AutomaticObsoleteLossPct
            foreach (Colony colony in this.colonies)
                if (obsoleteDesigns.Contains(colony.Buildable as ShipDesign))
                    colony.SetBuildable(newDesign, Consts.AutomaticObsoleteLossPct);

            this.designs.Add(newDesign);
            PlanetDefense old = new PlanetDefense(this.planetDefense);
            this.planetDefense.GetStats(newDesign);

            this.LastResearched = designResearch;

            handler.OnResearch(newDesign, obsoleteDesigns, old, this.planetDefense);
        }

        private void ResetResearchChance()
        {
            this._rKey = Game.Random.FloatHalf();
            this._rChance = Game.Random.NextFloat();
            this._rMult = Game.Random.FloatHalf();
        }

        private void RandResearchDisplay()
        {
            Player[] research = Game.GetResearchOrder();
            if (research.Length > 1)
            {
                double totalIncome = GetTotalIncome();
                double low = totalIncome * 1 / ( 1 + 2 * Consts.EmphasisValue );
                double high = totalIncome * Consts.EmphasisValue / ( Consts.EmphasisValue + 2 );
                float diff = (float)( ( high - low ) / research[1].ResearchDisplay );

                float add = Game.Random.Gaussian(_rDispChange * diff, Consts.ResearchDisplayRndm);
                bool sign = ( _rDisp > _rDispTrg );
                if (sign)
                    _rDisp -= add;
                else
                    _rDisp += add;

                if (sign != ( _rDisp > _rDispTrg ) || _rDisp == _rDispTrg)
                {
                    _rDisp = _rDispTrg;
                    _rDispTrg = ( _rDispTrg + Game.Random.GaussianCapped(1, Consts.ResearchDisplayRndm) + 1 ) / 3;
                    _rDispChange = (float)Consts.FLOAT_ERROR + Game.Random.Weighted(1 -
                            Consts.ResearchDisplayRndm / ( Consts.ResearchDisplayRndm + Math.Abs(_rDisp - _rDispTrg) * 3 ));
                }
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

            int research = 0;
            foreach (Colony colony in Game.Random.Iterate<Colony>(this.colonies))
                colony.EndTurn(handler, ref this._goldOffset, ref research);
            this.newResearch += research;

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

                    AddGold(Consts.GetProductionUpkeepMult(Game.MapSize));
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
                    AddGold(ship.Upkeep);
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
            this.ships.Add(ship);
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

        internal void AddGold(double gold)
        {
            AddGold(gold, false);
        }
        internal void AddGold(double gold, bool random)
        {
            double rounded;
            AddGold(gold, random, out rounded);
        }
        internal void AddGold(double gold, bool random, out double rounded)
        {
            if (random)
                rounded = Game.Random.Round(gold * 10) / 10.0;
            else
                rounded = RoundGold(gold);
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
            return Math.Round(gold, 1);
        }

        public static double FloorGold(double gold)
        {
            return Math.Floor(gold * 10 + Consts.FLOAT_ERROR) / 10;
        }

        public static double CeilGold(double gold)
        {
            return Math.Ceiling(gold * 10 - Consts.FLOAT_ERROR) / 10;
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

        public double IncomeTotal
        {
            get
            {
                return this._incomeTotal;
            }
            internal set
            {
                this._incomeTotal = (float)value;
            }
        }

        internal double ResearchDisplay
        {
            get
            {
                return this.Research * this._rDisp;
            }
        }

        public bool GoldEmphasis
        {
            get
            {
                TurnException.CheckTurn(this);

                return this._goldEmphasis;
            }
        }
        public bool ResearchEmphasis
        {
            get
            {
                TurnException.CheckTurn(this);

                return _researchEmphasis;
            }
        }
        public bool ProductionEmphasis
        {
            get
            {
                TurnException.CheckTurn(this);

                return _productionEmphasis;
            }
        }

        public void SetGoldEmphasis(IEventHandler handler, bool value)
        {
            handler = new HandlerWrapper(handler, this.Game, false);
            TurnException.CheckTurn(this);

            this._goldEmphasis = value;
        }
        public void SetResearchEmphasis(IEventHandler handler, bool value)
        {
            handler = new HandlerWrapper(handler, this.Game, false);
            TurnException.CheckTurn(this);

            this._researchEmphasis = value;
        }
        public void SetProductionEmphasis(IEventHandler handler, bool value)
        {
            handler = new HandlerWrapper(handler, this.Game, false);
            TurnException.CheckTurn(this);

            this._productionEmphasis = value;
        }

        public PlanetDefense PlanetDefense
        {
            get
            {
                TurnException.CheckTurn(this);

                return this.planetDefense;
            }
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

            float newResearch = this.Research - this.LastResearched;
            if (researchInc > 0 && newResearch > 0)
            {
                double chance = RandResearch(newResearch / ( newResearch + Consts.NewResearchFactor ));

                //parameters that may be modified during a players turn are done after RandResearch
                //so that a change in them doesnt have an inverse or exaggerated effect
                double newResearchPct = Math.Pow(researchInc / (double)( researchInc + this.LastResearched / Consts.ResearchIncMult ), Consts.ResearchIncPower);
                double numDesignsPct = Math.Pow(Consts.NumDesignsFactor / ( Consts.NumDesignsFactor + this.designs.Count ), Consts.NumDesignsPower);

                return chance * newResearchPct * numDesignsPct;
            }
            return 0;
        }

        //analogous to MTRandom.Weighted, but using constants for the random values
        private double RandResearch(double avg)
        {
            bool neg = avg > .5;
            if (neg)
                avg = 1 - avg;

            double key = _rKey * avg;
            if (_rChance < ( avg - .5 ) / ( key - .5 ))
                key *= 2;
            else
                key = 1;
            key *= _rMult;

            if (neg)
                key = 1 - key;
            return key;
        }

        //additionalLossPct and accountForIncome are only there to be passed back into the handler; they do not affect the call itself
        public void MarkObsolete(IEventHandler handler, ShipDesign obsoleteDesign, bool accountForIncome, params double[] additionalLosses)
        {
            handler = new HandlerWrapper(handler, this.Game, false);
            TurnException.CheckTurn(this);
            AssertException.Assert(obsoleteDesign != null);
            AssertException.Assert(this.designs.Contains(obsoleteDesign));

            double[] losses = new double[additionalLosses.Length + 1];
            losses[0] = Consts.ManualObsoleteLossPct;
            Array.Copy(additionalLosses, 0, losses, 1, additionalLosses.Length);

            this.designs.Remove(obsoleteDesign);
            //manualy marking a design as obsolete allows build switching at ManualObsoleteLossPct
            foreach (Colony colony in this.colonies)
                if (colony.Buildable == obsoleteDesign)
                    colony.SetBuildable(handler.getNewBuild(colony, accountForIncome, false, losses), Consts.ManualObsoleteLossPct);
        }

        public void AutoRepairShips(IEventHandler handler)
        {
            handler = new HandlerWrapper(handler, this.Game);
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
                        ship.AutoRepair = ship.GetAutoRepairForHP(ship.GetHPForGold(ship.GetGoldForHP(ship.GetAutoRepairHP()) * cost));

                    int hp = Game.Random.Round(ship.GetAutoRepairHP());
                    while (hp > 0 && ship.GetGoldForHP(hp) > this.Gold + RoundGold(goldLoss))
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
                    cost += ship.GetGoldForHP(hp);
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
    }
}
