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

            double c = 1.3;
            double a = c;
            int b;
            do
            {
                b = MTRandom.GetOEIntMax(a /= c);
                Console.WriteLine("{0}\t\t{1}", a, b);
            }
            while (b > 1);

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
