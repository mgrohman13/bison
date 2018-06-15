using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class BuyProd : SliderController
    {
        private static BuildableControl control = new BuildableControl();

        private readonly Colony colony;
        private readonly Buildable buildable;
        private readonly int prodLoss, initial;

        public BuyProd(Colony colony, Buildable buildable, int prodLoss, int initial)
        {
            this.colony = colony;
            this.buildable = buildable;
            this.prodLoss = prodLoss;
            this.initial = initial;

            if (initial > GetMax())
                MessageBox.Show("You need " + initial * Consts.GoldForProduction + " gold to complete this ship.");
        }

        public override Control GetCustomControl()
        {
            if (buildable is PlanetDefense)
            {
                control.SetColony(colony, buildable, prodLoss);
                return control;
            }
            return null;
        }

        public override double GetInitial()
        {
            return initial;
        }

        protected override int GetMaxInternal()
        {
            return (int)( colony.Player.Gold / Consts.GoldForProduction );
        }

        protected override double GetResult()
        {
            control.RefreshBuildable(GetValue());
            return GetValue() * Consts.GoldForProduction;
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            lblTitle.Text = "Buy Production";
            lblSlideType.Text = "Production";
        }
    }
}
