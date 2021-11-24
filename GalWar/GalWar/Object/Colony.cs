using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private bool _built;
        private sbyte _defenseAttChange, _defenseDefChange;
        private short _defenseHPChange;
        private float _defenseResearch, _soldierChange, _prodGuess, _infrastructureRounding, _researchRounding, _productionRounding, _upgPDTrgStr, _upgPD, _upgSoldiers;

        internal Colony(IEventHandler handler, Player player, Planet planet, int population, double soldiers, double production)
            : base(null, 1, 1, 0, population, soldiers)
        {
            checked
            {
                this.Planet = planet;
                planet.Colony = this;

                this._player = player;

                //set the build intially to StoreProd so it can be changed to anything with no production loss
                StoreProd storeProd = new StoreProd(this);
                this.buildable = new HashSet<Buildable>();
                this.buildable.Add(new BuildGold(this));
                this.buildable.Add(new BuildInfrastructure(this));
                this.buildable.Add(storeProd);
                this.buildable.UnionWith(player.GetDesigns().Select(design => new BuildShip(this, design)));
                this._curBuild = storeProd;

                this._built = (handler == null);

                this._defenseAttChange = 0;
                this._defenseDefChange = 0;
                this._defenseHPChange = 0;

                this._defenseResearch = (float)player.Research;

                this._soldierChange = 0;
                this._prodGuess = 0;

                this._infrastructureRounding = float.NaN;
                this._researchRounding = float.NaN;
                this._productionRounding = float.NaN;

                this._upgPDTrgStr = float.NaN;
                this._upgPD = float.NaN;
                this._upgSoldiers = float.NaN;
            }

            ResetRounding();
            SetUpgFactors();

            double goldAdded;
            int prodAdded;
            if (handler != null)
                ChangeBuild(handler, production, false, 1, out goldAdded, out prodAdded);
            else
                AddProduction(production, false, true, 1, out goldAdded, out prodAdded);
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

                return this.curBuild;
            }
        }
        private Buildable curBuild
        {
            get
            {
                return this._curBuild;
            }
            set
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

        internal int totalProd
        {
            get
            {
                return buildable.Sum(build => build.production);
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

        private double infrastructureRounding
        {
            get
            {
                return this._infrastructureRounding;
            }
            set
            {
                checked
                {
                    this._infrastructureRounding = (float)value;
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
        private double upgPDTrgStr
        {
            get
            {
                return this._upgPDTrgStr;
            }
            set
            {
                checked
                {
                    this._upgPDTrgStr = (float)value;
                }
            }
        }
        private double upgPD
        {
            get
            {
                return this._upgPD;
            }
            set
            {
                checked
                {
                    this._upgPD = (float)value;
                }
            }
        }
        private double upgSoldiers
        {
            get
            {
                return this._upgSoldiers;
            }
            set
            {
                checked
                {
                    this._upgSoldiers = (float)value;
                }
            }
        }

        #endregion //fields and constructors

        #region internal

        private void ChangeBuild(IEventHandler handler, double production, bool floor, double rate, out double goldAdded, out int prodAdded)
        {
            Buildable build = handler.GetNewBuild(this, production, floor);
            ChangeBuild(build);
            AddProduction(production, floor, false, rate, out goldAdded, out prodAdded);
        }
        internal void ChangeBuild(IEventHandler handler)
        {
            Buildable newBuild = handler.GetNewBuild(this, 0, false);
            ChangeBuild(newBuild);
        }
        private void ChangeBuild(Buildable newBuild)
        {
            if (this.curBuild != newBuild)
            {
                AssertException.Assert(CanBuild(newBuild));
                this.curBuild = newBuild;
            }
        }

        internal void NewShipDesign(ShipDesign newDesign, HashSet<ShipDesign> obsoleteDesigns)
        {
            var obsoleteBuilds = this.buildable.OfType<BuildShip>().Where(buildShip => obsoleteDesigns.Contains(buildShip.ShipDesign)).ToList();
            foreach (BuildShip buildShip in obsoleteBuilds)
                this.buildable.Remove(buildShip);
            BuildShip newBuild = new BuildShip(this, newDesign);
            this.buildable.Add(newBuild);

            int production = obsoleteBuilds.Sum(buildShip => buildShip.Production);
            int prodAdded = Game.Random.Round(production * Consts.AutomaticObsoleteRatio);
            newBuild.AddProduction(prodAdded);
            this.Player.AddGold((production - prodAdded) / Consts.ProductionForGold);

            if (obsoleteBuilds.Contains(this.curBuild))
                ChangeBuild(newBuild);
        }
        internal Tuple<BuildShip, int, int, double, Buildable, Buildable> MarkObsolete(IEventHandler handler, ShipDesign obsoleteDesign)
        {
            BuildShip obsoleteBuild = GetBuildShip(obsoleteDesign);
            this.buildable.Remove(obsoleteBuild);

            int oldProduction = obsoleteBuild.Production;
            Buildable oldBuild = this.curBuild;

            int prodAdded;
            double goldAdded;
            double production = oldProduction * Consts.ManualObsoleteRatio;
            if (production * Consts.FLOAT_ERROR_ONE >= 1 || this.curBuild == obsoleteBuild)
            {
                double gold = (oldProduction - production) / Consts.ProductionForGold;
                ChangeBuild(handler, production, true, Consts.ProductionForGold, out goldAdded, out prodAdded);
                goldAdded += gold;
                this.Player.AddGold(gold);
            }
            else
            {
                prodAdded = 0;
                goldAdded = oldProduction / Consts.ProductionForGold;
                this.Player.AddGold(goldAdded);
            }

            return new Tuple<BuildShip, int, int, double, Buildable, Buildable>(obsoleteBuild, oldProduction, prodAdded, goldAdded, this.curBuild, oldBuild);
        }
        internal void UndoMarkObsolete(Tuple<BuildShip, int, int, double, Buildable, Buildable> undoArgs)
        {
            BuildShip obsoleteBuild = undoArgs.Item1;
            int oldProduction = undoArgs.Item2;
            int prodAdded = undoArgs.Item3;
            double goldAdded = undoArgs.Item4;
            Buildable newBuild = undoArgs.Item5;
            Buildable oldBuild = undoArgs.Item6;

            AssertException.Assert(obsoleteBuild.Production == oldProduction);

            this.buildable.Add(obsoleteBuild);
            newBuild.AddProduction(-prodAdded);
            this.Player.AddGold(-goldAdded);

            ChangeBuild(oldBuild);
        }
        private BuildShip GetBuildShip(ShipDesign shipDesign)
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
                if (!(neighbor.SpaceObject is Anomaly))
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
            GetUpgMins(out int PD, out int soldier);
            TurnStuff(out double population, out int production, out double gold, ref research, out int infrastructure);

            ResetMoved();
            ResetRounding();

            //add to guess before adding population, so income is accurate
            double income = GetTotalIncome();
            double avgInfr = Math.Pow(income * Consts.InfrastructureAvg, Consts.InfrastructurePow);
            this.ProdGuess += avgInfr + (income - avgInfr) / 3.0;

            //add population before attacking, so soldierMult is updated
            this.Population += RoundValue(population, false, true, ref gold, Consts.PopulationForGoldHigh);

            //add prodcution to build before applying infrastructure 
            this.curBuild.Build(production);

            //build infrastructure before attacking, so built planet defenses can attack
            ApplyInfrastructure(infrastructure, PD, soldier);

            //attack
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
            List<Ship> builtShips = new List<Ship>();
            foreach (Buildable b in Game.Random.Iterate(this.buildable))
                builtShips.AddRange(b.BuildShips(handler));
            Player.AddGold(gold);

            //additional bookkeeping at the end, so its based on actual results
            DoChange(this.SoldierChange, this.DefenseAttChange, this.DefenseDefChange, this.DefenseHPChange);
            if (builtShips != null && builtShips.Any())
                ProdGuess -= builtShips.Sum(ship => ship.GetCostAvgResearch(true));
            if (ProdGuess < 0)
                ProdGuess = 0;
        }

        private void TurnStuff(out double population, out int production, out double gold, ref int research, out int infrastructure)
        {
            //pay upkeep for stored production before adding production income
            gold = -Upkeep;
            GetTurnValues(out production, out double goldInc, out int researchInc, out infrastructure);
            population = GetPopulationGrowth();
            gold += goldInc;
            research += researchInc;
        }

        public void GetIncomeAvgs(out double gold, out double research, out double production, out double infrastructure)
        {
            TurnException.CheckTurn(this.Player);

            double income = this.GetTotalIncome();
            infrastructure = GetInfrastructureIncome(income);
            income -= infrastructure;

            if (income > 0)
            {
                GetEmphasisPcts(false, out research, out production);
                research *= income;
                production *= income;
            }
            else
            {
                research = production = 0;
            }
            gold = income - research - production;
        }
        public void GetTurnValues(out int production, out double gold, out int research, out int infrastructure)
        {
            TurnException.CheckTurn(this.Player);

            GetTurnValues(this.Population, out production, out gold, out research, out infrastructure);
        }
        private void GetTurnValues(int population, out int production, out double gold, out int research, out int infrastructure)
        {
            GetTurnValues(population, out production, out gold, out research, out infrastructure, true);
        }
        private void GetTurnValues(int population, out int production, out double gold, out int research, out int infrastructure, bool checkNegGold)
        {
            double income = this.GetTotalIncome(population);
            infrastructure = MTRandom.Round(GetInfrastructureIncome(income), this.infrastructureRounding);
            income -= infrastructure;

            if (income > 0)
            {
                GetEmphasisPcts(checkNegGold, out double researchPct, out double productionPct);
                research = MTRandom.Round(researchPct * income, this.researchRounding);
                production = MTRandom.Round(productionPct * income, this.productionRounding);
            }
            else
            {
                research = production = 0;
            }
            gold = income - research - production;

            if (checkNegGold && this.Player.NegativeGold())
            {
                GetTurnValues(population, out _, out double baseGold, out _, out _, false);
                //the forced amount above the EmphasisValue pays a significantly reduced rate
                gold = baseGold + .91 * (gold - baseGold) / Consts.GoldProductionForGold;
            }
        }
        private double GetInfrastructureIncome(double income)
        {
            return Math.Pow(income * this.Planet.infrastructureInc, Consts.InfrastructurePow);
        }
        private void GetEmphasisPcts(bool checkNegGold, out double researchPct, out double productionPct)
        {
            researchPct = this.GetPct(this.Player.ResearchEmphasis, checkNegGold);
            productionPct = this.GetPct(this.Player.ProductionEmphasis, checkNegGold) * this.Planet.prodMult;
            double totalPct = this.GetPct(this.Player.GoldEmphasis, checkNegGold) + researchPct + productionPct;
            researchPct /= totalPct;
            productionPct /= totalPct;
        }

        private double GetPct(bool emphasis, bool checkNegGold)
        {
            double retVal = 1;
            if (emphasis)
            {
                retVal = Consts.EmphasisValue;
                if (checkNegGold && this.Player.NegativeGold())
                    retVal *= this.Player.negativeGoldMult;
            }
            return retVal;
        }

        private void ResetRounding()
        {
            this.infrastructureRounding = Game.Random.NextFloat();
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
            double valueExp = (initAttackers - attackers) + (initPop - this.Population) + reduceQuality;

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
                double pop = Math.Sqrt((initPop + 13.0) / (curPop + 13.0)) * curPop;
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
            return 1.3 / (1.3 + (soldiers / 1.69 + addSoldiers) / pop);
        }

        private void TroopBattle(ref int attackers, double attSoldiers, int gold, out double attack, out double defense)
        {
            defense = Consts.GetInvadeDefenseStrength(this.Population, this.Soldiers);
            attack = Consts.GetInvadeStrength(attackers, attSoldiers, gold, this.Population * defense);

            double attMult = attack / defense;
            double attStr = attMult * attackers;
            if (attStr > this.Population)
            {
                attackers = Game.Random.Round((attStr - this.Population) / attMult);
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
            reduceQuality = Consts.GetPlanetDamage((initAttackers - attackers) + (initPop - this.Population));
            goldSpent = gold;

            int killPlanet = Planet.Quality + 1;
            if (reduceQuality > killPlanet)
            {
                double pct = 1 - (killPlanet / (double)reduceQuality);
                reduceQuality = killPlanet;

                //only pay for the portion of gold spent until the planet is destroyed
                double rounded;
                attackPlayer.AddGold(gold * pct, out rounded);
                goldSpent -= rounded;

                attackers += Game.Random.Round((initAttackers - attackers) * pct);
                this.Population += Game.Random.Round((initPop - this.Population) * pct);
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
            double gold = (this.Population / Consts.PopulationForGoldLow)
                    + (this.Soldiers / Consts.SoldiersForGold)
                    + (this.totalProd / Consts.ProductionForGold)
                    + (this.GetActualDisbandValue(this.HP, out newAtt, out newDef));
            this.Player.AddGold(gold, true);

            Console.WriteLine("Destroy Gold:  " + gold);
            Console.WriteLine("Population:  " + this.Population / Consts.PopulationForGoldLow);
            Console.WriteLine("Soldiers:  " + this.Soldiers / Consts.SoldiersForGold);
            Console.WriteLine("Production:  " + this.totalProd / Consts.ProductionForGold);
            Console.WriteLine("Planet Defense:  " + this.GetActualDisbandValue(this.HP, out newAtt, out newDef));
            Console.WriteLine();

            if ((!this.MinDefenses) ? (newAtt != 1 || newDef != 1) : (newAtt != Math.Max(this.Att - 1, 1) || newDef != Math.Max(this.Def - 1, 1)))
                throw new Exception();

            this.Population = 0;
            this.Soldiers = 0;
            this.buildable.Clear();
            this.curBuild.AddProduction(-curBuild.production);
            this.HP = 0;

            this.Player.RemoveColony(this);
            this.Player.DeathCheck();
        }

        public int GetAddProduction(double production)
        {
            TurnException.CheckTurn(this.Player);

            return GetAddProduction(production, false);
        }
        internal void AddProduction(double production, bool curOnly)
        {
            if (curOnly)
            {
                AddProduction(production, false, true, 1, out _, out _);
            }
            else
            {
                int count = this.buildable.Count - 1, idx = 0;
                double div = Game.Random.GaussianOE(Math.Pow(count, .91), .52, .13, 1);
                foreach (Buildable build in Game.Random.Iterate(this.buildable))
                    if (!(build is BuildGold))
                    {
                        double cur, p1 = production / div, p2 = .65, p3 = .21;
                        if (build is StoreProd)
                            cur = Game.Random.GaussianOE(p1, p2, p3);
                        else
                            cur = Game.Random.GaussianOEInt(p1, p2, p3);
                        if (++idx == count || cur > production)
                            cur = production;
                        AddProduction(build, cur, false, true, 1, out _, out _);
                        production -= cur;
                        div = Math.Max(1, div - Game.Random.Gaussian(1, .39));
                    }
                if (production != 0)
                    ;
            }
        }
        internal void AddProduction(double production, bool floor, bool random, double rate, out double goldAdded, out int prodAdded)
        {
            AddProduction(this.curBuild, production, floor, random, rate, out goldAdded, out prodAdded);
        }
        private void AddProduction(Buildable build, double production, bool floor, bool random, double rate, out double goldAdded, out int prodAdded)
        {
            double add = build.GetAddProduction(production, false);
            goldAdded = (production - add) / Consts.ProductionForGold;
            prodAdded = RoundValue(add, floor, random, ref goldAdded, rate);
            build.AddProduction(prodAdded);
            this.Player.AddGold(goldAdded);
        }
        internal void UndoAddProduction(Buildable buildable, int undo)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(undo <= buildable.Production && undo >= 0);
            AssertException.Assert(this.CanBuild(buildable));

            buildable.AddProduction(-undo);
            ChangeBuild(buildable);
        }
        private int GetAddProduction(double production, bool floor)
        {
            return (int)Math.Round(this.curBuild.GetAddProduction(production, floor) * Consts.FLOAT_ERROR_ONE);
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
                return (this.Planet == null || this.Planet.Colony != this || this.Planet.Dead);
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

        public int TotalProd
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.totalProd;
            }
        }

        public double Upkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.PDUpkeep + GetProdUpkeep() + Consts.GetSoldierUpkeep(this);
            }
        }

        private double GetProdUpkeep()
        {
            return this.buildable.Sum(b => b.Upkeep);
        }

        public bool MinDefenses
        {
            get
            {
                return (this.HP == 0);
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
        public int GetProductionIncome()
        {
            return GetProductionIncome(this.Population);
        }

        public int GetProductionIncome(int population)
        {
            TurnException.CheckTurn(this.Player);

            GetTurnValues(population, out int production, out _, out _, out _);
            return production;
        }

        public int GetInfrastructureIncome(bool? addProd)
        {
            TurnException.CheckTurn(this.Player);

            GetTurnValues(out _, out _, out _, out int infrastructure);
            if (addProd.HasValue ? addProd.Value : this.CurBuild is BuildInfrastructure)
                infrastructure += GetProductionIncome();
            return infrastructure;
        }

        public void GetTurnIncome(ref double population, ref double production, ref double gold, ref int research, ref int infrastructure)
        {
            TurnException.CheckTurn(this.Player);

            TurnStuff(out double popInc, out int prodInt, out double goldInc, ref research, out int infrInc);
            double prodInc = prodInt;
            curBuild.GetTurnIncome(ref prodInc, ref goldInc, ref infrInc);

            //modify parameter values
            population += popInc;
            production += prodInc;
            gold += goldInc;
            infrastructure += infrInc;
        }

        private static int RoundValue(double value, bool floor, bool random, ref double addGold, double rate)
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

            addGold += (value - rounded) / rate;
            return rounded;
        }

        public void TradeProduction(IEventHandler handler, Dictionary<Buildable, int> trade)
        {
            handler = new HandlerWrapper(handler, Player.Game, false, true);
            double addGold, goldIncome;
            AssertException.Assert(GetTradeProduction(trade, out goldIncome));

            TradeProduction(trade);

            if (goldIncome > 0)
                addGold = Player.FloorGold(goldIncome);
            else
                addGold = -Player.CeilGold(-goldIncome);
            goldIncome -= addGold;

            Player.AddGold(addGold);
            Player.GoldIncome(goldIncome);

            Player.Game.PushUndoCommand(new Game.UndoCommand<Dictionary<Buildable, int>, double, double>(
                    new Game.UndoMethod<Dictionary<Buildable, int>, double, double>(UndoTradeProduction), trade, addGold, goldIncome));
        }
        private Tile UndoTradeProduction(Dictionary<Buildable, int> trade, double addGold, double goldIncome)
        {
            trade = trade.ToDictionary(pair => pair.Key, pair => -pair.Value);

            //just for Assert
            double unused;
            GetTradeProduction(trade, out unused);

            TradeProduction(trade);

            Player.AddGold(-addGold);
            Player.GoldIncome(-goldIncome);

            return this.Tile;
        }
        private void TradeProduction(Dictionary<Buildable, int> trade)
        {
            foreach (KeyValuePair<Buildable, int> pair in trade)
                pair.Key.AddProduction(pair.Value);
        }

        public bool GetTradeProduction(Dictionary<Buildable, int> trade, out double gold)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(trade.Count == this.buildable.Count && !trade.Keys.Except(this.buildable).Any() && !this.buildable.Except(trade.Keys).Any());
            foreach (KeyValuePair<Buildable, int> pair in trade)
            {
                AssertException.Assert(CanBuild(pair.Key));
                if (pair.Key is BuildGold)
                    AssertException.Assert(pair.Value == 0);
                AssertException.Assert(-pair.Value <= pair.Key.Production);
            }

            StoreProd storeProd = trade.Keys.OfType<StoreProd>().Single();
            BuildInfrastructure infrastructure = GetInfrastructure();
            Func<Func<KeyValuePair<Buildable, int>, bool>, int> Sum = Check => trade.Where(pair => pair.Key != storeProd && Check(pair)).Sum(pair => pair.Value);

            double sell = -Sum(p => p.Value < 0 && p.Key != infrastructure);
            double sellInfrastructure = -Sum(p => p.Value < 0 && p.Key == infrastructure);
            sell += sellInfrastructure * Consts.SellInfrastructurePenalty;
            double buy = Sum(p => p.Value > 0);
            int tradeStore = trade[storeProd];
            if (tradeStore < 0)
            {
                buy += tradeStore;
                if (buy < 0)
                {
                    sell -= buy;
                    buy = 0;
                }
            }
            else if (tradeStore > 0)
            {
                sell -= tradeStore / Consts.StoreProdRatio / Consts.SwitchBuildRatio;
                if (sell < 0)
                {
                    buy -= sell * Consts.SwitchBuildRatio;
                    sell = 0;
                }
            }

            double sellValue = sell * Consts.SwitchBuildRatio;
            if (buy > sellValue)
            {
                buy -= sellValue;
                gold = -buy * Consts.GoldForProduction;
            }
            else
            {
                sell -= buy / Consts.SwitchBuildRatio;
                gold = sell / Consts.ProductionForGold;
            }

            return (gold > 0 || (Player.HasGold(-gold) && !(tradeStore > 0 && -gold * Consts.FLOAT_ERROR_ONE > Consts.GoldForProduction)));
        }

        public bool CanBuild(Buildable buildable)
        {
            return (buildable != null && this.buildable.Contains(buildable));
        }

        public void StartBuilding(IEventHandler handler, Buildable newBuild)
        {
            handler = new HandlerWrapper(handler, Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(CanBuild(newBuild));

            ChangeBuild(newBuild);
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
            AssertException.Assert(gold || this.curBuild != null);

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
                AddProduction(GetActualDisbandValue(hp, out newAtt, out newDef), true, false, 1, out goldIncome, out production);
                buildable = this.curBuild;

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
            double oldCost = (GetPDHPCost() * this.HP);

            newAtt = this.Att;
            newDef = this.Def;

            int newHP = this.HP - hp;
            double hpMult = this.HP / ShipDesign.GetHPStr(this.Att, this.Def);

            double mult = 1, step = 1 / (this.Att * this.Def * Consts.FLOAT_ERROR_ONE);
            do
            {
                mult -= step;
                newAtt = Math.Max(1, (int)Math.Floor(Att * mult));
                newDef = Math.Max(1, (int)Math.Floor(Def * mult));
            } while (mult > 0 && ShipDesign.GetHPStr(newAtt, newDef) * hpMult > newHP);

            return (oldCost - GetPDHPCost(newAtt, newDef) * newHP) * Consts.DisbandPct;
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
            return GetPDHPUpkeep(att, 1) * Math.Min(hp, (att - 1) * shipDef + 1) * Consts.PlanetDefensesAttackCostMult;
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
            this.Player.SpendGold(cost * (1 - pct));
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

        public int GetInfrastructureProd(bool? addProd)
        {
            TurnException.CheckTurn(this.Player);

            return GetInfrastructure().production + GetInfrastructureIncome(addProd);
        }
        private BuildInfrastructure GetInfrastructure()
        {
            return buildable.OfType<BuildInfrastructure>().Single();
        }

        private void SetUpgFactors()
        {
            this.upgPDTrgStr = this.GetPDHPStrength(Player.PlanetDefenses.Att, Player.PlanetDefenses.Def) * Player.PlanetDefenses.HP;
            this.upgPD = Game.Random.GaussianCapped(1f, .26f);
            this.upgSoldiers = Game.Random.GaussianCapped(1f, .26f);
        }
        public void GetUpgMins(out int PD, out int soldier)
        {
            TurnException.CheckTurn(this.Player);

            BuildInfrastructure infrastructure = GetInfrastructure();

            double pdMult = 2 / (1 + 2 * this.PDStrength / this.upgPDTrgStr);
            pdMult *= (Consts.ResearchFactor + this.Player.Research - this.defenseResearch) / Consts.ResearchFactor;
            pdMult *= pdMult;

            double soldierMult = 1;
            if (Population > 0)
            {
                soldierMult = GetSoldierMult(this.Population, this.Soldiers, infrastructure.production / 2.0 / Consts.ProductionForSoldiers);
                soldierMult *= soldierMult;
            }

            double incomeMult = Math.Sqrt((this.GetTotalIncome() + Consts.PlanetConstValue * Consts.Income) * Consts.AverageQuality * Consts.Income);
            PD = (int)Math.Ceiling(incomeMult / pdMult * this.upgPD + Consts.FLOAT_ERROR_ZERO);
            soldier = (int)Math.Ceiling(incomeMult / soldierMult * this.upgSoldiers + Consts.FLOAT_ERROR_ZERO);
        }

        public void GetInfrastructure(bool? addProd, out Dictionary<Ship, double> repairShips,
                out double att, out double def, out double hp, out double soldiers)
        {
            TurnException.CheckTurn(this.Player);

            GetInfrastructure(GetInfrastructureProd(addProd), false, out repairShips, out att, out def, out hp, out soldiers);
            att -= this.Att;
            def -= this.Def;
            hp -= this.HP;
        }
        private void ApplyInfrastructure(int infrastructure, int PD, int soldier)
        {
            GetInfrastructure(infrastructure, true, out _, out _, out _, out _, out _, PD, soldier);
        }
        private void GetInfrastructure(int infrastructure, bool apply, out Dictionary<Ship, double> repairShips,
             out double att, out double def, out double hp, out double soldiers, int PD = -1, int soldier = -1)
        {
            repairShips = new Dictionary<Ship, double>();
            att = this.Att;
            def = this.Def;
            hp = this.HP;
            soldiers = 0;

            BuildInfrastructure build = GetInfrastructure();
            double prod = infrastructure;
            if (apply)
            {
                build.AddProduction(infrastructure);
                prod = build.production;
            }
            double avg = prod / 2.0;
            if (apply)
                avg = Game.Random.GaussianCapped(avg, Consts.PlanetDefenseRndm);

            List<Ship> repair = Tile.GetNeighbors(this.Tile)
                    .Select(t => t.SpaceObject as Ship)
                    .Where(s => s != null && s.Player.IsTurn && s.GetRepairedFrom() == this)
                    .OrderBy(s => s.GetProdForHP(s.MaxHP - s.HP))
                    .ToList();
            double count = repair.Count;
            if (count > 0)
            {
                prod -= avg;
                foreach (Ship ship in repair)
                {
                    double useProd = avg / count--;
                    if (apply)
                        useProd = Game.Random.GaussianCapped(useProd, Consts.PlanetDefenseRndm);
                    double repairHP = ship.ProductionRepair(ref avg, useProd, apply);
                    repairShips.Add(ship, repairHP);
                    if (apply)
                        this.ProdGuess -= ship.GetCostAvgResearch(true) * Consts.RepairCostMult * repairHP / (double)ship.MaxHP;
                }
                prod += avg;
                if (apply)
                    SetBuildProd(prod, false);
            }
            else
            {
                if (PD == -1 || soldier == -1)
                    GetUpgMins(out PD, out soldier);
                if (prod * Consts.FLOAT_ERROR_ONE > PD)
                    BuildPlanetDefense(0, avg, apply, out att, out def, out hp);
                else if (prod * Consts.FLOAT_ERROR_ONE > soldier)
                    BuildSoldiers(0, avg, apply, out soldiers);
            }
        }

        internal void BuildSoldiersAndDefenses(double prodInc)
        {
            prodInc /= 2.0;
            BuildPlanetDefense(prodInc);
            BuildSoldiers(prodInc);
        }

        internal void BuildSoldiers(double prod)
        {
            BuildSoldiers(prod, prod);
        }
        internal void BuildSoldiers(double addProd, double avg)
        {
            BuildSoldiers(addProd, avg, true, out _);
        }
        internal void BuildSoldiers(double addProd, double avg, bool apply, out double soldiers)
        {
            if (apply)
                addProd += GetInfrastructure().production;

            soldiers = 0;
            if (this.Population > 0)
            {
                if (apply)
                    avg = Game.Random.GaussianCapped(avg, Consts.PlanetDefenseRndm);
                soldiers = avg / Consts.ProductionForSoldiers;
                if (apply)
                {
                    addProd -= avg;
                    this.Soldiers += soldiers;
                    this.ProdGuess -= avg;
                }
            }

            if (apply)
                SetBuildProd(addProd, true);
        }
        private void SetBuildProd(double production, bool reset)
        {
            double gold = 0;
            int prod = RoundValue(production, false, true, ref gold, 1);
            if (prod < 0)
            {
                gold += prod;
                prod = 0;
            }
            BuildInfrastructure infrastructure = GetInfrastructure();
            infrastructure.AddProduction(prod - infrastructure.production);
            this.Player.GoldIncome(gold);

            if (reset)
                SetUpgFactors();
        }

        internal void BuildPlanetDefense(double prod)
        {
            BuildPlanetDefense(prod, prod);
        }
        internal void BuildPlanetDefense(double addProd, double avg)
        {
            BuildPlanetDefense(addProd, avg, true, out _, out _, out _);
        }
        internal void BuildPlanetDefense(double addProd, double avg, bool apply, out double newAtt, out double newDef, out double newHP)
        {
            avg += this.PDCost;
            double trgAtt = this.Player.PlanetDefenses.Att, trgDef = this.Player.PlanetDefenses.Def;
            ModPD(avg, this.Player.Research, this.Att, trgAtt, this.Def, trgDef, out newAtt, out newDef, out newHP);

            if (apply)
            {
                double trgCost = ShipDesign.GetPlanetDefenseCost(newAtt, newDef, this.Player.Research) * newHP;
                SetPD(trgCost, this.Player.Research, newAtt, newDef, out int att, out int def, out int hp);

                addProd += GetInfrastructure().production
                        + this.PDCost - ShipDesign.GetPlanetDefenseCost(att, def, this.Player.Research) * hp;

                double pubCost = GetPDHPCostAvgResearch() * this.HP;
                this.Att = att;
                this.Def = def;
                this.HP = hp;
                this.defenseResearch = this.Player.Research;
                this.ProdGuess -= (GetPDHPCostAvgResearch() * this.HP - pubCost);

                SetBuildProd(addProd, true);
            }
        }

        private void ReduceDefenses(double mult)
        {
            double totalCost = GetPDHPCost() * this.HP;

            if (!this.MinDefenses && (mult > Consts.FLOAT_ERROR_ZERO && mult < 1 / Consts.FLOAT_ERROR_ONE))
            {
                double newAtt, newDef;
                ModPD(totalCost * mult, this.defenseResearch, this.Att, 1, this.Def, 1, out newAtt, out newDef, out _);
                SetPD(totalCost * mult, this.defenseResearch, newAtt, newDef);
            }
            else
                ;
            if (this.MinDefenses)
            {
                this.Att = 1;
                this.Def = 1;
                this.HP = 0;
            }

            this.Player.AddGold((totalCost - (GetPDHPCost() * this.HP)) * Consts.DisbandPct);
        }

        private static void ModPD(double trgCost, double trgResearch, int att, double trgAtt, int def, double trgDef,
                out double newAtt, out double newDef, out double newHP)
        {
            bool inc = (trgAtt > att || trgDef > def);
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
                        newAtt = att + mult * (trgAtt - att);
                        newDef = def + mult * (trgDef - def);
                        if (inc == TestPD(trgCost, trgResearch, newAtt, newDef))
                            min = mult;
                        else
                            max = mult;
                        mult = (min + max) / 2.0;
                    } while (max - min > Consts.FLOAT_ERROR_ZERO);
                }
            }

            newHP = GetPDHP(trgCost, trgResearch, newAtt, newDef);
        }
        private static bool TestPD(double trgCost, double trgResearch, double s1, double s2)
        {
            return (GetPDHP(trgCost, trgResearch, s1, s2) > ShipDesign.GetHPStr(s1, s2));
        }
        private static double GetPDHP(double trgCost, double trgResearch, double s1, double s2)
        {
            return trgCost / ShipDesign.GetPlanetDefenseCost(s1, s2, trgResearch);
        }

        private void SetPD(double newCost, double newResearch, double newAtt, double newDef)
        {
            int att, def, hp;
            SetPD(newCost, newResearch, newAtt, newDef, out att, out def, out hp);
            this.Att = att;
            this.Def = def;
            this.HP = hp;
        }
        private void SetPD(double newCost, double newResearch, double newAtt, double newDef, out int att, out int def, out int hp)
        {
            att = SetPDStat(newAtt, this.Att, this.Player.PlanetDefenses.Att);
            def = SetPDStat(newDef, this.Def, this.Player.PlanetDefenses.Def);
            double avg = GetPDHP(newCost, newResearch, att, def);
            if (avg > 1)
                hp = Game.Random.GaussianCappedInt(avg, Consts.PlanetDefenseRndm, 1);
            else
                hp = Game.Random.Round(avg);
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
