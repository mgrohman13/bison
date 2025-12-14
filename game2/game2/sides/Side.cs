using game2.game;
using game2.pieces;
using game2.pieces.behavior;

namespace game2.sides
{
    [Serializable]
    public abstract class Side(Game game)
    {
        protected readonly Game _game = game;
        protected readonly List<Piece> _pieces = [];

        protected Resources _resources;

        public Game Game => _game;
        internal IReadOnlyList<Piece> Pieces => _pieces;
        public bool IsPlayer => this == Game.Player;

        internal IEnumerable<T?> PiecesOfType<T>() where T : class, IBehavior =>
             Pieces.Select(p => p is T t ? t : p.GetBehavior<T>()).Where(b => b != null);

        internal void AddPiece(Piece piece) => _pieces.Add(piece);
        internal void RemovePiece(Piece piece) => _pieces.Remove(piece);

        //internal abstract bool Spend(int energy, int mass);

        //protected void StartTurn()
        //{
        //    foreach (Piece piece in Game.Rand.Iterate(_pieces))
        //        piece.StartTurn();
        //}
        //protected void EndTurn(out float energyUpk, out float massUpk)
        //{
        //    energyUpk = massUpk = 0;
        //    foreach (Piece piece in Game.Rand.Iterate(_pieces))
        //        piece.EndTurn(ref energyUpk, ref massUpk);
        //}
    }
}
