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
            for (int a = 0; a < spawns && _nextAlien.AlienCost() + 13 < this.Energy; a++)
                SpawnAlien(() => Game.Map.GetEnemyTile(Alien.GetPathFindingMovement(_nextAlien.Movable)));

            Debug.WriteLine($"Enemy energy: {_energy}");

            _research.EndTurn(Math.Pow(difficulty, Consts.DifficultyResearchPow));

            //we start turn here so the player sees things in the correct state for the enemy's next moves
            base.StartTurn();
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
                SpawnAlien(() =>
                {
                    Tile tile;
                    int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(range / 1.69 + Attack.MIN_RANGED));
                    do
                        tile = Game.Map.GetTile(RandCoord(hive.Tile.X), RandCoord(hive.Tile.Y));
                    while (tile == null || tile.Piece != null);

                    while (Alien.GetPathFindingMovement(_nextAlien.Movable) < Game.Map.GetMinSpawnMove(tile))
                        this._nextAlien = MechBlueprint.Alien(_research);

                    return tile;
                });
            }
        }

        private void SpawnAlien(Func<Tile> GetTile)
        {
            Tile tile;
            List<Point> path;
            while (true)
            {
                tile = GetTile();
                path = tile.Map.PathFind(tile, Alien.GetPathFindingMovement(_nextAlien.Movable), blocked => !blocked.Any());
                if (path == null)
                    this._nextAlien = MechBlueprint.Alien(_research);
                else
                    break;
            }

            this._energy -= Game.Rand.Round(_nextAlien.AlienCost());
            Alien.NewAlien(tile, path, _nextAlien.Killable, _nextAlien.Resilience, _nextAlien.Attacker, _nextAlien.Movable);
            this._nextAlien = MechBlueprint.Alien(_research);
        }
        internal override bool Spend(int energy, int mass)
        {
            this._energy = Game.Rand.Round(this.Energy - energy - mass * Consts.MechMassDiv);
            return true;
        }

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
                        foreach (var attack in attacker.Attacks.Where(a => a.CanAttack()))
                        {
                            var weights = GetDefenders(attack, moveTile, allTargets).Select(k => GetKillWeight(k, avgHp));
                            if (weights.Any())
                            {
                                double weight = weights.Max();
                                attackWeight += 130 * attack.AttackCur * (.13 + Math.Pow(weight * weight * weights.Average(), 1 / 3.0) / avgWeight);
                            }
                        }
                        result *= attackWeight;//+=?
                    }
                    return Game.Rand.Round(Math.Pow(1 + 13 * result, .91));
                });

                Tile moveTo = Game.Rand.SelectValue(moveTiles);
                var newTargets = targets;
                if (attacker != null)
                {
                    const double rand = .013;
                    bool attackFirst = Game.Rand.Bool(rand);
                    newTargets = GetTargets(attacker, moveTo, allTargets);
                    if (!attackFirst)
                        attackFirst = targets.Keys.Any(a => !(newTargets.ContainsKey(a) && newTargets[a].Any(k => DoAttack(a, moveTo, k))));
                    if (attackFirst)// && Game.Rand.Bool(1 - rand))
                        Fire(attacker, targets, avgHp, avgWeight);
                }

                if (movable.EnemyMove(moveTo))
                    targets = newTargets;
                if (piece.Tile != moveTo)
                    ;
            }

            if (attacker != null)
                Fire(attacker, targets, avgHp, avgWeight);
        }

        private static Dictionary<Attack, IEnumerable<IKillable>> GetTargets(IAttacker attacker, Tile attackFrom, IEnumerable<IKillable> allTargets)
        {
            return attacker.Attacks.Where(a => a.CanAttack())
                .Select(a => new Tuple<Attack, IEnumerable<IKillable>>(a, GetDefenders(a, attackFrom, allTargets)))
                .Where(t => t.Item2.Any())
                .ToDictionary(t => t.Item1, t => t.Item2);
        }
        private static void Fire(IAttacker attacker, Dictionary<Attack, IEnumerable<IKillable>> targets, double avgHp, double avgWeight)
        {
            Dictionary<IKillable, int> targWeights = targets.Values.SelectMany(v => v).Distinct().ToDictionary(k => k, k => Game.Rand.Round(1 + 13 * GetKillWeight(k, avgHp) / avgWeight));
            while (attacker.Attacks.Any(a => a.CanAttack() && targets.ContainsKey(a)) && targWeights.Any())
            {
                IKillable trg = Game.Rand.SelectValue(targWeights);
                targWeights.Remove(trg);
                if (attacker.Attacks.Any(a => DoAttack(a, a.Piece.Tile, trg)))
                    if (attacker.EnemyFire(trg))
                    {
                        //targets = GetTargets();
                        //Fire();
                    }
            }
        }
        private static bool DoAttack(Attack attack, Tile attackFrom, IKillable target)
        {
            return attack.AttackCur == attack.AttackMax || (!target.Piece.HasBehavior<IAttacker>() && Game.Rand.Bool())
                || attack.AttackCur > Game.Rand.Next(GetDefenders(attack, attackFrom, target).SelectMany(k => k.TotalDefenses).Max(d => d?.DefenseCur) ?? 0);
        }
        private static IEnumerable<IKillable> GetDefenders(Attack attack, Tile attackFrom, IEnumerable<IKillable> allTargets) =>
            allTargets.SelectMany(k => GetDefenders(attack, attackFrom, k)).Distinct();
        private static IEnumerable<IKillable> GetDefenders(Attack attack, Tile attackFrom, IKillable target) =>
            attack.GetDefenders(target.Piece, attackFrom).Keys;//.Concat(new[] { target }).Distinct();
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
