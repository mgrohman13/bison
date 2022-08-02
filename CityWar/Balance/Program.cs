using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CityWar;

namespace UnitBalance
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

            int damage = Game.Random.RangeInt(3, 12), divide = Game.Random.RangeInt(1, 3), armor = Game.Random.RangeInt(-1, 15), shield = Game.Random.RangeInt(0, 99);
            //const double damMult = damage * DamMultPercent, damStatic = damage - damMult;
            double sum = 0, tot = 1000000;
            for (int a = 0; a < tot; ++a)
                sum += Math.Max(0, Attack.DoDamage(damage, divide, armor, shield, out _));

            Console.WriteLine(damage);
            Console.WriteLine(divide);
            Console.WriteLine(armor);
            Console.WriteLine(shield);
            Console.WriteLine(sum / tot);
            Console.WriteLine(Attack.GetAverageDamage(damage, divide, armor, shield, int.MaxValue));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Game.Random.Dispose();
        }
    }
}