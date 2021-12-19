using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    public interface IRepair : IBehavior
    {
        public double Range { get; }
        public double RangeBase { get; }
        public double Rate { get; }
        public double RateBase { get; }

        internal double GetRepairInc(IKillable killable);

        internal void Upgrade(Values repair);

        [Serializable]
        public struct Values
        {
            private readonly double _range, _rate;
            public Values(double range, double rate)
            {
                this._range = range;
                this._rate = rate;
            }
            public double Range => _range;
            public double Rate => _rate;
        }
    }
}
