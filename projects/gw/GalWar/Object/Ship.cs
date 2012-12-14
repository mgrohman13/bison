using System;
using System.Collections.Generic;
using MattUtil;

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
        private ushort _maxTrans, _maxHP, _repair;
        private float _curExp, _totalExp, _needExpMult, _expDiv, _autoRepair;
        private double _cost;

        internal Ship(IEventHandler handler, Player player, Tile tile, ShipDesign design)
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

            this.AutoRepair = double.NaN;

            this.cost = design.AdjustCost(this.Player.Game.MapSize);

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
                    ds += GetBombardLevelInc();
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
            return ShipDesign.GetValue(att, def, hp, speed, trans, this.Colony, ds, this.Player.Game.AvgResearch);
        }

        internal double GetCostLastResearched()
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

        public double AutoRepair
        {
            get
            {
                if (this._autoRepair <= 0)
                    this.AutoRepair = 0;
                else if (this.HP == this.MaxHP)
                    this.AutoRepair = float.NaN;
                else if (this._autoRepair > 0)
                    this.AutoRepair = this.GetAutoRepairForHP(this.GetAutoRepairHP(this._autoRepair));

                return this._autoRepair;
            }
            set
            {
                this._autoRepair = (float)value;
            }
        }

        public bool DoAutoRepair
        {
            get
            {
                return ( !HasRepaired && AutoRepair > 0 );
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

        internal int ProductionRepair(ref double production, ref double gold, bool doRepair, bool minGold)
        {
            int retVal = 0;

            double hp = GetHPForProd(production);
            double max = this.MaxHP - HP;
            if (!doRepair && DoAutoRepair)
                max -= GetAutoRepairHP();
            if (hp > max)
                hp = max;

            if (doRepair)
            {
                int repairHP = Game.Random.Round(hp);
                this.HP += repairHP;
                hp = repairHP;

                this.Repair += repairHP;
                retVal = repairHP;
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
                    return retVal;
                }
            }

            production -= GetProdForHP(hp);
            if (production < 0)
            {
                gold += production;
                production = 0;
            }

            return retVal;
        }

        internal void Destroy(bool addGold)
        {
            if (this.Dead)
                throw new Exception();

            double destroyGold = GetDestroyGold();
            if (addGold)
                this.Player.AddGold(destroyGold);
            else
                this.Player.GoldIncome(destroyGold);

            this.Population = 0;
            this.soldiers = 0;
            this.curExp = 0;
            this.CurSpeed = 0;

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

        public double GetCurrentCost()
        {
            TurnException.CheckTurn(this.Player);

            return GetCostLastResearched();
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
            return ( damage / (double)this.MaxHP * this.MaxPop * Consts.TransLossMult
                    * Math.Pow(this.Population / (double)this.MaxPop, Consts.TransLossPctPower) );
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
            return (int)Math.Round(104 * ( this.totalExp + exp ) / this._expDiv);
        }

        public double GetDestroyGold()
        {
            double gold = this.Population / Consts.PopulationForGold + this.soldiers / Consts.SoldiersForGold
                    + GetCostExperience(this.curExp) / Consts.ExpForGold;
            if (this.Player.IsTurn)
                gold += GetUpkeepReturn();
            Console.WriteLine("Destroy Gold:  " + gold);
            Console.WriteLine("Population:  " + this.Population / Consts.PopulationForGold);
            Console.WriteLine("Soldiers:  " + this.soldiers / Consts.SoldiersForGold);
            Console.WriteLine("Experience:  " + GetCostExperience(this.curExp) / Consts.ExpForGold);
            Console.WriteLine("Upkeep:  " + ( this.Player.IsTurn ? GetUpkeepReturn() : 0 ));
            Console.WriteLine();
            return gold;
        }

        public void Disband(IEventHandler handler, Colony colony)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(colony == null || colony.Player == this.Player);

            bool gold = ( colony == null );

            if (gold)
                this.Player.AddGold(DisbandValue);
            else
                colony.AddProduction(DisbandValue);

            Destroy(gold);
        }

        public void Move(IEventHandler handler, Tile tile)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(tile != null);
            AssertException.Assert(this.CurSpeed > 0);
            AssertException.Assert(CheckZOC(this.Player, this.Tile, tile));
            AssertException.Assert(tile.SpaceObject == null);

            Player.Game.PushUndoCommand(new Game.UndoCommand<Tile>(
                    new Game.UndoMethod<Tile>(UndoMove), this.Tile));

            Move(tile, false);
        }
        internal Tile UndoMove(Tile tile)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(tile != null);
            AssertException.Assert(this.CurSpeed < this.MaxSpeed);
            AssertException.Assert(CheckZOC(this.Player, this.Tile, tile));
            AssertException.Assert(tile.SpaceObject == null);

            Move(tile, true);

            return this.Tile;
        }
        private void Move(Tile tile, bool undo)
        {
            if (undo)
                ++this.CurSpeed;
            else
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
                if (ship != null && ship.Player != player && ( neighbors == null ? neighbors = Tile.GetNeighbors(from) : neighbors ).Contains(neighbor))
                    return false;
            }

            return true;
        }

        public void AttackShip(IEventHandler handler, Ship ship)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(ship != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, ship.Tile));
            AssertException.Assert(this.Player != ship.Player);
            AssertException.Assert(this.CurSpeed > 0);

            --this.CurSpeed;

            double pct = Combat(handler, ship);
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

        internal void StartTurn(IEventHandler handler)
        {
            this.Repair = 0;
            LevelUp(handler);
        }

        internal void LevelUp(IEventHandler handler)
        {
            handler.OnLevel(this, 0, GetExp(0), 0);

            float needExp;
            while (this.HP > 0 && !this.Dead && this.curExp > ( needExp = (float)( this.needExpMult * ( GetValue(this.NextExpType) - GetValue() ) ) ))
            {
                double costInc = this.GetCostLastResearched();

                //temporarily add upkeep to cost, using pre-level payoff and mult
                this.cost += this.Upkeep * GetUpkeepPayoff() * GetExperienceUpkeepPayoffMult();

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
                    this.bombardDamageMult += GetBombardLevelInc();
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

                costInc = this.GetCostLastResearched() - costInc;

                //add/subtract gold for level randomness and percent of ship injured 
                Player.GoldIncome(( this.needExpMult - pct ) * costInc / Consts.ExpForGold);

                double basePayoff = GetUpkeepPayoff();
                double minCost = basePayoff * Consts.MinCostMult;
                double multPayoff = basePayoff * GetExperienceUpkeepPayoffMult();

                double upkeepInc = costInc * this.Upkeep / this.cost * Consts.ScalePct(0, 1 / Consts.ExperienceUpkeepPayoffMult, GetNonColonyPct());
                this.Upkeep += Game.Random.Round(upkeepInc);
                //remove upkeep back out of cost, using post-level payoff and mult, and add in the cost increase
                this.cost += costInc - this.Upkeep * multPayoff;

                //upkeep should never account for more than half of the ship's cost
                while (this.Upkeep > 1 && ( ( this.cost < minCost ) || ( this.Upkeep * basePayoff > this.cost ) ))
                {
                    --this.Upkeep;
                    this.cost += multPayoff;
                }
                if (this.cost < 1)
                    throw new Exception();
                if (this.Upkeep < 1)
                    throw new Exception();

                this.totalExp += needExp;
                this.curExp -= needExp;

                GetNextLevel(handler);

                handler.OnLevel(this, pct, 0, GetExp(0));
            }
        }

        private float GetBombardLevelInc()
        {
            return this.needExpMult / Consts.BombardAttackMult;
        }

        private double GetUpkeepPayoff()
        {
            return Consts.GetUpkeepPayoff(this.Player.Game.MapSize, GetNonColonyPct(), GetNonTransPct(), this.MaxSpeed);
        }

        private double GetExperienceUpkeepPayoffMult()
        {
            return Consts.ScalePct(1, Consts.ExperienceUpkeepPayoffMult, GetNonColonyPct());
        }

        private double GetNonColonyPct()
        {
            return Consts.GetNonColonyPct(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.bombardDamageMult, this.Player.Game.AvgResearch);
        }

        private double GetNonTransPct()
        {
            return Consts.GetNonTransPct(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.bombardDamageMult, this.Player.Game.AvgResearch);
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
                int ds = Game.Random.Round(Math.Sqrt(total * this.bombardDamageMult) / 39.0);
                total += ds;
                stats.Add(ExpType.DS, ds);
            }
            else
            {
                int trans = Game.Random.Round(( this.MaxPop + ( this.Colony ? 26 : 0 ) ) / Math.PI);
                total += trans;
                stats.Add(ExpType.Trans, trans);
            }
            stats.Add(ExpType.Speed, Game.Random.Round(Math.Sqrt(total * this.MaxSpeed) / 16.9));

            this.NextExpType = Game.Random.SelectValue<ExpType>(stats);
        }

        private int GetStatExpChance(int stat, int other)
        {
            double hpStr = ShipDesign.GetHPStr(stat, other);
            hpStr /= ShipDesign.GetHPStr(stat + 1, other) - hpStr;
            hpStr *= stat / (double)( stat + other );
            return Game.Random.Round(hpStr);
        }

        public void Bombard(IEventHandler handler, Planet planet)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(planet != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, planet.Tile));
            AssertException.Assert(this.CurSpeed > 0);
            Colony colony = planet.Colony;
            bool friendly = ( colony != null && this.Player == colony.Player );
            if (friendly)
                AssertException.Assert(this.DeathStar);

            --this.CurSpeed;

            double pct = 1;
            int freeDmg = 0, colonyDamage = 0, planetDamage = 0, startExp = GetTotalExp();
            bool enemy = ( !friendly && colony != null );
            if (enemy && colony.HP > 0)
                pct = AttackColony(handler, colony, out freeDmg);
            else
                handler.OnBombard(this, planet, 0, 0, 0);

            if (pct > 0)
                if (enemy && colony.HP > 0)
                    this.Player.GoldIncome(GetUpkeepReturn(pct));
                else
                    Bombard(handler, planet, friendly, pct, out colonyDamage, out planetDamage);

            handler.OnBombard(this, planet, 0, colonyDamage, planetDamage);

            LevelUp(handler);
        }

        private double AttackColony(IEventHandler handler, Colony colony, out int bombard)
        {
            double freeDmg = GetFreeDmg(colony), combatDmg, avgDef;
            Consts.GetDamageTable(this.Att, colony.Def, out combatDmg, out avgDef);

            double freePct = 0;
            bombard = GetBombardDamage(freeDmg);
            if (bombard > colony.HP)
            {
                freePct = ( 1 - ( colony.HP / (double)bombard ) );
                bombard = colony.HP;
            }
            handler.OnBombard(this, colony.Planet, bombard, 0, 0);
            colony.HP -= bombard;

            double combatPct = 1;
            if (colony.HP > 0 && ( !this.DeathStar || handler.ConfirmCombat(this, colony) ))
                combatPct = Combat(handler, colony);

            return ( ( freePct * freeDmg + combatPct * combatDmg ) / ( freeDmg + combatDmg ) );
        }

        private double GetFreeDmg(Colony colony)
        {
            return this.BombardDamage * Consts.PlanetDefensesDeathStarMult / colony.PlanetDefenseCost;
        }

        private void Bombard(IEventHandler handler, Planet planet, bool friendly, double pct, out int colonyDamage, out int planetDamage)
        {
            colonyDamage = GetColonyDamage(friendly, pct);
            planetDamage = GetPlanetDamage(colonyDamage, pct);

            int tempPop = ( planet.Colony == null ? 0 : planet.Colony.Population );

            //bombard the planet first, since it might get destroyed
            int initQuality = BombardPlanet(handler, planet, planetDamage);
            //bombard the colony second, if it exists
            int initPop = BombardColony(handler, planet.Colony, colonyDamage);

            double move = GetBombardMoveLeft(planetDamage, initQuality, colonyDamage, initPop, pct);
            if (move > 0)
                this.Player.GoldIncome(GetUpkeepReturn(move));

            if (planet.Dead)
                colonyDamage = tempPop;
            else if (colonyDamage > initPop)
                colonyDamage = initPop;
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

        private static int GetDeathStarDamage(double damage)
        {
            return Game.Random.GaussianCappedInt(damage, Consts.DeathStarDamageRndm);
        }

        private int BombardPlanet(IEventHandler handler, Planet planet, int planetDamage)
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

        private int BombardColony(IEventHandler handler, Colony colony, int colonyDamage)
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

        private static double GetBombardMoveLeft(double planetDamage, double quality, double colonyDamage, double pop, double pct)
        {
            //the 1 movement is split evenly between planet and colony damage
            double move = 0;
            //planet quality overkill
            if (planetDamage > quality)
                move += ( 1 - ( quality / planetDamage ) );
            //colony population overkill
            if (colonyDamage > pop)
                move += ( 1 - ( pop / colonyDamage ) );
            return move / 2 * pct;
        }

        public void Invade(IEventHandler handler, Colony target, int population, int gold)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(target != null);
            AssertException.Assert(population > 0);
            AssertException.Assert(population <= this.AvailablePop);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, target.Tile));
            AssertException.Assert(this.Player != target.Player);
            if (target.Population > 0)
            {
                AssertException.Assert(gold > 0);
                AssertException.Assert(gold < Player.Gold);
            }
            else
            {
                gold = 0;
            }

            handler.OnInvade(this, target);

            //all attackers cost gold to move regardless of where they end up
            this.Player.SpendGold(GetActualGoldCost(population) + gold, gold);

            double soldiers = GetSoldiers(population, this.soldiers);
            this.soldiers -= soldiers;

            //all attackers cannot be moved again regardless of where they end up
            this.Population -= population;
            target.Invasion(handler, this.Player, ref population, ref soldiers, gold);
            this.Population += population;
            this.movedPop += population;

            this.soldiers += soldiers;

            handler.OnInvade(this, target);
        }

        public void Colonize(IEventHandler handler, Planet planet)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(planet != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, planet.Tile));
            AssertException.Assert(this.Colony);
            AssertException.Assert(planet.Colony == null);
            AssertException.Assert(this.AvailablePop == this.Population);
            AssertException.Assert(this.Population > 0);
            AssertException.Assert(planet.ColonizationCost < Player.Gold);

            this.Player.SpendGold(planet.ColonizationCost);
            this.Player.GoldIncome(-GetActualGoldCost(this.Population));

            int production = Game.Random.Round(ColonizationValue);
            this.Player.NewColony(handler, planet, this.Population, this.soldiers, production);
            this.Player.GoldIncome(ColonizationValue - production);

            this.Population = 0;
            this.soldiers = 0;

            Destroy(false);
        }

        public void GoldRepair(IEventHandler handler, int hp)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(hp <= this.MaxHP - HP);
            AssertException.Assert(!this.HasRepaired);
            AssertException.Assert(GetGoldForHP(hp) < Player.Gold);

            Player.Game.PushUndoCommand(new Game.UndoCommand<int>(
                    new Game.UndoMethod<int>(UndoGoldRepair), hp));

            GoldRepair(hp, false);
        }
        internal Tile UndoGoldRepair(int hp)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(this.HP - hp > 0);
            AssertException.Assert(this.HasRepaired);

            GoldRepair(hp, true);

            return this.Tile;
        }
        private void GoldRepair(int hp, bool undo)
        {
            double spend = GetGoldForHP(hp);
            if (undo)
            {
                this.HP -= hp;
                this.Repair -= hp;
            }
            else
            {
                this.HP += hp;
                this.Repair += hp;
                spend = -spend;
            }
            Player.AddGold(spend);
            this.HasRepaired = !undo;
        }

        public Colony GetRepairedFrom()
        {
            TurnException.CheckTurn(this.Player);

            foreach (Tile neighbor in Tile.GetNeighbors(this.Tile))
            {
                Planet planet = ( neighbor.SpaceObject as Planet );
                if (planet != null && planet.Colony != null && planet.Colony.Player.IsTurn && planet.Colony.RepairShip == this)
                    return planet.Colony;
            }
            return null;
        }

        public double GetGoldForHP(double hp)
        {
            return GetGoldForHP(hp, true);
        }
        private double GetGoldForHP(double hp, bool isTotal)
        {
            TurnException.CheckTurn(this.Player);

            int floor = (int)Math.Floor(hp);
            double lower = CalcGoldForHP(floor, isTotal);
            if (floor == hp)
                return lower;

            double upper = CalcGoldForHP(floor + 1, isTotal);
            return ( lower + ( upper - lower ) * ( hp - floor ) );
        }
        private double CalcGoldForHP(int hp, bool isTotal)
        {
            return ( isTotal ? hp : 1 ) * RepairCost * Math.Pow(Consts.RepairGoldIncPowBase, hp / (double)this.MaxHP / Consts.RepairGoldHPPct);
        }

        public double GetHPForGold(double gold)
        {
            TurnException.CheckTurn(this.Player);

            return GetHPForGold(gold, true);
        }
        private double GetHPForGold(double gold, bool isTotal)
        {
            int upper = this.MaxHP - this.HP;
            if (upper > 0)
            {
                upper = TBSUtil.FindValue(delegate(int hp)
                {
                    return ( GetGoldForHP(hp, isTotal) >= gold );
                }, 0, upper, true);

                if (upper > 0 && GetGoldForHP(upper, isTotal) > gold)
                {
                    double actual, low = upper - 1, high = upper, mid = Game.Random.Range(low, high);
                    while (Math.Abs(( actual = GetGoldForHP(mid, isTotal) ) - gold) > Consts.FLOAT_ERROR)
                    {
                        if (actual > gold)
                            high = mid;
                        else
                            low = mid;
                        mid = ( low + high ) / 2;
                    }
                    return mid;
                }

            }
            return upper;
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

        public double GetAutoRepairHP()
        {
            TurnException.CheckTurn(this.Player);

            return GetAutoRepairHP(DoAutoRepair ? AutoRepair : 1);
        }
        public double GetAutoRepairHP(double autoRepair)
        {
            TurnException.CheckTurn(this.Player);

            double hp = GetHPForGold(GetGoldRepairTarget() * autoRepair, false);
            if (hp < 1 && this.HP < this.MaxHP)
                hp = 1;
            return hp;
        }

        public double GetAutoRepairForHP(double hp)
        {
            TurnException.CheckTurn(this.Player);

            if (hp <= 0)
                return 0;
            if (hp > this.MaxHP - this.HP)
                hp = this.MaxHP - this.HP;
            if (hp < 1)
                hp = 1;

            return GetGoldForHP(hp, false) / GetGoldRepairTarget();
        }

        private double GetGoldRepairTarget()
        {
            return ( this.GetCostLastResearched() * ( 1 - Consts.CostUpkeepPct ) / this.MaxHP );
        }

        public override string ToString()
        {
            return ShipNames.GetName(this._name, this._mark);
        }

        #endregion //public

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
