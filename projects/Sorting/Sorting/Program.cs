using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sorting
{
    static class Program
    {
        public static MattUtil.MTRandom Random;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();

            //System.Reflection.FieldInfo[] info = typeof(System.Diagnostics.StackFrame).GetFields(System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SuppressChangeType);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Graph());

            Random.Dispose();
        }
    }
}