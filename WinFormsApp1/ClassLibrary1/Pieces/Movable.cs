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
        private readonly Piece _piece;
        private IMovable.Values _values;

        private double _moveCur;

        public Piece Piece => _piece;

        public Movable(Piece piece, IMovable.Values values)
        {
            this._piece = piece;
            this._values = values;

            this._moveCur = 0;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        public double MoveCur => _moveCur;
        public double MoveInc => _values.MoveInc;
        public double MoveMax => _values.MoveMax;
        public double MoveLimit => _values.MoveLimit;

        void IMovable.Upgrade(IMovable.Values movable)
        {
            this._values = movable;
        }

        bool IMovable.Move(Map.Tile to)
        {
            bool move = (Piece.IsPlayer && to != null && to.Piece == null && to.Visible);
            return Move(move, to);
        }
        bool IMovable.EnemyMove(Map.Tile to)
        {
            bool move = (Piece.IsEnemy && to != null && to.Piece == null);
            return Move(move, to);
        }
        private bool Move(bool Move, Map.Tile to)
        {
            if (Move && Piece.Tile != to)
            {
                double dist = Piece.Tile.GetDistance(to);
                if (dist <= MoveCur)
                {
                    this._moveCur -= dist;
                    Piece.SetTile(to);
                    return true;
                }
            }
            return false;
        }

        public double GetInc()
        {
            return IncMove(false);
        }
        private double IncMove(bool doInc)
        {
            double moveInc = Consts.IncValueWithMaxLimit(MoveCur, MoveInc, Consts.MoveDev, MoveMax, MoveLimit, Consts.MoveLimitPow, doInc);
            if (doInc)
                this._moveCur += moveInc;
            return moveInc;
        }
        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk);
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(true, ref energyUpk);
        }
        private void EndTurn(bool doEndTurn, ref double energyUpk)
        {
            energyUpk += IncMove(doEndTurn) * Consts.UpkeepPerMove;
        }
    }
}
