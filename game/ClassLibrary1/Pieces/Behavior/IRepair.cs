using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Behavior
{
    public interface IRepair : IBuilder
    {
        public double Rate { get; }
        public int RateBase { get; }

        internal bool Repaired { get; set; }

        internal void Upgrade(Values values);

        [Serializable]
        public new readonly struct Values
        {
            public readonly IBuilder.Values Builder;

            private readonly int _rate;
            public Values(IBuilder.Values builder, int rate)
            {
                Builder = builder;
                _rate = rate;
            }
            public int Rate => _rate;
        }
    }
}
