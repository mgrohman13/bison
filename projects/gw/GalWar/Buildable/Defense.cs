using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class Defense : PlanetDefense
    {
        internal Defense()
            : base()
        {
        }

        public override string ToString()
        {
            return "Defense";
        }
    }
}
