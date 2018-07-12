using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class SellProd : SliderController
    {
        private readonly Colony colony;

        public SellProd(Colony colony)
        {
            this.colony = colony;
        }

        public override double GetInitial()
        {
            return colony.Production2 / 2.0;
        }

        protected override int GetMaxInternal()
        {
            return colony.Production2;
        }

        protected override double GetResult()
        {
            return GetValue() / Consts.ProductionForGold;
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            lblTitle.Text = "Sell Production";
            lblSlideType.Text = "Production";
        }
    }
}
