using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class SellPlanetDefense : SliderController
    {
        private readonly Colony colony;
        private SellPlanetDefenseControl control;

        public SellPlanetDefense(Colony colony)
        {
            this.colony = colony;

            this.control = new SellPlanetDefenseControl(this.sellForProd_CheckedChanged);
            this.control.SetProdEnabled(colony.CurBuild != null);
        }

        public bool Gold
        {
            get
            {
                return control.Gold;
            }
        }

        private void sellForProd_CheckedChanged(object sender, EventArgs e)
        {
            base.Refresh();
        }

        public override Control GetCustomControl()
        {
            return control;
        }

        public override double GetInitial()
        {
            return colony.HP / 2.0;
        }

        public override int GetMin()
        {
            return 0;
        }

        protected override int GetMaxInternal()
        {
            return colony.HP;
        }

        protected override double GetResult()
        {
            int newAtt, newDef;
            double result = colony.GetPlanetDefenseDisbandValue(GetValue(), Gold, out newAtt, out newDef);
            control.SetAttDefDiff(newAtt - colony.Att, newDef - colony.Def);
            return result;
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
