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

        internal override double GetAddProduction(double production, bool floor)
        { 
            return 0;
        }
        internal override void GetTurnIncome(ref double production, ref double gold, ref int infrastructure)
        {
            gold += production / Consts.GoldProductionForGold;
            production = 0;
        }

        internal override void Build(double production)
        {
            if (this.production != 0)
                throw new Exception();
            colony.Player.AddGold(production / Consts.GoldProductionForGold);
        }

        public override string ToString()
        {
            return "Gold";
        }
    }
}
