﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public abstract class PlanetDefense : Buildable
    {
        internal PlanetDefense(Colony colony)
            : base(colony)
        {
        }

        internal override bool Build(IEventHandler handler, int production)
        {
            production += this.Production;
            this.Production = 0;
            colony.BuildPlanetDefense(production);
            return false;
        }
    }
}
