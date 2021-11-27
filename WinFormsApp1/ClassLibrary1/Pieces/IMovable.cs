using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    public interface IMovable
    {
        public double MoveCur { get; }
        public double MoveInc { get; }
        public double MoveMax { get; }
        public double MoveLimit { get; }

        public bool Move(Map.Tile to);
        public void EndTurn();
    }
}
