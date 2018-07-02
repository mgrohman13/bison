using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class BuildDefense : PlanetDefense
    {
        internal BuildDefense(Colony colony)
            : base(colony)
        {
        }

        public override string ToString()
        {
            return "Defense";
        }
    }
}
