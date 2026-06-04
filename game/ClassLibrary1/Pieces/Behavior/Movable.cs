using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Point = MattUtil.Point;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Behavior
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Movable(Piece piece, IMovable.Values values, double moveCur) : IMovable
    {
        private readonly Piece _piece = piece;
        private IMovable.Values _values = values;

        private double _moveCur = moveCur;
        private bool _moved = true;

        public Piece Piece => _piece;

        public Movable(Piece piece, IMovable.Values values)
            : this(piece, values, 0)
        {
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
            _values = values;

            if (MoveCur > MoveLimit)
            {
                double costE = (MoveLimit - MoveCur) * Consts.EnergyPerMove;
                Piece.Side.AddResources(-costE, 0);
                _moveCur = MoveLimit;
            }

            _moved = true;
        }

        bool IMovable.Move(Tile to)
        {
            Tile from = Piece.Tile;
            //bool move = Piece.IsPlayer && to != null && to.Piece == null && Piece.Tile != to && CanMove;

            if (CanMoveTo(to) && to.Visible && Piece is PlayerPiece piece)
            {
                IEnumerable<Point> ps = Tile.GetLinePoints(from.Location, to.Location);

                bool stop = false;
                foreach (var p in ps)
                {
                    Tile tile = from.Map.GetTile(p);
                    stop |= from.Map.UpdateVision(p, piece.Vision);
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
                double dist = Piece.Tile.MoveDistTo(to);
                _moved = true;
                _moveCur -= dist;
                Piece.SetTile(to);
                return true;
            }
            return false;
        }
        public bool CanMoveTo(Tile to) =>
            Piece.Tile != to && CanMove && to != null && to.Piece == null && ((IMovable)this).DistTo(to) <= MoveCur;
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
            //base.StartTurn();
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
        double IBehavior.Die()
        {
            double treasure = MoveCur * Consts.EnergyPerMove;
            this._moveCur = 0;
            return treasure;
        }
    }
}
