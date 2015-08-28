using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace randTest
{
    class Program
    {
        static MTRandom rand = new MTRandom();

        static void Main(string[] args)
        {
            rand.StartTick();

            //SeedTest();

            CWPortalStart();

            //IterateTest();

            //BinarySearchSqrt();

            //CWMapGen();

            //GenerateTerrain();

            //rand = null;
            //while (true)
            //{
            //    Thread.Sleep(1);
            //    GC.Collect();
            //}
            //for (int a = 0 ; a < 13 ; ++a)
            //    new MTRandom().StartTick(0);

            Console.ReadKey(true);
        }

        #region SeedTest
        //static void SeedTest()
        //{
        //    Console.BufferHeight *= 2;
        //    Console.BufferWidth *= 2;
        //    const double AvgSeedSize = 520;
        //    const int max = MTRandom.MAX_SEED_SIZE - 1;
        //    uint[] seed = MTRandom.GenerateSeed((ushort)( rand.WeightedInt(max, ( AvgSeedSize - 1.0 ) / max) + 1 ));
        //    Write(new MTRandom(true, seed));
        //    seed[rand.Next(seed.Length)] ^= ( (uint)1 << rand.Next(32) );
        //    Write(new MTRandom(true, seed));
        //}
        //static void Write(MTRandom rand)
        //{
        //    Console.WriteLine("seed:");
        //    foreach (uint seed in rand.Seed)
        //        Write(seed);
        //    Console.WriteLine();
        //    Console.WriteLine();
        //    Console.WriteLine("state:");
        //    Write(rand.lcgn);
        //    Write(rand.lfsr);
        //    Write(rand.mwc1);
        //    Write(rand.mwc2);
        //    Console.WriteLine();
        //    foreach (uint v in rand.m)
        //        Write(v);
        //    Console.WriteLine();
        //    Console.WriteLine();
        //}
        static void Write(uint seed)
        {
            Console.Write(Convert.ToString(seed, 2).PadLeft(32, '0'));
        }
        #endregion //SeedTest

        #region CWPortalStart
        static void CWPortalStart()
        {
            Console.BufferHeight = 6500;
            CWPortalStart(077, 059.0177976546929, "Zombies"); //        1
            CWPortalStart(081, 077.1171385555153, "Golem"); //          2
            CWPortalStart(129, 077.1171385555153, "Bats"); //           3
            CWPortalStart(137, 066.4231558922337, "Gryphon"); //        4
            CWPortalStart(137, 053.5436012924079, "Pyroraptor"); //     5
            CWPortalStart(150, 100.4116741003120, "Shield"); //         6
            CWPortalStart(156, 067.6899674689582, "Fanglers"); //       7
            CWPortalStart(190, 063.9845569584666, "Giant"); //          8
            CWPortalStart(202, 067.6899674689582, "Salamander"); //     9
            CWPortalStart(219, 073.2961632985778, "Dryads"); //        10
            CWPortalStart(222, 100.4116741003120, "Roc"); //           11
            CWPortalStart(226, 063.9845569584666, "Scorpion"); //      12
            CWPortalStart(232, 073.2961632985778, "Unicorn"); //       13
            CWPortalStart(242, 073.2961632985778, "Treant"); //        14
            CWPortalStart(283, 041.9128572788207, "Wyrm"); //          15
            CWPortalStart(301, 067.6899674689582, "Leviathan"); //     16
            CWPortalStart(317, 077.1171385555153, "Troll"); //         17
            CWPortalStart(320, 043.4823892578576, "Wraith"); //        18
            CWPortalStart(321, 040.2159889856090, "Kraken"); //        19
            CWPortalStart(356, 059.0177976546929, "Behemoth"); //      20
            CWPortalStart(368, 037.2169472931034, "Wind Sprite"); //   21
            CWPortalStart(374, 100.4116741003120, "Pegasi"); //        22
            CWPortalStart(393, 066.4231558922337, "Wyverns"); //       23
            CWPortalStart(402, 050.1118884368771, "Hydra"); //         24
            CWPortalStart(403, 065.8706680810215, "Daemon"); //        25
            CWPortalStart(408, 065.8706680810215, "Spirit"); //        26
            CWPortalStart(435, 053.5436012924079, "Elemental"); //     27
            CWPortalStart(560, 054.5983483459748, "Dragon"); //        28
        }

        static void CWPortalStart(int baseUnitCost, double portalTurnInc, string name)
        {
            const int digits = 6;
            int max = int.Parse("".PadLeft(digits, '9'));
            string format = "".PadLeft(digits, '0');
            double sum = 0;
            const double StartAmt = .26;
            int upper = (int)Math.Ceiling(baseUnitCost - portalTurnInc + portalTurnInc * StartAmt);
            int[] val = new int[upper + 1];
            for (int x = 0 ; x < max ; ++x)
            {
                int l = rand.Round(rand.Weighted(baseUnitCost - portalTurnInc, StartAmt) + portalTurnInc * StartAmt);
                sum += l;
                val[l]++;
            }
            //Console.BufferHeight = baseUnitCost + 3;
            int runtot = 0;

            Console.WriteLine("{0} - {1} - {2}", name, sum / max, StartAmt * baseUnitCost);
            Console.WriteLine();
            for (int x = (int)( portalTurnInc * StartAmt ) ; x <= upper ; ++x)
                Console.WriteLine("{0} - {1} - {3}", x.ToString("000"), val[x].ToString(format),
                        ( ( max - runtot ) / ( 1.0 + upper - x ) ).ToString(format), ( runtot += val[x] ).ToString(format));
            Console.WriteLine();
            Console.WriteLine(baseUnitCost);
            Console.WriteLine();
            Console.WriteLine();
        }
        #endregion //CWPortalStart

        #region IterateTest
        static void IterateTest()
        {
            const int count = 4;
            IEnumerable<int> e = rand.Iterate(count);
            ThreadStart func = () =>
            {
                int[] r = new int[count];
                for (int a = 0 ; a < 222222 ; ++a)
                    lock (e)
                        foreach (int i in e)
                        {
                            if (i == 0)
                            {
                                if (rand.Bool())
                                    r[i]++;
                                break;
                            }
                            r[i]++;
                        }
                lock (typeof(Program))
                {
                    foreach (int c in r)
                        Console.WriteLine(c);
                    Console.WriteLine();
                }
            };
            const int tests = 13;
            Thread[] threads = new Thread[tests];
            for (int a = 0 ; a < tests ; ++a)
                threads[a] = new Thread(func);
            for (int a = 0 ; a < tests ; ++a)
                threads[a].Start();
        }
        #endregion //IterateTest

        #region BinarySearchSqrt
        static void BinarySearchSqrt()
        {
            do
            {
                bool f = rand.Bool();
                if (f)
                {
                    float val = rand.OEFloat();
                    //float act = (float)Math.Sqrt(val);
                    Console.WriteLine(val);
                    float s = BinarySearchSqrt(val);
                    Console.WriteLine(s);
                    Console.WriteLine(Math.Sqrt(val));
                    //if (val != act) 
                    if (s * s != val)
                        Console.WriteLine("off");
                    if (Math.Sqrt(val) * Math.Sqrt(val) != val)
                        Console.WriteLine("Math");
                }
                else
                {
                    double val = rand.OE();
                    //double act = Math.Sqrt(val);
                    Console.WriteLine(val);
                    double s = BinarySearchSqrt(val);
                    Console.WriteLine(s);
                    Console.WriteLine(Math.Sqrt(val));
                    //if (val != act)
                    if (s * s != val)
                        Console.WriteLine("off");
                    if (Math.Sqrt(val) * Math.Sqrt(val) != val)
                        Console.WriteLine("Math");
                }
                Console.WriteLine();
                Thread.Sleep(3900);
            } while (rand.Bool(999 / 1000.0));
        }
        static float BinarySearchSqrt(float n)
        {
            return (float)BinarySearchSqrt(n, true);
        }
        static double BinarySearchSqrt(double n)
        {
            return BinarySearchSqrt(n, false);
        }
        static double BinarySearchSqrt(double n, bool isFloat)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException();
            int count = 0;
            double min, max;
            if (n < 1)
            {
                min = n;
                max = 1;
            }
            else
            {
                min = 1;
                max = n;
            }
            double val;
            while (true)
            {
                ++count;
                val = ( min + max ) / 2.0;
                if (isFloat ? ( (float)val == (float)min || (float)val == (float)max ) : ( val == min || val == max ))
                {
                    if (Math.Abs(( min * min ) - n) < Math.Abs(( max * max ) - n))
                        val = min;
                    else
                        val = max;
                    break;
                }
                double sqr = val * val;
                if (isFloat ? ( (float)sqr == (float)n ) : ( sqr == n ))
                {
                    Console.WriteLine("break");
                    break;
                }
                else if (sqr < n)
                    min = val;
                else
                    max = val;
            }
            Console.WriteLine(count);
            return val;
        }
        #endregion //BinarySearchSqrt

        #region CWMapGen
        static void CWMapGen()
        {
            int width = 18, height = 18;
            Console.WindowHeight = height * 2 + 3;
            Tile[,] map = new Tile[width, height];
            CreateMap(map, width, height, (point) =>
            {
                Tile t = map[point.X, point.Y];
                char c = ' ';
                if (t != null)
                    c = t.Terrain;
                int x = point.X * 2;
                if (point.Y % 2 == 0)
                    ++x;
                Console.SetCursorPosition(x, point.Y * 2);
                Console.Write(c);
                Console.SetCursorPosition(x, point.Y * 2);
                Console.ReadKey(true);
            });
        }

        class Tile
        {
            public int x, y;
            public char Terrain;
            public Tile(int x, int y)
            {
                this.x = x;
                this.y = y;
                this.Terrain = rand.SelectValue(new[] { '_', '.', '!', 'M' });
            }
            public Tile(int x, int y, char Terrain)
            {
                this.x = x;
                this.y = y;
                this.Terrain = Terrain;
            }
        }
        static Tile[,] CreateMap(Tile[,] map, int width, int height, Action<Point> Callback)//, int numPlayers)
        {
            foreach (Point coord in rand.Iterate(width, height))
            {
                map[coord.X, coord.Y] = CreateTile(map, coord.X, coord.Y, width, height);
                Callback(coord);
            }
            return map;
        }
        static Tile CreateTile(Tile[,] map, int x, int y, int width, int height)
        {
            Tile tile = null;
            //try three times to find a neighbor that has already been initialized
            for (int i = 0 ; i < 3 ; ++i)
            {
                tile = GetTileIn(map, x, y, rand.Next(6), width, height);
                if (tile != null)
                    break;
            }
            Tile result;
            if (tile == null)
                result = new Tile(x, y);
            else
                result = new Tile(x, y, tile.Terrain);

            return result;
        }
        static Tile GetTileIn(Tile[,] map, int x, int y, int direction, int width, int height)
        {
            //this methoid is called to set up the neighbors array
            bool odd = y % 2 > 0;
            switch (direction)
            {
            case 0:
                if (odd)
                    --x;
                --y;
                break;
            case 1:
                if (!odd)
                    ++x;
                --y;
                break;
            case 2:
                --x;
                break;
            case 3:
                ++x;
                break;
            case 4:
                if (odd)
                    --x;
                ++y;
                break;
            case 5:
                if (!odd)
                    ++x;
                ++y;
                break;
            default:
                throw new Exception();
            }
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            else
                return map[x, y];
        }
        #endregion //CWMapGen

        #region GenerateTerrain
        static void GenerateTerrain()
        {
            do
            {
                int width = Console.LargestWindowWidth, height = Console.LargestWindowHeight;
                if (width > Console.WindowWidth)
                    Console.WindowWidth = Console.BufferWidth = width;
                else
                    Console.BufferWidth = Console.WindowWidth = width;
                if (height > Console.WindowHeight)
                    Console.WindowHeight = ( Console.BufferHeight = height + 1 ) - 1;
                else
                    Console.BufferHeight = ( Console.WindowHeight = height ) + 1;

                Terrain[,] res = new Terrain[width, height];
                for (int y = 0 ; y < height ; ++y)
                    for (int x = 0 ; x < width ; ++x)
                        res[x, y] = new Terrain();

                DoStat(width, height, res, Terrain.GetHeight, Terrain.SetHeight);
                DoStat(width, height, res, Terrain.GetRain, Terrain.SetRain);
                DoStat(width, height, res, Terrain.GetTemp, Terrain.SetTemp);

                float equator = rand.FloatHalf(), eqDif = rand.GaussianCapped(1f - Math.Abs(.5f - equator), .169f, .39f);

                for (int y = 0 ; y < height ; ++y)
                    for (int x = 0 ; x < width ; ++x)
                    {
                        float h = 0, t = 0, r = 0, count = 0;
                        for (int a = -2 ; a < 3 ; ++a)
                            if (x + a >= 0 && x + a < width)
                                for (int b = -2 ; b < 3 ; ++b)
                                    if (y + b >= 0 && y + b < height)
                                    {
                                        float mult = 1 / ( a * a + b * b + .3f );
                                        h += Terrain.GetHeight(res[x + a, y + b]) * mult;
                                        t += Terrain.GetTemp(res[x + a, y + b]) * mult;
                                        r += Terrain.GetRain(res[x + a, y + b]) * mult;
                                        count += mult;
                                    }
                        h /= count;
                        t /= count;
                        r /= count;

                        double pow = .5 + 3 * Math.Abs(( y + .5 ) / height - equator) * eqDif;
                        t = (float)Math.Pow(t, pow * pow);
                        DrawTerrain(h, t, r);
                    }

                Console.SetCursorPosition(0, 0);
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
        }

        static void DrawTerrain(float height, float temp, float rain)
        {
            if (height > .39 && height < .5)
            {
                rain = (float)Math.Pow(rain, 1 + ( height - .5 ) * 7.8);
            }
            else if (height > .75 && height < .87)
            {
                rain = (float)Math.Pow(rain, 1 + ( .75 - height ) * 3.9);
                temp -= .13f;
                if (temp > 0)
                    temp = (float)Math.Pow(temp, 1 + ( height - .75 ) * 6.5);
                temp += .13f;
            }

            temp *= 500;
            rain *= temp;

            if (height > .87)
                if (height % .02f < ( height - .87f ) / 5f && height < .98)
                    Console.BackgroundColor = ConsoleColor.DarkGray; // mountain
                else if (temp < 6 || height > .98)
                    Console.BackgroundColor = ConsoleColor.White; // glacier 
                else
                    Console.BackgroundColor = ConsoleColor.Gray; // alpine tundra
            else if (temp < 6)
                Console.BackgroundColor = ConsoleColor.White; // glacier
            else if (height < .169)
                Console.BackgroundColor = ConsoleColor.DarkBlue; // deep sea
            else if (height < .39)
                Console.BackgroundColor = ConsoleColor.Blue; // sea
            else if (height > .97)
                Console.BackgroundColor = ConsoleColor.White; // alpine glacier
            else if (( temp > 100 ) && ( ( rain < ( temp - 100 ) * ( temp - 100 ) / 1000f ) ))
                if (rain < temp - 313)
                    Console.BackgroundColor = ConsoleColor.Red; // sub. desert
                else
                    Console.BackgroundColor = ConsoleColor.DarkRed; // temp. grass / desert
            else if (( temp > 300 ) && ( rain < 25 + 490 / ( 1.0 + Math.Pow(Math.E, ( 390 - temp ) / 26.0) ) ))
                if (rain < 260 + ( temp - 333 ) * ( temp - 333 ) / 260f)
                    Console.BackgroundColor = ConsoleColor.Yellow; // trop. seas. forest / savannah
                else
                    Console.BackgroundColor = ConsoleColor.Cyan; // trop. rain forest
            else if (rain < ( temp - 65 ) / 2.1f)
                Console.BackgroundColor = ConsoleColor.DarkYellow; // woodland / shrubland
            else if (( temp > 200 ) || ( ( temp > 130 ) && ( rain < 21 + 196 / ( 1.0 + Math.Pow(Math.E, ( 169 - temp ) / 13.0) ) ) ))
                if (( temp < 169 ) || rain < 125 + 10 * Math.Sqrt(temp - 169))
                    Console.BackgroundColor = ConsoleColor.Green; // temp. dec. forest
                else
                    Console.BackgroundColor = ConsoleColor.DarkCyan; // temp. rain forest
            else if (( temp > 100 ) || ( rain < 120 / ( 1.0 + Math.Pow(Math.E, ( 78 - temp ) / 13.0) ) ))
                Console.BackgroundColor = ConsoleColor.DarkGreen; // taiga
            else
                Console.BackgroundColor = ConsoleColor.Gray; // tundra

            Console.Write(' ');
        }

        static float DoStat(int width, int height, Terrain[,] res, Terrain.GetStat Get, Terrain.SetStat Set)
        {
            float cur = 1, frq = 1.95f, amp = 1.5f;
            Dictionary<Point, float> points = new Dictionary<Point, float>();
            //int lim = rand.Round(Math.Sqrt(width * height / 1300.0));
            for (int dim = rand.Round(res.Length / frq) ; dim >= 2 ; dim = rand.GaussianInt(dim / frq, .03f))
            {
                cur *= rand.Gaussian(amp, .03f);

                points.Clear();
                for (int a = 0 ; a < dim ; ++a)
                    points[new Point(rand.Next(width), rand.Next(height))] = rand.DoubleHalf(cur);
                if (points.Count < rand.OEInt())
                    continue;

                for (int y = 0 ; y < height ; ++y)
                    for (int x = 0 ; x < width ; ++x)
                    {
                        int modX = x + rand.GaussianInt(), modY = y + rand.GaussianInt();

                        int found = int.MaxValue, total = 0;
                        float sum = 0;
                        foreach (KeyValuePair<Point, float> pair in points)
                        {
                            int distX = ( pair.Key.X - modX );
                            int distY = ( pair.Key.Y - modY );
                            int dist = distX * distX + distY * distY;
                            if (dist < found)
                            {
                                sum = pair.Value;
                                total = 1;
                                found = dist;
                            }
                            else if (dist == found)
                            {
                                sum += pair.Value;
                                ++total;
                            }
                        }
                        Set(res[x, y], Get(res[x, y]) + sum / total);
                    }
            }

            SortedDictionary<float, Point> sort = new SortedDictionary<float, Point>();
            foreach (Point p in rand.Iterate(width, height))
            {
                int x = p.X, y = p.Y;
                while (sort.ContainsKey(Get(res[x, y])))
                    Set(res[x, y], Get(res[x, y]) + rand.GaussianFloat());
                sort.Add(Get(res[x, y]), p);
            }
            sort.First();
            sort.Last();
            int v1 = 0;
            float div = res.Length - 1;
            foreach (Point value in sort.Values)
                Set(res[value.X, value.Y], v1++ / div);
            return cur;
        }

        class Terrain
        {
            public float height, temp, rain;

            public Terrain()
            {
                height = rand.DoubleHalf(1f);
                temp = rand.DoubleHalf(1f);
                rain = rand.DoubleHalf(1f);
            }

            public delegate float GetStat(Terrain tile);
            public delegate void SetStat(Terrain tile, float stat);
            public static float GetHeight(Terrain tile)
            {
                return tile.height;
            }
            public static void SetHeight(Terrain tile, float stat)
            {
                tile.height = stat;
            }
            public static float GetTemp(Terrain tile)
            {
                return tile.temp;
            }
            public static void SetTemp(Terrain tile, float stat)
            {
                tile.temp = stat;
            }
            public static float GetRain(Terrain tile)
            {
                return tile.rain;
            }
            public static void SetRain(Terrain tile, float stat)
            {
                tile.rain = stat;
            }
        }
        #endregion //GenerateTerrain
    }
}
