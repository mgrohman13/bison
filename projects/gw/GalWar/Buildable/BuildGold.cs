using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    public class BuildGold : Buildable
    {
        internal BuildGold(Colony colony)
            : base(colony)
        {
        }

        internal override bool Build(IEventHandler handler, double production)
        {
            if (this.Production != 0)
                throw new Exception();
            colony.Player.AddGold(production / Consts.GoldProductionForGold);
            return false;
        }
        internal override void GetTurnIncome(ref double production, ref double gold, bool minGold)
        {
            gold += production / Consts.GoldProductionForGold;
            production = 0;
        }

        internal override double GetAddProduction(double production, bool floor)
        {
            return 0;
        }

        public override string ToString()
        {
            return "Gold";
        }
    }
}
