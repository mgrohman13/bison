using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class BuildGold : Buildable
    {
        internal BuildGold(Colony colony)
            : base(colony)
        {
        }

        internal override bool Build(IEventHandler handler, double production)
        {
            if (this.production != 0)
                throw new Exception();
            colony.Player.AddGold(production / Consts.GoldProductionForGold);
            return false;
        }
        internal override void GetTurnIncome(ref double production, ref double gold, bool minGold)
        {
            gold += production / Consts.GoldProductionForGold;
            production = 0;
        }

        public override double GetAddProduction(double production, bool floor)
        {
            return 0;
        }

        public override string ToString()
        {
            return "Gold";
        }
    }
}
