using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class MoveTroops : SliderController
    {
        private readonly Game game;

        private readonly PopCarrier from;
        private readonly PopCarrier to;

        private readonly Colony colony;

        private readonly int? max;
        private readonly int free;

        private readonly int totalPop;
        private readonly double soldiers;

        public MoveTroops(Game game, PopCarrier from, PopCarrier to)
        {
            this.game = game;

            this.from = from;
            this.to = to;

            if (from.Player == to.Player)
                if (( colony = from as Colony ) == null)
                    colony = to as Colony;
        }

        public MoveTroops(Game game, Colony from, int max, int free, int totalPop, double soldiers)
        {
            this.game = game;

            this.from = from;
            this.colony = from;

            this.max = max;
            this.free = free;

            this.totalPop = totalPop;
            this.soldiers = soldiers;
        }

        public override double GetInitial()
        {
            if (max.HasValue && colony != null)
                return 0;
            return GetMax();
        }

        protected override int GetMaxInternal()
        {
            int max;
            if (this.max.HasValue)
                max = this.max.Value;
            else
                max = Math.Min(from.AvailablePop, to.FreeSpace);
            return Math.Min(max, free + (int)Math.Floor(game.CurrentPlayer.Gold / Consts.MovePopulationGoldCost));
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
                        soldiers = ( from.GetMoveSoldiers(value) + to.Soldiers) / ( value + to.Population ) - to.GetSoldierPct();
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
            lblTitle.Text = "Move Troops";
            lblSlideType.Text = "Troop";
            if (GetValue() != 1)
                lblSlideType.Text += "s";
        }
    }
}
