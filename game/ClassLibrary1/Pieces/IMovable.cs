using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

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
        public bool Move(Map.Tile to);
        internal bool EnemyMove(Map.Tile to);
        public double GetInc();

        [Serializable]
        public struct Values
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
