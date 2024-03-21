using System;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces
{
    public interface IMovable : IBehavior
    {
        public double MoveCur { get; }
        public double MoveInc { get; }
        public double MoveIncBase { get; }
        public int MoveMax { get; }
        public int MoveLimit { get; }

        void Upgrade(Values values);
        public bool Move(Tile to);
        internal bool EnemyMove(Tile to);
        public double GetInc();

        [Serializable]
        public readonly struct Values
        {
            private readonly int _moveMax, _moveLimit;
            private readonly double _moveInc;
            public Values(double moveInc, int moveMax, int moveLimit)
            {
                if (moveInc <= 0 || moveMax <= 0 || moveLimit <= 0)
                    moveInc = moveMax = moveLimit = 0;
                this._moveInc = moveInc;
                this._moveMax = moveMax;
                this._moveLimit = moveLimit;
            }
            public double MoveInc => _moveInc;
            public int MoveMax => _moveMax;
            public int MoveLimit => _moveLimit;
        }
    }
}
