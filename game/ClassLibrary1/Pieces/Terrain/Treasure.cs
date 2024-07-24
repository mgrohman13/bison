using System;
using System.Collections.Generic;
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
            Tile tile = Tile;
            this.Die();

            Dictionary<Func<Tile, int, int>, int> choices = new() { { CollectResources, 13 }, { NewResource, 1 } };
            var Func = Game.Rand.SelectValue(choices);

            int value = Func(tile, Game.Rand.GaussianOEInt((130 + Game.Turn) * 16.9, .26, .13, 650));
            Game.Enemy.AddEnergy(Game.Rand.Round(value / 2.1));
        }

        private int CollectResources(Tile tile, int value)
        {
            int v1 = Game.Rand.Bool() ? Game.Rand.RangeInt(value - 1, 1) : 0;
            int v2 = value - v1;
            if (Game.Rand.Bool())
                (v1, v2) = (v2, v1);
            Game.Player.AddResources(v1, v2);
            return value;
        }
        private int NewResource(Tile tile, int value)
        {
            Game.Map.GenResources(_ => tile, Game.Rand.DoubleHalf());
            return 0;
        }
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
        //    return Game.Rand.Bool() ? Resources(value) : 0;
        //}

        public override string ToString() => "Unknown Object";
    }
}
