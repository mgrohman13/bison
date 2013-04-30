using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace balance
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine(CityWar.Attack.GetAverageDamage(CityWar.Balance.AverageDamage, CityWar.Balance.AverageAP, CityWar.Balance.AverageArmor, int.MaxValue));

            CityWar.Game.Random.ToString();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            CityWar.Game.Random.Dispose();
        }
    }
}