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

            //int width = Console.LargestWindowWidth, height = Console.LargestWindowHeight;
            //if (width > Console.WindowWidth)
            //    Console.WindowWidth = Console.BufferWidth = width;
            //else
            //    Console.BufferWidth = Console.WindowWidth = width;
            //if (height > Console.WindowHeight)
            //    Console.WindowHeight = ( Console.BufferHeight = height + 1 ) - 1;
            //else
            //    Console.BufferHeight = ( Console.WindowHeight = height ) + 1;

            int width = 500, height = 300;
            Console.BufferWidth = width;
            Console.BufferHeight = height;

            float cur = 1, frq = 1.95f, amp = 1.3f;

            float[,] res = new float[width, height];
            for (int y = 0 ; y < height ; ++y)
                for (int x = 0 ; x < width ; ++x)
                    res[x, y] += rand.DoubleHalf(cur);

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
                        res[x, y] += sum / total;
                    }
            }

            SortedDictionary<float, Point> sort = new SortedDictionary<float, Point>();
            foreach (int x in rand.Iterate(width))
                foreach (int y in rand.Iterate(height))
                {
                    while (sort.ContainsKey(res[x, y]))
                        res[x, y] += rand.GaussianFloat();
                    sort.Add(res[x, y], new Point(x, y));
                }
            float v1 = 0, inc = 1f / res.Length;
            foreach (Point value in sort.Values)
            {
                res[value.X, value.Y] = v1;
                v1 += inc;
            }

            for (int y = 0 ; y < height ; ++y)
                for (int x = 0 ; x < width ; ++x)
                {
                    float val = 0, count = 0;
                    for (int a = -2 ; a < 3 ; ++a)
                        if (x + a >= 0 && x + a < width)
                            for (int b = -2 ; b < 3 ; ++b)
                                if (y + b >= 0 && y + b < height)
                                {
                                    float mult = 1 / ( a * a + b * b + .3f );
                                    val += res[x + a, y + b] * mult;
                                    count += mult;
                                }
                    const int tot = 100;
                    val /= count / tot;
                    int num = 0;
                    if (val < ( num += 25 ))
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    else if (val < ( num += 20 ))
                        Console.BackgroundColor = ConsoleColor.Blue;
                    else if (val < ( num += 4 ))
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                    else if (val < ( num += 6 ))
                        Console.BackgroundColor = ConsoleColor.Yellow;
                    else if (val < ( num += 15 ))
                        Console.BackgroundColor = ConsoleColor.Green;
                    else if (val < ( num += 10 ))
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    else if (val < ( num += 7 ))
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    else if (val < ( num += 5 ))
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    else if (val < ( num += 4 ))
                        Console.BackgroundColor = ConsoleColor.Gray;
                    else if (val < ( num += 4 ) && num == tot)
                        Console.BackgroundColor = ConsoleColor.White;
                    else
                        throw new Exception();
                    Console.Write(' ');
                }

            Console.SetCursorPosition(0, 0);

            rand.Dispose();
            Console.ReadKey();
        }

        private struct Point
        {
            public int X, Y;
            public Point(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
            public override int GetHashCode()
            {
                return X + Y * Console.BufferWidth;
            }
            public override string ToString()
            {
                return string.Format("({0},{1})", X, Y);
            }
        }
    }
}
