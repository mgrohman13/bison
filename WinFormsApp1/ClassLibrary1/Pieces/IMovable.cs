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
        public double MoveMax { get; }
        public double MoveLimit { get; }

        public double GetInc();

        public bool Move(Map.Tile to);
        internal bool EnemyMove(Map.Tile to);

        public class Values
        {
            private readonly double _moveInc, _moveMax, _moveLimit;
            public Values(double moveInc, double moveMax, double moveLimit)
            {
                if (moveInc <= 0 || moveMax <= 0 || moveLimit <= 0)
                    moveInc = moveMax = moveLimit = 0;
                this._moveInc = moveInc;
                this._moveMax = moveMax;
                this._moveLimit = moveLimit;
            }
            public double MoveInc => _moveInc;
            public double MoveMax => _moveMax;
            public double MoveLimit => _moveLimit;
        }
    }
}
