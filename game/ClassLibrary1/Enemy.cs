using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
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
            : base(game, Game.Rand.Round(Consts.EnemyEnergy * -Consts.EnemySkipStart), 0)
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

            this._energy += Game.Rand.OEInt(Math.Pow(difficulty, Consts.DifficultyEnergyPow) * Consts.EnemyEnergy) + this.Mass - Game.Rand.Round(energyUpk + massUpk);
            this._mass = 0;

            while (true)
            {
                int cost = _nextAlien.Energy + _nextAlien.Mass;
                if (this.Energy > cost)
                {
                    this._energy -= cost;
                    Alien.NewAlien(Game.Map.GetEnemyTile(), _nextAlien.Killable, _nextAlien.Attacks, _nextAlien.Movable);
                    _nextAlien = MechBlueprint.Alien(_research);
                }
                else break;
            }

            _research.EndTurn(Math.Pow(difficulty, Consts.DifficultyResearchPow));
        }
        private void PlayTurn(Piece piece, double difficulty)
        {
            IAttacker attacker = piece.GetBehavior<IAttacker>();

            IEnumerable<IKillable> allTargets = Enumerable.Empty<IKillable>();
            Dictionary<Attacker.Attack, IEnumerable<IKillable>> targets = new();
            double avgHp = 1, avgWeight = 1;
            if (attacker != null)
            {
                allTargets = Game.Player.PiecesOfType<IKillable>();
                if (allTargets.Any())
                {
                    avgHp = allTargets.Average(k => k.HitsMax);
                    avgWeight = (allTargets.Average(k => GetKillWeight(k, avgHp)));
                }
                targets = GetTargets(attacker, piece.Tile, allTargets);
            }

            if (piece.HasBehavior(out IMovable movable) && movable.MoveCur >= 1)
            {
                double d = piece.Tile.GetDistance(Game.Player.Core.Tile);
                double distMult = 1.3 * difficulty * Math.Pow((d + 52) / 52, Consts.DistanceMoveDirPow);

                double minDist = Math.Max(0, d - movable.MoveCur);
                double maxDist = d + movable.MoveCur;
                Dictionary<Tile, int> moveTiles = piece.Tile.GetTilesInRange(movable.MoveCur).Where(t => t.Piece == null || t.Piece == piece).ToDictionary(t => t, t =>
                {
                    double result = distMult * (1 - (t.GetDistance(Game.Player.Core.Tile) - minDist) / (maxDist - minDist));
                    result = 1 + result * result;
                    if (attacker != null)
                    {
                        double attackWeight = 1;
                        foreach (var attack in attacker.Attacks.Where(a => !a.Attacked))
                        {
                            var weights = allTargets.Where(k => t.GetDistance(k.Piece.Tile) <= attack.Range).Select(k => GetKillWeight(k, avgHp));
                            if (weights.Any())
                            {
                                double weight = weights.Max();
                                attackWeight += 130 * attack.Damage * (.13 + Math.Pow(weight * weight * weights.Average(), 1 / 3.0) / avgWeight);
                            }
                            else
                            { }
                        }
                        result *= attackWeight;
                    }
                    return Game.Rand.Round(Math.Pow(1 + 13 * result, .91));
                });

                Tile moveTo = Game.Rand.SelectValue(moveTiles);
                bool attackFirst = attacker != null && Game.Rand.Bool(.013);
                Dictionary<Attacker.Attack, IEnumerable<IKillable>> newTargets = GetTargets(attacker, moveTo, allTargets);
                if (!attackFirst && attacker != null)
                    attackFirst = targets.Keys.Any(a => !newTargets.ContainsKey(a));
                if (attackFirst)
                    Attack(attacker, targets, avgHp, avgWeight);

                if (movable.EnemyMove(moveTo))
                    targets = newTargets;
                else if (piece.Tile != moveTo)
                { }
            }

            if (attacker != null)
                Attack(attacker, targets, avgHp, avgWeight);
        }
        private static Dictionary<Attacker.Attack, IEnumerable<IKillable>> GetTargets(IAttacker attacker, Tile from, IEnumerable<IKillable> allTargets)
        {
            return attacker.Attacks.Where(a => !a.Attacked)
                .Select(a => new Tuple<Attacker.Attack, IEnumerable<IKillable>>(a,
                allTargets.Where(k => from.GetDistance(k.Piece.Tile) <= a.Range)))
                .Where(t => t.Item2.Any())
                .ToDictionary(t => t.Item1, t => t.Item2);
        }
        private static void Attack(IAttacker attacker, Dictionary<Attacker.Attack, IEnumerable<IKillable>> targets, double avgHp, double avgWeight)
        {
            Dictionary<IKillable, int> targWeights = targets.Values.SelectMany(v => v).Distinct().ToDictionary(k => k, k => Game.Rand.Round(1 + 13 * GetKillWeight(k, avgHp) / avgWeight));
            while (attacker.Attacks.Any(a => !a.Attacked && targets.ContainsKey(a)) && targWeights.Any())
            {
                IKillable trg = Game.Rand.SelectValue(targWeights);
                targWeights.Remove(trg);
                if (!attacker.EnemyFire(trg))
                { }
            }
        }
        private static double GetKillWeight(IKillable killable, double avgHp)
        {
            double attacks = 0, repair = 0;

            if (killable.Piece.HasBehavior(out IAttacker attacker))
                attacks += attacker.Attacks.Sum(a => a.Damage * (a.Range + 7.8));
            if (killable.Piece.HasBehavior(out IRepair repairs))
                repair += 26 + avgHp * repairs.Rate * (repairs.Range + 3.9);
            if (killable.Piece.HasBehavior(out IBuilder builder))
                repair += 13 + avgHp * (builder.Range + 13) / 130.0;

            double gc = (1 + attacks + 21 * repair) / (killable.HitsCur / (1 - killable.Armor) + killable.ShieldCur);
            double damagePct = 2 - killable.HitsCur / (double)killable.HitsMax;
            double shieldFactor = 3 + 13 * killable.ShieldInc / (killable.HitsMax / 6.5 + killable.ShieldCur);
            double final = gc * gc * damagePct * shieldFactor;
            if (!(final > 0))
            { }
            return final;
        }
    }
}
