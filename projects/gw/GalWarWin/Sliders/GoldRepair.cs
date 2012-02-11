using System;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin.Sliders
{
    public class GoldRepair : SliderController
    {
        //private static AutoRepairControl control = new AutoRepairControl();

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

        //public override Control GetCustomControl()
        //{
        //    control.SetShip(ship);
        //    return control;
        //}

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
            return ( ship.MaxHP * Consts.RepairGoldHPPct );
        }

        private int GetOptimalProd()
        {
            return MattUtil.TBSUtil.FindValue(delegate(int repair)
            {
                if (repair < this.max)
                {
                    double cur = GetProdSaved(repair);
                    double next = GetProdSaved(repair + 1);
                    return ( cur > next );
                }
                return true;
            }, 0, this.max, true);
        }

        private double GetProdSaved(int repair)
        {
            return Consts.GoldForProduction * ship.GetProdForHP(repair) - ship.GetGoldForHP(repair);
        }

        private int GetOptimalColony()
        {
            return MattUtil.TBSUtil.FindValue(delegate(int repair)
            {
                if (repair < this.max)
                {
                    double cur = GetColony(repair);
                    double next = GetColony(repair + 1);
                    return ( cur > next );
                }
                return true;
            }, 0, this.max, true);
        }

        private double GetColony(int repair)
        {
            return ship.GetColonizationValue(repair) - ship.GetGoldForHP(repair);
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
