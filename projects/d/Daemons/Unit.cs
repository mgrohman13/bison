using System;
using System.Collections.Generic;
using System.Text;

namespace Daemons
{
    [Serializable]
    public class Unit
    {
        private Tile tile;
        public readonly Player Owner;
        public readonly UnitType Type;

        private int hits, movement;
        public readonly int MaxHits, Regen, MaxMove;
        private readonly int damage;
        public readonly int Souls;

        [NonSerialized]
        private bool dead;

        public Unit(UnitType type, Tile tile, Player owner)
        {
            this.tile = tile;
            this.Owner = owner;
            this.Type = type;

            owner.Add(this);
            tile.Add(this);

            this.movement = 0;
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
                souls = 1.6;
                break;

            default:
                throw new Exception();
            }

            this.hits = MaxHits;
            souls = Math.Pow(hits * damage, .65) * ( 3.9 + Regen * MaxMove ) * souls / 7.5;
            this.Souls = Game.Random.GaussianCappedInt(souls, .06, Game.Random.Round(souls * .65));
        }

        public double Damage
        {
            get
            {
                return damage * Math.Sqrt(HealthPct);
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

        public bool Healed
        {
            get
            {
                return ( hits == MaxHits );
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

        public double HealthPct
        {
            get
            {
                return (double)hits / (double)MaxHits;
            }
        }

        public double Strength
        {
            get
            {
                return Math.Pow(Hits * Math.Pow(Damage
                        + ( Type == UnitType.Daemon ? Damage / 13.0 : Type == UnitType.Archer ? Damage / 39.0 : 0 )
                        , 24 / 25.0) / 7.0, 2 / 3.0);
            }
        }

        public bool CanMove(Tile t)
        {
            return ( movement > 0 && ( tile.IsSideNeighbor(t) || ( Type == UnitType.Daemon && tile.IsCornerNeighbor(t) ) ) );
        }

        public void Move(Tile toTile)
        {
            if (Owner.Game.GetCurrentPlayer() == this.Owner && CanMove(toTile))
            {
                this.tile.Remove(this);
                toTile.Add(this);
                this.tile = toTile;
                this.movement--;
            }
        }

        private int DamageUnit(Unit defender)
        {
            float bonus = GetDamageMult(this.Type, defender.Type);
            int damage = Game.Random.WeightedInt(Game.Random.Round(this.Damage), bonus / 2f);

            defender.hits -= damage;
            if (defender.hits <= 0)
            {
                damage += defender.hits;
                defender.hits = 0;
                defender.Die();

                this.Owner.AddSouls(defender.Souls);
            }
            if (damage > 0)
                this.Owner.AddSouls(( damage / (float)defender.MaxHits ) * defender.Souls);

            Owner.Game.Log(String.Format("{8} -> {9}\r\n({0}, {1}/{2})   {7}{3}   ({4}, {5}/{6})",
                    this.DamageStr, this.hits, this.MaxHits, damage, defender.DamageStr, defender.hits, defender.MaxHits, damage > 0 ? "-" : "", this, defender));

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
            MakeArrow(movement);

            this.tile.Remove(this);
            this.Owner.Remove(this);

            this.dead = true;
        }

        internal void Attack()
        {
            if (this.dead)
                return;

            Unit defender = this.tile.GetTarget(this, false);
            if (defender != null)
                DamageUnit(defender);
        }

        public static void Fire(List<Unit> move, Tile target)
        {
            move.Sort(Tile.UnitDamageComparison);

            while (move.Count > 0 && target.NumUnits > 0)
            {
                double totalHits = 0;
                foreach (Unit defender in target.GetAllUnits())
                    if (defender.Owner != target.Game.GetCurrentPlayer())
                        totalHits += defender.hits / GetDamageMult(UnitType.Archer, defender.Type);
                double totalDamage = 0;
                foreach (Unit u in move)
                    totalDamage += u.Damage;

                Unit fire;
                if (totalDamage > MultHits(totalHits))
                {
                    fire = move[0];
                    foreach (Unit u in move)
                        if (u.Healed)
                        {
                            fire = u;
                            break;
                        }
                }
                else
                {
                    fire = null;
                    Unit defender = target.GetBestTarget(move[0]);
                    double hits = defender.hits / GetDamageMult(UnitType.Archer, defender.Type);
                    for (int a = move.Count ; --a > -1 ; )
                    {
                        fire = move[a];
                        if (fire.Damage > MultHits(hits))
                            break;
                    }
                }

                fire.Fire(target);
                move.Remove(fire);
            }
        }

        private static double MultHits(double totalHits)
        {
            return totalHits * Game.Random.Gaussian(1.9f, .019f);
        }

        public void Fire(Tile target)
        {
            if (( Owner.Game.GetCurrentPlayer() != this.Owner ) || ( this.Type != UnitType.Archer || this.movement <= 0 ))
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
            this.Owner.UseArrows(needed);
        }

        public void MakeArrow(float amt)
        {
            if (this.movement <= 0)
                return;
            this.Owner.MakeArrow(amt / ( this.Type == UnitType.Archer ? 1.3f : 6.5f ));
            this.movement--;
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
                if (!center.used && center.type == type)
                {
                    center.Use(this.Owner);
                    this.movement--;
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
                }
            }
        }

        public void Heal()
        {
            if (Owner.Game.GetCurrentPlayer() == this.Owner)
                HealInternal();
        }

        internal void ResetMove()
        {
            while (this.movement > 0)
                HealInternal();

            this.movement = this.MaxMove;
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