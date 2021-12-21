﻿using ClassLibrary1.Pieces;
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

        //temp
        private MechBlueprint _blueprint1;
        private MechBlueprint _blueprint2;
        public MechBlueprint Blueprint1 => _blueprint1;
        public MechBlueprint Blueprint2 => _blueprint2;

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

            GenBlueprints();
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
            return (tile == null | tile.Piece != null || tile.Visible || tile.GetDistance(Player.Core.Tile) <= Player.Core.GetBehavior<IRepair>().Range);
        }

        public void EndTurn()
        {
            Player.EndTurn();

            double difficulty = (Turn + Consts.DifficultyTurns) / Consts.DifficultyTurns;
            difficulty = Math.Pow(difficulty, Consts.DifficultyPow);
            Enemy.PlayTurn(difficulty, () => GenBlueprint(difficulty));

            _turn++;
        }
        internal void GenBlueprints()
        {
            double difficulty = 1 + ((Player.Research.ResearchCur + Consts.ResearchFactor) / Consts.ResearchFactor);
            this._blueprint1 = GenBlueprint(difficulty);
            this._blueprint2 = GenBlueprint(difficulty);

            Mech.Cost(this, out double e1, out double m1, Blueprint1);
            Mech.Cost(this, out double e2, out double m2, Blueprint2);
            if ((e1 > m1) == (e2 > m2))
                GenBlueprints();
        }
        private static MechBlueprint GenBlueprint(double difficulty)
        {
            double vision = Game.Rand.GaussianOE(6.5, .39, .26, 1);
            vision *= Math.Pow(difficulty, .3);
            return new(vision, GenKillable(difficulty), GenAttacker(difficulty), GenMovable(difficulty));
        }
        private static IKillable.Values GenKillable(double difficulty)
        {
            double hitsMax = Game.Rand.GaussianOE(39, .26, .13, 1);

            double armor = 0;

            double shieldInc = 0;
            double shieldMax = 0;
            double shieldLimit = 0;

            if (Game.Rand.Bool())
            {
                shieldInc = Game.Rand.GaussianOE(1.3, .26, .26);
                shieldMax = Game.Rand.GaussianOE(shieldInc * 13, .39, .26, shieldInc);
                shieldLimit = Game.Rand.GaussianOE(shieldInc * 13 + shieldMax, .39, .39, shieldMax);
            }

            difficulty = Math.Pow(difficulty, .6);
            hitsMax *= difficulty;
            if (Game.Rand.Bool())
                armor = Game.Rand.Weighted(.95, 1 - Math.Pow(.78, difficulty));
            shieldInc *= difficulty;
            shieldMax *= difficulty;
            shieldLimit *= difficulty;

            return new(hitsMax, Consts.MechResilience, armor, shieldInc, shieldMax, shieldLimit);
        }
        private static List<IAttacker.Values> GenAttacker(double difficulty)
        {
            int num = Game.Rand.GaussianOEInt(1.69, .26, .13, 1);
            List<IAttacker.Values> attacks = new(num);
            for (int a = 0; a < num; a++)
            {
                double damage = Game.Rand.GaussianOE(6.5, .26, .26);
                double armorPierce = 0;
                double shieldPierce = 0;
                double dev = Game.Rand.Weighted(.13);
                double range = Game.Rand.GaussianOE(5.2, .39, .26, 1);

                difficulty = Math.Pow(difficulty, .5);
                damage *= difficulty;
                if (Game.Rand.Bool())
                    armorPierce = Game.Rand.Weighted(.95, 1 - Math.Pow(.65, difficulty));
                if (Game.Rand.Bool())
                    shieldPierce = Game.Rand.Weighted(.95, 1 - Math.Pow(.65, difficulty));
                range *= difficulty;

                attacks.Add(new(damage, armorPierce, shieldPierce, dev, range));
            }
            return attacks;
        }
        private static IMovable.Values GenMovable(double difficulty)
        {
            double move = Game.Rand.GaussianOE(2.6, .13, .26, 1);
            double max = Game.Rand.GaussianOE(move * 2, .39, .26, move);
            double limit = Game.Rand.GaussianOE(max + move, .39, .39, max);

            difficulty = Math.Pow(difficulty, .4);
            move *= difficulty;
            max *= difficulty;
            limit *= difficulty;

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
