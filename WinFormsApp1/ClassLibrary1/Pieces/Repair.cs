using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Repair : Builder, IRepair
    {
        private IRepair.Values _values;

        public double Rate => Consts.GetDamagedValue(Piece, RateBase, 0);
        public double RateBase => _values.Rate;

        public Repair(Piece piece, IRepair.Values repair)
            : base(piece, repair.Builder)
        {
            this._values = repair;
        }

        void IRepair.Upgrade(IRepair.Values repair)
        {
            ((IBuilder)this).Upgrade(repair.Builder);
            this._values = repair;
        } 
    }
}
