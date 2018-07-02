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
                if (!this.StoresProduction)
                    throw new Exception();
                return this._production;
            }
            protected set
            {
                if (!this.StoresProduction)
                    throw new Exception();
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
        public abstract bool StoresProduction
        {
            get;
        }

        internal abstract bool Build(IEventHandler handler, double production);

        public string GetProdText(string curProd)
        {
            return curProd + ( this.Cost.HasValue ? " / " + this.Cost.Value.ToString() : string.Empty );
        }

        internal abstract void GetTurnIncome(ref double production, ref double gold, bool minGold);

        protected void LoseProduction(double loseProduction)
        {
            double production = this.production, gold = 0;
            LoseProduction(loseProduction, ref production, ref gold, Consts.ProductionForGold);

            this.production = RoundValue(production, ref gold, Consts.ProductionForGold);

            this.Player.AddGold(gold);
        }
        protected void LoseProduction(double loseProduction, ref double production, ref double gold, double rate)
        {
            gold += loseProduction / rate;
            production -= loseProduction;
        }
    }
}
