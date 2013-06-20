using System;
using System.Collections.Generic;
using MattUtil;

namespace GalWar
{
    [Serializable]
    public class Ship : Combatant
    {
        #region fields and constructors

        public readonly bool Colony;

        private readonly Player _player;

        private readonly byte _name, _mark;
        private readonly float _expDiv;

        //private Tile _vector;

        private bool _hasRepaired;
        private byte _expType, _upkeep, _curSpeed, _maxSpeed;
        private ushort _maxHP, _maxTrans, _bombardDamage, _repair;
        private float _autoRepair, _needExpMult, _curExp, _totalExp;
        private double _cost;

        internal Ship(IEventHandler handler, Player player, Tile tile, ShipDesign design)
            : base(tile, design.Att, design.Def, design.HP, 0, 0)
        {
            checked
            {
                this.Colony = design.Colony;

                this._player = player;

                this._name = (byte)design.Name;
                this._mark = (byte)design.Mark;

                //this._vector = null;

                this._hasRepaired = false;

                this._expType = byte.MaxValue;

                this._upkeep = (byte)design.Upkeep;
                this._curSpeed = (byte)design.Speed;
                this._maxSpeed = (byte)design.Speed;
                this._maxHP = (ushort)design.HP;
                this._maxTrans = (ushort)design.Trans;

                if (design.DeathStar)
                    this._bombardDamage = (ushort)ShipDesign.SetBombardDamage(design.BombardDamage, design.Att);
                else
                    this._bombardDamage = 0;

                this._repair = 0;

                this._autoRepair = float.NaN;
                this._needExpMult = float.NaN;

                this._curExp = 0;
                this._totalExp = 0;

                this._cost = design.AdjustCost(this.Player.Game.MapSize, this.Player.LastResearched);

                this._expDiv = (float)GetValue();

                GetNextLevel(handler);
                Player.GoldIncome(-GetUpkeepReturn(this.CurSpeed));
            }
        }

        public override Player Player
        {
            get
            {
                return this._player;
            }
        }

        private int name
        {
            get
            {
                return this._name;
            }
        }
        private int mark
        {
            get
            {
                return this._mark;
            }
        }

        private double expDiv
        {
            get
            {
                return this._expDiv;
            }
        }

        //public Tile Vector
        //{
        //    get
        //    {
        //        TurnException.CheckTurn(this.Player);

        //        return this._vector;
        //    }
        //    set
        //    {
        //        checked
        //        {
        //            TurnException.CheckTurn(this.Player);

        //            this._vector = value;
        //        }
        //    }
        //}

        public bool HasRepaired
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this._hasRepaired;
            }
            private set
            {
                checked
                {
                    this._hasRepaired = value;
                }
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
                checked
                {
                    this._expType = (byte)value;
                }
            }
        }

        public double Upkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.baseUpkeep - this.GetUpkeepReturn() + Consts.GetSoldierUpkeep(this);
            }
        }
        public int BaseUpkeep
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return this.baseUpkeep;
            }
        }
        private int baseUpkeep
        {
            get
            {
                return this._upkeep;
            }
            set
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
        private int maxPop
        {
            get
            {
                return this._maxTrans;
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
                checked
                {
                    if (this.DeathStar)
                        this._bombardDamage = (ushort)value;
                    if (this.BombardDamage != value)
                        throw new Exception();
                }
            }
        }
        public bool DeathStar
        {
            get
            {
                return ( this._bombardDamage > 0 );
            }
            private set
            {
                checked
                {
                    if (DeathStar || !value || this.NextExpType != ExpType.DS)
                        throw new Exception();
                    this._bombardDamage = ushort.MaxValue;
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
        public double AutoRepair
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                double autoRepair = this._autoRepair;
                if (!double.IsNaN(autoRepair))
                {
                    if (autoRepair <= 0)
                        autoRepair = 0;
                    else if (this.HP < this.MaxHP)
                        autoRepair = GetAutoRepairForHP(GetAutoRepairHP(autoRepair));
                    this.AutoRepair = autoRepair;
                }

                return autoRepair;
            }
            set
            {
                checked
                {
                    TurnException.CheckTurn(this.Player);

                    if (this.HP == this.MaxHP)
                        value = double.NaN;
                    this._autoRepair = (float)value;
                }
            }
        }

        private double needExpMult
        {
            get
            {
                return this._needExpMult;
            }
            set
            {
                checked
                {
                    this._needExpMult = (float)value;
                }
            }
        }
        private double curExp
        {
            get
            {
                return this._curExp;
            }
            set
            {
                checked
                {
                    this._curExp = (float)value;
                }
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
                checked
                {
                    this._totalExp = (float)value;
                }
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
                checked
                {
                    this._cost = value;
                }
            }
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
        internal double GetCostAvgResearch()
        {
            return ShipDesign.GetTotCost(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.BombardDamage, this.Player.Game.AvgResearch);
        }

        private double RepairCost
        {
            get
            {
                return ( Consts.RepairCostMult * this.cost / (double)this.MaxHP );
            }
        }

        public bool DoAutoRepair
        {
            get
            {
                return ( !HasRepaired && AutoRepair > 0 );
            }
        }

        #endregion //fields and constructors

        #region internal

        internal void EndTurn()
        {
            //must be calculated before speed is restored
            this.Player.SpendGold(this.Upkeep);

            ResetMoved();

            this.CurSpeed = MaxSpeed;
            this.HasRepaired = false;
        }

        internal void LoseMove()
        {
            if (!this.Player.IsTurn)
                throw new Exception();

            this.Player.GoldIncome(GetUpkeepReturn());
            this.CurSpeed = 0;
        }

        public double GetUpkeepReturn()
        {
            TurnException.CheckTurn(this.Player);

            return GetUpkeepReturn(this.CurSpeed);
        }
        internal double GetUpkeepReturn(double speedLeft)
        {
            return speedLeft / (double)MaxSpeed * Consts.UpkeepUnmovedReturn * this.baseUpkeep;
        }

        internal int ProductionRepair(ref double production, ref double gold, bool doRepair, bool minGold)
        {
            int retVal = 0;
            double hp = GetHPForProd(production, !doRepair);

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
            this.Soldiers = 0;
            this.curExp = 0;
            this.CurSpeed = 0;

            this.Player.RemoveShip(this);
            this.Tile.SpaceObject = null;
        }

        internal void AddSoldiers(double soldiers)
        {
            this.Soldiers += soldiers;
        }

        internal void AddPopulation(int pop)
        {
            this.Population += pop;
            this.movedPop += pop;
        }

        #endregion //internal

        #region public

        public override int MaxPop
        {
            get
            {
                return this.maxPop;
            }
        }

        public bool Dead
        {
            get
            {
                return ( this.Tile == null || this.Tile.SpaceObject != this );
            }
        }

        public double DisbandValue
        {
            get
            {
                TurnException.CheckTurn(this.Player);

                return GetDisbandValue(this.HP);
            }
        }

        internal double GetDisbandValue(double hp)
        {
            return ( Consts.DisbandPct * this.cost * hp / (double)this.MaxHP );
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

        protected override void OnDamaged(int damage)
        {
            if (this.Population > 0)
            {
                int losePopulation;
                if (this.HP > damage)
                    //if hp is lost for any reason, some transported population is killed off
                    losePopulation = Game.Random.GaussianOEInt(Consts.GetTransLoss(this, damage), Consts.TransLossRndm, Consts.TransLossRndm);
                else
                    losePopulation = this.Population;
                LosePopulation(losePopulation);
            }
        }

        public double GetColonizationValue(double repair)
        {
            TurnException.CheckTurn(this.Player);

            return ShipDesign.GetColonizationValue(this.cost, this.Att, this.Def, this.HP + repair, this.MaxHP,
                    this.MaxSpeed, this.MaxPop, this.Colony, this.BombardDamage, this.Player.LastResearched);
        }

        public int GetTotalExp()
        {
            return GetExp(this.curExp);
        }
        private int GetExp(double exp)
        {
            return (int)Math.Round(100 * ( this.totalExp + exp ) / this.expDiv);
        }

        public double GetDestroyGold()
        {
            double gold = ( this.Population / Consts.PopulationForGoldLow )
                    + ( this.Soldiers / Consts.SoldiersForGold )
                    + ( GetValueExpForRawExp(this.curExp) / Consts.ExpForGold )
                    + ( this.Player.IsTurn ? GetUpkeepReturn() : 0 );

            Console.WriteLine("Destroy Gold:  " + gold);
            Console.WriteLine("Population:  " + this.Population / Consts.PopulationForGoldLow);
            Console.WriteLine("Soldiers:  " + this.Soldiers / Consts.SoldiersForGold);
            Console.WriteLine("Experience:  " + GetValueExpForRawExp(this.curExp) / Consts.ExpForGold);
            Console.WriteLine("Upkeep:  " + ( this.Player.IsTurn ? GetUpkeepReturn() : 0 ));
            Console.WriteLine();

            return gold;
        }

        public void Disband(IEventHandler handler, Colony colony)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(colony == null || colony.Player == this.Player);

            bool isGold = ( colony == null );

            int population = this.Population;
            double soldiers = this.Soldiers;
            double curExp = this.curExp;
            int curSpeed = this.CurSpeed;
            int production = 0;
            double addGold = 0, goldIncome;

            if (isGold)
            {
                addGold = Player.RoundGold(DisbandValue) + Player.RoundGold(GetDestroyGold());
                goldIncome = DisbandValue - addGold;

                this.Player.AddGold(DisbandValue);
            }
            else
            {
                colony.AddProduction(DisbandValue, false, out goldIncome, out production);
            }

            goldIncome += GetDestroyGold();

            Destroy(isGold);

            Player.Game.PushUndoCommand(new Game.UndoCommand<Colony, int, double, double, int, int, double, double>(
                    new Game.UndoMethod<Colony, int, double, double, int, int, double, double>(UndoDisband),
                    colony, population, soldiers, curExp, curSpeed, production, addGold, goldIncome));
        }
        private Tile UndoDisband(Colony colony, int population, double soldiers, double curExp, int curSpeed, int production, double addGold, double goldIncome)
        {
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(population >= 0);
            AssertException.Assert(population <= this.MaxPop);
            AssertException.Assert(soldiers >= 0);
            AssertException.Assert(curExp >= 0);
            AssertException.Assert(curSpeed >= 0);
            AssertException.Assert(curSpeed <= this.MaxSpeed);
            AssertException.Assert(addGold >= 0);
            if (colony == null)
            {
                AssertException.Assert(production == 0);
                AssertException.Assert(addGold < this.Player.Gold);
                AssertException.Assert(goldIncome > -.6);
            }
            else
            {
                AssertException.Assert(production >= 0);
                AssertException.Assert(addGold == 0);
                AssertException.Assert(goldIncome > -.2);
            }

            this.Population = population;
            this.Soldiers = soldiers;
            this.curExp = curExp;
            this.CurSpeed = curSpeed;

            if (colony != null)
                colony.UndoAddProduction(production);
            this.Player.AddGold(-addGold);
            this.Player.GoldIncome(-goldIncome);

            this.Player.AddShip(this);
            this.Tile.SpaceObject = this;

            return this.Tile;
        }

        internal void Teleport(Tile tile)
        {
            this.Tile = tile;
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
        private Tile UndoMove(Tile tile)
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
            Teleport(tile);
        }

        public static bool CheckZOC(Player player, Tile from, Tile to)
        {
            AssertException.Assert(player != null);
            AssertException.Assert(from != null);
            AssertException.Assert(to != null);

            if (!Tile.IsNeighbor(from, to))
                return false;

            //Zone of Control: cannot move from and to tiles that are both adjacent to the same enemy
            foreach (Tile neighbor in Tile.GetNeighbors(to))
            {
                SpaceObject spaceObject = neighbor.SpaceObject;
                Planet planet = spaceObject as Planet;
                if (( spaceObject is Ship || ( planet != null && planet.Colony != null && planet.Colony.HP > 0 ) )
                        && spaceObject.Player != player && Tile.IsNeighbor(from, neighbor))
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
        internal double AttackAnomalyShip(IEventHandler handler, Ship ship)
        {
            double pct = Combat(handler, ship);
            this.LevelUp(handler);
            ship.LevelUp(handler);
            return pct;
        }

        internal override double GetExpForDamage(double damage)
        {
            return damage / (double)this.MaxHP * this.GetStrength() * Consts.ExperienceMult;
        }

        protected override double GetKillExp()
        {
            return this.GetExpForDamage(this.MaxHP) * Consts.ExperienceDestroyMult;
        }

        internal override void AddExperience(double rawExp, double valueExp)
        {
            rawExp += valueExp / GetValueExpForRawExp(1);

            if (this.Population > 0 && this.HP > 0)
            {
                double soldiers = this.Population / Consts.PopulationForGoldMid;
                soldiers *= rawExp / ( soldiers + this.GetCostLastResearched() );
                rawExp -= soldiers;
                soldiers = GetValueExpForRawExp(soldiers) / Consts.ExpForSoldiers;
                this.Soldiers += Consts.GetExperience(soldiers);
            }

            this.curExp += Consts.GetExperience(rawExp);
        }

        internal void AddAnomalyExperience(IEventHandler handler, double cost, bool funky, bool noChange)
        {
            AddExperience(0, cost);
            if (funky)
                GetNextLevel(handler, funky, false);
            LevelUp(handler, funky, noChange);
        }

        internal double GetValueExpForRawExp(double experience)
        {
            return experience * GetCostLastResearched() / GetValue();
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
            double needExp;
            while (this.HP > 0 && !this.Dead && this.curExp > ( needExp = this.needExpMult * ( GetValue(this.NextExpType) - GetValue() ) ))
            {
                if (first)
                {
                    handler.OnLevel(this, double.NaN, GetExp(0), GetExp(needExp));
                    first = false;
                }

                double costInc = this.GetCostLastResearched();

                double pct = this.HP / (double)this.MaxHP;
                if (this.NextExpType == ExpType.HP)
                {
                    int inc = Game.Random.Round(pct);
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
                if (this.NextExpType == ExpType.DS && !this.DeathStar)
                    this.DeathStar = true;
                this.BombardDamage = ds;

                costInc = this.GetCostLastResearched() - costInc;

                //add/subtract gold for level randomness and percent of ship injured
                double goldIncome = ( this.needExpMult - pct ) * costInc / Consts.ExpForGold;
                Console.WriteLine(goldIncome);
                this.Player.GoldIncome(goldIncome);

                double basePayoff = GetUpkeepPayoff();
                double minCost = basePayoff * Consts.MinCostMult;
                double multPayoff = basePayoff * GetExperienceUpkeepPayoffMult();

                int addUpkeep = Game.Random.Round(costInc * this.baseUpkeep / this.cost
                        * Consts.ScalePct(0, 1 / Consts.ExperienceUpkeepPayoffMult, GetNonColonyPct()));
                this.baseUpkeep += addUpkeep;
                this.cost += costInc - addUpkeep * multPayoff;

                //upkeep should never account for more than half of the ship's cost
                while (this.baseUpkeep > 1 && ( ( this.cost < minCost ) || ( this.baseUpkeep * basePayoff > this.cost ) ))
                {
                    --this.baseUpkeep;
                    this.cost += multPayoff;
                }
                if (this.cost < 1 / Consts.RepairCostMult)
                    throw new Exception();
                if (this.baseUpkeep < 1)
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
                    else
                        ds = ShipDesign.GetBombardDamage(0, att);
                    break;
                case ExpType.Def:
                    ++def;
                    break;
                case ExpType.HP:
                    ++hp;
                    break;
                case ExpType.DS:
                    if (this.DeathStar)
                        ++ds;
                    else
                        ds = ShipDesign.SetBombardDamage(0, att);
                    break;
                case ExpType.Speed:
                    ++speed;
                    if (this.DeathStar)
                        ds = ShipDesign.SetBombardDamage(Math.Ceiling(ds * ( speed - 1.0 ) / (double)speed), att);
                    break;
                case ExpType.Trans:
                    ++trans;
                    break;
                default:
                    throw new Exception();
                }
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
            return GetNonColonyPct(true);
        }
        private double GetNonColonyPct(bool sqr)
        {
            return Consts.GetNonColonyPct(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.BombardDamage, this.Player.LastResearched, sqr);
        }

        private double GetNonTransPct()
        {
            return GetNonTransPct(true);
        }
        private double GetNonTransPct(bool sqr)
        {
            return Consts.GetNonTransPct(this.Att, this.Def, this.MaxHP, this.MaxSpeed, this.MaxPop, this.Colony, this.BombardDamage, this.Player.LastResearched, sqr);
        }

        private void GetNextLevel(IEventHandler handler)
        {
            GetNextLevel(handler, false, false);
        }
        private void GetNextLevel(IEventHandler handler, bool funky, bool noChange)
        {
            this.needExpMult = Game.Random.GaussianCapped(1, Consts.ExperienceRndm, Consts.FLOAT_ERROR);

            if (!noChange)
            {
                //randomly select a stat to increase next based on the current ratios
                Dictionary<ExpType, int> stats = new Dictionary<ExpType, int>();

                int att = GetStatExpChance(Att, Def);
                int def = GetStatExpChance(Def, Att);
                int hp = this.MaxHP;
                stats.Add(ExpType.Att, att);
                stats.Add(ExpType.Def, def);
                stats.Add(ExpType.HP, hp);
                int total = att + def + hp;

                double strInc = ( GetValue(ExpType.Att) * att + GetValue(ExpType.Def) * def + GetValue(ExpType.HP) * hp )
                        / (double)total - GetValue();

                int ds = 0, trans = 0;
                if (this.DeathStar)
                {
                    ds = GetExpChance(ref total, ref strInc, ExpType.DS,
                            ShipDesign.GetTotCost(Att, Def, MaxHP, MaxSpeed, MaxPop, Colony, 0, Player.LastResearched)
                            / ShipDesign.GetTotCost(Att, Def, MaxHP, MaxSpeed, MaxPop, Colony, BombardDamage, Player.LastResearched));
                    stats.Add(ExpType.DS, ds);
                }
                else if (this.MaxPop > 0)
                {
                    trans = GetExpChance(ref total, ref strInc, ExpType.Trans, GetNonTransPct(false) * Math.Sqrt(GetNonColonyPct(false)));
                    stats.Add(ExpType.Trans, trans);
                }

                int speed = GetExpChance(ref total, ref strInc, ExpType.Speed,
                        Math.Sqrt(ShipDesign.GetTotCost(Att, Def, MaxHP, -1, MaxPop, Colony, BombardDamage, Player.LastResearched)
                        / ShipDesign.GetTotCost(Att, Def, MaxHP, MaxSpeed, MaxPop, Colony, BombardDamage, Player.LastResearched))
                        , Math.Sqrt(MaxSpeed / 6.5));
                stats.Add(ExpType.Speed, speed);

                if (funky)
                    stats = FunkyChance(total, ds, trans, speed);

                this.NextExpType = Game.Random.SelectValue<ExpType>(stats);
            }
        }
        private static int GetStatExpChance(int stat, int other)
        {
            double hpStr = ShipDesign.GetHPStr(stat, other);
            hpStr /= ShipDesign.GetHPStr(stat + 1, other) - hpStr;
            hpStr *= stat / (double)( stat + other );
            return Game.Random.Round(hpStr);
        }
        private int GetExpChance(ref int total, ref double strInc, ExpType expType, double nonTypePct)
        {
            return GetExpChance(ref total, ref strInc, expType, nonTypePct, 1);
        }
        private int GetExpChance(ref int total, ref double strInc, ExpType expType, double nonTypePct, double mult)
        {
            nonTypePct = Math.Sqrt(nonTypePct);
            double typeInc = GetValue(expType) - GetValue();
            double avg = ( total * strInc / nonTypePct - total * strInc ) / typeInc;
            const double add = .39;
            if (avg < 0)
                avg = add / ( 1.0 - avg );
            else
                avg += add;
            int type = Game.Random.Round(avg * mult);
            strInc = ( strInc * total + typeInc * type ) / ( total + type );
            total += type;
            return type;
        }
        private Dictionary<ExpType, int> FunkyChance(int oldTotal, int ds, int trans, int speed)
        {
            int newTotal;
            Dictionary<ExpType, int> stats = ShipDesign.IncreaseAttDef(Att, Def, MaxHP, out newTotal);
            double mult = oldTotal / (double)newTotal;
            newTotal = 0;

            int def = stats[ExpType.Att];
            int att = Game.Random.Round(mult * stats[ExpType.Def]);
            def = Game.Random.Round(mult * def);
            int hp = Game.Random.Round(mult * stats[ExpType.HP]);
            newTotal += att + def + hp;
            stats[ExpType.Att] = att;
            stats[ExpType.Def] = def;
            stats[ExpType.HP] = hp;

            speed = Game.Random.Round(speed * 3.9 / (double)this.MaxSpeed);
            newTotal += speed;
            stats.Add(ExpType.Speed, speed);

            if (this.DeathStar)
            {
                stats.Add(ExpType.DS, FunkyChance(ds, oldTotal, newTotal));
            }
            else if (this.MaxPop > 0)
            {
                stats.Add(ExpType.Trans, FunkyChance(trans, oldTotal, newTotal));
            }
            else
            {
                double amt = newTotal * .013 + 1.3;
                stats.Add(ExpType.DS, Game.Random.Round(amt));
                stats.Add(ExpType.Trans, Game.Random.Round(amt));
            }
            return stats;
        }
        private static int FunkyChance(int value, int oldTotal, int newTotal)
        {
            return Game.Random.Round(newTotal * .065 * ( 3.9 + oldTotal - value ) / ( 1.0 + value ));
        }

        public void Bombard(IEventHandler handler, Planet planet)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(planet != null);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, planet.Tile));
            AssertException.Assert(this.CurSpeed > 0);
            bool friendly = ( this.Player == planet.Player );
            if (friendly)
                AssertException.Assert(this.DeathStar);

            //log initial state
            handler.OnBombard(this, planet, int.MinValue, int.MinValue, int.MinValue);

            --this.CurSpeed;

            //set freeDmg to 0 initially to ensure we log something, even if it is just "No Damage"
            int freeDmg = 0;

            Colony colony = planet.Colony;
            double pct = 1, rawExp = 0, valueExp = 0;
            bool enemy = ( !friendly && colony != null );
            if (enemy && colony.HP > 0)
                pct = AttackColony(handler, colony, out freeDmg, ref rawExp, ref valueExp);

            if (pct > 0)
                if (enemy && colony.HP > 0)
                {
                    this.Player.GoldIncome(GetUpkeepReturn(pct));
                }
                else
                {
                    int colonyDamage, planetDamage;
                    Bombard(handler, planet, friendly, pct, out colonyDamage, out planetDamage, ref rawExp, ref valueExp);

                    if (freeDmg == -1 && ( colonyDamage != 0 || planetDamage != 0 ))
                        freeDmg = 0;
                    //log actual damage to planet
                    if (freeDmg != -1)
                        handler.OnBombard(this, planet, freeDmg, colonyDamage, planetDamage);
                    //freeDmg of -1 means we have logged something
                    freeDmg = -1;
                }
            //freeDmg will only be -1 if we have already logged something
            if (freeDmg != -1)
                handler.OnBombard(this, planet, freeDmg, 0, 0);

            this.AddExperience(rawExp, valueExp);
            if (planet.Colony != null)
                planet.Colony.AddExperience(rawExp, valueExp);
            else
                this.Player.GoldIncome(-this.GetValueExpForRawExp(rawExp) - valueExp);

            LevelUp(handler);
        }

        private double AttackColony(IEventHandler handler, Colony colony, out int freeDmg, ref double rawExp, ref double valueExp)
        {
            double freeAvg = GetFreeDmg(colony.PlanetDefenseCostPerHP), combatAvg, avgDef;
            Consts.GetDamageTable(this.Att, colony.Def, out combatAvg, out avgDef);

            double freePct = 0;
            freeDmg = GetBombardDamage(freeAvg);
            if (freeDmg > colony.HP)
            {
                freePct = ( 1 - ( colony.HP / (double)freeDmg ) );
                freeDmg = colony.HP;
            }

            colony.Damage(freeDmg, ref rawExp, ref valueExp);

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

        public double GetFreeDmg(Colony colony)
        {
            TurnException.CheckTurn(colony.Player);

            return GetFreeDmg(colony.PlanetDefenseCostPerHP);
        }
        private double GetFreeDmg(double costPerHP)
        {
            return this.BombardDamage * Consts.BombardFreeDmgMult / costPerHP;
        }

        private void Bombard(IEventHandler handler, Planet planet, bool friendly, double pct, out int colonyDamage, out int planetDamage, ref double rawExp, ref double valueExp)
        {
            colonyDamage = GetColonyDamage(planet, pct);

            int dmgBase = colonyDamage, tempPop = ( planet.Colony == null ? 0 : planet.Colony.Population );
            double dmgMult = pct;
            bool stopBombard = ( !friendly && tempPop > 0 && colonyDamage > tempPop && !handler.Continue() );
            if (stopBombard)
                if (this.DeathStar)
                    dmgMult *= tempPop / (double)colonyDamage;
                else
                    dmgBase = tempPop;
            planetDamage = GetPlanetDamage(dmgBase, dmgMult);

            //bombard the planet first, since it might get destroyed
            int initQuality = BombardPlanet(handler, planet, planetDamage, ref rawExp, ref valueExp);
            //bombard the colony second, if it exists
            int initPop = BombardColony(handler, planet.Colony, colonyDamage, ref rawExp, ref valueExp);

            if (friendly)
                colonyDamage = initPop - colonyDamage;
            double moveLeftPlanetDmg = planetDamage;
            if (stopBombard)
            {
                initQuality = Math.Min(initQuality, planetDamage);
                moveLeftPlanetDmg = planetDamage * colonyDamage / (double)tempPop;
            }
            this.Player.GoldIncome(GetUpkeepReturn(GetBombardMoveLeft(moveLeftPlanetDmg, initQuality, colonyDamage, initPop, pct)));

            if (planet.Dead)
                colonyDamage = tempPop;
            else if (colonyDamage > initPop)
                colonyDamage = initPop;
        }

        private int GetColonyDamage(Planet planet, double pct)
        {
            double damage = this.BombardDamage * pct;
            if (planet.Player == this.Player)
                damage *= planet.Colony.Population / (double)( planet.Colony.Population + ( 3 * planet.Quality + 1 * Consts.AverageQuality ) / 4.0 );
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

        private int BombardPlanet(IEventHandler handler, Planet planet, int planetDamage, ref double rawExp, ref double valueExp)
        {
            //quality has to drop to -1 to destroy planet
            int initQuality = planet.Quality + 1;

            if (planetDamage > 0)
            {
                planet.ReduceQuality(planetDamage);

                double exp = Math.Min(initQuality, planetDamage);
                if (planet.Dead)
                    exp += Consts.PlanetConstValue;
                valueExp += exp * Consts.TroopExperienceMult;
            }

            return initQuality;
        }

        private int BombardColony(IEventHandler handler, Colony colony, int colonyDamage, ref double rawExp, ref double valueExp)
        {
            int initPop = 0;
            if (colony != null)
            {
                initPop = colony.Population;

                if (colonyDamage > 0)
                {
                    valueExp += Math.Min(colony.Population, colonyDamage) * Consts.TroopExperienceMult;
                    colony.LosePopulation(colonyDamage);
                }
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
            return move / 2.0 * pct;
        }

        public void Invade(IEventHandler handler, Colony target, int population, int gold)
        {
            handler = new HandlerWrapper(handler, this.Player.Game);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(target != null);
            AssertException.Assert(this.AvailablePop > 0);
            AssertException.Assert(Tile.IsNeighbor(this.Tile, target.Tile));
            AssertException.Assert(this.Player != target.Player);
            if (target.Population > 0)
            {
                AssertException.Assert(population == this.AvailablePop);
                AssertException.Assert(gold > 0);
                AssertException.Assert(gold < this.Player.Gold);
            }
            else
            {
                AssertException.Assert(population > 0);
                AssertException.Assert(population <= this.AvailablePop);
                AssertException.Assert(gold == 0);
            }

            double soldiers;
            if (target.Population > 0)
                soldiers = GetSoldiers(population);
            else
                soldiers = GetMoveSoldiers(population);

            this.Player.SpendGold(gold);
            //all attackers cost gold to move regardless of where they end up
            this.Player.GoldIncome(-GetGoldCost(population, soldiers));

            //all attackers cannot be moved again regardless of where they end up
            this.Population -= population;
            this.Soldiers -= soldiers;
            target.Invasion(handler, this, ref population, ref soldiers, gold);
            this.movedPop += population;
            this.Population += population;
            this.Soldiers += soldiers;

            LevelUp(handler);
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
            AssertException.Assert(planet.ColonizationCost < this.Player.Gold);

            this.Player.SpendGold(planet.ColonizationCost);
            this.Player.GoldIncome(-GetGoldCost(this.Population, this.Soldiers));

            int production = Game.Random.Round(ColonizationValue);
            this.Player.NewColony(handler, planet, this.Population, this.Soldiers, production);
            this.Player.GoldIncome(ColonizationValue - production);

            this.Population = 0;
            this.Soldiers = 0;

            Destroy(false);
        }

        public void GoldRepair(IEventHandler handler, int hp)
        {
            handler = new HandlerWrapper(handler, this.Player.Game, false);
            TurnException.CheckTurn(this.Player);
            AssertException.Assert(hp > 0);
            AssertException.Assert(hp <= this.MaxHP - HP);
            AssertException.Assert(!this.HasRepaired);
            AssertException.Assert(GetGoldForHP(hp) < this.Player.Gold);

            Player.Game.PushUndoCommand(new Game.UndoCommand<int>(
                    new Game.UndoMethod<int>(UndoGoldRepair), hp));

            GoldRepair(hp, false);
        }
        private Tile UndoGoldRepair(int hp)
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
                //base.SetHP to avoid OnDamaged processing
                base.SetHP(this.HP - hp);
                this.Repair -= hp;
            }
            else
            {
                this.HP += hp;
                this.Repair += hp;
                spend = -spend;
            }
            this.Player.AddGold(spend);
            this.HasRepaired = !undo;
        }

        public Colony GetRepairedFrom()
        {
            TurnException.CheckTurn(this.Player);

            foreach (Tile neighbor in Tile.GetNeighbors(this.Tile))
            {
                Planet planet = ( neighbor.SpaceObject as Planet );
                if (planet != null && planet.Player == this.Player && planet.Colony.RepairShip == this)
                    return planet.Colony;
            }
            return null;
        }

        public double GetGoldForHP(double hp)
        {
            TurnException.CheckTurn(this.Player);

            return GetGoldForHP(hp, true);
        }
        private double GetGoldForHP(double hp, bool isTotal)
        {
            //the rounding logic here is reversed by:
            //private double GetHPForGold(double gold, bool isTotal)

            int floor = (int)Math.Floor(hp);
            double gold = CalcGoldForHP(floor);
            if (floor != hp)
                gold += ( CalcGoldForHP(floor + 1) - gold ) * ( hp - floor );

            if (!isTotal)
                gold /= hp;
            return gold;
        }
        public double CalcGoldForHP(int hp)
        {
            return Consts.GetGoldRepairCost(hp, this.MaxHP, this.RepairCost);
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
                //brute force algorithm to find the integer bounds
                upper = TBSUtil.FindValue(delegate(int hp)
                {
                    return ( GetGoldForHP(hp, isTotal) >= gold );
                }, 0, upper, true);

                if (upper > 0 && GetGoldForHP(upper, isTotal) > gold)
                {
                    //this reverses the rounding logic in:
                    //private double GetGoldForHP(double hp, bool isTotal)

                    double hp, low = CalcGoldForHP(upper - 1), high = CalcGoldForHP(upper);

                    if (isTotal)
                        hp = upper - 1 + ( gold - low ) / ( high - low );
                    else
                        hp = ( upper * ( low - high ) + high ) / ( ( low - high ) + gold );

                    if (Math.Abs(GetGoldForHP(hp, isTotal) - gold) > Consts.FLOAT_ERROR)
                        throw new Exception();
                    return hp;
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
            return GetHPForProd(production, true);
        }
        internal double GetHPForProd(double production, bool checkAutoRepair)
        {
            TurnException.CheckTurn(this.Player);

            double hp = production / RepairCost;
            double max = this.MaxHP - HP;
            if (checkAutoRepair && DoAutoRepair)
                max -= GetAutoRepairHP();
            if (hp > max)
                hp = max;
            return hp;
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
            return ( this.GetCostLastResearched() * ( 1 - Consts.CostUpkeepPct ) / (double)this.MaxHP );
        }

        public override string ToString()
        {
            return ShipNames.GetName(this.name, this.mark);
        }

        #endregion //public

        public enum ExpType
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
