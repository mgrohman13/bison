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
        static void Main(string[] args)
        {
            MTRandom r = new MTRandom(true);
            r.StartTick();

            //List<int> b = new List<int>();
            //b.Add(3);
            //foreach (int c in r.Iterate(b))
            //    b.Remove(c);
            //foreach (int a in r.Iterate(100))
            //    Console.WriteLine("{0:00}", a);

            int iter = 0;
            List<double> values = new List<double>();
            double current = 0;
            const double limit = 2.2795310419041614;
            for (int target = 1 ; current < limit ; ++target)
            {
                int curValue;
                double min = current, max = limit + 1 - r.NextDouble();
                do
                {
                    ++iter;
                    current = ( min + max ) / 2.0;
                    curValue = MTRandom.GetOEIntMax(current);
                    max = current;
                } while (curValue != target);
                values.Add(current);
            }
            for (int idx = 1 ; idx < values.Count ; ++idx)
            {
                double min = values[idx - 1], max = values[idx];
                while (true)
                {
                    ++iter;
                    double mid = ( min + max ) / 2.0;
                    if (mid == min || mid == max)
                        break;
                    if (MTRandom.GetOEIntMax(mid) > idx)
                        max = mid;
                    else
                        min = mid;
                }
                Console.WriteLine();
                ShowOEIntMax(min);
                ShowOEIntMax(max);
            }
            Console.Write(iter);

            //double total = 0;
            //float w = .368421048f;
            //int times = 1000000, max = 19;
            //int[] b = new int[max + 1];
            //for (int a = 0 ; a < times ; ++a)
            //{
            //    int c = r.WeightedInt(max, w);
            //    total += c;
            //    ++b[c];
            //}
            //Console.WriteLine(max * w);
            //Console.WriteLine(total / times);
            //Console.WriteLine();

            //for (int d = 0 ; d <= max ; ++d)
            //    Console.WriteLine("{0:00} - {1:00.0}%", d + 6, b[d] * 100f / times);

            r.Dispose();
            Console.ReadKey();
        }

        private static void ShowOEIntMax(double value)
        {
            string str = value.ToString("e16");
            str = DoSplit(str, '-');
            str = DoSplit(str, '+');
            Console.WriteLine("{0}\t\t{1}", str, MTRandom.GetOEIntMax(value).ToString().PadLeft(3));
        }

        private static string DoSplit(string str, char separator)
        {
            string[] split = str.Split(separator);
            if (split.Length == 2)
                return split[0] + separator + split[1].TrimStart('0').PadLeft(1, '0');
            return str;
        }

        private static void DoTickTest(MTRandom r)
        {
            Console.BufferWidth = Console.LargestWindowWidth;
            Console.WindowWidth = Console.LargestWindowWidth;
            Console.WindowHeight = Console.LargestWindowHeight;

            MTRandom[] rs = new MTRandom[Console.BufferWidth];
            for (int b = 0 ; b < rs.Length ; ++b)
            {
                rs[b] = new MTRandom(r.Seed);
                rs[b].Bool();
            }
            for (int b = 0 ; b < rs.Length ; ++b)
                rs[b].StartTick();
            Thread.Sleep(13);

            bool[,] c = new bool[Console.BufferHeight, rs.Length];
            for (int a = 0 ; a < Console.BufferHeight ; ++a)
            {
                for (int b = 0 ; b < rs.Length ; ++b)
                    c[a, b] = rs[b].Bool();
                Thread.Sleep(r.OEInt(1000));
            }

            for (int a = 0 ; a < Console.BufferHeight ; ++a)
            {
                for (int b = 0 ; b < Console.BufferWidth ; ++b)
                {
                    Console.BackgroundColor = ( b < rs.Length && c[a, b] ? ConsoleColor.White : ConsoleColor.Black );
                    Console.Write(' ');
                }
            }
        }
    }
}
