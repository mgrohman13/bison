using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Game.Random.ToString();


            //double minC = int.MaxValue, maxC = 0, avgC = 0, minW = int.MaxValue, maxW = 0, avgW = 0;

            //int times = 1000000;
            //for (int a = 0 ; a < times ; ++a)
            //{
            //    bool city = ( a % 2 == 0 );
            //    p.NewPlayer(g, city, new[] { "armor", "armor", "armor" }, 300);
            //    if (city)
            //    {
            //        minC = Math.Min(minC, p.Relic);
            //        maxC = Math.Max(maxC, p.Relic);
            //        avgC += p.Relic;
            //    }
            //    else
            //    {
            //        minW = Math.Min(minW, p.Relic);
            //        maxW = Math.Max(maxW, p.Relic);
            //        avgW += p.Relic;
            //    }
            //}
            //avgC /= times / 2.0;
            //avgW /= times / 2.0;

            //Console.WriteLine();

            //Console.WriteLine(minC);
            //Console.WriteLine(avgC);
            //Console.WriteLine(maxC);

            //Console.WriteLine();

            //Console.WriteLine(minW);
            //Console.WriteLine(avgW);
            //Console.WriteLine(maxW);


            //Player p1 = new Player("Dwarf", System.Drawing.Color.FloralWhite, "hi");
            //Player p2 = new Player("Fae", System.Drawing.Color.FloralWhite, "hi");
            //Player p3 = new Player("Human", System.Drawing.Color.FloralWhite, "hi");
            //Game g = new Game(new[] { p1, p2, p3 }, 13);

            //PrintCosts(Portal.SplitPortalCost(g, "Dwarf"));
            //PrintCosts(Portal.SplitPortalCost(g, "Fae"));
            //PrintCosts(Portal.SplitPortalCost(g, "Human"));


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new MainMenu().Show();
            Application.Run();

            Game.Random.Dispose();
        }

        private static void PrintCosts(Dictionary<CostType, int[]> dictionary)
        {
            Console.WriteLine("{0}\t{1}", dictionary[CostType.Air][0], dictionary[CostType.Air][1]);
            Console.WriteLine("{0}\t{1}", dictionary[CostType.Death][0], dictionary[CostType.Death][1]);
            Console.WriteLine("{0}\t{1}", dictionary[CostType.Earth][0], dictionary[CostType.Earth][1]);
            Console.WriteLine("{0}\t{1}", dictionary[CostType.Nature][0], dictionary[CostType.Nature][1]);
            Console.WriteLine("{0}\t{1}", dictionary[CostType.Water][0], dictionary[CostType.Water][1]);
        }
    }
}
