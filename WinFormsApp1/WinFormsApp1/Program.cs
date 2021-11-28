using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassLibrary1;

namespace WinFormsApp1
{
    static class Program
    {
        public static Game Game;

        public static Main Form;

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Game = new Game();
            Form = new Main();

            Application.Run(Form);
        }

        internal static void EndTurn()
        {
            Game.EndTurn();
        }
    }
}
