using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Repair : IRepair
    {
        private readonly Piece _piece;
        private IRepair.Values _values;

        public Piece Piece => _piece;

        public double Range => Consts.GetDamagedValue(Piece, _values.Range, 1);
        public double RangeBase => _values.Range;
        public double Rate => Consts.GetDamagedValue(Piece, _values.Rate, 0);
        public double RateBase => _values.Rate;

        public Repair(Piece piece, IRepair.Values values)
        {
            this._piece = piece;
            this._values = values;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return this as T;
        }

        void IRepair.Upgrade(IRepair.Values repair)
        {
            this._values = repair;
        }

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
        }
    }
}
