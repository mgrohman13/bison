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
            CityWar.Game.Random.ToString();

            const double damage = 6, armor = 9, divide = 1, DamMultPercent = .39;
            const double damMult = damage * DamMultPercent, damStatic = damage - damMult;

            double sum = 0, tot = 1000000;
            for (int a = 0 ; a < tot ; ++a)
                sum += Math.Max(0, CityWar.Game.Random.Round(damStatic - armor / (double)divide) + CityWar.Game.Random.OEInt(damMult));
            Console.WriteLine(sum / tot);
            Console.WriteLine(CityWar.Attack.GetAverageDamage(damage, divide, armor, int.MaxValue));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            CityWar.Game.Random.Dispose();
        }
    }
}