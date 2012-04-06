using System;

namespace Zombies.Terrain_Types
{
    class Rubble : Terrain
    {
        const char bc = ';';
        const char ec = ',';

        bool explored = false;

        public Rubble(int x, int y)
            : base(x, y, 4)
        {
            if (Program.rand.Next(13) == 0)
                explored = true;

            if (explored)
                symbol = ec;
            else
                symbol = bc;

            base.symbol = symbol;
            base.view = 3;
            base.hides = true;

            color = ConsoleColor.Gray;
        }

        public Rubble(int x, int y, bool explored, Terrain old)
            : base(x, y, 4)
        {
            if (Program.rand.Next(13) == 0)
                explored = true;

            if (explored)
                symbol = ec;
            else
                symbol = bc;

            base.view = 3;
            base.hides = true;
            this.explored = explored;
            base.visible = old.Visible;
            base.fire = old.fire;

            if (!visible)
                base.resetView();

            color = ConsoleColor.Gray;
        }

        public Rubble(int x, int y, bool explored, bool visible, int fire)
            : base(x, y, 4)
        {
            if (Program.rand.Next(13) == 0)
                explored = true;

            if (explored)
                symbol = ec;
            else
                symbol = bc;

            base.view = 3;
            base.hides = true;
            this.explored = explored;
            base.visible = visible;
            base.fire = fire;

            if (!visible)
                base.resetView();

            color = ConsoleColor.Gray;
        }

        public override bool Move()
        {
            return move(true);
        }

        bool move(bool player)
        {
            if (Program.rand.Next(2) == 0)
                return false;

            if (!explored && player)
            {
                explore(1.3, 13, 6, 21, 1, 39);//  053.3     050.7

                explored = true;

                if (!base.towerTop)
                    base.symbol = ec;
            }

            return true;
        }

        public override bool ZombieMove()
        {
            if (base.bombed)
            {
                Program.explode(base.x, base.y);
                bombed = false;
            }

            return true;
        }

        public override void TowerTopOff()
        {
            if (explored)
                base.symbol = ec;
            else
                base.symbol = bc;
        }

        public override bool fireStuff()
        {
            if (Program.rand.Next(26) == 0)
            {
                Program.map[x, y] = new Floor(x, y, Visible, Program.round(fire * 1.3));
                return true;
            }
            else if (Program.rand.Next(6) != 0)
                fire--;

            return false;
        }

		public override string ToString()
		{
			return "Rubble";
		}
	}
}