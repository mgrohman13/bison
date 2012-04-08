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

                Tile[,] res = new Tile[width, height];
                for (int y = 0 ; y < height ; ++y)
                    for (int x = 0 ; x < width ; ++x)
                        res[x, y] = new Tile();

                DoStat(width, height, res, Tile.GetHeight, Tile.SetHeight);
                DoStat(width, height, res, Tile.GetRain, Tile.SetRain);
                DoStat(width, height, res, Tile.GetTemp, Tile.SetTemp);

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
                                        h += Tile.GetHeight(res[x + a, y + b]) * mult;
                                        t += Tile.GetTemp(res[x + a, y + b]) * mult;
                                        r += Tile.GetRain(res[x + a, y + b]) * mult;
                                        count += mult;
                                    }
                        h /= count;
                        t /= count;
                        r /= count;

                        r *= 430 * t;
                        t *= 430;

                        if (h < .39)
                            Console.BackgroundColor = ConsoleColor.Blue;
                        else if (r < ( t - 130 ) * ( t - 130 ) / 900f)
                            if (r < ( t - 247 ) / 2.1f)
                                Console.BackgroundColor = ConsoleColor.DarkRed; // sub.desert
                            else
                                Console.BackgroundColor = ConsoleColor.Red; // temp. grass / desert
                        else if (r < 30 + 400 / ( 1 + Math.Pow(Math.E, .039 * ( 338 - t )) ))
                            if (r < 247 + ( t - 338 ) * ( t - 338 ) / 260f)
                                Console.BackgroundColor = ConsoleColor.Yellow; // trop.seas.forest / savannah
                            else
                                Console.BackgroundColor = ConsoleColor.Cyan; // trop.rain forest
                        else if (r < ( t - 65 ) / 2.1f)
                            Console.BackgroundColor = ConsoleColor.DarkYellow; // woodland / shrubland
                        else if (r < 39 + 169 / ( 1 + Math.Pow(Math.E, .13 * ( 169 - t )) ))
                            if (r < 169 + 10 * Math.Log(t - 169))
                                Console.BackgroundColor = ConsoleColor.Green; // temp.dec. forest
                            else
                                Console.BackgroundColor = ConsoleColor.DarkCyan; // temp.rain forest
                        else if (r < 100 / ( 1 + Math.Pow(Math.E, .39 * ( 78 - t )) ))
                            Console.BackgroundColor = ConsoleColor.DarkGreen; // taiga
                        else
                            Console.BackgroundColor = ConsoleColor.White; // tundra

                        Console.Write(' ');
                    }

                Console.SetCursorPosition(0, 0);
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);

            rand.Dispose();
        }

        private static float DoStat(int width, int height, Tile[,] res, Tile.GetStat Get, Tile.SetStat Set)
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
            foreach (int x in rand.Iterate(width))
                foreach (int y in rand.Iterate(height))
                {
                    while (sort.ContainsKey(Get(res[x, y])))
                        Set(res[x, y], Get(res[x, y]) + rand.GaussianFloat());
                    sort.Add(Get(res[x, y]), new Point(x, y));
                }
            int v1 = 0;
            float div = res.Length - 1;
            foreach (Point value in sort.Values)
                Set(res[value.X, value.Y], v1++ / div);
            return cur;
        }


        private class Tile
        {
            private float height, temp, rain;

            public Tile()
            {
                height = rand.DoubleHalf(1f);
                temp = rand.DoubleHalf(1f);
                rain = rand.DoubleHalf(1f);
            }

            public delegate float GetStat(Tile tile);
            public delegate void SetStat(Tile tile, float stat);
            public static float GetHeight(Tile tile)
            {
                return tile.height;
            }
            public static void SetHeight(Tile tile, float stat)
            {
                tile.height = stat;
            }
            public static float GetTemp(Tile tile)
            {
                return tile.temp;
            }
            public static void SetTemp(Tile tile, float stat)
            {
                tile.temp = stat;
            }

            public static float GetRain(Tile tile)
            {
                return tile.rain;
            }
            public static void SetRain(Tile tile, float stat)
            {
                tile.rain = stat;
            }
        }
    }
}
