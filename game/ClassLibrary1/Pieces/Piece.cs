using System;
using System.Collections.Generic;
using System.Linq;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public abstract class Piece : IBehavior
    {
        public readonly Game Game;
        public readonly Side _side;
        public readonly int PieceNum;

        Piece IBehavior.Piece => this;
        protected IReadOnlyCollection<IBehavior> behavior = Array.Empty<IBehavior>();

        private Tile _tile;

        public Side Side => _side;
        public Tile Tile => _tile;

        public bool IsPlayer => Side != null && Side == Game.Player;
        public bool IsEnemy => Side != null && Side == Game.Enemy;

        internal Piece(Side side, Tile tile)
        {
            this.Game = tile.Map.Game;
            this._side = side;
            this._tile = tile;
            this.PieceNum = Game.GetPieceNum(this.GetType());
        }
        public IEnumerable<T> GetBehaviors<T>() where T : class, IBehavior
        {
            IEnumerable<T> all = behavior.OfType<T>();
            if (all.Count() > 1 && !all.All(b => b.AllowMultiple))
                throw new Exception();
            return all;
        }
        public T GetBehavior<T>() where T : class, IBehavior
        {
            IEnumerable<T> all = GetBehaviors<T>();
            if (all.All(b => b.AllowMultiple))
                return all.FirstOrDefault();
            return all.SingleOrDefault();
        }
        public bool HasBehavior<T>(out T behavior) where T : class, IBehavior
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
        }

        internal virtual void Die()
        {
            Game.RemovePiece(this);
        }

        internal void SetTile(Tile tile)
        {
            if (this.Tile != null)
                Game.Map.RemovePiece(this);
            this._tile = tile;
            if (tile != null)
                Game.Map.AddPiece(this);
        }

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            GetUpkeep(ref energyUpk, ref massUpk);
        }
        internal virtual void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            foreach (IBehavior behavior in this.behavior)
                behavior.GetUpkeep(ref energyUpk, ref massUpk);
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            EndTurn(ref energyUpk, ref massUpk);
        }
        internal virtual void EndTurn(ref double energyUpk, ref double massUpk)
        {
            //bool moved = HasBehavior(out IMovable movable) && movable.Moved;
            //bool attacked = HasBehavior(out IAttacker attacker) && attacker.Attacked;
            foreach (IBehavior behavior in Game.Rand.Iterate(behavior))
                behavior.EndTurn(ref energyUpk, ref massUpk);
        }
    }
}
