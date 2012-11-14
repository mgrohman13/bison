using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public partial class AutoRepairControl : UserControl
    {
        private Ship ship;

        //private GoldRepair.SetValueDelegate SetValue;

        public AutoRepairControl()
        {
            InitializeComponent();
        }

        public void SetShip(Ship ship)
        {
            this.ship = ship;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double result = AutoRepairForm.ShowDialog(ship);
            if (double.IsNaN(result) || result > -1)
            {
                if (!double.IsNaN(result) && result > 0)
                    result = ship.GetAutoRepairForHP(result);
                ship.AutoRepair = result;
                ( (Form)this.Parent ).DialogResult = DialogResult.Cancel;
            }
        }

        //internal void SetSetValueDelegate(GoldRepair.SetValueDelegate SetValue)
        //{
        //    this.SetValue = SetValue;
        //}
    }
}
