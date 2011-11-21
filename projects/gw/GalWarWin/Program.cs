using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GalWar;
using System.Drawing;
using System.ComponentModel;

namespace GalWarWin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            //for (int a = 0 ; a < 13 ; ++a)
            //    CombatPerformance();

            //Console.WriteLine();
            //Console.WriteLine(CombatForm.cc);

            //kick off random ticker
            Game.Random.ToString();

            //run app
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        //private static void CombatPerformance()
        //{
        //    const int att = 30, def = 21;
        //    const int attHP = 100, defHP = 300;

        //    S ar = new S(att, def, attHP);
        //    S dr = new S(att, def, defHP);
        //    CombatForm form = new CombatForm();
        //    form.attacker = ar;
        //    form.defender = dr;
        //    Dictionary<int, double> damageTable = new Dictionary<int, double>();
        //    for (int a = 0 ; a <= att ; ++a)
        //        for (int d = 0 ; d <= def ; ++d)
        //            AddChance<int>(damageTable, a - d, 1);

        //    if (!CombatForm.old)
        //    {
        //        double zero = damageTable[0];
        //        damageTable.Remove(0);
        //        foreach (KeyValuePair<int, double> pair in new List<KeyValuePair<int, double>>(damageTable))
        //            damageTable[pair.Key] = pair.Value / zero;
        //    }

        //    DoWorkEventArgs args = new DoWorkEventArgs(damageTable);

        //    form.RefreshShips();
        //    form.InitializeWorker();
        //    int time = Environment.TickCount;
        //    form.worker_DoWork(new BackgroundWorker(), args);
        //    time = Environment.TickCount - time;
        //    Console.WriteLine(time);
        //}

        //private static void AddChance<TKey>(Dictionary<TKey, double> dictionary, TKey key, double chance)
        //{
        //    double val;
        //    dictionary.TryGetValue(key, out val);
        //    dictionary[key] = val + chance;
        //}

        //private class S : Ship
        //{
        //    int att;
        //    int def;
        //    int hp;
        //    public S(int att, int def, int hp)
        //    {
        //        this.att = att;
        //        this.def = def;
        //        this.hp = hp;
        //        this.player = new Player(null, Color.AliceBlue);
        //    }
        //    public override int Att
        //    {
        //        get
        //        {
        //            return att;
        //        }
        //    }
        //    public override int Def
        //    {
        //        get
        //        {
        //            return def;
        //        }
        //    }
        //    public override int CurHP
        //    {
        //        get
        //        {
        //            return hp;
        //        }
        //    }
        //}
    }
}
