using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class SellPlanetDefense : SliderController
    {
        private static CheckBox sellForProd;
        private static EventHandler eventHandler;

        static SellPlanetDefense()
        {
            sellForProd = new CheckBox();
            sellForProd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            sellForProd.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            sellForProd.Text = "Production";
            sellForProd.Checked = true;
        }

        public static bool Gold
        {
            get
            {
                return !sellForProd.Checked;
            }
        }

        private readonly Colony colony;

        public SellPlanetDefense(Colony colony)
        {
            this.colony = colony;

            if (eventHandler != null)
                sellForProd.CheckedChanged -= eventHandler;
            eventHandler = new EventHandler(this.sellForProd_CheckedChanged);
            sellForProd.CheckedChanged += eventHandler;
        }

        private void sellForProd_CheckedChanged(object sender, EventArgs e)
        {
            base.Refresh();
        }

        public override Control GetCustomControl()
        {
            return sellForProd;
        }

        public override double GetInitial()
        {
            return colony.HP / 2.0;
        }

        protected override int GetMaxInternal()
        {
            return colony.HP;
        }

        protected override double GetResult()
        {
            return colony.GetPlanetDefenseDisbandValue(GetValue());
        }

        protected override string GetResultType()
        {
            return Gold ? "Gold" : "Production";
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            lblTitle.Text = "Disband Defenses";
            lblSlideType.Text = "HP";
        }
    }
}
