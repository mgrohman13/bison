namespace Zombies
{
	public class Zombie
	{
		int x, y;
		public int X { get { return x; } set { x = value; } }
		public int Y { get { return y; } set { y = value; } }

		bool large;
		public bool Large { get { return large; } }

		public Zombie(int x, int y)
		{
			this.x = x;
			this.y = y;
			this.large = (Program.rand.Next(6) == 0);
		}

		public void Move(int dir)
		{
			switch (dir)
			{
				case 0:
					if (y != 0)
						if (Program.map[x, y - 1].ZombieMove())
						{
							Program.MoveZombie(this, x, y - 1);
						}
					break;

				case 1:
					if (x != 0)
						if (Program.map[x - 1, y].ZombieMove())
						{
							Program.MoveZombie(this, x - 1, y);
						}
					break;

				case 2:
					if (y != Program.height - 1)
						if (Program.map[x, y + 1].ZombieMove())
						{
							Program.MoveZombie(this, x, y + 1);

						}
					break;

				case 3:
					if (x != Program.width - 1)
						if (Program.map[x + 1, y].ZombieMove())
						{
							Program.MoveZombie(this, x + 1, y);
						}
					break;
			}
		}
	}
}