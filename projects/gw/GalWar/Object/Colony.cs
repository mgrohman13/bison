using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace GalWar
{
    [Serializable]
    public class Colony : Combatant
    {
        #region fields and constructors

        public readonly Planet Planet;

        private readonly Player _player;

        private readonly HashSet<Buildable> buildable;
        private Buildable _curBuild;
        private Ship _repairShip;

        private bool _built, _pauseBuild;
        private sbyte _defenseAttChange, _defenseDefChange;
        private short _defenseHPChange;
        private float _defenseResearch, _soldierChange, _prodGuess, _researchRounding, _productionRounding;

        internal Colony(IEventHandler handler, Player player, Planet planet, int population, double soldiers, int production)
            : base(null, 1, 1, 0, population, soldiers)
        {
            checked
            {
                this.Planet = planet;
                planet.Colony = this;

                this._player = player;

                //set the build intially to StoreProd so it can be changed to anything with no production loss
                StoreProd storeProd = new StoreProd(this, production);
                this.buildable = new HashSet<Buildable>();
                this.buildable.Add(new BuildGold(this));
                this.buildable.Add(storeProd);
                this.buildable.Add(new BuildAttack(this));
                this.buildable.Add(new BuildDefense(this));
                this.buildable.UnionWith(player.GetShipDesigns().Select(design => new BuildShip(this, design)));
                this._curBuild = storeProd;

                this._repairShip = null;

                this._built = ( handler == null );
                this._pauseBuild = false;

                this._defenseAttChange = 0;
                this._defenseDefChange = 0;
                this._defenseHPChange = 0;

                this._defenseResearch = (float)player.Game.PDResearch;

                this._soldierChange = 0;
                this._prodGuess = 0;

                this._researchRounding = float.NaN;
                this._productionRounding = float.NaN;

                ResetRounding();
                if (handler != null)
                    ChangeBuild(handler);
            }
        }

        public override Player Player
        {
            get
            {
                return this._player;
            }
        }

        public Buildable CurBuild
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this._curBuild;
            }
            private set
            {
                checked
                {
                    if (value == null || !this.buildable.Contains(value))
                        throw new Exception();
                    this._curBuild = value;
                }
            }
        }
        public HashSet<Buildable> Buildable
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return new HashSet<Buildable>(this.buildable);
            }
        }
        public bool PauseBuild
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return ( this._pauseBuild && ( this.CurBuild is ShipDesign ) );
            }
            private set
            {
                checked
                {
                    this._pauseBuild = value;
                }
            }
        }
        public Ship RepairShip
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                Ship repairShip = this._repairShip;
                if (repairShip != null &&
                        ( repairShip.HP == repairShip.MaxHP || repairShip.Dead
                        || !Tile.IsNeighbor(this.Tile, repairShip.Tile) ))
                    this.RepairShip = repairShip = null;

                return repairShip;
            }
            set
            {
                checked
                {
                    TurnException.CheckTurn(this.Player);
                    AssertException.Assert(value == null || this.Player == value.Player);

                    Ship repairShip = this._repairShip;
                    if (repairShip != value)
                    {
                        if (repairShip != null && repairShip.AutoRepair == 0)
                            repairShip.AutoRepair = double.NaN;
                        if (value != null)
                            value.AutoRepair = 0;

                        this._repairShip = value;
                    }
                }
            }
        }

        private bool built
        {
            get
            {
                return this._built;
            }
            set
            {
                checked
                {
                    this._built = value;
                }
            }
        }

        public int DefenseAttChange
        {
            get
            {
                return this._defenseAttChange;
            }
            private set
            {
                checked
                {
                    this._defenseAttChange = (sbyte)value;
                }
            }
        }
        public int DefenseDefChange
        {
            get
            {
                return this._defenseDefChange;
            }
            private set
            {
                checked
                {
                    this._defenseDefChange = (sbyte)value;
                }
            }
        }
        public int DefenseHPChange
        {
            get
            {
                return this._defenseHPChange;
            }
            private set
            {
                checked
                {
                    this._defenseHPChange = (short)value;
                }
            }
        }

        internal int production2
        {
            get
            {
                return buildable.Sum(build => build.Production);
            }
        }

        private double defenseResearch
        {
            get
            {
                return this._defenseResearch;
            }
            set
            {
                checked
                {
                    this._defenseResearch = (float)value;
                }
            }
        }

        public double SoldierChange
        {
            get
            {
                return this._soldierChange;
            }
            private set
            {
                checked
                {
                    this._soldierChange = (float)value;
                }
            }
        }

        public double ProdGuess
        {
            get
            {
                return this._prodGuess;
            }
            internal set
            {
                checked
                {
                    this._prodGuess = (float)value;
                }
            }
        }

        private double researchRounding
        {
            get
            {
                return this._researchRounding;
            }
            set
            {
                checked
                {
                    this._researchRounding = (float)value;
                }
            }
        }
        private double productionRounding
        {
            get
            {
                return this._productionRounding;
            }
            set
            {
                checked
                {
                    this._productionRounding = (float)value;
                }
            }
        }

        #endregion //fields and constructors

        #region internal
        internal void ChangeBuild(IEventHandler handler)
        {
            bool newPause;
            Buildable newBuild = handler.getNewBuild(this, 0, out newPause);
            ChangeBuild(newBuild, newPause);
        }
        private void ChangeBuild(Buildable newBuild)
        {
            if (this.CurBuild != newBuild)
                ChangeBuild(newBuild, false);
        }
        private void ChangeBuild(Buildable newBuild, bool pause)
        {
            AssertException.Assert(CanBuild(newBuild));
            this.CurBuild = newBuild;
            this.PauseBuild = pause;
        }

        internal void NewShipDesign(ShipDesign newDesign, HashSet<ShipDesign> obsoleteDesigns)
        {
            var obsoleteBuilds = this.buildable.OfType<BuildShip>().Where(buildShip => obsoleteDesigns.Contains(buildShip.ShipDesign));
            foreach (BuildShip buildShip in obsoleteBuilds)
                this.buildable.Remove(buildShip);
            BuildShip newBuild = new BuildShip(this, newDesign, obsoleteBuilds.Sum(buildShip => buildShip.Production));
            this.buildable.Add(newBuild);

            if (obsoleteBuilds.Contains(this.CurBuild))
                ChangeBuild(newBuild);
        }
        internal Tuple<ShipDesign, int, int, double, Buildable, Buildable, bool> MarkObsolete(IEventHandler handler, ShipDesign obsoleteDesign)
        {
            BuildShip obsoleteBuild = getBuildShip(obsoleteDesign);
            this.buildable.Remove(obsoleteBuild);

            int oldProduction = obsoleteBuild.Production;
            bool newPause, oldPause = this.PauseBuild;
            Buildable oldBuild = this.CurBuild;
            Buildable newBuild = handler.getNewBuild(this, oldProduction, out newPause);
            ChangeBuild(newBuild, newPause);

            //double prod = oldProduction, gold = 0;
            //if (newBuild is BuildShip)
            //    prod *= Consts.ManualObsoleteRatio;
            //else
            //    prod *= Consts.SwitchBuildRatio;

            double goldAdded;
            int prodAdded;
            AddProduction(oldProduction, true, false, out goldAdded, out prodAdded);

            return new Tuple<ShipDesign, int, int, double, Buildable, Buildable, bool>(obsoleteDesign, oldProduction, prodAdded, goldAdded, newBuild, oldBuild, oldPause);
        }
        internal void UndoMarkObsolete(Tuple<ShipDesign, int, int, double, Buildable, Buildable, bool> undoArgs)
        {
            ShipDesign obsoleteDesign = undoArgs.Item1;
            int oldProduction = undoArgs.Item2;
            int prodAdded = undoArgs.Item3;
            double goldAdded = undoArgs.Item4;
            Buildable newBuild = undoArgs.Item5;
            Buildable oldBuild = undoArgs.Item6;
            bool oldPause = undoArgs.Item7;

            this.buildable.Add(new BuildShip(this, obsoleteDesign, oldProduction));
            newBuild.AddProduction(-prodAdded);
            this.Player.AddGold(-goldAdded);

            if (oldBuild is BuildShip && ( (BuildShip)oldBuild ).ShipDesign == obsoleteDesign)
                oldBuild = getBuildShip(obsoleteDesign);
            ChangeBuild(oldBuild, oldPause);
        }
        private BuildShip getBuildShip(ShipDesign shipDesign)
        {
            return this.buildable.OfType<BuildShip>().Where(buildShip => buildShip.ShipDesign == shipDesign).Single();
        }

        private void DoChange(double soldierChange, int defenseAttChange, int defenseDefChange, int defenseHPChange)
        {
            this.SoldierChange = this.GetSoldierPct() - soldierChange;

            int att = this.Att;
            int def = this.Def;
            if (this.MinDefenses)
            {
                --att;
                --def;
            }

            this.DefenseAttChange = att - defenseAttChange;
            this.DefenseDefChange = def - defenseDefChange;
            this.DefenseHPChange = this.HP - defenseHPChange;
        }

        internal void ClearChange()
        {
            DoChange(0, 0, 0, 0);
        }
        internal void StartTurn(IEventHandler handler)
        {
            bool blocked = true;
            foreach (Tile neighbor in Tile.GetNeighbors(this.Tile))
                if (!( neighbor.SpaceObject is Anomaly ))
                {
                    blocked = false;
                    break;
                }
            if (blocked)
                Game.Random.SelectValue(Tile.GetNeighbors(this.Tile)).SpaceObject = null;

            if (this.built)
            {
                ChangeBuild(handler);
                this.built = false;
            }
        }

        internal void EndTurn(IEventHandler handler, ref int research)
        {
            double pdChange = this.PDCostAvgResearch;

            ResetMoved();

            //modify real values
            double population = 0, production = 0, gold = 0;
            TurnStuff(ref population, ref production, ref gold, ref research, true, false);

            this.ProdGuess += GetTotalIncome() / 3.0;

            this.Population += RoundValue(population, true, false, ref gold, Consts.PopulationForGoldHigh);

            ResetRounding();

            //build planet defenses first so they can attack this turn
            bool buildFirst = ( this.CurBuild is PlanetDefense ), built = false;
            if (buildFirst)
                built = this.CurBuild.Build(handler, production);

            if (!this.MinDefenses)
                foreach (Tile tile in Game.Random.Iterate<Tile>(Tile.GetNeighbors(this.Tile)))
                {
                    Ship ship = tile.SpaceObject as Ship;
                    if (ship != null && ship.Player != this.Player && handler.ConfirmCombat(this, ship))
                    {
                        AttackShip(ship, handler);
                        if (this.MinDefenses)
                            break;
                    }
                }

            //build ships after attacking so cleared tiles can be built on
            if (!buildFirst)
                built = this.CurBuild.Build(handler, production);

            if (!( this.CurBuild is PlanetDefense ))
                UpgradePlanetDefense();

            Player.AddGold(gold);

            DoChange(this.SoldierChange, this.DefenseAttChange, this.DefenseDefChange, this.DefenseHPChange);

            if (built || this.ProdGuess < 0)
                this.ProdGuess = 0;
        }

        private void TurnStuff(ref double population, ref double production, ref double gold, ref int research, bool doTurn, bool minGold)
        {
            //pay upkeep for stored production before adding production income
            gold -= Upkeep;

            double goldInc;
            int prodInt, researchInc;
            GetTurnValues(out prodInt, out goldInc, out researchInc);

            double prodInc = prodInt;
            Ship repairShip = RepairShip;
            if (repairShip != null)
                repairShip.ProductionRepair(ref prodInc, ref gold, doTurn, minGold);

            //modify parameter values
            population += GetPopulationGrowth();
            production += prodInc;
            gold += goldInc;
            research += researchInc;
        }

        public void GetTurnValues(out int production, out double gold, out int research)
        {
            TurnException.CheckTurn(this.Player);

            GetTurnValues(this.Population, out production, out gold, out research);
        }

        private void GetTurnValues(int population, out int production, out double gold, out int research)
        {
            double income = GetTotalIncome(population);

            double researchPct = GetPct(this.Player.ResearchEmphasis);
            double productionPct = GetPct(this.Player.ProductionEmphasis);
            double totalPct = GetPct(this.Player.GoldEmphasis) + researchPct + productionPct;
            researchPct /= totalPct;
            productionPct /= totalPct;

            research = MTRandom.Round(researchPct * income, this.researchRounding);
            production = MTRandom.Round(productionPct * income, this.productionRounding);
            gold = income - research - production;
        }

        private double GetPct(bool emphasis)
        {
            double retVal = 1;
            if (emphasis)
            {
                retVal = Consts.EmphasisValue;
                if (this.Player.NegativeGold())
                {
                    double mult = Math.Pow(-( this.Player.Gold + this.Player.TotalGold ) * Consts.AverageQuality * Consts.Income / this.Player.GetTotalIncome() / 91.0 + 1.69, 0.78);
                    if (mult > 1)
                        retVal *= mult;
                    else
                        ;
                }
            }
            return retVal;
        }

        private void ResetRounding()
        {
            this.researchRounding = Game.Random.NextFloat();
            this.productionRounding = Game.Random.NextFloat();
        }

        internal void Invasion(IEventHandler handler, Ship ship, ref int attackers, ref double attSoldiers, int gold)
        {
            handler.OnInvade(ship, this, attackers, attSoldiers, gold, double.NaN, double.NaN);

            Player attackPlayer = ship.Player;
            int initAttackers = attackers;
            int initPop = this.Population;

            double attack = 1, defense = 0;
            if (this.Population > 0)
                TroopBattle(ref attackers, attSoldiers, gold, out attack, out defense);

            int reduceQuality;
            double goldSpent;
            CheckPlanet(ref attackers, gold, attackPlayer, initAttackers, initPop, out reduceQuality, out goldSpent);

            double mult = attackers / (double)initAttackers;
            attSoldiers *= mult;
            mult = 0;
            if (initPop > 0)
                mult = this.Population / (double)initPop;
            this.Soldiers *= mult;
            ReduceDefenses(mult);
            double valueExp = ( initAttackers - attackers ) + ( initPop - this.Population ) + reduceQuality;

            handler.OnInvade(ship, this, attackers, attSoldiers, goldSpent, attack, defense);

            Planet.ReduceQuality(reduceQuality);
            if (attackers > 0 && this.Population > 0)
                throw new Exception();

            if (Planet.Dead)
                valueExp += Consts.PlanetConstValue;
            valueExp *= Consts.TroopExperienceMult;
            this.AddExperience(0, valueExp, initPop);
            double shipValueExp;
            attSoldiers += GetExperienceSoldiers(attackers, attSoldiers, initAttackers, valueExp, out shipValueExp);
            ship.AddExperience(0, shipValueExp, ship.Population);

            handler.OnInvade(ship, this, attackers, attSoldiers, goldSpent, attack, defense);

            //in the event of a tie, the defender keeps the planet with the remaining population of 0
            if (attackers > 0 && !Planet.Dead)
            {
                Destroy();
                OccupyPlanet(handler, Planet, attackPlayer, ref attackers, ref attSoldiers, initPop);
            }
        }

        private static double GetExperienceSoldiers(Player player, int curPop, double curSoldiers, int initPop, double valueExp)
        {
            double other;
            double soldiers = GetExperienceSoldiers(curPop, curSoldiers, initPop, valueExp, out other);
            player.GoldIncome(other / Consts.ExpForGold);
            return soldiers;
        }
        internal static double GetExperienceSoldiers(int curPop, double curSoldiers, int initPop, double valueExp, out double other)
        {
            double mult = 0;
            if (curPop > 0)
            {
                double pop = Math.Sqrt(( initPop + 13.0 ) / ( curPop + 13.0 )) * curPop;
                mult = GetSoldierMult(pop, curSoldiers, GetSoldiersForExp(valueExp)) * valueExp;
            }

            other = valueExp - mult;
            return Consts.GetExperience(GetSoldiersForExp(mult));
        }
        private static double GetSoldiersForExp(double valueExp)
        {
            return valueExp / Consts.ExpForSoldiers;
        }
        private static double GetSoldierMult(double pop, double soldiers, double addSoldiers)
        {
            return 1.3 / ( 1.3 + ( soldiers / 1.69 + addSoldiers ) / pop );
        }

        private void TroopBattle(ref int attackers, double attSoldiers, int gold, out double attack, out double defense)
        {
            defense = Consts.GetInvadeDefenseStrength(this.Population, this.Soldiers);
            attack = Consts.GetInvadeStrength(attackers, attSoldiers, gold, this.Population * defense);

            double attMult = attack / defense;
            double attStr = attMult * attackers;
            if (attStr > this.Population)
            {
                attackers = Game.Random.Round(( attStr - this.Population ) / attMult);
                this.Population = 0;
            }
            else
            {
                attackers = 0;
                this.Population = Game.Random.Round(this.Population - attStr);
            }
        }

        private void CheckPlanet(ref int attackers, int gold, Player attackPlayer, int initAttackers, int initPop, out int reduceQuality, out double goldSpent)
        {
            reduceQuality = Consts.GetPlanetDamage(( initAttackers - attackers ) + ( initPop - this.Population ));
            goldSpent = gold;

            int killPlanet = Planet.Quality + 1;
            if (reduceQuality > killPlanet)
            {
                double pct = 1 - ( killPlanet / (double)reduceQuality );
                reduceQuality = killPlanet;

                //only pay for the portion of gold spent until the planet is destroyed
                double rounded;
                attackPlayer.AddGold(gold * pct, out rounded);
                goldSpent -= rounded;

                attackers += Game.Random.Round(( initAttackers - attackers ) * pct);
                this.Population += Game.Random.Round(( initPop - this.Population ) * pct);
            }
        }

        private static void OccupyPlanet(IEventHandler handler, Planet planet, Player occupyingPlayer, ref int attackers, ref double soldiers, double initPop)
        {
            int occupy = attackers;
            if (initPop > 0 && attackers > 1)
            {
                occupy = handler.MoveTroops(null, occupy, occupy, soldiers, false);
                if (occupy < 1)
                    occupy = 1;
            }

            double moveSoldiers = GetMoveSoldiers(attackers, soldiers, occupy);

            attackers -= occupy;
            soldiers -= moveSoldiers;

            occupyingPlayer.NewColony(handler, planet, occupy, moveSoldiers, 0);
        }

        internal void Destroy()
        {
            if (this.Dead)
                throw new Exception();

            int newAtt, newDef;
            double gold = ( this.Population / Consts.PopulationForGoldLow )
                    + ( this.Soldiers / Consts.SoldiersForGold )
                    + ( this.production2 / Consts.ProductionForGold )
                    + ( this.GetActualDisbandValue(this.HP, out newAtt, out newDef) );
            this.Player.AddGold(gold, true);

            Console.WriteLine("Destroy Gold:  " + gold);
            Console.WriteLine("Population:  " + this.Population / Consts.PopulationForGoldLow);
            Console.WriteLine("Soldiers:  " + this.Soldiers / Consts.SoldiersForGold);
            Console.WriteLine("Production:  " + this.production2 / Consts.ProductionForGold);
            Console.WriteLine("Planet Defense:  " + this.GetActualDisbandValue(this.HP, out newAtt, out newDef));
            Console.WriteLine();

            if (( !this.MinDefenses ) ? ( newAtt != 1 || newDef != 1 ) : ( newAtt != Math.Max(this.Att - 1, 1) || newDef != Math.Max(this.Def - 1, 1) ))
                throw new Exception();

            this.Population = 0;
            this.Soldiers = 0;
            this.buildable.Clear();
            this.CurBuild = null;
            this.HP = 0;

            this.Player.RemoveColony(this);
            this.Player.DeathCheck();
        }

        public int GetAddProduction(double production)
        {
            TurnException.CheckTurn(this.Player);

            return GetAddProduction(production, false);
        }
        internal void AddProduction(double production)
        {
            double goldAdded;
            int prodAdded;
            AddProduction(production, false, true, out goldAdded, out prodAdded);
        }
        internal void AddProduction(double production, bool floor, bool random, out double goldAdded, out int prodAdded)
        {
            double add = this.CurBuild.GetAddProduction(production, false);
            goldAdded = ( production - add ) / Consts.ProductionForGold;
            prodAdded = RoundValue(add, random, floor, ref goldAdded, 1);
            this.CurBuild.AddProduction(prodAdded);
            this.Player.AddGold(goldAdded);
        }
        internal void UndoAddProduction(Buildable buildable, int undo)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(undo > buildable.Production || undo < 0);
            AssertException.Assert(this.CanBuild(buildable));

            buildable.AddProduction(-undo);
            ChangeBuild(buildable);
        }
        private int GetAddProduction(double production, bool floor)
        {
            return (int)Math.Round(this.CurBuild.GetAddProduction(production, floor) * Consts.FLOAT_ERROR_ONE);
        }

        #endregion //internal

        #region public

        public override int MaxPop
        {
            get
            {
                return int.MaxValue;
            }
        }

        public bool Dead
        {
            get
            {
                return ( this.Planet == null || this.Planet.Colony != this || this.Planet.Dead );
            }
        }

        public override Tile Tile
        {
            get
            {
                return Planet.Tile;
            }
            protected set
            {
                throw new Exception();
            }
        }

        public int Production2
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.production2;
            }
        }

        public double Upkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.PDUpkeep + Consts.GetProductionUpkeepMult(Player.Game.MapSize) * this.production2 + Consts.GetSoldierUpkeep(this);
            }
        }

        public bool MinDefenses
        {
            get
            {
                return ( this.HP == 0 );
            }
        }

        public double GetPopulationGrowth()
        {
            return Consts.GetPopulationGrowth(this.Population, Planet.Quality);
        }

        public double GetTotalIncome()
        {
            return GetTotalIncome(this.Population);
        }

        private double GetTotalIncome(int population)
        {
            return population * Consts.Income;
        }

        public double GetAfterRepairProdInc()
        {
            double production = GetProductionIncome();
            Ship repairShip = RepairShip;
            if (repairShip != null)
            {
                double gold = 0;
                repairShip.ProductionRepair(ref production, ref gold, false, false);
            }
            return production;
        }

        public int GetProductionIncome()
        {
            return GetProductionIncome(this.Population);
        }

        public int GetProductionIncome(int population)
        {
            TurnException.CheckTurn(this.Player);

            double gold;
            int production, research;
            GetTurnValues(population, out production, out gold, out research);
            return production;
        }

        public void GetTurnIncome(ref double population, ref double production, ref double gold, ref int research, bool minGold)
        {
            TurnException.CheckTurn(this.Player);

            double popInc = 0, prodInc = 0;
            TurnStuff(ref popInc, ref prodInc, ref gold, ref research, false, minGold);

            CurBuild.GetTurnIncome(ref prodInc, ref gold, minGold);

            //modify parameter values
            population += popInc;
            production += prodInc;
        }

        private int RoundValue(double value, bool random, bool floor, ref double gold, double rate)
        {
            if (floor && random)
                throw new Exception();

            int rounded;
            if (random)
                rounded = Game.Random.Round(value);
            else if (floor)
                rounded = (int)Math.Floor(value * Consts.FLOAT_ERROR_ONE);
            else
                rounded = (int)Math.Round(value * Consts.FLOAT_ERROR_ONE);

            gold += ( value - rounded ) / rate;
            return rounded;
        }

        //public void SellProduction(IEventHandler handler, int production)
        //{
        //    handler = new HandlerWrapper(handler, Player.Game, false, true);
        //    TurnException.CheckTurn(this.Player);
        //    AssertException.Assert(production > 0);
        //    AssertException.Assert(production <= this.production2);

        //    Player.Game.PushUndoCommand(new Game.UndoCommand<int>(
        //            new Game.UndoMethod<int>(UndoSellProduction), production));

        //    TradeProduction(-production, 1 / Consts.ProductionForGold);
        //}
        //private Tile UndoSellProduction(int production)
        //{
        //    TurnException.CheckTurn(this.Player);
        //    AssertException.Assert(production > 0);

        //    TradeProduction(production, 1 / Consts.ProductionForGold);

        //    return this.Tile;
        //}
        //public void BuyProduction(IEventHandler handler, int production)
        //{
        //    handler = new HandlerWrapper(handler, Player.Game, false, true);
        //    TurnException.CheckTurn(this.Player);
        //    AssertException.Assert(production > 0);
        //    AssertException.Assert(production * Consts.GoldForProduction < this.Player.Gold);

        //    Player.Game.PushUndoCommand(new Game.UndoCommand<int>(
        //            new Game.UndoMethod<int>(UndoBuyProduction), production));

        //    TradeProduction(production, Consts.GoldForProduction);
        //}

        //private Tile UndoBuyProduction(int production)
        //{
        //    TurnException.CheckTurn(this.Player);
        //    AssertException.Assert(production > 0);
        //    AssertException.Assert(production <= this.production2);

        //    TradeProduction(-production, Consts.GoldForProduction);

        //    return this.Tile;
        //}
        //private void TradeProduction(int production, double rate)
        //{
        //    this.production2 += production;
        //    this.Player.SpendGold(production * rate);
        //}

        public bool CanBuild(Buildable buildable)
        {
            return ( buildable != null && this.buildable.Contains(buildable) );
        }

        public void StartBuilding(IEventHandler handler, Buildable newBuild, bool pause)
        {
            handler = new HandlerWrapper(handler, Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(CanBuild(newBuild));

            ChangeBuild(newBuild, pause);
        }

        public double GetLossPct(Buildable oldBuild, Buildable newBuild, int production)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(CanBuild(oldBuild));
            AssertException.Assert(CanBuild(newBuild));

            double pct = production;
            if (oldBuild != newBuild)
                pct *= Consts.SwitchBuildRatio;
            pct = newBuild.GetAddProduction(pct, true);
            return ( production - pct ) / production;
        }

        public override string ToString()
        {
            return "Planetary Defenses";
        }

        #endregion //public

        #region planet defense

        public double PDUpkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return GetPDHPUpkeep() * this.HP;
            }
        }
        public double PDCostAvgResearch
        {
            get
            {
                return GetPDHPCostAvgResearch() * this.HP;
            }
        }
        public double PDCost
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return GetPDHPCost() * this.HP;
            }
        }
        public double PDStrength
        {
            get
            {
                return GetPDHPStrength() * this.HP;
            }
        }

        private double GetPDHPCostAvgResearch()
        {
            return GetPDHPCostAvgResearch(this.Att, this.Def);
        }
        private double GetPDHPCostAvgResearch(double att, double def)
        {
            return ShipDesign.GetPlanetDefenseCost(att, def, Player.Game.AvgResearch);
        }
        private double GetPDHPCost()
        {
            return GetPDHPCost(this.Att, this.Def);
        }
        private double GetPDHPCost(double att, double def)
        {
            return ShipDesign.GetPlanetDefenseCost(att, def, this.defenseResearch);
        }
        private double GetPDHPStrength()
        {
            return GetPDHPStrength(this.Att, this.Def);
        }
        private double GetPDHPStrength(double att, double def)
        {
            return ShipDesign.GetPlanetDefenseStrength(att, def);
        }

        public void DisbandPlanetDefense(IEventHandler handler, int hp, bool gold)
        {
            handler = new HandlerWrapper(handler, Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp >= 0);
            AssertException.Assert(hp <= this.HP);
            AssertException.Assert(gold || this.CurBuild != null);

            int newAtt, newDef;

            int production;
            double addGold, goldIncome;
            Buildable buildable = null;
            if (gold)
            {
                GetDisbandGoldValue(hp, out goldIncome, out addGold, out newAtt, out newDef);
                this.Player.AddGold(goldIncome, addGold);

                production = 0;
                goldIncome -= addGold;
            }
            else
            {
                AddProduction(GetActualDisbandValue(hp, out newAtt, out newDef), true, false, out goldIncome, out production);
                buildable = this.CurBuild;

                addGold = 0;
            }

            newAtt = this.Att - newAtt;
            newDef = this.Def - newDef;

            this.HP -= hp;
            this.Att -= newAtt;
            this.Def -= newDef;

            Player.Game.PushUndoCommand(new Game.UndoCommand<int, int, int, Buildable, int, double, double>(
                    new Game.UndoMethod<int, int, int, Buildable, int, double, double>(UndoDisbandPlanetDefense), newAtt, newDef, hp, buildable, production, addGold, goldIncome));
        }
        private Tile UndoDisbandPlanetDefense(int att, int def, int hp, Buildable buildable, int production, double addGold, double goldIncome)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp >= 0);
            AssertException.Assert(att >= 0);
            AssertException.Assert(def >= 0);
            AssertException.Assert(production >= 0);
            AssertException.Assert(production <= buildable.Production);
            AssertException.Assert(addGold >= 0);
            AssertException.Assert(goldIncome > -.1);
            AssertException.Assert(production == 0 || this.CanBuild(buildable));

            this.HP += hp;
            this.Att += att;
            this.Def += def;
            if (this.CanBuild(buildable) && production > 0)
                this.UndoAddProduction(buildable, production);
            this.Player.AddGold(-addGold);
            this.Player.GoldIncome(-goldIncome);

            return this.Tile;
        }

        public double GetPlanetDefenseDisbandValue(int hp, bool gold, out int newAtt, out int newDef)
        {
            TurnException.CheckTurn(this.Player);

            if (gold)
            {
                double actual, rounded;
                GetDisbandGoldValue(hp, out actual, out rounded, out newAtt, out newDef);
                return rounded;
            }
            else
            {
                return GetAddProduction(GetActualDisbandValue(hp, out newAtt, out newDef), true);
            }
        }
        private double GetActualDisbandValue(int hp, out int newAtt, out int newDef)
        {
            double oldCost = this.PDCost;

            newAtt = this.Att;
            newDef = this.Def;

            int newHP = this.HP - hp;
            double hpMult = this.HP / ShipDesign.GetHPStr(this.Att, this.Def);

            double mult = 1, step = 1 / ( this.Att * this.Def * Consts.FLOAT_ERROR_ONE );
            do
            {
                mult -= step;
                newAtt = Math.Max(1, (int)Math.Floor(Att * mult));
                newDef = Math.Max(1, (int)Math.Floor(Def * mult));
            } while (mult > 0 && ShipDesign.GetHPStr(newAtt, newDef) * hpMult > newHP);

            return ( oldCost - GetPDHPCost(newAtt, newDef) * newHP ) * Consts.DisbandPct;
        }
        private void GetDisbandGoldValue(int hp, out double actual, out double rounded, out int newAtt, out int newDef)
        {
            actual = GetActualDisbandValue(hp, out newAtt, out newDef);
            rounded = Player.FloorGold(actual);
        }

        private double GetAttackCost(int shipDef)
        {
            return GetAttackCost(shipDef, this.Att, this.Def, this.HP);
        }
        private double GetAttackCost(int shipDef, int att, int def, int hp)
        {
            //only pay for Attack and the maximum HP you could possibly use
            return GetPDHPUpkeep(att, 1) * Math.Min(hp, ( att - 1 ) * shipDef + 1) * Consts.PlanetDefensesAttackCostMult;
        }
        private double GetPDHPUpkeep()
        {
            return GetPDHPUpkeep(this.Att, this.Def);
        }
        private double GetPDHPUpkeep(double att, double def)
        {
            return ShipDesign.GetPlanetDefenseCost(att, def, this.Player.Research) * Consts.PlanetDefensesUpkeepMult * Consts.GetProductionUpkeepMult(Player.Game.MapSize);
        }

        private void AttackShip(Ship ship, IEventHandler handler)
        {
            //get the attack cost before possibly being injured
            double cost = GetAttackCost(ship.Def);
            double pct = Combat(handler, ship);
            this.Player.SpendGold(cost * ( 1 - pct ));
        }

        internal override double GetExpForDamage(double damage)
        {
            return damage * GetPDHPStrength() * Consts.ExperienceMult;
        }
        protected override double GetKillExp()
        {
            return GetExpForDamage(13 / GetPDHPCostAvgResearch());
        }

        internal override void AddExperience(double rawExp, double valueExp, int initPop)
        {
            valueExp += rawExp * GetPDHPCostAvgResearch() / GetPDHPStrength();
            this.Soldiers += GetExperienceSoldiers(this.Player, this.Population, this.Soldiers, initPop, valueExp);
        }

        internal void BuildPlanetDefense(double prodInc, bool always)
        {
            if (prodInc > Consts.FLOAT_ERROR_ZERO)
            {
                double newAtt, newDef, newHP, newResearch, production;
                GetPlanetDefenseInc(prodInc, this.Player.Research, out newAtt, out newDef, out newHP, out newResearch, out production, always, true);
                SetPD(newAtt, newDef, newHP, newResearch, production);
            }
        }
        internal void UpgradePlanetDefense()
        {
            if (this.Player.Research > this.defenseResearch && !this.MinDefenses)
            {
                double maxResearch = this.defenseResearch + Game.Random.Weighted(this.Player.Research - this.defenseResearch, Consts.PlanetDefensesUpgradeWeight);
                if (maxResearch > this.defenseResearch * Consts.FLOAT_ERROR_ONE)
                {
                    double newAtt, newDef, newHP, newResearch, production;
                    GetPlanetDefenseInc(0, maxResearch, out newAtt, out newDef, out newHP, out newResearch, out production, false, true);
                    SetPD(newAtt, newDef, newHP, newResearch, production);
                }
                else
                    ;
            }
            else
                ;
        }
        private void SetPD(double newAtt, double newDef, double newHP, double newResearch, double production)
        {
            this.defenseResearch = newResearch;
            double newCost = GetPDHPCost(newAtt, newDef) * newHP;
            SetPD(newCost, newAtt, newDef);

            if (this.CurBuild is PlanetDefense && production > Consts.FLOAT_ERROR_ZERO)
                this.AddProduction(production);
            else
                this.Player.GoldIncome(production);
        }

        internal void BuildSoldiersAndDefenses(double prodInc)
        {
            prodInc /= 2.0;
            BuildPlanetDefense(prodInc, true);
            BuildSoldiers(prodInc);
        }
        internal void BuildSoldiers(double prod)
        {
            double soldiers = Consts.GetExperience(prod / Consts.ProductionForSoldiers);
            if (this.Population == 0)
                this.Player.AddGold(soldiers / Consts.SoldiersForGold);
            else
                this.Soldiers += soldiers;
        }

        public void GetPlanetDefenseInc(double prodInc, double maxResearch, out double newAtt, out double newDef, out double newHP, out double newResearch, out double newProd, bool always, bool random)
        {
            GetPlanetDefenseInc(this.CurBuild, prodInc, maxResearch, out newAtt, out newDef, out newHP, out newResearch, out newProd, always, random);
        }
        public void GetPlanetDefenseInc(Buildable buildable, double prodInc, double maxResearch, out double newAtt, out double newDef, out double newHP, out double newResearch, out double newProd, bool always, bool random)
        {
            TurnException.CheckTurn(this.Player);

            always |= ( buildable is PlanetDefense && this.MinDefenses );

            double maxHPTot = ( this.PDCost + prodInc ) / ShipDesign.GetPlanetDefenseCost(this.Att, this.Def, maxResearch);
            double mult = this.GetTotalIncome() / 3.0;
            double add = ( maxHPTot - this.HP ) * GetPDHPCostAvgResearch() / mult;
            if (add > 1)
            {
                add = Math.Pow(add, .78);
                if (random)
                    add = Game.Random.GaussianCappedInt(add, Consts.PlanetDefenseRndm, 1);
            }
            else
            {
                if (always)
                    add = 1;
                else
                    add = 0;
            }
            add *= mult;

            if (add > Consts.FLOAT_ERROR_ZERO)
            {
                bool? stat = null;
                if (buildable is BuildAttack)
                    stat = true;
                else if (buildable is BuildDefense)
                    stat = false;
                else if (random)
                    stat = Game.Random.Bool();
                else
                    ;
                double trgAtt = this.Player.PDAtt, trgDef = this.Player.PDDef;
                if (stat.HasValue)
                    if (stat.Value)
                        trgDef = AdjustStatRatio(trgDef, random);
                    else
                        trgAtt = AdjustStatRatio(trgAtt, random);
                else
                    ;

                ModPD(this.PDCostAvgResearch + add, Player.Game.AvgResearch, this.Att, trgAtt, this.Def, trgDef, out newAtt, out newDef, out newHP);

                double att = newAtt, def = newDef, hp = newHP;
                newResearch = TBSUtil.FindValue(research => ShipDesign.GetPlanetDefenseCost(att, def, research) * hp,
                        this.PDCost + prodInc, Math.Max(Player.Game.PDResearch, maxResearch), Math.Min(Player.Game.PDResearch, maxResearch));
                if (newResearch / Player.Game.PDResearch < Consts.FLOAT_ERROR_ONE || maxResearch / newResearch < Consts.FLOAT_ERROR_ONE)
                    ;
                newProd = this.PDCost + prodInc - ShipDesign.GetPlanetDefenseCost(att, def, newResearch) * hp;
            }
            else
            {
                newAtt = this.Att;
                newDef = this.Def;
                newHP = this.HP;
                newResearch = this.defenseResearch;
                newProd = prodInc;
            }
        }
        private double AdjustStatRatio(double stat, bool random)
        {
            stat *= Consts.PlanetDefenseStatRatio;
            if (random)
                stat = Game.Random.GaussianCapped(stat, Consts.PlanetDefenseRndm, 1);
            return stat;
        }

        private void ReduceDefenses(double mult)
        {
            double totalCost = this.PDCost;

            if (!this.MinDefenses && ( mult > Consts.FLOAT_ERROR_ZERO && mult < 1 - Consts.FLOAT_ERROR_ZERO ))
            {
                double newAtt, newDef, newHP;
                ModPD(totalCost * mult, this.defenseResearch, this.Att, 1, this.Def, 1, out newAtt, out newDef, out newHP);
                SetPD(totalCost * mult, newAtt, newDef);
            }
            if (this.MinDefenses)
            {
                this.Att = 1;
                this.Def = 1;
                this.HP = 0;
            }

            this.Player.AddGold(( totalCost - this.PDCost ) * Consts.DisbandPct);
        }

        private static void ModPD(double trgCost, double trgResearch, int att, double trgAtt, int def, double trgDef,
                out double newAtt, out double newDef, out double newHP)
        {
            bool inc = ( trgAtt > att || trgDef > def );
            if (inc)
            {
                trgAtt = Math.Max(trgAtt, att);
                trgDef = Math.Max(trgDef, def);
            }

            if (trgAtt == att && trgDef == def)
            {
                newAtt = att;
                newDef = def;
            }
            else
            {
                double minAtt = Math.Min(trgAtt, att), maxAtt = Math.Max(trgAtt, att), minDef = Math.Min(trgDef, def), maxDef = Math.Max(trgDef, def);
                if (TestPD(trgCost, trgResearch, maxAtt, maxDef))
                {
                    newAtt = maxAtt;
                    newDef = maxDef;
                }
                else if (!TestPD(trgCost, trgResearch, minAtt, minDef))
                {
                    newAtt = minAtt;
                    newDef = minDef;
                }
                else
                {
                    double min = 0, max = 1, mult = .5;
                    do
                    {
                        newAtt = att + mult * ( trgAtt - att );
                        newDef = def + mult * ( trgDef - def );
                        if (inc == TestPD(trgCost, trgResearch, newAtt, newDef))
                            min = mult;
                        else
                            max = mult;
                        mult = ( min + max ) / 2.0;
                    } while (max - min > Consts.FLOAT_ERROR_ZERO);
                }
            }

            newHP = GetPDHP(trgCost, trgResearch, newAtt, newDef);

            //if (att < trgAtt && def > 1 && att == newAtt)
            //    ModPD(trgCost, att, att, def, 1, out newAtt, out newDef, out newHP);
            //else if (def < trgDef && att > 1 && def == newDef)
            //    ModPD(trgCost, att, 1, def, def, out newAtt, out newDef, out newHP);
        }
        private static bool TestPD(double trgCost, double trgResearch, double minAtt, double minDef)
        {
            return ( GetPDHP(trgCost, trgResearch, minAtt, minDef) > ShipDesign.GetHPStr(minAtt, minDef) );
        }
        private static double GetPDHP(double trgCost, double trgResearch, double s1, double s2)
        {
            return trgCost / ShipDesign.GetPlanetDefenseCost(s1, s2, trgResearch);
        }

        private void SetPD(double newCost, double newAtt, double newDef)
        {
            this.Att = SetPDStat(newAtt, this.Att, this.Player.PDAtt);
            this.Def = SetPDStat(newDef, this.Def, this.Player.PDDef);
            this.HP = SetPDStat(newCost / GetPDHPCost(), this.HP, ushort.MaxValue);
        }
        private static int SetPDStat(double target, int current, int max)
        {
            if (target == current)
                return current;

            double add = target - current;

            int min = 1;
            if (target > current)
                min = current;
            else
                max = current;
            int lowerCap = Math.Max(min - current, (int)Math.Ceiling(2.0 * add - max + current));

            if (add > lowerCap)
                return current + Game.Random.GaussianCappedInt(add, Consts.PlanetDefenseRndm, lowerCap);
            else
                return Game.Random.Round(target);
        }

        #endregion //planet defense
    }
}
