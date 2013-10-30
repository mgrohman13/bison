using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons
{
    [Serializable]
    public class Unit
    {
        private Tile tile;
        private Player owner;
        public readonly UnitType Type;

        private int hits, movement, reserve;
        public readonly int MaxHits, Regen, MaxMove;
        private readonly int damage;
        private int souls;
        private double _morale;
        private float battles;

        [NonSerialized]
        private bool dead;

        public Unit(UnitType type, Tile tile, Player owner)
        {
            this.tile = tile;
            this.owner = owner;
            this.Type = type;

            owner.Add(this);
            tile.Add(this);

            this.movement = 0;
            this.reserve = 0;
            this.MaxMove = 1;

            double souls;
            switch (type)
            {
            case UnitType.Infantry:
                this.MaxHits = Game.Random.GaussianCappedInt(25, .21f, 19);//0.24
                this.Regen = Game.Random.GaussianCappedInt(3, .09f, 2);//0.33
                this.damage = Game.Random.GaussianCappedInt(8, .13f, 7);//0.13
                souls = 1.1;
                break;

            case UnitType.Archer:
                this.MaxHits = Game.Random.GaussianCappedInt(20, .09f, 17);//0.15
                this.Regen = Game.Random.GaussianCappedInt(2, .06f, 1);//0.50
                this.damage = Game.Random.GaussianCappedInt(12, .21f, 9);//0.25
                souls = 1.0;
                break;

            case UnitType.Knight:
                this.MaxHits = Game.Random.GaussianCappedInt(30, .13f, 21);//0.30
                this.Regen = Game.Random.GaussianCappedInt(2, .03f, 1);//0.50
                this.damage = Game.Random.GaussianCappedInt(11, .09f, 9);//0.18
                this.MaxMove = 2;
                souls = 0.8;
                break;

            case UnitType.Daemon:
                this.MaxHits = Game.Random.GaussianCappedInt(40, .13f, 34);//0.15
                this.Regen = Game.Random.GaussianCappedInt(6, .21f, 4);//0.33
                this.damage = Game.Random.GaussianCappedInt(25, .13f, 21);//0.16
                this.movement = 1;
                souls = 0.5;
                break;

            case UnitType.Indy:
                this.MaxHits = Game.Random.GaussianCappedInt(35, .30f, 21);//0.40
                this.Regen = Game.Random.GaussianCappedInt(4, .39f, 1);//0.75
                this.damage = Game.Random.GaussianCappedInt(10, .39f, 5);//0.50
                souls = 1.0;
                break;

            default:
                throw new Exception();
            }

            this.Morale = Game.Random.Weighted(.91);
            if (Type == UnitType.Daemon)
                GainMorale(1.3);

            this.hits = MaxHits;
            if (owner.Independent)
                souls *= 1.6;
            souls = Math.Pow(hits * damage, .65) * ( 3.9 + Regen * MaxMove ) * souls / 7.5;
            this.souls = Game.Random.GaussianCappedInt(souls, .06, Game.Random.Round(souls * .65));
        }

        public Player Owner
        {
            get
            {
                return owner;
            }
        }

        public int Souls
        {
            get
            {
                return souls;
            }
        }

        public double Damage
        {
            get
            {
                double mult = Math.Sqrt(HealthPct * Morale);
                if (mult < double.Epsilon)
                    mult = double.Epsilon;
                double retVal = damage * mult;
                if (retVal < 1)
                {
                    double turns = Math.Log(Math.Log(1.0 / damage) / Math.Log(mult)) / Math.Log(.39);
                    double max = Math.Log(Math.Log(1.0 / damage) / Math.Log(double.Epsilon)) / Math.Log(.39);
                    retVal = ( max - turns + .13 ) / ( .13 + max );
                }
                return retVal;
            }
        }

        public string DamageStr
        {
            get
            {
                return Math.Ceiling(Damage).ToString("0");
            }
        }

        public int BaseDamage
        {
            get
            {
                return damage;
            }
        }

        public double Morale
        {
            get
            {
                if (this.hits > 0)
                    if (this._morale <= double.Epsilon)
                        this._morale = double.Epsilon;
                    else if (this._morale >= 1)
                        this._morale = 1;
                return this._morale;
            }
            private set
            {
                double old = this._morale;

                this._morale = Game.Random.GaussianCapped(value, Math.Abs(this._morale - value) / value * .26, Math.Max(0, 2 * value - 1));

                if (this._morale < old)
                {
                    double lost = Math.Log(Math.Log(old) / Math.Log(this._morale)) / Math.Log(.39);
                    if (this.owner.Independent)
                    {
                        if (this.Type != UnitType.Daemon)
                            this._morale = Math.Pow(this._morale, Math.Pow(1 / .39, lost / 5.2));
                    }
                    else if (this.Type == UnitType.Daemon)
                    {
                        this._morale = Math.Pow(this._morale, Math.Pow(.39, lost / 3.9));
                    }
                }
            }
        }
        public double Recover1
        {
            get
            {
                double target = 1 - .039 / BaseDamage;
                target *= target;
                return GetRecover(target);
            }
        }
        public double Recover2
        {
            get
            {
                double target = 1 - 1.0 / BaseDamage;
                target *= target;
                return GetRecover(target);
            }
        }
        public double Recover3
        {
            get
            {
                return GetRecover(.00117);
            }
        }
        private double GetRecover(double target)
        {
            double retVal = 0;
            if (this.Morale < target)
            {
                double start = Math.Log(Math.Log(target) / Math.Log(Morale)) / Math.Log(.39);
                retVal = start + battles;
                if (this.movement + this.reserve < this.MaxMove)
                    retVal += ( this.MaxMove - this.movement - this.reserve ) * .21f / this.MaxMove;
                start = Math.Min(start, 1);
                if (retVal < start)
                    retVal = start;
                if (this.Type == UnitType.Daemon)
                    retVal /= 1.3;
            }
            return retVal;
        }

        public bool Healed
        {
            get
            {
                return ( Hits == MaxHits && this.movement > 0 && this.Morale > .00117 );
            }
        }

        public double TargetFactor
        {
            get
            {
                return .3 + HealthPct * Math.Sqrt(Hits / Damage);
            }
        }

        public Tile Tile
        {
            get
            {
                return tile;
            }
        }

        public int Hits
        {
            get
            {
                return hits;
            }
        }

        public int Movement
        {
            get
            {
                return movement;
            }
        }
        public int ReserveMove
        {
            get
            {
                if (this.Morale < .00117)
                    return 0;
                return reserve;
            }
        }

        public double HealthPct
        {
            get
            {
                return hits / (double)MaxHits;
            }
        }

        public double Strength
        {
            get
            {
                return GetStrength(Type, this.Hits, this.Damage);
            }
        }
        public double MaxStrength
        {
            get
            {
                return GetStrength(Type, this.MaxHits, this.BaseDamage);
            }
        }
        public static double GetStrength(UnitType type, int hits, double damage)
        {
            return Math.Pow(hits * Math.Pow(damage +
                    ( type == UnitType.Daemon ? damage / 13.0 : type == UnitType.Archer ? damage / 39.0 : 0 ),
                    24 / 25.0) / 7.0, 2 / 3.0);
        }

        internal void Won(Player independent)
        {
            this.movement = 0;
            this.souls = Game.Random.Round(this.souls * 1.6);
            this.owner = independent;
            this.owner.Add(this);
        }

        public bool CanMove(Tile t)
        {
            return ( this.Movement + this.ReserveMove > 0 && ( tile.IsSideNeighbor(t) || ( Type == UnitType.Daemon && tile.IsCornerNeighbor(t) ) ) );
        }

        public bool Move(Tile toTile)
        {
            if (Owner.Game.GetCurrentPlayer() == this.Owner && CanMove(toTile))
            {
                DoMove(toTile);

                if (this.movement > 0)
                    this.movement--;
                else
                    UseReserve();

                return true;
            }
            return false;
        }
        private void DoMove(Tile toTile)
        {
            this.tile.Remove(this);
            toTile.Add(this);
            this.tile = toTile;
        }

        private int DamageUnit(Unit defender)
        {
            float bonus = GetDamageMult(this.Type, defender.Type);
            int damage = Game.Random.WeightedInt(Game.Random.Round(this.Damage), bonus / 2f);

            double addMorale = 0;
            defender.hits -= damage;
            if (defender.hits <= 0)
            {
                damage += defender.hits;
                defender.hits = 0;
                defender.Die();

                List<Unit> units = defender.Tile.GetUnits(defender.Owner);
                double mult = 1 - defender.MaxStrength / ( defender.MaxStrength + Tile.GetArmyStr(units) );
                foreach (Unit unit in units)
                    unit.Morale *= mult;
                addMorale += defender.MaxStrength;

                this.Owner.AddSouls(defender.Souls, true);
            }
            if (damage > 0)
            {
                this.Owner.AddSouls(( damage / (float)defender.MaxHits ) * defender.Souls, true);
                defender.Morale *= 1 - damage / (double)( defender.hits + damage );

                addMorale += defender.MaxStrength * damage / (double)defender.MaxHits;
            }

            this.battles += .169f * damage / (float)this.BaseDamage;
            defender.battles += .169f * damage / (float)defender.MaxHits;

            double turns = addMorale / 39.0;
            this.battles += (float)turns;
            this.GainMorale(turns);

            Owner.Game.Log(String.Format("{5} -> {6}\r\n({0},{1}/{2}) {7}{3} {4}",
                    this.DamageStr, this.hits, this.MaxHits, damage,
                    defender.hits > 0 ? string.Format("({0},{1}/{2})", defender.DamageStr, defender.hits, defender.MaxHits) : "KILLED!",
                    this, defender, damage > 0 ? "-" : ""));

            return damage;
        }

        internal static float GetDamageMult(UnitType attacker, UnitType defender)
        {
            const float dmgPos = 1.17f, dmgNeg = .78f, dmgInd = 1.03f, dmgDmn = .97f;
            if (attacker == UnitType.Infantry)
            {
                if (defender == UnitType.Archer)
                    return dmgNeg;
                else if (defender == UnitType.Knight)
                    return dmgPos;
            }
            else if (attacker == UnitType.Archer)
            {
                if (defender == UnitType.Knight)
                    return dmgNeg;
                else if (defender == UnitType.Infantry)
                    return dmgPos;
            }
            else if (attacker == UnitType.Knight)
            {
                if (defender == UnitType.Infantry)
                    return dmgNeg;
                else if (defender == UnitType.Archer)
                    return dmgPos;
            }
            if (attacker != UnitType.Daemon && attacker != UnitType.Indy)
            {
                if (defender == UnitType.Indy)
                    return dmgInd;
                else if (defender == UnitType.Daemon)
                    return dmgDmn;
            }
            return 1;
        }

        private void Die()
        {
            MakeArrow(movement + ( reserve + .13f ) / ( 3.9f + battles ));

            this.tile.Remove(this);
            this.Owner.Remove(this);

            this.dead = true;
        }

        internal Tile Retreat(Tile prev)
        {

            if (this.movement > 0)
            {
                this.reserve += this.movement - 1;
                this.movement = 0;
            }
            else if (this.ReserveMove > 0)
            {
                UseReserve();
            }
            else
            {
                return prev;
            }
            LoseMorale(.39);

            Tile cur;
            if (prev != null && Game.Random.Bool())
            {
                cur = prev;
            }
            else
            {
                Dictionary<Tile, int> chances = new Dictionary<Tile, int>();
                IEnumerable<Tile> options = this.Tile.GetSideNeighbors();
                if (this.Type == UnitType.Daemon)
                    options = options.Union(this.Tile.GetCornerNeighbors());
                foreach (Tile t in options)
                    chances.Add(t, t.GetRetreatValue(this.Owner));
                cur = Game.Random.SelectValue(chances);
            }

            DoMove(cur);
            return Game.Random.Bool() ? cur : prev;
        }
        private void UseReserve()
        {
            if (this.ReserveMove < 1)
                throw new Exception();

            this.reserve--;
            this.battles += .39f;
            LoseMorale(1.0 / this.MaxMove);
        }
        private void LoseMorale(double mult)
        {
            this.Morale = Math.Pow(this.Morale / ( 1.0 + .21 * mult ), Math.Pow(3.9, mult));
        }

        internal void OnBattle()
        {
            this.battles += .52f;
        }

        internal void Attack()
        {
            if (this.dead)
                return;

            Unit defender = this.tile.GetTarget(this, false);
            if (defender != null)
                DamageUnit(defender);
        }

        public static void Fire(IEnumerable<Unit> move, Tile target)
        {
            move = move.OrderByDescending((u) => u.Tile.GetDamage(u));

            int arrows = target.Game.GetCurrentPlayer().Arrows;
            if (move.FirstOrDefault() != null && move.FirstOrDefault().Tile.IsCornerNeighbor(target))
                arrows /= 2;
            while (arrows > 0 && move.Any() && target.GetAllUnits().Any((u) => ( u.Owner != target.Game.GetCurrentPlayer() )))
            {
                double totalHits = MultHits(target.GetAllUnits().Where((defender) => ( defender.Owner != target.Game.GetCurrentPlayer() ))
                        .Aggregate<Unit, double>(0, (t, defender) => t + ( defender.hits / GetDamageMult(UnitType.Archer, defender.Type) )));
                int count = 0;
                double totalDamage = move.Aggregate<Unit, double>(0, (t, u) => t + ( ++count <= arrows ? u.Damage : 0 ));

                Unit fire;
                if (totalDamage > totalHits)
                {
                    fire = move.FirstOrDefault((u) => u.Healed);
                    if (fire == null)
                        fire = move.First();
                }
                else
                {
                    bool healed = ( arrows < move.Count() );
select:
                    Unit first = move.FirstOrDefault((u) => !healed || u.Healed);
                    if (first == null)
                    {
                        healed = false;
                        goto select;
                    }
                    else
                    {
                        Unit defender = target.GetBestTarget(first);
                        double hits = defender.hits / GetDamageMult(UnitType.Archer, defender.Type);
                        fire = null;
                        foreach (Unit u in move.Reverse())
                            if (!healed || u.Healed)
                            {
                                fire = u;
                                if (fire.Damage > MultHits(hits))
                                    break;
                            }
                    }
                }

                fire.Fire(target);
                --arrows;
                move = move.Except(new[] { fire });
            }
        }

        private static double MultHits(double totalHits)
        {
            return totalHits * Game.Random.Gaussian(1.9f, .019f);
        }

        public void Fire(Tile target)
        {
            if (Owner.Game.GetCurrentPlayer() != this.Owner || this.Type != UnitType.Archer || this.Movement <= 0)
                return;

            int needed = 1;
            if (this.tile.IsCornerNeighbor(target))
                needed++;
            else if (!( this.tile.IsSideNeighbor(target) ))
                return;
            if (this.Owner.Arrows < needed)
                return;

            Unit defender = target.GetTarget(this, true);
            if (defender == null)
                return;

            Owner.Game.Log("--------------------------");

            DamageUnit(defender);

            this.movement--;
            this.reserve--;

            this.Owner.UseArrows(needed);
        }

        public void MakeArrow(float amt)
        {
            if (this.movement <= 0)
                return;
            this.Owner.MakeArrow(amt / ( this.Type == UnitType.Archer ? 1.3f : 6.5f ));
            this.movement--;
            this.reserve--;
        }

        public void Build()
        {
            if (Owner.Game.GetCurrentPlayer() != this.Owner)
                return;

            int move = this.movement;
            if (move > 0)
            {
                Build(ProductionType.Knight);
                if (move == this.movement)
                    Build(ProductionType.Archer);
                if (move == this.movement)
                    Build(ProductionType.Infantry);
            }
        }

        public void Build(ProductionType type)
        {
            if (Owner.Game.GetCurrentPlayer() != this.Owner || this.movement == 0)
                return;

            foreach (ProductionCenter center in this.tile.GetProduction())
                if (!center.Used && center.type == type && this.tile.NumAttackers == 0)
                {
                    center.Use(this.Owner);
                    this.movement--;
                    this.reserve--;
                    break;
                }
        }

        internal void HealInternal()
        {
            if (this.movement > 0)
            {
                this.hits += this.Regen;
                if (this.hits > this.MaxHits)
                {
                    float diff = this.hits - (float)this.MaxHits;
                    hits = MaxHits;

                    //MakeArrow also reduces movement
                    MakeArrow(diff / (float)this.Regen);
                }
                else
                {
                    this.movement--;
                    this.reserve--;
                }
            }
        }

        public void Heal()
        {
            if (Owner.Game.GetCurrentPlayer() == this.Owner && this.tile.GetAttackers().Length == 0)
                HealInternal();
        }

        internal void ResetMove()
        {
            this.movement += this.reserve;
            while (this.movement > this.MaxMove)
                HealInternal();
            if (this.movement < this.MaxMove)
                this.battles += ( this.MaxMove - this.movement ) * .21f / this.MaxMove;

            this.movement = this.MaxMove;
            this.reserve = this.MaxMove;

            this.battles += Game.Random.Gaussian(.13f);
            float turns = 1 - battles;
            if (turns < .13)
                turns = (float)( .13 / ( Math.Pow(1.13 - turns, .78) ) );
            GainMorale(turns);
            battles -= 1 - turns;
            if (this.Type == UnitType.Daemon)
                battles -= .3f;
        }
        private void GainMorale(double turns)
        {
            if (turns > 0)
                if (BaseDamage * ( 1 - Math.Sqrt(Morale) ) > .039)
                {
                    if (this.Type == UnitType.Daemon)
                        turns *= 1.3;
                    Morale = Math.Pow(Morale, Math.Pow(.39, turns));
                }
                else
                {
                    this.battles -= (float)turns / 1.3f;
                }
        }

        public string GetMoveString()
        {
            return string.Format("{0} / {1}", movement, MaxMove);
        }

        public System.Drawing.Bitmap GetPic()
        {
            return Owner.GetPic(Type);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Owner.Name, Type);
        }

        public static int UnitComparison(Unit u1, Unit u2)
        {
            return ( u2.Strength > u1.Strength ? 1 : ( u1.Strength > u2.Strength ? -1 : 0 ) );
        }
    }

    public enum UnitType
    {
        Infantry,
        Archer,
        Knight,
        Daemon,
        Indy
    }
}