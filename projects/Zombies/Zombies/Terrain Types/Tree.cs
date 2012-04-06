using System;

namespace Zombies.Terrain_Types
{
    class Tree : Terrain
    {
        char cc = '!';

        public Tree(int x, int y)
            : base(x, y,1)
        {
            base.symbol = cc;
            base.view = 2;

            color = ConsoleColor.Green;
        }

        public override bool Move()
        {
            return move();
        }

        bool move()
        {
            return (Program.rand.Next(2) == 0);
        }

        public override bool ZombieMove()
        {
            if (move())
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
                Program.map[x, y] = new Rubble(x, y, true, Visible, Program.round(fire * 1.3));
                return true;
            }
            else if (Program.rand.Next(3) == 0)
            {
                Program.map[x, y] = new Floor(x, y, Visible, Program.round(fire * 2.1));
                return true;
            }
            else if (Program.rand.Next(39) == 0)
                fire--;

            return false;
        }

		public override string ToString()
		{
			return "Tree";
		}
	}
}