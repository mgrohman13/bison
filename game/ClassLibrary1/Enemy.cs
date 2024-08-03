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
        private double _portalSpawn;

        public IEnumerable<Piece> VisiblePieces => _pieces.Where(p => p.Tile.Visible);

        public IEnumerable<Tuple<Tile, Tile>> LastAttacks => PiecesOfType<EnemyPiece>().SelectMany(a => a.LastAttacks);
        public IEnumerable<Tuple<Tile, Tile>> LastMoves => PiecesOfType<EnemyPiece>().Where(a => a.LastMove != null).Select(a => Tuple.Create(a.Tile, a.LastMove));

        internal Enemy(Game game)
            : base(game, Game.Rand.Round(Consts.EnemyStartEnergy), 0)
        {
            this._research = new EnemyResearch(game);
            this._nextAlien = MechBlueprint.Alien(_research);
            this._portalSpawn = 0;
        }

        internal void PlayTurn(Action<Tile, double> UpdateProgress)
        {
            double difficulty = GetDifficulty(Game);

            bool portal = false;
            if (this._research.TypeVailable(EnemyResearch.PortalType))
                portal = BuildPortals();

            EnemyMovement.PlayTurn(Game, Math.Pow(difficulty, Consts.DifficultyAIPow), portal, UpdateProgress);

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

        private bool BuildPortals()
        {
            int researchLevel = Game.Rand.Round((_research.GetBlueprintLevel() + Game.Player.Research.GetBlueprintLevel()) / 2.0);

            Game.Player.GetIncome(out double energyInc, out double massInc, out double researchInc);
            double pInc = energyInc + Consts.MechMassDiv * (massInc + researchInc * Consts.ResearchMassConversion);
            double pRes = Game.Player.Energy + Game.Player.Mass * Consts.MechMassDiv;
            double pStr = Game.Player.Pieces.Sum(p => p.Strength(researchLevel, false));

            double eInc = GetEneryIncome(Game);
            double eRes = this.Energy + this.Mass * Consts.MechMassDiv;
            double eStr = this.Pieces.Sum(p => p.Strength(researchLevel, false));

            pStr += pRes;
            eStr += eRes;

            pStr *= Consts.PortalSpawnStrMult;

            static double Inc(double e, double p) => 2 * e / (e + p) - 1;
            double inc = 0;
            bool str = eStr > pStr;
            if (str)
                inc += Inc(eStr, pStr);
            if (eInc > pInc)
            {
                inc += Inc(eInc, pInc);
                if (str)
                    inc *= inc + 1;
            }
            inc /= Consts.PortalSpawnTime;

            var portals = PiecesOfType<Portal>();
            bool hasEntrance = portals.Any(p => !p.Exit);
            bool hasExit = portals.Any(p => p.Exit);
            double count = portals.Sum(p => Consts.StatValue(p.GetBehavior<IKillable>().Hits.DefenseCur)) / Consts.PortalExitDef + 1;
            if (count > 2)
                count *= count - 1;
            inc /= count;

            this._portalSpawn += inc;

            bool portal = false;
            double needed = hasEntrance && !hasExit ? .5 : 1;
            if ((hasExit && !hasEntrance) || _portalSpawn > needed)
                if (hasExit)
                {
                    portal |= BuildPortal(false);
                }
                else
                {
                    portal |= BuildPortal(true);
                    if (!hasEntrance)
                        portal |= BuildPortal(false);
                }
            return portal;
        }
        private bool BuildPortal(bool exit)
        {
            Tile tile;
            if (exit)
            {
                //exits always place near core, avoiding immediate player attacks 
                Core core = Game.Player.Core;
                double deviation = core.GetBehavior<IRepair>().Range + Game.Rand.Range(Attack.MELEE_RANGE, Attack.MIN_RANGED);
                var avoid = EnemyMovement.GetPlayerAttacks(Game).Keys
                    .Concat(core.Tile.GetAllPointsInRange(deviation).Select(Game.Map.GetTile))
                    .Where(t => t is not null).ToHashSet();
                do
                {
                    deviation += Game.Rand.DoubleFull(Math.Sqrt(Consts.PathWidth));
                    tile = Game.Map.GetTile(Game.Rand.GaussianInt(deviation), Game.Rand.GaussianInt(deviation));
                }
                while (tile is null || tile.Piece is not null || avoid.Contains(tile));
            }
            else
            {
                //entrances chosen based on prioximity to aliens and distance from player pieces or resources
                static bool CanPlace(Tile t) => t.Piece is null;
                var pieces = Pieces.Where(p => p.HasBehavior<IMovable>() && p.Tile.GetAdjacentTiles().Any(CanPlace));
                if (pieces.Any())
                {
                    Dictionary<Piece, int> select = new();
                    foreach (var piece in Game.Rand.Iterate(pieces))
                    {
                        double mult = 3.9, div = 1;
                        foreach (var check in Game.Rand.Iterate(Game.AllPieces))
                            if (piece != check)
                            {
                                double factor = 2.1 * Consts.PathWidth / (Consts.CavePathSize + piece.Tile.GetDistance(check.Tile));
                                factor *= factor;
                                if (check.IsEnemy && check is not Portal)
                                    mult += factor;
                                else
                                    div += factor;
                            }
                        mult /= div;
                        mult *= mult;
                        select.Add(piece, Game.Rand.Round(mult + 1));
                    }
                    tile = Game.Rand.SelectValue(Game.Rand.SelectValue(select).Tile
                        .GetAdjacentTiles().Where(CanPlace));
                }
                else
                {
                    return false;
                }
            }

            Portal portal = Portal.NewPortal(tile, GetDifficulty(Game), exit, out double cost);
            Spend(Game.Rand.Round(cost), 0);
            this._portalSpawn--;
            return true;
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
