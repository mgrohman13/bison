using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static ClassLibrary1.Map.Map;

namespace ClassLibrary1
{
    [Serializable]
    public class Game
    {
        public const int POINTS_TO_WIN = 3;
        public static readonly int? TEST_MAP_GEN;

        public static readonly MTRandom Rand;
        static Game()
        {
            Rand = new MTRandom();
            Rand.StartTick();
            //TEST_MAP_GEN = Rand.GaussianCappedInt(1.3 * Consts.CaveDistance * Math.Sqrt(2), .13);
        }

        public readonly ClassLibrary1.Map.Map Map;
        public readonly Player Player;
        public readonly Enemy Enemy;
        public readonly Log Log;

        private int _turn, _victory;
        public int Turn => _turn;
        public int Victory => _victory;

        public readonly string SavePath;
        private bool _gameOver, _win;
        public bool GameOver => _gameOver;
        public bool Win => _win;
        internal IEnumerable<Piece> AllPieces => Map.AllPieces;

        private readonly Dictionary<string, int> _pieceNums;

        public Game(string savePath)
        {
            this.Map = new(this);
            this.Player = new(this);
            this.Enemy = new(this);
            this.Log = new(this);

            this._turn = 0;
            this._victory = 0;
            this.SavePath = savePath;
            this._gameOver = false;
            this._win = false;

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

            Player.NewGame(constructor);
            Constructor.NewConstructor(Map.GetTile(Player.Core.Tile.X + constructor.X, Player.Core.Tile.Y + constructor.Y), true);

            Enemy.NewGame();

            Map.NewGame();

            Player.Research.NewGame();
        }

        internal int GetPieceNum(Type type)
        {
            string key = type.FullName;
            _pieceNums.TryGetValue(key, out int num);
            _pieceNums[key] = ++num;
            return num;
        }

        internal void CollectResources(Tile tile, double value, out int energy, out int mass)
        {
            const double massWeight = Consts.EnergyMassRatio / (1 + Consts.EnergyMassRatio);
            double massMax = value / Consts.EnergyMassRatio;
            double massAvg;
            if (Game.Rand.Bool())
                massAvg = Game.Rand.Weighted(massMax, massWeight);
            else
                massAvg = Game.Rand.Bool(massWeight) ? massMax : 0;

            mass = Consts.IncomeRounding(massAvg);
            energy = Consts.IncomeRounding(value - mass * Consts.EnergyMassRatio);

            Treasure.RaiseCollectEvent(tile, energy, mass);
            Player.AddResources(energy, mass);
        }
        internal void VictoryPoint()
        {
            if (++_victory >= POINTS_TO_WIN)
                End(true);
            else
                Enemy.VictoryPoint();
        }
        internal void End(bool win = false)
        {
            this._gameOver = true;
            this._win = win;

            this.Map.GameOver();
            SaveGame();
        }

        public Research.Type? EndTurn(Action<Tile, double> UpdateProgress)
        {
            if (this.GameOver)
                return null;

            Player.GenerateResources(out double energyInc, out double massInc, out double researchInc);
            double playerIncome = energyInc + Consts.EnergyMassRatio * (massInc + researchInc * Consts.ResearchMassConversion);

            Research.Type? researched = Player.EndTurn();
            _turn++;
            Map.PlayTurn(Turn);
            Enemy.PlayTurn(UpdateProgress, playerIncome);
            Player.StartTurn();

            SaveGame();
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

        private object additionalData;
        public void SaveGame(object data)
        {
            this.additionalData = data;
            SaveGame();
        }
        internal void SaveGame()
        {
            Debug.WriteLine("SaveGame");
            TBSUtil.SaveGame(this, SavePath);
        }
        public static Game LoadGame<T>(string filePath, out T data) //where T : ISerializable
        {
            Game game = TBSUtil.LoadGame<Game>(filePath);
            game.OnDeserialization();
            data = (T)game.additionalData;
            return game;
        }
        private void OnDeserialization()
        {
            //base.OnDeserialization(sender);
            this.ToString();
        }
    }
}
