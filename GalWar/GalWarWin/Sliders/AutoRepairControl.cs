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

        public AutoRepairControl()
        {
            InitializeComponent();
        }

        public void SetShip(Ship ship)
        {
            this.ship = ship;
            RefreshChecked();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (AutoRepairForm.ShowForm(ship) && this.Parent is SliderForm)
                ((Form)this.Parent).DialogResult = DialogResult.Cancel;
            RefreshChecked();
        }

        private void RefreshChecked()
        {
            this.cbAuto.Checked = (ship.AutoRepair > 0);
            this.cbxProdRepair.Checked = (ship.ProdRepair);
        }

        private void cbxProdRepair_CheckedChanged(object sender, EventArgs e)
        {
            ship.ProdRepair = this.cbxProdRepair.Checked;
        }
    }
}
