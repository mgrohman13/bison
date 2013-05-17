using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    public partial class Log : Form
    {
        private static string log = "";
        public static void LogAttack(Unit attacker, Attack attack, Unit defender, int damage, int oldHits)
        {
            log = string.Format("{6} {0}, {1} -> {7} {2} ({3}, {5}) : {4}{8}\r\n", attacker, attack.GetLogString(), defender,
                    oldHits, -damage, defender.Armor, attacker.Owner, defender.Owner, defender.Dead ? ", Killed!" : "") + log;
        }

        public Log()
        {
            InitializeComponent();
        }

        private void Log_Load(object sender, EventArgs e)
        {
            textBox1.Text = log;
            textBox1.Select();
            textBox1.Select(log.Length - 3, 1);
            textBox1.ScrollToCaret();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
