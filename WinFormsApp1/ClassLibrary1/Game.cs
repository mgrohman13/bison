using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;

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

            Player.CreateCore();
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
            Constructor.NewConstructor(Map.GetTile(constructor.X, constructor.Y), true);

            for (int a = 0; a < 1; a++)
                Biomass.NewBiomass(StartTile());
            for (int b = 0; b < 2; b++)
                Artifact.NewArtifact(StartTile());
            for (int c = 0; c < 3; c++)
                Metal.NewMetal(StartTile());
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

        private Map.Tile StartTile()
        {
            Map.Tile tile;
            do
                tile = Map.GetTile(Game.Rand.RangeInt(Map.left, Map.right), Game.Rand.RangeInt(Map.down, Map.up));
            while (InvalidStartTile(tile));
            return tile;
        }
        internal bool InvalidStartTile(Map.Tile tile)
        {
            return (tile == null || tile.Piece != null || tile.Visible || tile.GetDistance(Player.Core.Tile) <= Player.Core.GetBehavior<IRepair>().Range);
        }

        public Research.Type? EndTurn()
        {
            Research.Type? researched = Player.EndTurn();

            double difficulty = (Turn + Consts.DifficultyTurns) / Consts.DifficultyTurns;
            difficulty = Math.Pow(difficulty, Consts.DifficultyPow);
            Enemy.PlayTurn(difficulty);

            _turn++;

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
