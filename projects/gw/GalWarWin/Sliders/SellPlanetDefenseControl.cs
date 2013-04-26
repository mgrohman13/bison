using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GalWarWin.Sliders
{
    public partial class SellPlanetDefenseControl : UserControl
    {
        public SellPlanetDefenseControl(EventHandler checkedChanged)
        {
            InitializeComponent();

            chkProd.CheckedChanged += checkedChanged;
        }

        public void SetProdEnabled(bool enabled)
        {
            chkProd.Checked = chkProd.Enabled = enabled;
        }

        public bool Gold
        {
            get
            {
                return !this.chkProd.Checked;
            }
        }

        internal void SetAttDefDiff(int attDiff, int defDiff)
        {
            int width = this.lblAtt.Width;
            this.lblAtt.Text = attDiff + " att";
            this.lblDef.Location = new Point(this.lblDef.Location.X + this.lblAtt.Width - width, this.lblDef.Location.Y);
            this.lblDef.Text = defDiff + " def";
        }
    }
}
