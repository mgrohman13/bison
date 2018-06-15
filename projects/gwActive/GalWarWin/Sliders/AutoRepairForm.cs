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
        private double result = -1;

        private AutoRepairForm()
        {
            InitializeComponent();
        }

        private void SetShip(Ship ship)
        {
            this.ship = ship;

            RadioButton checkRB;
            double setValue;
            double autoRepair = ship.AutoRepair;
            if (double.IsNaN(autoRepair))
            {
                checkRB = this.rbManual;
                setValue = 1;
            }
            else if (autoRepair == 0)
            {
                checkRB = this.rbNone;
                setValue = 1;
            }
            else
            {
                checkRB = this.rbDefault;
                setValue = autoRepair;
            }

            this.txtDefault.Text = string.Empty;
            SetValue(this.txtDefault, setValue);

            checkRB.Checked = true;
            this.txtDefault.Enabled = false;
        }
        private void AutoRepairForm_Shown(object sender, EventArgs e)
        {
            RefreshEnabled();
        }

        private void RefreshEnabled()
        {
            Enable(txtDefault, rbDefault);
            Enable(txtCost, rbCost);
            Enable(txtConst, rbConst);
            Enable(txtPct, rbPct);
            Enable(txtTurn, rbTurn);
        }
        private void Enable(TextBox textBox, RadioButton radioButton)
        {
            bool enabled = textBox.Enabled;

            textBox.Enabled = radioButton.Checked;

            if (textBox.Enabled && !enabled)
            {
                textBox.SelectAll();
                textBox.Focus();
            }
        }

        private void RefreshValues(object sender, double value)
        {
            if (events && !double.IsNaN(value))
            {
                if (value < 1)
                {
                    value = 1;
                    sender = null;
                }
                result = value;

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

        private void SetValue(TextBox textBox, double value)
        {
            string format;
            if (textBox == this.txtDefault)
                format = GetFloatErrorPrecisionFormat();
            else
                format = "0.000";
            textBox.Text = value.ToString(format);
        }

        public static string GetFloatErrorPrecisionFormat()
        {
            return "0.".PadRight((int)Math.Floor(-Math.Log10(Consts.FLOAT_ERROR_ZERO)) + 2, '0');
        }

        public static bool ShowForm(Ship ship)
        {
            double result = -1;
            form.SetShip(ship);
            if (form.ShowDialog() == DialogResult.OK)
                if (form.rbManual.Checked)
                    result = double.NaN;
                else if (form.rbNone.Checked)
                    result = 0;
                else
                    result = form.result;

            if (double.IsNaN(result) || result > -1)
            {
                if (result > 0)
                    result = ship.GetAutoRepairForHP(result);
                ship.AutoRepair = result;

                return true;
            }
            return false;
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
            if (!double.IsNaN(value))
                value = ship.GetAutoRepairHP(value);
            RefreshValues(sender, value);
        }
        private void setDefault(double value)
        {
            value = ship.GetAutoRepairForHP(value);
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
            if (!double.IsNaN(value))
                value = ship.GetHPForGold(value);
            RefreshValues(sender, value);
        }
        private void setCost(double value)
        {
            value = ship.GetGoldForHP(value);
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