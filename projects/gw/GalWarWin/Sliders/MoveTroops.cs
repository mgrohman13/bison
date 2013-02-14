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

        private readonly Colony colony;

        private readonly int? max;
        private readonly int free;

        private readonly int totalPop;
        private readonly double soldiers;

        public MoveTroops(PopCarrier from, PopCarrier to)
        {
            this.from = from;
            this.to = to;

            if (from.Player == to.Player)
            {
                if (( colony = from as Colony ) == null)
                    this.colony = to as Colony;
            }
            else
            {
                this.free = GetMoveTroopsMax();
            }
        }

        public MoveTroops(Colony from, int max, int free, int totalPop, double soldiers)
        {
            this.from = from;
            this.colony = from;

            this.max = max;
            this.free = free;

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

        protected override int GetMaxInternal()
        {
            return GetMoveTroopsMax();
        }
        private int GetMoveTroopsMax()
        {
            int max;
            if (this.max.HasValue)
                max = this.max.Value;
            else
                max = Math.Min(from.AvailablePop, to.FreeSpace);
            return Math.Min(max, free + GetMaxMovePop(MainForm.Game.CurrentPlayer.Gold));
        }

        public static int GetMaxMovePop(double gold)
        {
            int max = (int)( gold / Consts.MovePopulationGoldCost ) - 1;
            while (gold > PopCarrier.GetGoldCost(max))
                ++max;
            return ( max - 1 );
        }

        protected override double GetResult()
        {
            int value = GetValue() - free;
            if (value < 1)
                return 0;
            return PopCarrier.GetGoldCost(value);
        }

        protected override string GetExtra()
        {
            int value = GetValue();
            if (value > 0)
            {
                double soldiers = 0;
                if (max.HasValue)
                {
                    soldiers = PopCarrier.GetMoveSoldiers(this.totalPop, this.soldiers, value) / value;
                }
                else if (from != null)
                {
                    if (to == null || !to.Player.IsTurn)
                        soldiers = from.GetMoveSoldiers(value) / value;
                    else
                        soldiers = ( from.GetMoveSoldiers(value) + to.Soldiers ) / ( value + to.Population ) - to.GetSoldierPct();
                }
                if (soldiers != 0)
                {
                    string retVal = MainForm.FormatPct(soldiers, true);
                    if (retVal != "0.0%")
                        return ( soldiers > 0 ? "+" : string.Empty ) + retVal;
                }
            }
            return string.Empty;
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
