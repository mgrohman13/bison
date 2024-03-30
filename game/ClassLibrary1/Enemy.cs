using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1
{
    [Serializable]
    public class Enemy : Side
    {
        private readonly EnemyResearch _research;
        private MechBlueprint _nextAlien;

        public IEnumerable<Piece> VisiblePieces => _pieces.Where(p => p.Tile.Visible);

        internal Enemy(Game game)
            : base(game, Game.Rand.Round(Consts.EnemyStartEnergy), 0)
        {
            this._research = new EnemyResearch(game);
            this._nextAlien = MechBlueprint.Alien(_research);
        }

        internal void PlayTurn()
        {
            double difficulty = (Game.Turn + Consts.DifficultyIncTurns) / Consts.DifficultyIncTurns;

            foreach (Piece piece in Game.Rand.Iterate(Pieces))
                PlayTurn(piece, Math.Pow(difficulty, Consts.DifficultyMoveDirPow));

            base.EndTurn(out double energyUpk, out double massUpk);

            double energy = Math.Pow(difficulty, Consts.DifficultyEnergyPow) * Consts.EnemyEnergy;
            if (Game.Turn < Consts.EnemyEnergyRampTurns)
                energy *= Game.Turn / Consts.EnemyEnergyRampTurns;
            this._energy += Game.Rand.OEInt(energy) + Game.Rand.Round((this.Mass - massUpk) * Consts.MechMassDiv - energyUpk);
            this._mass = 0;

            int spawns = Game.Rand.OEInt(Game.Turn / 13.0);
            for (int a = 0; a < spawns && NextAlienCost() + 13 < this.Energy; a++)
                SpawnAlien(Game.Map.GetEnemyTile());

            Debug.WriteLine($"Enemy energy: {_energy}");

            _research.EndTurn(Math.Pow(difficulty, Consts.DifficultyResearchPow));

        }

        internal void HiveDamaged(Hive hive, ref double energy, int hits, double hitsPct, double range)
        {
            hitsPct = 1 - hitsPct;
            int xfer = Game.Rand.Round(energy);
            if (hive.Dead)
                hitsPct = 1;
            else
                xfer = Game.Rand.GaussianInt(energy * hitsPct, 1);
            this._energy += xfer;
            energy -= xfer;

            if (this.Energy > 0 && Game.Rand.Bool(hitsPct / Math.Sqrt(hits)))
            {
                Tile tile;
                int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(Math.Sqrt(range) + Attack.MELEE_RANGE));
                do
                    tile = Game.Map.GetTile(RandCoord(hive.Tile.X), RandCoord(hive.Tile.Y));
                while (tile == null || tile.Piece != null);
                SpawnAlien(tile);
            }
        }
        private void SpawnAlien(Tile tile)
        {
            this._energy -= Game.Rand.Round(NextAlienCost());
            Alien.NewAlien(tile, _nextAlien.Killable, _nextAlien.Resilience, _nextAlien.Attacker, _nextAlien.Movable);
            _nextAlien = MechBlueprint.Alien(_research);
        }
        private double NextAlienCost() => _nextAlien.Energy + _nextAlien.Mass * Consts.MechMassDiv;

        private void PlayTurn(Piece piece, double difficulty)
        {
            IAttacker attacker = piece.GetBehavior<IAttacker>();

            IEnumerable<IKillable> allTargets = Enumerable.Empty<IKillable>();
            Dictionary<Attack, IEnumerable<IKillable>> targets = new();
            double avgHp = 1, avgWeight = 1;
            if (attacker != null)
            {
                allTargets = Game.Player.PiecesOfType<IKillable>();
                if (allTargets.Any())
                {
                    avgHp = allTargets.Average(k => k.Hits.DefenseMax);
                    avgWeight = (allTargets.Average(k => GetKillWeight(k, avgHp)));
                }
                targets = GetTargets(attacker, piece.Tile, allTargets);
            }

            if (piece.HasBehavior(out IMovable movable) && movable.MoveCur >= 1)
            {
                double d = piece.Tile.GetDistance(Game.Player.Core.Tile);
                double distMult = 1.3 * difficulty * Math.Pow((d + 52) / 52.0, Consts.DistanceMoveDirPow);

                double minDist = Math.Max(0, d - movable.MoveCur);
                double maxDist = d + movable.MoveCur;
                Dictionary<Tile, int> moveTiles = piece.Tile.GetTilesInRange(movable).Where(t => t.Piece == null || t.Piece == piece).ToDictionary(t => t, moveTile =>
                {
                    double result = distMult * (1 - (moveTile.GetDistance(Game.Player.Core.Tile) - minDist) / (maxDist - minDist));
                    result = 1 + result * result;
                    if (attacker != null)
                    {
                        double attackWeight = 1;
                        foreach (var attack in attacker.Attacks.Where(a => !a.Attacked))
                        {
                            var weights = allTargets.SelectMany(k => attack.GetDefenders(moveTile, k.Piece)).Select(k => GetKillWeight(k, avgHp));
                            if (weights.Any())
                            {
                                double weight = weights.Max();
                                attackWeight += 130 * attack.AttackCur * (.13 + Math.Pow(weight * weight * weights.Average(), 1 / 3.0) / avgWeight);
                            }
                        }
                        result *= attackWeight;
                    }
                    return Game.Rand.Round(Math.Pow(1 + 13 * result, .91));
                });

                Tile moveTo = Game.Rand.SelectValue(moveTiles);
                bool attackFirst = attacker != null && Game.Rand.Bool(.013);
                Dictionary<Attack, IEnumerable<IKillable>> newTargets = GetTargets(attacker, moveTo, allTargets);
                if (!attackFirst && attacker != null)
                    attackFirst = targets.Keys.Any(a => !newTargets.ContainsKey(a));
                if (attackFirst)
                    Fire(attacker, targets, avgHp, avgWeight);

                if (movable.EnemyMove(moveTo))
                    targets = newTargets;
                else if (piece.Tile != moveTo)
                    ;
            }

            if (attacker != null)
                Fire(attacker, targets, avgHp, avgWeight);
        }
        private static Dictionary<Attack, IEnumerable<IKillable>> GetTargets(IAttacker attacker, Tile from, IEnumerable<IKillable> allTargets)
        {
            return attacker.Attacks.Where(a => !a.Attacked)
                .Select(a => new Tuple<Attack, IEnumerable<IKillable>>(a, allTargets.SelectMany(k => a.GetDefenders(from, k.Piece))))
                .Where(t => t.Item2.Any())
                .ToDictionary(t => t.Item1, t => t.Item2);
        }
        private static void Fire(IAttacker attacker, Dictionary<Attack, IEnumerable<IKillable>> targets, double avgHp, double avgWeight)
        {
            Dictionary<IKillable, int> targWeights = targets.Values.SelectMany(v => v).Distinct().ToDictionary(k => k, k => Game.Rand.Round(1 + 13 * GetKillWeight(k, avgHp) / avgWeight));
            while (attacker.Attacks.Any(a => !a.Attacked && targets.ContainsKey(a)) && targWeights.Any())
            {
                IKillable trg = Game.Rand.SelectValue(targWeights);
                targWeights.Remove(trg);
                if (attacker.Attacks.Any(a => a.AttackCur == a.AttackMax || !trg.Piece.HasBehavior<IAttacker>() || a.AttackCur > Game.Rand.Next(trg.TotalDefenses.Max(d => d.DefenseCur))))
                    if (!attacker.EnemyFire(trg))
                        ;
            }
        }
        private static double GetKillWeight(IKillable killable, double avgHp)
        {
            double attacks = 0, repair = 0;

            if (killable.Piece.HasBehavior(out IAttacker attacker))
                attacks += attacker.Attacks.Sum(a => a.AttackCur * (a.Range + 7.8));
            if (killable.Piece.HasBehavior(out IRepair repairs))
                repair += 26 + avgHp * repairs.Rate * (repairs.Range + 3.9);
            if (killable.Piece.HasBehavior(out IBuilder builder))
                repair += 13 + avgHp * (builder.Range + 13) / 130.0;

            double defCur = killable.TotalDefenses.Sum(d => d.DefenseCur);
            double defMax = killable.TotalDefenses.Sum(d => d.DefenseMax);
            double gc = (1 + attacks + 21 * repair) / defCur;
            //double gc = (1 + attacks + 21 * repair) / (killable.HitsCur / (1 - killable.Armor) + killable.ShieldCur);
            double damagePct = 2 - defCur / (double)defMax;
            //double shieldFactor = 3 + 13 * killable.ShieldInc / (killable.HitsMax / 6.5 + killable.ShieldCur);
            double final = gc * gc * damagePct;// * shieldFactor;
            if (!(final > 0))
                ;
            return final;
        }
    }
}
