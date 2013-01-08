using System;
using System.Collections.Generic;
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

            //uint v = rand.NextUInt();
            //MTRandom r1 = new MTRandom(new uint[] { v, v, v, v });
            //MTRandom r2 = new MTRandom(new uint[] { v, v, v, v, v + 8, v + 8, v + 8, v + 8 });
            //Console.WriteLine(r1.NextBits(64));
            //Console.WriteLine(r2.NextBits(64));

            //Console.BufferHeight *= 2;
            //Console.BufferWidth *= 2;
            //const double AvgSeedSize = 390;
            //const int max = MTRandom.MAX_SEED_SIZE - 1;
            //uint[] seed = MTRandom.GenerateSeed((ushort)( rand.WeightedInt(max, ( AvgSeedSize - 1.0 ) / max) + 1 ));
            //Write(new MTRandom(true, seed));
            //seed[rand.Next(seed.Length)] ^= ( (uint)1 << rand.Next(32) );
            //Write(new MTRandom(true, seed));

            int digits = 9;
            int max = int.Parse("".PadLeft(digits, '9'));
            string format = "".PadLeft(digits, '0');
            double sum = 0;
            int[] val = new int[MTRandom.MAX_SEED_SIZE + 2];
            for (int x = 0 ; x < max ; ++x)
            {
                rand.Gaussian();
                const float AvgSeedSize = 13;
                const int wmax = MattUtil.MTRandom.MAX_SEED_SIZE - 1;
                int l = rand.WeightedInt(wmax, ( AvgSeedSize - 1f ) / wmax) + 1;
                sum += l;
                val[l]++;
            }

            Console.BufferHeight = MTRandom.MAX_SEED_SIZE + 13;

            int runtot = 0;
            for (int x = 0 ; x < MTRandom.MAX_SEED_SIZE + 2 ; ++x)
                Console.WriteLine("{0} - {1} - {3} - {2}", x.ToString("000"), val[x].ToString(format),
                        ( ( max - runtot ) / ( 1.0 + MTRandom.MAX_SEED_SIZE - x ) ).ToString(format.Substring(0, digits - 2)), ( runtot += val[x] ).ToString(format));
            Console.WriteLine();
            Console.WriteLine(MTRandom.MAX_SEED_SIZE);
            Console.WriteLine(sum / max);

            Console.ReadKey();

            //do
            //{
            //    int width = Console.LargestWindowWidth, height = Console.LargestWindowHeight;
            //    if (width > Console.WindowWidth)
            //        Console.WindowWidth = Console.BufferWidth = width;
            //    else
            //        Console.BufferWidth = Console.WindowWidth = width;
            //    if (height > Console.WindowHeight)
            //        Console.WindowHeight = ( Console.BufferHeight = height + 1 ) - 1;
            //    else
            //        Console.BufferHeight = ( Console.WindowHeight = height ) + 1;

            //    for (int y = 0 ; y < height ; ++y)
            //        for (int x = 0 ; x < width ; ++x)
            //            DrawTerrain(.5f, ( x + .5f ) / width, 1 - ( y + .5f ) / height);

            //    Console.SetCursorPosition(0, 0);
            //} while (Console.ReadKey(true).Key != ConsoleKey.Q);

            //do
            //{
            //    int width = Console.LargestWindowWidth, height = Console.LargestWindowHeight;
            //    if (width > Console.WindowWidth)
            //        Console.WindowWidth = Console.BufferWidth = width;
            //    else
            //        Console.BufferWidth = Console.WindowWidth = width;
            //    if (height > Console.WindowHeight)
            //        Console.WindowHeight = ( Console.BufferHeight = height + 1 ) - 1;
            //    else
            //        Console.BufferHeight = ( Console.WindowHeight = height ) + 1;

            //    Tile[,] res = new Tile[width, height];
            //    for (int y = 0 ; y < height ; ++y)
            //        for (int x = 0 ; x < width ; ++x)
            //            res[x, y] = new Tile();

            //    DoStat(width, height, res, Tile.GetHeight, Tile.SetHeight);
            //    DoStat(width, height, res, Tile.GetRain, Tile.SetRain);
            //    DoStat(width, height, res, Tile.GetTemp, Tile.SetTemp);

            //    float equator = rand.FloatHalf(), eqDif = rand.GaussianCapped(1f - Math.Abs(.5f - equator), .169f, .39f);

            //    for (int y = 0 ; y < height ; ++y)
            //        for (int x = 0 ; x < width ; ++x)
            //        {
            //            float h = 0, t = 0, r = 0, count = 0;
            //            for (int a = -2 ; a < 3 ; ++a)
            //                if (x + a >= 0 && x + a < width)
            //                    for (int b = -2 ; b < 3 ; ++b)
            //                        if (y + b >= 0 && y + b < height)
            //                        {
            //                            float mult = 1 / ( a * a + b * b + .3f );
            //                            h += Tile.GetHeight(res[x + a, y + b]) * mult;
            //                            t += Tile.GetTemp(res[x + a, y + b]) * mult;
            //                            r += Tile.GetRain(res[x + a, y + b]) * mult;
            //                            count += mult;
            //                        }
            //            h /= count;
            //            t /= count;
            //            r /= count;

            //            double pow = .5 + 3 * Math.Abs(( y + .5 ) / height - equator) * eqDif;
            //            t = (float)Math.Pow(t, pow * pow);
            //            DrawTerrain(h, t, r);
            //        }

            //    Console.SetCursorPosition(0, 0);
            //} while (Console.ReadKey(true).Key != ConsoleKey.Q);

            rand.Dispose();
        }

        //private static void Write(MTRandom rand)
        //{
        //    Console.WriteLine("seed:");
        //    foreach (uint seed in rand.Seed)
        //        Write(seed);
        //    Console.WriteLine();
        //    Console.WriteLine();
        //    Console.WriteLine("state:");
        //    Write(rand.lcg);
        //    Write(rand.lfsr);
        //    Write(rand.mwc1);
        //    Write(rand.mwc2);
        //    Console.WriteLine();
        //    foreach (uint v in rand.mt)
        //        Write(v);
        //    Console.WriteLine();
        //    Console.WriteLine();
        //}

        //private static void Write(uint seed)
        //{
        //    Console.Write(Convert.ToString(seed, 2).PadLeft(32, '0'));
        //}

        //private static void DrawTerrain(float height, float temp, float rain)
        //{
        //    if (height > .39 && height < .5)
        //    {
        //        rain = (float)Math.Pow(rain, 1 + ( height - .5 ) * 7.8);
        //    }
        //    else if (height > .75 && height < .87)
        //    {
        //        rain = (float)Math.Pow(rain, 1 + ( .75 - height ) * 3.9);
        //        temp -= .13f;
        //        if (temp > 0)
        //            temp = (float)Math.Pow(temp, 1 + ( height - .75 ) * 6.5);
        //        temp += .13f;
        //    }

        //    temp *= 500;
        //    rain *= temp;

        //    if (temp < 39)
        //        Console.BackgroundColor = ConsoleColor.White; // glacier
        //    else if (height < .169)
        //        Console.BackgroundColor = ConsoleColor.DarkBlue; // deep sea
        //    else if (height < .39)
        //        Console.BackgroundColor = ConsoleColor.Blue; // sea
        //    else if (height > .97)
        //        Console.BackgroundColor = ConsoleColor.White; // alpine glacier
        //    else if (height > .87)
        //        if (height % .02f < ( height - .87f ) / 5f)
        //            Console.BackgroundColor = ConsoleColor.DarkGray; // mountain
        //        else
        //            Console.BackgroundColor = ConsoleColor.Gray; // alpine tundra
        //    else if (( temp > 100 ) && ( ( rain < ( temp - 100 ) * ( temp - 100 ) / 1000f ) ))
        //        if (rain < temp - 313)
        //            Console.BackgroundColor = ConsoleColor.Red; // sub. desert
        //        else
        //            Console.BackgroundColor = ConsoleColor.DarkRed; // temp. grass / desert
        //    else if (( temp > 300 ) && ( rain < 25 + 490 / ( 1.0 + Math.Pow(Math.E, ( 390 - temp ) / 26.0) ) ))
        //        if (rain < 260 + ( temp - 333 ) * ( temp - 333 ) / 260f)
        //            Console.BackgroundColor = ConsoleColor.Yellow; // trop. seas. forest / savannah
        //        else
        //            Console.BackgroundColor = ConsoleColor.Cyan; // trop. rain forest
        //    else if (rain < ( temp - 65 ) / 2.1f)
        //        Console.BackgroundColor = ConsoleColor.DarkYellow; // woodland / shrubland
        //    else if (( temp > 200 ) || ( ( temp > 130 ) && ( rain < 21 + 196 / ( 1.0 + Math.Pow(Math.E, ( 169 - temp ) / 13.0) ) ) ))
        //        if (( temp < 169 ) || rain < 125 + 10 * Math.Sqrt(temp - 169))
        //            Console.BackgroundColor = ConsoleColor.Green; // temp. dec. forest
        //        else
        //            Console.BackgroundColor = ConsoleColor.DarkCyan; // temp. rain forest
        //    else if (( temp > 100 ) || ( rain < 120 / ( 1.0 + Math.Pow(Math.E, ( 78 - temp ) / 13.0) ) ))
        //        Console.BackgroundColor = ConsoleColor.DarkGreen; // taiga
        //    else
        //        Console.BackgroundColor = ConsoleColor.Gray; // tundra

        //    Console.Write(' ');
        //}

        //private static float DoStat(int width, int height, Tile[,] res, Tile.GetStat Get, Tile.SetStat Set)
        //{
        //    float cur = 1, frq = 1.95f, amp = 1.3f;
        //    Dictionary<Point, float> points = new Dictionary<Point, float>();
        //    for (int dim = rand.Round(res.Length / frq) ; dim >= 3 ; dim = rand.GaussianInt(dim / frq, .03f))
        //    {
        //        cur *= rand.GaussianInt(amp, .03f);

        //        points.Clear();
        //        for (int a = 0 ; a < dim ; ++a)
        //            points[new Point(rand.Next(width), rand.Next(height))] = rand.DoubleHalf(cur);
        //        if (points.Count < 2 + rand.OEInt())
        //            break;

        //        for (int y = 0 ; y < height ; ++y)
        //            for (int x = 0 ; x < width ; ++x)
        //            {
        //                int modX = x + rand.GaussianInt(), modY = y + rand.GaussianInt();

        //                int found = int.MaxValue, total = 0;
        //                float sum = 0;
        //                foreach (KeyValuePair<Point, float> pair in points)
        //                {
        //                    int distX = ( pair.Key.X - modX );
        //                    int distY = ( pair.Key.Y - modY );
        //                    int dist = distX * distX + distY * distY;
        //                    if (dist < found)
        //                    {
        //                        sum = pair.Value;
        //                        total = 1;
        //                        found = dist;
        //                    }
        //                    else if (dist == found)
        //                    {
        //                        sum += pair.Value;
        //                        ++total;
        //                    }
        //                }
        //                Set(res[x, y], Get(res[x, y]) + sum / total);
        //            }
        //    }

        //    SortedDictionary<float, Point> sort = new SortedDictionary<float, Point>();
        //    foreach (Point p in rand.Iterate(width, height))
        //    {
        //        int x = p.X, y = p.Y;
        //        while (sort.ContainsKey(Get(res[x, y])))
        //            Set(res[x, y], Get(res[x, y]) + rand.GaussianFloat());
        //        sort.Add(Get(res[x, y]), p);
        //    }
        //    int v1 = 0;
        //    float div = res.Length - 1;
        //    foreach (Point value in sort.Values)
        //        Set(res[value.X, value.Y], v1++ / div);
        //    return cur;
        //}


        //private class Tile
        //{
        //    private float height, temp, rain;

        //    public Tile()
        //    {
        //        height = rand.DoubleHalf(1f);
        //        temp = rand.DoubleHalf(1f);
        //        rain = rand.DoubleHalf(1f);
        //    }

        //    public delegate float GetStat(Tile tile);
        //    public delegate void SetStat(Tile tile, float stat);
        //    public static float GetHeight(Tile tile)
        //    {
        //        return tile.height;
        //    }
        //    public static void SetHeight(Tile tile, float stat)
        //    {
        //        tile.height = stat;
        //    }
        //    public static float GetTemp(Tile tile)
        //    {
        //        return tile.temp;
        //    }
        //    public static void SetTemp(Tile tile, float stat)
        //    {
        //        tile.temp = stat;
        //    }

        //    public static float GetRain(Tile tile)
        //    {
        //        return tile.rain;
        //    }
        //    public static void SetRain(Tile tile, float stat)
        //    {
        //        tile.rain = stat;
        //    }
        //}
    }
}
