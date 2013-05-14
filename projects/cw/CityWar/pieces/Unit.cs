using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using MattUtil;

namespace CityWar
{
    [Serializable]
    public class Unit : Piece, IDeserializationCallback
    {
        #region fields and constructors
        public readonly UnitType Type;
        public readonly int regen, maxHits;
        public readonly string Race;
        private double _regenPct;
        private bool recoverRegenPct;

        internal readonly CostType costType;
        internal readonly bool isThree;
        private readonly int cost, pplCost;

        private int _hits, armor;
        private Attack[] attacks;
        private bool randed;

        [NonSerialized]
        private double _randedCostMult;
        [NonSerialized]
        private int length;

        private Unit(string race, string name, Tile tile, Player owner, int cost, int pplCost, CostType costType, Abilities abil,
            bool isThree, UnitType Type, int hits, int armor, int regen, int movement, Attack[] attacks, double _regenPct)
            : base(movement, owner, tile)
        {
            this.Race = race;
            this.ability = abil;
            this.name = name;
            this.cost = cost;
            this.costType = costType;
            this.Type = Type;
            this.maxHits = hits;
            this.armor = armor;
            this.regen = regen;
            this.isThree = isThree;
            this.pplCost = pplCost;
            this._regenPct = _regenPct;
            this.recoverRegenPct = true;

            this.randed = false;

            this.attacks = attacks;
            this.hits = hits;
            CheckAttacks();
            foreach (Attack attack in this.attacks)
                attack.SetOwner(this);

            this.OnDeserialization(null);
        }
        #endregion //fields and constructors

        #region overrides
        public override string Name
        {
            get
            {
                if (isThree)
                    return base.Name + " (" + attacks.Length + ")";

                return base.Name;
            }
        }
        #endregion //overrides

        #region public methods and properties
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
                return BaseCost * randedCostMult;
            }
        }

        public double InverseCost
        {
            get
            {
                return BaseCost / randedCostMult;
            }
        }

        public int BaseCost
        {
            get
            {
                return BaseOtherCost + BasePplCost;
            }
        }

        public int BasePplCost
        {
            get
            {
                return pplCost;
            }
        }

        public int BaseOtherCost
        {
            get
            {
                return cost;
            }
        }

        public Attack[] Attacks
        {
            get
            {
                return (Attack[])attacks.Clone();
            }
        }

        public int Hits
        {
            get
            {
                return hits;
            }
        }

        public bool Dead
        {
            get
            {
                return hits < 1;
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
                return armor + tile.GetArmorBonus(Type);
            }
        }

        public int BaseRegen
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
                double heal = regen * regenPct;
                int result = (int)heal;
                if (owner.HealRound < ( heal % 1 ))
                    ++result;
                return result;
            }
        }

        internal double WorkRegen
        {
            get
            {
                return ( BaseRegen + Regen ) * .5;
            }
        }

        public bool RegenRecover
        {
            get
            {
                return this.recoverRegenPct;
            }
        }

        public double GetDisbandAmount()
        {
            double healthPct = GetHealthPct();
            return InverseCost / Attack.DeathDivide * ( 1 - healthPct ) + InverseCost / Attack.DisbandDivide * healthPct;
        }

        public double GetHealthPct()
        {
            return hits / (double)maxHits;
        }
        #endregion //public methods and properties

        #region internal methods
        internal bool CaptureCity()
        {
            if (movement < 1 || tile.CityTime < 0 || movement < MaxMove || tile.MadeCity)
                return false;

            movement = 0;
            return tile.CaptureCity(this);
        }
        internal void UndoCaptureCity()
        {
            movement = MaxMove;
            tile.UndoCaptureCity(this);
        }

        internal void Attacked(int length)
        {
            this.length = Math.Min(this.Length, length);
            this.recoverRegenPct = false;
        }

        internal void Wound(int damage)
        {
            this.recoverRegenPct = false;

            hits -= damage;
            if (Dead)
            {
                EndBattle();
                Die();
            }
            else
            {
                SetRegenPct(regenPct * ( 1 - damage / (double)( damage + hits ) ));
                if (isThree)
                    tile.hasCenterPiece = false;
                tile.AdjustPiece(this);
            }
        }
        private void Die()
        {
            owner.AddDeath(InverseCost / Attack.DeathDivide);
            owner.AddWork(WorkRegen * movement);
            Tile.Remove(this);
            Owner.Remove(this, false);
        }

        internal override double Heal()
        {
            if (movement > 0)
            {
                --movement;

                int regen = Regen;
                owner.AddWork(( BaseRegen - regen ) * .5);

                hits += regen;
                double pctWork = 0;
                if (hits > maxHits)
                {
                    pctWork = ( hits - maxHits ) / (double)regen;
                    owner.AddWork(hits - maxHits);
                    hits = maxHits;
                }
                if (isThree)
                    tile.hasCenterPiece = false;
                tile.AdjustPiece(this);

                return pctWork;
            }
            return -1;
        }
        internal override void UndoHeal(double pctWork)
        {
            ++movement;

            int regen = Regen;
            owner.AddWork(-( BaseRegen - regen ) * .5);

            int work = Game.Random.Round(pctWork * regen);
            Owner.AddWork(-work);
            regen -= work;

            hits -= regen;
            if (Dead)
            {
                while (movement > 0 && Dead)
                    Heal();
                if (Dead)
                    Die();
            }
            else if (isThree)
            {
                tile.hasCenterPiece = false;
                tile.AdjustPiece(this);
            }
        }

        internal override void ResetMove()
        {
            ResetMoveIntern(true);
        }
        private void ResetMoveIntern(bool checkAircraft)
        {
            if (!randed)
            {
                RandStats();
            }
            else if (checkAircraft && ability == Abilities.Aircraft && !tile.HasCarrier())
            {
                Disband();
            }
            else
            {
                if (MaxMove < 1 && hits < maxHits)
                {
                    //healing when you have no max movement happens automatically but costs a few resources
                    int newHits = hits + Regen;
                    if (newHits > maxHits)
                        newHits = maxHits;

                    double needed = ( newHits - hits ) / (double)maxHits / 3.0;
                    double pplNeeded = needed * BasePplCost * randedCostMult;
                    needed *= BaseOtherCost * randedCostMult;

                    owner.Spend(Game.Random.Round(needed), costType, Game.Random.Round(pplNeeded));

                    hits = newHits;
                }

                while (movement > 0)
                    Heal();

                if (this.recoverRegenPct)
                    SetRegenPct(Math.Pow(regenPct, .39));
                this.recoverRegenPct = true;

                movement = MaxMove;

                if (isThree)
                    tile.hasCenterPiece = false;
                tile.AdjustPiece(this);
            }
        }

        internal Stack<double> Disband()
        {
            Stack<double> result = new Stack<double>();

            while (movement > 0)
                result.Push(Heal());

            owner.AddDeath(GetDisbandAmount());

            tile.Remove(this);
            owner.Remove(this, false);

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

        internal int Length
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
                        double greenPct = Math.Pow(( 2.0 * pct - 1.0 ) * sign, power) * sign / 2.0 + 1.0 / 2.0;

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
        internal int hits
        {
            get
            {
                return _hits;
            }
            set
            {
                if (( _hits == value ) || ( ( _hits = value ) < 0 ))
                    return;
                CheckAttacks();
                if (_healthBrush != null)
                {
                    _healthBrush.Dispose();
                    _healthBrush = null;
                }
            }
        }

        private double randedCostMult
        {
            get
            {
                if (_randedCostMult < 0)
                    GetRandedCostMult();

                return _randedCostMult;
            }
        }

        private void GetRandedCostMult()
        {
            _randedCostMult = Balance.getCost(MaxMove, regen, Abilty, BaseArmor, Type, Attacks, isThree, maxHits) / (double)( BaseCost );
        }

        public double regenPct
        {
            get
            {
                return _regenPct;
            }
        }

        private void SetRegenPct(double value)
        {
            _regenPct = value + Game.Random.GaussianCapped(1, Math.Abs(_regenPct - value) * .21, 1 - Math.Min(value, 1 - value)) - 1;
        }

        private void CheckAttacks()
        {
            if (isThree)
            {
                double used = 0;
                foreach (Attack a in attacks)
                    if (a.Used)
                        ++used;
                used /= attacks.Length;
                Attack attack = attacks[0].Clone();
                int numAttacks = ( 3 * hits + maxHits - 1 ) / maxHits;
                attacks = new Attack[numAttacks];
                for (int a = -1 ; ++a < numAttacks ; )
                {
                    attacks[a] = attack.Clone();
                    attacks[a].Used = Game.Random.Bool(used);
                }
            }
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
            UnitSchema schema = UnitTypes.GetSchema();
            UnitSchema.UnitRow unitRow = schema.Unit.FindByName(name);

            Abilities ability;
            if (unitRow.Special == "AircraftCarrier")
                ability = Abilities.AircraftCarrier;
            else if (unitRow.Special == "Aircraft")
                ability = Abilities.Aircraft;
            else
                ability = Abilities.None;

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
            for (int i = 0 ; i < numAttacks ; ++i)
            {
                UnitSchema.AttackRow attackRow = attackRows[i];

                EnumFlags<TargetType> targets = new EnumFlags<TargetType>();

                if (attackRow.Target_Type.Contains("G"))
                    targets.Add(TargetType.Ground);
                if (attackRow.Target_Type.Contains("W"))
                    targets.Add(TargetType.Water);
                if (attackRow.Target_Type.Contains("A"))
                    targets.Add(TargetType.Air);

                attacks[i] = new Attack(attackRow.Name, targets, attackRow.Length, attackRow.Damage, attackRow.Divide_By);
            }

            Unit unit = new Unit(unitRow.Race, name, tile, owner, unitRow.Cost, unitRow.People, costType, ability,
                unitRow.IsThree, type, unitRow.Hits, unitRow.Armor, unitRow.Regen, unitRow.Move, attacks, 1);

            if (add)
            {
                tile.Add(unit);
                owner.Add(unit);
            }

            return unit;
        }
        #endregion //new units

        #region unit stat randomization
        private void RandStats()
        {
            Unit newUnit;
            const double maxMult = 1.69;
            do
            {
                Attack[] randedAttacks = new Attack[attacks.Length];
                for (int a = 0 ; a < randedAttacks.Length ; ++a)
                    randedAttacks[a] = attacks[a].Clone().RandStats();
                newUnit = new Unit(Race, name, tile, owner, cost, pplCost, costType, ability, isThree, Type,
                        isThree ? 3 * RandStat(maxHits / 3.0, true) : RandStat(maxHits, true),
                        RandStat(armor, false), RandStat(regen, true), MaxMove, randedAttacks, regenPct);
            }
            while (newUnit.randedCostMult > maxMult || newUnit.randedCostMult < ( 1 / maxMult ));

            tile.Add(newUnit);
            owner.Add(newUnit);

            newUnit.hits = Game.Random.Round(GetHealthPct() * newUnit.maxHits);
            newUnit.movement = movement;
            newUnit.group = group;

            tile.Remove(this);
            owner.Remove(this, false);

            newUnit.randed = true;
            newUnit.ResetMoveIntern(false);

            if (newUnit.Dead)
                newUnit.Die();

            tile.hasCenterPiece = false;
        }
        internal static int RandStat(double stat, bool keepPositive)
        {
            int lowerCap = Game.Random.Round(stat * .65);
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
            bool any = false;
            foreach (Unit u in attackers)
            {
                any = true;
                //make all attackers untargetable
                u.length = int.MaxValue;
            }
            if (!any)
                return null;

            any = false;
            foreach (Unit u in defenders)
            {
                any = true;
                //make all defenders targetable
                u.length = int.MinValue;
            }
            if (!any)
                return null;

            return new Battle(attackers, defenders);
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
            int usedAttacks = 0;
            //reset attacks
            foreach (Attack a in attacks)
                if (a.Used)
                {
                    a.Used = false;
                    ++usedAttacks;
                }
            if (usedAttacks > 0 && Owner == this.Owner.Game.CurrentPlayer)
            {
                //only reduce movement if any attacks were used
                --movement;
                Owner.AddWork(Attack.OverkillPercent * WorkRegen * ( attacks.Length - usedAttacks ) / (double)attacks.Length);
            }
        }
        #endregion //start and end battle

        #region moving
        internal static bool UnitGroupMove(List<Unit> units, Tile t, Dictionary<Piece, bool> undoPieces, bool gamble)
        {
            if (units.Count < 2)
            {
                //no need to move by group
                Piece move = units[0];
                bool canUndo;
                bool moved = move.Move(t, gamble, out canUndo);
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
                //just subtract the value that was used to roll for success
                foreach (Unit u in units)
                    u.movement -= minMove;

                //either move everyone or no one
                bool move = Game.Random.Bool(minMove / (double)needed);
                foreach (Unit u in units)
                {
                    if (move)
                        u.ActualMove(t);
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
                    bool canUndo;
                    u.Move(t, gamble, out canUndo);
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
                move = ( Game.Random.Bool(useMove / (double)needed) );
                //cant undo a random move
                canUndo = false;
                movement -= useMove;
            }
            else
            {
                move = true;
                movement -= needed;
            }

            if (move)
                canUndo &= ActualMove(t);

            return move;
        }
        private bool ActualMove(Tile t)
        {
            tile.Remove(this);
            t.Add(this);
            this.tile = t;
            tile.CurrentGroup = group;

            return tile.CheckCapture(owner);
        }

        protected override bool CanMoveChild(Tile t)
        {
            Player p;
            return ( GetNeeded(t) > -1 && ( ability != Abilities.Aircraft || !t.Occupied(out p) || p == owner ) );
        }

        private int GetNeeded(Tile t)
        {
            //this method can be called for any type of move, so check all possibilities
            int needed;
            if (t.HasCity())
            {
                needed = 1;
            }
            else if (Type == UnitType.Air || Type == UnitType.Immobile)
            {
                needed = 1;
            }
            else if (Type == UnitType.Ground && t.Terrain != Terrain.Water)
            {
                switch (t.Terrain)
                {
                case Terrain.Plains:
                    needed = 1;
                    break;
                case Terrain.Forest:
                    needed = 2;
                    break;
                case Terrain.Mountain:
                    needed = 3;
                    break;
                default:
                    throw new Exception();
                }
            }
            else if (Type == UnitType.Water && t.Terrain == Terrain.Water)
            {
                needed = 1;
            }
            else if (Type == UnitType.Amphibious)
            {
                switch (t.Terrain)
                {
                case Terrain.Water:
                    needed = 1;
                    break;
                case Terrain.Plains:
                    needed = 1;
                    break;
                case Terrain.Forest:
                    needed = 2;
                    break;
                case Terrain.Mountain:
                    needed = 3;
                    break;
                default:
                    throw new Exception();
                }
            }
            else
            {
                //not the right unit type to move on that terrain
                needed = -1;
            }
            return needed;
        }
        #endregion //moving

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            this._randedCostMult = -1;
            this.length = int.MaxValue;
            this._healthBrush = null;
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
