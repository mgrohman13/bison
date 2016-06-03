using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace ConsoleApplication1
{
    class Spell : Weapon
    {
        private int mana;

        public Spell(int mana, double hit, double crit, int dmg, double dmgDev, double dmvMin, int critDmg)
            : base(hit, crit, dmg, dmgDev, dmvMin, critDmg)
        {
            this.mana = mana;
        }

        public int Mana
        {
            get
            {
                return mana;
            }
        }

        public override double Hit
        {
            get
            {
                return hit;
            }
        }
    }
}
