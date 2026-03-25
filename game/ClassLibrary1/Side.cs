using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ClassLibrary1
{
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class Side(Game game, int energy, int mass)
    {
        protected readonly Game _game = game;
        protected readonly List<Piece> _pieces = [];

        protected int _energy = energy, _mass = mass;

        internal int Energy => _energy;
        internal int Mass => _mass;

        public Game Game => _game;
        internal IReadOnlyList<Piece> Pieces => _pieces;
        public bool IsPlayer => this == Game.Player;
        public bool IsEnemy => this == Game.Enemy;

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
