using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        [DataContract(IsReference = true)]
        public new readonly struct Values(IBuilder.Values builder, int rate)
        {
            public readonly IBuilder.Values Builder = builder;

            private readonly int _rate = rate;

            public int Rate => _rate;
        }
    }
}
