using System;

namespace Zombies.Terrain_Types
{
    class House : Terrain
    {
        char cc = Convert.ToChar(8486);

        bool explored = false;

        public House(int x, int y)
            : base(x, y, 2)
        {
            base.symbol = cc;
            base.view = 2;

            color = ConsoleColor.White;
        }

        public override bool Move()
        {
            return move(true);
        }

        bool move(bool player)
        {
            if (!explored && player)
            {
                explore(3.9, 21, 13, 6, 3, 39);//    167.7,     152.1

                explored = true;

                cc = Convert.ToChar(8745);

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
            if (Program.rand.Next(6) == 0)
            {
                Program.map[x, y] = new Rubble(x, y, explored, Visible, Program.round(fire * 2.1));
                return true;
            }
            else if (Program.rand.Next(13) == 0)
                fire--;

            return false;
        }

        public override string ToString()
        {
            return "House";
        }
    }
}