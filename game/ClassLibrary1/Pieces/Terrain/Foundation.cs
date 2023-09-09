using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Foundation : Piece
    {
        internal Foundation(Map.Tile tile)
            : base(null, tile)
        {
        }
        internal static Foundation NewFoundation(Map.Tile tile)
        {
            Foundation obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        public override string ToString()
        {
            return "Foundation";
        }
    }
}
