using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1
{
    [Serializable]
    public class Enemy : Side
    {
        private readonly EnemyResearch _research;
        private MechBlueprint _nextAlien;
        private MechBlueprint NextAlien => _nextAlien;

        public IEnumerable<Piece> VisiblePieces => _pieces.Where(p => p.Tile.Visible);

        public IEnumerable<Tuple<Tile, Tile>> LastAttacks => PiecesOfType<EnemyPiece>().SelectMany(a => a.LastAttacks);
        public IEnumerable<Tuple<Tile, Tile>> LastMoves => PiecesOfType<EnemyPiece>().Where(a => a.LastMove != null).Select(a => Tuple.Create(a.Tile, a.LastMove));

        internal Enemy(Game game)
            : base(game, Game.Rand.Round(Consts.EnemyStartEnergy), 0)
        {
            this._research = new EnemyResearch(game);
            this._nextAlien = MechBlueprint.Alien(_research);
        }

        internal void PlayTurn(Action<Tile, double> UpdateProgress)
        {
            double difficulty = GetDifficulty(Game);

            EnemyMovement.PlayTurn(Game, Math.Pow(difficulty, Consts.DifficultyAIPow), UpdateProgress);

            base.EndTurn(out double energyUpk, out double massUpk);
            double energy = GetEneryIncome(Game);
            if (Game.Turn < Consts.EnemyEnergyRampTurns)
                energy *= Game.Turn / Consts.EnemyEnergyRampTurns;
            AddEnergy(Game.Rand.OEInt(energy) + Game.Rand.Round((this.Mass - massUpk) * Consts.MechMassDiv - energyUpk));
            this._mass = 0;

            int spawns = Game.Rand.OEInt(Game.Turn / 13.0);
            for (int a = 0; a < spawns && NextAlien.AlienCost() + 13 < this.Energy; a++)
                SpawnAlien(() => Game.Map.GetEnemyTile(Alien.GetPathFindingMovement(NextAlien.Movable)));

            Debug.WriteLine($"Enemy energy: {_energy}");

            _research.EndTurn(Math.Pow(difficulty, Consts.DifficultyResearchPow));

            //we start turn here so the player sees things in the correct state for the enemy's next moves
            base.StartTurn();
        }
        internal static double GetDifficulty(Game game) =>
            (game.Turn + Consts.DifficultyIncTurns) / Consts.DifficultyIncTurns;
        internal static double GetEneryIncome(Game game) =>
            Math.Pow(GetDifficulty(game), Consts.DifficultyEnergyPow) * Consts.EnemyEnergy;

        internal void HiveDamaged(Hive hive, Tile defTile, Map.Map.SpawnChance spawn, ref double energy,
            int hits, double hitsPct, double dev)
        {
            hitsPct = 1 - hitsPct;
            int xfer = Game.Rand.Round(energy);
            if (hive.Dead)
            {
                hitsPct = 1;
            }
            else
            {
                xfer = Game.Rand.GaussianInt(energy * hitsPct, 1);
                hitsPct /= Math.Sqrt(hits);
            }
            AddEnergy(xfer);
            energy -= xfer;
            Debug.WriteLine($"Enemy energy: {_energy} ({(xfer > 0 ? "+" : "")}{xfer})");

            if (this.Energy > 0 && Game.Rand.Bool(hitsPct / 2.0))
            {
                SpawnAlien(() =>
                {
                    Tile tile;
                    int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(dev)); //push up to caller
                    do
                        tile = Game.Map.GetTile(RandCoord(defTile.X), RandCoord(defTile.Y));
                    while (tile == null || tile.Piece != null);

                    while (Alien.GetPathFindingMovement(NextAlien.Movable) < Game.Map.GetMinSpawnMove(tile))
                        this._nextAlien = MechBlueprint.Alien(_research);

                    return tile;
                });
                spawn.Spawned();
            }
            else
            {
                spawn.Mult(1 + hitsPct);
            }
        }
        internal void AddEnergy(int energy) => this._energy += energy;

        internal double SpawnAlien(Func<Tile> GetTile, double? value = null)
        {
            void GenAlien()
            {
                IResearch research = _research;
                if (value.HasValue)
                {
                    int min = Game.Rand.Round(value.Value / 2.1);
                    int max = Game.Rand.Round(value.Value * 1.3);
                    research = new ResearchMinMaxCost(research, min, max);
                }
                this._nextAlien = MechBlueprint.Alien(research);
            };

            if (value.HasValue)
                GenAlien();

            Tile tile;
            List<Point> path;
            while (true)
            {
                tile = GetTile();
                path = tile.Map.PathFindCore(tile, Alien.GetPathFindingMovement(NextAlien.Movable), blocked => !blocked.Any());
                if (path == null)
                    GenAlien();
                else
                    break;
            }

            double energy = NextAlien.AlienCost();
            this._energy -= Game.Rand.Round(energy);
            Alien.NewAlien(tile, path, energy, NextAlien.Killable, NextAlien.Resilience, NextAlien.Attacker, NextAlien.Movable);
            GenAlien();

            return energy;
        }
        internal override bool Spend(int energy, int mass)
        {
            this._energy = Game.Rand.Round(this.Energy - energy - mass * Consts.MechMassDiv);
            return true;
        }
    }
}
