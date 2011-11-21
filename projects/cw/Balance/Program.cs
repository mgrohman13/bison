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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            CityWar.Game.Random.Dispose();
        }
    }
}