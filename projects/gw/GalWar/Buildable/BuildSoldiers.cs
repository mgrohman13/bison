using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class BuildSoldiers : Buildable
    {
        internal BuildSoldiers(Colony colony)
            : base(colony)
        {
        }

        internal override bool Build(IEventHandler handler, double production)
        {
            colony.BuildSoldiers(production, true);
            return false;
        }

        public override string ToString()
        {
            return "Soldiers";
        }
    }
}
