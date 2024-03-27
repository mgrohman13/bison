using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    public interface IRepair : IBuilder
    {
        public double Rate { get; }
        public double RateBase { get; }

        internal void Upgrade(IRepair.Values values);

        [Serializable]
        public new readonly struct Values
        {
            public readonly IBuilder.Values Builder;

            private readonly double _rate;
            public Values(IBuilder.Values builder, double rate)
            {
                this.Builder = builder;
                this._rate = rate;
            }
            public double Rate => _rate;
        }
    }
}
