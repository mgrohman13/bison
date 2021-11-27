using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1
{
    [Serializable]
    public class Game
    {
        public static readonly MTRandom Rand;
        static Game()
        {
            Rand = new MTRandom();
            Rand.StartTick();
        }

        public readonly Map Map;
        public readonly Player Player;
        public readonly Enemy Enemy;
        public Game()
        {
            this.Map = new Map(this);
            this.Player = new Player(this);
            this.Enemy = new Enemy(this);

            Core.NewCore(Player);
        }

        public void SaveGame(string filePath)
        {
            TBSUtil.SaveGame(this, filePath);
        }
        public static Game LoadGame(string filePath)
        {
            Game game = TBSUtil.LoadGame<Game>(filePath);
            game.OnDeserialization();
            return game;
        }
        private void OnDeserialization()
        {
            Map.OnDeserialization();
        }

        public void TestMove(int x, int y)
        {
            Map.Tile tile = Map.GetTile(x, y);
            if (tile != null)
                Player.Pieces.OfType<Core>().Single().SetTile(tile);
        }
    }
}
