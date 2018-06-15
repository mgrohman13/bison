using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class ResearchFocusForm : Form, IComparer<ShipDesign>
    {
        private static ResearchFocusForm form = new ResearchFocusForm();

        private ResearchFocusForm()
        {
            InitializeComponent();
        }

        private void SetResearchFocus()
        {
            this.lbxUpgrade.Items.Clear();
            SortedSet<ShipDesign> designs = new SortedSet<ShipDesign>(MainForm.Game.CurrentPlayer.GetShipDesigns(), this);
            foreach (ShipDesign shipDesign in designs)
                this.lbxUpgrade.Items.Add(shipDesign);
            this.lbxUpgrade.SelectedIndex = this.lbxUpgrade.Items.Count - 1;

            if (MainForm.Game.CurrentPlayer.ResearchFocusDesign == null)
            {
                this.cbUpgrade.Checked = false;

                this.cbTransport.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.Trans);
                this.cbDS.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.DS);
                this.cbColony.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.Colony);

                this.cbDefense.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.Def);
                this.cbAttack.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.Att);

                this.cbSpeed.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.Speed);

                this.cbCost.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.Cost);
                this.cbUpkeep.Checked = MainForm.Game.CurrentPlayer.IsFocusing(ShipDesign.FocusStat.Upkeep);
            }
            else
            {
                this.cbUpgrade.Checked = true;
                this.lbxUpgrade.SelectedItem = MainForm.Game.CurrentPlayer.ResearchFocusDesign;
            }

            RefreshChance();
        }

        private void RefreshChance()
        {
            int research;
            double population, production, gold;
            MainForm.Game.CurrentPlayer.GetTurnIncome(out population, out research, out production, out gold);

            this.lblChance.Text = string.Format("{0} ( {1} )", MainForm.FormatPctWithCheck(MainForm.Game.CurrentPlayer.GetResearchChance(research, GetResearchFocusDesign())),
                    MainForm.FormatPctWithCheck(MainForm.Game.CurrentPlayer.GetMaxResearchChance(research, GetResearchFocusDesign())));
        }

        private ShipDesign GetResearchFocusDesign()
        {
            if (this.cbUpgrade.Checked)
                return (ShipDesign)this.lbxUpgrade.SelectedItem;
            return null;
        }

        private ShipDesign.FocusStat GetResearchFocus()
        {
            ShipDesign.FocusStat focus = ShipDesign.FocusStat.None;
            if (!this.cbUpgrade.Checked)
            {
                if (this.cbAttack.Checked)
                    focus |= ShipDesign.FocusStat.Att;
                if (this.cbColony.Checked)
                    focus |= ShipDesign.FocusStat.Colony;
                if (this.cbCost.Checked)
                    focus |= ShipDesign.FocusStat.Cost;
                if (this.cbDefense.Checked)
                    focus |= ShipDesign.FocusStat.Def;
                if (this.cbDS.Checked)
                    focus |= ShipDesign.FocusStat.DS;
                if (this.cbSpeed.Checked)
                    focus |= ShipDesign.FocusStat.Speed;
                if (this.cbTransport.Checked)
                    focus |= ShipDesign.FocusStat.Trans;
                if (this.cbUpkeep.Checked)
                    focus |= ShipDesign.FocusStat.Upkeep;
            }
            return focus;
        }

        int IComparer<ShipDesign>.Compare(ShipDesign x, ShipDesign y)
        {
            return y.Research - x.Research;
        }

        private void lbxUpgrade_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void lbxUpgrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.sdUpgrade.SetBuildable((Buildable)this.lbxUpgrade.SelectedItem);

            RefreshChance();
        }

        private void cbTransport_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTransport.Checked)
            {
                cbColony.Checked = false;
                cbDS.Checked = false;
                cbUpgrade.Checked = false;
            }
        }
        private void cbColony_CheckedChanged(object sender, EventArgs e)
        {
            if (cbColony.Checked)
            {
                cbTransport.Checked = false;
                cbDS.Checked = false;
                cbUpgrade.Checked = false;
            }
        }
        private void cbDS_CheckedChanged(object sender, EventArgs e)
        {
            if (cbDS.Checked)
            {
                cbTransport.Checked = false;
                cbColony.Checked = false;
                cbUpgrade.Checked = false;
            }
        }

        private void cbAttack_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAttack.Checked)
            {
                cbDefense.Checked = false;
                cbUpgrade.Checked = false;
            }
        }
        private void cbDefense_CheckedChanged(object sender, EventArgs e)
        {
            if (cbDefense.Checked)
            {
                cbAttack.Checked = false;
                cbUpgrade.Checked = false;
            }
        }

        private void cbSpeed_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSpeed.Checked)
                cbUpgrade.Checked = false;
        }

        private void cbCost_CheckedChanged(object sender, EventArgs e)
        {
            if (cbCost.Checked)
                cbUpgrade.Checked = false;
            else
                cbUpkeep.Checked = false;
        }
        private void cbUpkeep_CheckedChanged(object sender, EventArgs e)
        {
            if (cbUpkeep.Checked)
            {
                cbCost.Checked = true;
                cbUpgrade.Checked = false;
            }
        }

        private void cbUpgrade_CheckedChanged(object sender, EventArgs e)
        {
            if (cbUpgrade.Checked)
            {
                cbTransport.Checked = false;
                cbColony.Checked = false;
                cbDS.Checked = false;
                cbAttack.Checked = false;
                cbDefense.Checked = false;
                cbSpeed.Checked = false;
                cbUpkeep.Checked = false;
                cbCost.Checked = false;
            }

            lbxUpgrade.Enabled = cbUpgrade.Checked;
            sdUpgrade.Enabled = cbUpgrade.Checked;

            RefreshChance();
        }

        public static void ShowForm()
        {
            MainForm.GameForm.SetLocation(form);

            form.SetResearchFocus();
            if (form.ShowDialog() == DialogResult.OK)
            {
                MainForm.Game.CurrentPlayer.ResearchFocusDesign = null;
                MainForm.Game.CurrentPlayer.ResearchFocus = form.GetResearchFocus();
                MainForm.Game.CurrentPlayer.ResearchFocusDesign = form.GetResearchFocusDesign();
            }
        }
    }
}
