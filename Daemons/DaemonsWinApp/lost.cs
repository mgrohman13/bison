using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DaemonsWinApp
{
    public partial class lost : Form
    {
        public lost(Daemons.Game game, bool log)
        {
            InitializeComponent();

            if (log)
            {
                this.Text = "Combat Log";
                this.textBox1.Text = game.log;
            }
            else
            {
                this.Text = "Player Ranks";
                foreach (Daemons.Player player in game.GetLost())
                    this.textBox1.Text += player.Name + "\r\n";
            }

            this.textBox1.Select(0, 0);
        }
    }
}