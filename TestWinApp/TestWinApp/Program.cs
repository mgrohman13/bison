using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace testwin
{
    static class Program
    {
        public static MTRandom Random;

        [STAThread]
        static void Main()
        {
            Random = new MTRandom();
            Random.StartTick();

            Dictionary<int, double>[] result = Logistics.RunCombat(out String[] labels);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(labels, result));
        }
    }

}
