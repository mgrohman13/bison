using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Enemy
    {
        private int hits, max;
        private Weapon weapon;

        public Enemy(int hits, Weapon weapon)
        {
            this.hits = this.max = hits;
            this.weapon = weapon;
        }

        public double Threat
        {
            get
            {
                return hits * weapon.AverageDmg;
            }
        }
        public double HitPct
        {
            get
            {
                return hits / (double)max;
            }
        }

        public int Attack()
        {
            return weapon.Attack();
        }

        public void Hit(int dmg)
        {
            this.hits -= dmg;
        }
    }
}
