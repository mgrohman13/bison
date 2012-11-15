using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class RepairAllForm : Form
    {
        private static RepairAllForm form = new RepairAllForm();

        public RepairAllForm()
        {
            InitializeComponent();

            this.rbMultiply.Checked = true;
            ShowHide();
        }

        private void Init()
        {
            MainForm.FormatIncome(this.lblGold, MainForm.Game.CurrentPlayer.Gold, true);
            int research;
            double population, production, gold;
            MainForm.Game.CurrentPlayer.GetTurnIncome(out population, out research, out production, out gold, false);
            MainForm.FormatIncome(this.lblIncome, gold, true);

            Recalculate();

            this.btnRepair.Focus();
        }

        private void Recalculate()
        {
            double cost = 0;
            double value = GetValue();
            foreach (Ship ship in MainForm.Game.CurrentPlayer.GetShips())
                if (!ship.HasRepaired && DoAutoRepair(ship))
                {
                    double autoRepair = GetAutoRepair(ship, value);
                    if (autoRepair > 0)
                        cost += ship.GetGoldForHP(ship.GetAutoRepairHP(autoRepair));
                }
            MainForm.FormatIncome(this.lblRepairs, -cost, true);
        }

        private double GetValue()
        {
            if (this.rbMultiply.Checked)
                return ParseValue(this.txtMultiply);
            return ParseValue(this.txtSet);
        }

        private static double ParseValue(TextBox textBox)
        {
            double value;
            if (double.TryParse(textBox.Text, out value))
                return value;

            if (textBox.Text != ".")
                textBox.Text = "1.0";
            return 1;
        }

        private bool DoAutoRepair(Ship ship)
        {
            return ( ship.AutoRepair > 0 || ( this.rbSet.Checked &&
                    ( ( this.cbManual.Checked && double.IsNaN(ship.AutoRepair) )
                    || ( this.cbOff.Checked && ship.AutoRepair == 0 ) ) ) );
        }

        private double GetAutoRepair(Ship ship, double value)
        {
            if (this.rbMultiply.Checked)
                return ship.AutoRepair * value;
            return value;
        }

        private void ShowHide()
        {
            bool isMult = this.rbMultiply.Checked;

            this.txtMultiply.Enabled = isMult;
            this.txtSet.Enabled = !isMult;
            this.cbManual.Enabled = !isMult;
            this.cbOff.Enabled = !isMult;
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            ShowHide();
            Recalculate();
        }

        private void txt_TextChanged(object sender, EventArgs e)
        {
            Recalculate();
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rbSet.Checked)
                Recalculate();
        }

        public static bool ShowDialog(MainForm gameForm)
        {
            gameForm.SetLocation(form);

            form.Init();

            DialogResult dialogResult = form.ShowDialog();
            if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
            {
                double value = form.GetValue();
                foreach (Ship ship in MainForm.Game.CurrentPlayer.GetShips())
                    if (form.DoAutoRepair(ship))
                        ship.AutoRepair = form.GetAutoRepair(ship, value);

                if (dialogResult == DialogResult.Yes)
                    MainForm.Game.CurrentPlayer.AutoRepairShips(gameForm);

                return true;
            }
            return false;
        }
    }
}
