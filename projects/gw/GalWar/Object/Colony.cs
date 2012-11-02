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
        private ushort _production, _prodHeal;
        private float _defenseSoldiers, _researchRounding, _productionRounding, _soldierChange;

        internal Colony(IEventHandler handler, Player player, Planet planet, int population, double soldiers, int production)
        {
            this.player = player;
            this.Planet = planet;
            planet.Colony = this;

            //set the build intially to StoreProd so it can be changed to anything with no production loss
            this._buildable = player.Game.StoreProd;
            this._repairShip = null;

            this.production = production;
            this.defenseSoldiers = 0;

            this.Population = population;
            this.movedPop = population;
            this.soldiers = soldiers;

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

        private float defenseSoldiers
        {
            get
            {
                return this._defenseSoldiers;
            }
            set
            {
                this._defenseSoldiers = value;
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
            this._soldierChange = (float)( this.GetTotalSoldierPct() - soldierChange );
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
            ProdHeal = 0;
            ResetMoved();

            //modify real values
            double population = 0, production = 0;
            TurnStuff(ref population, ref production, ref gold, ref research, true, false);

            this.Population += RoundValue(population, ref gold, Consts.PopulationIncomeForGold);

            ResetRounding();

            //build planet defences first so they can attack this turn
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
            {
                int hp = repairShip.HP;
                repairShip.ProductionRepair(ref productionInc, ref gold, doTurn, minGold);
                if (doTurn)
                    ProdHeal = repairShip.HP - hp;
            }

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
            this.soldiers += GetExperienceSoldiers(this.Player, this.Population, initPop, exp);
            return exp;
        }

        internal void Invasion(IEventHandler handler, Player attackPlayer, ref int attackers, ref double soldiers, int gold)
        {
            double initAttackers = attackers;
            double initPop = this.Population;

            double planetDamageMult = Consts.GetPlanetDamageMult();

            TroopBattle(attackPlayer, ref attackers, ref soldiers, gold, planetDamageMult);

            //damage planet for every dead troop
            double deadPop = ( initAttackers - attackers + initPop - this.Population );
            int reduceQuality = Game.Random.Round(planetDamageMult * deadPop);
            if (reduceQuality > Planet.Quality + 3)
                throw new Exception();
            Planet.ReduceQuality(reduceQuality);

            if (attackers > 0 && this.Population > 0)
                throw new Exception();

            deadPop *= Consts.TroopExperienceMult;
            this.soldiers += GetExperienceSoldiers(this.Player, this.Population, initPop, deadPop);
            soldiers += GetExperienceSoldiers(attackPlayer, attackers, initAttackers, deadPop);

            //in the event of a tie, the defender keeps the planet with the remaining population of 0
            if (attackers > 0 && !Planet.Dead)
            {
                Destroy();
                OccupyPlanet(handler, attackPlayer, ref attackers, ref soldiers, initPop);
            }
        }

        private static float GetExperienceSoldiers(Player player, int curPop, double initPop, double exp)
        {
            double mult = ( initPop > 0 ? Math.Sqrt(curPop / initPop) : 0 ) * exp;
            player.GoldIncome(exp - mult);
            return Game.Random.GaussianCapped((float)( mult / Consts.ProductionForSoldiers ), Consts.SoldiersRndm);
        }

        private void TroopBattle(Player attackPlayer, ref int attackers, ref double soldiers, int gold, double planetDamageMult)
        {
            double initAttackers = attackers;
            double initDefenders = this.Population;

            if (initDefenders > 0)
            {
                double defense = Consts.GetPlanetDefenseStrength(this.Population, this.TotalSoldiers);
                double attMult = Consts.GetInvasionStrength(attackers, soldiers, gold, initDefenders * defense) / defense;

                double attStr = attMult * initAttackers;

                double attackersLeft = ( attStr - initDefenders ) / attMult;
                double defendersLeft = initDefenders - attStr;

                double deadPop = initAttackers + initDefenders;
                if (attStr > initDefenders)
                    deadPop -= attackersLeft;
                else
                    deadPop -= defendersLeft;

                if (Math.Floor(Math.Floor(deadPop) * planetDamageMult) > Planet.Quality)
                {
                    double deadAttackers = (int)Math.Ceiling(( Planet.Quality + 1 ) / planetDamageMult) / ( attMult + 1 );
                    deadAttackers = Math.Min(Math.Ceiling(deadAttackers), Math.Ceiling(deadAttackers * attMult) / attMult);
                    double deadDefenders = attMult * deadAttackers;

                    //only pay for the portion of gold spent until the planet is destroyed
                    attackPlayer.AddGold(gold * Math.Min(( initAttackers - deadAttackers ) / initAttackers, ( initDefenders - deadDefenders ) / initDefenders));

                    attackers -= Game.Random.Round(deadAttackers);
                    this.Population -= Game.Random.Round(deadDefenders);
                }
                else if (attStr > initDefenders)
                {
                    attackers = Game.Random.Round(attackersLeft);
                    this.Population = 0;
                }
                else
                {
                    attackers = 0;
                    this.Population = Game.Random.Round(defendersLeft);
                }

                soldiers *= attackers / initAttackers;

                double defLeftMult = this.Population / initDefenders;
                this.soldiers *= defLeftMult;
                //will reduce defendingSoldiers
                ReduceDefenses(defLeftMult);
            }
            else if (this.soldiers > Consts.FLOAT_ERROR)
            {
                throw new Exception();
            }
        }

        private void OccupyPlanet(IEventHandler handler, Player occupyingPlayer, ref int attackers, ref double soldiers, double initPop)
        {
            int occupy = attackers;
            if (initPop > 0 && attackers > 1)
            {
                occupy = handler.MoveTroops(null, occupy, occupy, occupy, soldiers);
                if (occupy < 1)
                    occupy = 1;
            }

            double moveSoldiers = MoveSoldiers(attackers, soldiers, occupy);
            occupyingPlayer.NewColony(handler, Planet, occupy, moveSoldiers, 0);

            attackers -= occupy;
            soldiers -= moveSoldiers;
        }

        internal void Destroy()
        {
            if (this.Dead)
                throw new Exception();

            this.Player.AddGold(( this.Population / Consts.PopulationForGold ) + ( this.soldiers / Consts.SoldiersForGold )
                    + ( this.production / Consts.ProductionForGold )
                    + ( this.TotalDisbandValue ) + ( this.defenseSoldiers / Consts.DefendingSoldiersForGold ));

            this.Population = 0;
            this.soldiers = 0;
            this.production = 0;
            base.SetHP(0);
            this.defenseSoldiers = 0;

            this.Player.RemoveColony(this);
            this.Player.DeathCheck();
        }

        internal void AddProduction(double production)
        {
            double addGold = 0;
            if (production > 0 && this.Buildable is StoreProd)
                LoseProduction(production * Consts.StoreProdLossPct, ref production, ref addGold, Consts.ProductionForGold);

            this.production += RoundValue(production, ref addGold);

            Player.GoldIncome(addGold);
        }

        internal void BuildSoldiers(double prodInc)
        {
            if (this.production + prodInc > 0)
            {
                this.soldiers += Game.Random.GaussianCapped(( this.production + prodInc ) / Consts.ProductionForSoldiers, Consts.SoldiersRndm);
                this.production = 0;
            }
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
        private int production
        {
            get
            {
                return this._production;
            }
            set
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

                return PlanetDefenseTotalUpkeep + Consts.GetProductionUpkeepMult(Player.Game.MapSize) * this.production;
            }
        }

        public Ship RepairShip
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                //best place to make sure ship is still valid to repair
                if (this._repairShip != null &&
                        ( this._repairShip.HP == this._repairShip.MaxHP || this._repairShip.Dead
                        || !Tile.IsNeighbor(this.Tile, this._repairShip.Tile) ))
                    this._repairShip = null;
                return this._repairShip;
            }
            internal set
            {
                this._repairShip = value;
            }
        }

        //blerg
        public void SetRepairShip(IEventHandler handler, Ship value)
        {
            handler = new HandlerWrapper(handler);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(value == null || this.Player == value.Player);

            this.RepairShip = value;
        }

        public int ProdHeal
        {
            get
            {
                return this._prodHeal;
            }
            private set
            {
                checked
                {
                    this._prodHeal = (ushort)value;
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
                MinGold(popInc, ref gold, Consts.PopulationIncomeForGold);
                if (this.Buildable == null || !this.Buildable.HandlesFraction)
                    MinGold(prodInc, ref gold, 1);

                //planet defense attack cost
                int att, def, hp;
                this.GetPlanetDefenseIncMinGold(this.production + prodInc, out att, out def, out hp);
                if (hp > 0)
                    foreach (Tile tile in Tile.GetNeighbors(this.Tile))
                    {
                        Ship ship = tile.SpaceObject as Ship;
                        if (ship != null && ship.Player != this.Player)
                            gold -= GetPlanetDefenseAttackCost(att, def, hp, ship.Def);
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
            if (minGold ? ( shipDesign != null && shipDesign.Trans > 0 ) : ( this.Buildable != null && this.Buildable.Cost > 0 ))
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
            if (!( this.Buildable is PlanetDefense ))
                production = 0;
            double att, def, hp;
            GetPlanetDefenseInc(production, out att, out def, out hp);
            newAtt = (int)Math.Ceiling(att);
            newDef = (int)Math.Ceiling(def);
            newHP = (int)Math.Ceiling(hp);
        }

        //blerg
        public void SellProduction(IEventHandler handler, int production)
        {
            handler = new HandlerWrapper(handler);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(production > 0);
            AssertException.Assert(production <= this.production);

            LoseProduction(production);
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

        //blerg
        public void BuyProduction(IEventHandler handler, int production)
        {
            handler = new HandlerWrapper(handler);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(production > 0);
            double gold = production * Consts.GoldForProduction;
            AssertException.Assert(gold < this.Player.Gold);

            this.production += production;
            this.Player.SpendGold(gold);
        }

        public bool CanBuild(Buildable buildable)
        {
            return ( buildable == null || buildable.CanBeBuiltBy(this) );
        }

        //blerg
        public void StartBuilding(IEventHandler handler, Buildable newBuild)
        {
            handler = new HandlerWrapper(handler);
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

        private void SetPlanetDefense(int att, int def, int hp)
        {
            double oldCost = this.PlanetDefenseTotalCost;
            base.SetAtt(att);
            base.SetDef(def);
            base.SetHP(hp);
            ModDefenseSoldiers(oldCost);
        }
        //should only be setting att and def through SetPlanetDefense()
        protected override void SetAtt(int value)
        {
            throw new Exception();
        }
        protected override void SetDef(int value)
        {
            throw new Exception();
        }
        protected override void SetHP(int value)
        {
            double oldCost = this.PlanetDefenseTotalCost;
            base.SetHP(value);
            ModDefenseSoldiers(oldCost);
        }
        private void ModDefenseSoldiers(double oldCost)
        {
            double cost = this.PlanetDefenseTotalCost - oldCost;
            if (cost != 0)
            {
                float diff = (float)( cost * Consts.PlanetDefensesSoldiersMult );
                this.defenseSoldiers += Game.Random.Gaussian(diff, Consts.SoldiersRndm);
                if (this.defenseSoldiers < 0 || ( this.HP == 0 && this.defenseSoldiers > 0 ))
                {
                    double gold = this.defenseSoldiers / Consts.DefendingSoldiersForGold;
                    Player.GoldIncome(gold);
                    this.defenseSoldiers = 0;
                }
            }
        }

        public double PlanetDefenseSoldiers
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.defenseSoldiers;
            }
        }

        public override double TotalSoldiers
        {
            get
            {
                return base.TotalSoldiers + this.defenseSoldiers;
            }
        }

        //blerg
        public void DisbandPlanetDefense(IEventHandler handler, int hp, bool gold)
        {
            handler = new HandlerWrapper(handler);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(hp <= this.HP);

            double actualValue = GetDisbandValue(hp);
            if (gold)
                this.Player.AddGold(actualValue, GetPlanetDefenseDisbandValue(hp, gold));
            else
                AddProduction(actualValue);

            this.HP -= hp;
        }

        public double GetPlanetDefenseDisbandValue(int hp, bool gold)
        {
            TurnException.CheckTurn(this.Player);

            double disbandValue = GetDisbandValue(1);
            if (gold)
            {
                disbandValue = Player.RoundGold(disbandValue);
                if (disbandValue < .1)
                    disbandValue = .1;
            }

            return ( hp * disbandValue );
        }

        private double TotalDisbandValue
        {
            get
            {
                return GetDisbandValue(this.HP);
            }
        }

        private double GetDisbandValue(int hp)
        {
            return hp * PlanetDefenseCost * Consts.DisbandPct;
        }

        public double PlanetDefenseTotalUpkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return GetPlanetDefenseUpkeep(this.HP, this.Att, this.Def, Consts.PlanetDefensesUpkeepMult);
            }
        }

        private double GetPlanetDefenseAttackCost(int shipDef)
        {
            return GetPlanetDefenseAttackCost(this.Att, this.Def, this.HP, shipDef);
        }

        private double GetPlanetDefenseAttackCost(int att, int def, int hp, int shipDef)
        {
            //only pay for the maximum HP you could possibly use
            return GetPlanetDefenseUpkeep(Math.Min(hp, ( att - 1 ) * shipDef + 1), att, def, Consts.PlanetDefensesAttackCostMult);
        }

        private double GetPlanetDefenseUpkeep(int hp, int att, int def, double mult)
        {
            return hp * mult * ShipDesign.GetPlanetDefenseCost(att, def, this.Player.LastResearched) * Consts.GetProductionUpkeepMult(Player.Game.MapSize);
        }

        internal double PlanetDefenseCost
        {
            get
            {
                return ShipDesign.GetPlanetDefenseCost(this.Att, this.Def, this.Player.LastResearched);
            }
        }

        private double PlanetDefenseTotalCost
        {
            get
            {
                return this.HP * PlanetDefenseCost;
            }
        }

        public double PlanetDefenseStrength
        {
            get
            {
                return ShipDesign.GetPlanetDefenseStrength(this.Att, this.Def);
            }
        }

        private void AttackShip(Ship ship, IEventHandler handler)
        {
            //get the attack cost before possibly being injured
            double cost = GetPlanetDefenseAttackCost(ship.Def);
            double pct = Combat(handler, ship);
            this.Player.SpendGold(cost * ( 1 - pct ));
        }

        protected override double GetExpForDamage(double damage)
        {
            return damage * PlanetDefenseStrength * Consts.ExperienceMult;
        }

        protected override double GetKillExp()
        {
            return GetExpForDamage(1 / PlanetDefenseCost);
        }

        protected override void AddExperience(double experience)
        {
            this.soldiers += GetExperienceSoldiers(this.Player, this.Population, this.Population, experience * this.PlanetDefenseCost / this.PlanetDefenseStrength);
        }

        internal void BuildPlanetDefense(double prodInc)
        {
            double newAtt, newDef, newHP;
            int att, def, hp;
            GetPlanetDefenseInc(this.production + prodInc, out att, out def, out hp, out newAtt, out newDef, out newHP, true);

            SetPlanetDefense(att, def, hp);
            this.production = 0;
        }

        public void GetPlanetDefenseInc(double production, out double newAtt, out double newDef, out double newHP)
        {
            int newAttInt, newDefInt, newHPInt;
            GetPlanetDefenseInc(production, out newAttInt, out newDefInt, out newHPInt, out newAtt, out newDef, out newHP, false);
        }

        private void GetPlanetDefenseInc(double production, out int newAttInt, out int newDefInt, out int newHPInt, out double newAtt, out double newDef, out double newHP, bool doRound)
        {
            if (production > 0)
            {
                double oldCost = PlanetDefenseCost;
                newHP = production / oldCost;

                ModStat(this.Player.PlanetDefense.Att, this.Att, newHP, oldCost, out newAtt, out newAttInt);
                ModStat(this.Player.PlanetDefense.Def, this.Def, newHP, oldCost, out newDef, out newDefInt);

                newHP += this.HP;
                newHP *= oldCost;
                if (doRound)
                    newHP /= ShipDesign.GetPlanetDefenseCost(newAttInt, newDefInt, this.Player.LastResearched);
                else
                    newHP /= GetCost(newAtt, newDef, this.Player.LastResearched);
                newHPInt = GetStat(newHP, this.HP);
            }
            else
            {
                newAtt = newAttInt = this.Att;
                newDef = newDefInt = this.Def;
                newHP = newHPInt = this.HP;
            }
        }

        private void ReduceDefenses(double mult)
        {
            if (mult < 1)
                if (mult > 0)
                {
                    if (!this.MinDefenses)
                    {
                        double oldCost = PlanetDefenseCost;
                        double newHP = GetCurHpAdj(oldCost) * ( 1 / mult - 1 );

                        double d;
                        int newAtt, newDef;
                        ModStat(1, this.Att, newHP, oldCost, out d, out newAtt);
                        ModStat(1, this.Def, newHP, oldCost, out d, out newDef);

                        SetPlanetDefense(newAtt, newDef, GetStat(mult * this.HP * oldCost /
                                ShipDesign.GetPlanetDefenseCost(newAtt, newDef, this.Player.LastResearched), this.HP));
                    }
                }
                else
                {
                    SetPlanetDefense(1, 1, 0);
                }
        }

        private void ModStat(int stat, int current, double newHP, double cost, out double newStat, out int newStatInt)
        {
            if (stat == current || newHP == 0)
            {
                newStat = current;
                newStatInt = current;
            }
            else
            {
                //higher power causes more of a bias towards lower values
                const double power = 3.9;

                newHP = Math.Sqrt(newHP);
                double curHP = Math.Sqrt(GetCurHpAdj(cost));
                double value = ( Math.Pow(current, 1 / power) * curHP + Math.Pow(stat, 1 / power) * newHP ) / ( curHP + newHP );

                newStat = Math.Pow(value, power);
                newStatInt = GetStat(newStat, current);
            }
        }

        private double GetCurHpAdj(double cost)
        {
            return ( this.HP + ( Consts.MoveOrderGold / Math.E / cost ) );
        }

        private static int GetStat(double stat, int current)
        {
            int lowerCap = 0;
            if (current > 2 * stat - 1)
                lowerCap = 1 + (int)Math.Ceiling(current - 2 * stat);
            double average = Math.Abs(stat - current);
            int randed;
            if (average > lowerCap)
            {
                randed = Game.Random.GaussianCappedInt(average, Consts.PlanetDefensesRndm, lowerCap);
                if (current > stat)
                    randed = current - randed;
                else
                    randed = current + randed;
            }
            else
            {
                randed = Game.Random.Round(stat);
            }

            return randed;
        }

        private static double GetCost(double newAtt, double newDef, int research)
        {
            int attLow = (int)newAtt;
            int defLow = (int)newDef;
            int attHigh = attLow + 1;
            int defHigh = defLow + 1;
            double attHighMult = newAtt - attLow;
            double defHighMult = newDef - defLow;
            double attLowMult = 1 - attHighMult;
            double defLowMult = 1 - defHighMult;
            return ShipDesign.GetPlanetDefenseCost(attLow, defLow, research) * attLowMult * defLowMult
                    + ShipDesign.GetPlanetDefenseCost(attHigh, defLow, research) * attHighMult * defLowMult
                    + ShipDesign.GetPlanetDefenseCost(attLow, defHigh, research) * attLowMult * defHighMult
                    + ShipDesign.GetPlanetDefenseCost(attHigh, defHigh, research) * attHighMult * defHighMult;
        }

        #endregion //planet defense
    }
}
