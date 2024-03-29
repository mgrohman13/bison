using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using MattUtil;
using System;
using System.Collections.Generic;

namespace ClassLibrary1
{
    [Serializable]
    public class Game
    {
        public static readonly int? TEST_MAP_GEN = null;

        public static readonly MTRandom Rand;
        static Game()
        {
            Rand = new MTRandom();
            Rand.StartTick();
        }

        public readonly Map Map;
        public readonly Player Player;
        public readonly Enemy Enemy;
        public readonly Log Log;

        private int _turn;
        public int Turn => _turn;

        public readonly string SavePath;
        private bool _gameOver;
        public bool GameOver => _gameOver;

        private readonly Dictionary<string, int> _pieceNums;

        public Game(string savePath)
        {
            this.Map = new(this);
            this.Player = new(this);
            this.Enemy = new(this);
            this.Log = new(this);

            this._turn = 0;
            this.SavePath = savePath;
            this._gameOver = false;

            this._pieceNums = new Dictionary<string, int>();

            Point constructor = Game.Rand.SelectValue(new Point[] {
                new(-2, -1),
                new(-2,  1),
                new(-1, -2),
                new(-1,  2),
                new( 1, -2),
                new( 1,  2),
                new( 2, -1),
                new( 2,  1),
            });
            Player.CreateCore(constructor);
            Constructor.NewConstructor(Map.GetTile(Player.Core.Tile.X + constructor.X, Player.Core.Tile.Y + constructor.Y), true);
            Map.GenerateStartResources();
        }

        internal int GetPieceNum(Type type)
        {
            string key = type.FullName;
            _pieceNums.TryGetValue(key, out int num);
            _pieceNums[key] = ++num;
            return num;
        }

        internal void End()
        {
            _gameOver = true;
            System.IO.File.Delete(SavePath);
        }

        public Research.Type? EndTurn()
        {
            Research.Type? researched = Player.EndTurn();
            _turn++;
            Enemy.PlayTurn();
            return researched;
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
            piece.SetTile(null);

            if (piece is PlayerPiece playerPiece)
                Player.RemovePiece(playerPiece);
            else if (piece is EnemyPiece enemyPiece)
                Enemy.RemovePiece(enemyPiece);
        }

        public void SaveGame()
        {
            TBSUtil.SaveGame(this, SavePath);
        }
        public static Game LoadGame(string filePath)
        {
            Game game = TBSUtil.LoadGame<Game>(filePath);
            game.OnDeserialization();
            return game;
        }
        private void OnDeserialization()
        {
            this.ToString();
        }
    }
}
