using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class GoldRepair : SliderController
    {
        public delegate void SetValueDelegate(double value);

        private static AutoRepairControl control = new AutoRepairControl();

        private readonly Ship ship;
        private readonly int max;

        public GoldRepair(Ship ship)
        {
            this.ship = ship;
            this.max = MattUtil.TBSUtil.FindValue(delegate(int hp)
            {
                return ( ship.GetGoldForHP(hp) < ship.Player.Gold );
            }, 0, ship.MaxHP - ship.HP, false);
        }

        internal void SetSetValueDelegate(GoldRepair.SetValueDelegate SetValue)
        {
            control.SetSetValueDelegate(SetValue);
        }

        public override Control GetCustomControl()
        {
            control.SetShip(ship);
            return control;
        }

        public override double GetInitial()
        {
            foreach (Tile tile in Tile.GetNeighbors(ship.Tile))
            {
                Planet planet = tile.SpaceObject as Planet;
                if (planet != null)
                {
                    if (planet.Colony != null && planet.Colony.Player.IsTurn && planet.Colony.RepairShip == ship)
                        return GetOptimalProd();
                    break;
                }
            }
            if (ship.Colony)
                return GetOptimalColony();
            return GetDefault();
        }

        private double GetDefault()
        {
            return ship.GetDefaultGoldRepair();
        }

        private int GetOptimalProd()
        {
            return FindValue(delegate(int repair)
            {
                return Consts.GoldForProduction * ship.GetProdForHP(repair) - ship.GetGoldForHP(repair);
            });
        }

        private int GetOptimalColony()
        {
            return FindValue(delegate(int repair)
            {
                return ship.GetColonizationValue(repair) - ship.GetGoldForHP(repair);
            });
        }

        private delegate double FindValueDelegate(int value);
        private int FindValue(FindValueDelegate FindValue)
        {
            return MattUtil.TBSUtil.FindValue(delegate(int value)
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
            return ship.GetGoldForHP(GetValue());
        }

        protected override string GetExtra()
        {
            return ship.GetProdForHP(1).ToString("0.00");
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
