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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new MainMenu().Show();
            Application.Run();

            Game.Random.Dispose();
        }
    }
}
