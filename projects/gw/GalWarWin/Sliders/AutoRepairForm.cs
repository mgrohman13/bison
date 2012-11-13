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
        private bool events = true;

        private AutoRepairForm()
        {
            InitializeComponent();
        }

        private void SetShip(Ship ship)
        {
            this.ship = ship;

            double autoRepair = ship.AutoRepair;
            this.txtDefault.Text = string.Empty;
            if (double.IsNaN(autoRepair))
            {
                this.rbNone.Checked = true;
                SetValue(this.txtDefault, 1);
            }
            else
            {
                this.rbDefault.Checked = true;
                SetValue(this.txtDefault, autoRepair);
            }

            RefreshEnabled();
        }

        private void RefreshEnabled()
        {
            this.txtDefault.Enabled = this.rbDefault.Checked;
            this.txtCost.Enabled = this.rbCost.Checked;
            this.txtConst.Enabled = this.rbConst.Checked;
            this.txtPct.Enabled = this.rbPct.Checked;
            this.txtTurn.Enabled = this.rbTurn.Checked;
        }

        private void RefreshValues(object sender, double value)
        {
            if (events && !double.IsNaN(value))
            {
                if (value <= 1)
                {
                    value = 1;
                    sender = null;
                }

                events = false;
                if (sender != this.txtDefault)
                    setDefault(value);
                if (sender != this.txtCost)
                    setCost(value);
                if (sender != this.txtConst)
                    setConst(value);
                if (sender != this.txtPct)
                    setPct(value);
                if (sender != this.txtTurn)
                    setTurn(value);
                events = true;
            }
        }

        private static double ParseValue(TextBox textBox)
        {
            double value;
            if (double.TryParse(textBox.Text, out value))
                return value;
            else if (textBox.Text != ".")
                textBox.Text = string.Empty;
            return double.NaN;
        }

        private static void SetValue(TextBox textBox, double value)
        {
            Console.WriteLine(textBox.Name);
            textBox.Text = value.ToString("0.000");
        }

        public static double ShowDialog(Ship ship)
        {
            form.SetShip(ship);
            if (form.ShowDialog() == DialogResult.OK)
                if (form.rbNone.Checked)
                    return double.NaN;
                else
                    return ParseValue(form.txtConst);
            return -1;
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            RefreshEnabled();
        }

        private void txtDefault_TextChanged(object sender, EventArgs e)
        {
            double value = ParseValue(this.txtDefault);
            if (value < 0)
                value = 0;
            else if (value > 1000)
                value = 1000;
            value = ship.GetDefaultGoldRepair(value);
            RefreshValues(sender, value);
        }
        private void setDefault(double value)
        {
            value = ship.GetGoldRepairMultiplyer(value);
            SetValue(this.txtDefault, value);
        }

        private void txtConst_TextChanged(object sender, EventArgs e)
        {
            double value = ParseValue(this.txtConst);
            if (value < 0)
                value = 0;
            else if (value > ship.MaxHP)
                value = ship.MaxHP;
            RefreshValues(sender, value);
        }
        private void setConst(double value)
        {
            SetValue(this.txtConst, value);
        }

        private void txtPct_TextChanged(object sender, EventArgs e)
        {
            double value = ParseValue(this.txtPct);
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;
            value *= ship.MaxHP;
            RefreshValues(sender, value);
        }
        private void setPct(double value)
        {
            value /= ship.MaxHP;
            SetValue(this.txtPct, value);
        }

        private void txtCost_TextChanged(object sender, EventArgs e)
        {
            double value = ParseValue(this.txtCost);
            if (value < 0)
                value = 0;
            else if (value > 10000)
                value = 10000;

            double target = value;
            int upper = MattUtil.TBSUtil.FindValue(delegate(int hp)
            {
                return ( ship.GetGoldForHP(hp) >= target );
            }, 0, ship.MaxHP - ship.HP, true);
            value = upper;
            if (upper > 0)
            {
                double high = ship.GetGoldForHP(upper);
                int lower = upper - 1;
                double low = ship.GetGoldForHP(lower);
                if (low <= target && target <= high)
                    value = ( lower + ( ( target - low ) / ( high - low ) ) );
            }

            RefreshValues(sender, value);
        }
        private void setCost(double value)
        {
            value = ship.CalcGoldForHP(value);
            SetValue(this.txtCost, value);
        }

        private void txtTurn_TextChanged(object sender, EventArgs e)
        {
            double value = ParseValue(this.txtTurn);
            if (value < 0)
                value = 0;
            else if (value > 1000)
                value = 1000;
            value = ( ship.MaxHP - ship.HP ) / value;
            RefreshValues(sender, value);
        }
        private void setTurn(double value)
        {
            value = ( ship.MaxHP - ship.HP ) / value;
            SetValue(this.txtTurn, value);
        }
    }
}