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

        public override double Upkeep
        {
            get
            {
                TurnException.CheckTurn(colony.Player);

                return base.Upkeep / Consts.StoreProdRatio;
            }

        }

        internal override double GetAddProduction(double production, bool floor)
        { 
            return base.GetAddProduction(production * Consts.StoreProdRatio, floor);
        }
        internal override void GetTurnIncome(ref double production, ref double gold, ref int infrastructure)
        {
            double addProd = production * Consts.StoreProdRatio;
            gold += (production - addProd) / Consts.ProductionForGold;
            production = addProd;
        }

        internal override void Build(double production)
        {
            int addProd = Game.Random.Round(production * Consts.StoreProdRatio);
            colony.Player.AddGold((production - addProd) / Consts.ProductionForGold);
            this.production += addProd;
        }

        public override string ToString()
        {
            return "Store Production";
        }
    }
}
