using System;

namespace Zombies.Terrain_Types
{
    class Door : Terrain
    {
        char cc = Convert.ToChar(9553);

        public int mapNum;
        public int toMap;

        public Door(int x, int y, int map, int mapNum)
            : base(x, y,5)
        {
            base.symbol = cc;
            base.view = 3;
            this.toMap = map;
            this.mapNum = mapNum;

            Program.doors.Add(this);

            color = ConsoleColor.DarkCyan;
        }

        public Door(int x, int y, int map, int mapNum, bool add)
            : base(x, y,6)
        {
            base.symbol = Convert.ToChar(9553);
            base.view = 3;
            this.toMap = map;
            this.mapNum = mapNum;

            if (add)
                Program.doors.Add(this);

            color = ConsoleColor.Gray;
        }

        public Door(int map, int mapNum)
            : base(-1, -1,6)
        {
            this.toMap = map;
            this.mapNum = mapNum;

            Program.doors.Add(this);
        }

        public void place(int x, int y)
        {
            base.x = x;
            base.y = y;
        }

        public override bool Move()
        {
            return true;
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
            if (Program.rand.Next(2) == 0)
                fire--;

            return false;
        }

		public override string ToString()
		{
			return "Door";
		}
	}
}