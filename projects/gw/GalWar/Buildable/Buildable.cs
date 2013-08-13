using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public abstract class Buildable
    {
        [NonSerialized]
        private double _production;

        protected Buildable()
        {
            checked
            {
                this._production = double.NaN;
            }
        }

        internal protected double production
        {
            get
            {
                return this._production;
            }
            protected set
            {
                checked
                {
                    this._production = value;
                }
            }
        }

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

        public virtual bool HandlesFraction
        {
            get
            {
                return false;
            }
        }

        internal void SetFraction(double production)
        {
            this.production = production;
        }

        internal abstract void Build(IEventHandler handler, Colony colony, Tile tile);

        internal abstract bool CanBeBuiltBy(Colony colony);

        public abstract string GetProdText(string curProd);
    }
}
