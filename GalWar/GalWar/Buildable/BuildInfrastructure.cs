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
