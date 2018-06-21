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

        private Buildable _buildable;
        private Ship _repairShip;

        private bool _built, _pauseBuild;
        private sbyte _defenseAttChange, _defenseDefChange;
        private short _defenseHPChange;
        private ushort _repair, _production;
        private uint _defenseResearch;
        private float _soldierChange, _prodGuess, _researchRounding, _productionRounding;

        internal Colony(IEventHandler handler, Player player, Planet planet, int population, double soldiers, int production)
            : base(null, 1, 1, 0, population, soldiers)
        {
            checked
            {
                this.Planet = planet;
                planet.Colony = this;

                this._player = player;

                //set the build intially to StoreProd so it can be changed to anything with no production loss
                this._buildable = player.Game.StoreProd;
                this._repairShip = null;

                this._built = ( handler == null );
                this._pauseBuild = false;

                this._defenseAttChange = 0;
                this._defenseDefChange = 0;
                this._defenseHPChange = 0;
                this._repair = 0;

                this._production = (ushort)production;

                this._defenseResearch = 0;

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

        public Buildable Buildable
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this._buildable;
            }
            private set
            {
                checked
                {
                    this._buildable = value;
                }
            }
        }
        public bool PauseBuild
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return ( this._pauseBuild && ( this.Buildable is ShipDesign ) );
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

        private int defenseResearch
        {
            get
            {
                checked
                {
                    if (this.HP == 0 || this._defenseResearch == 0)
                        defenseResearch = Player.Research;
                    return (int)this._defenseResearch;
                }
            }
            set
            {
                checked
                {
                    this._defenseResearch = (uint)value;
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
            Buildable newBuild;
            bool newPause;
            handler.getNewBuild(this, out newBuild, out newPause);
            StartBuilding(handler, newBuild, newPause);
        }
        internal void SetBuildable(Buildable newBuild, double losspct, bool pause)
        {
            this.Buildable = newBuild;
            this.PauseBuild = pause;

            LoseProductionPct(losspct);
        }
        internal int SetBuildableCeilLoss(Buildable newBuild, double losspct, bool pause)
        {
            this.Buildable = newBuild;
            this.PauseBuild = pause;

            int loss = (int)Math.Ceiling(this.production * losspct);
            LoseProduction(loss);
            return loss;
        }

        internal bool DoBuild(IEventHandler handler, double productionInc, ref double gold)
        {
            bool retVal = false;

            if (this.Buildable != null && this.Buildable.HandlesFraction)
                this.Buildable.SetFraction(productionInc);
            else
                this.production += RoundValue(productionInc, ref gold);

            //actual building of new ships happens at turn end
            if (this.Buildable != null && !this.PauseBuild)
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
                        retVal = true;
                    }

                    //build the ship
                    this.Buildable.Build(handler, this, tile);
                    this.production -= this.Buildable.Cost;
                    LoseProductionPct(Consts.CarryProductionLossPct);

                    if (!this.Buildable.Multiple)
                        break;
                }

                if (!double.IsNaN(this.Buildable.production) && this.Buildable.production != 0)
                    throw new Exception();
            }

            return retVal;
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
            bool buildFirst = ( this.Buildable is PlanetDefense ), built = false;
            if (buildFirst)
                built = this.DoBuild(handler, production, ref gold);

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
                built = this.DoBuild(handler, production, ref gold);

            if (built || this.Buildable is PlanetDefense)
                this.ProdGuess = 0;
            else if (this.Repair == 0)
                this.ProdGuess += GetTotalIncome() / 3.0;
            else if (this.RepairShip == null)
                this.ProdGuess += GetTotalIncome() / 6.0;

            DoChange(this.SoldierChange, this.DefenseAttChange, this.DefenseDefChange, this.DefenseHPChange);
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
                if (Player.NegativeGold())
                {
                    double mult = Math.Pow(-( Player.Gold + Player.TotalGold ) * Consts.AverageQuality * Consts.Income / Player.GetTotalIncome() / 91.0 + 1.69, 0.78);
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
            double exp = ( initAttackers - attackers ) + ( initPop - this.Population ) + reduceQuality;

            handler.OnInvade(ship, this, attackers, attSoldiers, goldSpent, attack, defense);

            Planet.ReduceQuality(reduceQuality);
            if (attackers > 0 && this.Population > 0)
                throw new Exception();

            if (Planet.Dead)
                exp += Consts.PlanetConstValue;
            exp *= Consts.TroopExperienceMult;
            this.Soldiers += GetExperienceSoldiers(this.Player, this.Population, this.Soldiers, initPop, exp);
            double shipValueExp;
            attSoldiers += GetExperienceSoldiers(attackers, attSoldiers, initAttackers, exp, out shipValueExp);
            ship.AddExperience(0, shipValueExp);

            handler.OnInvade(ship, this, attackers, attSoldiers, goldSpent, attack, defense);

            //in the event of a tie, the defender keeps the planet with the remaining population of 0
            if (attackers > 0 && !Planet.Dead)
            {
                Destroy();
                OccupyPlanet(handler, Planet, attackPlayer, ref attackers, ref attSoldiers, initPop);
            }
        }

        private static double GetExperienceSoldiers(Player player, int curPop, double curSoldiers, double initPop, double exp)
        {
            double other;
            double soldiers = GetExperienceSoldiers(curPop, curSoldiers, initPop, exp, out other);
            player.GoldIncome(other);
            return soldiers;
        }
        private static double GetExperienceSoldiers(int curPop, double curSoldiers, double initPop, double exp, out double other)
        {
            double mult = ( initPop > 0 ? Math.Sqrt(curPop / initPop) : 0 ) * exp;
            if (curPop > 0)
                mult *= GetSoldierMult(curPop, curSoldiers, GetSoldiersForExp(mult));
            else
                mult = 0;
            other = ( exp - mult );
            return Consts.GetExperience(GetSoldiersForExp(mult));
        }
        private static double GetSoldiersForExp(double exp)
        {
            return exp / Consts.ExpForSoldiers;
        }

        internal static double GetSoldierMult(int curPop, double curSoldiers, double addSoldiers)
        {
            return 1 / ( 1 + ( curSoldiers + addSoldiers ) / curPop );
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
            occupyingPlayer.NewColony(handler, planet, occupy, moveSoldiers, 0);

            attackers -= occupy;
            soldiers -= moveSoldiers;
        }

        internal void Destroy()
        {
            if (this.Dead)
                throw new Exception();

            int newAtt, newDef;
            double gold = ( this.Population / Consts.PopulationForGoldLow )
                    + ( this.Soldiers / Consts.SoldiersForGold )
                    + ( this.production / Consts.ProductionForGold )
                    + ( this.GetActualDisbandValue(this.HP, out newAtt, out newDef) );
            this.Player.AddGold(gold, true);

            Console.WriteLine("Destroy Gold:  " + gold);
            Console.WriteLine("Population:  " + this.Population / Consts.PopulationForGoldLow);
            Console.WriteLine("Soldiers:  " + this.Soldiers / Consts.SoldiersForGold);
            Console.WriteLine("Production:  " + this.production / Consts.ProductionForGold);
            Console.WriteLine("Planet Defense:  " + this.GetActualDisbandValue(this.HP, out newAtt, out newDef));
            Console.WriteLine();

            if (( this.HP > 0 ) ? ( newAtt != 1 || newDef != 1 ) : ( newAtt != Math.Max(this.Att - 1, 1) || newDef != Math.Max(this.Def - 1, 1) ))
                throw new Exception();

            this.Population = 0;
            this.Soldiers = 0;
            this.production = 0;
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
            if (this.Buildable == null)
                throw new Exception();

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

        public int Production
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.production;
            }
        }

        public double Upkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return PlanetDefenseUpkeep + Consts.GetProductionUpkeepMult(Player.Game.MapSize) * this.production + Consts.GetSoldierUpkeep(this);
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
                    gold += loss / Consts.ProductionForGold - shipDesign.Upkeep * Consts.UpkeepUnmovedReturn;
                    if (minGold)
                    {
                        int thisPop = this.Population + (int)Math.Floor(this.GetPopulationGrowth());
                        int move = shipDesign.Trans;
                        if (move > thisPop)
                            move = thisPop + 1;
                        gold -= GetGoldCost(move, PopCarrier.GetMoveSoldiers(thisPop, this.Soldiers, move));
                    }
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
                GetPlanetDefenseInc(production, out att, out def, out hp, out soldiers);
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

            TradeProduction(production, 1 / Consts.ProductionForGold);

            return this.Tile;
        }
        public void BuyProduction(IEventHandler handler, int production)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false, true);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(production > 0);
            AssertException.Assert(production * Consts.GoldForProduction < this.Player.Gold);

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

        public void StartBuilding(IEventHandler handler, Buildable newBuild, bool pause)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(CanBuild(newBuild));

            if (this.Buildable != newBuild || this.PauseBuild != pause)
            {
                Buildable oldBuild = this.Buildable;
                bool oldPause = this.PauseBuild;
                int loss = 0;
                if (this.Buildable != newBuild)
                    loss = SetBuildableCeilLoss(newBuild, GetLossPct(newBuild), pause);
                else
                    this.PauseBuild = pause;
                Player.Game.PushUndoCommand(new Game.UndoCommand<Buildable, int, bool>(
                        new Game.UndoMethod<Buildable, int, bool>(UndoStartBuilding), oldBuild, loss, oldPause));
            }
        }
        internal Tile UndoStartBuilding(Buildable oldBuild, int loss, bool oldPause)
        {
            AssertException.Assert(CanBuild(oldBuild));
            AssertException.Assert(( this.Buildable != oldBuild ) || ( loss == 0 && this.PauseBuild != oldPause ));
            AssertException.Assert(loss >= 0);

            this.Buildable = oldBuild;
            this.PauseBuild = oldPause;
            this.production += loss;
            this.Player.SpendGold(loss / Consts.ProductionForGold);

            return this.Tile;
        }

        public double GetLossPct(Buildable newBuild)
        {
            if (Buildable == newBuild)
                return 0;
            if (Buildable == null || newBuild == null)
                return 1;

            double lossPct;

            if (( Buildable is ShipDesign ) == ( newBuild is ShipDesign ))
                lossPct = Consts.SwitchBuildLossPct;
            else
                lossPct = Consts.SwitchBuildTypeLossPct;

            if (Buildable is StoreProd)
                lossPct = 0;
            else if (newBuild is StoreProd)
                lossPct = 1 - ( ( 1 - lossPct ) * ( 1 - Consts.StoreProdLossPct ) );

            return lossPct;
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

                return GetPDUpkeep(this.Att, this.Def, this.HP);
            }
        }
        internal double PlanetDefenseCostAvgResearch
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
            return ShipDesign.GetPlanetDefenseCost(att, def, this.defenseResearch);
        }

        public void DisbandPlanetDefense(IEventHandler handler, int hp, bool gold)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp >= 0);
            AssertException.Assert(hp <= this.HP);
            AssertException.Assert(gold || this.Buildable != null);

            int newAtt, newDef;

            int production;
            double addGold, goldIncome;
            if (gold)
            {
                GetDisbandGoldValue(hp, out goldIncome, out addGold, out newAtt, out newDef);
                Player.AddGold(goldIncome, addGold);

                production = 0;
                goldIncome -= addGold;
            }
            else
            {
                AddProduction(GetActualDisbandValue(hp, out newAtt, out newDef), true, out goldIncome, out production);

                addGold = 0;
            }

            newAtt = this.Att - newAtt;
            newDef = this.Def - newDef;

            this.HP -= hp;
            this.Att -= newAtt;
            this.Def -= newDef;

            Player.Game.PushUndoCommand(new Game.UndoCommand<int, int, int, int, double, double>(
                    new Game.UndoMethod<int, int, int, int, double, double>(UndoDisbandPlanetDefense), newAtt, newDef, hp, production, addGold, goldIncome));
        }
        private Tile UndoDisbandPlanetDefense(int att, int def, int hp, int production, double addGold, double goldIncome)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp >= 0);
            AssertException.Assert(att >= 0);
            AssertException.Assert(def >= 0);
            AssertException.Assert(production >= 0);
            AssertException.Assert(production <= this.production);
            AssertException.Assert(addGold >= 0);
            AssertException.Assert(goldIncome > -.1);

            this.HP += hp;
            this.Att += att;
            this.Def += def;
            this.UndoAddProduction(production);
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
        internal double GetActualDisbandValue(int hp, out int newAtt, out int newDef)
        {
            double oldCost = this.PlanetDefenseCost;

            newAtt = this.Att;
            newDef = this.Def;

            int newHP = this.HP - hp;
            double hpMult = HP / ShipDesign.GetHPStr(Att, Def);

            double mult = 1, step = 1 / ( Att * Def * Consts.FLOAT_ERROR_ONE );
            do
            {
                mult -= step;
                newAtt = Math.Max(1, (int)Math.Floor(Att * mult));
                newDef = Math.Max(1, (int)Math.Floor(Def * mult));
            } while (mult > 0 && ShipDesign.GetHPStr(newAtt, newDef) * hpMult > newHP);

            return ( oldCost - newHP * GetPDCost(newAtt, newDef) ) * Consts.DisbandPct;
        }
        private void GetDisbandGoldValue(int hp, out double actual, out double rounded, out int newAtt, out int newDef)
        {
            actual = GetActualDisbandValue(hp, out newAtt, out newDef);
            rounded = Player.FloorGold(actual);
        }

        private double GetAttackCost(int shipDef)
        {
            return GetAttackCost(this.Att, this.Def, this.HP, shipDef);
        }
        private double GetAttackCost(int att, int def, int hp, int shipDef)
        {
            //only pay for the maximum HP you could possibly use
            return GetPDUpkeep(att, def, Math.Min(hp, ( att - 1 ) * shipDef + 1), Consts.PlanetDefensesAttackCostMult);
        }

        internal double GetPDUpkeep(int att, int def, int hp)
        {
            return GetPDUpkeep(att, def, hp, Consts.PlanetDefensesUpkeepMult);
        }
        private double GetPDUpkeep(int att, int def, int hp, double mult)
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

        internal override void AddExperience(double rawExp, double valueExp)
        {
            valueExp += rawExp * this.PlanetDefenseCostPerHP / this.PlanetDefenseStrengthPerHP;
            this.Soldiers += GetExperienceSoldiers(this.Player, this.Population, this.Soldiers, this.Population, valueExp);
        }

        internal void BuildPlanetDefense(double prodInc)
        {
            if (this.Buildable is PlanetDefense)
            {
                prodInc += this.production;
                this.production = 0;
            }

            double newAtt, newDef, newHP;
            GetPlanetDefenseInc(prodInc, out newAtt, out newDef, out newHP);
            ModPD(this.PlanetDefenseCost + prodInc, newAtt, newDef);
        }
        internal void BuildSoldiersAndDefenses(double prodInc)
        {
            prodInc /= 2.0;
            BuildPlanetDefense(prodInc);
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

        public void GetPlanetDefenseInc(double prod, out double newAtt, out double newDef, out double newHP)
        {
            TurnException.CheckTurn(this.Player);

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

            if (this.HP == 0 || mult < Consts.FLOAT_ERROR_ZERO)
            {
                this.Att = 1;
                this.Def = 1;
                this.HP = 0;
            }
            else if (mult < 1 - Consts.FLOAT_ERROR_ZERO)
            {
                double newAtt, newDef, newHP;
                ModPD(totalCost * mult, this.Att, 1, this.Def, 1, out newAtt, out newDef, out newHP);
                ModPD(totalCost * mult, newAtt, newDef);
            }

            this.Player.AddGold(( totalCost - this.PlanetDefenseCost ) * Consts.DisbandPct);
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
                    double min = 0, max = 1, mult = Game.Random.Range(min, max);
                    do
                    {
                        newAtt = att + mult * ( trgAtt - att );
                        newDef = def + mult * ( trgDef - def );
                        if (inc == TestPD(trgCost, newAtt, newDef))
                            min = mult;
                        else
                            max = mult;
                        mult = ( min + max ) / 2.0;
                    } while (max - min > Consts.FLOAT_ERROR_ZERO);
                }
            }

            newHP = GetPDHP(trgCost, newAtt, newDef);

            //if (att < trgAtt && def > 1 && att == newAtt)
            //    ModPD(trgCost, att, att, def, 1, out newAtt, out newDef, out newHP);
            //else if (def < trgDef && att > 1 && def == newDef)
            //    ModPD(trgCost, att, 1, def, def, out newAtt, out newDef, out newHP);
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
            this.Att = GetPDStat(newAtt, this.Att, this.Player.PDAtt);
            this.Def = GetPDStat(newDef, this.Def, this.Player.PDDef);
            this.HP = GetPDStat(newCost / PlanetDefenseCostPerHP, this.HP, ushort.MaxValue);

            double cost = GetPDCost(Att, Def);
            if (Math.Abs(cost - GetPDCost(Def, Att)) > cost * Consts.FLOAT_ERROR_ZERO)
                throw new Exception();
        }
        private static int GetPDStat(double target, int current, int max)
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
                return current + Game.Random.GaussianCappedInt(add, Consts.PlanetDefenseBuildRndm, lowerCap);
            else
                return Game.Random.Round(target);
        }

        #endregion //planet defense
    }
}
