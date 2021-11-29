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

        private int _turn;
        public int Turn => _turn;

        //temp
        private MechBlueprint _blueprint1;
        private MechBlueprint _blueprint2;
        public MechBlueprint Blueprint1 => _blueprint1;
        public MechBlueprint Blueprint2 => _blueprint2;

        public Game()
        {
            this.Map = new Map(this);
            this.Player = new Player(this);
            this.Enemy = new Enemy(this);

            this._turn = 0;

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
            Constructor.NewConstructor(Map.GetTile(constructor.X, constructor.Y), Player.GetResearchMult());

            for (int a = 0; a < 1; a++)
                Biomass.NewBiomass(StartTile());
            for (int b = 0; b < 2; b++)
                Artifact.NewArtifact(StartTile());
            for (int c = 0; c < 3; c++)
                Metal.NewMetal(StartTile());

            GenBlueprints();
        }

        private Map.Tile StartTile()
        {
            Map.Tile tile;
            do
                tile = Map.GetTile(Game.Rand.RangeInt(Map.left, Map.right), Game.Rand.RangeInt(Map.down, Map.up));
            while (tile == null || tile.Piece != null || tile.Visible);
            return tile;
        }

        public void EndTurn()
        {
            Player.EndTurn();

            Enemy.PlayTurn();

            double difficulty = (Turn + 52.0) / 52.0;
            double chance = (difficulty - .65) / 3.9;
            if (chance < 1)
                chance *= chance;
            if (chance > .5)
                chance /= (chance + .5);
            while (Game.Rand.Bool(chance))
                Alien.NewAlien(Map.GetEnemyTile(), GenKillable(difficulty), GenAttacker(difficulty), GenMovable(difficulty));

            _turn++;
        }
        internal void GenBlueprints()
        {
            this._blueprint1 = GenBlueprint();
            this._blueprint2 = GenBlueprint();

            Mech.Cost(out double e1, out double m1, Blueprint1, Player.GetResearchMult());
            Mech.Cost(out double e2, out double m2, Blueprint2, Player.GetResearchMult());
            if ((e1 > m1) == (e2 > m2))
                GenBlueprints();
        }
        private MechBlueprint GenBlueprint()
        {
            double difficulty = 1 + Player.GetResearchMult();
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
                armor = Game.Rand.Weighted(.9, .13);

            if (Game.Rand.Bool())
            {
                shieldInc = Game.Rand.GaussianOE(1.3, .26, .26);
                shieldMax = Game.Rand.GaussianOE(shieldInc * 13, .39, .26, shieldInc);
                shieldLimit = Game.Rand.GaussianOE(shieldInc * 13 + shieldMax, .39, .39, shieldMax);
            }

            difficulty = Math.Pow(difficulty, .6);
            hitsMax *= difficulty;
            if (armor > 0)
                armor = 1 - (1 - armor) / difficulty;
            shieldInc *= difficulty;
            shieldMax *= difficulty;
            shieldLimit *= difficulty;

            return new(hitsMax, armor, shieldInc, shieldMax, shieldLimit);
        }
        private static List<IAttacker.Values> GenAttacker(double difficulty)
        {
            int num = Game.Rand.GaussianOEInt(1.69, .26, .13, 1);
            List<IAttacker.Values> attacks = new(num);
            for (int a = 0; a < num; a++)
            {
                double damage = Game.Rand.GaussianOE(3.9, .26, .26);
                double armorPierce = Game.Rand.Bool() ? Game.Rand.Weighted(.9, .13) : 0;
                double shieldPierce = Game.Rand.Bool() ? Game.Rand.Weighted(.9, .13) : 0;
                double dev = Game.Rand.Weighted(.9, .13);
                double range = Game.Rand.GaussianOE(5.2, .39, .26, 1);

                difficulty = Math.Pow(difficulty, .4);
                damage *= difficulty;
                if (armorPierce > 0)
                    armorPierce = 1 - (1 - armorPierce) / difficulty;
                if (shieldPierce > 0)
                    shieldPierce = 1 - (1 - shieldPierce) / difficulty;
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

            difficulty = Math.Pow(difficulty, .5);
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
    }
}
