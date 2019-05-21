using System;

namespace Zombies.Terrain_Types
{
    class Chest : Terrain
    {
        char cc = Convert.ToChar(9574);

        bool explored = false;

        public Chest(int x, int y)
            : base(x, y, 3)
        {
            base.symbol = cc;
            base.view = 1;

            color = ConsoleColor.Yellow;
        }

        public override bool Move()
        {
            return move(true);
        }

        bool move(bool player)
        {
            if (!explored && player)
            {
                explore(1, 15, 25, 60, 2, 5);//    100.0    5.0

                explored = true;

                cc = Convert.ToChar(9644);

                if (!base.towerTop)
                    base.symbol = cc;
            }

            return true;
        }

        public override bool ZombieMove()
        {
            if (move(false))
            {
                if (base.bombed)
                {
                    Program.explode(base.x, base.y);
                    bombed = false;
                }

                return true;
            }
            return false;
        }

        public override void TowerTopOff()
        {
            base.symbol = cc;
        }

        public override bool fireStuff()
        {
            if (Program.rand.Next(9) == 0)
            {
                Program.map[x, y] = new Floor(x, y, Visible, fire);
                return true;
            }
            else if (Program.rand.Next(6) == 0)
                fire--;

            return false;
        }

        public override string ToString()
        {
            return "Chest";
        }
    }
}