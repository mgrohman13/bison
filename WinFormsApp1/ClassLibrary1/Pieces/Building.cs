using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Building : IBuilding
    {
        private IPiece piece;
        public void Build(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit)
        {
            Mech.NewMech(player, tile, vision, moveInc, moveMax, moveLimit);
        }

        public void EndTurn()
        {
        }
    }
}
