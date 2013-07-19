﻿using System;
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
        private static string LogPath = "cw.log";

        private static string log;
        private static List<string> lines;

        static Log()
        {
            log = "";
            lines = new List<string>();

            LogPath = MainMenu.SavePath + LogPath;

            try
            {
                if (File.Exists(LogPath))
                    using (StreamReader streamReader = new StreamReader(LogPath))
                        log = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void LogAttack(Unit attacker, Attack attack, Unit defender, int damage, int oldHits)
        {
            string line = string.Format("{6} {0}, {1} -> {7} {2} ({3}, {5}) : {4}{8}\r\n", attacker, attack.GetLogString(), defender,
                    oldHits, -damage, defender.Armor, attacker.Owner, defender.Owner, defender.Dead ? ", Killed!" : "");
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
            textBox1.Select(log.Length - 3, 1);
            textBox1.ScrollToCaret();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static void Flush()
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(LogPath, true))
                    foreach (string line in lines.Reverse<string>())
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
