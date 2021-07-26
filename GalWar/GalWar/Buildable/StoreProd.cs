using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class StoreProd : Buildable
    {
        internal StoreProd(Colony colony)
            : base(colony)
        {
        }

        internal override List<Ship> Build(IEventHandler handler, double production)
        {
            int addProd = Game.Random.Round(production * Consts.StoreProdRatio);
            colony.Player.AddGold(( production - addProd ) / Consts.ProductionForGold);
            this.production += addProd;
            return null;
        }
        internal override void GetTurnIncome(ref double production, ref double gold, bool minGold)
        {
            double addProd = production * Consts.StoreProdRatio;
            gold += ( production - addProd ) / Consts.ProductionForGold;
            production = addProd;
        }

        public override double GetAddProduction(double production, bool floor)
        {
            return base.GetAddProduction(production * Consts.StoreProdRatio, floor);
        }

        public override string ToString()
        {
            return "Store Production";
        }
    }
}
