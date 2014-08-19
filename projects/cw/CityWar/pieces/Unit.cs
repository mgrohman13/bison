using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.Serialization;
using MattUtil;

namespace CityWar
{
    [Serializable]
    public class Unit : Piece, IDeserializationCallback
    {
        #region fields and constructors

        //power to which regeneration percent is raised every turn that it recovers
        public const double RegenRecoverPower = .39;

        //only used during a battle
        [NonSerialized]
        private int length;

        public readonly UnitType Type;
        public readonly CostType CostType;
        public readonly string Race;
        public readonly bool IsThree;
        public readonly int BaseOtherCost, BasePplCost;

        private Attack[] attacks;
        private bool randed;
        private int maxHits, armor, regen;
        private bool recoverRegen;

        private int _hits;
        private double _regenPct;

        private Unit(string race, string name, Tile tile, Player owner, int otherCost, int pplCost, CostType costType, Abilities ability,
                bool isThree, UnitType Type, int hits, int armor, int regen, int move, Attack[] attacks)
            : base(move, owner, tile, name, ability)
        {
            this.Type = Type;
            this.CostType = costType;
            this.Race = race;
            this.IsThree = isThree;
            this.BaseOtherCost = otherCost;
            this.BasePplCost = pplCost;

            this.attacks = attacks;
            this.randed = false;
            this.maxHits = hits;
            this.armor = armor;
            this.regen = regen;
            this.recoverRegen = true;

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
                return BaseTotalCost * randedCostMult;
            }
        }

        public double InverseCost
        {
            get
            {
                return BaseTotalCost / randedCostMult;
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
                return ( hits <= 0 );
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
                double heal = MaxRegen * regenPct;
                int result = (int)heal;
                if (owner.HealRound < ( heal % 1 ))
                    ++result;
                return result;
            }
        }

        public double WorkRegen
        {
            get
            {
                return ( MaxRegen + Regen ) * .5;
            }
        }

        public bool RegenRecover
        {
            get
            {
                return this.recoverRegen;
            }
        }

        public double GetDisbandAmount()
        {
            double healthPct = GetHealthPct();
            return InverseCost / Attack.DeathDivide * ( 1 - healthPct ) + InverseCost / Attack.DisbandDivide * healthPct;
        }

        public double GetHealthPct()
        {
            return hits / (double)MaxHits;
        }

        #endregion //public methods and properties

        #region internal methods

        internal bool CaptureCity()
        {
            if (Ability == Abilities.Aircraft || movement < 1 || tile.CityTime < 0 || movement < MaxMove || tile.MadeCity)
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
            this.recoverRegen = false;
        }

        internal void Wound(int damage)
        {
            this.recoverRegen = false;

            hits -= damage;
            if (Dead)
            {
                EndBattle();
                Die();
            }
            else
            {
                this.regenPct = ( regenPct * ( 1 - damage / (double)( damage + hits ) ) );
                if (IsThree)
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
                owner.AddWork(( MaxRegen - regen ) * .5);

                hits += regen;
                double pctWork = 0;
                if (hits > MaxHits)
                {
                    pctWork = ( hits - MaxHits ) / (double)regen;
                    owner.AddWork(hits - MaxHits);
                    hits = MaxHits;
                }
                if (IsThree)
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
            owner.AddWork(-( MaxRegen - regen ) * .5);

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
            else if (IsThree)
            {
                tile.hasCenterPiece = false;
                tile.AdjustPiece(this);
            }
        }

        internal override void ResetMove()
        {
            if (Ability == Abilities.Aircraft && !tile.HasCarrier())
            {
                Disband();
            }
            else
            {
                if (!randed)
                    RandStats();

                if (!Dead)
                {
                    if (MaxMove < 1 && hits < MaxHits && Regen > 0)
                    {
                        //healing when you have no max movement happens automatically but costs a few resources
                        int newHits = hits + Regen;
                        if (newHits > MaxHits)
                            newHits = MaxHits;

                        double needed = ( newHits - hits ) / (double)MaxHits / 3.9;
                        double pplNeeded = needed * BasePplCost * randedCostMult;
                        needed *= BaseOtherCost * randedCostMult;

                        owner.Spend(Game.Random.Round(needed), CostType, Game.Random.Round(pplNeeded));

                        hits = newHits;
                    }

                    while (movement > 0)
                        Heal();

                    if (this.recoverRegen)
                        this.regenPct = ( Math.Pow(regenPct, RegenRecoverPower) );
                    this.recoverRegen = true;

                    movement = MaxMove;
                }

                if (IsThree)
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
                return this._hits;
            }
            set
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

        private double randedCostMult
        {
            get
            {
                return Balance.getCost(owner.Game.UnitTypes, MaxMove, MaxRegen, Ability, BaseArmor, Type, Attacks, IsThree, MaxHits) / (double)( BaseTotalCost );
            }
        }

        public double regenPct
        {
            get
            {
                return this._regenPct;
            }
            set
            {
                this._regenPct = Game.Random.GaussianCapped(value,
                        Math.Abs(this._regenPct - value) / value * .169, Math.Max(0, 2 * value - 1));
            }
        }

        private void CheckAttacks()
        {
            if (IsThree)
            {
                int numAttacks = ( 3 * hits + MaxHits - 1 ) / MaxHits;
                double used = 0;
                foreach (Attack a in attacks)
                    if (a.Used)
                        ++used;
                used /= attacks.Length;

                if (used > 0 && used < 1)
                {
                }

                Attack attack = attacks[0].Clone();
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

        public static Unit CreateTempUnit(Game game, string name)
        {
            return NewUnit(game, name, null, null, false);
        }
        internal static Unit NewUnit(string name, Tile tile, Player owner)
        {
            return NewUnit(owner.Game, name, tile, owner, true);
        }
        private static Unit NewUnit(Game game, string name, Tile tile, Player owner, bool add)
        {
            UnitSchema schema = game.UnitTypes.GetSchema();
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
                    unitRow.IsThree, type, unitRow.Hits, unitRow.Armor, unitRow.Regen, unitRow.Move, attacks);

            if (add)
            {
                owner.Add(unit);
                tile.Add(unit);
            }

            return unit;
        }

        #endregion //new units

        #region unit stat randomization

        private void RandStats()
        {
            const double maxMult = 1.3;

            foreach (Attack attack in this.attacks)
                attack.RandStats();
            this.armor = RandStat(this.armor, false);
            this.regen = RandStat(this.regen, true);

            int oldHits = this.maxHits;
            int newHits = ( IsThree ? 3 * RandStat(oldHits / 3.0, true) : RandStat(oldHits, true) );
            int hitInc = ( IsThree ? 3 : 1 );
            do
            {
                this.maxHits = newHits;
                newHits -= hitInc;
            } while (newHits > 0 && randedCostMult > maxMult);
            while (randedCostMult < ( 1 / maxMult ))
            {
                newHits += hitInc;
                this.maxHits = newHits;
            }

            this.hits = Game.Random.Round(this.hits * this.maxHits / (double)oldHits);
            if (Dead)
                Die();

            this.randed = true;
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
            //make all attackers untargetable
            foreach (Unit u in attackers)
                u.length = int.MaxValue;
            //make all defenders targetable
            foreach (Unit u in defenders)
                u.length = int.MinValue;
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
                //either move everyone or no one
                double chance = minMove / (double)needed;
                bool move = Game.Random.Bool(chance);
                foreach (Unit u in units)
                {
                    //subtract the value that was used to roll for success
                    u.movement -= minMove;
                    if (move)
                        u.ActualMove(t);

                    u.BalanceForMove(minMove, move, chance);

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
                movement -= useMove;

                double chance = useMove / (double)needed;
                move = Game.Random.Bool(chance);

                BalanceForMove(useMove, move, chance);

                //cant undo a random move
                canUndo = false;
            }
            else
            {
                movement -= needed;
                move = true;
            }

            if (move)
                canUndo &= ActualMove(t);

            return move;
        }
        private void BalanceForMove(int usedMove, bool moved, double chance)
        {
            if (moved)
                owner.AddUpkeep(.91 * Player.GetUpkeep(this) * ( 1 - chance ) * usedMove / (double)MaxMove, .13);
            else
                owner.AddWork(.65 * WorkRegen * usedMove);
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
            Player p;
            return ( GetNeeded(t) > -1 && ( Ability != Abilities.Aircraft || !t.Occupied(out p) || p == owner ) );
        }

        private int GetNeeded(Tile t)
        {
            if (t.HasCity())
                return 1;

            if (Type == UnitType.Air || Type == UnitType.Immobile)
                return 1;

            if (( Type == UnitType.Water || Type == UnitType.Amphibious ) && t.Terrain == Terrain.Water)
                return 1;

            if (( Type == UnitType.Ground || Type == UnitType.Amphibious ) && t.Terrain != Terrain.Water)
                switch (t.Terrain)
                {
                case Terrain.Plains:
                    return 1;
                case Terrain.Forest:
                    return 2;
                case Terrain.Mountain:
                    return 3;
                default:
                    throw new Exception();
                }

            //not the right unit type to move on that terrain
            return -1;
        }

        #endregion //moving

        #region pathfinding

        public static void PathFind(IEnumerable<Piece> pieces, Tile clicked, bool makeGroup = false)
        {
            int origCount = pieces.Count();
            pieces = pieces.Where(piece => piece.MaxMove > 0 && piece.Owner == piece.Owner.Game.CurrentPlayer
                    && ( !( piece is Unit ) || ( (Unit)piece ).GetNeeded(clicked) != -1 ));
            if (clicked == null || pieces.GroupBy(piece => piece.Tile).Count() != 1)
                return;

            var units = pieces.OfType<Unit>();
            var groups = units.Where(unit => unit.Type != UnitType.Air && unit.Type != UnitType.Immobile).GroupBy(unit => unit.Type);
            if (groups.Count(group => group.Key == UnitType.Ground || group.Key == UnitType.Water) == 2)
            {
                foreach (var group in Game.Random.Iterate(groups.Concat(new[] { pieces.Where(piece => !( piece is Unit )),
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
            else if (( yDist % 2 != 0 ) && ( ( y2 % 2 == 0 ) == ( x2 < x1 ) ))
                --xDist;
            return yDist + xDist;
        }

        #endregion //pathfinding

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
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
