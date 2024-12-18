﻿using ClassLibrary1.Pieces.Enemies;
using System;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces
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

        void Upgrade(Values values);
        public bool Move(Tile to);
        internal bool EnemyMove(Tile to);
        internal bool Port(Portal portal);
        public double GetInc();

        [Serializable]
        public readonly struct Values
        {
            private readonly int _moveMax, _moveLimit;
            private readonly double _moveInc;

            public Values(double moveInc, int moveMax, int moveLimit)
            {
                if (!(moveInc < moveMax && moveMax < moveLimit)) throw new Exception();

                this._moveInc = moveInc;
                this._moveMax = moveMax;
                this._moveLimit = moveLimit;
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
        }
    }
}
