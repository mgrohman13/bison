using MattUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace CityWar
{
    [Serializable]
    public class Unit : Piece, IDeserializationCallback
    {
        #region fields and constructors

        //power to which regeneration percent is raised every turn that it recovers
        public const double RegenRecoverPower = .39;

        public const double FuelWorkCost = .39;

        //only used during a battle
        [NonSerialized]
        private int length;

        public readonly UnitType Type;
        public readonly CostType CostType;
        public readonly string Race;
        public readonly bool IsThree;
        public readonly int BaseOtherCost, BasePplCost;

        private Attack[] attacks;
        private Attack[] isThreeAttacks;
        private bool randed;
        private int maxHits, armor, regen;
        private bool recoverRegen;

        private int shield, fuel, maxFuel;

        private int _hits;
        private double _regenPct;

        private Unit(string race, string name, Tile tile, Player owner, int otherCost, int pplCost, CostType costType,
                EnumFlags<Ability> abilities, int shield, int fuel,
                bool isThree, UnitType Type, int hits, int armor, int regen, int move, Attack[] attacks)
            : base(move, owner, tile, name, abilities)
        {
            this.Type = Type;
            this.CostType = costType;
            this.Race = race;
            this.IsThree = isThree;
            this.BaseOtherCost = otherCost;
            this.BasePplCost = pplCost;
            this.maxFuel = fuel;

            this.attacks = attacks;
            this.randed = false;
            this.maxHits = hits;
            this.armor = armor;
            this.regen = regen;
            this.recoverRegen = true;

            this.shield = shield;
            this.Fuel = fuel;

            this._hits = hits;
            this._regenPct = 1;

            OnDeserialization(null);

            foreach (Attack attack in this.attacks)
                attack.SetOwner(this);
        }

        #endregion //fields and constructors

        #region overrides

        internal string Name
        {
            get
            {
                return base.ToString();
            }
        }
        public override string ToString()
        {
            if (IsThree)
                return Name + " (" + attacks.Length + ")";
            return Name;
        }

        #endregion //overrides

        #region public methods and properties

        public int MaxHits
        {
            get
            {
                return maxHits;
            }
        }
        public bool IsRanded
        {
            get
            {
                return randed;
            }
        }

        public double RandedCost
        {
            get
            {
                return BaseTotalCost * RandedCostMult;
            }
        }

        public double InverseCost
        {
            get
            {
                return BaseTotalCost / RandedCostMult;
            }
        }

        public int BaseTotalCost
        {
            get
            {
                return BaseOtherCost + BasePplCost;
            }
        }

        public Attack[] Attacks
        {
            get
            {
                return (Attack[])attacks.Clone();
            }
        }

        public bool Dead
        {
            get
            {
                return (Hits <= 0 || !owner.GetPieces().Contains(this));
            }
        }

        public int BaseArmor
        {
            get
            {
                return armor;
            }
        }

        public int Armor
        {
            get
            {
                return BaseArmor + tile.GetArmorBonus(Type);
            }
        }

        public int Shield
        {
            get
            {
                return shield;
            }
        }

        public int MaxRegen
        {
            get
            {
                return regen;
            }
        }

        public int Regen
        {
            get
            {
                double heal = MaxRegen * RegenPct;
                int result = (int)heal;
                if (owner.HealRound < (heal % 1))
                    ++result;
                return result;
            }
        }

        public double WorkRegen
        {
            get
            {
                return (MaxRegen + Regen) * .5;
            }
        }

        public bool RegenRecover
        {
            get
            {
                return this.recoverRegen;
            }
        }

        public int Fuel
        {
            get
            {
                return fuel;
            }
            internal set
            {
                fuel = value;
            }
        }
        public int MaxFuel
        {
            get
            {
                return maxFuel;
            }
        }

        public static int GetTotalDamageShield(Unit attacker, Unit defender)
        {
            bool submerge = IsSubmerged(attacker.Type, defender.abilities) && defender.Tile.Terrain == Terrain.Water;
            return submerge ? 50 : defender.Shield;
        }
        public static bool IsSubmerged(UnitType attackerType, EnumFlags<Ability> defenderAbilities)
        {
            return defenderAbilities.Contains(Ability.Submerged) && (attackerType == UnitType.Air || attackerType == UnitType.Ground);
        }

        public double GetDisbandAmount()
        {
            double healthPct = GetHealthPct();
            return InverseCost / Attack.DeathDivide * (1 - healthPct) + InverseCost / Attack.DisbandDivide * healthPct;
        }

        public double GetHealthPct()
        {
            return Hits / (double)MaxHits;
        }

        #endregion //public methods and properties

        #region internal methods

        internal bool CollectTreasure()
        {
            if (IsAir() || movement < 1 || movement < MaxMove || tile.Treasure == null || tile.Treasure.Collected || !tile.Treasure.UnitCollect)
                return false;

            movement = 0;
            return tile.CollectTreasure(this);
        }
        internal bool UndoCollectTreasure(Treasure treasure)
        {
            movement = MaxMove;
            return tile.UndoCollectTreasure(this, treasure);
        }

        internal void Attacked(int length)
        {
            this.length = Math.Min(this.Length, length);
            this.recoverRegen = false;
        }

        internal void Wound(int damage)
        {
            this.recoverRegen = false;

            Hits -= damage;
            if (Dead)
            {
                EndBattle();
                Die();
            }
            else
            {
                this.RegenPct *= 1 - damage / (double)(damage + Hits);
                if (IsThree)
                    tile.hasCenterPiece = false;
                tile.AdjustPiece(this);
            }
        }
        private void Die()
        {
            owner.AddDeath(InverseCost / Attack.DeathDivide);
            owner.AddWork(WorkRegen * movement);
            if (IsAir())
                owner.AddWork(Fuel * FuelWorkCost);
            RegenPct = 1;
            double work = RegenPctCost(1) * Player.WorkMult / Player.UpkeepMult;
            owner.AddWork(work);

            Tile.Remove(this);
            Owner.Remove(this);
        }

        internal override double Heal()
        {
            if (movement > 0)
            {
                --movement;
                return HealOne();
            }
            return -1;
        }
        private double HealOne()
        {
            int regen = Regen;
            owner.AddWork((MaxRegen - regen) * .5);

            double pctWork = 1;
            if (IsAir() && !tile.HasCarrier())
                owner.AddWork(regen);
            else
                Heal(regen, out pctWork);
            return pctWork;
        }
        private void Heal(int regen, out double pctWork)
        {
            pctWork = 0;
            Hits += regen;
            if (Hits > MaxHits)
            {
                pctWork = (Hits - MaxHits) / (double)regen;
                owner.AddWork(Hits - MaxHits);
                Hits = MaxHits;
            }
            if (IsThree)
                tile.hasCenterPiece = false;
            tile.AdjustPiece(this);
        }

        internal override void UndoHeal(double pctWork)
        {
            ++movement;

            int regen = Regen;
            owner.AddWork(-(MaxRegen - regen) * .5);

            int work = Game.Random.Round(pctWork * regen);
            Owner.AddWork(-work);
            regen -= work;

            Hits -= regen;
            if (Dead)
            {
                while (movement > 0 && Dead)
                    Heal();
                if (Dead)
                    Die();
            }
            else if (IsThree)
            {
                tile.hasCenterPiece = false;
                tile.AdjustPiece(this);
            }
        }

        internal override void ResetMove()
        {
            if (!randed)
                RandStats();
            if (!Dead)
            {
                foreach (Attack attack in this.attacks)
                    attack.RandStats();
                if (IsThree && isThreeAttacks == null)
                {
                    isThreeAttacks = new[] { attacks[1] ?? GetAttack(GetAttackRow()), attacks[2] ?? GetAttack(GetAttackRow()) };
                    foreach (Attack attack in this.isThreeAttacks)
                        attack.RandStats();
                }

                if (IsAir())
                {
                    if (tile.HasCarrier())
                    {
                        double upkeep = (maxFuel - Fuel) * FuelWorkCost * Player.UpkeepMult / Player.WorkMult;
                        owner.AddUpkeep(upkeep, .169);
                        Fuel = maxFuel;
                    }
                    else if (Fuel <= 0)
                        Disband();
                    else
                        Fuel--;
                }

                if (!Dead)
                {
                    if (MaxMove < 1 && Hits < MaxHits && Regen > 0)
                    {
                        //healing when you have no max movement happens automatically but costs a few resources
                        int newHits = Hits + Regen;
                        if (newHits > MaxHits)
                            newHits = MaxHits;

                        double needed = (newHits - Hits) / (double)MaxHits / 3.9;
                        double pplNeeded = needed * BasePplCost * RandedCostMult;
                        needed *= BaseOtherCost * RandedCostMult;

                        owner.Spend(Game.Random.Round(needed), CostType, Game.Random.Round(pplNeeded));

                        Hits = newHits;
                    }

                    while (movement > 0)
                        Heal();
                    if (IsAbility(Ability.Regen))
                        HealOne();

                    if (this.recoverRegen)
                    {
                        this.RegenPct = Math.Pow(RegenPct, RegenRecoverPower);
                    }
                    else
                    {
                        if (IsAbility(Ability.Regen))
                            this.RegenPct = Math.Pow(RegenPct, .91);
                        this.recoverRegen = true;
                    }

                    movement = MaxMove;

                    tile.AdjustPiece(this);
                }
            }

            tile.hasCenterPiece = false;

            //base.ResetMove();
        }

        internal Stack<double> Disband()
        {
            Stack<double> result = new();

            while (movement > 0)
                result.Push(Heal());

            owner.AddDeath(GetDisbandAmount());

            tile.Remove(this);
            owner.Remove(this);

            return result;
        }
        internal void UndoDisband(Stack<double> healInfo)
        {
            owner.Add(this);
            tile.Add(this);

            owner.AddDeath(-GetDisbandAmount());

            while (healInfo.Count > 0)
            {
                double intInfo = healInfo.Pop();
                if (intInfo > -1)
                    UndoHeal(intInfo);
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
        }

        #endregion //internal methods

        #region hits

        [NonSerialized]
        private Brush _healthBrush;
        public Brush HealthBrush
        {
            get
            {
                if (_healthBrush == null)
                {
                    double pct = GetHealthPct();
                    if (pct < 1)
                    {
                        const double power = .666;

                        double sign = Math.Sign(pct - .5);
                        double greenPct = Math.Pow((2.0 * pct - 1.0) * sign, power) * sign / 2.0 + 1.0 / 2.0;

                        _healthBrush = new SolidBrush(Color.FromArgb(255, Game.Random.Round(255 * greenPct), 0));
                    }
                    else
                    {
                        _healthBrush = new SolidBrush(Color.FromArgb(0, 255, 0));
                    }
                }
                return _healthBrush;
            }
        }

        public int Hits
        {
            get
            {
                return this._hits;
            }
            internal set
            {
                if (this._hits != value)
                {
                    this._hits = value;
                    if (!Dead)
                    {
                        CheckAttacks();
                        if (_healthBrush != null)
                        {
                            _healthBrush.Dispose();
                            _healthBrush = null;
                        }
                    }
                }
            }
        }

        [NonSerialized]
        private double _randedCostMult = double.NaN;
        private double RandedCostMult
        {
            get
            {
                if (double.IsNaN(_randedCostMult))
                    _randedCostMult = GetCost(out _);
                return _randedCostMult;
            }
        }

        public double GetCost(out double gc)
        {
            Attack[] attacks;
            if (IsThree && this.attacks.Length < 3)
            {
                attacks = new Attack[3];
                for (int a = 0; a < this.attacks.Length; ++a)
                    attacks[a] = this.attacks[a];
                for (int b = this.attacks.Length; b < 3; ++b)
                    attacks[b] = GetAttack(b);
            }
            else
            {
                attacks = this.attacks;
            }

            return Balance.GetCost(Game.UnitTypes, Race, Type, IsThree, Abilities, shield, maxFuel,
                    MaxHits, BaseArmor, MaxRegen, MaxMove, attacks, out gc) / (double)(BaseTotalCost);
        }

        public double RegenPct
        {
            get
            {
                return this._regenPct;
            }
            set
            {
                if (value > float.Epsilon)
                    value = Game.Random.GaussianCapped(value,
                            Math.Abs(this._regenPct - value) / value * .169, Math.Max(float.Epsilon, 2 * value - 1));
                else
                    value = Math.Max(value, double.Epsilon);

                double pay = value - this._regenPct;
                if (pay > 0)
                    owner.AddUpkeep(RegenPctCost(pay));

                this._regenPct = value;
            }
        }
        private double RegenPctCost(double pct)
        {
            pct *= MaxMove * MaxRegen * 2.6;
            pct *= Player.UpkeepMult / Player.WorkMult;
            return pct;
        }

        private void CheckAttacks()
        {
            if (IsThree)
            {
                int curAttacks = this.attacks.Length;
                int newAttacks = (3 * Hits + MaxHits - 1) / MaxHits;

                if (curAttacks > newAttacks)
                {
                    this.attacks = this.attacks.Take(newAttacks).ToArray();
                }
                else if (curAttacks < newAttacks && newAttacks <= 3)
                {
                    Attack[] attacks = new Attack[newAttacks];
                    for (int a = 0; a < curAttacks; ++a)
                        attacks[a] = this.attacks[a];
                    for (int b = curAttacks; b < newAttacks; ++b)
                    {
                        attacks[b] = GetAttack(b);
                        attacks[b].SetOwner(this);
                    }
                    this.attacks = attacks;
                }
            }
        }
        private UnitSchema.AttackRow GetAttackRow()
        {
            UnitSchema unitSchema = Game.UnitTypes.GetSchema();
            UnitSchema.UnitRow unitRow = unitSchema.Unit.FindByName(Name);
            return unitRow.GetAttackRows()[0];
        }

        #endregion //hits

        #region new units

        public static Unit CreateTempUnit(string name)
        {
            return NewUnit(name, null, null, false);
        }
        internal static Unit NewUnit(string name, Tile tile, Player owner)
        {
            return NewUnit(name, tile, owner, true);
        }
        private static Unit NewUnit(string name, Tile tile, Player owner, bool add)
        {
            UnitSchema schema = Game.UnitTypes.GetSchema();
            UnitSchema.UnitRow unitRow = schema.Unit.FindByName(name);

            CostType costType;
            if (unitRow.CostType == "A")
                costType = CostType.Air;
            else if (unitRow.CostType == "D")
                costType = CostType.Death;
            else if (unitRow.CostType == "E")
                costType = CostType.Earth;
            else if (unitRow.CostType == "N")
                costType = CostType.Nature;
            else if (unitRow.CostType == "W")
                costType = CostType.Water;
            else
                costType = CostType.Production;

            UnitType type;
            if (unitRow.Type == "A")
                type = UnitType.Air;
            else if (unitRow.Type == "GWA")
                type = UnitType.Immobile;
            else if (unitRow.Type == "GW")
                type = UnitType.Amphibious;
            else if (unitRow.Type == "G")
                type = UnitType.Ground;
            else if (unitRow.Type == "W")
                type = UnitType.Water;
            else
                throw new Exception();

            UnitSchema.AttackRow[] attackRows = unitRow.GetAttackRows();
            int numAttacks = attackRows.Length;
            Attack[] attacks = new Attack[numAttacks];
            for (int i = 0; i < numAttacks; ++i)
                attacks[i] = GetAttack(attackRows[i]);

            EnumFlags<Ability> abilities = GetAbilities(unitRow, out int shield, out int fuel);
            Unit unit = new(unitRow.Race, name, tile, owner, unitRow.Cost, unitRow.People, costType, abilities, shield, fuel,
                    unitRow.IsThree, type, unitRow.Hits, unitRow.Armor, unitRow.Regen, unitRow.Move, attacks);

            if (add)
            {
                owner.Add(unit);
                tile.Add(unit);

                if (unit.IsAir())
                    owner.AddUpkeep(fuel * FuelWorkCost * Player.UpkeepMult / Player.WorkMult);
            }

            return unit;
        }
        public static EnumFlags<Ability> GetAbilities(UnitSchema.UnitRow unit, out int shield, out int fuel)
        {
            shield = 0;
            fuel = int.MaxValue;
            EnumFlags<Ability> abilities = new();
            foreach (UnitSchema.SpecialRow s in unit.GetSpecialRows())
            {
                if (s.Special == "Aircraft")
                    fuel = s.Value;
                else if (s.Special == "Shield")
                    shield = s.Value;
                abilities.Add((Ability)Enum.Parse(typeof(Ability), s.Special));
            }
            return abilities;
        }
        private Attack GetAttack(int a)
        {
            return GetAttack(isThreeAttacks == null ? GetAttackRow() : null, isThreeAttacks, a);
        }
        private static Attack GetAttack(UnitSchema.AttackRow attackRow)
        {
            return GetAttack(attackRow, null, -1);
        }
        private static Attack GetAttack(UnitSchema.AttackRow attackRow, Attack[] isThreeAttacks, int a)
        {
            if (isThreeAttacks != null)
                return isThreeAttacks[a - 1];
            else
                return new Attack(attackRow.Name, UnitTypes.GetAttackTargets(attackRow.Target_Type),
                    attackRow.Length, attackRow.Damage, attackRow.Divide_By, UnitTypes.GetAttackSpecial(attackRow.Special));
        }

        #endregion //new units

        #region unit stat randomization

        private void RandStats()
        {
            const double maxMult = 1.3;

            if (IsAbility(Ability.Shield))
            {
                bool flip = shield > 50;
                if (flip)
                    shield = 100 - shield;
                shield = RandStat(shield, true);
                if (flip)
                    shield = 100 - shield;
            }
            if (IsAir())
                maxFuel = RandStat(maxFuel, true, MaxMove * 2);

            this.armor = RandStat(this.armor, false);
            this.regen = RandStat(this.regen, true);

            int oldHits = this.maxHits;
            int newHits = (IsThree ? 3 * RandStat(oldHits / 3.0, true) : RandStat(oldHits, true));
            int hitInc = (IsThree ? 3 : 1);
            do
            {
                this.maxHits = newHits;
                newHits -= hitInc;
            } while (newHits > 0 && RandedCostMult > maxMult);
            while (RandedCostMult < (1 / maxMult))
            {
                newHits += hitInc;
                this.maxHits = newHits;
            }

            Hits = Game.Random.Round(Hits * this.maxHits / (double)oldHits);
            if (Dead)
                Die();

            this.randed = true;
            this._randedCostMult = double.NaN;
        }
        internal static int RandStat(double stat, bool keepPositive, int lowerCap = int.MinValue)
        {
            lowerCap = Math.Max(lowerCap, Game.Random.Round(stat * .65));
            if (keepPositive)
            {
                if (lowerCap < 1)
                    lowerCap = 1;
            }
            else
            {
                --lowerCap;
            }
            return Game.Random.GaussianCappedInt(stat + 1, .078, lowerCap + 1) - 1;
        }

        #endregion //unit stat randomization

        #region start and end battle

        internal static Battle StartBattle(IEnumerable<Unit> attackers, IEnumerable<Unit> defenders)
        {
            //make all attackers untargetable
            foreach (Unit u in attackers)
                u.length = int.MaxValue;
            //make all defenders targetable
            foreach (Unit u in defenders)
                u.length = int.MinValue;
            return new Battle(attackers, defenders);
        }

        internal void StartRetaliation()
        {
            int usedAttacks = attacks.Where(a => a.Used).Count();
            if (usedAttacks > 0 && Owner == this.Owner.Game.CurrentPlayer)
            {
                //only reduce movement if any attacks were used
                --movement;
                if (IsAir())
                    --Fuel;
                //double work = owner.WorkRegen * overkill * OverkillPercent * 1 / (double)owner.Attacks.Length;
                double work = WorkRegen * 1 * Attack.OverkillPercent * (attacks.Length - usedAttacks) / (double)attacks.Length;
                Owner.AddWork(work);
            }
        }

        internal static void EndBattle(Battle b)
        {
            foreach (Unit u in b.attackers)
                u.EndBattle();
            foreach (Unit u in b.defenders)
                u.EndBattle();
        }
        private void EndBattle()
        {
            //reset length
            length = int.MaxValue;
            //reset attacks
            foreach (Attack a in attacks)
                a.Used = false;
        }

        #endregion //start and end battle

        #region moving

        internal static bool UnitGroupMove(List<Unit> units, Tile t, Dictionary<Piece, bool> undoPieces, bool gamble)
        {
            if (units.Count < 2)
            {
                //no need to move by group
                Piece move = units[0];
                bool moved = move.Move(t, gamble, out bool canUndo);
                undoPieces.Add(move, canUndo);
                return moved;
            }

            int minMove;
            if (gamble)
            {
                minMove = 1;
            }
            else
            {
                minMove = int.MaxValue;
                foreach (Unit u in units)
                    minMove = Math.Min(minMove, u.movement);
            }

            //this method will only be called for forest or mountain moves
            int needed;
            if (t.Terrain == Terrain.Forest)
                needed = 2;
            else if (t.Terrain == Terrain.Mountain)
                needed = 3;
            else
                throw new Exception();

            if (minMove < needed)
            {
                //either move everyone or no one
                double chance = minMove / (double)needed;
                bool move = Game.Random.Bool(chance);
                foreach (Unit u in units)
                {
                    //subtract the value that was used to roll for success
                    u.movement -= minMove;
                    if (u.IsAir())
                        u.Fuel -= minMove;
                    if (move)
                        u.ActualMove(t);

                    u.BalanceForMove(minMove, needed, move);

                    //cant undo a random move
                    undoPieces.Add(u, false);
                }
                return move;
            }
            else
            {
                //everyone has enough move, so just do it
                foreach (Unit u in Game.Random.Iterate<Unit>(units))
                {
                    u.Move(t, gamble, out bool canUndo);
                    undoPieces.Add(u, canUndo);
                }
                return true;
            }
        }
        protected override bool DoMove(Tile t, bool gamble, out bool canUndo)
        {
            canUndo = true;
            if (movement < 1)
                return false;
            if (IsAir() && Fuel < 1)
                return false;

            int needed = GetNeeded(t);
            if (needed < 0)
                return false;

            int useMove;
            if (gamble)
                useMove = 1;
            else
                useMove = movement;

            bool move;
            if (useMove < needed)
            {
                movement -= useMove;

                double chance = useMove / (double)needed;
                move = Game.Random.Bool(chance);

                BalanceForMove(useMove, needed, move);

                //cant undo a random move
                canUndo = false;
            }
            else
            {
                useMove = needed;
                movement -= useMove;
                move = true;
            }
            if (IsAir())
                Fuel -= useMove;

            if (move)
                canUndo &= ActualMove(t);

            return move;
        }
        private void BalanceForMove(int used, int needed, bool moved)
        {
            if (moved)
            {
                double balance = 1.3 * Player.GetUpkeep(this) * (needed - used) / (double)MaxMove;
                owner.AddUpkeep(balance, .13);
            }
            else
            {
                double balance = .65 * WorkRegen * used;
                owner.AddWork(balance);
            }
        }
        private bool ActualMove(Tile t)
        {
            tile.Remove(this);
            tile = t;
            bool canUndo = tile.Add(this);

            tile.CurrentGroup = group;
            return canUndo && tile.CheckCapture(owner);
        }

        protected override bool CanMoveChild(Tile t)
        {
            return (GetNeeded(t) > -1 && (!IsAir() || !t.Occupied(out Player p) || p == owner));
        }

        private int GetNeeded(Tile t)
        {
            if (t.HasCity())
                return 1;

            if (Type == UnitType.Air || Type == UnitType.Immobile)
                return 1;

            if ((Type == UnitType.Water || Type == UnitType.Amphibious) && t.Terrain == Terrain.Water)
                return 1;

            if ((Type == UnitType.Ground || Type == UnitType.Amphibious) && t.Terrain != Terrain.Water)
                return t.Terrain switch
                {
                    Terrain.Plains => 1,
                    Terrain.Forest => 2,
                    Terrain.Mountain => 3,
                    _ => throw new Exception(),
                };

            //not the right unit type to move on that terrain
            return -1;
        }

        #endregion //moving

        #region pathfinding

        public static void PathFind(IEnumerable<Piece> pieces, Tile clicked, bool makeGroup = false)
        {
            int origCount = pieces.Count();
            pieces = pieces.Where(piece => piece.MaxMove > 0 && piece.Owner == piece.Owner.Game.CurrentPlayer
                    && (piece is not Unit unit || unit.GetNeeded(clicked) != -1));
            if (clicked == null || pieces.GroupBy(piece => piece.Tile).Count() != 1)
                return;

            var units = pieces.OfType<Unit>();
            var groups = units.Where(unit => unit.Type != UnitType.Air && unit.Type != UnitType.Immobile).GroupBy(unit => unit.Type);
            if (groups.Count(group => group.Key == UnitType.Ground || group.Key == UnitType.Water) == 2)
            {
                foreach (var group in Game.Random.Iterate(groups.Concat(new[] { pieces.Where(piece => piece is not Unit),
                        units.Where(unit => unit.Type == UnitType.Air || unit.Type == UnitType.Immobile) }.Where(group => group.Any()))))
                    PathFind(group, clicked, true);
            }
            else
            {
                Tile from = pieces.First().Tile;
                int newGroup;
                if (makeGroup || origCount != pieces.Count())
                    newGroup = Game.NewGroup();
                else
                    newGroup = from.CurrentGroup;

                List<Tile> tiles = TBSUtil.PathFind(Game.Random, from, clicked, tile => tile.GetNeighbors()
                        .Select(neighbor => new Tuple<Tile, int>(neighbor, GetWorstNeeded(pieces, neighbor)))
                        .Where(tuple => tuple.Item2 != -1), GetDistance);

                foreach (Piece piece in pieces)
                {
                    piece.Path = tiles;
                    piece.Group = newGroup;
                }
                from.CurrentGroup = newGroup;
            }
        }
        private static int GetWorstNeeded(IEnumerable<Piece> pieces, Tile neighbor)
        {
            if (pieces.First().Tile == neighbor)
                return 1;
            IEnumerable<int> needed = pieces.OfType<Unit>().Select(unit => unit.GetNeeded(neighbor));
            if (!needed.Any())
                return 1;
            if (needed.Contains(-1))
                return -1;
            return needed.Max();
        }
        private static int GetDistance(Tile t1, Tile t2)
        {
            int x1 = t1.X, y1 = t1.Y, x2 = t2.X, y2 = t2.Y;
            int yDist = Math.Abs(y2 - y1);
            int xDist = Math.Abs(x2 - x1) - yDist / 2;
            //determine if the odd y distance will save an extra x move or not
            if (xDist < 1)
                xDist = 0;
            else if ((yDist % 2 != 0) && ((y2 % 2 == 0) == (x2 < x1)))
                --xDist;
            return yDist + xDist;
        }

        #endregion //pathfinding

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            this.length = int.MaxValue;
            this._healthBrush = null;
            this._randedCostMult = double.NaN;
        }

        #endregion
    }

    [Serializable]
    public enum CostType
    {
        Air,
        Death,
        Earth,
        Nature,
        Production,
        Water
    }

    [Serializable]
    public enum UnitType
    {
        Ground,
        Water,
        Air,
        Amphibious,
        Immobile
    }
}
