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
        private static readonly MTRandom rand = new MTRandom();

        public static void Main(string[] args)
        {
            rand.StartTick();

            //Console.BufferHeight *= 2;
            //Console.BufferWidth *= 2;
            //const double AvgSeedSize = 390;
            //const int max = MTRandom.MAX_SEED_SIZE - 1;
            //uint[] seed = MTRandom.GenerateSeed((ushort)( rand.WeightedInt(max, ( AvgSeedSize - 1.0 ) / max) + 1 ));
            //Write(new MTRandom(true, seed));
            //seed[rand.Next(seed.Length)] ^= ( (uint)1 << rand.Next(32) );
            //Write(new MTRandom(true, seed));

            //int digits = 6;
            //int max = int.Parse("".PadLeft(digits, '9'));
            //string format = "".PadLeft(digits, '0');
            //double sum = 0;
            //const int baseCost = 402;
            //const float StartAmt = .26f, inc = 50.1f;
            //int upper = (int)Math.Ceiling(baseCost - inc + inc * StartAmt);
            //int[] val = new int[upper + 1];
            //for (int x = 0 ; x < max ; ++x)
            //{
            //    int l = rand.Round(rand.Weighted(baseCost - inc, StartAmt) + inc * StartAmt);
            //    sum += l;
            //    val[l]++;
            //}
            //Console.BufferHeight = baseCost + 3;
            //int runtot = 0;
            //for (int x = (int)( inc * StartAmt ) ; x <= upper ; ++x)
            //    Console.WriteLine("{0} - {1} - {3} - {2}", x.ToString("000"), val[x].ToString(format),
            //            ( ( max - runtot ) / ( 1.0 + upper - x ) ).ToString(format.Substring(0, digits - 2)), ( runtot += val[x] ).ToString(format));
            //Console.WriteLine();
            //Console.WriteLine(sum / max);
            //Console.WriteLine(StartAmt * baseCost);

            //while (true)
            //{
            //    var addUnits = Enumerable.Range(13, 21).Where(i => rand.Bool(1 / ( 1.0 + i )));
            //    bool any = addUnits.Any(i => i > 16.9);
            //    Console.WriteLine(any);
            //    bool any2 = false;
            //    foreach (int i in addUnits)
            //    {
            //        Console.WriteLine(i);
            //        if (i > 16.9)
            //            any2 = true;
            //    }
            //    Console.WriteLine(any2);
            //    Console.ReadKey(true);
            //    Console.WriteLine();
            //}


            IEnumerable<Point> e0 = rand.Iterate(-1, 1, -1, 1);
            IDictionary<int, int> occ1 = new SortedDictionary<int, int>();
            IDictionary<int, int> occ2 = new SortedDictionary<int, int>();
            for (int c = -1 ; c < 2 ; ++c)
            {
                occ1.Clear();
                occ2.Clear();
                for (int a = 0 ; a < 100000 ; ++a)
                {
                    int idx = 0;
                    foreach (Point i in e0)
                    {
                        if (i.X == c)
                        {
                            int v;
                            occ1.TryGetValue(idx, out v);
                            occ1[idx] = v + 1;
                        }
                        if (i.Y == c)
                        {
                            int v;
                            occ2.TryGetValue(idx, out v);
                            occ2[idx] = v + 1;
                        }
                        ++idx;
                    }
                }
                foreach (var p in occ1)
                    Console.WriteLine("{0} {1}", p.Key, p.Value);
                Console.WriteLine();
                foreach (var p in occ2)
                    Console.WriteLine("{0} {1}", p.Key, p.Value);
                Console.WriteLine();
            }

            for (int i = 0 ; i < 13 ; ++i)
            {
                var arr = new[] { 0, 1, 2 };
                rand.Shuffle(arr);
                foreach (var p in arr)
                    Console.WriteLine(p);
                Console.WriteLine();
            }
            IEnumerable<int> e1 = rand.Iterate(3);
            Dictionary<int, int> occ = new Dictionary<int, int>();
            for (int c = 0 ; c < 3 ; ++c)
            {
                occ.Clear();
                for (int a = 0 ; a < 100000 ; ++a)
                {
                    int idx = 0;
                    foreach (int i in e1)
                    {
                        if (i == c)
                        {
                            int v;
                            occ.TryGetValue(idx, out v);
                            occ[idx] = v + 1;
                        }
                        ++idx;
                    }
                }
                foreach (var p in occ)
                    Console.WriteLine("{0} {1}", p.Key, p.Value);
                Console.WriteLine();
            }
            while (true)
            {
                foreach (var v in e1)
                {
                    if (v == 2)
                        break;
                    Console.WriteLine(v);
                }
                Console.WriteLine();
                Console.ReadKey(true);
            }


            rand.Dispose();
            Console.ReadKey();
        }

        //private static void Write(MTRandom rand)
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
        //private static void Write(uint seed)
        //{
        //    Console.Write(Convert.ToString(seed, 2).PadLeft(32, '0'));
        //}

        #region CityWar map
        private static void CWMapTest()
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
        #endregion //CityWar map

        #region terrain

        private static void GenerateTerrain()
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

        private static void DrawTerrain(float height, float temp, float rain)
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

            if (temp < 39)
                Console.BackgroundColor = ConsoleColor.White; // glacier
            else if (height < .169)
                Console.BackgroundColor = ConsoleColor.DarkBlue; // deep sea
            else if (height < .39)
                Console.BackgroundColor = ConsoleColor.Blue; // sea
            else if (height > .97)
                Console.BackgroundColor = ConsoleColor.White; // alpine glacier
            else if (height > .87)
                if (height % .02f < ( height - .87f ) / 5f)
                    Console.BackgroundColor = ConsoleColor.DarkGray; // mountain
                else
                    Console.BackgroundColor = ConsoleColor.Gray; // alpine tundra
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

        private static float DoStat(int width, int height, Terrain[,] res, Terrain.GetStat Get, Terrain.SetStat Set)
        {
            float cur = 1, frq = 1.95f, amp = 1.3f;
            Dictionary<Point, float> points = new Dictionary<Point, float>();
            for (int dim = rand.Round(res.Length / frq) ; dim >= 3 ; dim = rand.GaussianInt(dim / frq, .03f))
            {
                cur *= rand.GaussianInt(amp, .03f);

                points.Clear();
                for (int a = 0 ; a < dim ; ++a)
                    points[new Point(rand.Next(width), rand.Next(height))] = rand.DoubleHalf(cur);
                if (points.Count < 2 + rand.OEInt())
                    break;

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
            int v1 = 0;
            float div = res.Length - 1;
            foreach (Point value in sort.Values)
                Set(res[value.X, value.Y], v1++ / div);
            return cur;
        }

        private class Terrain
        {
            private float height, temp, rain;

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

        #endregion //terrain
    }
}
