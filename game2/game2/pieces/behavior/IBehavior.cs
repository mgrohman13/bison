using game2.game;
using System.Diagnostics.CodeAnalysis;

namespace game2.pieces.behavior
{
    public interface IBehavior
    {
        public virtual bool AllowMultiple => false;

        public Piece Piece { get; }
        public Game Game => Piece.Game;

        public bool HasBehavior<T>() where T : class, IBehavior => HasBehavior<T>(out _);
        public bool HasBehavior<T>([NotNullWhen(true)] out T? behavior) where T : class, IBehavior => (behavior = GetBehavior<T>()) != null;
        public T? GetBehavior<T>() where T : class, IBehavior => Piece.GetBehavior<T>();

        internal void Wound(float woundPct);

        //internal void GetUpkeep(ref float energy, ref float mass);
        //public void StartTurn();
        //public void EndTurn(ref float energyUpk, ref float massUpk);

        public Resources GetTurnEnd();
        internal void EndTurn(ref Resources resources);
        internal void StartTurn();
    }
}
