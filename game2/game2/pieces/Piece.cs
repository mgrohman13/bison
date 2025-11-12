using game2.game;
using game2.map;
using game2.pieces.behavior;
using game2.sides;
using System.Diagnostics.CodeAnalysis;

namespace game2.pieces
{
    public abstract class Piece
    {
        public Game Game => Tile.Map.Game;
        public readonly Side? Side;
        protected IReadOnlyList<IBehavior> behavior = [];
        private Tile _tile;
        public Tile Tile => _tile;

        internal Piece(Side? side, Tile tile)
        {
            Side = side;
            _tile = tile;
        }
        public IEnumerable<T> GetBehaviors<T>() where T : class, IBehavior
        {
            IEnumerable<T> all = behavior.OfType<T>();
            if (all.Count() > 1 && !all.All(b => b.AllowMultiple))
                throw new Exception();
            return all;
        }
        public T? GetBehavior<T>() where T : class, IBehavior
        {
            IEnumerable<T> all = GetBehaviors<T>();
            if (all.All(b => b.AllowMultiple))
                return all.FirstOrDefault();
            return all.SingleOrDefault();
        }
        public bool HasBehavior<T>([NotNullWhen(true)] out T? behavior) where T : class, IBehavior
        {
            return (behavior = GetBehavior<T>()) != null;
        }
        public bool HasBehavior<T>() where T : class, IBehavior
        {
            return GetBehaviors<T>().Any();
        }
        protected void SetBehavior(params IBehavior[] behavior)
        {
            if (this.behavior.Any(b => behavior.Any(b2 => b.GetType() == b2.GetType())))
                throw new Exception();
            this.behavior = this.behavior.Concat(behavior).ToList().AsReadOnly();

            //if (HasBehavior(out IKillable killable) && killable.Hits == null)
            //    throw new Exception();
            //if (HasBehavior(out IAttacker attacker) && !attacker.Attacks.Any())
            //    throw new Exception();
        }

        internal void SetTile(Tile tile)
        {
            if (this.Tile != null)
                Game.Map.RemovePiece(this);
            this._tile = tile;
            if (tile != null)
                Game.Map.AddPiece(this);
        }

        internal virtual void Wound(float woundPct)
        {
            foreach (var behavior in Game.Rand.Iterate(behavior))
                behavior.Wound(woundPct);
        }
        internal virtual void Die() =>
            Game.RemovePiece(this);

        public virtual Resources GetTurnEnd()
        {
            Resources r = new();
            foreach (IBehavior b in Game.Rand.Iterate(behavior))
                r += b.GetTurnEnd();
            return r;
        }
        internal virtual void EndTurn(ref Resources resources)
        {
            foreach (IBehavior b in Game.Rand.Iterate(behavior))
                b.EndTurn(ref resources);
        }

        internal virtual void StartTurn()
        {
            foreach (IBehavior b in Game.Rand.Iterate(behavior))
                b.StartTurn();
        }
    }
}
