using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Daemons;

namespace DaemonsWinApp
{
    public partial class TextForm : Form
    {
        private static TextForm form = new TextForm();

        private TextForm()
        {
            InitializeComponent();
        }

        private void SetupStuff(Game game, bool log)
        {
            if (log)
            {
                this.Text = "Combat Log";
                this.textBox1.Text = game.log;

                this.textBox1.Select(0, 0);
            }
            else
            {
                this.Text = "Player Ranks";
                this.textBox1.Text = string.Empty;
                foreach (Daemons.Player player in game.GetLost())
                    this.textBox1.Text += player.Name + "\r\n";

                this.textBox1.SelectAll();
            }

        }

        public static DialogResult ShowDialog(Game game, bool log)
        {
            form.SetupStuff(game, log);
            return form.ShowDialog();
        }
    }
}