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
                TurnException.CheckTurn(colony.Player);

                return production * Consts.GetProductionUpkeepMult(colony.Player.Game.MapSize);
            }
        }

        public string GetProdText(string curProd)
        {
            TurnException.CheckTurn(colony.Player);

            return (this.Cost.HasValue ? curProd + " / " + this.Cost.Value.ToString() : curProd);
        }
        public virtual double GetAddProduction(double production, bool floor)
        {
            TurnException.CheckTurn(colony.Player);

            if (floor)
                return Math.Floor(production * Consts.FLOAT_ERROR_ONE);
            else
                return production;
        }
        internal virtual void GetTurnIncome(ref double production, ref double gold, ref int infrastructure)
        {
        }

        internal void AddProduction(int production)
        {
            AssertException.Assert(-production <= this.production);

            this.production += production;
        }
        internal virtual void Build(double production)
        {
            this.production += Game.Random.Round(production);
        }
        internal virtual List<Ship> BuildShips(IEventHandler handler)
        {
            return new List<Ship>();
        }
    }
}
