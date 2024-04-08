using System;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Movable : IMovable
    {
        private readonly Piece _piece;
        private IMovable.Values _values;

        private double _moveCur;
        private bool _moved;

        public Piece Piece => _piece;

        public Movable(Piece piece, IMovable.Values values)
            : this(piece, values, 0)
        {
        }
        public Movable(Piece piece, IMovable.Values values, double moveCur)
        {
            this._piece = piece;
            this._values = values;

            this._moveCur = moveCur;
            this._moved = true;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            return _piece.GetBehavior<T>();
        }

        public double MoveCur => _moveCur;
        public double MoveInc => Consts.GetDamagedValue(Piece, MoveIncBase, 1);//, true);
        public double MoveIncBase => _values.MoveInc;
        public int MoveMax => _values.MoveMax;
        public int MoveLimit => _values.MoveLimit;
        public bool Moved => _moved;

        void IMovable.Upgrade(IMovable.Values values)
        {
            double oldMove = MoveLimit;

            this._values = values;

            if (MoveLimit < oldMove)
                _moveCur = _moveCur * MoveLimit / oldMove;
        }

        bool IMovable.Move(Tile to)
        {
            bool move = (Piece.IsPlayer && to != null && to.Piece == null && to.Visible);
            return Move(move, to);
        }
        bool IMovable.EnemyMove(Tile to)
        {
            bool move = (Piece.IsEnemy && to != null && to.Piece == null);
            return Move(move, to);
        }
        private bool Move(bool Move, Tile to)
        {
            if (Move && Piece.Tile != to)
            {
                //check blocks
                double dist = Piece.Tile.GetDistance(to);
                if (dist <= MoveCur)
                {
                    this._moved = true;
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
        private double IncMove(bool doEndTurn)
        {
            double moveInc = Consts.IncValueWithMaxLimit(MoveCur, MoveInc, Consts.MoveDev, MoveMax, MoveLimit, Consts.MoveLimitPow, doEndTurn);
            if (doEndTurn)
            {
                //this._moved = false;
                this._moveCur += moveInc;
            }
            return moveInc;
        }
        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk);
        }
        void IBehavior.StartTurn()
        {
            this._moved = false;
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(true, ref energyUpk);
        }
        private void EndTurn(bool doEndTurn, ref double energyUpk)
        {
            energyUpk += IncMove(doEndTurn) * Consts.EnergyPerMove;
        }
    }
}
