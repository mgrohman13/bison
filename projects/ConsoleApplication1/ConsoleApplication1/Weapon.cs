using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace ConsoleApplication1
{
    class Weapon
    {
        private static RandBooleans skillGain = new RandBooleans(Program.Random, .78);

        protected RandBooleans randHit, randCrit;
        protected double hit, crit, dmgDev, dmvMin, skill;
        protected int dmg, critDmg, durability, maxDurability;

        public Weapon(double hit, double crit, int dmg, double dmgDev, double dmgMin, int critDmg)
        {
            this.randHit = new RandBooleans(Program.Random, hit);
            this.randCrit = new RandBooleans(Program.Random, crit);

            this.hit = hit;
            this.crit = crit;
            this.dmg = dmg;
            this.dmgDev = dmgDev;
            this.dmvMin = dmgMin;
            this.critDmg = critDmg;
            this.durability = this.maxDurability = Program.Random.GaussianOEInt(39, .52, .26, 1);
            this.skill = 0;
        }

        public virtual double Hit
        {
            get
            {
                return hit * Math.Sqrt(Durability);
            }
        }
        public double Crit
        {
            get
            {
                return crit;
            }
        }
        public int Dmg
        {
            get
            {
                return dmg;
            }
        }
        public int CritDmg
        {
            get
            {
                return critDmg;
            }
        }
        public double Durability
        {
            get
            {
                return durability / (double)maxDurability;
            }
        }

        public double AverageDmg
        {
            get
            {
                return ( this.Hit * ( this.dmg * ( 1 - this.crit ) + this.critDmg * this.crit ) );
            }
        }

        public int Attack()
        {
            int dmg = 0;
            if (randHit.GetResult())
                if (randCrit.GetResult())
                    dmg = DoDamage(this.critDmg);
                else
                    dmg = DoDamage(this.dmg);

            if (dmg > 0)
            {
                double dmgMult = Program.Random.GaussianCapped(dmg / AverageDmg, .26);
                if (skillGain.GetResult())
                {
                    this.skill += dmgMult / 13;
                    while (skill > 1)
                    {
                        --skill;
                        switch (Program.Random.Next(4))
                        {
                        case 0:
                            ImprovePct(ref hit);
                            randHit.Chance = Hit;
                            break;
                        case 1:
                            ImprovePct(ref crit);
                            randCrit.Chance = crit;
                            break;
                        case 2:
                            ImproveDmg(ref dmg);
                            break;
                        case 3:
                            ImproveDmg(ref critDmg);
                            break;
                        }
                    }
                }
                else
                {
                    double reduce = 16.9;
                    if (this is Spell)
                        reduce /= 2.1;
                    int reduced = Program.Random.Round(durability * ( reduce / ( reduce + dmgMult ) ));
                    if (durability != reduced)
                    {
                        this.durability = reduced;
                        if (!( this is Spell ))
                            randHit.Chance = Hit;
                    }
                }
            }

            return dmg;
        }
        private int DoDamage(double avg)
        {
            return Program.Random.GaussianCappedInt(avg, dmgDev, Program.Random.Round(avg * dmvMin));
        }

        private void ImprovePct(ref double pct)
        {
            double imprv = GetImprove();
            pct = Math.Min(pct, 1 - ( 1 - pct ) / imprv);
        }
        private void ImproveDmg(ref int dmg)
        {
            double imprv = GetImprove();
            dmg = Program.Random.Round(dmg * imprv);
        }
        private double GetImprove()
        {
            return Program.Random.GaussianCapped(1.13, .0169, 1);
        }
    }
}
