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
    public partial class ProductionForm : Form
    {
        private static ProductionForm form = new ProductionForm();

        private MainForm gameForm;

        private Colony colony;

        private bool accountForIncome;
        private bool switchLoss;
        private double[] additionalLosses;

        private ProductionForm()
        {
            InitializeComponent();
        }

        private void SetColony(Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            this.colony = colony;

            this.accountForIncome = accountForIncome;
            this.switchLoss = switchLoss;
            this.additionalLosses = additionalLosses;

            this.lbxDesigns.Items.Clear();
            this.lbxDesigns.Items.Add("Gold");
            this.lbxDesigns.Items.Add(colony.Player.Game.StoreProd);
            this.lbxDesigns.Items.Add(colony.Player.Game.Soldiering);
            this.lbxDesigns.Items.Add(colony.Player.PlanetDefense);
            this.lbxDesigns.Items.Add(string.Empty);
            foreach (ShipDesign shipDesign in colony.Player.GetShipDesigns())
                this.lbxDesigns.Items.Add(shipDesign);

            if (colony.Buildable == null)
                this.lbxDesigns.SelectedIndex = 0;
            else if (this.lbxDesigns.Items.Contains(colony.Buildable))
                this.lbxDesigns.SelectedItem = colony.Buildable;
            else
                this.lbxDesigns.SelectedItem = colony.Player.Game.StoreProd;

            RefreshBuild();
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

        private void RefreshBuild()
        {
            Buildable newBuild = GetSelectedDesign();

            //this.sdForm.Visible = newBuild is ShipDesign;
            this.chkObsolete.Visible = newBuild is ShipDesign;

            this.sdForm.SetBuildable(newBuild);
            this.chkObsolete.Checked = true;

            double lossAmt = GetLossPct(newBuild) * colony.Production;
            if (lossAmt > 0)
                this.lblProdLoss.Text = "-" + MainForm.FormatDouble(lossAmt) + " production";
            else
                this.lblProdLoss.Text = string.Empty;
            this.lblProd.Text = MainForm.GetProdText(colony, newBuild, colony.Production - lossAmt);

            //this makes sure you wont screw yourself over and pay too much for production
            this.btnBuy.Enabled = !( !switchLoss && additionalLosses.Length > 0 && accountForIncome && newBuild != colony.Buildable );

            //this stops you from marking another ship as obsolete during the event for marking a first one 
            this.chkObsolete.Enabled = colony.CanBuild(colony.Buildable);

            ////this prevents cancel from switching to gold and wasting production on new colony or manual obsolete
            //this.btnCancel.Enabled = !( !switchLoss && colony.Production > 0 && !colony.CanBuild(colony.Buildable) );
        }

        private double GetLossPct(Buildable buildable)
        {
            double lossAmt = 0;
            for (int i = this.switchLoss ? -1 : 0 ; i < this.additionalLosses.Length ; ++i)
            {
                double loss;
                if (i > -1)
                    loss = this.additionalLosses[i];
                else
                    loss = this.colony.GetLossPct(buildable);
                lossAmt = 1 - ( ( 1 - loss ) * ( 1 - lossAmt ) );
            }
            return lossAmt;
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            Buildable buildable = GetSelectedDesign();
            if (buildable != null)
            {
                bool switchFirst = ( switchLoss && buildable != colony.Buildable );
                int initial = GetInitialBuy(buildable, switchFirst);
                int prod = SliderForm.ShowDialog(gameForm, new BuyProd(colony.Player, initial));
                if (prod > 0)
                {
                    if (switchFirst)
                    {
                        colony.StartBuilding(buildable);
                        if (prod == initial)
                            prod = GetInitialBuy(buildable, false);
                    }
                    colony.BuyProduction(prod);
                    RefreshBuild();
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
                buy = Game.Random.Round(( this.colony.Player.Gold / Consts.GoldForProduction ) / 2.0);
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
            return (int)Math.Floor(prod * ( 1 - loss ));
        }

        private int GetIncome()
        {
            int income = 0;
            if (accountForIncome)
            {
                income = colony.GetProductionIncome();
                Ship repairShip = colony.RepairShip;
                if (repairShip != null)
                {
                    income -= (int)Math.Ceiling(repairShip.GetProdForHP(repairShip.MaxHP - repairShip.HP));
                    if (income < 0)
                        income = 0;
                }
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

            for (int i = this.additionalLosses.Length ; --i > -1 ; )
                cost = MultForLossPct(cost, this.additionalLosses[i]);

            return cost;
        }

        private static int MultForLossPct(int cost, double pct)
        {
            return (int)Math.Ceiling(cost / ( 1 - pct ));
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            int prod = SliderForm.ShowDialog(gameForm, new SellProd(colony));
            if (prod > 0)
            {
                colony.SellProduction(prod);
                RefreshBuild();
            }
        }

        private void chkObsolete_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.chkObsolete.Checked)
            {
                if (MainForm.ShowOption("Are you sure you want to mark this design as obsolete?"))
                    this.DialogResult = DialogResult.Abort;
                else
                    this.chkObsolete.Checked = true;
            }
        }

        public static Buildable ShowDialog(MainForm gameForm, Colony colony, bool accountForIncome, bool switchLoss, params double[] additionalLosses)
        {
            form.gameForm = gameForm;
            gameForm.SetLocation(form);

            form.SetColony(colony, accountForIncome, switchLoss, additionalLosses);
            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
                return form.GetSelectedDesign();
            else if (result == DialogResult.Abort)
                gameForm.Game.CurrentPlayer.MarkObsolete((ShipDesign)form.GetSelectedDesign(), gameForm, accountForIncome, additionalLosses);

            if (colony.CanBuild(colony.Buildable))
                return colony.Buildable;
            return gameForm.Game.StoreProd;
        }

        private void lbxDesigns_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
