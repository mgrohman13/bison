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

            //const float v = 0.70710678118654752440084436210485f;

            //float d1 = 0;
            //float d2 = 0;
            //float t1 = 100000;
            //float av = GetMoveSoldiers(r, 500, 100, 100, false);
            //float av2 = GetMoveSoldiers(r, 500, 100, 250, false);
            //for (int a = 0 ; a < t1 ; ++a)
            //{
            //    float b = GetMoveSoldiers(r, 500, 100, 100, true) - av;
            //    d1 += b * b;
            //    b = GetMoveSoldiers(r, 500, 100, 250, true) - av2;
            //    d2 += b * b;

            //    //float b = 0;
            //    //for (int c = 0 ; c < 100 ; ++c)
            //    //    b += r.Gaussian(v);
            //    //d1 += b * b;
            //    //b = r.Gaussian(v * (float)Math.Sqrt(100));
            //    //d2 += b * b;
            //}

            //Console.WriteLine(Math.Sqrt(d1 / t1));
            //Console.WriteLine(Math.Sqrt(d2 / t1));

            //uint[] seed = new uint[1 + r.OEInt(21)];
            //for (int b = 0 ; b < seed.Length ; ++b)
            //    seed[b] = r.NextUInt();

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

                //Thread.Sleep(a += r.OEInt(.0003 * rs.Length));
                //Thread.Sleep(390);
            }

            r.Dispose();
            Console.ReadKey();
        }

        //private const float MoveSoldiersMult = 3.9f;
        //private const float SoldiersRndm = .26f;

        //private static float GetMoveSoldiers(MTRandom r, int population, double soldiers, int movePop, bool doMove)
        //{
        //    float moveSoldiers = 0;
        //    if (soldiers > 0)
        //    {
        //        if (population == movePop)
        //            moveSoldiers = (float)soldiers;
        //        else
        //            for (int mov = 1 ; mov <= movePop ; ++mov)
        //            {
        //                float available = (float)( soldiers - moveSoldiers );
        //                float chunk = available * MoveSoldiersMult / ( MoveSoldiersMult + population - mov );
        //                if (doMove)
        //                    chunk = r.GaussianCapped(chunk, SoldiersRndm, Math.Max(0, 2 * chunk - available));
        //                moveSoldiers += chunk;
        //            }
        //    }
        //    return moveSoldiers;
        //}

        //private static float GetMoveSoldiers2(MTRandom r, int population, double soldiers, int movePop, bool doMove)
        //{
        //    float moveSoldiers = 0;
        //    if (soldiers > 0)
        //    {
        //        if (population == movePop)
        //        {
        //            moveSoldiers = (float)soldiers;
        //        }
        //        else
        //        {
        //            for (int mov = 1 ; mov <= movePop ; ++mov)
        //                moveSoldiers += (float)( soldiers - moveSoldiers ) * MoveSoldiersMult / ( MoveSoldiersMult + population - mov );
        //            if (doMove)
        //                moveSoldiers = r.Gaussian(moveSoldiers, SoldiersRndm * (float)Math.Sqrt(movePop));
        //        }
        //    }
        //    return moveSoldiers;
        //}
    }
}
