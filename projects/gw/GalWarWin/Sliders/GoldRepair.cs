using System;
using System.Windows.Forms;
using GalWar;
using MattUtil;

namespace GalWarWin.Sliders
{
    public class GoldRepair : SliderController
    {
        public delegate void SetValueDelegate(double value);

        private static AutoRepairControl control = new AutoRepairControl();

        private readonly Ship ship;
        private readonly int max;

        private int maxHP;
        private double repairCost;

        public GoldRepair(Ship ship)
        {
            this.ship = ship;

            this.max = TBSUtil.FindValue(delegate (int hp)
            {
                return ship.Player.HasGold(ship.GetGoldForHP(hp));
            }, 0, ship.MaxHP - ship.HP, false);
        }
        public GoldRepair(int maxHP, double repairCost)
        {
            this.maxHP = maxHP;
            this.repairCost = repairCost;

            this.max = maxHP - 1;
        }

        public override Control GetCustomControl()
        {
            if (ship == null)
                return null;
            control.SetShip(ship);
            return control;
        }

        public override double GetInitial()
        {
            if (ship == null || ship.GetRepairedFrom() != null)
                return GetOptimalProd();
            if (ship.Colony)
                return GetOptimalColony();
            return GetDefault();
        }

        public override int GetMin()
        {
            return 0;
        }

        private double GetDefault()
        {
            if (ship == null)
                return 1;
            return ship.GetAutoRepairHP();
        }

        private int GetOptimalProd()
        {
            return FindValue(delegate (int repair)
            {
                if (ship == null)
                    return Consts.GoldForProduction * this.repairCost * repair - Consts.GetGoldRepairCost(repair, this.maxHP, this.repairCost);
                return Consts.GoldForProduction * ship.GetProdForHP(repair) - ship.GetGoldForHP(repair);
            });
        }

        private int GetOptimalColony()
        {
            return FindValue(delegate (int repair)
            {
                return ship.GetColonizationValue(repair) - ship.GetGoldForHP(repair);
            });
        }

        private delegate double FindValueDelegate(int value);
        private int FindValue(FindValueDelegate FindValue)
        {
            return TBSUtil.FindValue(delegate (int value)
            {
                if (value < this.max)
                    return ( FindValue(value) > FindValue(value + 1) );
                else
                    return true;
            }, 0, this.max, true);
        }

        protected override int GetMaxInternal()
        {
            return this.max;
        }

        protected override double GetResult()
        {
            if (ship == null)
                return Consts.GetGoldRepairCost(GetValue(), this.maxHP, this.repairCost);
            return ship.GetGoldForHP(GetValue());
        }

        protected override string GetExtra()
        {
            double repairCost;
            if (ship == null)
                repairCost = this.repairCost;
            else
                repairCost = ship.GetProdForHP(1);
            return repairCost.ToString("0.00");
        }

        protected override void SetText(Label lblTitle, Label lblSlideType)
        {
            lblTitle.Text = "Repair Ship";
            lblSlideType.Text = "HP";
        }

        internal override double lblExtra_Click()
        {
            return GetOptimalProd();
        }

        internal override double lblEffcnt_Click()
        {
            return GetDefault();
        }
    }
}
