using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons
{
    [Serializable]
    public class Unit
    {
        public readonly UnitType Type;

        public readonly int MoveMax, DamageMax, HitsMax, Regen;

        private Tile tile;
        private Player owner;

        private int movement, reserve, hits, souls;
        private double _morale, battles;

        public Unit(UnitType type, Tile t, Player o)
        {
            this.tile = t;
            this.owner = o;
            this.Type = type;

            o.Add(this);
            t.Add(this);

            this.movement = 0;
            this.reserve = 0;
            this.MoveMax = 1;

            double s;
            switch (type)
            {
            case UnitType.Infantry:
                this.HitsMax = Game.Random.GaussianCappedInt(Consts.InfantryHits, .21, 19);     //.24
                this.Regen = Game.Random.GaussianCappedInt(3, .091, 2);                         //.33
                this.DamageMax = Game.Random.GaussianCappedInt(Consts.InfantryDamage, .13, 7);  //.13
                s = 1.1;
                break;

            case UnitType.Archer:
                this.HitsMax = Game.Random.GaussianCappedInt(Consts.ArcherHits, .091, 17);      //.15
                this.Regen = Game.Random.GaussianCappedInt(2, .052, 1);                         //.50
                this.DamageMax = Game.Random.GaussianCappedInt(Consts.ArcherDamage, .21, 9);    //.25
                s = 1.0;
                break;

            case UnitType.Knight:
                this.HitsMax = Game.Random.GaussianCappedInt(Consts.KnightHits, .13, 21);       //.30
                this.Regen = Game.Random.GaussianCappedInt(2, .039, 1);                         //.50
                this.DamageMax = Game.Random.GaussianCappedInt(Consts.KnightDamage, .091, 9);   //.18
                this.MoveMax = 2;
                s = .8;
                break;

            case UnitType.Daemon:
                this.HitsMax = Game.Random.GaussianCappedInt(40, .13, 34);                      //.15
                this.Regen = Game.Random.GaussianCappedInt(6, .21, 4);                          //.33
                this.DamageMax = Game.Random.GaussianCappedInt(25, .13, 21);                    //.16
                this.movement = 1;
                s = .5;
                break;

            case UnitType.Indy:
                this.HitsMax = Game.Random.GaussianCappedInt(Consts.IndyHits, .26, 21);         //.40
                this.Regen = Game.Random.GaussianCappedInt(4, .39, 1);                          //.75
                this.DamageMax = Game.Random.GaussianCappedInt(Consts.IndyDamage, .39, 5);      //.50
                s = 1.0;
                break;

            default:
                throw new Exception();
            }

            this.battles = -.26;
            this._morale = Game.Random.Weighted(.91);
            if (this.Type == UnitType.Daemon)
                GainMorale(Consts.MoraleDaemonGain);

            this.hits = this.HitsMax;
            if (this.owner.Independent)
                s *= Consts.IndySoulMult;
            s = Math.Pow(this.hits * this.DamageMax, .65) * ( 3.9 + this.Regen * this.MoveMax ) * s / 7.8;
            this.souls = Game.Random.GaussianCappedInt(s, .065, Game.Random.Round(s * .65));
        }

        public Player Owner
        {
            get
            {
                return this.owner;
            }
        }

        public int Souls
        {
            get
            {
                return this.souls;
            }
        }

        public double Damage
        {
            get
            {
                double mult = Math.Sqrt(HealthPct * Morale);
                if (mult < double.Epsilon)
                    mult = double.Epsilon;
                double retVal = this.DamageMax * mult;
                if (retVal < 1)
                {
                    double turns = Consts.GetMoraleTurns(mult, 1.0 / this.DamageMax);
                    double max = Consts.GetMoraleTurns(double.Epsilon, 1.0 / this.DamageMax);
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

        public double Morale
        {
            get
            {
                double morale = this._morale;
                if (this.hits > 0)
                    if (morale <= double.Epsilon)
                        morale = double.Epsilon;
                    else if (morale >= 1)
                        morale = 1;
                return morale;
            }
            private set
            {
                if (value < this.Morale)
                {
                    double loss = this.Morale - value;
                    double turns = Consts.GetMoraleTurns(value, this.Morale);
                    if (this.owner.Independent)
                    {
                        if (this.Type != UnitType.Daemon)
                        {
                            const double IndyLoss = 5.2;
                            loss = value - loss / IndyLoss;
                            turns = Math.Pow(value, Math.Pow(1 / Consts.MoraleTurnPower, turns / IndyLoss));
                            value = Math.Max(loss, turns);
                        }
                    }
                    else if (this.Type == UnitType.Daemon)
                    {
                        const double DaemonGain = 3.9;
                        loss = value + loss / DaemonGain;
                        turns = Math.Pow(value, Math.Pow(Consts.MoraleTurnPower, turns / DaemonGain));
                        value = Math.Min(loss, turns);
                    }
                }

                this._morale = Game.Random.GaussianCapped(value, Math.Abs(this.Morale - value) / value * .26, Math.Max(0, 2 * value - 1));
            }
        }
        public double RecoverFull
        {
            get
            {
                double target = 1 - Consts.MoraleMaxDmgLoss / this.DamageMax;
                target *= target;
                return GetRecover(target);
            }
        }
        public double RecoverDmg
        {
            get
            {
                double target = 1 - 1.0 / this.DamageMax;
                target *= target;
                return GetRecover(target);
            }
        }
        public double RecoverCritical
        {
            get
            {
                return GetRecover(Consts.MoraleCritical);
            }
        }
        private double GetRecover(double target)
        {
            double retVal = 0;
            if (this.Morale < target)
            {
                double start = Consts.GetMoraleTurns(Morale, target);
                retVal = start + this.battles;
                if (this.movement + this.reserve < this.MoveMax)
                    retVal += ( this.MoveMax - this.movement - this.reserve ) * Consts.NoReserveBattles / this.MoveMax;
                start = Math.Min(start, 1);
                if (retVal < start)
                    retVal = start;
                if (this.Type == UnitType.Daemon)
                    retVal /= Consts.MoraleDaemonGain;
            }
            return retVal;
        }

        public bool Healed
        {
            get
            {
                return ( this.movement > 0 && this.hits == this.HitsMax && this.Morale > Consts.MoraleCritical );
            }
        }

        public double TargetFactor
        {
            get
            {
                return .3 + HealthPct * Math.Sqrt(this.hits / Damage);
            }
        }

        public Tile Tile
        {
            get
            {
                return this.tile;
            }
        }

        public int Hits
        {
            get
            {
                return this.hits;
            }
        }

        public int Movement
        {
            get
            {
                return this.movement;
            }
        }
        public int ReserveMovement
        {
            get
            {
                if (this.Morale < Consts.MoraleCritical)
                    return 0;
                return this.reserve;
            }
        }

        public double HealthPct
        {
            get
            {
                return this.hits / (double)this.HitsMax;
            }
        }

        public double Strength
        {
            get
            {
                return Consts.GetStrength(this.Type, this.hits, this.Damage);
            }
        }
        public double StrengthMax
        {
            get
            {
                return Consts.GetStrength(this.Type, this.HitsMax, this.DamageMax);
            }
        }

        internal void Won(Player independent)
        {
            this.movement = 0;
            this.souls = Game.Random.Round(this.souls * Consts.IndySoulMult);
            this.owner = independent;
            this.owner.Add(this);
        }

        public bool CanMove(Tile t)
        {
            return ( this.movement + this.ReserveMovement > 0 && ( this.tile.IsSideNeighbor(t) || ( this.Type == UnitType.Daemon && this.tile.IsCornerNeighbor(t) ) ) );
        }

        public bool Move(Tile toTile)
        {
            if (this.owner.Game.GetCurrentPlayer() == this.owner && CanMove(toTile))
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
            double bonus = GetDamageMult(this.Type, defender.Type);
            int damage = Game.Random.WeightedInt(Game.Random.Round(this.Damage), bonus / 2);

            double addMorale = 0;
            defender.hits -= damage;
            if (defender.hits <= 0)
            {
                damage += defender.hits;
                defender.hits = 0;
                defender.Die();

                IEnumerable<Unit> units = defender.tile.GetUnits(defender.owner);
                double mult = 1 - defender.StrengthMax / ( defender.StrengthMax + Tile.GetArmyStr(units) );
                foreach (Unit unit in units)
                    unit.Morale *= mult;
                addMorale += defender.StrengthMax;

                this.owner.AddSouls(defender.souls, true);
            }
            if (damage > 0)
            {
                this.owner.AddSouls(( damage / (double)defender.HitsMax ) * defender.souls, true);
                defender.Morale *= 1 - damage / (double)( defender.hits + damage );

                addMorale += defender.StrengthMax * damage / (double)defender.HitsMax;
            }

            this.battles += .169 * damage / (double)this.DamageMax;
            defender.battles += .169 * damage / (double)defender.HitsMax;

            double turns = addMorale / 39.0;
            this.battles += turns;
            this.GainMorale(turns);

            this.owner.Game.Log(string.Format("{5} -> {6}\r\n({0},{1}/{2}) {7}{3} {4}",
                    this.DamageStr, this.hits, this.HitsMax, damage,
                    defender.hits > 0 ? string.Format("({0},{1}/{2})", defender.DamageStr, defender.hits, defender.HitsMax) : "KILLED!",
                    this, defender, damage > 0 ? "-" : ""));

            return damage;
        }

        internal static double GetDamageMult(UnitType attacker, UnitType defender)
        {
            if (attacker == UnitType.Infantry)
            {
                if (defender == UnitType.Archer)
                    return Consts.DmgNeg;
                else if (defender == UnitType.Knight)
                    return Consts.DmgPos;
            }
            else if (attacker == UnitType.Archer)
            {
                if (defender == UnitType.Knight)
                    return Consts.DmgNeg;
                else if (defender == UnitType.Infantry)
                    return Consts.DmgPos;
            }
            else if (attacker == UnitType.Knight)
            {
                if (defender == UnitType.Infantry)
                    return Consts.DmgNeg;
                else if (defender == UnitType.Archer)
                    return Consts.DmgPos;
            }
            if (attacker != UnitType.Daemon && attacker != UnitType.Indy)
            {
                if (defender == UnitType.Indy)
                    return Consts.DmgIndy;
                else if (defender == UnitType.Daemon)
                    return Consts.DmgDaemon;
            }
            return 1;
        }

        private void Die()
        {
            MakeArrow(this.movement + ( this.reserve + .13 ) / ( 3.9 + this.battles ));

            this.tile.Remove(this);
            this.owner.Remove(this);
        }

        internal Tile Retreat(Tile prev)
        {

            if (this.movement > 0)
            {
                this.reserve += this.movement - 1;
                this.movement = 0;
            }
            else if (this.ReserveMovement > 0)
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
                IEnumerable<Tile> options = this.tile.GetSideNeighbors();
                if (this.Type == UnitType.Daemon)
                    options = options.Union(this.tile.GetCornerNeighbors());
                foreach (Tile t in options)
                    chances.Add(t, t.GetRetreatValue(this.owner));
                cur = Game.Random.SelectValue(chances);
            }

            DoMove(cur);
            return Game.Random.Bool() ? cur : prev;
        }
        private void UseReserve()
        {
            if (this.ReserveMovement < 1)
                throw new Exception();

            this.reserve--;
            this.battles += .39 / this.MoveMax;
            LoseMorale(1.0 / this.MoveMax);
        }
        private void LoseMorale(double mult)
        {
            this.Morale = Math.Pow(this.Morale / ( 1.0 + .21 * mult ), Math.Pow(3.9, mult));
        }

        internal void OnBattle()
        {
            this.battles += .52;
        }

        internal void Attack()
        {
            if (this.hits > 0)
            {
                Unit defender = this.tile.GetTarget(this, false);
                if (defender != null)
                    DamageUnit(defender);
            }
        }

        public static void Fire(IEnumerable<Unit> move, Tile target)
        {
            move = move.OrderByDescending(unit => unit.tile.GetDamage(unit));

            int arrows = target.Game.GetCurrentPlayer().Arrows;
            if (move.FirstOrDefault() != null && move.FirstOrDefault().tile.IsCornerNeighbor(target))
                arrows /= 2;
            while (arrows > 0 && move.Any() && target.GetUnits().Any(unit => unit.owner != target.Game.GetCurrentPlayer()))
            {
                double totalHits = MultHits(target.GetUnits().Where(unit => unit.owner != target.Game.GetCurrentPlayer())
                        .Aggregate<Unit, double>(0, (sum, unit) => ( sum + ( unit.hits / GetDamageMult(UnitType.Archer, unit.Type) ) )));
                int count = 0;
                double totalDamage = move.Aggregate<Unit, double>(0, (sum, unit) => ( sum + ( ++count <= arrows ? unit.Damage : 0 ) ));

                Unit fire;
                if (totalDamage > totalHits)
                {
                    fire = move.FirstOrDefault(unit => unit.Healed);
                    if (fire == null)
                        fire = move.First();
                }
                else
                {
                    bool healed = ( arrows < move.Count() );
select:
                    Unit first = move.FirstOrDefault(unit => !healed || unit.Healed);
                    if (first == null)
                    {
                        healed = false;
                        goto select;
                    }
                    else
                    {
                        Unit defender = target.GetBestTarget(first);
                        double h = defender.hits / GetDamageMult(UnitType.Archer, defender.Type);
                        fire = null;
                        foreach (Unit u in move.Reverse())
                            if (!healed || u.Healed)
                            {
                                fire = u;
                                if (fire.Damage > MultHits(h))
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
            return totalHits * Game.Random.Gaussian(1.9, .019);
        }

        public void Fire(Tile target)
        {
            if (this.owner.Game.GetCurrentPlayer() != this.owner || this.Type != UnitType.Archer || this.movement <= 0)
                return;

            int needed = 1;
            if (this.tile.IsCornerNeighbor(target))
                needed++;
            else if (!( this.tile.IsSideNeighbor(target) ))
                return;
            if (this.owner.Arrows < needed)
                return;

            Unit defender = target.GetTarget(this, true);
            if (defender == null)
                return;

            this.owner.Game.Log("--------------------------");

            DamageUnit(defender);

            this.movement--;
            LoseReserve();

            this.owner.UseArrows(needed);
        }

        public void MakeArrow(double amt)
        {
            if (this.movement <= 0)
                return;
            this.owner.MakeArrow(amt / ( this.Type == UnitType.Archer ? 1.3 : 6.5 ));
            this.movement--;
            LoseReserve();
        }

        public void Build()
        {
            if (this.owner.Game.GetCurrentPlayer() != this.owner)
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
            if (this.owner.Game.GetCurrentPlayer() != this.owner || this.movement == 0)
                return;

            foreach (ProductionCenter center in this.tile.GetProduction())
                if (!center.Used && center.Type == type && !this.tile.Occupied(this.owner))
                {
                    center.Use(this.owner);
                    this.movement--;
                    LoseReserve();
                    break;
                }
        }

        internal void HealInternal()
        {
            if (this.movement > 0)
            {
                this.hits += this.Regen;
                if (this.hits > this.HitsMax)
                {
                    double diff = this.hits - this.HitsMax;
                    this.hits = this.HitsMax;

                    //MakeArrow also reduces movement
                    MakeArrow(diff / (double)this.Regen);
                }
                else
                {
                    this.movement--;
                    LoseReserve();
                }
            }
        }

        public void Heal()
        {
            if (this.owner.Game.GetCurrentPlayer() == this.owner && !this.tile.Occupied(this.owner))
                HealInternal();
        }

        private void LoseReserve()
        {
            this.reserve--;
            this.battles -= Consts.NoReserveBattles / this.MoveMax;
        }
        internal void ResetMove()
        {
            this.movement += this.reserve;
            while (this.movement > this.MoveMax)
                HealInternal();
            if (this.movement < this.MoveMax)
                this.battles += ( this.MoveMax - this.movement ) * Consts.NoReserveBattles / this.MoveMax;

            this.movement = this.MoveMax;
            this.reserve = this.MoveMax;

            this.battles += Game.Random.Gaussian(.13);
            double turns = 1 - this.battles;
            const double minTurns = .13;
            if (turns < minTurns)
                turns = ( minTurns / ( Math.Pow(1 + minTurns - turns, .78) ) );
            GainMorale(turns);
            this.battles -= 1 - turns;
            if (this.Type == UnitType.Daemon)
                this.battles -= ( Consts.MoraleDaemonGain - 1 );
        }
        private void GainMorale(double turns)
        {
            if (turns > 0)
                if (this.DamageMax * ( 1 - Math.Sqrt(Morale) ) > Consts.MoraleMaxDmgLoss)
                {
                    if (this.Type == UnitType.Daemon)
                        turns *= Consts.MoraleDaemonGain;
                    Morale = Math.Pow(Morale, Math.Pow(Consts.MoraleTurnPower, turns));
                }
                else
                {
                    this.battles -= (double)turns / 1.3;
                }
        }

        public string GetMoveString()
        {
            return string.Format("{0} / {1}", this.movement, this.MoveMax);
        }

        public System.Drawing.Bitmap GetPic()
        {
            return this.owner.GetPic(this.Type);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.owner.Name, this.Type);
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