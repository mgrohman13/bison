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
        private readonly IRepair.Values _values;

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

        void IBehavior.GetUpkeep(ref double energy, ref double mass)
        {
            mass += GetRepairs().Sum(p => GetRepairCost(p.Key, p.Value));
        }
        void IBehavior.EndTurn()
        {
            foreach (var p in GetRepairs())
                p.Key.Repair(p.Value);
        }

        double IRepair.GetRepairInc(IKillable killable)
        {
            GetRepairs().TryGetValue(killable, out double repair);
            return repair;
        }

        internal Dictionary<IKillable, double> GetRepairs()
        {
            return Piece.Tile.GetTilesInRange(Range).Select(t => t.Piece).OfType<IKillable>()
                .Where(k => k.Piece != this.Piece && k.Piece.Side == this.Piece.Side && k.HitsCur < k.HitsMax)
                .ToDictionary(k => k, k => Math.Min(k.HitsMax - k.HitsCur, k.HitsMax * this.Rate));
        }
        public static double GetRepairCost(IKillable killable, double amt)
        {
            return killable.RepairCost * amt / killable.HitsMax;
        }
    }
}
