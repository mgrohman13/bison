using System;

namespace Zombies.Terrain_Types
{
    class Wall : Terrain
    {
        char cc = Convert.ToChar(9608);

        public Wall(int x, int y)
            : base(x, y, 6)
        {
            base.symbol = cc;
            base.view = 3;

            color = ConsoleColor.Gray;
        }

        public override bool CanMove()
        {
            return false;
        }

        public override bool Move()
        {
            return false;
        }

        public override bool ZombieMove()
        {
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
                Program.map[x, y] = new Rubble(x, y, Program.rand.Next(13) == 0, Visible, Program.round(fire * 1.3));
                return true;
            }
            else if (Program.rand.Next(10) != 0)
                fire--;

            return false;
        }

        public override string ToString()
        {
            return "Wall";
        }
    }
}