﻿using ClassLibrary1.Pieces.Players;
using System;
using System.Linq;
using Tile = ClassLibrary1.Map.Map.Tile;

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
            Tile from = Piece.Tile;
            bool move = Piece.IsPlayer && to != null && to.Piece == null && to.Visible && Piece.Tile != to && CanMove;

            if (move && Piece is PlayerPiece piece)
            {
                const double lineDist = .5;
                foreach (var p in Game.Rand.Iterate(Math.Min(from.X, to.X), Math.Max(from.X, to.X), Math.Min(from.Y, to.Y), Math.Max(from.Y, to.Y))
                    .OrderBy(from.GetDistance))
                {
                    bool isX = from.X == to.X;
                    Tile tile = to.Map.GetTile(p);
                    if (tile != null && tile.Piece == null)
                    {
                        double a = isX ? 0 : (to.Y - from.Y) / (to.X - from.X);
                        double c = to.Y - a * to.X;
                        double b = -1;
                        if (isX || Map.Map.PointLineDistanceAbs(a, b, c, p) < lineDist)
                            if (to.Map.UpdateVision(tile, piece.Vision))
                            {
                                to = tile;
                                break;
                            }
                    }
                }

                move &= Move(to);
            }
            return move;
        }
        bool IMovable.EnemyMove(Tile to)
        {
            bool move = Piece.IsEnemy && to != null && to.Piece == null;
            return move && Move(to);
        }
        private bool Move(Tile to)
        {
            if (Piece.Tile != to && CanMove)
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
        public bool CanMove => !(_moved && Piece.HasBehavior(out IAttacker attacker) && attacker.RestrictMove);

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
