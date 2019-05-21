using System;
using System.Linq;
using System.Collections.Generic;
using Zombies.Terrain_Types;

namespace Zombies
{
    class Program
    {
        public const int height = 21, width = 80, bombDist = 6, playerView = 3;

        public static List<Terrain[,]> maps;
        public static List<List<Zombie>> zombieMaps;
        public static Terrain[,] map;
        private static List<Zombie> zombies;
        public static List<Door> doors;

        public static MattUtil.MTRandom rand;

        public static int x, y;
        public static int health, shotgun, rifle, bombs, exp;

        public static string output;

        public static void KillZombie(Zombie z)
        {
            map[z.X, z.Y].RemoveZombie(z);
            zombies.Remove(z);
        }
        public static void MoveZombie(Zombie z, int newX, int newY)
        {
            map[z.X, z.Y].RemoveZombie(z);
            map[newX, newY].AddZombie(z);
            z.X = newX;
            z.Y = newY;
        }
        public static void CreateZombie(int x, int y)
        {
            Zombie z = new Zombie(x, y);
            zombies.Add(z);
            map[x, y].AddZombie(z);
        }

        static void Main(string[] args)
        {
            rand = new MattUtil.MTRandom();
            rand.StartTick();

            Console.CursorVisible = false;
            Console.SetWindowSize(width, height + 4);
            Console.SetBufferSize(width, height + 4);

            do
            {
                maps = new List<Terrain[,]>();
                zombieMaps = new List<List<Zombie>>();
                doors = new List<Door>();
                health = 13;
                shotgun = 21;
                rifle = 6;
                bombs = 1;
                exp = 0;
                output = "";

                newMap(-1, 0);
                int index = maps.IndexOf(map);
                zombies = new List<Zombie>();
                createMap(0, 0);
                maps[index] = map;
                zombieMaps[index] = zombies;

                Goto(0);

                do
                {
                    x = rand.Next(width);
                    y = rand.Next(height);
                }
                while (map[x, y] is Wall);

                while (!map[x, y].Move())
                {
                }

                while (health > 0)
                {
                    drawInfo();
                }

                Console.SetCursorPosition(0, 0);
                for (int yy = 0 ; yy < height + 3 ; yy++)
                    for (int xx = 0 ; xx < width ; xx++)
                        Console.Write(' ');

                resetCons();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Game over - thanks for playing.");
                Console.WriteLine("Final score: {0}", exp);

            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            rand.Dispose();
        }

        public static void Goto(int mapNum)
        {
            int from = maps.IndexOf(map);

            if (maps.Count <= mapNum)
                newMap(from, mapNum);

            int num = maps.IndexOf(map);

            map = maps[mapNum];
            zombies = zombieMaps[mapNum];

            for (int xx = 0 ; xx < width ; xx++)
                for (int yy = 0 ; yy < height ; yy++)
                    if (map[xx, yy] == null)
                    {
                        int index = maps.IndexOf(map);
                        zombies = new List<Zombie>();
                        createMap(mapNum, from);
                        maps[index] = map;
                        zombieMaps[index] = zombies;
                        break;
                    }

            foreach (Terrain t in map)
            {
                Door d = t as Door;
                if (d != null)
                    if (d.toMap == num)
                    {
                        x = t.X;
                        y = t.Y;
                        break;
                    }
            }

            Console.SetCursorPosition(0, 0);
            for (int y = 0 ; y < height ; y++)
            {
                for (int x = 0 ; x < width ; x++)
                {
                    Console.ForegroundColor = map[x, y].Color;
                    Console.Write(map[x, y].Symbol);
                }
            }
        }

        static void drawInfo()
        {
            Vision(Program.x, Program.y, playerView);

            RefreshMap();

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(Convert.ToChar(1));

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(0, 21);
            string inv = string.Format("Life: {0}   Shotgun: {1}   Rifle: {2}   Bombs: {5}   Score: {4}   Terrain: {3}",
                health.ToString("00"), shotgun.ToString("00"), rifle.ToString("00"), map[x, y].ToString(), exp, bombs);
            Console.Write(inv.PadRight(80, ' '));

            if (map[x, y].NumZombies > 0)
            {
                FightZombies();
                return;
            }

            Console.Write(output.PadRight(160, ' '));
            output = "";

            Console.SetWindowPosition(0, 0);
            UserInput();
        }

        private static void RefreshMap()
        {
            foreach (Terrain t in map)
                t.draw();
        }

        public static void Vision(int x, int y, int sight)
        {
            View(x + 1, y, sight, 1);
            View(x - 1, y, sight, 3);
            View(x, y + 1, sight, 0);
            View(x, y - 1, sight, 2);
        }

        static void View(int x, int y, int sight, int from)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                if (map[x, y].viewed >= sight)
                    return;

                map[x, y].viewed = sight;
                sight -= map[x, y].view;

                if (sight >= 0)
                    map[x, y].show();

                if (sight > 0)
                {
                    if (from != 3)
                        View(x + 1, y, sight, 1);
                    if (from != 1)
                        View(x - 1, y, sight, 3);
                    if (from != 2)
                        View(x, y + 1, sight, 0);
                    if (from != 0)
                        View(x, y - 1, sight, 2);
                }
            }
        }

        static void UserInput()
        {
            string input = "";

            resetCons();

            ConsoleKeyInfo key = Console.ReadKey(true);
            input = key.KeyChar.ToString().ToLower();
            if (key.Key == ConsoleKey.UpArrow)
                input = "w";
            if (key.Key == ConsoleKey.LeftArrow)
                input = "a";
            if (key.Key == ConsoleKey.RightArrow)
                input = "d";
            if (key.Key == ConsoleKey.DownArrow)
                input = "s";
            if (key.Modifiers == ConsoleModifiers.Alt || key.Modifiers == ConsoleModifiers.Control
                || key.Modifiers == ConsoleModifiers.Shift)
                input += " ";

            Console.SetWindowPosition(0, 0);

            if (input.Length == 2)
            {
                if (input[1] == ' ')
                    input = input[1].ToString() + input[0].ToString();

                if (input[0] == ' ')
                {
                    switch (input[1])
                    {
                    case 'w':
                        if (y != 0)
                            shootZombie(x, y - 1);
                        break;

                    case 'a':
                        if (x != 0)
                            shootZombie(x - 1, y);
                        break;

                    case 's':

                        if (y != height - 1)
                            shootZombie(x, y + 1);
                        break;

                    case 'd':
                        if (x != width - 1)
                            shootZombie(x + 1, y);
                        break;

                    case ' ':
                        if (bombs > 0)
                            if (map[x, y].bomb())
                                bombs--;
                            else
                                output += "There is already a bomb here.";
                        else
                            output += "You are out of bombs.";
                        break;

                    default:
                        drawInfo();
                        return;
                    }
                }
            }
            int newX = x, newY = y;
            switch (input)
            {
            case "w":
                --newY;
                break;

            case "a":
                --newX;
                break;

            case "s":
                ++newY;
                break;

            case "d":
                ++newX;
                break;

            case " ":
                Door d = map[x, y] as Door;
                if (d != null)
                    Program.Goto(d.toMap);
                else if (map[x, y] is Grave)
                    map[x, y].Move();
                goto _skip;

            default:
                drawInfo();
                return;
            }

            if (newX < width && newX > -1 && newY < height && newY > -1 && ( map[newX, newY].CanMove() ))
            {
                if (map[newX, newY].Move())
                {
                    map[x, y].Invalidate();
                    x = newX;
                    y = newY;
                }
            }
            else
            {
                drawInfo();
                return;
            }

_skip:

            Burn();
            MoveZombies();
        }

        private static void Burn()
        {
            switch (rand.Next(8))
            {
            case 0:
                for (int x = 0 ; x < width ; x++)
                    for (int y = 0 ; y < height ; y++)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;

            case 1:
                for (int x = width - 1 ; x >= 0 ; x--)
                    for (int y = 0 ; y < height ; y++)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;

            case 2:
                for (int x = 0 ; x < width ; x++)
                    for (int y = height - 1 ; y >= 0 ; y--)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;

            case 3:
                for (int x = width - 1 ; x >= 0 ; x--)
                    for (int y = height - 1 ; y >= 0 ; y--)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;

            case 4:
                for (int y = 0 ; y < height ; y++)
                    for (int x = 0 ; x < width ; x++)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;

            case 5:
                for (int y = 0 ; y < height ; y++)
                    for (int x = width - 1 ; x >= 0 ; x--)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;

            case 6:
                for (int y = height - 1 ; y >= 0 ; y--)
                    for (int x = 0 ; x < width ; x++)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;

            case 7:
                for (int y = height - 1 ; y >= 0 ; y--)
                    for (int x = width - 1 ; x >= 0 ; x--)
                        if (map[x, y].fire > 0)
                            map[x, y].Burn();
                break;
            }

            if (map[Program.x, Program.y].fire > 0)
            {
                output += "You have been burned!  ";
                health -= map[Program.x, Program.y].fire;
            }
        }

        private static void shootZombie(int x, int y)
        {
            if (rifle <= 0)
            {
                output += "You have no more rifle rounds...";
                return;
            }

            if (map[x, y].NumZombies > 0)
            {
                rifle--;
                exp += 2;

                KillZombie(map[x, y].RandomZombie());
            }
        }

        static void MoveZombies()
        {
            foreach (Zombie z in GetZombieArray())
            {
                if (rand.Next(2) == 0 && !( z.X == x && z.Y == y ))
                {
                    int dir = rand.Next(4);
                    switch (rand.Next(6))
                    {
                    case 0:
                        if (z.X > x)
                            dir = 1;
                        break;

                    case 1:
                        if (z.Y > y)
                            dir = 0;
                        break;

                    case 2:
                        if (z.X < x)
                            dir = 3;
                        break;

                    case 3:
                        if (z.Y < y)
                            dir = 2;
                        break;
                    }

                    z.Move(dir);
                }
            }

            createZombies();
        }

        static void createZombies()
        {
            Terrain[,] temp = (Terrain[,])map.Clone();
            foreach (Terrain t in temp)
                if (t is Grave)
                    if (rand.Next(round(( (double)width * (double)height ) / 39.0)) == 0)
                    {
                        CreateZombie(t.X, t.Y);
                        t.ZombieMove();
                    }
        }

        static void FightZombies()
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('X');

            //Console.WriteLine("There {2} {0} zombie{1} here!",
            //    map[x, y].Zombies, map[x, y].Zombies == 1 ? "" : "s", map[x, y].Zombies == 1 ? "is" : "are");

            //while (map[x, y].Zombies > 0)
            //{
            //Console.WriteLine("Life: {0}\tShotgun: {1}\tRifle: {2}",
            //                health.ToString("00"), shotgun.ToString("00"), rifle.ToString("00"));

            //Console.WriteLine("{0} zombie{1} left...",
            //map[x, y].Zombies, map[x, y].Zombies == 1 ? "" : "s");

            Zombie z = map[x, y].RandomZombie();

            //Console.WriteLine("You attack a{0}zombie...", z.Large ? " large " : " ");
            int roll = rand.Next(6) + ( map[x, y] is House ? 2 : 1 );
            //Console.WriteLine("Rolled a {0}.", roll);

            int zomb = 4;
            if (z.Large)
                zomb++;

            if (roll >= zomb)
            {
                BattleKillZombie(z);
            }
            else
            {
                int diff = zomb - roll;
                if (shotgun < diff)
                {
                    wound();

                    if (health <= 0)
                    {
                        //Console.WriteLine("The zombie eats you.");
                        return;
                    }
                }
                else
                {
                    bool yes = true;
                    if (health > 1)
                    {
                        resetCons();

                        Console.SetCursorPosition(0, 23);
                        Console.WriteLine("Kill {2} zombie for {0} shell{1}?   ",
                            diff, diff == 1 ? "" : "s", z.Large ? "large" : "small");

                        bool loop = true;
                        while (loop)
                        {
                            resetCons();

                            ConsoleKeyInfo key = Console.ReadKey(true);
                            string input = key.KeyChar.ToString().ToLower();
                            if (key.Key == ConsoleKey.Enter)
                                input = "e";

                            switch (input)
                            {
                            case "y":
                            case "1":
                            case " ":
                                loop = false;
                                break;

                            case "n":
                            case "0":
                            case "e":
                                yes = false;
                                loop = false;
                                break;
                            }

                            Console.SetWindowPosition(0, 0);
                        }
                    }

                    if (yes)
                    {
                        shotgun -= diff;
                        BattleKillZombie(z);
                    }
                    else
                        wound();
                }
            }
            //}
            //Console.WriteLine("Press enter to continue.");
            //ReadKey(true).KeyChar.ToString;

            drawInfo();
        }

        private static void resetCons()
        {
            Console.SetCursorPosition(0, 0);
            Console.SetWindowPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void wound()
        {
            health--;
            //Console.WriteLine("You have been wounded.");
        }

        static void BattleKillZombie(Zombie z)
        {
            exp += ( z.Large ? 4 : 3 );
            KillZombie(z);
            //Console.WriteLine("Zombie killed!");
        }

        static void newMap(int from, int newMapNum)
        {
            if (newMapNum > maps.Count)
            {
                foreach (Door d in doors)
                    if (d.toMap == newMapNum - 1)
                    {
                        newMap(d.mapNum, newMapNum - 1);
                        break;
                    }
            }

            map = new Terrain[width, height];
            zombies = new List<Zombie>();

            int numDoors = round(1.5 + random(2));
            if (numDoors == 1 && doors.Any() && doors.Select(d => d.toMap).Max() <= maps.Count)
                numDoors = 2;

            for (int c = 1 ; c <= numDoors ; c++)
            {
                int x, y;
                if (rand.Next(2) == 0)
                {
                    x = rand.Next(width);
                    y = ( rand.Next(2) == 0 ? 0 : height - 1 );
                }
                else
                {
                    y = rand.Next(height);
                    x = ( rand.Next(2) == 0 ? 0 : width - 1 );
                }

                if (map[x, y] is Door)
                    c--;
                else
                {
                    if (from == -1)
                        map[x, y] = new Door(x, y, c, newMapNum);
                    else if (c == 1)
                    {
                        map[x, y] = new Door(x, y, from, newMapNum);
                        Program.x = x;
                        Program.y = y;
                    }
                    else
                    {
                        int max = 1;
                        foreach (Door d in doors)
                            max = Math.Max(max, d.toMap);
                        map[x, y] = new Door(x, y, max + 1, newMapNum);
                    }
                }
            }

            maps.Add(map);
            zombieMaps.Add(zombies);
        }

        private static void startZombies(Terrain[,] map, int numZoms)
        {
            for (int c = 0 ; c < numZoms ; c++)
            {
                int x = rand.Next(width), y = rand.Next(height);
                if (map[x, y] is Wall || map[x, y] is Door)
                    c--;
                else
                    CreateZombie(x, y);
            }
        }

        static void createMap(int mapNum, int from)
        {
            Terrain[,] map = new Terrain[width, height];

            for (int y = 0 ; y < height ; y++)
                for (int x = 0 ; x < width ; x++)
                {
                    map[x, y] = new Floor(x, y);
                }

            for (int type = 0 ; type < 2 ; type++)
            {
                int blobs;
                if (type == 0)
                    blobs = round(random((double)width * (double)height / 210.0));
                else
                    blobs = round(random((double)width * (double)height / 130.0));
                for (int c = 0 ; c < blobs ; c++)
                {
                    int size;
                    if (type == 0)
                        size = round(random(6));
                    else
                        size = round(random(13));

                    int x = rand.Next(width), y = rand.Next(height);

                    for (int c2 = 0 ; c2 < size ; c2++)
                    {
                        if (type == 0)
                            map[x, y] = new Rubble(x, y);
                        else
                            map[x, y] = new Tree(x, y);

                        int dist = round(random(1.3));
                        for (int final = 0 ; final <= dist ; final++)
                            switch (rand.Next(4))
                            {
                            case 0:
                                x--;
                                break;

                            case 1:
                                y++;
                                break;

                            case 2:
                                x++;
                                break;

                            case 3:
                                y--;
                                break;
                            }

                        if (x < 0)
                            x = 0;
                        else if (x > width - 1)
                            x = width - 1;
                        if (y < 0)
                            y = 0;
                        else if (y > height - 1)
                            y = height - 1;
                    }
                }
            }

            int chest = 0, grave = 0, house = 0;
            while (chest == grave || chest == house || grave == house)
            {
                chest = rand.Next(3);
                grave = rand.Next(3);
                house = rand.Next(3);
            }
            int count = round(random((double)width * (double)height / 169.0));
            for (int c = 0 ; c < 3 ; c++)
            {
                for (int c2 = 0 ; c2 < count ; c2++)
                {
                    int x = rand.Next(width), y = rand.Next(height);

                    if (c == chest)
                        map[x, y] = new Chest(x, y);
                    else if (c == grave)
                        map[x, y] = new Grave(x, y);
                    else if (c == house)
                        map[x, y] = new House(x, y);
                }
            }

            int walls = round(random((double)width * (double)height / 210.0));
            for (int c = 0 ; c < walls ; c++)
            {
                int length = round(random((double)width * (double)height / 130.0));

                int x = rand.Next(width), y = rand.Next(height), dir = rand.Next(4);

                for (int c2 = 0 ; c2 < length ; c2++)
                {
                    if (y >= 0 && y < height && x >= 0 && x < width)
                        if (rand.Next(39) == 0)
                            map[x, y] = new Floor(x, y);
                        else if (rand.Next(13) == 0)
                            map[x, y] = new Rubble(x, y);
                        else
                            map[x, y] = new Wall(x, y);

                    switch (dir)
                    {
                    case 0:
                        y--;
                        if (rand.Next(6) == 0)
                            if (rand.Next(6) == 0)
                                dir = 3;
                            else
                                dir = ( rand.Next(2) == 0 ? 1 : 2 );
                        break;

                    case 1:
                        x--;
                        if (rand.Next(6) == 0)
                            if (rand.Next(6) == 0)
                                dir = 2;
                            else
                                dir = ( rand.Next(2) == 0 ? 3 : 0 );
                        break;

                    case 2:
                        x++;
                        if (rand.Next(6) == 0)
                            if (rand.Next(6) == 0)
                                dir = 1;
                            else
                                dir = ( rand.Next(2) == 0 ? 3 : 0 );
                        break;

                    case 3:
                        y++;
                        if (rand.Next(6) == 0)
                            if (rand.Next(6) == 0)
                                dir = 0;
                            else
                                dir = ( rand.Next(2) == 0 ? 1 : 2 );
                        break;
                    }
                }
            }

            int towers = round(random((double)width * (double)height / 666.0));
            for (int c = 0 ; c < towers ; c++)
            {
                int x = rand.Next(width - 2) + 1, y = rand.Next(height - 2) + 1;
                map[x, y] = new Tower(x, y);
            }

            for (int c = 0 ; c < rand.Next(4) ; c++)
            {
                int Twidth = rand.Next(round(Math.Sqrt((double)width * (double)height) / 2.1)) + 1;
                int Theight = rand.Next(round(Math.Sqrt((double)width * (double)height) / 2.1)) + 1;
                int Woffset = round((double)Twidth / 2.0), Hoffset = round((double)Theight / 2.0);

                int startX = rand.Next(0 - Woffset, width - Twidth + 1 + Woffset),
                    startY = rand.Next(0 - Hoffset, height - Theight + 1 + Hoffset);


                for (int x = startX ; x <= startX + Twidth ; x += Twidth)
                {
                    for (int y = startY ; y <= startY + Theight ; y++)
                        if (x >= 0 && x < width && y >= 0 && y < height)
                            if (rand.Next(13) == 0)
                                map[x, y] = new Floor(x, y);
                            else if (rand.Next(6) == 0)
                                map[x, y] = new Rubble(x, y);
                            else
                                map[x, y] = new Wall(x, y);
                }
                for (int y = startY ; y <= startY + Theight ; y += Theight)
                {
                    for (int x = startX ; x <= startX + Twidth ; x++)
                        if (y >= 0 && y < height && x >= 0 && x < width)
                            if (rand.Next(13) == 0)
                                map[x, y] = new Floor(x, y);
                            else if (rand.Next(6) == 0)
                                map[x, y] = new Rubble(x, y);
                            else
                                map[x, y] = new Wall(x, y);
                }

                for (int x = startX + 1 ; x < startX + Twidth ; x++)
                    for (int y = startY + 1 ; y < startY + Theight ; y++)
                    {
                        if (y >= 0 && y < height && x >= 0 && x < width)
                        {
                            if (map[x, y] is House || map[x, y] is Tree || map[x, y] is Tower)
                                map[x, y] = new Floor(x, y);
                            if (rand.Next(13) == 0)
                                map[x, y] = new Grave(x, y);
                            else if (rand.Next(6) == 0)
                                map[x, y] = new Chest(x, y);
                        }
                    }
            }

            for (int a = 0 ; a < doors.Count ; a++)
            {
                Door d = doors[a];
                if (d.mapNum == mapNum)
                {
                    int x, y;
                    do
                    {
                        x = rand.Next(width);
                        y = rand.Next(height);
                    }
                    while (map[x, y] is Door);

                    d.place(x, y);
                    map[x, y] = d;

                    if (d.toMap == from)
                    {
                        Program.x = d.X;
                        Program.y = d.Y;
                    }
                }
            }

            for (int x = 0 ; x < width ; x++)
                for (int y = 0 ; y < height ; y++)
                    if (map[x, y] is Tower && y != 0)
                        if (map[x, y - 1] is Door || map[x, y - 1] is Wall || map[x, y - 1] is Tower)
                            map[x, y] = new Floor(x, y);
                        else
                            map[x, y - 1].TowerTop();

            int numZoms = round(random((double)width * (double)height / 13.0));

            double chests = ( ( (double)numZoms + ( (double)width * (double)height / 13.0 ) )
                / ( (double)width * (double)height / 13.0 ) ) * 0.5 * 1.3;
            double graves = ( ( (double)width * (double)height / 13.0 ) /
                ( (double)numZoms + ( (double)width * (double)height / 13.0 ) ) ) * 2.0 / 1.3;

            chest = 0;
            grave = 0;
            while (chest == grave)
            {
                chest = rand.Next(2);
                grave = rand.Next(2);
            }
            double countc = ( (double)width * (double)height / 169.0 );
            for (int c = 0 ; c < 2 ; c++)
            {
                if (c == grave)
                    count = round(countc * graves);
                else if (c == chest)
                    count = round(countc * chests);

                for (int c2 = 0 ; c2 < count ; c2++)
                {
                    int x, y;

                    do
                    {
                        x = rand.Next(width);
                        y = rand.Next(height);
                    } while (map[x, y] is Wall || map[x, y] is Door ||
                        map[x, y] is Tower || ( y != height - 1 && map[x, y + 1] is Tower ));

                    if (c == chest)
                        map[x, y] = new Chest(x, y);
                    else if (c == grave)
                        map[x, y] = new Grave(x, y);
                }
            }

            Program.map = map;
            startZombies(map, numZoms);
        }

        public static double random(double average)
        {
            int count = 0;
            while (true)
            {
                if ((int)( rand.NextDouble() * 13 ) == 0)
                    break;
                else
                    count++;
            }
            return ( (double)( count ) * rand.NextDouble() ) * average / 6.0;
        }

        public static int round(double number)
        {
            bool neg = false;

            if (number < 0)
                neg = true;

            if (neg)
                number *= -1;

            int result = (int)number;

            if (rand.NextDouble() < number % 1)
                result++;

            if (neg)
                result *= -1;

            return result;
        }

        public static void explode(int x, int y)
        {
            int playDist = bombDist - distance(Program.x, Program.y, x, y, bombDist);
            if (playDist > 0)
            {
                output += "You were too close to the blast and have been injured.  ";
                health -= playDist;
            }

            foreach (Zombie z in GetZombieArray())
            {
                int dist = distance(z.X, z.Y, x, y, bombDist);

                if (dist < bombDist)
                {
                    if (!( rand.Next(13) == 0 && rand.Next(dist + 1) == 0 ) &&
                        ( rand.Next(13) != 0 || !z.Large ))
                    {
                        exp++;
                        KillZombie(z);
                    }
                }
            }

            for (int xx = 0 ; xx < width ; xx++)
                for (int yy = 0 ; yy < height ; yy++)
                {
                    int dist = distance(xx, yy, x, y, bombDist);

                    if (dist < bombDist)
                    {
                        map[xx, yy].startFire(dist);

                        if (rand.Next(dist + 1) == 0 && !( rand.Next(13) == 0 ))
                        {
                            if (map[xx, yy] is Chest)
                            {
                                map[xx, yy] = new Floor(xx, yy, map[x, y]);
                            }
                            else if (map[xx, yy] is Grave)
                            {
                                if (rand.Next(dist + 2) == 0)
                                    map[xx, yy] = new Floor(xx, yy, map[x, y]);
                            }
                            else if (map[xx, yy] is House)
                            {
                                map[xx, yy] = new Rubble(xx, yy, true, map[x, y]);
                            }
                            else if (map[xx, yy] is Rubble)
                            {
                                if (rand.Next(dist + 2) == 0)
                                    map[xx, yy] = new Floor(xx, yy, map[x, y]);
                            }
                            else if (map[xx, yy] is Tower)
                            {
                                if (yy != 0)
                                    map[xx, yy - 1].TowerTop(false);
                                map[xx, yy] = new Rubble(xx, yy, true, map[x, y]);
                            }
                            else if (map[xx, yy] is Tree)
                            {
                                if (rand.Next(13 - dist) == 0)
                                    map[xx, yy] = new Rubble(xx, yy, true, map[x, y]);
                                else
                                    map[xx, yy] = new Floor(xx, yy, map[x, y]);
                            }
                            else if (map[xx, yy] is Wall)
                            {
                                if (rand.Next(13 + dist) == 0)
                                    map[xx, yy] = new Floor(xx, yy, map[x, y]);
                                else
                                    map[xx, yy] = new Rubble(xx, yy, false, map[x, y]);
                            }
                        }
                    }
                }
        }

        static int getDist(int x1, int y1, int x2, int y2, int dist, int from, int max)
        {
            if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
            {
                if (x1 == x2 && y1 == y2)
                    return dist;

                dist += map[x1, y1].view;

                if (dist >= max)
                    return max;

                return Math.Min(Math.Min(Math.Min(
                    ( from == 1 ? max : getDist(x1 + 1, y1, x2, y2, dist, 0, max) ),
                    ( from == 0 ? max : getDist(x1 - 1, y1, x2, y2, dist, 1, max) )),
                    ( from == 3 ? max : getDist(x1, y1 + 1, x2, y2, dist, 2, max) )),
                    ( from == 2 ? max : getDist(x1, y1 - 1, x2, y2, dist, 3, max) ));
            }

            return bombDist;
        }

        public static int distance(int x1, int y1, int x2, int y2, int max)
        {
            return getDist(x1, y1, x2, y2, 0, -1, max);
        }

        internal static IEnumerable<Zombie> GetZombieArray()
        {
            return zombies.ToArray();
        }
    }
}