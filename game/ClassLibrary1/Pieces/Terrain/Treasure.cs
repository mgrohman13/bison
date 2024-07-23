using System;
using static ClassLibrary1.Map.Map;

namespace ClassLibrary1.Pieces.Terrain
{
    [Serializable]
    public class Treasure : Piece
    {
        public Treasure(Tile tile) : base(null, tile) { }

        internal static Treasure NewTreasure(Tile tile)
        {
            Treasure obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        internal static void Collect(Tile tile)
        {
            foreach (var n in Game.Rand.Iterate(tile.GetAdjacentTiles()))
                if (n.Piece is Treasure t)
                    t.Collect();
        }
        private void Collect()
        {
            //Tile tile = Tile;
            //this.Die();

            //int value = Game.Rand.GaussianOEInt((130 + Game.Turn) * 13, .26, .13, 650);

            //switch (Game.Rand.Next(13))
            //{
            //    case 0:
            //    case 1:
            //    case 2:
            //    case 3:
            //    case 4:
            //    case 5:
            //    case 6:
            //        value = Resources(value);
            //        break;
            //    case 7:
            //    case 8:
            //    case 9:
            //        value = Mech(value);
            //        break;
            //    case 10:
            //    case 11:
            //        value = Alien(value);
            //        break;
            //    case 12:
            //        value = Resource(value);
            //        break;
            //    default: throw new Exception();
            //}

            //Game.Enemy.AddEnergy(Game.Rand.Round(value / 1.69));
        }

        //private int Resources(Tile tile, int value)
        //{
        //    Game.Player.Collect(value);
        //    return value;
        //}
        //private int Mech(Tile tile, int value)
        //{
        //    Pieces.Players.Mech.NewMech();
        //    return value;
        //}
        //private int Blueprint(Tile tile, int value)
        //{
        //    Game.Player.Blueprint();
        //    return value;
        //}
        //private int Alien(Tile tile, int value)
        //{
        //    Pieces.Enemies.Alien.NewAlien();
        //    value = Game.Enemy.Alien(tile, value);
        //    return Resources(value);
        //}
        //private int Resource(Tile tile, int value)
        //{
        //    Game.Map.GenResource(tile);
        //    return 0;
        //}

        public override string ToString() => "Unknown Object";
    }
}
