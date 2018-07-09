using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class RepairShip : Buildable
    {
        private Ship ship;

        internal RepairShip(Colony colony, Ship ship)
            : base(colony)
        {
            this.ship = ship;
        }

        internal override bool Build(IEventHandler handler, int prodInc)
        {
            double gold = 0;
            double production = this.Production + prodInc;
            this.Production = 0;
            ship.ProductionRepair(ref production, ref gold, true, false);
            colony.Player.AddGold(gold);
            colony.AddProduction(production);
            return false;
        }

        internal override void GetTurnIncome(ref double production, ref double gold, bool minGold)
        {
            ship.ProductionRepair(ref production, ref gold, false, minGold);
        }

        public override string ToString()
        {
            return "Repair " + ship.ToString();
        }
    }
}
