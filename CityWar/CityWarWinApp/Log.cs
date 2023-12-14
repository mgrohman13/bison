using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using CityWar;

namespace CityWarWinApp
{
    public partial class Log : Form
    {
        private static readonly string LogPath = "cw.log";

        private static string log = "";
        private static readonly List<string> lines;

        static Log()
        {
            LogPath = MainMenu.SavePath + LogPath;

            lines = new List<string>();
            try
            {
                if (File.Exists(LogPath))
                    using (StreamReader streamReader = new(LogPath))
                        while (!streamReader.EndOfStream)
                        {
                            string line = streamReader.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                                log = line + "\r\n" + log;
                        }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public const string ScoreFormat = "0.0";
        public static void LogAttack(Unit attacker, Attack attack, Unit defender, int damage, int oldHits, double relic, string suffix = "")
        {
            string score = relic.ToString(ScoreFormat);
            if (score == ScoreFormat)
                score = null;
            else if (relic > 0)
                score = "+" + score;

            string line = string.Format("{8} {0}, {1} -> {9} {2} ({3}, {6}{7}) :{5} {4}{11}{10}\r\n",
                attacker, attack.GetLogString(), defender,
                oldHits, -damage, score != null ? string.Format(" {0} :", score) : "",
                defender.Armor, defender.IsAbility(Ability.Shield) ? string.Format(", {0}%", defender.Shield) : "",
                attacker.Owner, defender.Owner, defender.Dead ? ", Killed!" : "", suffix);

            lines.Add(line);
            log = line + log;
        }

        public Log()
        {
            InitializeComponent();
        }

        private void Log_Load(object sender, EventArgs e)
        {
            textBox1.Text = log;
            textBox1.Select();
            if (log.Length > 3)
                textBox1.Select(log.Length - 3, 1);
            textBox1.ScrollToCaret();
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static void Flush()
        {
            try
            {
                using (StreamWriter streamWriter = new(LogPath, true))
                    foreach (string line in lines)
                    {
                        streamWriter.Write(line);
                        Console.Write(line);
                    }
                lines.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
