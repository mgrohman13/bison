using System;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Repair : Builder, IRepair
    {
        private IRepair.Values _values;

        public int Rate => RateBase;// Consts.GetDamagedValue(Piece, RateBase, 0);
        public int RateBase => _values.Rate;

        public Repair(Piece piece, IRepair.Values repair)
            : base(piece, repair.Builder)
        {
            this._values = repair;
        }

        void IRepair.Upgrade(IRepair.Values repair)
        {
            Builder.UpgradeAll(Piece, repair.Builder);
            this._values = repair;
        }
    }
}
