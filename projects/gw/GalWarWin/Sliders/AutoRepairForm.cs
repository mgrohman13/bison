using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public partial class AutoRepairForm : Form
    {
        private static AutoRepairForm form = new AutoRepairForm();

        private Ship ship;

        private AutoRepairForm()
        {
            InitializeComponent();
        }

        private void SetShip(Ship ship)
        {
            this.ship = ship;

            RefreshEnabled();
        }

        private void RefreshEnabled()
        {
            foreach (Control control in this.groupBox1.Controls)
                if (control is TextBox)
                    control.Enabled = false;

            if (this.rbConst.Checked)
                this.txtConst.Enabled = true;
            else if (this.rbPct.Checked)
                this.txtPct.Enabled = true;
            else if (this.rbTurn.Checked)
                this.txtTurn.Enabled = true;
        }

        private void RefreshValues(object sender)
        {
            if (sender == this.txtConst)
            {
                int cnst;
                if (int.TryParse(this.txtConst.Text, out cnst))
                    SetPct(cnst / (double)ship.MaxHP);
                else
                    this.txtConst.Text = string.Empty;
            }
            else if (sender == this.txtTurn)
            {
                int turns;
                if (int.TryParse(this.txtTurn.Text, out turns))
                    SetPct(( ship.MaxHP - ship.HP ) / (double)( turns * ship.MaxHP ));
                else
                    this.txtTurn.Text = string.Empty;
            }

            double pct;
            if (double.TryParse(this.txtPct.Text, out pct))
            {
                this.txtConst.Text = Game.Random.Round(pct * ship.MaxHP).ToString();
                this.txtTurn.Text = Game.Random.Round(( ship.MaxHP - ship.HP ) / ( pct * ship.MaxHP )).ToString();
            }
            else
            {
                this.txtPct.Text = string.Empty;
            }
        }

        private void SetPct(double pct)
        {
            this.txtPct.Text = pct.ToString("0.00");
        }

        public static void ShowDialog(Ship ship)
        {
            form.SetShip(ship);
            if (form.ShowDialog() == DialogResult.OK)
            {
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ( (Form)this.Parent ).DialogResult = DialogResult.Cancel;
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            RefreshEnabled();
        }

        private void txt_TextChanged(object sender, EventArgs e)
        {
            RefreshValues(sender);
        }
    }
}