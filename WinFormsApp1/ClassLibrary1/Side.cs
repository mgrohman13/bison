using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1
{
    [Serializable]
    public abstract class Side
    {
        protected readonly Game _game;
        protected readonly List<Piece> _pieces;

        protected double _energy, _mass;

        internal double Energy => _energy;
        internal double Mass => _mass;

        public Game Game => _game;

        protected Side(Game game, double energy, double mass)
        {
            this._game = game;
            this._pieces = new List<Piece>();
            this._energy = energy;
            this._mass = mass;
        }

        internal void AddPiece(Piece piece)
        {
            this._pieces.Add(piece);
        }
        internal void RemovePiece(Piece piece)
        {
            this._pieces.Remove(piece);
        }

        internal virtual void EndTurn()
        {
            foreach (Piece piece in Game.Rand.Iterate(_pieces))
                piece.EndTurn();
        }
    }
}
