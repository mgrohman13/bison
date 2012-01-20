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
        private ushort _production;
        private float _defenseSoldiers, _researchRounding, _productionRounding, _soldierChange;

        internal Colony(Player player, Planet planet, int population, double soldiers, int production, IEventHandler handler)
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
                StartBuilding(handler.getNewBuild(this, true, false));
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

        internal void DoBuild(IEventHandler handler)
        {
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
                    this.Buildable.Build(this, tile, handler);
                    this.production -= this.Buildable.Cost;
                    LoseProductionPct(Consts.CarryProductionLossPct);

                    if (!this.Buildable.Multiple)
                        break;
                }
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
                StartBuilding(handler.getNewBuild(this, true, true));
                this.built = false;
            }
        }

        internal void EndTurn(ref double gold, ref int research, IEventHandler handler)
        {
            ResetMoved();

            //modify real values
            double population = 0, production = 0;
            TurnStuff(ref population, ref production, ref gold, ref research, true, false);

            this.Population += RoundValue(population, ref gold, Consts.PopulationIncomeForGold);
            this.production += RoundValue(production, ref gold);

            ResetRounding();

            //build planet defences first so they can attack this turn
            bool buildFirst = ( this.Buildable is PlanetDefense );
            if (buildFirst)
                this.DoBuild(handler);
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
                this.DoBuild(handler);

            DoChange(this._soldierChange, this._defenseAttChange, this._defenseDefChange, this._defenseHPChange);
        }

        private void TurnStuff(ref double population, ref double production, ref double gold, ref int research, bool doTurn, bool minGold)
        {
            //pay upkeep for stored production before getting income
            gold -= Upkeep;

            double goldInc;
            int prodInt, researchInc;
            GetTurnValues(out prodInt, out goldInc, out researchInc);
            double productionInc = prodInt;

            Ship repairShip = RepairShip;
            if (repairShip != null)
                repairShip.ProductionRepair(ref productionInc, ref gold, doTurn, minGold);

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
            TurnException.CheckTurn(this.player);

            GetTurnValues(this.Population, out production, out gold, out research);
        }

        private void GetTurnValues(int population, out int production, out double gold, out int research)
        {
            double income = GetTotalIncome(population);

            double researchPct = GetPct(this.player.ResearchEmphasis);
            double productionPct = GetPct(this.player.ProductionEmphasis);
            double totalPct = GetPct(this.player.GoldEmphasis) + researchPct + productionPct;
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

        internal void Bombard(int damage, out double exp)
        {
            int initPop = this.Population;
            exp = damage * Consts.TroopExperienceMult;

            LosePopulation(damage);

            this.soldiers += GetExperienceSoldiers(this.player, this.Population, initPop, exp);
        }

        internal int Invasion(Player player, int attackers, ref double soldiers, double gold, IEventHandler handler, ref int extra)
        {
            double initAttackers = attackers;
            double initPop = this.Population;

            double planetDamageMult = Consts.GetPlanetDamageMult();

            attackers = InvadePlanet(attackers, soldiers, ref gold, planetDamageMult);
            player.AddGold(gold);

            soldiers *= attackers / initAttackers;
            if (initPop > 0)
            {
                this.soldiers *= this.Population / initPop;
                //will reduce defendingSoldiers
                ReduceDefenses(this.Population / initPop);
            }
            else if (this.soldiers > Consts.FLOAT_ERROR)
            {
                throw new Exception();
            }

            //damage planet for every dead troop
            double deadPop = ( initAttackers - attackers + initPop - this.Population );
            Planet.ReduceQuality(Game.Random.Round(planetDamageMult * deadPop));

            if (attackers > 0 && this.Population > 0)
                throw new Exception();

            deadPop *= Consts.TroopExperienceMult;
            this.soldiers += GetExperienceSoldiers(this.player, this.Population, initPop, deadPop);
            soldiers += GetExperienceSoldiers(player, attackers, initAttackers, deadPop);

            //in the event of a tie, the defender keeps the planet with the remaining population of 0
            if (attackers > 0 && !Planet.Dead)
            {
                Destroy();
                return OccupyPlanet(player, attackers, ref soldiers, initPop, handler, ref extra);
            }
            else
            {
                //if the planet is destroyed any surviving attackers remain in transport
                extra = 0;
                return attackers;
            }
        }

        private static float GetExperienceSoldiers(Player player, int curPop, double initPop, double exp)
        {
            double mult = ( initPop > 0 ? Math.Sqrt(curPop / initPop) : 0 ) * exp;
            player.AddGold(exp - mult);
            return Game.Random.GaussianCapped((float)( mult / Consts.ProductionForSoldiers ), Consts.SoldiersRndm);
        }

        private int InvadePlanet(int attackers, double soldiers, ref double gold, double planetDamageMult)
        {
            if (this.Population > 0)
            {
                double defense = Consts.GetPlanetDefenseStrength(this.Population, this.TotalSoldiers);

                //when possible, try to maximize use of the spent gold and minimize losses
                int initialWave;
                if (gold > 0)
                    initialWave = GetInitialWave(attackers, soldiers, gold, this.Population, defense);
                else
                    initialWave = attackers;

                attackers = TroopBattle(attackers, soldiers, initialWave, ref gold, defense, planetDamageMult);
            }
            return attackers;
        }

        private int TroopBattle(int attackers, double soldiers, int initialWave, ref double gold, double defense, double planetDamageMult)
        {
            double mult = Consts.GetInvasionStrength(attackers, soldiers, initialWave, gold) / defense;
            attackers -= initialWave;

            double attStr = mult * initialWave;
            double defStr = this.Population;
            int planetStr = (int)Math.Ceiling(( Planet.Quality + 1 ) / planetDamageMult);

            double attLeft = ( attStr - defStr ) / mult;
            double defLeft = defStr - attStr;

            int dead = initialWave + this.Population;
            if (attStr > defStr)
                dead -= (int)Math.Ceiling(attLeft);
            else
                dead -= (int)Math.Ceiling(defLeft);

            if (dead > planetStr)
            {
                double attDead = Math.Min(Math.Ceiling(planetStr / ( mult + 1 )), Math.Ceiling(planetStr / ( mult + 1 ) * mult) / mult);

                gold *= ( initialWave - attDead ) / initialWave;
                initialWave -= Game.Random.Round(attDead);
                this.Population -= Game.Random.Round(mult * attDead);
            }
            else if (attStr > defStr)
            {
                gold = 0;
                initialWave = Game.Random.Round(attLeft);
                this.Population = 0;
            }
            else
            {
                gold = 0;
                initialWave = 0;
                this.Population = Game.Random.Round(defLeft);
            }

            return attackers + initialWave;
        }

        private int OccupyPlanet(Player player, int attackers, ref double soldiers, double initPop, IEventHandler handler, ref int extra)
        {
            int occupy = attackers;
            if (occupy + extra > 1 && initPop > 0)
            {
                occupy = handler.MoveTroops(null, occupy + extra, occupy, attackers, soldiers);
                extra = 0;
                //must occupy with at least 1 troop to capture
                if (occupy < 1)
                {
                    occupy = 1;
                }
                else if (occupy > attackers)
                {
                    extra = occupy - attackers;
                    occupy = attackers;
                }
            }
            else
            {
                extra = 0;
            }

            double moveSoldiers = MoveSoldiers(attackers, soldiers, occupy);
            soldiers -= moveSoldiers;
            player.NewColony(Planet, occupy, moveSoldiers, 0, handler);

            return attackers - occupy;
        }

        internal void Destroy()
        {
            if (this.Dead)
                throw new Exception();

            LosePopulation(this.Population);
            LoseProduction(this.production);
            if (this.HP > 0)
            {
                this.player.AddGold(this.GetDisbandValue());
                this.HP = 0;
            }
            if (this.defenseSoldiers > 0)
                this.player.AddGold(this.defenseSoldiers / Consts.DefendingSoldiersForGold);

            this.player.RemoveColony(this);

            this.player.DeathCheck();
        }

        internal void AddProduction(double production)
        {
            double addGold = 0;
            if (production > 0 && this.Buildable is StoreProd)
                LoseProduction(production * Consts.StoreProdLossPct, ref production, ref addGold, Consts.ProductionForGold);

            this.production += RoundValue(production, ref addGold);

            player.AddGold(addGold);
        }

        internal void BuildSoldiers()
        {
            if (this.production > 0)
            {
                this.soldiers += Game.Random.GaussianCapped(this.production / Consts.ProductionForSoldiers, Consts.SoldiersRndm);
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
                TurnException.CheckTurn(this.player);

                return this._buildable;
            }
        }

        public int Production
        {
            get
            {
                TurnException.CheckTurn(this.player);

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
                TurnException.CheckTurn(this.player);

                return PlanetDefenseUpkeep + Consts.GetProductionUpkeepMult(player.Game.MapSize) * this.production;
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
            set
            {
                TurnException.CheckTurn(this.Player);
                AssertException.Assert(value == null || this.Player == value.Player);

                this._repairShip = value;
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

        public static int GetInitialWave(int attackers, double soldiers, double gold, int population, double defense)
        {
            defense *= population;
            //find the least number of troops you can attack with and still win 100% of the time
            return MattUtil.TBSUtil.FindValue(delegate(int initialWave)
            {
                return ( Consts.GetInvasionStrengthBase(attackers, soldiers, initialWave, gold) * initialWave >= defense );
            }, 1, attackers, true);
        }

        public double GetPopulationGrowth()
        {
            //logistic growth
            double growth;
            if (this.Population > Planet.Quality)
                growth = 2 * Planet.Quality - this.Population;
            else
                growth = this.Population;

            growth *= Consts.PopulationGrowth;

            //plus 1 constant as a bonus for acquiring new planets before population exceeds quality on existing planets
            //and to make even pitiful planets have a small carrying capacity
            return growth + 1;
        }

        //public int GetTransIn(int max)
        //{
        //    if (this.Population + max <= this.Planet.Quality)
        //        return max;
        //    else
        //        return GetTrans(Math.Max(this.Planet.Quality - this.Population, 0), max, false,
        //                Math.Max(this.Population, this.Planet.Quality), true);
        //}

        //public int GetTransOut(int max)
        //{
        //    if (this.Population - max >= this.Planet.Quality)
        //        return max;
        //    else
        //        return GetTrans(Math.Max(this.Population - this.Planet.Quality, 0), max, false,
        //                Math.Min(this.Population, this.Planet.Quality), false);
        //}

        //private int GetTrans(int min, int max, bool trueHigh, int bestPop, bool add)
        //{
        //    int growth = GetPopulationGrowth(bestPop);
        //    return MattUtil.TBSUtil.FindValue(delegate(int trans)
        //    {
        //        return ( growth == GetPopulationGrowth(this.Population + ( add ? trans : -trans )) );
        //    }, min, max, trueHigh);
        //}

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
            TurnException.CheckTurn(this.player);

            double gold;
            int production, research;
            GetTurnValues(population, out production, out gold, out research);
            return production;
        }

        public void GetTurnIncome(ref double population, ref double production, ref double gold, ref int research, bool minGold)
        {
            TurnException.CheckTurn(this.player);

            //modify parameter values
            TurnStuff(ref population, ref production, ref gold, ref research, false, minGold);

            if (minGold)
            {
                MinGold(population, ref gold, Consts.PopulationIncomeForGold);
                MinGold(production, ref gold, 1);

                if (this.HP > 0)
                    foreach (Tile tile in Tile.GetNeighbors(this.Tile))
                    {
                        Ship ship = tile.SpaceObject as Ship;
                        if (ship != null && ship.Player != this.Player)
                            gold -= PlanetDefenseAttackCost;
                    }
            }
        }

        private void MinGold(double value, ref double gold, double rate)
        {
            gold += ( value - (int)Math.Ceiling(value) ) / rate;
        }

        public void SellProduction(int production)
        {
            TurnException.CheckTurn(this.player);
            AssertException.Assert(production > 0);
            AssertException.Assert(production <= this.production);

            LoseProduction(production);
        }

        private void LoseProduction(double loseProduction)
        {
            double production = this.production, gold = 0;
            LoseProduction(loseProduction, ref production, ref gold, Consts.ProductionForGold);

            this.production = RoundValue(production, ref gold, Consts.ProductionForGold);

            player.AddGold(gold);
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

        public void BuyProduction(int production)
        {
            TurnException.CheckTurn(this.player);
            AssertException.Assert(production > 0);
            double gold = production * Consts.GoldForProduction;
            AssertException.Assert(gold < this.player.Gold);

            this.production += production;
            this.player.SpendGold(gold);
        }

        public bool CanBuild(Buildable buildable)
        {
            return ( buildable == null || buildable.CanBeBuiltBy(this) );
        }

        public void StartBuilding(Buildable newBuild)
        {
            TurnException.CheckTurn(this.player);
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
            double oldCost = this.PlanetDefenseCost * this.HP;
            base.SetAtt(att);
            base.SetDef(def);
            base.SetHP(hp);
            ModDefenseSoldiers(oldCost);
        }
        protected override void SetAtt(int value)
        {
            double oldCost = this.PlanetDefenseCost * this.HP;
            base.SetAtt(value);
            ModDefenseSoldiers(oldCost);
        }
        protected override void SetDef(int value)
        {
            double oldCost = this.PlanetDefenseCost * this.HP;
            base.SetDef(value);
            ModDefenseSoldiers(oldCost);
        }
        protected override void SetHP(int value)
        {
            double oldCost = this.PlanetDefenseCost * this.HP;
            base.SetHP(value);
            ModDefenseSoldiers(oldCost);
        }
        private void ModDefenseSoldiers(double oldCost)
        {
            double cost = this.PlanetDefenseCost * this.HP - oldCost;
            if (cost != 0)
            {
                Console.WriteLine(this.defenseSoldiers);
                float diff = (float)( cost * Consts.PlanetDefensesSoldiersMult );
                Console.WriteLine(diff);
                this.defenseSoldiers += Game.Random.Gaussian(diff, Consts.SoldiersRndm);
                Console.WriteLine(this.defenseSoldiers);
                if (this.defenseSoldiers < 0 || ( this.HP == 0 && this.defenseSoldiers > 0 ))
                {
                    double gold = this.defenseSoldiers / Consts.DefendingSoldiersForGold;
                    Console.WriteLine(gold);
                    Player.AddGold(gold);
                    this.defenseSoldiers = 0;
                }
            }
        }

        public double GetPlanetDefenseSoldiers()
        {
            TurnException.CheckTurn(this.player);

            return this.defenseSoldiers;
        }

        public override double TotalSoldiers
        {
            get
            {
                return base.TotalSoldiers + this.defenseSoldiers;
            }
        }

        public void DisbandPlanetDefense(int hp, bool gold)
        {
            TurnException.CheckTurn(this.player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(hp <= this.HP);

            double actualValue = GetDisbandValue(hp);
            if (gold)
            {
                double disbandValue = GetPlanetDefenseDisbandValue(hp, gold);
                this.player.AddGold(disbandValue);
                for (double totalExtra = actualValue - disbandValue ; totalExtra != 0 ; )
                {
                    const double MIN = .05 - Consts.FLOAT_ERROR;
                    double extra;
                    if (totalExtra > 0)
                        extra = Math.Min(totalExtra, MIN);
                    else
                        extra = Math.Max(totalExtra, -MIN);
                    this.player.AddGold(extra);
                    totalExtra -= extra;
                }
            }
            else
            {
                AddProduction(actualValue);
            }
        }

        public double GetPlanetDefenseDisbandValue(int hp, bool gold)
        {
            TurnException.CheckTurn(this.player);

            double disbandValue = GetDisbandValue(1);
            if (gold)
            {
                double rounded = Player.RoundGold(disbandValue);
                if (rounded < .1)
                    rounded = .1;
                disbandValue = rounded * hp;
            }
            else
            {
                disbandValue *= hp;
            }

            return disbandValue;
        }

        private double GetDisbandValue()
        {
            return GetDisbandValue(this.HP);
        }

        private double GetDisbandValue(int hp)
        {
            return hp * PlanetDefenseCost * Consts.DisbandPct;
        }

        public double PlanetDefenseUpkeep
        {
            get
            {
                TurnException.CheckTurn(this.player);

                return this.HP * PlanetDefenseCost * Consts.PlanetDefensesUpkeepMult * Consts.GetProductionUpkeepMult(player.Game.MapSize);
            }
        }

        private double PlanetDefenseAttackCost
        {
            get
            {
                return PlanetDefenseUpkeep * Consts.PlanetDefensesAttackCostMult;
            }
        }

        internal double PlanetDefenseCost
        {
            get
            {
                return ShipDesign.GetPlanetDefenseCost(this.Att, this.Def, this.player.LastResearched);
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
            double cost = PlanetDefenseAttackCost;
            double pct = Combat(ship, handler);
            this.player.SpendGold(cost * ( 1 - pct ));
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
            this.soldiers += GetExperienceSoldiers(this.player, this.Population, this.Population, experience * this.PlanetDefenseCost / this.PlanetDefenseStrength);
        }

        internal void BuildPlanetDefense()
        {
            double newAtt, newDef, newHP;
            int att, def, hp;
            GetPlanetDefenseInc(this.production, out att, out def, out hp, out newAtt, out newDef, out newHP, true);

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

                ModStat(this.player.PlanetDefense.Att, this.Att, newHP, oldCost, out newAtt, out newAttInt);
                ModStat(this.player.PlanetDefense.Def, this.Def, newHP, oldCost, out newDef, out newDefInt);

                newHP += this.HP;
                newHP *= oldCost;
                if (doRound)
                    newHP /= ShipDesign.GetPlanetDefenseCost(newAttInt, newDefInt, this.player.LastResearched);
                else
                    newHP /= GetCost(newAtt, newDef, this.player.LastResearched);
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
                        double newHP = ( this.HP + Consts.MoveOrderGold / 3 / oldCost ) * ( 1 / mult - 1 );

                        double d;
                        int newAtt, newDef;
                        ModStat(1, this.Att, newHP, oldCost, out d, out newAtt);
                        ModStat(1, this.Def, newHP, oldCost, out d, out newDef);

                        SetPlanetDefense(newAtt, newDef, GetStat(mult * this.HP * oldCost /
                                ShipDesign.GetPlanetDefenseCost(newAtt, newDef, this.player.LastResearched), this.HP));
                    }
                }
                else
                {
                    this.HP = 0;
                    this.Att = 1;
                    this.Def = 1;
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
                double curHP = Math.Sqrt(this.HP + Consts.MoveOrderGold / 3 / cost);
                double value = ( Math.Pow(current, 1.0 / power) * curHP + Math.Pow(stat, 1.0 / power) * newHP ) / ( curHP + newHP );

                newStat = Math.Pow(value, power);
                newStatInt = GetStat(newStat, current);
            }
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

        //public IEnumerable<Ship> GetPlanetDefenseAttacks()
        //{
        //    if (this.HP > 0)
        //    {
        //        HashSet<Tile> tiles = Tile.GetNeighbors(this.Planet.Tile);
        //        foreach (Tile tile in Game.Random.Iterate<Tile>(tiles))
        //        {
        //            Ship ship = tile.SpaceObject as Ship;
        //            if (ship != null && ship.Player != this.Player)
        //            {
        //                double avgAtt, avgDef;
        //                Consts.GetDamageTable(this.Att, ship.Def, out avgAtt, out avgDef);
        //                bool attack = ( avgAtt >= ship.HP && avgAtt / ship.HP > avgDef / this.HP );

        //                if (!attack)
        //                {
        //                    if (ship.Population > 0)
        //                    {
        //                        double damage = avgAtt;
        //                        if (avgDef > this.HP)
        //                            damage *= this.HP / avgDef;
        //                        attack = ( ship.GetTransLoss(damage) * 3.9 > GetPlanetDefenseSoldiers(Math.Min(this.HP, avgDef)) );
        //                    }

        //                    if (!attack)
        //                    {
        //                        double defAtt, defDef;
        //                        Consts.GetDamageTable(ship.Att, this.Def, out defAtt, out defDef);
        //                        attack = ( avgAtt / avgDef >= defDef / ( defAtt + ship.GetFreeDmg(this) * ship.MaxSpeed ) );

        //                        if (!attack)
        //                            attack = !HasFreeSpace(tiles, tile);
        //                    }
        //                }

        //                if (attack)
        //                {
        //                    yield return ship;
        //                    if (this.HP == 0)
        //                        yield break;
        //                }
        //            }
        //        }
        //    }
        //}

        //private bool HasFreeSpace(HashSet<Tile> tiles, Tile tile)
        //{
        //    bool freeSpace = false;
        //    foreach (Tile t2 in tiles)
        //    {
        //        Ship s2 = tile.SpaceObject as Ship;
        //        if (s2 != null && s2.Player != this.Player)
        //        {
        //            freeSpace = true;
        //            break;
        //        }
        //    }
        //    return freeSpace;
        //}

        #endregion //planet defense
    }
}
