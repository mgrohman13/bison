using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;
using GalWarWin.Sliders;

namespace GalWarWin
{
    public partial class ProductionForm : Form, IComparer<ShipDesign>
    {
        private static ProductionForm form = new ProductionForm();

        private Colony colony;

        private ProductionForm()
        {
            InitializeComponent();
        }

        private void SetColony(Colony colony)
        {
            this.colony = colony;

            this.rbValue.Checked = true;
            RefreshDesigns();
            RefreshBuild();
        }

        private void RefreshDesigns()
        {
            this.lbxDesigns.Items.Clear();

            this.lbxDesigns.Items.Add("Gold");
            this.lbxDesigns.Items.Add(colony.Player.Game.StoreProd);
            this.lbxDesigns.Items.Add(colony.Player.Game.PlanetDefense);
            //this.lbxDesigns.Items.Add(colony.Player.Game.Attack);
            //this.lbxDesigns.Items.Add(colony.Player.Game.Defense);
            this.lbxDesigns.Items.Add(string.Empty);

            SortedSet<ShipDesign> designs = new SortedSet<ShipDesign>(colony.Player.GetShipDesigns(), this);
            foreach (ShipDesign shipDesign in designs)
                this.lbxDesigns.Items.Add(shipDesign);

            if (colony.Buildable == null)
                this.lbxDesigns.SelectedIndex = 0;
            else if (this.lbxDesigns.Items.Contains(colony.Buildable))
                this.lbxDesigns.SelectedItem = colony.Buildable;
            else
                this.lbxDesigns.SelectedItem = colony.Player.Game.StoreProd;
        }

        int IComparer<ShipDesign>.Compare(ShipDesign x, ShipDesign y)
        {
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
            return ( x.GetStrength() / GetTotalCost(x) );
        }
        private double GetTrans(ShipDesign x)
        {
            return ( x.Trans * x.Speed / GetTotalCost(x) );
        }
        private double GetTotalCost(ShipDesign x)
        {
            return ( x.Cost + x.Upkeep * x.GetUpkeepPayoff(colony.Player.Game) );
        }

        private Buildable GetSelectedDesign()
        {
            if (this.lbxDesigns.SelectedIndex == 0)
                return null;
            if (string.Empty.Equals(this.lbxDesigns.SelectedItem))
                return colony.Buildable;
            return (Buildable)this.lbxDesigns.SelectedItem;
        }

        private void lbxDesigns_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshBuild();
        }

        private void RefreshBuild(bool? setPause = true)
        {
            Buildable newBuild = GetSelectedDesign();

            this.chkObsolete.Enabled = newBuild is ShipDesign;

            int lossAmt = GetLossAmt(newBuild);

            this.sdForm.SetColony(colony, newBuild, lossAmt);
            this.chkObsolete.Checked = false;

            if (lossAmt > 0)
                this.lblProdLoss.Text = "-" + lossAmt + " production";
            else
                this.lblProdLoss.Text = string.Empty;
            this.lblProd.Text = MainForm.GetProdText(colony, newBuild, colony.Production - lossAmt, this.chkPause.Checked);

            //this stops you from marking your last deisgn as obsolete
            this.chkObsolete.Enabled = ( colony.Player.GetShipDesigns().Count > 1 );

            if (setPause.HasValue)
            {
                bool canPause = ( newBuild is ShipDesign );
                this.chkPause.Enabled = canPause;
                if (canPause)
                    if (setPause.Value)
                        canPause = ( colony.PauseBuild && colony.Buildable == newBuild );
                    else
                        canPause = false;
                this.chkPause.Checked = canPause;
            }
        }

        private int GetLossAmt(Buildable newBuild)
        {
            int lossAmt = (int)Math.Ceiling(GetLossPct(newBuild) * colony.Production);
            return lossAmt;
        }

        private double GetLossPct(Buildable buildable)
        {
            return this.colony.GetLossPct(buildable);
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            Buildable buildable = GetSelectedDesign();
            if (buildable != null)
            {
                bool switchFirst = ( buildable != colony.Buildable );
                int initial = GetInitialBuy(buildable, switchFirst);
                int prod = SliderForm.ShowForm(new BuyProd(colony, buildable, GetLossAmt(buildable), initial));
                if (prod > 0)
                {
                    if (switchFirst)
                    {
                        colony.StartBuilding(MainForm.GameForm, buildable, false);
                        if (prod == initial)
                            prod = GetInitialBuy(buildable, false);
                    }
                    colony.BuyProduction(MainForm.GameForm, prod);
                    RefreshBuild(false);
                }
            }
        }

        private int GetInitialBuy(Buildable buildable, bool switchFirst)
        {
            int buy;
            if (buildable.Cost > 0 && buildable.Cost < int.MaxValue)
            {
                int amount = 0;
                int prod = GetTotalProd(buildable, switchFirst);
                do
                {
                    buy = GetCost(buildable, ++amount) - prod;
                } while (buy <= 0);
            }
            else
            {
                buy = Math.Max(Game.Random.Round(( this.colony.Player.Gold / Consts.GoldForProduction ) / 2.0), 0);
            }
            return buy;
        }

        private int GetTotalProd(Buildable buildable, bool switchFirst)
        {
            int prod = colony.Production;
            if (switchFirst)
                prod = LoseProduction(prod, this.colony.GetLossPct(buildable));
            return prod + GetIncome();
        }

        private static int LoseProduction(int prod, double loss)
        {
            return (int)( prod * ( 1 - loss ) );
        }

        private int GetIncome()
        {
            int income = colony.GetProductionIncome();
            Ship repairShip = colony.RepairShip;
            if (repairShip != null)
            {
                income -= (int)Math.Ceiling(repairShip.GetProdForHP(repairShip.MaxHP - repairShip.HP));
                if (income < 0)
                    income = 0;
            }
            return income;
        }

        private int GetCost(Buildable buildable, int amount)
        {
            int cost = 0;
            while (--amount > -1)
            {
                cost = MultForLossPct(cost, Consts.CarryProductionLossPct);
                cost += buildable.Cost;
            }
            return cost;
        }

        private static int MultForLossPct(int cost, double pct)
        {
            return (int)Math.Ceiling(cost / ( 1 - pct ));
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            int prod = SliderForm.ShowForm(new SellProd(colony));
            if (prod > 0)
            {
                colony.SellProduction(MainForm.GameForm, prod);
                RefreshBuild(null);
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

        public static bool ShowForm(Colony colony, out Buildable buildable, out bool pause)
        {
            MainForm.GameForm.SetLocation(form);

            form.SetColony(colony);
            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
            {
                buildable = form.GetSelectedDesign();
                pause = form.chkPause.Checked;
            }
            else
            {
                if (result == DialogResult.Abort)
                    MainForm.Game.CurrentPlayer.MarkObsolete(MainForm.GameForm, (ShipDesign)form.GetSelectedDesign());

                if (colony.CanBuild(colony.Buildable))
                    buildable = colony.Buildable;
                else
                    buildable = MainForm.Game.StoreProd;
                pause = colony.PauseBuild;
            }

            return ( result != DialogResult.Cancel );
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
