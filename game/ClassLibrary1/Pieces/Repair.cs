using System;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Repair : Builder, IRepair
    {
        private IRepair.Values _values;
        private bool _repaired = false, _resetRepaired = true;

        public double Rate => Consts.GetDamagedValue(Piece, RateBase, 0);
        public int RateBase => _values.Rate;

        bool IRepair.Repaired
        {
            get => _repaired;
            set
            {
                this._repaired = value;
                this._resetRepaired = false;
            }
        }

        void IBehavior.StartTurn()
        {
            this._resetRepaired = true;
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            if (this._resetRepaired)
                this._repaired = false;
        }

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
