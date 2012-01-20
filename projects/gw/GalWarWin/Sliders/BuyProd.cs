using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class BuyProd : SliderController
    {
        private readonly Player player;
        private readonly int initial;

        public BuyProd(Player player, int initial)
        {
            this.player = player;
            this.initial = initial;

            if (initial > GetMax())
                MessageBox.Show("You need " + initial * Consts.GoldForProduction + " gold to complete this ship.");
        }

        public override double GetInitial()
        {
            return initial;
        }

        protected override int GetMaxInternal()
        {
            return (int)( player.Gold / Consts.GoldForProduction );
        }

        protected override double GetResult()
        {
            return GetValue() * Consts.GoldForProduction;
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            lblTitle.Text = "Buy Production";
            lblSlideType.Text = "Production";
        }
    }
}
