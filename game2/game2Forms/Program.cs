using game2.game;

namespace game2Forms
{
    public static class Program
    {
#pragma warning disable CS8618 // Assuming these are set in Main() method
        private static Game _game;
        private static MainForm _form;
#pragma warning restore CS8618

        public static Game Game => _game;
        public static MainForm Form => _form;

        //#pragma warning disable CS8618 // Assuming _game is set in Main()
        //        static Program()
        //#pragma warning restore CS8618  
        //        {
        //            //Application.SetHighDpiMode(HighDpiMode.);
        //            //Application.EnableVisualStyles();
        //            //Application.SetCompatibleTextRenderingDefault(false);

        //            //Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); //SystemAware
        //            //Application.EnableVisualStyles();
        //            //Application.SetCompatibleTextRenderingDefault(false);
        //        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _game = new();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationConfiguration.Initialize();
            // Ensure the final, authoritative DPI mode is PerMonitorV2
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            _form = new();
            Application.Run(Form);
        }
    }
}