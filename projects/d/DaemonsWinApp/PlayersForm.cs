using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Daemons;

namespace DaemonsWinApp
{
    public partial class PlayersForm : Form
    {
        public PlayersForm(Game game)
        {
            InitializeComponent();

            if (game.GetPlayers().Length > 1)
                foreach (Player p in game.GetPlayers())
                {
                    this.lbxPlayer.Items.Add(p);
                    this.lbxSoul.Items.Add(p.Score.ToString("0"));
                    this.lbxStr.Items.Add(p.GetStrength().ToString("0"));
                    this.lbxProd.Items.Add(game.GetProduction(p).ToString("+0.0"));
                }
            foreach (Player p in game.GetWinners())
            {
                this.lbxPlayer.Items.Add(p);
                this.lbxSoul.Items.Add("WON");
                this.lbxStr.Items.Add(0);
                this.lbxProd.Items.Add(0);
            }
            foreach (Player p in game.GetLosers())
            {
                this.lbxPlayer.Items.Add(p);
                this.lbxSoul.Items.Add(p.Score.ToString("0"));
                this.lbxStr.Items.Add(0);
                this.lbxProd.Items.Add(0);
            }
        }

        private void lbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = ( (ListBox)sender ).SelectedIndex;
            this.lbxPlayer.SelectedIndex = idx;
            this.lbxSoul.SelectedIndex = idx;
            this.lbxStr.SelectedIndex = idx;
            this.lbxProd.SelectedIndex = idx;
        }
    }
}
