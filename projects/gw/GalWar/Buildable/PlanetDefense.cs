using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public abstract class PlanetDefense : Buildable
    {
        internal PlanetDefense()
        {
        }

        public override int Cost
        {
            get
            {
                return 0;
            }
        }

        internal override bool NeedsTile
        {
            get
            {
                return false;
            }
        }

        internal override bool Multiple
        {
            get
            {
                return false;
            }
        }

        public override bool HandlesFraction
        {
            get
            {
                return true;
            }
        }

        internal override void Build(IEventHandler handler, Colony colony, Tile tile)
        {
            colony.BuildPlanetDefense(this.production);
            this.production = 0;
        }

        internal override bool CanBeBuiltBy(Colony colony)
        {
            return true;
        }

        public override string GetProdText(string curProd)
        {
            return string.Empty;
        }
    }
}
