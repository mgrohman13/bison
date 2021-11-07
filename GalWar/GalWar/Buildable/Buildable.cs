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
                TurnException.CheckTurn(colony.Player);

                return production;
            }
            protected set
            {
                checked
                {
                    this._production = (ushort)value;
                }
            }
        }
        protected internal int production
        {
            get
            {
                return this._production;
            }
            set
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
                TurnException.CheckTurn(colony.Player);

                return null;
            }
        }

        public virtual double Upkeep
        {
            get
            {
                return production * Consts.GetProductionUpkeepMult(colony.Player.Game.MapSize);
            }
        }

        public string GetProdText(string curProd)
        {
            TurnException.CheckTurn(colony.Player);

            return (this.Cost.HasValue ? curProd + " / " + this.Cost.Value.ToString() : curProd);
        }

        internal abstract List<Ship> Build(IEventHandler handler, double production);

        internal virtual void GetTurnIncome(ref double production, ref double gold, bool minGold)
        {
        }
        public virtual double GetAddProduction(double production, bool floor)
        {
            if (floor)
                return Math.Floor(production * Consts.FLOAT_ERROR_ONE);
            else
                return production;
        }
        internal void AddProduction(int production)
        {
            AssertException.Assert(-production <= this.production);

            this.production += production;
        }
    }
}
