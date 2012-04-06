using System;

namespace Zombies.Terrain_Types
{
    class Tower : Terrain
    {
        char cc = Convert.ToChar(9578);

        bool explored = false;
        int range;

        public Tower(int x, int y)
            : base(x, y,2)
        {
            base.symbol = cc;
            base.view = 2;
            base.hides = true;

            range = Program.round(4.5 + Program.random(4.5));

            color = ConsoleColor.White;
        }

        public override bool Move()
        {
            if (!explored)
            {
                Program.Vision(x, y, range);
                explored = true;
            }

            return move(true);
        }

        bool move(bool player)
        {
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
                if (y != 0)
                    Program.map[x, y - 1].TowerTop(false);
                Program.map[x, y] = new Rubble(x, y, explored, Visible, Program.round(fire * 2.1));
                return true;
            }
            else if (Program.rand.Next(13) == 0)
                fire--;

            return false;
        }

		public override string ToString()
		{
			return "Tower";
		}
	}
}