using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class Attack : PlanetDefense
    {
        internal Attack()
            : base()
        {
        }

        public override string ToString()
        {
            return "Attack";
        }
    }
}
