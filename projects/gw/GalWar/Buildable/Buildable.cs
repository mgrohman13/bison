using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    [Serializable]
    public abstract class Buildable
    {
        protected Colony colony;
        private ushort _production;

        protected Buildable(Colony colony)
        {
            this.colony = colony;
        }

        public int Production
        {
            get
            {
                return this._production;
            }
            protected set
            {
                checked
                {
                    this._production = (ushort)value;
                }
            }
        }
        public virtual int? Cost
        {
            get
            {
                return null;
            }
        }
        public string GetProdText(string curProd)
        {
            return curProd + ( this.Cost.HasValue ? " / " + this.Cost.Value.ToString() : string.Empty );
        }

        internal abstract bool Build(IEventHandler handler, int production);

        internal virtual void GetTurnIncome(ref double production, ref double gold, bool minGold)
        {
        }
        internal virtual double GetAddProduction(double production, bool floor)
        {
            if (floor)
                return Math.Floor(production * Consts.FLOAT_ERROR_ONE);
            else
                return production;
        }
        internal void AddProduction(int production)
        {
            this.Production += production;
        }
    }
}
