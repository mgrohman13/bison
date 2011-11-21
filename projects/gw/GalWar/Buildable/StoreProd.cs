using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class StoreProd : Buildable
    {
        internal StoreProd()
        {
        }

        public override int Cost
        {
            get
            {
                return int.MaxValue;
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

        internal override void Build(Colony colony, Tile tile, IEventHandler handler)
        {
        }

        internal override bool CanBeBuiltBy(Colony colony)
        {
            return true;
        }

        public override string GetProdText(string curProd)
        {
            return curProd;
        }

        public override string ToString()
        {
            return "Store Production";
        }
    }
}
