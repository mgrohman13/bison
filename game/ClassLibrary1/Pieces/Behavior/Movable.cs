using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using System;
using System.Linq;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Behavior
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
            _piece = piece;
            _values = values;

            _moveCur = moveCur;
            _moved = true;
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

            _values = values;

            if (MoveLimit < oldMove)
                _moveCur = _moveCur * MoveLimit / oldMove;
        }

        bool IMovable.Move(Tile to)
        {
            Tile from = Piece.Tile;
            //bool move = Piece.IsPlayer && to != null && to.Piece == null && Piece.Tile != to && CanMove;

            if (CanMoveTo(to) && to.Visible && Piece is PlayerPiece piece)
            {
                const double lineDist = .5;
                bool isX = from.X == to.X;
                double a = isX ? 0 : (to.Y - from.Y) / (double)(to.X - from.X);
                double c = to.Y - a * to.X;
                double b = -1;

                IOrderedEnumerable<MattUtil.Point> ps =
                    Game.Rand.Iterate(Math.Min(from.X, to.X), Math.Max(from.X, to.X), Math.Min(from.Y, to.Y), Math.Max(from.Y, to.Y))
                    .Where(p => isX || Map.Map.PointLineDistanceAbs(a, b, c, p) < lineDist)
                    .OrderBy(from.GetDistance);

                bool stop = false;
                foreach (var p in ps)
                {
                    stop |= from.Map.UpdateVision(p, piece.Vision);
                    Tile tile = from.Map.GetTile(p);
                    if (stop && tile != null && tile.Piece == null)
                    {
                        to = tile;
                        break;
                    }
                }

                if (!Move(to))
                    throw new Exception();
                return true;
            }
            return false;
        }
        bool IMovable.EnemyMove(Tile to) => Piece.IsEnemy && Move(to);
        private bool Move(Tile to)
        {
            if (CanMoveTo(to))
            {
                double dist = Piece.Tile.GetDistance(to);
                _moved = true;
                _moveCur -= dist;
                Piece.SetTile(to);
                return true;
            }
            return false;
        }
        public bool CanMoveTo(Tile to)
        {
            if (Piece.Tile != to && CanMove && to != null && to.Piece == null)
            {
                //check blocks
                double dist = Piece.Tile.GetDistance(to);
                if (dist <= MoveCur)
                    return true;
            }
            return false;
        }
        public bool CanMove => !(_moved && Piece.HasBehavior(out IAttacker attacker) && attacker.RestrictMove);

        bool IMovable.Port(Portal portal)
        {
            if (portal.CanPort(this, out Portal exit, out double dist))
            {
                _moveCur -= dist;
                _moved = true;
                if (Piece.HasBehavior(out IAttacker attacker))
                    attacker.Attacked = true;
                // Piece.DrainMove();

                Piece.SetTile(exit.GetOutTile());
                return true;
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
                _moveCur += moveInc;
            }
            return moveInc;
        }
        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            EndTurn(false, ref energyUpk);
        }
        void IBehavior.StartTurn()
        {
            _moved = false;
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
