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
    [Serializable]
    public class Movable : IMovable
    {
        private readonly Piece piece;
        private readonly double _moveInc, _moveMax, _moveLimit;

        private double _moveCur;

        public Movable(Piece piece, double moveInc, double moveMax, double moveLimit)
        {
            this.piece = piece;
            this._moveCur = 0;
            this._moveInc = moveInc;
            this._moveMax = moveMax;
            this._moveLimit = moveLimit;
        }

        public double MoveCur => _moveCur;
        public double MoveInc => _moveInc;
        public double MoveMax => _moveMax;
        public double MoveLimit => _moveLimit;

        public bool Move(Map.Tile to)
        {
            if (to.Piece == null)
            {
                double dist = piece.Tile.GetDistance(to);
                if (dist <= MoveCur)
                {
                    this._moveCur -= dist;
                    piece.SetTile(to);
                    return true;
                }
            }
            return false;
        }

        public void EndTurn()
        {
            AddMove(MoveInc);
        }

        private void AddMove(double inc)
        {
            double max = Math.Max(this.MoveCur, MoveMax);
            this._moveCur += inc;

            double extra = MoveCur - max;
            if (extra > 0)
            {
                double limit = MoveLimit - max;
                double mult = limit / (limit + MoveMax);
                extra *= Math.Pow(mult, Consts.MoveLimitPow);
                extra += max;

                this._moveCur = extra;
            }

            Debug.WriteLine(MoveCur);
        }
    }
}
