using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class BuildAttack : PlanetDefense
    {
        internal BuildAttack(Colony colony)
            : base(colony)
        {
        }

        public override string ToString()
        {
            return "Attack";
        }
    }
}
