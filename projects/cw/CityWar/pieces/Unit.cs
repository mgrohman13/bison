using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MattUtil;
using System.IO;

namespace CityWar
{
    public class Unit : Piece
    {
        #region fields and constructors
        public readonly UnitType Type;
        public readonly int regen, maxHits;
        public readonly string Race;
        private double _regenPct;

        internal readonly CostType costType;
        internal readonly bool isThree;
        private readonly int cost, pplCost;

        internal int Length = int.MaxValue;

        private int _hits, armor;
        private Attack[] attacks;
        private bool randed;
        private double _randedCostMult = -1;

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

            this.randed = false;

            this.attacks = attacks;
            this.hits = hits;
            CheckAttacks();
            foreach (Attack attack in this.attacks)
                attack.SetOwner(this);
        }

        //constructor for loading games
        private Unit(Player owner, string race, int group, int movement, int maxMove, string name, Tile tile, Abilities ability,
            int cost, int regen, int maxHits, int hits, int armor, bool isThree,
            bool randed, CostType costType, UnitType Type, Attack[] attacks, int pplCost, double _regenPct)
            : base(group, movement, maxMove, name, tile, ability)
        {
            this.owner = owner;
            this.Race = race;
            this.cost = cost;
            this.regen = regen;
            this.maxHits = maxHits;
            this.hits = hits;
            this.armor = armor;
            this.isThree = isThree;
            this.randed = randed;
            this.costType = costType;
            this.Type = Type;
            this.attacks = attacks;
            this.pplCost = pplCost;
            this._regenPct = _regenPct;
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

        public double GetDisbandAmount()
        {
            double low = Math.Min(Attack.DisbandDivide, Attack.DeathDivide),
                    high = Math.Max(Attack.DisbandDivide, Attack.DeathDivide);
            double healthPct = GetHealthPct();
            return InverseCost / high * ( 1 - healthPct ) + InverseCost / low * healthPct;
        }

        public double GetHealthPct()
        {
            return hits / (double)maxHits;
        }
        #endregion //public methods and properties

        #region internal methods
        internal void Wound(int damage)
        {
            hits -= damage;
            if (Dead)
            {
                EndBattle();
                Die();
            }
            else
            {
                SetRegenPct(regenPct * .97 * ( maxHits + 30.0 - Math.Pow(3.9 * damage, 0.74) ) / ( maxHits + 30.0 ));
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
                SetRegenPct(( 1 - ( 1 - regenPct ) / 1.69 ));

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
        #endregion //internal methods

        #region hits
        private Brush healthBrush = Brushes.Lime;
        public Brush HealthBrush
        {
            get
            {
                return healthBrush;
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

                if (healthBrush != Brushes.Lime)
                    healthBrush.Dispose();

                //set the health brush to the right color
                double pct = GetHealthPct();
                if (pct < 1)
                {
                    const double power = .666;

                    double sign = Math.Sign(pct - .5);
                    double greenPct = Math.Pow(( 2.0 * pct - 1.0 ) * sign, power) * sign / 2.0 + 1.0 / 2.0;

                    healthBrush = new SolidBrush(Color.FromArgb(255, Game.Random.Round(255f * (float)greenPct), 0));
                }
                else
                    healthBrush = Brushes.Lime;
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
            _regenPct = value + Game.Random.GaussianCapped(1, Math.Abs(_regenPct - value) * .26, 1 - Math.Min(value, 1 - value)) - 1;
        }

        private void CheckAttacks()
        {
            if (isThree)
            {
                float used = 0;
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
            foreach (Attack a in attacks)
                a.RandStats();

            Unit newUnit = new Unit(Race, name, tile, owner, cost, pplCost, costType, ability, isThree, Type,
                    isThree ? 3 * RandStat(maxHits / 3.0, true) : RandStat(maxHits, true),
                    RandStat(armor, false), RandStat(regen, true), MaxMove, attacks, regenPct);

            tile.Add(newUnit);
            owner.Add(newUnit);

            newUnit.hits = Game.Random.Round(GetHealthPct() * newUnit.maxHits);
            newUnit.movement = movement;
            newUnit.group = group;

            tile.Remove(this);
            owner.Remove(this, false);

            newUnit.randed = true;
            newUnit.ResetMoveIntern(false);

            double maxMult = Math.Sqrt(Math.Min(Math.Min(Attack.DeathDivide, Attack.DisbandDivide), Attack.RelicDivide));
            if (newUnit.randedCostMult > maxMult || newUnit.randedCostMult < ( 1 / maxMult ))
                newUnit.RandStats();
            else if (newUnit.Dead)
                newUnit.Die();

            tile.hasCenterPiece = false;
        }
        internal static int RandStat(double stat, bool keepPositive)
        {
            int lowerCap = Game.Random.Round(stat * 2 / 3.0);
            if (keepPositive)
            {
                if (lowerCap < 1)
                    lowerCap = 1;
            }
            else
            {
                --lowerCap;
            }
            return Game.Random.GaussianCappedInt(stat + 1, .0666, lowerCap + 1) - 1;
        }
        #endregion //unit stat randomization

        #region start and end battle
        internal static Battle StartBattle(Unit[] attackers, Tile t)
        {
            //only attack with units that have movement remaining
            attackers = Tile.FindAllUnits(attackers, delegate(Unit unit)
            {
                return unit.movement > 0;
            });

            //units of type all defend first, by themselves
            Unit[] defenders = t.FindAllUnits(delegate(Unit unit)
                {
                    return ( unit.Type == UnitType.Immobile );
                });
            if (defenders.Length < 1)
                defenders = t.GetAllUnits();

            //make all defenders targetable
            foreach (Unit u in defenders)
                u.Length = -1;

            return new Battle(attackers, defenders);
        }

        internal static void EndBattle(Battle b)
        {
            foreach (Unit u in b.defenders)
                u.EndBattle();
            foreach (Unit u in b.extraDefenders)
                u.EndBattle();

            foreach (Unit u in b.attackers)
                u.EndBattle();
        }
        private void EndBattle()
        {
            //reset length
            Length = int.MaxValue;
            int usedAttacks = 0;
            //reset attacks
            foreach (Attack a in attacks)
                if (a.Used)
                {
                    a.Used = false;
                    ++usedAttacks;
                }
            if (usedAttacks > 0 && Owner == Game.CurrentPlayer)
            {
                //only reduce movement if any attacks were used
                --movement;
                Owner.AddWork(Attack.OverkillPercent * WorkRegen * ( attacks.Length - usedAttacks ) / (double)attacks.Length);
            }
        }
        #endregion //start and end battle

        #region moving
        internal static bool UnitGroupMove(List<Unit> units, Tile t, Dictionary<Piece, bool> undoPieces)
        {
            if (units.Count < 2)
            {
                //no need to move by group
                Piece move = units[0];
                bool canUndo;
                bool moved = move.Move(t, out canUndo);
                undoPieces.Add(move, canUndo);
                return moved;
            }

            int minMove = int.MaxValue;
            foreach (Unit u in units)
                minMove = Math.Min(minMove, u.movement);

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
                if (Game.Random.Bool(minMove / (float)needed))
                {
                    foreach (Unit u in units)
                    {
                        u.ActualMove(t);
                        //cant undo a random move
                        undoPieces.Add(u, false);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //everyone has enough move, so just do it
                foreach (Unit u in Game.Random.Iterate<Unit>(units))
                {
                    bool canUndo;
                    u.Move(t, out canUndo);
                    undoPieces.Add(u, canUndo);
                }
                return true;
            }
        }

        protected override bool DoMove(Tile t, out bool canUndo)
        {
            canUndo = true;
            if (movement < 1)
                return false;

            int needed = GetNeeded(t);
            if (needed < 0)
                return false;

            bool move;
            if (movement < needed)
            {
                move = ( Game.Random.Bool(movement / (float)needed) );
                //cant undo a random move
                canUndo = false;
                movement = 0;
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
            return GetNeeded(t) > -1;
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

        #region saving and loading
        internal override void SavePiece(BinaryWriter bw)
        {
            bw.Write("Unit");

            SavePieceStuff(bw);

            //int
            bw.Write(cost);
            bw.Write(pplCost);
            bw.Write(regen);
            bw.Write(maxHits);
            bw.Write(hits);
            bw.Write(armor);

            //double
            bw.Write(regenPct);

            //bool
            bw.Write(isThree);
            bw.Write(randed);

            //string
            bw.Write(Race);
            bw.Write(costType.ToString());
            bw.Write(Type.ToString());

            //attack
            bw.Write(attacks.Length);
            foreach (Attack a in attacks)
                a.SaveAttack(bw);
        }

        internal static Unit LoadUnit(BinaryReader br, Player owner)
        {
            int group, movement, maxMove;
            string name;
            Tile tile;
            Abilities ability;
            Piece.LoadPieceStuff(br, out group, out movement, out maxMove, out name, out tile, out ability);

            int cost = br.ReadInt32(),
                pplCost = br.ReadInt32(),
                regen = br.ReadInt32(),
                maxHits = br.ReadInt32(),
                hits = br.ReadInt32(),
                armor = br.ReadInt32();

            double regenPct = br.ReadDouble();

            bool isThree = br.ReadBoolean(),
                randed = br.ReadBoolean();

            string race = br.ReadString();

            CostType costType;
            switch (br.ReadString())
            {
            case "Air":
                costType = CostType.Air;
                break;

            case "Death":
                costType = CostType.Death;
                break;

            case "Earth":
                costType = CostType.Earth;
                break;

            case "Nature":
                costType = CostType.Nature;
                break;

            case "Production":
                costType = CostType.Production;
                break;

            case "Water":
                costType = CostType.Water;
                break;

            default:
                throw new Exception();
            }

            UnitType Type;
            switch (br.ReadString())
            {
            case "Ground":
                Type = UnitType.Ground;
                break;

            case "Water":
                Type = UnitType.Water;
                break;

            case "Air":
                Type = UnitType.Air;
                break;

            case "Amphibious":
                Type = UnitType.Amphibious;
                break;

            case "Immobile":
                Type = UnitType.Immobile;
                break;

            default:
                throw new Exception();
            }

            //attack
            int attackCount = br.ReadInt32();
            Attack[] attacks = new Attack[attackCount];
            for (int a = -1 ; ++a < attackCount ; )
                attacks[a] = Attack.LoadAttack(br);

            Unit u = new Unit(owner, race, group, movement, maxMove, name, tile, ability, cost, regen, maxHits,
                hits, armor, isThree, randed, costType, Type, attacks, pplCost, regenPct);

            for (int a = -1 ; ++a < attackCount ; )
                attacks[a].SetOwner(u);

            return u;
        }
        #endregion //saving and loading
    }

    public enum CostType
    {
        Air,
        Death,
        Earth,
        Nature,
        Production,
        Water
    }

    public enum UnitType
    {
        Ground,
        Water,
        Air,
        Amphibious,
        Immobile
    }
}
