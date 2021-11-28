using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    public interface IBehavior
    {
        internal double GetUpkeep();
        public void EndTurn();
    }
}
