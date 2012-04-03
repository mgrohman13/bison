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

            SimulateAll();

            rand.Dispose();
            Console.ReadKey();
        }

        private static void SimulateAll()
        {
            Console.WriteLine("100%-100%");
            SimulateSetup(24, 500, .5f, 1, 1);
            SimulateSetup(6, 1200, .8f, 1, 1);

            Console.WriteLine();
            Console.WriteLine("50%-100%");
            SimulateSetup(24, 500, .5f, .5, 1);
            SimulateSetup(6, 1200, .8f, .5, 1);

            Console.WriteLine();
            Console.WriteLine("50%-150%");
            SimulateSetup(24, 500, .5f, .5, 1.5);
            SimulateSetup(6, 1200, .8f, .5, 1.5);

            Console.WriteLine();
            Console.WriteLine("0%-200%");
            SimulateSetup(24, 500, .5f, 0, 2);
            SimulateSetup(6, 1200, .8f, 0, 2);
        }

        private static void SimulateSetup(int shots, int damage, float acc, double minMult, double maxMult)
        {
            const int trials = 1500000;
            int minDmg = GetInt(damage, minMult), maxDmg = GetInt(damage, maxMult), ships = 0;
            for (int a = 0 ; a < trials ; ++a)
                if (SimulateAttack(shots, acc, minDmg, maxDmg))
                    ++ships;
            Console.WriteLine("{0} shots,\t{1} damage,\t{2}% accuracy:", shots, damage, acc * 100);
            Console.WriteLine("{0:00.00} ships", trials / (double)ships);
        }

        private static int GetInt(int damage, double mult)
        {
            mult *= damage;
            int dmg = (int)( mult );
            if (dmg != mult)
                throw new Exception();
            return dmg;
        }

        private static bool SimulateAttack(int shots, float acc, int minDmg, int maxDmg)
        {
            const int HP = 3200;
            int total = 0;
            for (int b = 0 ; total < HP && b < shots ; ++b)
                if (rand.Bool(acc))
                    total += rand.RangeInt(minDmg, maxDmg);
            return ( total < HP );
        }
    }
}
