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

        public override double Upkeep
        {
            get
            {
                return base.Upkeep * Math.Max(Consts.SoldierUpkeepMult, Consts.PlanetDefensesUpkeepMult);
            }
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

        internal override void GetTurnIncome(ref double production, ref double gold, ref int infrastructure)
        {
            AssertException.Assert(production == (int)production);
            infrastructure += (int)production;
            production = 0;
        }

        internal override List<Ship> Build(IEventHandler handler, double production)
        {
            this.production += Game.Random.Round(production);
            return null;
        }

        public override string ToString()
        {
            return "Infrastructure";
        }
    }
}
