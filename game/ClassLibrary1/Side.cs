using ClassLibrary1.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1
{
    [Serializable]
    public abstract class Side
    {
        protected readonly Game _game;
        protected readonly List<Piece> _pieces;

        protected int _energy, _mass;

        internal int Energy => _energy;
        internal int Mass => _mass;

        public Game Game => _game;
        internal IReadOnlyList<Piece> Pieces => _pieces;
        public bool IsPlayer => this == Game.Player;
        public bool IsEnemy => this == Game.Enemy;

        protected Side(Game game, int energy, int mass)
        {
            this._game = game;
            this._pieces = new List<Piece>();
            this._energy = energy;
            this._mass = mass;
        }

        internal IEnumerable<T> PiecesOfType<T>() where T : class, IBehavior
        {
            return Pieces.Select(p => p is T t ? t : p.GetBehavior<T>()).Where(b => b != null);
        }

        internal void AddPiece(Piece piece)
        {
            this._pieces.Add(piece);
        }
        internal void RemovePiece(Piece piece)
        {
            this._pieces.Remove(piece);
        }

        internal abstract bool Spend(int energy, int mass);

        protected void StartTurn()
        {
            foreach (Piece piece in Game.Rand.Iterate(_pieces))
                piece.StartTurn();
        }
        protected void EndTurn(out double energyUpk, out double massUpk)
        {
            energyUpk = massUpk = 0;
            foreach (Piece piece in Game.Rand.Iterate(_pieces))
                piece.EndTurn(ref energyUpk, ref massUpk);
        }
    }
}
