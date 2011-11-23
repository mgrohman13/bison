using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public abstract class Buildable
    {
        public abstract int Cost
        {
            get;
        }

        internal abstract bool NeedsTile
        {
            get;
        }

        internal abstract bool Multiple
        {
            get;
        }

        internal abstract void Build(Colony colony, Tile tile, IEventHandler handler);

        internal abstract bool CanBeBuiltBy(Colony colony);

        public abstract string GetProdText(string curProd);
    }
}
