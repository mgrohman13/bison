using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Movable : IMovable
    {
        private IPiece piece;
        private double _moveCur, _moveInc, _moveMax, _moveLimit;

        public Movable(IPiece piece, double moveInc, double moveMax, double moveLimit)
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
            this._moveCur += inc;

            double extra = MoveCur - MoveMax;
            if (extra > 0)
            { 
                extra /= MoveLimit - MoveMax;
                if (extra > .5)
                    extra /= (extra + .5);
                extra *= MoveLimit - MoveMax;
                extra += MoveMax;

                this._moveCur = extra;
            }
        }
    }
}
