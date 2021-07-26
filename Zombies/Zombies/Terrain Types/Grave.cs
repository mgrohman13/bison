using System;

namespace Zombies.Terrain_Types
{
    class Grave : Terrain
    {
        char cc = Convert.ToChar(127);

        public Grave(int x, int y)
            : base(x, y, 3)
        {
            base.symbol = cc;
            base.view = 2;

            color = ConsoleColor.Blue;
        }

        public override bool Move()
        {
            return move(true);
        }

        bool move(bool player)
        {
            if (player)
            {
                explore(.91, 8, 2, 1, 0, 95);//    10.0    86.4
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
            if (Program.rand.Next(39) == 0)
            {
                Program.map[x, y] = new Floor(x, y, Visible, Program.round(fire * 2.1));
                return true;
            }
            else if (Program.rand.Next(13) == 0)
            {
                Program.map[x, y] = new Rubble(x, y, Program.rand.Next(6) != 0, Visible, Program.round(fire * 1.3));
                return true;
            }
            else if (Program.rand.Next(21) != 0)
                fire--;

            return false;
        }

        public override string ToString()
        {
            return "Grave";
        }
    }
}