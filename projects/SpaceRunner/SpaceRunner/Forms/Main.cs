using System;
using System.Windows.Forms;

namespace SpaceRunner.Forms
{
    internal class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if TRACE
            ScoresForm.Scoring = false;
#endif
            Game game = Game.StaticInit();
            GameForm mainForm = new GameForm(game);

            Application.Run(mainForm);

            Game.StaticDispose();
            mainForm.Dispose();
        }
    }
}
