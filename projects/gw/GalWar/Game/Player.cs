using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _goldValue;
        private uint _research, _lastResearched;
        private float _rDisp, _rDispTrg, _rDispChange, _incomeTotal;
        private double _goldOffset;

        internal Player(int id, Game Game, StartingPlayer player, Planet planet,
                int population, List<int> research)
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
                //the second highest research value is the highest starting design research
                this._lastResearched = (uint)research[2];
                //the highest value is the actual starting research
                this._research = (uint)research[3];

                this._goldValue = 0;
                this._goldOffset = 0;

                this._rDisp = 1;
                this._rDispTrg = 1;
                this._rDispChange = 1;
                //gold and production are added in later
                this._incomeTotal = this.Research;
            }

            this.designs = ShipDesign.GetStartDesigns(this, research);
            foreach (ShipDesign design in this.designs)
                SetPlanetDefense(design);

            //starting production is handled after all players have been created
            NewColony(null, planet, population, 0, 0);
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

                CheckGold();
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

                CheckGold();
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

                CheckGold();
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
        }
        private void AddResearch(int amount, bool random)
        {
            if (random)
                amount = Game.Random.GaussianOEInt(amount, Consts.ResearchRndm, Consts.ResearchRndm, 1);

            checked
            {
                this._research += (uint)amount;
            }
        }
        private int LastResearched
        {
            get
            {
                checked
                {
                    return (int)this._lastResearched;
                }
            }
            set
            {
                checked
                {
                    this._lastResearched = (uint)value;
                }
            }
        }
        public int GetCurrentResearch()
        {
            TurnException.CheckTurn(this);

            return this.Research;
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
                    this._goldValue = (int)Math.Round(value * 10);
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

            int min = Game.Random.GaussianCappedInt(Math.Max(PDAtt, PDDef) * Consts.PlanetDefenseStatRatio + 1, Consts.PlanetDefenseRndm, 1);
            if (PDAtt < min)
                PDAtt = min;
            if (PDDef < min)
                PDDef = min;
        }
        private int GetPDStat(int cur, int add)
        {
            double count = Math.Sqrt(designs.Count + 1);
            return Math.Max(GetPDStat(( count * cur + add ) / ( count + 1 )), GetPDStat(add));
        }
        private static int GetPDStat(double stat)
        {
            return Game.Random.GaussianOEInt(stat + 1, Consts.PlanetDefenseRndm, Consts.PlanetDefenseRndm, 1);
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

        #endregion //fields and constructors

        #region internal

        internal void SetGame(Game game)
        {
            if (this.AI != null)
                this.AI.SetGame(game);
        }

        internal void StartTurn(IEventHandler handler)
        {
            //clear colony changes before research to account for PD upgrading
            foreach (Colony colony in this.colonies)
                colony.ClearChange();

            //actual researching happens at turn start
            HashSet<ShipDesign> obsoleteDesigns;
            ShipDesign newDesign = CheckResearch(out obsoleteDesigns);

            //randomize research display skew
            RandResearchDisplay();

            //gain any levels for exp acquired during enemy turns
            foreach (Ship ship in this.ships)
                ship.StartTurn(handler);

            //consolidate all gold to start the turn
            ConsolidateGold();

            //notify after randomization so the screen shows the correct chance
            if (newDesign != null)
                handler.OnResearch(newDesign, obsoleteDesigns);

            //change build prompt after everything else (true turn start)
            foreach (Colony colony in this.colonies)
                colony.StartTurn(handler);
        }

        private void ConsolidateGold()
        {
            double gold = this.TotalGold;
            this.goldValue = 0;
            this.goldOffset = 0;
            AddGold(gold, true);
        }

        internal ShipDesign FreeResearch(IEventHandler handler, int freeResearch, int designResearch)
        {
            AddResearch(freeResearch, false);
            HashSet<ShipDesign> obsoleteDesigns;
            ShipDesign newDesign = NewShipDesign(false, designResearch, false, out obsoleteDesigns);
            handler.OnResearch(newDesign, obsoleteDesigns);
            return newDesign;
        }

        private ShipDesign CheckResearch(out HashSet<ShipDesign> obsoleteDesigns)
        {
            ShipDesign newDesign = null;
            obsoleteDesigns = null;

            if (this.newResearch > 0)
                AddResearch(this.newResearch, true);

            if (Game.Random.Bool(GetResearchChance(this.newResearch)))
            {
                bool doObsolete = ( this.ResearchFocusDesign != null );
                int minResearch = Game.Random.Round(GetLastResearched());
                if (doObsolete)
                    minResearch = this.ResearchFocusDesign.Research
                            + Game.Random.GaussianCappedInt(Consts.UpgDesignResearch, Consts.UpgDesignRndm, Consts.UpgDesignMin);
                int designResearch = Game.Random.RangeInt(minResearch, this.Research);
                if (doObsolete)
                {
                    double upgResearch = designResearch - this.ResearchFocusDesign.Research;
                    minResearch = Game.Random.GaussianCappedInt(Consts.UpgDesignMin, Consts.UpgDesignRndm, Consts.UpgDesignAbsMin);
                    upgResearch = Consts.LimitMin(upgResearch, minResearch);
                    designResearch = Game.Random.Round(upgResearch + this.ResearchFocusDesign.Research);
                }
                newDesign = NewShipDesign(true, designResearch, doObsolete, out obsoleteDesigns);
            }

            return newDesign;
        }

        private ShipDesign NewShipDesign(bool useFocus, int designResearch, bool doObsolete, out HashSet<ShipDesign> obsoleteDesigns)
        {
            ShipDesign newDesign = new ShipDesign(this, useFocus, designResearch);
            obsoleteDesigns = newDesign.GetObsolete(Game, this.designs);
            if (doObsolete)
                obsoleteDesigns.Add(ResearchFocusDesign);
            foreach (ShipDesign obsoleteDesign in obsoleteDesigns)
                this.designs.Remove(obsoleteDesign);
            //switch to the new production at AutomaticObsoleteLossPct
            foreach (Colony colony in this.colonies)
                if (obsoleteDesigns.Contains(colony.Buildable as ShipDesign))
                    colony.SetBuildable(newDesign, Consts.AutomaticObsoleteLossPct, false);

            this.designs.Add(newDesign);

            SetPlanetDefense(newDesign);
            this.LastResearched = Math.Max(LastResearched, newDesign.Research);

            if (doObsolete)
                ResearchFocusDesign = newDesign;
            return newDesign;
        }

        private void ResetResearchChance(out double rChance, out double rDesignMult)
        {
            double storedResearch = this.Research - GetLastResearched();
            double min = Math.Sqrt(this.Research) * Consts.MinStoredResearchFactor;
            storedResearch = Consts.LimitMin(storedResearch, min);
            rChance = ResetResearchChance(storedResearch, Math.Sqrt(GetLastResearched()) * Consts.NewDesignFactor);

            rDesignMult = 1;
            if (this.ResearchFocusDesign != null)
            {
                storedResearch = this.Research - this.ResearchFocusDesign.Research;
                storedResearch = Consts.LimitMin(storedResearch, Consts.UpgDesignAbsMin);
                rDesignMult = ResetResearchChance(storedResearch, Consts.UpgDesignResearch);
                if (storedResearch < Consts.UpgDesignMin)
                    rDesignMult *= ResetResearchChance(storedResearch, Consts.UpgDesignMin - storedResearch);
            }
        }
        private static double ResetResearchChance(double storedResearch, double factor)
        {
            factor = storedResearch / ( storedResearch + factor );
            factor *= factor;
            return factor;
        }

        private void RandResearchDisplay()
        {
            Player[] research = Game.GetResearchDisplayOrder();
            //the maximum possible skew change can plausibly be accounted for by economy emphasis choices
            double totalIncome = GetTotalIncome();
            double low = totalIncome / ( 1.0 + 2.0 * Consts.EmphasisValue );
            double high = totalIncome * Consts.EmphasisValue / ( Consts.EmphasisValue + 2.0 );
            double diff = ( high - low ) / research[0].ResearchDisplay;

            double add = Game.Random.Gaussian(rDispChange * diff, Consts.ResearchRndm);
            bool sign = ( rDisp > rDispTrg );
            if (sign)
                rDisp -= add;
            else
                rDisp += add;

            if (sign != ( rDisp > rDispTrg ) || rDisp == rDispTrg)
            {
                rDisp = rDispTrg;
                double cap = Math.Max(3 / Consts.ResearchVictoryMin - rDispTrg - 1, 3 + rDispTrg - 3 * Consts.ResearchVictoryMin);
                rDispTrg = ( rDispTrg + Game.Random.GaussianCapped(1, Consts.ResearchDisplayRndm, cap) + 1 ) / 3.0;

                //rate is based on distance to new value
                rDispChange = Consts.FLOAT_ERROR_ZERO + Game.Random.Weighted(1 -
                        Consts.ResearchDisplayRndm / ( Consts.ResearchDisplayRndm + 3 * Math.Abs(rDisp - rDispTrg) ));
            }
        }

        internal void EndTurn(IEventHandler handler)
        {
            AutoRepairShips(handler);

            //income happens at turn end so that it always matches what was expected
            this.IncomeTotal += GetTotalIncome();

            foreach (Ship ship in this.ships)
                ship.EndTurn();

            int research = 0;
            foreach (Colony colony in Game.Random.Iterate<Colony>(this.colonies))
                colony.EndTurn(handler, ref research);
            this.newResearch = research;
        }

        private void CheckGold()
        {
            if (NegativeGold())
            {
                GoldEmphasis = true;
                ResearchEmphasis = false;
                ProductionEmphasis = false;
            }
        }
        public bool NegativeGold()
        {
            TurnException.CheckTurn(this);
            return ( goldValue < 0 );
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
            GoldIncome(gold - rounded);
            this.goldValue += rounded;
        }

        public static void VerifyRounded(double rounded)
        {
            if (Math.Abs(rounded - RoundGold(rounded)) > Consts.FLOAT_ERROR_ZERO)
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
            return Math.Floor(gold * 10 * Consts.FLOAT_ERROR_ONE) / 10.0;
        }

        public static double CeilGold(double gold)
        {
            return Math.Ceiling(gold * 10 / Consts.FLOAT_ERROR_ONE) / 10.0;
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

        internal double GetLastResearched()
        {
            double lastResearched = LastResearched;
            lastResearched = ( 2.0 * lastResearched + 1.0 * this.designs.Max(design => design.Research) ) / 3.0;
            if (lastResearched > this.Research)
                lastResearched = ( 1.0 * lastResearched + 2.0 * this.Research ) / 3.0;
            return lastResearched;
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

                return this.goldValue + .05 / Consts.FLOAT_ERROR_ONE;
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
            GetTurnIncome(out population, out research, out production, out gold, true, true);
            return gold;
        }

        public void GetTurnIncome(out double population, out int research, out double production, out double gold)
        {
            GetTurnIncome(out population, out research, out production, out gold, true);
        }
        public void GetTurnIncome(out double population, out int research, out double production, out double gold, bool includeAll)
        {
            GetTurnIncome(out population, out research, out production, out gold, false, includeAll);
        }
        private void GetTurnIncome(out double population, out int research, out double production, out double gold, bool minGold, bool includeAll)
        {
            TurnException.CheckTurn(this);

            population = 0;
            production = 0;
            gold = 0;
            research = 0;

            foreach (Colony colony in this.colonies)
                colony.GetTurnIncome(ref population, ref production, ref gold, ref research, minGold);
            foreach (Ship ship in this.ships)
                gold -= ship.Upkeep;


            if (includeAll)
            {
                gold += this.goldOffset;
                AutoRepairIncome(ref gold, minGold);
            }
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
            TurnException.CheckTurn(this);

            return GetDesigns();
        }

        internal ReadOnlyCollection<ShipDesign> GetDesigns()
        {
            return designs.AsReadOnly();
        }

        public double GetResearchChance(int newResearch)
        {
            TurnException.CheckTurn(this);

            return GetResearchChance(newResearch, this.ResearchFocusDesign);
        }
        public double GetResearchChance(int newResearch, ShipDesign researchFocusDesign)
        {
            TurnException.CheckTurn(this);

            double newResearchPct = Math.Pow(newResearch / ( newResearch + Math.Sqrt(GetLastResearched()) / Consts.NewResearchMult ), Consts.NewResearchPower);
            double numDesignsPct = Math.Pow(Consts.NumDesignsFactor / ( Consts.NumDesignsFactor + this.designs.Count ), Consts.NumDesignsPower);

            double rChance, rDesignMult;
            ResetResearchChance(out rChance, out rDesignMult);

            double chance = rChance * newResearchPct * numDesignsPct;

            if (researchFocusDesign != null)
                chance *= rDesignMult * Math.Sqrt(this.Research / ( Consts.ResearchFactor + Consts.UpgDesignResearch + researchFocusDesign.Research ));

            return Consts.LimitPct(chance);
        }

        public void MarkObsolete(IEventHandler handler, ShipDesign obsoleteDesign)
        {
            handler = new HandlerWrapper(handler, Game, false, true);
            TurnException.CheckTurn(this);
            AssertException.Assert(obsoleteDesign != null);
            AssertException.Assert(this.designs.Contains(obsoleteDesign));
            AssertException.Assert(this.designs.Count > 1);

            //remove design
            this.designs.Remove(obsoleteDesign);
            //manualy marking a design as obsolete switches build to StoreProd at ManualObsoleteLossPct
            Dictionary<Colony, Tuple<int, bool>> colonies = new Dictionary<Colony, Tuple<int, bool>>();
            foreach (Colony colony in this.colonies)
                if (colony.Buildable == obsoleteDesign)
                {
                    bool oldPause = colony.PauseBuild;
                    int loss = colony.SetBuildableCeilLoss(Game.StoreProd, Consts.ManualObsoleteLossPct, false);
                    colonies.Add(colony, new Tuple<int, bool>(loss, oldPause));
                }
            //can no longer focus research on upgrading design
            bool researchFocus = ( this.ResearchFocusDesign == obsoleteDesign );
            if (researchFocus)
                this.ResearchFocusDesign = null;

            Game.PushUndoCommand(new Game.UndoCommand<ShipDesign, Dictionary<Colony, Tuple<int, bool>>, bool>(
                     new Game.UndoMethod<ShipDesign, Dictionary<Colony, Tuple<int, bool>>, bool>(UndoMarkObsolete),
                     obsoleteDesign, colonies, researchFocus));

            foreach (Colony colony in colonies.Keys)
                colony.ChangeBuild(handler);
        }
        private Tile UndoMarkObsolete(ShipDesign obsoleteDesign, Dictionary<Colony, Tuple<int, bool>> colonies, bool researchFocus)
        {
            AssertException.Assert(!this.designs.Contains(obsoleteDesign));
            AssertException.Assert(obsoleteDesign != null);
            AssertException.Assert(colonies != null);

            this.designs.Add(obsoleteDesign);
            foreach (var pair in colonies)
            {
                AssertException.Assert(pair.Key.Buildable is StoreProd);
                pair.Key.UndoStartBuilding(obsoleteDesign, pair.Value.Item1, pair.Value.Item2);
            }
            if (researchFocus)
                this.ResearchFocusDesign = obsoleteDesign;

            if (colonies.Count == 1)
                foreach (Colony colony in colonies.Keys)
                    return colony.Tile;
            return null;
        }

        public void AutoRepairShips(IEventHandler handler)
        {
            handler = new HandlerWrapper(handler, Game, false);
            TurnException.CheckTurn(this);

            if (this.goldValue > 0)
                foreach (Ship ship in Game.Random.Iterate(this.ships))
                    if (ship.DoAutoRepair)
                    {
                        double cost = this.goldValue / GetAutoRepairCost();
                        if (cost < 1)
                            ship.AutoRepair = ship.GetAutoRepairForHP(ship.GetHPForGold(cost * ship.GetGoldForHP(ship.GetAutoRepairHP())));

                        int hp = Game.Random.Round(ship.GetAutoRepairHP());
                        while (hp > 0 && ship.GetGoldForHP(hp) > this.Gold)
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
                    if (Math.Abs(hp - Math.Round(hp)) < hp * Consts.FLOAT_ERROR_ZERO)
                        gold = Player.RoundGold(gold);
                    cost += gold;
                }
            return cost;
        }
        private void AutoRepairIncome(ref double gold, bool minGold)
        {
            double repairCost = GetAutoRepairCost(minGold);
            if (repairCost > Math.Max(this.goldValue, 0))
                repairCost = Math.Max(this.goldValue, 0);
            gold -= repairCost;
        }

        internal IEnumerable<Tile> PlayTurn(IEventHandler handler, IEnumerable<Tile> anomalies)
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
                anomalies = anomalies.Concat(Game.EndTurn(handler, true));
            }
            return anomalies;
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
