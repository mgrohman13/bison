using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class Ship : Combatant, ISpaceObject
    {
        #region fields and constructors

        public readonly bool Colony;
        private readonly Player player;

        private Tile _tile;

        private float bombardDamageMult;

        private byte _name, _mark;

        private bool _hasRepaired;
        private byte _expType, _upkeep, _curSpeed, _maxSpeed;
        private ushort _maxTrans;
        private ushort _maxHP;
        private float _curExp, _totalExp, _needExpMult, _expDiv;
        private double _cost;

        internal Ship(Player player, Tile tile, ShipDesign design, IEventHandler handler)
        {
            this.Colony = design.Colony;

            this.player = player;

            this.tile = tile;
            tile.SpaceObject = this;

            this._name = design._name;
            this._mark = design._mark;
            checked
            {
                this.MaxSpeed = (byte)design.Speed;
                this.maxPop = (ushort)design.Trans;
            }
            this.bombardDamageMult = design.BombardDamageMult;

            this.HasRepaired = false;
            this.Upkeep = design.Upkeep;
            this.CurSpeed = this.MaxSpeed;
            this.MaxHP = design.HP;

            this.Att = design.Att;
            this.Def = design.Def;

            this.HP = this.MaxHP;

            this.cost = design.AdjustCost(player.Game.MapSize);

            this.curExp = 0;
            this.totalExp = 0;
            this._expDiv = (float)GetValue();
            GetNextLevel(handler);
        }

        public double GetStrength()
        {
            return ShipDesign.GetStrength(this.Att, this.Def, this.MaxHP, this.MaxSpeed);
        }

        private double GetValue()
        {
            return GetValue(null);
        }

        private double GetValue(ExpType? expType)
        {
            int att = this.Att, def = this.Def, hp = this.MaxHP, speed = this.MaxSpeed, trans = this.MaxPop;
            float ds = this.bombardDamageMult;
            if (expType.HasValue)
                switch (expType.Value)
                {
                case ExpType.Att:
                    ++att;
                    break;
                case ExpType.Def:
                    ++def;
                    break;
                case ExpType.HP:
                    ++hp;
                    break;
                case ExpType.DS:
                    ds += this.needExpMult / Consts.BombardAttackMult;
                    break;
                case ExpType.Speed:
                    ++speed;
                    break;
                case ExpType.Trans:
                    ++trans;
                    break;
                default:
                    throw new Exception();
                }
            return ShipDesign.GetValue(att, def, hp, speed, trans, this.Colony, ds, this.Player.Game.ExpResearch);
        }

        private double GetCostLastResearched()
        {
            return ShipDesign.GetTotCost(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.bombardDamageMult, this.Player.LastResearched);
        }

        private Tile tile
        {
            set
            {
                this._tile = value;
            }
        }

        private double RepairCost
        {
            get
            {
                return ( Consts.RepairCostMult * this.cost / this.MaxHP );
            }
        }

        private double cost
        {
            get
            {
                return this._cost;
            }
            set
            {
                this._cost = value;
            }
        }

        private float curExp
        {
            get
            {
                return this._curExp;
            }
            set
            {
                this._curExp = value;
            }
        }
        private float needExpMult
        {
            get
            {
                return this._needExpMult;
            }
            set
            {
                this._needExpMult = value;
            }
        }
        private double totalExp
        {
            get
            {
                return this._totalExp;
            }
            set
            {
                this._totalExp = (float)value;
            }
        }

        #endregion //fields and constructors

        #region internal

        internal double EndTurn()
        {
            //must be calculated before speed is restored
            double upkeep = ( this.Upkeep - this.GetUpkeepReturn() );

            ResetMoved();

            this.CurSpeed = MaxSpeed;
            this.HasRepaired = false;

            return upkeep;
        }

        public double GetUpkeepReturn()
        {
            TurnException.CheckTurn(this.Player);

            return GetUpkeepReturn(this.CurSpeed);
        }

        private double GetUpkeepReturn(double speedLeft)
        {
            return speedLeft / (double)MaxSpeed * Consts.UpkeepUnmovedReturn * this.Upkeep;
        }

        internal void ProductionRepair(ref double production, ref double gold, bool doRepair, bool minGold)
        {
            double hp = GetHPForProd(production);
            if (hp > this.MaxHP - HP)
                hp = this.MaxHP - HP;

            if (doRepair)
            {
                int repairHP = Game.Random.Round(hp);
                this.HP += repairHP;
                hp = repairHP;
            }
            else if (minGold)
            {
                hp = Math.Ceiling(hp);
            }
            else
            {
                int low = (int)hp;
                if (hp != low)
                {
                    gold += ( hp - low ) * ( production - GetProdForHP(low + 1) );
                    production = ( low + 1 - hp ) * ( production - GetProdForHP(low) );
                    return;
                }
            }

            production -= GetProdForHP(hp);
            if (production < 0)
            {
                gold += production;
                production = 0;
            }
        }

        internal void Destroy(bool addGold)
        {
            if (this.Dead)
                throw new Exception();

            double destroyGold = GetDestroyGold();
            if (destroyGold > 0)
                if (addGold)
                    this.Player.AddGold(destroyGold);
                else
                    this.Player.GoldIncome(destroyGold);

            this.Player.RemoveShip(this);
            this.Tile.SpaceObject = null;
        }

        #endregion //internal

        #region public

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
                return this._tile;
            }
        }
        public int MaxSpeed
        {
            get
            {
                return this._maxSpeed;
            }
            private set
            {
                checked
                {
                    this._maxSpeed = (byte)value;
                }
            }
        }

        public override int MaxPop
        {
            get
            {
                return this._maxTrans;
            }
        }
        private int maxPop
        {
            get
            {
                return this.MaxPop;
            }
            set
            {
                checked
                {
                    this._maxTrans = (ushort)value;
                }
            }
        }

        public double BombardDamage
        {
            get
            {
                return ShipDesign.GetBombardDamage(this.Att, this.bombardDamageMult);
            }
        }

        public bool DeathStar
        {
            get
            {
                return ( this.bombardDamageMult > 1 );
            }
        }

        public ExpType NextExpType
        {
            get
            {
                return (ExpType)this._expType;
            }
            private set
            {
                this._expType = (byte)value;
            }
        }

        public int Upkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this._upkeep;
            }
            private set
            {
                checked
                {
                    this._upkeep = (byte)value;
                }
            }
        }

        public int CurSpeed
        {
            get
            {
                return this._curSpeed;
            }
            private set
            {
                checked
                {
                    this._curSpeed = (byte)value;
                }
            }
        }

        public int MaxHP
        {
            get
            {
                return this._maxHP;
            }
            private set
            {
                checked
                {
                    this._maxHP = (ushort)value;
                }
            }
        }


        public bool Dead
        {
            get
            {
                return ( this.Tile.SpaceObject != this );
            }
        }

        public bool HasRepaired
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this._hasRepaired;
            }
            private set
            {
                this._hasRepaired = value;
            }
        }

        public double DisbandValue
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return ShipDesign.GetDisbandValue(this.cost, this.HP, this.MaxHP);
            }
        }

        public double ColonizationValue
        {
            get
            {
                return GetColonizationValue(0);
            }
        }

        protected override void SetHP(int value)
        {
            if (this.Population > 0 && value < this.HP)
            {
                int losePopulation;
                if (value > 0)
                    //if hp is lost for any reason, some transported population is killed off
                    losePopulation = Game.Random.GaussianCappedInt(GetTransLoss(this.HP - value), Consts.TransLossRndm);
                else
                    losePopulation = this.Population;
                LosePopulation(losePopulation);
            }
            base.SetHP(value);
        }

        private double GetTransLoss(int damage)
        {
            if (damage >= this.HP)
                return this.Population;
            return ( damage / (double)this.MaxHP * this.MaxPop * Math.Pow(this.Population / (double)this.MaxPop, Consts.TransLossPctPower) * Consts.TransLossMult );
        }

        public double GetColonizationValue(int repair)
        {
            TurnException.CheckTurn(this.Player);

            return ShipDesign.GetColonizationValue(this.MaxSpeed, this.cost, this.HP + repair, this.MaxHP);
        }

        public int GetTotalExp()
        {
            return GetExp(this.curExp);
        }
        private int GetExp(float exp)
        {
            return (int)Math.Round(100.0 * ( this.totalExp + exp ) / this._expDiv);
        }

        public double GetDestroyGold()
        {
            double gold = this.Population / Consts.PopulationForGold + this.soldiers / Consts.SoldiersForGold + GetCostExperience(this.curExp) / Consts.ExpForGold;
            if (this.Player.IsTurn)
                gold += GetUpkeepReturn();
            return gold;
        }

        public void Disband(Colony colony)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(colony == null || colony.Player == this.Player);

            bool gold = ( colony == null );

            if (gold)
                this.Player.AddGold(DisbandValue);
            else
                colony.AddProduction(DisbandValue);

            Destroy(gold);
        }

        public void Move(Tile tile)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(this.CurSpeed > 0);
            AssertException.Assert(CheckZOC(this.Player, this.Tile, tile));
            AssertException.Assert(tile.SpaceObject == null);

            --this.CurSpeed;

            this.Tile.SpaceObject = null;
            this.tile = tile;
            this.Tile.SpaceObject = this;
        }

        public static bool CheckZOC(Player player, Tile from, Tile to)
        {
            AssertException.Assert(player != null);
            AssertException.Assert(from != null);
            AssertException.Assert(to != null);

            if (!Tile.IsNeighbor(from, to))
                return false;

            //check ZOC: cannot move from and to tiles that are both adjacent to the same enemy ship
            HashSet<Tile> neighbors = null;
            foreach (Tile neighbor in Tile.GetNeighbors(to))
            {
                Ship ship = neighbor.SpaceObject as Ship;
                if (!( ship == null || ship.Player == player || !( neighbors == null ? neighbors = Tile.GetNeighbors(from) : neighbors ).Contains(neighbor) ))
                    return false;
            }

            return true;
        }

        public void AttackShip(Ship ship, IEventHandler handler)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(ship != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, ship.Tile));
            AssertException.Assert(this.Player != ship.Player);
            AssertException.Assert(this.CurSpeed > 0);
            handler = new HandlerWrapper(handler);

            --this.CurSpeed;

            double pct = Combat(ship, handler);
            this.Player.GoldIncome(this.GetUpkeepReturn(pct));

            //only the ship whose turn it is can immediately gain levels from the exp
            this.LevelUp(handler);
        }

        protected override double GetExpForDamage(double damage)
        {
            return damage / (double)this.MaxHP * this.GetStrength() * Consts.ExperienceMult;
        }

        protected override double GetKillExp()
        {
            return this.GetExpForDamage(this.MaxHP) * Consts.ExperienceDestroyMult;
        }

        protected override void AddExperience(double experience)
        {
            this.curExp += Game.Random.GaussianCapped((float)experience, Consts.ExperienceRndm);
        }

        private void AddCostExperience(double experience)
        {
            AddExperience(experience / GetCostExpForExp());
        }

        private double GetCostExperience(double experience)
        {
            return experience * GetCostExpForExp();
        }

        private double GetCostExpForExp()
        {
            return GetCostLastResearched() / GetValue();
        }

        internal void LevelUp(IEventHandler handler)
        {
            float needExp;
            if (this.HP > 0 && !this.Dead && this.curExp > ( needExp = (float)( this.needExpMult * ( GetValue(this.NextExpType) - GetValue() ) ) ))
            {
                double costInc = this.GetCostLastResearched();
                this.cost += this.Upkeep * Consts.ExperienceUpkeepPayoffMult
                        * Consts.GetUpkeepPayoff(this.Tile.Game.MapSize, this.Colony, this.MaxPop, this.MaxSpeed);

                double pct = this.HP / (double)this.MaxHP;
                switch (this.NextExpType)
                {
                case ExpType.Att:
                    ++this.Att;
                    break;
                case ExpType.Def:
                    ++this.Def;
                    break;
                case ExpType.HP:
                    int inc = Game.Random.Round((float)pct);
                    pct = inc;
                    this.HP += inc;
                    ++this.MaxHP;
                    break;
                case ExpType.DS:
                    this.bombardDamageMult += this.needExpMult / Consts.BombardAttackMult;
                    break;
                case ExpType.Speed:
                    ++this.MaxSpeed;
                    break;
                case ExpType.Trans:
                    ++this.maxPop;
                    break;
                default:
                    throw new Exception();
                }

                handler.OnLevel(this, this.NextExpType, pct, GetExp(needExp), GetExp(0));

                //increase the repair cost and upkeep of the ship as it levels
                costInc = this.GetCostLastResearched() - costInc;

                //add gold for level randomness and percent of ship injured 
                Player.GoldIncome(( this.needExpMult - pct ) * costInc / Consts.ExpForGold);

                double upkeepPayoff = Consts.GetUpkeepPayoff(this.Tile.Game.MapSize, this.Colony, this.MaxPop, this.MaxSpeed);
                double minCost = upkeepPayoff * Consts.MinCostMult;

                //upkeep payoff is higher when leveling up since production cost is now only used for repairs
                upkeepPayoff *= Consts.ExperienceUpkeepPayoffMult;
                this.cost -= this.Upkeep * upkeepPayoff;

                if (!this.Colony)
                {
                    int upkeepInc = Game.Random.Round(costInc * this.Upkeep / ( this.cost + this.Upkeep * upkeepPayoff ) / Consts.ExperienceUpkeepPayoffMult);

                    //make sure we wont go below the minimum cost
                    while (this.Upkeep + upkeepInc > 1 && this.cost + costInc - upkeepInc * upkeepPayoff < minCost)
                        --upkeepInc;
                    costInc -= upkeepInc * upkeepPayoff;

                    this.Upkeep += upkeepInc;
                    if (this.Upkeep < 1)
                        throw new Exception();
                }
                this.cost += costInc;

                this.totalExp += needExp;
                this.curExp -= needExp;
                GetNextLevel(handler);

                if (this.cost < minCost)
                {
                }
            }
        }

        private void GetNextLevel(IEventHandler handler)
        {
            this.needExpMult = Game.Random.GaussianCapped(1f, Consts.ExperienceRndm, (float)Consts.FLOAT_ERROR);

            //randomly select a stat to increase next based on the current ratios
            Dictionary<ExpType, int> stats = new Dictionary<ExpType, int>();

            int att = GetStatExpChance(Att, Def);
            int total = att;
            stats.Add(ExpType.Att, att);
            int def = GetStatExpChance(Def, Att);
            total += def;
            stats.Add(ExpType.Def, def);
            int hp = this.MaxHP;
            total += hp;
            stats.Add(ExpType.HP, hp);

            if (this.DeathStar)
            {
                int ds = Game.Random.Round(Math.Sqrt(total * this.bombardDamageMult) / 39.0 / this.needExpMult);
                total += ds;
                stats.Add(ExpType.DS, ds);
            }
            else
            {
                int trans = Game.Random.Round(( this.MaxPop + ( this.Colony ? 26 : 0 ) ) / 3.0);
                total += trans;
                stats.Add(ExpType.Trans, trans);
            }
            stats.Add(ExpType.Speed, Game.Random.Round(Math.Sqrt(total * this.MaxSpeed) / 16.9));

            this.NextExpType = Game.Random.SelectValue<ExpType>(stats);

            LevelUp(handler);
        }

        private int GetStatExpChance(int stat, int other)
        {
            double hpStr = ShipDesign.GetHPStr(stat, other);
            hpStr /= ShipDesign.GetHPStr(stat + 1, other) - hpStr;
            hpStr *= stat / (double)( stat + other );
            return Game.Random.Round(hpStr);
        }

        public void AttackColony(Colony colony, IEventHandler handler)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(colony != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, colony.Tile));
            AssertException.Assert(this.Player != colony.Player);
            AssertException.Assert(this.CurSpeed > 0);
            handler = new HandlerWrapper(handler);

            double pct;
            int oldHP = colony.HP;
            if (oldHP > 0)
            {
                double damage, d;
                Consts.GetDamageTable(this.Att, colony.Def, out damage, out d);

                double freeDmg = this.GetFreeDmg(colony);
                int bombardDamage = GetBombardDamage(freeDmg);
                colony.HP -= bombardDamage;

                if (colony.HP > 0 && ( !this.DeathStar || handler.ConfirmCombat(this, colony) ))
                    pct = Combat(colony, handler);
                else
                    pct = 1;

                pct *= damage / ( damage + freeDmg );
                if (bombardDamage > oldHP)
                    pct += ( 1 - ( oldHP / (double)bombardDamage ) ) * freeDmg / ( damage + freeDmg );
            }
            else
            {
                pct = 1;
            }

            if (pct > 0)
                if (colony.HP > 0)
                    this.Player.GoldIncome(GetUpkeepReturn(pct));
                else
                    Bombard(colony.Planet, false, pct, handler);

            --this.CurSpeed;
            LevelUp(handler);
        }

        internal double GetFreeDmg(Colony colony)
        {
            return this.BombardDamage * Consts.PlanetDefensesDeathStarMult / colony.PlanetDefenseCost;
        }

        public void Bombard(Planet planet, IEventHandler handler)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(planet != null);
            bool friendly = ( planet.Colony != null && this.Player == planet.Colony.Player );
            AssertException.Assert(friendly ? this.DeathStar : planet.Colony == null || planet.Colony.HP == 0);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, planet.Tile));
            AssertException.Assert(this.CurSpeed > 0);
            handler = new HandlerWrapper(handler);

            Bombard(planet, friendly, 1, handler);

            --this.CurSpeed;
            LevelUp(handler);
        }

        private void Bombard(Planet planet, bool friendly, double pct, IEventHandler handler)
        {
            int colonyDamage = GetColonyDamage(friendly, pct);
            int planetDamage = GetPlanetDamage(colonyDamage, pct);

            //bombard the planet first, since it might get destroyed
            int initQuality = BombardPlanet(planet, planetDamage, handler);
            //bombard the colony second, if it exists
            double initPop = BombardColony(planet.Colony, colonyDamage, handler);

            double popDmg;
            if (friendly)
            {
                //when bombarding a friendly colony, you actually get overkill gold for population you do kill
                popDmg = this.BombardDamage;
                initPop = popDmg - Math.Min(colonyDamage, initPop);
            }
            else
            {
                popDmg = colonyDamage;
            }
            BombardOverkill(planetDamage, initQuality, popDmg, initPop, pct);
        }

        private int GetColonyDamage(bool friendly, double pct)
        {
            double damage = this.BombardDamage * pct;
            if (friendly)
                damage *= Consts.DeathStarFriendlyPopDamageMult;
            return GetBombardDamage(damage);
        }

        private int GetBombardDamage(double damage)
        {
            if (this.DeathStar)
                return GetDeathStarDamage(damage);
            else
                return Game.Random.OEInt(damage);
        }

        private int GetPlanetDamage(int colonyDamage, double pct)
        {
            if (this.DeathStar)
                //death stars do quality and population damage independently
                return GetDeathStarDamage(( Consts.DeathStarPlanetDamageMult * ( this.bombardDamageMult - 1 ) + 1 ) / this.bombardDamageMult
                       * Consts.PlanetDamageAvg * this.BombardDamage * pct);
            else
                //regular ships do standard quality damage as a function of killed population
                return Consts.GetPlanetDamage(colonyDamage);
        }

        private int GetDeathStarDamage(double damage)
        {
            return Game.Random.GaussianCappedInt(damage, Consts.DeathStarDamageRndm);
        }

        private int BombardPlanet(Planet planet, int planetDamage, IEventHandler handler)
        {
            //quality has to drop to -1 to destroy planet
            int initQuality = planet.Quality + 1;

            if (planetDamage > 0)
            {
                planet.ReduceQuality(planetDamage);

                double exp = Math.Min(initQuality, planetDamage) * Consts.TroopExperienceMult;
                this.Player.GoldIncome(-exp);
                AddCostExperience(exp);
            }

            return initQuality;
        }

        private int BombardColony(Colony colony, int colonyDamage, IEventHandler handler)
        {
            int initPop = 0;
            if (colony != null)
            {
                initPop = colony.Population;

                if (colonyDamage > 0)
                    AddCostExperience(colony.Bombard(colonyDamage));
            }

            return initPop;
        }

        private void BombardOverkill(double planetDamage, double quality, double colonyDamage, double pop, double pct)
        {
            //the 1 movement is split evenly between planet and colony damage
            double move = 0;
            //planet quality overkill
            if (planetDamage > quality)
                move += ( 1 - ( quality / planetDamage ) );
            //colony population overkill
            if (colonyDamage > pop)
                move += ( 1 - ( pop / colonyDamage ) );
            if (move > 0)
                this.Player.GoldIncome(GetUpkeepReturn(move / 2 * pct));
        }

        public void Invade(Colony destination, int invadeGold, int population, ref int extraPop, IEventHandler handler)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(destination != null);
            AssertException.Assert(population > 0);
            AssertException.Assert(population <= this.AvailablePop);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, destination.Tile));
            AssertException.Assert(this.Player != destination.Player);
            handler = new HandlerWrapper(handler);

            double goldCost = GetGoldCost(population);
            AssertException.Assert(invadeGold >= goldCost);
            double gold = 0;
            if (destination.Population > 0)
            {
                //all attackers cost gold to move regardless of where they end up
                gold = invadeGold - goldCost;
                goldCost = invadeGold;
            }
            AssertException.Assert(goldCost < Player.Gold);

            this.Player.SpendGold(goldCost);

            double soldiers = GetSoldiers(population, this.soldiers);
            this.soldiers -= soldiers;

            //all attackers cannot be moved again regardless of where they end up
            this.Population -= population;
            population = destination.Invasion(this.Player, population, ref soldiers, gold, handler, ref extraPop);
            this.Population += population;
            this.movedPop += population;

            this.soldiers += soldiers;
        }

        public void Colonize(Planet planet, IEventHandler handler)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(planet != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, planet.Tile));
            AssertException.Assert(this.Colony);
            AssertException.Assert(planet.Colony == null);
            AssertException.Assert(this.AvailablePop == this.Population);
            AssertException.Assert(Population > 0);
            double gold = GetGoldCost(Population) + planet.ColonizationCost;
            AssertException.Assert(gold < Player.Gold);
            handler = new HandlerWrapper(handler);

            this.Player.SpendGold(gold);

            int production = Game.Random.Round(ColonizationValue);
            this.Player.NewColony(planet, this.Population, this.soldiers, production, handler);
            this.Player.GoldIncome(ColonizationValue - production);

            this.Population = 0;
            this.soldiers = 0;

            Destroy(false);
        }

        public void GoldRepair(int hp)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(hp <= this.MaxHP - HP);
            AssertException.Assert(!this.HasRepaired);
            double spend = GetGoldForHP(hp);
            AssertException.Assert(spend < Player.Gold);

            this.HP += hp;
            Player.SpendGold(spend);
            this.HasRepaired = true;
        }

        public double GetGoldForHP(int hp)
        {
            TurnException.CheckTurn(this.Player);

            return hp * RepairCost * Math.Pow(Consts.RepairGoldIncPowBase, hp / (double)this.MaxHP / Consts.RepairGoldHPPct);
        }

        public double GetProdForHP(double hp)
        {
            TurnException.CheckTurn(this.Player);

            return hp * RepairCost;
        }

        public double GetHPForProd(double production)
        {
            TurnException.CheckTurn(this.Player);

            return production / RepairCost;
        }

        public override string ToString()
        {
            return ShipNames.GetName(this._name, this._mark);
        }

        #endregion //public

        [Serializable]
        public enum ExpType : byte
        {
            HP,
            Att,
            Def,
            DS,
            Speed,
            Trans,
        }
    }
}
