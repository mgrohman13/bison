using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class BuildInfrastructure : Buildable
    {
        internal BuildInfrastructure(Colony colony)
            : base(colony)
        {
        }

        public override int? Cost
        {
            get
            {
                TurnException.CheckTurn(colony.Player);

                colony.GetUpgMins(out int pd, out int sold);
                return Math.Min(pd, sold);
            }
        }
        public override double Upkeep
        {
            get
            {
                TurnException.CheckTurn(colony.Player);

                return base.Upkeep * Math.Max(Consts.SoldierUpkeepMult, Consts.PlanetDefensesUpkeepMult);
            }
        }

        internal override void GetTurnIncome(ref double production, ref double gold, ref int infrastructure)
        {
            if (production != (int)production)
                throw new Exception();

            infrastructure += (int)production;
            production = 0;
        }

        public override string ToString()
        {
            return "Infrastructure";
        }
    }
}
