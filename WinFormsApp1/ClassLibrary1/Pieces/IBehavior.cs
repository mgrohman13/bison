using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    public interface IBehavior
    {
        public virtual bool AllowMultiple => false;

        public Piece Piece { get; }

        public T GetBehavior<T>() where T : class, IBehavior;
        public bool HasBehavior<T>(out T behavior) where T : class, IBehavior
        {
            return (behavior = GetBehavior<T>()) != null;
        }

        internal void GetUpkeep(ref double energy, ref double mass);
        public void EndTurn(ref double energyUpk, ref double massUpk);
    }
}
