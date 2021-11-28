using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

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

            Core.NewCore(this);
        }

        public void EndTurn()
        {
            Player.EndTurn();

            if (Game.Rand.Bool(.13))
            {
                Core core = Player.Pieces.OfType<Core>().First();

                Map.Tile tile;
                do
                {
                    tile = Map.GetTile(core.Tile.X + Game.Rand.GaussianInt(3), core.Tile.Y + Game.Rand.GaussianInt(3));
                } while (tile == null || tile.Piece != null);

                core.Build(Player, tile, Game.Rand.GaussianOE(6.5, .13, .13, 1), GenKillablealues(), GenAttacks(), GenMovableValues());
            }

            Enemy.PlayTurn();

            if (Game.Rand.Bool(.169))
            {
                Map.Tile tile;
                do
                {
                    tile = Map.GetTile(Game.Rand.GaussianInt(26), Game.Rand.GaussianInt(26));
                } while (tile == null || tile.Piece != null);


                Alien.NewAlien(this, tile, GenKillablealues(), GenAttacks(), GenMovableValues());
            }
        }

        private static IKillable.Values GenKillablealues()
        {
            double hitsMax = Game.Rand.GaussianOE(13, .13, .13, 1);

            double armor = 0;

            double shieldInc = 0;
            double shieldMax = 0;
            double shieldLimit = 0;

            if (Game.Rand.Bool())
                armor = Game.Rand.Weighted(.9, .13);

            if (Game.Rand.Bool())
            {
                shieldInc = Game.Rand.GaussianOE(2.1, .13, .13);
                shieldMax = Game.Rand.GaussianOE(shieldInc * 13, .13, .13, shieldInc);
                shieldLimit = Game.Rand.GaussianOE(shieldInc * 13 + shieldMax, .13, .13, shieldMax);
            }

            return new(hitsMax, armor, shieldInc, shieldMax, shieldLimit);
        }
        private static List<IAttacker.Values> GenAttacks()
        {
            int num = Game.Rand.GaussianOEInt(1.69, .13, .13, 1);
            List<IAttacker.Values> attacks = new(num);
            for (int a = 0; a < num; a++)
            {
                double damage = Game.Rand.GaussianOE(1.3, .13, .13);
                double armorPierce = Game.Rand.Weighted(.9, .13);
                double shieldPierce = Game.Rand.Weighted(.9, .13);
                double dev = Game.Rand.Weighted(.9, .13);
                double range = Game.Rand.GaussianOE(2.6, .13, .13, 1);
                attacks.Add(new(damage, armorPierce, shieldPierce, dev, range));
            }
            return attacks;
        }
        private static IMovable.Values GenMovableValues()
        {
            double move = Game.Rand.GaussianOE(2.6, .13, .13, 1);
            double max = Game.Rand.GaussianOE(move * 2, .13, .13, move);
            double limit = Game.Rand.GaussianOE(max + move, .13, .13, max);
            return new(move, max, limit);
        }

        internal void AddPiece(Piece piece)
        {
            Map.AddPiece(piece);

            if (piece is PlayerPiece playerPiece)
                Player.AddPiece(playerPiece);
            else if (piece is EnemyPiece enemyPiece)
                Enemy.AddPiece(enemyPiece);
        }
        internal void RemovePiece(Piece piece)
        {
            Map.RemovePiece(piece);

            if (piece is PlayerPiece playerPiece)
                Player.RemovePiece(playerPiece);
            else if (piece is EnemyPiece enemyPiece)
                Enemy.RemovePiece(enemyPiece);
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
