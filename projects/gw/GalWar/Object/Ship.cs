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

        private byte _name, _mark;

        private bool _hasRepaired;
        private byte _expType, _upkeep, _curSpeed, _maxSpeed;
        private ushort _maxTrans, _maxHP, _repair, _bombardDamage;
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
            if (design.DeathStar)
                this.BombardDamage = design.BombardDamage;

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
            int att, def, hp, speed, trans;
            double ds;
            LevelUpStats(expType, out att, out def, out hp, out speed, out trans, out ds);
            return ShipDesign.GetValue(att, def, hp, speed, trans, this.Colony, ds, this.Player.Game.AvgResearch);
        }

        internal double GetCostLastResearched()
        {
            return ShipDesign.GetTotCost(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.BombardDamage, this.Player.LastResearched);
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

        internal void EndTurn()
        {
            //must be calculated before speed is restored
            Player.SpendGold(this.Upkeep - this.GetUpkeepReturn());

            ResetMoved();

            this.CurSpeed = MaxSpeed;
            this.HasRepaired = false;
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
            Destroy(addGold, false);
        }
        internal void Destroy(bool addGold, bool random)
        {
            if (this.Dead)
                throw new Exception();

            double destroyGold = GetDestroyGold();
            if (addGold)
                this.Player.AddGold(destroyGold, random);
            else
                this.Player.GoldIncome(destroyGold);

            this.Population = 0;
            this.soldiers = 0;
            this.curExp = 0;
            this.CurSpeed = 0;

            this.Player.RemoveShip(this);
            this.Tile.SpaceObject = null;
        }

        internal void AddSoldiers(double soldiers)
        {
            this.soldiers += soldiers;
        }

        internal void AddPopulation(int pop)
        {
            this.Population += pop;
            this.movedPop += pop;
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
                return ShipDesign.GetBombardDamage(this._bombardDamage, this.Att);
            }
            private set
            {
                if (this.DeathStar)
                    this._bombardDamage = (ushort)value;
                if (this.BombardDamage != value)
                    throw new Exception();
            }
        }
        public bool DeathStar
        {
            get
            {
                return ( this._bombardDamage > 0 );
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
                    losePopulation = Game.Random.GaussianOEInt(GetTransLoss(this.HP - value), Consts.TransLossRndm, Consts.TransLossRndm);
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
            double gold = ( this.Population / Consts.PopulationForGoldLow )
                    + ( this.soldiers / Consts.SoldiersForGold )
                    + ( GetCostExperience(this.curExp) / Consts.ExpForGold )
                    + ( this.Player.IsTurn ? GetUpkeepReturn() : 0 );

            Console.WriteLine("Destroy Gold:  " + gold);
            Console.WriteLine("Population:  " + this.Population / Consts.PopulationForGoldLow);
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

        internal override double GetExpForDamage(double damage)
        {
            return damage / (double)this.MaxHP * this.GetStrength() * Consts.ExperienceMult;
        }

        protected override double GetKillExp()
        {
            return this.GetExpForDamage(this.MaxHP) * Consts.ExperienceDestroyMult;
        }

        internal override void AddExperience(double experience)
        {
            if (this.Population > 0 && this.HP > 0)
            {
                double soldiers = this.Population / Consts.PopulationForGoldMid;
                soldiers *= experience / ( soldiers + this.GetCurrentCost() ) / 1.69;
                experience -= soldiers;
                soldiers *= GetCostExpForExp() / Consts.ExpForSoldiers;
                this.soldiers += Game.Random.GaussianCapped((float)soldiers, Consts.ExperienceRndm);
            }

            this.curExp += Game.Random.GaussianCapped((float)experience, Consts.ExperienceRndm);
        }

        internal void AddAnomalyExperience(IEventHandler handler, double cost, bool funky, bool noChange)
        {
            AddCostExperience(cost);
            if (funky)
                GetNextLevel(handler, funky, false);
            LevelUp(handler, funky, noChange);
        }

        internal override void AddCostExperience(double cost)
        {
            AddExperience(cost / GetCostExpForExp());
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
            LevelUp(handler, false, false);
        }

        private void LevelUp(IEventHandler handler, bool funky, bool noChange)
        {
            bool first = true;
            float needExp;
            while (this.HP > 0 && !this.Dead && this.curExp > ( needExp = (float)( this.needExpMult * ( GetValue(this.NextExpType) - GetValue() ) ) ))
            {
                if (first)
                {
                    handler.OnLevel(this, double.NaN, GetExp(0), GetExp(needExp));
                    first = false;
                }

                double costInc = this.GetCostLastResearched();

                //temporarily add upkeep to cost, using pre-level payoff and mult
                this.cost += this.Upkeep * GetUpkeepPayoff() * GetExperienceUpkeepPayoffMult();

                double pct = this.HP / (double)this.MaxHP;
                if (this.NextExpType == ExpType.HP)
                {
                    int inc = Game.Random.Round((float)pct);
                    pct = inc;
                    this.HP += inc;
                }

                int att, def, hp, speed, trans;
                double ds;
                LevelUpStats(this.NextExpType, out att, out def, out hp, out speed, out trans, out ds);
                this.Att = att;
                this.Def = def;
                this.MaxHP = hp;
                this.MaxSpeed = speed;
                this.maxPop = trans;
                if (funky && this.NextExpType == ExpType.DS && !this.DeathStar)
                {
                    this._bombardDamage = (ushort)ds;
                    if (this.BombardDamage != ds)
                        throw new Exception();
                }
                else
                {
                    this.BombardDamage = ds;
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

                GetNextLevel(handler, funky, noChange);

                handler.OnLevel(this, pct, int.MinValue, GetExp(0));
            }
        }

        private void LevelUpStats(ExpType? expType, out int att, out int def, out int hp, out int speed, out int trans, out double ds)
        {
            att = this.Att;
            def = this.Def;
            hp = this.MaxHP;
            speed = this.MaxSpeed;
            trans = this.maxPop;
            ds = this.BombardDamage;
            if (expType.HasValue)
                switch (expType.Value)
                {
                case ExpType.Att:
                    ++att;
                    if (this.DeathStar)
                        ds = ShipDesign.SetBombardDamage(ds, att);
                    break;
                case ExpType.Def:
                    ++def;
                    break;
                case ExpType.HP:
                    ++hp;
                    break;
                case ExpType.DS:
                    ++ds;
                    ds = ShipDesign.SetBombardDamage(ds, att);
                    break;
                case ExpType.Speed:
                    ++speed;
                    if (this.DeathStar)
                        ds = ReduceDS(ds, speed);
                    break;
                case ExpType.Trans:
                    ++trans;
                    break;
                default:
                    throw new Exception();
                }
        }
        private static double ReduceDS(double bombardDamage, int speed)
        {
            return Math.Ceiling(bombardDamage * ( speed - 1.0 ) / ( speed ));
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
            return Consts.GetNonColonyPct(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.BombardDamage, this.Player.Game.AvgResearch);
        }

        private double GetNonTransPct()
        {
            return Consts.GetNonTransPct(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.BombardDamage, this.Player.Game.AvgResearch);
        }

        private void GetNextLevel(IEventHandler handler)
        {
            GetNextLevel(handler, false, false);
        }
        private void GetNextLevel(IEventHandler handler, bool funky, bool noChange)
        {
            this.needExpMult = Game.Random.GaussianCapped(1f, Consts.ExperienceRndm, (float)Consts.FLOAT_ERROR);

            if (!noChange)
            {
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
                    int ds = Game.Random.Round(this.BombardDamage * Math.E);
                    total += ds;
                    stats.Add(ExpType.DS, ds);
                }
                else if (this.MaxPop > 0)
                {
                    int trans = Game.Random.Round(( this.MaxPop + ( this.Colony ? 26 : 0 ) ) / Math.PI);
                    total += trans;
                    stats.Add(ExpType.Trans, trans);
                }

                int speed = Game.Random.Round(Math.Sqrt(total * this.MaxSpeed) / 16.9);
                total += speed;
                stats.Add(ExpType.Speed, speed);

                if (funky)
                {
                    FunkyChance(stats, ExpType.HP, total);
                    FunkyChance(stats, ExpType.Att, total);
                    FunkyChance(stats, ExpType.Def, total);
                    if (this.MaxPop == 0)
                        FunkyChance(stats, ExpType.DS, total);
                    FunkyChance(stats, ExpType.Speed, total);
                    if (!this.DeathStar)
                        FunkyChance(stats, ExpType.Trans, total);
                }

                this.NextExpType = Game.Random.SelectValue<ExpType>(stats);
            }
        }
        private int GetStatExpChance(int stat, int other)
        {
            double hpStr = ShipDesign.GetHPStr(stat, other);
            hpStr /= ShipDesign.GetHPStr(stat + 1, other) - hpStr;
            hpStr *= stat / (double)( stat + other );
            return Game.Random.Round(hpStr);
        }
        private void FunkyChance(Dictionary<ExpType, int> stats, ExpType expType, int total)
        {
            int value;
            stats.TryGetValue(expType, out value);
            double inv = ( total + 1f ) / ( value + 1f );
            double opp = 6f * ( total - value ) / total;
            stats[expType] = Game.Random.Round(inv + opp);
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

            //log initial state
            handler.OnBombard(this, planet, int.MinValue, int.MinValue, int.MinValue);

            --this.CurSpeed;

            //set freeDmg to 0 initially to ensure we log something, even if it is just "No Damage"
            int freeDmg = 0;

            double pct = 1;
            bool enemy = ( !friendly && colony != null );
            if (enemy && colony.HP > 0)
                pct = AttackColony(handler, colony, out freeDmg);

            if (pct > 0)
                if (enemy && colony.HP > 0)
                {
                    this.Player.GoldIncome(GetUpkeepReturn(pct));
                }
                else
                {
                    int colonyDamage, planetDamage;
                    Bombard(handler, planet, friendly, pct, out colonyDamage, out planetDamage);

                    //log actual damage to planet
                    if (freeDmg == -1)
                        freeDmg = 0;
                    handler.OnBombard(this, planet, freeDmg, colonyDamage, planetDamage);
                    //freeDmg of -1 means we have logged something
                    freeDmg = -1;
                }
            //freeDmg will only be -1 if we have already logged something
            if (freeDmg != -1)
                handler.OnBombard(this, planet, freeDmg, 0, 0);

            LevelUp(handler);
        }

        private double AttackColony(IEventHandler handler, Colony colony, out int freeDmg)
        {
            double freeAvg = GetFreeDmg(colony), combatAvg, avgDef;
            Consts.GetDamageTable(this.Att, colony.Def, out combatAvg, out avgDef);

            double freePct = 0;
            freeDmg = GetBombardDamage(freeAvg);
            if (freeDmg > colony.HP)
            {
                freePct = ( 1 - ( colony.HP / (double)freeDmg ) );
                freeDmg = colony.HP;
            }
            colony.HP -= freeDmg;

            double exp = colony.GetExpForDamage(freeDmg);
            this.AddExperience(exp);
            colony.AddExperience(exp);

            double combatPct = 1;
            if (colony.HP > 0 && ( !this.DeathStar || handler.ConfirmCombat(this, colony) ))
            {

                //log free damage, if any, before combat
                if (freeDmg > 0)
                    handler.OnBombard(this, colony.Planet, freeDmg, 0, 0);

                combatPct = Combat(handler, colony);

                //re-log intial state, since combat has broken up our logging
                handler.OnBombard(this, colony.Planet, int.MinValue, int.MinValue, int.MinValue);
                //we have at least logged combat, so we do not need to ensure we log anything else
                freeDmg = -1;
            }

            return ( ( freePct * freeAvg + combatPct * combatAvg ) / ( freeAvg + combatAvg ) );
        }

        private double GetFreeDmg(Colony colony)
        {
            return this.BombardDamage * Consts.PlanetDefensesDeathStarMult / colony.PlanetDefenseCost;
        }

        private void Bombard(IEventHandler handler, Planet planet, bool friendly, double pct, out int colonyDamage, out int planetDamage)
        {
            colonyDamage = GetColonyDamage(planet, pct);

            int dmgBase = colonyDamage, tempPop = ( planet.Colony == null ? 0 : planet.Colony.Population );
            double dmgMult = pct;
            if (!friendly && tempPop > 0 && colonyDamage > tempPop && !handler.Continue())
                if (this.DeathStar)
                    dmgMult *= tempPop / colonyDamage;
                else
                    dmgBase = tempPop;
            planetDamage = GetPlanetDamage(dmgBase, dmgMult);

            //bombard the planet first, since it might get destroyed
            int initQuality = BombardPlanet(handler, planet, planetDamage);
            //bombard the colony second, if it exists
            int initPop = BombardColony(handler, planet.Colony, colonyDamage);

            double move = GetBombardMoveLeft(planetDamage, initQuality, friendly ? 0 : colonyDamage, initPop, pct);
            if (move > 0)
                this.Player.GoldIncome(GetUpkeepReturn(move));

            if (planet.Dead)
                colonyDamage = tempPop;
            else if (colonyDamage > initPop)
                colonyDamage = initPop;
        }

        private int GetColonyDamage(Planet planet, double pct)
        {
            double damage = this.BombardDamage * pct;
            if (planet.Colony != null && planet.Colony.Player == this.player)
                damage *= planet.Colony.Population / ( planet.Colony.Population + ( 3 * planet.Quality + 1 * Consts.AverageQuality ) / 4.0 );
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
                return GetDeathStarDamage(this.BombardDamage * pct * Consts.DeathStarPlanetDamage);
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
                if (planet.Dead)
                    exp += Planet.ConstValue;

                this.AddCostExperience(exp);
                if (!planet.Dead && planet.Colony != null)
                    planet.Colony.AddCostExperience(exp);
                else
                    this.Player.GoldIncome(-exp);
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

            //all attackers cost gold to move regardless of where they end up
            this.Player.SpendGold(GetActualGoldCost(population) + gold, gold);

            double exp;

            double soldiers = GetSoldiers(population, this.soldiers);
            this.soldiers -= soldiers;

            //all attackers cannot be moved again regardless of where they end up
            this.Population -= population;
            target.Invasion(handler, this, ref population, ref soldiers, gold, out exp);
            this.Population += population;
            this.movedPop += population;

            this.soldiers += soldiers;

            this.AddCostExperience(exp);
        }

        public void Explore(IEventHandler handler, Anomaly anomaly)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(anomaly != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, anomaly.Tile));
            AssertException.Assert(this.CurSpeed > 0);

            --this.CurSpeed;

            anomaly.Explore(handler, this);
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
