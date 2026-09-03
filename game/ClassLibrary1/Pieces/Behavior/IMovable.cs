using ClassLibrary1.Pieces.Enemies;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Behavior
{
    public interface IMovable : IBehavior
    {
        public double MoveCur { get; }
        public double MoveInc { get; }
        public double MoveIncBase { get; }
        public int MoveMax { get; }
        public int MoveLimit { get; }
        bool Moved { get; }
        public bool CanMove { get; }

        void Upgrade(Values values, bool resetFlags = false, double? cur = null);
        internal void Damage(double dmgPct);
        public bool Move(Tile to);
        internal bool EnemyMove(Tile to);
        internal bool Port(Portal portal);
        public double GetInc();

        public bool CanMoveTo(Tile other);
        public double DistTo(Tile other) => Piece.Tile.MoveDistTo(other);

        [Serializable]
        [DataContract(IsReference = true)]
        public readonly struct Values
        {
            private readonly int _moveMax, _moveLimit;
            private readonly double _moveInc;

            public Values(double moveInc, int moveMax, int moveLimit)
            {
                if (!(moveInc < moveMax && moveMax < moveLimit)) throw new Exception();

                _moveInc = moveInc;
                _moveMax = moveMax;
                _moveLimit = moveLimit;
            }
            public Values(IMovable movable)
                : this(movable.MoveInc, movable.MoveMax, movable.MoveLimit)
            { }
            public Values(Values movable)
                : this(movable.MoveInc, movable.MoveMax, movable.MoveLimit)
            { }

            public double MoveInc => _moveInc;
            public int MoveMax => _moveMax;
            public int MoveLimit => _moveLimit;

            public static bool operator !=(Values left, Values right) => !(left == right);
            public static bool operator ==(Values left, Values right) => left.Equals(right);

            public override bool Equals([NotNullWhen(true)] object obj)
            {
                if (obj == null)
                    return false;
                Values other = (Values)obj;
                return _moveMax == other._moveMax && _moveLimit == other._moveLimit && _moveInc == other._moveInc;
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(MoveInc, MoveMax, MoveLimit);
            }
        }
    }
}
