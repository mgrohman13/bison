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

            foreach (KeyValuePair<Player, int> pair in game.GetWinners())
            {
                Player p = pair.Key;
                this.lbxPlayer.Items.Add(p);
                this.lbxSoul.Items.Add("WON +" + ( Consts.WinPoints / pair.Value ).ToString("0.0"));
                this.lbxStr.Items.Add(0);
                this.lbxProd.Items.Add(0);
            }
            if (game.GetPlayers().Count > 1)
            {
                foreach (Player p in game.GetPlayers())
                {
                    this.lbxPlayer.Items.Add(p);
                    this.lbxSoul.Items.Add(p.Score.ToString("0"));
                    string str = p.GetStrength().ToString("0");
                    double win = game.GetWinPct(p);
                    if (win > 0)
                        str += " (" + ( win * 100 ).ToString("0") + "%)";
                    this.lbxStr.Items.Add(str);
                    this.lbxProd.Items.Add(game.GetProduction(p).ToString("+0.0"));
                }
                this.lbxPlayer.Items.Add(game.GetIndependent());
                this.lbxSoul.Items.Add(0);
                this.lbxStr.Items.Add(game.GetIndependent().GetStrength().ToString("0"));
                this.lbxProd.Items.Add(( game.IndyProd() / Consts.DaemonSouls * Consts.GetStrength(UnitType.Indy, Consts.IndyHits, Consts.IndyDamage) ).ToString("+0.0"));

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
