using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class Colony : Combatant
    {
        #region fields and constructors

        private readonly Player player;
        public readonly Planet Planet;

        private Buildable _buildable;
        private Ship _repairShip;

        private bool _built;
        private sbyte _defenseAttChange, _defenseDefChange;
        private short _defenseHPChange;
        private ushort _production, _repair;
        private float _researchRounding, _productionRounding, _soldierChange;

        internal Colony(IEventHandler handler, Player player, Planet planet, int population, double soldiers, int production)
            : base(1, 1, 0, population, soldiers)
        {
            this.player = player;
            this.Planet = planet;
            planet.Colony = this;

            //set the build intially to StoreProd so it can be changed to anything with no production loss
            this._buildable = player.Game.StoreProd;
            this._repairShip = null;

            this.production = production;

            ResetRounding();

            if (handler == null)
                this.built = true;
            else
                StartBuilding(handler, handler.getNewBuild(this, true, false));
        }

        private bool built
        {
            get
            {
                return this._built;
            }
            set
            {
                this._built = value;
            }
        }

        #endregion //fields and constructors

        #region internal

        internal void SetBuildable(Buildable newDesign, double losspct)
        {
            LoseProductionPct(losspct);
            this._buildable = newDesign;
        }

        internal void DoBuild(IEventHandler handler, double productionInc, ref double gold)
        {
            if (this.Buildable != null && this.Buildable.HandlesFraction)
                this.Buildable.SetFraction(productionInc);
            else
                this.production += RoundValue(productionInc, ref gold);

            //actual building of new ships happens at turn end
            if (this.Buildable != null)
            {
                while (this.production >= this.Buildable.Cost)
                {
                    this.built = ( this.Buildable.Cost > 0 );

                    Tile tile = null;
                    if (this.Buildable.NeedsTile)
                    {
                        foreach (Tile neighbor in Tile.GetNeighbors(this.Tile))
                            if (neighbor.SpaceObject == null)
                            {
                                //only ask for a tile if there is one available
                                tile = handler.getBuildTile(this);
                                break;
                            }
                        //null means to not actually produce the ship
                        if (tile == null)
                            break;
                        //invalid selection; just ask again
                        if (tile.SpaceObject != null || !Tile.IsNeighbor(this.Tile, tile))
                            continue;
                    }

                    //build the ship
                    this.Buildable.Build(handler, this, tile);
                    this.production -= this.Buildable.Cost;
                    LoseProductionPct(Consts.CarryProductionLossPct);

                    if (!this.Buildable.Multiple)
                        break;
                }

                if (this.Buildable.production != 0)
                    throw new Exception();
            }
        }

        private void DoChange(double soldierChange, int defenseAttChange, int defenseDefChange, int defenseHPChange)
        {
            this._soldierChange = (float)( this.GetSoldierPct() - soldierChange );
            int att = this.Att;
            int def = this.Def;
            if (this.MinDefenses)
            {
                --att;
                --def;
            }
            checked
            {
                this._defenseAttChange = (sbyte)( att - defenseAttChange );
                this._defenseDefChange = (sbyte)( def - defenseDefChange );
                this._defenseHPChange = (short)( this.HP - defenseHPChange );
            }
        }

        internal void StartTurn(IEventHandler handler)
        {
            DoChange(0, 0, 0, 0);

            if (this.built)
            {
                StartBuilding(handler, handler.getNewBuild(this, true, true));
                this.built = false;
            }
        }

        internal void EndTurn(IEventHandler handler, ref double gold, ref int research)
        {
            this.Repair = 0;
            ResetMoved();

            //modify real values
            double population = 0, production = 0;
            TurnStuff(ref population, ref production, ref gold, ref research, true, false);

            this.Population += RoundValue(population, ref gold, Consts.PopulationForGoldHigh);

            ResetRounding();

            //build planet defenses first so they can attack this turn
            bool buildFirst = ( this.Buildable is PlanetDefense );
            if (buildFirst)
                this.DoBuild(handler, production, ref gold);
            if (this.HP > 0)
                foreach (Tile tile in Game.Random.Iterate<Tile>(Tile.GetNeighbors(this.Tile)))
                {
                    Ship ship = tile.SpaceObject as Ship;
                    if (ship != null && ship.Player != this.Player && handler.ConfirmCombat(this, ship))
                    {
                        AttackShip(ship, handler);
                        if (this.HP == 0)
                            break;
                    }
                }
            //build ships after attacking so cleared tiles can be built on
            if (!buildFirst)
                this.DoBuild(handler, production, ref gold);

            DoChange(this._soldierChange, this._defenseAttChange, this._defenseDefChange, this._defenseHPChange);
        }

        private void TurnStuff(ref double population, ref double production, ref double gold, ref int research, bool doTurn, bool minGold)
        {
            //pay upkeep for stored production before adding production income
            gold -= Upkeep;

            double goldInc;
            int prodInt, researchInc;
            GetTurnValues(out prodInt, out goldInc, out researchInc);
            double productionInc = prodInt;

            Ship repairShip = RepairShip;
            if (repairShip != null)
                this.Repair += repairShip.ProductionRepair(ref productionInc, ref gold, doTurn, minGold);

            if (this.Buildable == null)
                LoseProduction(productionInc, ref productionInc, ref gold, Consts.GoldProductionForGold);
            else if (this.Buildable is StoreProd)
                LoseProduction(productionInc * Consts.StoreProdLossPct, ref productionInc, ref gold, Consts.ProductionForGold);

            //modify parameter values
            population += GetPopulationGrowth();
            production += productionInc;
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

            research = Round(researchPct * income, this._researchRounding);
            production = Round(productionPct * income, this._productionRounding);
            gold = income - research - production;
        }

        private double GetPct(bool emphasis)
        {
            return emphasis ? Consts.EmphasisValue : 1;
        }

        //analogous to MTRandom.Round, but using a constant for the random value
        private int Round(double number, float round)
        {
            int result = (int)Math.Floor(number);
            if (round < number - result)
                ++result;
            return result;
        }

        private void ResetRounding()
        {
            this._researchRounding = Game.Random.NextFloat();
            this._productionRounding = Game.Random.NextFloat();
        }

        internal double Bombard(int damage)
        {
            int initPop = this.Population;
            LosePopulation(damage);

            double exp = Math.Min(initPop, damage) * Consts.TroopExperienceMult;
            this.Soldiers += GetExperienceSoldiers(this.Player, this.Population, initPop, exp);
            return exp;
        }

        internal void Invasion(IEventHandler handler, Ship ship, ref int attackers, ref double attSoldiers, int gold, out double attExperience)
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
            mult = this.Population / (double)initPop;
            this.Soldiers *= mult;
            ReduceDefenses(mult);

            handler.OnInvade(ship, this, attackers, attSoldiers, goldSpent, attack, defense);

            Planet.ReduceQuality(reduceQuality);
            if (attackers > 0 && this.Population > 0)
                throw new Exception();

            double experience = ( initAttackers - attackers ) + ( initPop - this.Population ) + reduceQuality;
            if (Planet.Dead)
                experience += Planet.ConstValue;
            experience *= Consts.TroopExperienceMult;
            this.Soldiers += GetExperienceSoldiers(this.Player, this.Population, initPop, experience);
            attSoldiers += GetExperienceSoldiers(attackers, initAttackers, experience, out attExperience);

            handler.OnInvade(ship, this, attackers, attSoldiers, goldSpent, attack, defense);

            //in the event of a tie, the defender keeps the planet with the remaining population of 0
            if (attackers > 0 && !Planet.Dead)
            {
                Destroy();
                OccupyPlanet(handler, Planet, attackPlayer, ref attackers, ref attSoldiers, initPop);
            }
        }

        private static float GetExperienceSoldiers(Player player, int curPop, double initPop, double exp)
        {
            double other;
            float soldiers = GetExperienceSoldiers(curPop, initPop, exp, out other);
            player.GoldIncome(other);
            return soldiers;
        }
        private static float GetExperienceSoldiers(int curPop, double initPop, double exp, out double other)
        {
            double mult = ( initPop > 0 ? Math.Sqrt(curPop / initPop) : 0 ) * exp;
            other = ( exp - mult );
            mult /= Consts.ExpForSoldiers;
            return Consts.GetExperience(mult);
        }

        private void TroopBattle(ref int attackers, double attSoldiers, int gold, out double attack, out double defense)
        {
            defense = Consts.GetPlanetDefenseStrength(this.Population, this.Soldiers);
            attack = Consts.GetInvasionStrength(attackers, attSoldiers, gold, this.Population * defense);

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
                occupy = handler.MoveTroops(null, occupy, occupy, occupy, soldiers);
                if (occupy < 1)
                    occupy = 1;
            }

            double moveSoldiers = GetMoveSoldiers(attackers, soldiers, occupy);
            occupyingPlayer.NewColony(handler, planet, occupy, moveSoldiers, 0);

            attackers -= occupy;
            soldiers -= moveSoldiers;
        }

        internal void Destroy()
        {
            if (this.Dead)
                throw new Exception();

            double gold = ( this.Population / Consts.PopulationForGoldLow )
                    + ( this.Soldiers / Consts.SoldiersForGold )
                    + ( this.production / Consts.ProductionForGold )
                    + ( this.GetActualDisbandValue(this.HP) );
            this.Player.AddGold(gold, true);

            Console.WriteLine("Destroy Gold:  " + gold);
            Console.WriteLine("Population:  " + this.Population / Consts.PopulationForGoldLow);
            Console.WriteLine("Soldiers:  " + this.Soldiers / Consts.SoldiersForGold);
            Console.WriteLine("Production:  " + this.production / Consts.ProductionForGold);
            Console.WriteLine("Planet Defense:  " + this.GetActualDisbandValue(this.HP));
            Console.WriteLine();

            this.Population = 0;
            this.Soldiers = 0;
            this.production = 0;
            this.HP = 0;

            this.Player.RemoveColony(this);
            this.Player.DeathCheck();
        }

        public int GetAddProduction(double production)
        {
            return GetAddProduction(production, false);
        }

        internal void AddProduction(double production)
        {
            double goldAdded;
            int prodAdded;
            AddProduction(production, false, true, out goldAdded, out prodAdded);
        }
        internal void AddProduction(double production, bool floor, out double goldAdded, out int prodAdded)
        {
            AddProduction(production, floor, false, out goldAdded, out prodAdded);
        }
        internal void UndoAddProduction(int undo)
        {
            if (undo > this.production || undo < 0)
                throw new Exception();
            this.production -= undo;
        }

        private void AddProduction(double production, bool floor, bool random, out double goldAdded, out int prodAdded)
        {
            double loss = GetAddProductionLoss(production);
            goldAdded = 0;
            if (!random)
            {
                goldAdded = production - GetAddProduction(production, floor) - loss;
                production -= goldAdded;
            }

            LoseProduction(loss, ref production, ref goldAdded, Consts.ProductionForGold);

            prodAdded = RoundValue(production, ref goldAdded);
            this.production += prodAdded;

            Player.GoldIncome(goldAdded);
        }
        private double GetAddProductionLoss(double production)
        {
            if (production > 0 && this.Buildable is StoreProd)
                return production * Consts.StoreProdLossPct;
            return 0;
        }
        private int GetAddProduction(double production, bool floor)
        {
            production -= GetAddProductionLoss(production);
            if (floor)
                return (int)Math.Floor(production);
            return (int)Math.Round(production);
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
                return ( this.Planet.Colony != this );
            }
        }

        public override Player Player
        {
            get
            {
                return this.player;
            }
        }

        public override Tile Tile
        {
            get
            {
                return Planet.Tile;
            }
        }

        public Buildable Buildable
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this._buildable;
            }
        }

        public int Production
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.production;
            }
        }
        internal int production
        {
            get
            {
                return this._production;
            }
            private set
            {
                checked
                {
                    this._production = (ushort)value;
                }
            }
        }

        public double Upkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return PlanetDefenseUpkeep + Consts.GetProductionUpkeepMult(Player.Game.MapSize) * this.production;
            }
        }

        public Ship RepairShip
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                if (this._repairShip != null &&
                        ( this._repairShip.HP == this._repairShip.MaxHP || this._repairShip.Dead
                        || !Tile.IsNeighbor(this.Tile, this._repairShip.Tile) ))
                    this.RepairShip = null;

                return this._repairShip;
            }
            internal set
            {
                if (this._repairShip != value)
                {
                    if (this._repairShip != null && this._repairShip.AutoRepair == 0)
                        this._repairShip.AutoRepair = double.NaN;
                    if (value != null)
                        value.AutoRepair = 0;

                    this._repairShip = value;
                }
            }
        }

        public void SetRepairShip(IEventHandler handler, Ship value)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(value == null || this.Player == value.Player);

            this.RepairShip = value;
        }

        public int Repair
        {
            get
            {
                return this._repair;
            }
            private set
            {
                checked
                {
                    this._repair = (ushort)value;
                }
            }
        }

        public double SoldierChange
        {
            get
            {
                return this._soldierChange;
            }
        }

        public int DefenseAttChange
        {
            get
            {
                return this._defenseAttChange;
            }
        }

        public int DefenseDefChange
        {
            get
            {
                return this._defenseDefChange;
            }
        }

        public int DefenseHPChange
        {
            get
            {
                return this._defenseHPChange;
            }
        }

        public bool MinDefenses
        {
            get
            {
                return ( this.Att == 1 && this.Def == 1 && this.HP == 0 );
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

            if (minGold)
            {
                //pop/prod rounding
                MinGold(popInc, ref gold, Consts.PopulationForGoldHigh);
                if (this.Buildable != null && !this.Buildable.HandlesFraction)
                    MinGold(prodInc, ref gold, 1);

                //planet defense attack cost
                int att, def, hp;
                this.GetPlanetDefenseIncMinGold(this.production + prodInc, out att, out def, out hp);
                if (hp > 0)
                    foreach (Tile tile in Tile.GetNeighbors(this.Tile))
                    {
                        Ship ship = tile.SpaceObject as Ship;
                        if (ship != null && ship.Player != this.Player)
                            gold -= GetAttackCost(att, def, hp, ship.Def);
                    }
            }

            gold += BuildingShipTurnIncome(prodInc, minGold);

            //modify parameter values
            population += popInc;
            production += prodInc;
        }

        private double BuildingShipTurnIncome(double prodInc, bool minGold)
        {
            double gold = 0;
            ShipDesign shipDesign = this.Buildable as ShipDesign;
            if (this.Buildable != null && this.Buildable.Cost > 0)
            {
                double totalProd = this.production + prodInc;
                while (totalProd > this.Buildable.Cost)
                {
                    totalProd -= this.Buildable.Cost;
                    double loss = totalProd * Consts.CarryProductionLossPct;
                    if (minGold)
                        loss = Math.Floor(loss);
                    totalProd -= loss;
                    gold += loss / Consts.ProductionForGold;
                    if (minGold)
                        gold -= GetActualGoldCost(shipDesign.Trans);
                    if (!this.Buildable.Multiple)
                        break;
                }
            }
            if (minGold && gold > 0)
                gold = 0;
            return gold;
        }

        private void MinGold(double value, ref double gold, double rate)
        {
            gold += ( value - (int)Math.Ceiling(value) ) / rate;
        }

        private void GetPlanetDefenseIncMinGold(double production, out int newAtt, out int newDef, out int newHP)
        {
            if (this.Buildable is PlanetDefense)
            {
                double att, def, hp, soldiers;
                GetPlanetDefenseInc(this.Buildable, production, out att, out def, out hp, out soldiers);
                newAtt = (int)Math.Ceiling(att);
                newDef = (int)Math.Ceiling(def);
                newHP = (int)Math.Ceiling(hp);
            }
            else
            {
                newAtt = this.Att;
                newDef = this.Def;
                newHP = this.HP;
            }
        }

        private void LoseProduction(double loseProduction)
        {
            double production = this.production, gold = 0;
            LoseProduction(loseProduction, ref production, ref gold, Consts.ProductionForGold);

            this.production = RoundValue(production, ref gold, Consts.ProductionForGold);

            Player.AddGold(gold);
        }

        private int RoundValue(double value, ref double gold)
        {
            return RoundValue(value, ref gold, 1);
        }

        private int RoundValue(double value, ref double gold, double rate)
        {
            int rounded = Game.Random.Round(value);
            gold += ( value - rounded ) / rate;
            return rounded;
        }

        private void LoseProduction(double loseProduction, ref double production, ref double gold, double rate)
        {
            gold += loseProduction / rate;
            production -= loseProduction;
        }

        public void SellProduction(IEventHandler handler, int production)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false, true);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(production > 0);
            AssertException.Assert(production <= this.production);

            Player.Game.PushUndoCommand(new Game.UndoCommand<int>(
                    new Game.UndoMethod<int>(UndoSellProduction), production));

            TradeProduction(-production, 1 / Consts.ProductionForGold);
        }
        private Tile UndoSellProduction(int production)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(production > 0);
            AssertException.Assert(production * Consts.ProductionForGold < this.Player.Gold);

            TradeProduction(production, 1 / Consts.ProductionForGold);

            return this.Tile;
        }
        public void BuyProduction(IEventHandler handler, int production)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false, true);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(production > 0);
            AssertException.Assert(production / Consts.GoldForProduction < this.Player.Gold);

            Player.Game.PushUndoCommand(new Game.UndoCommand<int>(
                    new Game.UndoMethod<int>(UndoBuyProduction), production));

            TradeProduction(production, Consts.GoldForProduction);
        }
        private Tile UndoBuyProduction(int production)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(production > 0);
            AssertException.Assert(production <= this.production);

            TradeProduction(-production, Consts.GoldForProduction);

            return this.Tile;
        }
        private void TradeProduction(int production, double rate)
        {
            this.production += production;
            Player.SpendGold(production * rate);
        }

        public bool CanBuild(Buildable buildable)
        {
            return ( buildable == null || buildable.CanBeBuiltBy(this) );
        }

        public void StartBuilding(IEventHandler handler, Buildable newBuild)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(CanBuild(newBuild));

            SetBuildable(newBuild, GetLossPct(newBuild));
        }

        public double GetLossPct(Buildable newBuild)
        {
            Buildable oldBuild = this.Buildable;
            if (oldBuild == null || newBuild == null)
                return 1;
            if (oldBuild is StoreProd)
                return 0;
            if (oldBuild.GetType() != newBuild.GetType())
                return Consts.SwitchBuildTypeLossPct;
            if (oldBuild != newBuild)
                return Consts.SwitchBuildLossPct;
            return 0;
        }

        private void LoseProductionPct(double pct)
        {
            LoseProduction(this.production * pct);
        }

        public override string ToString()
        {
            return "Planetary Defenses";
        }

        #endregion //public

        #region planet defense

        public double PlanetDefenseUpkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return GetPDUpkeep(this.HP, this.Att, this.Def, Consts.PlanetDefensesUpkeepMult);
            }
        }
        internal double ArmadaCost
        {
            get
            {
                return HP * ShipDesign.GetPlanetDefenseCost(this.Att, this.Def, this.Player.Game.AvgResearch);
            }
        }
        internal double PlanetDefenseCostPerHP
        {
            get
            {
                return GetPDCost(this.Att, this.Def);
            }
        }
        private double PlanetDefenseCost
        {
            get
            {
                return this.HP * PlanetDefenseCostPerHP;
            }
        }
        public double PlanetDefenseStrengthPerHP
        {
            get
            {
                return ShipDesign.GetPlanetDefenseStrength(this.Att, this.Def);
            }
        }
        public double PlanetDefenseStrength
        {
            get
            {
                return this.HP * PlanetDefenseStrengthPerHP;
            }
        }

        private double GetPDCost(double att, double def)
        {
            return ShipDesign.GetPlanetDefenseCost(att, def, this.Player.LastResearched);
        }

        public void DisbandPlanetDefense(IEventHandler handler, int hp, bool gold)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(hp <= this.HP);

            int production;
            double addGold, goldIncome;
            if (gold)
            {
                GetDisbandGoldValue(hp, out goldIncome, out addGold);
                Player.AddGold(goldIncome, addGold);

                production = 0;
                goldIncome -= addGold;
            }
            else
            {
                AddProduction(GetActualDisbandValue(hp), true, out goldIncome, out production);

                addGold = 0;
            }

            this.HP -= hp;

            Player.Game.PushUndoCommand(new Game.UndoCommand<int, int, double, double>(
                    new Game.UndoMethod<int, int, double, double>(UndoDisbandPlanetDefense), hp, production, addGold, goldIncome));
        }
        private Tile UndoDisbandPlanetDefense(int hp, int production, double addGold, double goldIncome)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(production >= 0);
            AssertException.Assert(production <= this.production);
            AssertException.Assert(addGold >= 0);
            AssertException.Assert(addGold <= this.Player.Gold);
            AssertException.Assert(goldIncome > -.1);

            this.HP += hp;
            this.UndoAddProduction(production);
            this.Player.AddGold(-addGold);
            this.Player.GoldIncome(-goldIncome);

            return this.Tile;
        }

        public double GetPlanetDefenseDisbandValue(int hp, bool gold)
        {
            TurnException.CheckTurn(this.Player);

            if (gold)
            {
                double actual, rounded;
                GetDisbandGoldValue(hp, out actual, out rounded);
                return rounded;
            }
            else
            {
                return GetAddProduction(GetActualDisbandValue(hp), true);
            }
        }
        internal double GetActualDisbandValue(int hp)
        {
            return hp * PlanetDefenseCostPerHP * Consts.DisbandPct;
        }
        private void GetDisbandGoldValue(int hp, out double actual, out double rounded)
        {
            actual = GetActualDisbandValue(hp);
            rounded = Player.FloorGold(actual);
        }

        private double GetAttackCost(int shipDef)
        {
            return GetAttackCost(this.Att, this.Def, this.HP, shipDef);
        }
        private double GetAttackCost(int att, int def, int hp, int shipDef)
        {
            //only pay for the maximum HP you could possibly use
            return GetPDUpkeep(Math.Min(hp, ( att - 1 ) * shipDef + 1), att, def, Consts.PlanetDefensesAttackCostMult);
        }

        private double GetPDUpkeep(int hp, int att, int def, double mult)
        {
            return hp * mult * GetPDCost(att, def) * Consts.GetProductionUpkeepMult(Player.Game.MapSize);
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
            return damage * PlanetDefenseStrengthPerHP * Consts.ExperienceMult;
        }
        protected override double GetKillExp()
        {
            return GetExpForDamage(1 / PlanetDefenseCostPerHP);
        }

        internal override void AddExperience(double experience)
        {
            AddCostExperience(experience * this.PlanetDefenseCostPerHP / this.PlanetDefenseStrengthPerHP);
        }
        internal override void AddCostExperience(double cost)
        {
            this.Soldiers += GetExperienceSoldiers(this.Player, this.Population, this.Population, cost);
        }


        internal void BuildPlanetDefense(double prodInc)
        {
            BuildPlanetDefense(prodInc, false);
        }
        internal void BuildPlanetDefense(double prodInc, bool attAndDef)
        {
            double prod = ( this.production + prodInc );
            if (prod > Consts.FLOAT_ERROR)
            {
                prod /= 2;

                if (attAndDef)
                    BuildAttAndDef(prod);
                else
                    BuildPlanetDefense(prod, this.Buildable);

                BuildSoldiers(prod);
            }

            this.production = 0;
        }
        internal void BuildAttAndDef(double prod)
        {
            bool b = Game.Random.Bool();
            BuildPlanetDefense(prod / 2.0, b ? (Buildable)new Attack() : new Defense());
            BuildPlanetDefense(prod / 2.0, b ? (Buildable)new Defense() : new Attack());
        }
        private void BuildPlanetDefense(double prod, Buildable build)
        {
            double newAtt, newDef, newHP;
            GetPlanetDefenseInc(build, prod, out newAtt, out newDef, out newHP);
            ModPD(this.PlanetDefenseCost + prod, newAtt, newDef);
        }

        internal void BuildSoldiers(double prod)
        {
            this.Soldiers += Consts.GetExperience(prod / Consts.ProductionForSoldiers);
        }

        public void GetPlanetDefenseInc(Buildable buildable, double prod, out double newAtt, out double newDef, out double newHP, out double newSoldiers)
        {
            if (prod > Consts.FLOAT_ERROR)
            {
                prod /= 2;

                GetPlanetDefenseInc(buildable, prod, out newAtt, out newDef, out newHP);

                newSoldiers = this.Soldiers + prod / Consts.ProductionForSoldiers;
            }
            else
            {
                newAtt = this.Att;
                newDef = this.Def;
                newHP = this.HP;

                newSoldiers = this.Soldiers;
            }
        }
        private void GetPlanetDefenseInc(Buildable buildable, double prod, out double newAtt, out double newDef, out double newHP)
        {
            if (buildable is Attack)
                ModPD(this.PlanetDefenseCost + prod, this.Att, this.Player.PDAtt,
                        this.Def, this.Def, out newAtt, out newDef, out newHP);
            else if (buildable is Defense)
                ModPD(this.PlanetDefenseCost + prod, this.Att, this.Att,
                        this.Def, this.Player.PDDef, out newAtt, out newDef, out newHP);
            else
                throw new Exception();
        }

        private void ReduceDefenses(double mult)
        {
            double totalCost = this.PlanetDefenseCost;

            if (this.HP == 0 || mult < Consts.FLOAT_ERROR)
            {
                this.Att = 1;
                this.Def = 1;
                this.HP = 0;
            }
            else if (mult < 1 - Consts.FLOAT_ERROR)
            {
                double newAtt, newDef, newHP;
                ModPD(totalCost * mult, this.Att, 1, this.Def, 1, out newAtt, out newDef, out newHP);
                ModPD(totalCost * mult, newAtt, newDef);
            }

            this.player.GoldIncome(( totalCost - this.PlanetDefenseCost ) * Consts.DisbandPct);
        }

        private void ModPD(double trgCost, int att, int trgAtt, int def, int trgDef,
                out double newAtt, out double newDef, out double newHP)
        {
            if (( trgAtt > att && trgDef < def ) || ( trgAtt < att && trgDef > def ))
                throw new Exception();

            if (trgAtt == att && trgDef == def)
            {
                newAtt = att;
                newDef = def;
            }
            else
            {
                double minAtt = Math.Min(trgAtt, att), maxAtt = Math.Max(trgAtt, att), minDef = Math.Min(trgDef, def), maxDef = Math.Max(trgDef, def);
                if (TestPD(trgCost, maxAtt, maxDef))
                {
                    newAtt = maxAtt;
                    newDef = maxDef;
                }
                else if (!TestPD(trgCost, minAtt, minDef))
                {
                    newAtt = minAtt;
                    newDef = minDef;
                }
                else
                {
                    bool inc = ( trgAtt > att || trgDef > def );
                    double min = 0, max = 1;
                    do
                    {
                        double mult = ( min + max ) / 2;
                        newAtt = att + mult * ( trgAtt - att );
                        newDef = def + mult * ( trgDef - def );
                        if (inc == TestPD(trgCost, newAtt, newDef))
                            min = mult;
                        else
                            max = mult;
                    } while (max - min > Consts.FLOAT_ERROR);
                }
            }

            newHP = GetPDHP(trgCost, newAtt, newDef);
        }
        private bool TestPD(double trgCost, double minAtt, double minDef)
        {
            return ( GetPDHP(trgCost, minAtt, minDef) > ShipDesign.GetHPStr(minAtt, minDef) );
        }
        private double GetPDHP(double trgCost, double s1, double s2)
        {
            return trgCost / GetPDCost(s1, s2);
        }

        private void ModPD(double newCost, double newAtt, double newDef)
        {
            this.Att = GetPDStat(newAtt, this.Att, this.player.PDAtt);
            this.Def = GetPDStat(newDef, this.Def, this.player.PDDef);
            this.HP = GetPDStat(newCost / PlanetDefenseCostPerHP, this.HP, ushort.MaxValue);

            if (Math.Abs(GetPDCost(Att, Def) - GetPDCost(Def, Att)) > Consts.FLOAT_ERROR)
                throw new Exception();
        }
        private static int GetPDStat(double target, int current, int max)
        {
            float add = (float)( target - current );

            int min = 1;
            if (target > current)
                min = current;
            else
                max = current;
            int lowerCap = Math.Max(min - current, (int)Math.Ceiling(2.0 * add - max + current));

            if (add > lowerCap)
                return current + Game.Random.GaussianCappedInt(add, Consts.PlanetDefensesRndm, lowerCap);
            else
                return Game.Random.Round((float)target);
        }

        #endregion //planet defense
    }
}
