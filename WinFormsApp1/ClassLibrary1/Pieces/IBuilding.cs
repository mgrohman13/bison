using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    public interface IBuilding
    {
        public void Build(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit);
        public void EndTurn();
    }
}
