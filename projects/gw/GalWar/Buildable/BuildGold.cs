using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    public class BuildGold : Buildable
    {
        internal BuildGold(Colony colony)
            : base(colony)
        {
        }

        public override bool StoresProduction
        {
            get
            {
                return false;
            }
        }

        internal override bool Build(IEventHandler handler, double production)
        {
            colony.Player.AddGold(production / Consts.GoldProductionForGold);
            return false;
        }

        public override string ToString()
        {
            return "Gold";
        }
    }
}
