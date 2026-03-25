using System;
using System.Runtime.Serialization;

namespace ClassLibrary1.Pieces.Behavior
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Repair(Piece piece, IRepair.Values repair) : Builder(piece, repair.Builder), IRepair
    {
        private IRepair.Values _values = repair;
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

        void IRepair.Upgrade(IRepair.Values repair)
        {
            UpgradeAll(Piece, repair.Builder);
            _values = repair;
        }
    }
}
