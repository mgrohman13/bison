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

        private readonly List<ShipDesign> designs;
        private readonly List<Ship> ships;
        private readonly List<Colony> colonies;

        private bool _goldEmphasis, _researchEmphasis, _productionEmphasis;
        private byte _id;
        private ushort _research, _newResearch, _lastResearched;
        private float _incomeTotal, _rKey, _rChance, _rMult;
        private double _gold;

        private PlanetDefense planetDefense;

        public Player(string name, Color color)
        {
            this.Name = name;
            this.Color = color;
        }

        internal Player(int id, Game Game, Player player, Planet planet,
                int population, double soldiers, double gold, List<int> research, out int needProd)
        {
            this.Game = Game;

            this.Name = player.Name;
            this.Color = player.Color;

            this.ships = new List<Ship>();

            this._goldEmphasis = false;
            this._researchEmphasis = false;
            this._productionEmphasis = false;
            checked
            {
                this._id = (byte)id;
            }
            AddGold(gold);

            //the highest research value is the actual starting research
            this.Research = research[3];
            this.newResearch = 0;
            this.LastResearched = research[2];

            this.designs = new List<ShipDesign>();
            needProd = ShipDesign.GetStartDesigns(Game.TotalTiles, research, this, this.designs, Game.ShipNames);
            this.planetDefense = new PlanetDefense(this, this.designs);

            this.colonies = new List<Colony>();
            //starting production is handled after all players have been created
            NewColony(planet, population, soldiers, 0, null);

            //production is added in later
            this.IncomeTotal = gold + this.Research;

            ResetResearchChance();
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

        #endregion //fields and constructors

        #region internal

        internal int ID
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
                return this._research;
            }
            private set
            {
                checked
                {
                    this._research = (ushort)value;
                }
            }
        }

        internal void StartTurn(IEventHandler handler)
        {
            //actual researching happens at turn start
            CheckResearch(handler);

            foreach (Colony colony in this.colonies)
                colony.StartTurn(handler);
            //gain any levels for exp acquired during enemy turns
            foreach (Ship ship in this.ships)
                ship.LevelUp(handler);
        }

        private void CheckResearch(IEventHandler handler)
        {
            if (this.newResearch > 0)
            {
                //new design chance happens before randomization so that it always matches what was expected
                bool newDesign = Game.Random.Bool(GetResearchChance(this.newResearch));

                //newResearch was calculated at the end of the last turn; now it is used and cleared
                int researchInc = Game.Random.GaussianCappedInt(this.newResearch, Consts.ResearchRndm, 1);
                this.newResearch = 0;

                if (newDesign)
                    NewShipDesign(researchInc, handler);

                this.Research += researchInc;
            }

            //re-randomize research chance
            ResetResearchChance();
        }

        private void NewShipDesign(int researchInc, IEventHandler handler)
        {
            //only a random portion of total research can be used in the new design
            int designResearch = this.Research + researchInc - this.LastResearched;
            if (designResearch > 1)
                designResearch = Game.Random.RangeInt(1, designResearch);
            designResearch += this.LastResearched;

            ShipDesign newDesign = new ShipDesign(Game.TotalTiles, designResearch, this, this.designs, this.Game.ShipNames);

            HashSet<ShipDesign> obsoleteDesigns = newDesign.GetObsolete(Game.TotalTiles, this.designs);
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

        internal void EndTurn(IEventHandler handler)
        {
            //income happens at turn end so that it always matches what was expected
            this.IncomeTotal += GetTotalIncome();

            foreach (Ship ship in this.ships)
                SpendGold(ship.EndTurn());
            int research = 0;
            foreach (Colony colony in Game.Random.Iterate<Colony>(this.colonies))
                colony.EndTurn(ref this._gold, ref research, handler);
            this.newResearch += research;

            CheckGold();
        }

        private void CheckGold()
        {
            if (this.Gold < 0)
            {
                Dictionary<Colony, int> production = new Dictionary<Colony, int>();
                foreach (Colony colony in this.colonies)
                    if (colony.Production > 0)
                        production.Add(colony, colony.Production);

                //first any production is sold
                while (this.Gold < 0 && production.Count > 0)
                {
                    Colony colony = Game.Random.SelectValue<Colony>(production);
                    colony.SellProduction(1);
                    if (colony.Production > 0)
                        production[colony] = colony.Production;
                    else
                        production.Remove(colony);
                }

                //then random ships are disbanded for gold
                while (this.Gold < 0 && this.ships.Count > 0)
                {
                    Ship ship = this.ships[Game.Random.Next(this.ships.Count)];
                    //the upkeep that was just paid for the ship this turn is re-added
                    AddGold(ship.Upkeep);
                    ship.Disband(null);
                }
            }
        }

        internal Colony NewColony(Planet planet, int population, double soldiers, int production, IEventHandler handler)
        {
            Colony colony = new Colony(this, planet, population, soldiers, production, handler);
            this.colonies.Add(colony);
            return colony;
        }

        internal Ship NewShip(Tile tile, ShipDesign design, IEventHandler handler)
        {
            Ship ship = new Ship(this, tile, design, handler);
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
            this._gold += gold;
        }

        internal void SpendGold(double gold)
        {
            AddGold(-gold);
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
            foreach (Colony colony in this.GetColonies())
            {
                Planet planet = colony.Planet;
                this.RemoveColony(colony);

                //the player will only be destroyed while still having colonies if they won the game
                planet.DamageVictory();
            }
            foreach (Ship ship in this.GetShips())
            {
                ship.Tile.SpaceObject = null;
                this.RemoveShip(ship);
            }
        }

        internal int LastResearched
        {
            get
            {
                return this._lastResearched;
            }
            private set
            {
                checked
                {
                    this._lastResearched = (ushort)value;
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

                return this._gold;
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

        public bool GoldEmphasis
        {
            get
            {
                TurnException.CheckTurn(this);

                return this._goldEmphasis;
            }
            set
            {
                TurnException.CheckTurn(this);

                this._goldEmphasis = value;
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
                TurnException.CheckTurn(this);

                this._researchEmphasis = value;
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
                TurnException.CheckTurn(this);

                this._productionEmphasis = value;
            }
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
            int research;
            double population, production, gold;
            GetTurnIncome(out population, out research, out production, out gold, true);
            return gold;
        }

        public void GetTurnIncome(out double population, out int research, out double production, out double gold)
        {
            GetTurnIncome(out population, out research, out production, out gold, false);
        }

        private void GetTurnIncome(out double population, out int research, out double production, out double gold, bool minGold)
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
                float chance = RandResearch(newResearch / ( newResearch + Consts.NewResearchFactor ));

                //parameters that may be modified during a players turn are done after RandResearch
                //so that a change in them doesnt have an inverse or exaggerated effect
                double newResearchPct = Math.Pow(researchInc / (double)( researchInc + this.LastResearched / Consts.ResearchIncMult ), Consts.ResearchIncPower);
                double numDesignsPct = Math.Pow(Consts.NumDesignsFactor / ( Consts.NumDesignsFactor + this.designs.Count ), Consts.NumDesignsPower);

                return chance * newResearchPct * numDesignsPct;
            }
            return 0;
        }

        //analogous to MTRandom.Weighted, but using constants for the random values
        private float RandResearch(float avg)
        {
            bool neg = avg > .5;
            if (neg)
                avg = 1 - avg;

            float key = _rKey * avg;
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
        public void MarkObsolete(ShipDesign obsoleteDesign, IEventHandler handler, bool accountForIncome, params double[] additionalLosses)
        {
            TurnException.CheckTurn(this);
            AssertException.Assert(obsoleteDesign != null);
            AssertException.Assert(this.designs.Contains(obsoleteDesign));
            handler = new HandlerWrapper(handler);

            double[] losses = new double[additionalLosses.Length + 1];
            losses[0] = Consts.ManualObsoleteLossPct;
            Array.Copy(additionalLosses, 0, losses, 1, additionalLosses.Length);

            this.designs.Remove(obsoleteDesign);
            //manualy marking a design as obsolete allows build switching at ManualObsoleteLossPct
            foreach (Colony colony in this.colonies)
                if (colony.Buildable == obsoleteDesign)
                    colony.SetBuildable(handler.getNewBuild(colony, accountForIncome, false, losses), Consts.ManualObsoleteLossPct);
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion //public
    }
}
