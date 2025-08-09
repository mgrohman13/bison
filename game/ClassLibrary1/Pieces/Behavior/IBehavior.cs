namespace ClassLibrary1.Pieces.Behavior
{
    public interface IBehavior
    {
        public virtual bool AllowMultiple => false;

        public Piece Piece { get; }

        public T GetBehavior<T>() where T : class, IBehavior;
        public bool HasBehavior<T>() where T : class, IBehavior => HasBehavior<T>(out _);
        public bool HasBehavior<T>(out T behavior) where T : class, IBehavior => (behavior = GetBehavior<T>()) != null;

        internal void GetUpkeep(ref double energy, ref double mass);
        public void StartTurn();
        public void EndTurn(ref double energyUpk, ref double massUpk);
    }
}
