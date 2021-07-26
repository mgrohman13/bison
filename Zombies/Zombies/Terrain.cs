using System;
using System.Collections.Generic;
using Zombies.Terrain_Types;

namespace Zombies
{
    abstract class Terrain
    {
        public Terrain(int x, int y, int fireChance)
        {
            this.x = x;
            this.y = y;
            this.fireChance = fireChance;

            while (Program.rand.Next(10000) == 0)
                fire++;

            zombies = new List<Zombie>();
        }

        protected bool hides = false, towerTop = false;
        protected char symbol;
        protected bool visible = false;
        public bool Visible
        {
            get
            {
                return visible;
            }
        }
        public int view = 1, viewed = 0;

        protected ConsoleColor color;

        public ConsoleColor Color
        {
            get
            {
                if (Visible)
                    if (bombed)
                        return ConsoleColor.Red;
                    else if (fire > 0)
                        return ConsoleColor.Red;
                    else if (towerTop)
                        return ConsoleColor.White;
                    else if (hides)
                        return color;
                    else
                        switch (GetZombieDisplay())
                        {
                        case 0:
                            return color;

                        default:
                            return ConsoleColor.DarkYellow;
                        }
                else
                    return ConsoleColor.Gray;
            }
        }

        private int GetZombieDisplay()
        {
            bool found = false;
            foreach (Zombie z in zombies)
                if (z.Large)
                    return 2;
                else if (found)
                    return 2;
                else
                    found = true;
            return found ? 1 : 0;
        }

        protected bool bombed = false;
        public int fire = 0;
        int fireChance;

        protected int x, y;
        public int X
        {
            get
            {
                return x;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
        }

        public char Symbol
        {
            get
            {
                if (Visible)
                    if (bombed)
                        return Convert.ToChar(164);
                    else if (fire > 0)
                        return 'M';
                    else if (hides)
                        return symbol;
                    else
                        switch (GetZombieDisplay())
                        {
                        case 0:
                            return symbol;

                        case 1:
                            return 'f';

                        default:
                            return 'F';
                        }
                else
                    return '*';
            }
        }

        public bool bomb()
        {
            if (bombed || this is Wall || this is Door)
                return false;

            bombed = true;
            return true;
        }

        public void show()
        {
            visible = true;
            Invalidate();
        }

        private List<Zombie> zombies;
        public int NumZombies
        {
            get
            {
                return zombies.Count;
            }
        }

        private bool invalidated = false;
        public void Invalidate()
        {
            invalidated = true;
        }
        public void draw()
        {
            if (invalidated)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = Color;
                Console.Write(Symbol);

                invalidated = false;
            }
        }

        public void TowerTop()
        {
            towerTop = true;
            TowerTop(true);
        }

        public void TowerTop(bool make)
        {
            if (make)
            {
                symbol = Convert.ToChar(9572);
                hides = true;
            }
            else
            {
                TowerTopOff();
                Invalidate();
            }
        }

        protected void explore(double chances, int health, int rifle, int shotgun, int bombs, int zombie)
        {
            int times = Program.round(Program.random(chances));

            int h = 0, r = 0, s = 0, z = 0, b = 0;

            for (int a = 0 ; a < times ; a++)
            {
                if (Program.rand.Next(100) < health)
                    h++;
                if (Program.rand.Next(100) < rifle)
                    r++;
                if (Program.rand.Next(100) < shotgun)
                    s++;
                if (Program.rand.Next(100) < bombs)
                    b++;

                if (Program.rand.Next(100) < zombie)
                {
                    z++;
                    Program.CreateZombie(x, y);
                }
            }

            Program.health += h;
            Program.rifle += r;
            Program.shotgun += s;
            Program.bombs += b;

            if (h > 0 || r > 0 || s > 0 || b > 0)
            {
                string output = "You have found ";

                if (b > 0)
                    output += string.Format("{0} bomb{1}{2}{3}", b, b == 1 ? "" : "s",
                        ( h > 0 || s > 0 || r > 0 ) ? ", " : ".  ", ( h > 0 || s > 0 || r > 0 )
                        && !( h > 0 && s > 0 ) && !( h > 0 && r > 0 ) && !( s > 0 && r > 0 ) ? "and " : "");
                if (h > 0)
                    output += string.Format("{0} health kit{1}{2}{3}", h, h == 1 ? "" : "s",
                        ( s > 0 || r > 0 ) ? ", " : ".  ", ( s > 0 || r > 0 ) && !( s > 0 && r > 0 ) ? "and " : "");
                if (s > 0)
                    output += string.Format("{0} shotgun shell{1}{2}", s, s == 1 ? "" : "s",
                        ( r > 0 ) ? ", and " : ".  ");
                if (r > 0)
                    output += string.Format("{0} rifle round{1}.  ", r, r == 1 ? "" : "s");

                Program.output += output;
            }
        }

        public void startFire(int dist)
        {
            for (int c = 0 ; c < Program.bombDist - dist ; c++)
                if (Program.rand.Next(fireChance) == 0 && Program.rand.Next(13) != 0)
                    fire++;

            Invalidate();
        }

        public void startFire()
        {
            if (Program.rand.Next(fireChance) == 0 && Program.rand.Next(13) != 0)
                fire++;

            Invalidate();
        }

        public void Burn()
        {
            for (int c = 0 ; c < fire ; c++)
            {
                if (zombies.Count > 0)
                {
                    foreach (Zombie z in Program.GetZombieArray())
                        if (x == z.X && y == z.Y)
                            if (Program.rand.Next(z.Large ? 3 : 2) == 0)
                            {
                                Program.exp++;
                                Program.KillZombie(z);
                            }
                }

                if (x < Program.width - 1)
                    Program.map[x + 1, y].startFire();
                if (x > 0)
                    Program.map[x - 1, y].startFire();
                if (y < Program.height - 1)
                    Program.map[x, y + 1].startFire();
                if (y > 0)
                    Program.map[x, y - 1].startFire();

                if (fireStuff())
                    break;

                startFire();
            }

            Invalidate();
        }

        protected void resetView()
        {
            resetView(Program.playerView + 1);
        }

        private void resetView(int dist)
        {
            this.viewed = 0;

            if (dist > 0)
            {
                if (x != Program.width - 1)
                    Program.map[x + 1, y].resetView(dist - 1);
                if (x != 0)
                    Program.map[x - 1, y].resetView(dist - 1);
                if (y != Program.height - 1)
                    Program.map[x, y + 1].resetView(dist - 1);
                if (y != 0)
                    Program.map[x, y - 1].resetView(dist - 1);
            }
        }

        public virtual bool CanMove()
        {
            return true;
        }
        public abstract bool Move();
        public abstract bool ZombieMove();
        public abstract void TowerTopOff();
        public abstract bool fireStuff();

        internal Zombie RandomZombie()
        {
            return zombies[Program.rand.Next(zombies.Count)];
        }

        internal void RemoveZombie(Zombie z)
        {
            zombies.Remove(z);
            Invalidate();
        }

        internal void AddZombie(Zombie z)
        {
            zombies.Add(z);
            Invalidate();
        }
    }
}