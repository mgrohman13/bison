using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
            CityWar.Game.Random.ToString();

            //CityWar.Player[] players = new CityWar.Player[6];
            //players[0] = new CityWar.Player(System.Drawing.Color.LightBlue, "Blue");
            //players[1] = new CityWar.Player(System.Drawing.Color.Magenta, "Pink");
            //players[2] = new CityWar.Player(System.Drawing.Color.Red, "Red");
            //players[3] = new CityWar.Player(System.Drawing.Color.DarkBlue, "Black");
            //players[4] = new CityWar.Player(System.Drawing.Color.Green, "Green");
            //players[5] = new CityWar.Player(System.Drawing.Color.Yellow, "Yellow");
            //CityWar.Data.StartNewGame(players, 18, 18);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new MainMenu().Show();
            Application.Run();

            CityWar.Game.Random.Dispose();
        }

        #region old shit - ignore this

        //Application.EnableVisualStyles();
        //Application.SetCompatibleTextRenderingDefault(false);
        //Application.Run();

        //CityWar.TargetType[] t = new CityWar.TargetType[2];
        //t[0] = CityWar.TargetType.Ground;
        //t[1] = CityWar.TargetType.Water;
        //string s =t.ToString();

        //CityWar.UnitSchema c = new CityWar.UnitSchema();

        /*
Infantry*	6	G	18	0	3	1	G	1	1	2	1	5.5	G	1	1	2	1	5.5	G	1	1	2	1	5.5
Commando*	10	G	15	1	3	1	GA	2	2	7	3	4.7	GA	2	2	7	3	4.7	GA	2	2	7	3	4.7
Sniper*	    13	G	21	4	3	1	GW	3	1	3	1	6.5	GW	3	1	3	1	6.5	GW	3	1	3	1	6.5
Zepplin	    17	A	18	0	1	3	A	3	1	3	1	6.5	GWA	1	1	1	1	4.5	-	-	-	-	-	-
Destroyer*	17	W	39	1	2	3	W	0	2	8	3	5.0	W	0	2	8	3	5.0	W	0	2	8	3	5.0
Cruiser 	18	W	28	4	2	2	A	3	2	5	2	6.0	W	3	2	1	2	4.0	GWA	1	2	1	2	4.0
Hummer	    23	G	30	3	2	2	A	3	2	5	2	6.0	GWA	2	2	2	2	4.5	-	-	-	-	-	-
Battleship	31	W	44	8	8	1	G	3	4	0	2	7.0	G	2	2	9	3	5.3	WA	2	2	2	2	4.5
Carrier	    35	W	39	1	4	2	W	2	2	6	3	4.3	WA	2	2	2	2	4.5	GWA	1	1	2	1	5.5
Tank	    37	G	22	8	1	3	GW	2	2	9	3	5.3	A	1	1	2	1	5.5	-	-	-	-	-	-
Aircraft	44	A	28	4	1	5	GW	3	4	0	3	4.7	GWA	2	2	2	2	4.5	A	1	1	0	1	3.5
Shield	    30S	GWA	13	10	1	0	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
Zombie*	    12D	G	42	2	5	1	G	0	1	3	1	6.5	G	0	1	3	1	6.5	G	0	1	3	1	6.5
Hydra	    30W	GW	26	1	12	1	GWA	0	1	1	1	4.5	GWA	0	2	3	2	5.0	GWA	0	3	5	3	5.2
Gryphon*	31A	A	42	1	2	3	GWA	0	2	1	2	4.0	GWA	0	2	1	2	4.0	GWA	0	2	1	2	4.0
Wyrm	    32E	G	27	5	4	2	GA	0	3	10	3	6.8	GWA	1	4	0	3	4.7	-	-	-	-	-	-
Dragon	    60N	A	25	7	3	2	GWA	0	3	2	2	6.3	GWA	2	4	2	2	8.0	-	-	-	-	-	-
        */

        //c.Unit.AddUnitRow("Infantry", 6, "G", 18, 0, 3, 1, "");
        //c.Unit.AddUnitRow("Commando", 10, "G", 15, 1, 3, 1, "");
        //c.Unit.AddUnitRow("Sniper", 13, "G", 21, 4, 3, 1, "");
        //c.Unit.AddUnitRow("Zepplin", 17, "A", 18, 0, 1, 3, "");
        //c.Unit.AddUnitRow("Destroyer", 17, "W", 39, 1, 2, 3, "");
        //c.Unit.AddUnitRow("Cruiser", 18, "W", 28, 4, 2, 2, "");
        //c.Unit.AddUnitRow("Hummer", 23, "G", 30, 3, 2, 2, "");
        //c.Unit.AddUnitRow("Battleship", 31, "W", 44, 8, 8, 1, "");
        //c.Unit.AddUnitRow("Carrier", 35, "W", 39, 1, 4, 2, "");
        //c.Unit.AddUnitRow("Tank", 37, "G", 22, 8, 1, 3, "");
        //c.Unit.AddUnitRow("Aircraft", 44, "A", 28, 4, 1, 5, "");
        //c.Unit.AddUnitRow("Shield", 30, "GWA", 13, 10, 1, 0, "S");
        //c.Unit.AddUnitRow("Zombie", 12, "G", 42, 2, 5, 1, "D");
        //c.Unit.AddUnitRow("Hydra", 30, "GW", 26, 1, 12, 1, "W");
        //c.Unit.AddUnitRow("Gryphon", 31, "A", 42, 1, 2, 3, "A");
        //c.Unit.AddUnitRow("Wyrm", 32, "G", 27, 5, 4, 2, "E");
        //c.Unit.AddUnitRow("Dragon", 60, "A", 25, 7, 3, 2, "N");

        //c.Attack.AddAttackRow("G", 1, 1, 2, 1, c.Unit[0]);
        //c.Attack.AddAttackRow("G", 1, 1, 2, 1, c.Unit[0]);
        //c.Attack.AddAttackRow("G", 1, 1, 2, 1, c.Unit[0]);
        //c.Attack.AddAttackRow("GA", 2, 2, 7, 3, c.Unit[1]);
        //c.Attack.AddAttackRow("GA", 2, 2, 7, 3, c.Unit[1]);
        //c.Attack.AddAttackRow("GA", 2, 2, 7, 3, c.Unit[1]);
        //c.Attack.AddAttackRow("GW", 3, 1, 3, 1, c.Unit[2]);
        //c.Attack.AddAttackRow("GW", 3, 1, 3, 1, c.Unit[2]);
        //c.Attack.AddAttackRow("GW", 3, 1, 3, 1, c.Unit[2]);
        //c.Attack.AddAttackRow("A", 3, 1, 3, 1, c.Unit[3]);
        //c.Attack.AddAttackRow("GWA", 1, 1, 1, 1, c.Unit[3]);
        //c.Attack.AddAttackRow("W", 0, 2, 8, 3, c.Unit[4]);
        //c.Attack.AddAttackRow("W", 0, 2, 8, 3, c.Unit[4]);
        //c.Attack.AddAttackRow("W", 0, 2, 8, 3, c.Unit[4]);
        //c.Attack.AddAttackRow("A", 3, 2, 5, 2, c.Unit[5]);
        //c.Attack.AddAttackRow("W", 3, 2, 1, 2, c.Unit[5]);
        //c.Attack.AddAttackRow("GWA", 1, 2, 1, 2, c.Unit[5]);
        //c.Attack.AddAttackRow("A", 3, 2, 5, 2, c.Unit[6]);
        //c.Attack.AddAttackRow("GWA", 2, 2, 2, 2, c.Unit[6]);
        //c.Attack.AddAttackRow("G", 3, 4, 0, 2, c.Unit[7]);
        //c.Attack.AddAttackRow("G", 2, 2, 9, 3, c.Unit[7]);
        //c.Attack.AddAttackRow("WA", 2, 2, 2, 2, c.Unit[7]);
        //c.Attack.AddAttackRow("W", 2, 2, 6, 3, c.Unit[8]);
        //c.Attack.AddAttackRow("WA", 2, 2, 2, 2, c.Unit[8]);
        //c.Attack.AddAttackRow("GWA", 1, 1, 2, 1, c.Unit[8]);
        //c.Attack.AddAttackRow("GW", 2, 2, 9, 3, c.Unit[9]);
        //c.Attack.AddAttackRow("A", 1, 1, 2, 1, c.Unit[9]);
        //c.Attack.AddAttackRow("GW", 3, 4, 0, 3, c.Unit[10]);
        //c.Attack.AddAttackRow("GWA", 2, 2, 2, 2, c.Unit[10]);
        //c.Attack.AddAttackRow("A", 1, 1, 0, 1, c.Unit[10]);
        //c.Attack.AddAttackRow("G", 0, 1, 3, 1, c.Unit[11]);
        //c.Attack.AddAttackRow("G", 0, 1, 3, 1, c.Unit[11]);
        //c.Attack.AddAttackRow("G", 0, 1, 3, 1, c.Unit[11]);
        //c.Attack.AddAttackRow("GWA", 0, 1, 1, 1, c.Unit[12]);
        //c.Attack.AddAttackRow("GWA", 0, 2, 3, 2, c.Unit[12]);
        //c.Attack.AddAttackRow("GWA", 0, 3, 5, 3, c.Unit[12]);
        //c.Attack.AddAttackRow("GWA", 0, 2, 1, 2, c.Unit[13]);
        //c.Attack.AddAttackRow("GWA", 0, 2, 1, 2, c.Unit[13]);
        //c.Attack.AddAttackRow("GWA", 0, 2, 1, 2, c.Unit[13]);
        //c.Attack.AddAttackRow("GA", 0, 3, 10, 3, c.Unit[14]);
        //c.Attack.AddAttackRow("GWA", 1, 4, 0, 3, c.Unit[14]);
        //c.Attack.AddAttackRow("GWA", 0, 3, 2, 2, c.Unit[15]);
        //c.Attack.AddAttackRow("GWA", 2, 4, 2, 2, c.Unit[15]);

        //c.WriteXml("Units.xml");

        #endregion
    }
}
