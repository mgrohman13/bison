using System;

namespace ClassLibrary1.Pieces.Behavior
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
                _repaired = value;
                _resetRepaired = false;
            }
        }

        void IBehavior.StartTurn()
        {
            _resetRepaired = true;
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            if (_resetRepaired)
                _repaired = false;
        }

        public Repair(Piece piece, IRepair.Values repair)
            : base(piece, repair.Builder)
        {
            _values = repair;
        }

        void IRepair.Upgrade(IRepair.Values repair)
        {
            UpgradeAll(Piece, repair.Builder);
            _values = repair;
        }
    }
}
