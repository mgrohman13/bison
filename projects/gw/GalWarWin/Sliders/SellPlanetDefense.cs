using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class SellPlanetDefense : SliderController
    {
        private static CheckBox chkProd;
        private static EventHandler eventHandler;

        static SellPlanetDefense()
        {
            chkProd = new CheckBox();
            chkProd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            chkProd.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            chkProd.Text = "Production";
            chkProd.Checked = true;
        }

        public static bool Gold
        {
            get
            {
                return !chkProd.Checked;
            }
        }

        private readonly Colony colony;

        public SellPlanetDefense(Colony colony)
        {
            this.colony = colony;

            if (eventHandler != null)
                chkProd.CheckedChanged -= eventHandler;
            eventHandler = new EventHandler(this.sellForProd_CheckedChanged);
            chkProd.CheckedChanged += eventHandler;

            if (colony.Buildable == null)
            {
                chkProd.Enabled = false;
                chkProd.Checked = false;
            }
        }

        private void sellForProd_CheckedChanged(object sender, EventArgs e)
        {
            base.Refresh();
        }

        public override Control GetCustomControl()
        {
            return chkProd;
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
            return colony.GetPlanetDefenseDisbandValue(GetValue(), Gold);
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
