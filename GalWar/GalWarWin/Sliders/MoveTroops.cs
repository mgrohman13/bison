using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class MoveTroops : SliderController
    {
        private static Label lblProd;

        static MoveTroops()
        {
            lblProd = new Label();
            lblProd.AutoEllipsis = true;
            lblProd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        }

        private readonly PopCarrier from;
        private readonly PopCarrier to;
        private readonly bool extraCost;

        private readonly Colony colony;

        private readonly int? max;

        private readonly int totalPop;
        private readonly double soldiers;

        public MoveTroops(PopCarrier from, PopCarrier to, bool extraCost)
        {
            this.from = from;
            this.to = to;
            this.extraCost = extraCost;

            if (from.Player == to.Player)
            {
                if (( colony = from as Colony ) == null)
                    this.colony = to as Colony;
            }
        }

        public MoveTroops(Colony from, int max, int totalPop, double soldiers, bool extraCost)
        {
            this.from = from;
            this.colony = from;
            this.extraCost = extraCost;

            this.max = max;

            this.totalPop = totalPop;
            this.soldiers = soldiers;
        }

        public override Control GetCustomControl()
        {
            if (( this.from is Colony && this.from.Player.IsTurn ) || ( this.to is Colony && this.to.Player.IsTurn ))
                return lblProd;
            return null;
        }

        public override double GetInitial()
        {
            if (max.HasValue && colony != null)
                return 0;
            return GetMax();
        }

        public override int GetMin()
        {
            return 0;
        }

        protected override int GetMaxInternal()
        {
            int max;
            if (this.max.HasValue)
                max = this.max.Value;
            else
                max = Math.Min(from.AvailablePop, to.FreeSpace);
            return max;
        }

        protected override double GetResult()
        {
            return Consts.GetMovePopCost(MainForm.Game.MapSize, GetValue(), GetSoldiers()) + ( this.extraCost ? GetValue() * Consts.Income : 0 );
        }

        protected override string GetExtra()
        {
            double soldiers = GetSoldiers();
            string retVal = MainForm.FormatPct(soldiers, true);
            if (retVal == "0.0%")
                retVal = string.Empty;
            return retVal;
        }
        private double GetSoldiers()
        {
            double soldiers = 0;
            int value = GetValue();
            if (value > 0)
                if (max.HasValue)
                    soldiers = PopCarrier.GetMoveSoldiers(this.totalPop, this.soldiers, value) / value;
                else if (from != null)
                    if (to == null || !to.Player.IsTurn)
                        soldiers = from.GetMoveSoldiers(value) / value;
                    else
                        soldiers = ( from.GetMoveSoldiers(value) + to.Soldiers ) / ( value + to.Population ) - to.GetSoldierPct();
            return soldiers;
        }

        private int GetPopulation()
        {
            int pop = colony.Population;
            int value = GetValue();
            if (from == colony)
                return pop - value;
            return pop + value;
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            int value = GetValue();

            lblTitle.Text = "Move Troops";
            lblSlideType.Text = "Troop";
            if (value != 1)
                lblSlideType.Text += "s";

            ShowProd(this.from as Colony, -value);
            ShowProd(this.to as Colony, value);
        }

        private static void ShowProd(Colony colony, int popChange)
        {
            if (colony != null && colony.Player.IsTurn)
                lblProd.Text = "Prod: " + colony.GetProductionIncome(colony.Population + popChange).ToString();
        }
    }
}
