﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public class StoreProd : Buildable
    {
        internal StoreProd(Colony colony, int production)
            : base(colony)
        {
            this.Production = production;
        }

        public override bool StoresProduction
        {
            get
            {
                return true;
            }
        }

        internal override bool Build(IEventHandler handler, double production)
        {
            this.Production += Game.Random.Round(production * ( 1 - Consts.StoreProdLossPct ));
            return false;
        }

        public override string ToString()
        {
            return "Store Production";
        }
    }
}
