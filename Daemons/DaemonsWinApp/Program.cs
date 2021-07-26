using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DaemonsWinApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Daemons.Player[] players = new Daemons.Player[6];
            players[0] = new Daemons.Player(System.Drawing.Color.Aqua, "Blue");
            players[1] = new Daemons.Player(System.Drawing.Color.Magenta, "Pink");
            players[2] = new Daemons.Player(System.Drawing.Color.Red, "Red");
            players[3] = new Daemons.Player(System.Drawing.Color.DarkBlue, "Black");
            players[4] = new Daemons.Player(System.Drawing.Color.Green, "Green");
            players[5] = new Daemons.Player(System.Drawing.Color.Yellow, "Yellow");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(new Daemons.Game(players, 8, 8)));
        }
    }
}