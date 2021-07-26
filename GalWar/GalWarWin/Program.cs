using System;
using System.Collections.Generic;
using System.Linq;
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
            //kick off random ticker
            Game.Random.StartTick();


            //MainForm mf = new MainForm();
            //for (int b = 0 ; b < 100 ; ++b)
            //{
            //    mf.btnNewGame_Click(null, null);
            //    Game game = MainForm.Game;
            //    for (int a = 0 ; a < 6 ; ++a)
            //    {
            //        Colony colony = game.CurrentPlayer.GetColonies()[0];
            //        int quality = colony.Planet.Quality;
            //        int gold = (int)( game.CurrentPlayer.Gold * 10 );
            //        int prod = colony.Production;
            //        Console.WriteLine(a + "\t" + quality + "\t" + gold + "\t" + prod);
            //        game.EndTurn(mf);
            //    }
            //    Console.WriteLine();
            //}
            //;


            //ShipDesign.Test();
            //return;

            //for (int a = 0 ; a < 13 ; ++a)
            //    CombatPerformance();

            //Console.WriteLine();
            //Console.WriteLine(CombatForm.cc);

            //for (int a = 1 ; a < 16 ; ++a)
            //{
            //    double tot = 0;
            //    double div = 0;
            //    for (int b = 1 ; b < 31 ; ++b)
            //    {
            //        double avg, c;
            //        Consts.GetDamageTable(a, b, out avg, out c);
            //        //Console.Write("{0:0.00}\t", avg);
            //        double diff = ( Math.Abs(a - b) - 3.0 ) / ( a + 3.9 );
            //        if (diff < .6)
            //        {
            //            diff = 1;
            //            //Math.Abs(1 - diff);
            //            tot += avg * diff;
            //            div += diff;
            //            Console.Write("{0:0.00}\t", diff);
            //        }
            //        else
            //            Console.Write("0.00\t");
            //    }
            //    double d = tot / div, e = ShipDesign.GetStatValue(a);
            //    Console.WriteLine("{0:0.00}\t{1:00.0}\t{2:0.000}", d, e, e / d);
            //}

            ShipDesign.DoCostTable();

            Player.VerifyRounded(1 / Consts.ProductionForGold);
            Player.VerifyRounded(Consts.GoldForProduction);

            //run app
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            Game.Random.Dispose();
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
