using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Player
    {
        private int hits, mana;
        private List<Weapon> weapons;

        public Player()
        {
        }

        public int Hits
        {
            get
            {
                return hits;
            }
        }

        public int Mana
        {
            get
            {
                return mana;
            }
        }

        public ReadOnlyCollection<Weapon> Weapons
        {
            get
            {
                return weapons.AsReadOnly();
            }
        }

        public void Hit(int dmg)
        {
            this.hits -= dmg;
        }
    }
}
