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
            //Game g = new Game();
            //Player p = new Player("dwarf", System.Drawing.Color.FloralWhite, "hi");

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


            Game.Random.ToString();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new MainMenu().Show();
            Application.Run();

            Game.Random.Dispose();
        }
    }
}
