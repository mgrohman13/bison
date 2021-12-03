using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    public interface IBehavior
    {
        public Piece Piece { get; }

        internal void GetUpkeep(ref double energy, ref double mass);
        public void EndTurn();
    }
}
