using System;

namespace Zombies.Terrain_Types
{
    class Floor : Terrain
    {
        char cc = ' ';

        public Floor(int x, int y)
            : base(x, y,6)
        {
            base.symbol = cc;
            base.view = 1;

            color = ConsoleColor.Gray;
        }

        public Floor(int x, int y, Terrain old)
            : base(x, y, 6)
        {
            base.symbol = cc;
            base.view = 1;
            base.visible = old.Visible;
            base.fire = old.fire;

            if (!visible)
                base.resetView();

            color = ConsoleColor.Gray;
        }

        public Floor(int x, int y, bool visible, int fire)
            : base(x, y,6)
        {
            base.symbol = cc;
            base.view = 1;
            base.visible = visible;
            base.fire = fire;

            if (!visible)
                base.resetView();

            color = ConsoleColor.Gray;
        }

        public override bool Move()
        {
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
            base.symbol = cc;
        }

        public override bool fireStuff()
        {
            if (Program.rand.Next(13) != 0)
                fire--;

            return false;
        }

		public override string ToString()
		{
			return "Floor";
		}
	}
}