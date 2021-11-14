using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;
using GalWarWin.Sliders;

namespace GalWarWin
{
    public partial class ProductionForm : Form, IComparer<Buildable>
    {
        private static ProductionForm form = new ProductionForm();

        private Colony colony;
        private bool callback, floor;
        private double addProd;
        private SortedSet<Buildable> designs;

        private ProductionForm()
        {
            InitializeComponent();
        }

        private void SetColony(Colony colony, bool callback, double production, bool floor)
        {
            this.colony = colony;
            this.callback = callback;
            this.addProd = production;
            this.floor = floor;

            this.btnBuy.Enabled = !callback;
            this.btnCancel.Enabled = !callback;

            this.rbValue.Checked = true;
            RefreshDesigns();
            RefreshBuild();
        }

        private void RefreshDesigns()
        {
            this.lbxDesigns.Items.Clear();

            designs = new SortedSet<Buildable>(colony.Buildable, this);
            this.lbxDesigns.Items.AddRange(designs.ToArray());
            this.lbxDesigns.Items.Insert(3, string.Empty);

            this.lbxDesigns.SelectedItem = colony.CurBuild;
        }

        private void lbxDesigns_DrawItem(object sender, DrawItemEventArgs e)
        {
            object item = ((ListBox)(sender)).Items[e.Index];
            Buildable build = item as Buildable;
            bool bold = false;

            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                Font font = e.Font;
                if (bold)
                    font = new Font(e.Font, FontStyle.Bold);
                e.DrawBackground();
                e.Graphics.DrawString(item.ToString(), font, brush, e.Bounds);

                if (build != null && build.Production > 0)
                {
                    string draw = string.Format("{1}{2}", build, build.Production, string.Empty);
                    float x = e.Bounds.Right - e.Graphics.MeasureString(draw, font).Width;
                    e.Graphics.DrawString(draw, font, brush, x, e.Bounds.Y);
                }

                e.DrawFocusRectangle();
                if (bold)
                    font.Dispose();
            }
        }
        //private void lbxDesigns_MeasureItem(object sender, MeasureItemEventArgs e)
        //{
        //}

        int IComparer<Buildable>.Compare(Buildable b1, Buildable b2)
        {
            if (b1 == b2)
                return 0;

            Func<Buildable, int> TypeComp = b =>
            {
                if (b is BuildGold)
                    return 0;
                if (b is StoreProd)
                    return 1;
                if (b is BuildInfrastructure)
                    return 2;
                if (b is BuildShip)
                    return 3;
                throw new Exception();
            };
            int retVal = TypeComp(b1) - TypeComp(b2);
            if (retVal != 0)
                return retVal;

            ShipDesign x = ((BuildShip)b1).ShipDesign;
            ShipDesign y = ((BuildShip)b2).ShipDesign;

            double xVal = 0, yVal = 0;

            if (this.rbCustom.Checked)
            {
                xVal = ShipDesignSortForm.GetValue(x);
                yVal = ShipDesignSortForm.GetValue(y);
            }
            else if (this.rbStr.Checked)
            {
                xVal = GetStrength(x);
                yVal = GetStrength(y);
            }
            else if (this.rbTrans.Checked)
            {
                xVal = GetTrans(x);
                yVal = GetTrans(y);
            }

            double value = yVal - xVal;
            if (Math.Abs(value) > Consts.FLOAT_ERROR_ZERO)
                return Math.Sign(value);
            else
                return y.Research - x.Research;
        }
        private double GetStrength(ShipDesign x)
        {
            return (x.GetStrength() / GetTotalCost(x));
        }
        private double GetTrans(ShipDesign x)
        {
            return (x.Trans * x.Speed / GetTotalCost(x));
        }
        private double GetTotalCost(ShipDesign x)
        {
            return (x.Cost + x.Upkeep * x.GetUpkeepPayoff(colony.Player.Game));
        }

        private Buildable GetSelectedDesign()
        {
            Buildable selected = this.lbxDesigns.SelectedItem as Buildable;
            if (selected == null)
                selected = colony.CurBuild;
            return selected;
        }

        private void lbxDesigns_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshBuild();
        }

        private void RefreshBuild(bool? setPause = true)
        {
            Buildable newBuild = GetSelectedDesign();

            int prod = GetAddProd(newBuild);

            this.sdForm.SetColony(colony, newBuild, null, newBuild is BuildInfrastructure);
            this.chkObsolete.Checked = false;

            if (prod > 0)
                this.lblProdLoss.Text = "+" + prod + " production";
            else
                this.lblProdLoss.Text = string.Empty;
            this.lblProd.Text = MainForm.GetProdIncText(colony, newBuild, newBuild.Production - prod, this.chkPause.Checked);

            //this stops you from marking your last deisgn as obsolete
            this.chkObsolete.Enabled = (!this.callback && newBuild is BuildShip && colony.Player.GetShipDesigns().Count > 1);

            if (setPause.HasValue)
            {
                bool canPause = (newBuild is BuildShip);
                this.chkPause.Enabled = canPause;
                if (canPause)
                    if (setPause.Value)
                        canPause = (colony.PauseBuild && colony.CurBuild == newBuild);
                    else
                        canPause = false;
                this.chkPause.Checked = canPause;
            }

            this.lbxDesigns.Invalidate();
        }

        private int GetAddProd(Buildable newBuild)
        {
            double value = newBuild.GetAddProduction(this.addProd, floor);
            return (int)Math.Round(value * Consts.FLOAT_ERROR_ONE);
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            if (TradeProdForm.ShowForm(this.colony, designs, form.GetSelectedDesign()))
            {
                RefreshBuild();
                MainForm.GameForm.RefreshAll();
            }
        }

        private void chkObsolete_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkObsolete.Checked)
            {
                if (MainForm.ShowOption("Are you sure you want to mark this design as obsolete?"))
                    this.DialogResult = DialogResult.Abort;
                else
                    this.chkObsolete.Checked = false;
            }
        }

        public static Buildable ShowForm(Colony colony, bool callback, double production, bool floor, out bool pause, out ShipDesign obsolete)
        {
            MainForm.GameForm.SetLocation(form);

            form.SetColony(colony, callback, production, floor);
            DialogResult result = form.ShowDialog();

            Buildable buildable;
            obsolete = null;
            if (result == DialogResult.OK)
            {
                buildable = form.GetSelectedDesign();
                pause = form.chkPause.Checked;
            }
            else
            {
                buildable = colony.CurBuild;
                pause = colony.PauseBuild;
                if (!callback && result == DialogResult.Abort)
                    obsolete = ((BuildShip)form.GetSelectedDesign()).ShipDesign;
            }

            return buildable;
        }

        private void lbxDesigns_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.rbCustom.Checked)
                this.RefreshDesigns();
        }

        private void rbCustom_Click(object sender, EventArgs e)
        {
            if (!ShipDesignSortForm.ShowForm())
                this.rbValue.Checked = true;
            this.RefreshDesigns();
        }
    }
}
