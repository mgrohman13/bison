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
            double difficulty = GetDifficulty(Game);

            PlayTurn(Math.Pow(difficulty, Consts.DifficultyAIPow));

            base.EndTurn(out double energyUpk, out double massUpk);
            double energy = GetEneryIncome(Game);
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
        internal static double GetDifficulty(Game game) =>
            (game.Turn + Consts.DifficultyIncTurns) / Consts.DifficultyIncTurns;
        internal static double GetEneryIncome(Game game) =>
            Math.Pow(GetDifficulty(game), Consts.DifficultyEnergyPow) * Consts.EnemyEnergy;

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

            double energy = _nextAlien.AlienCost();
            this._energy -= Game.Rand.Round(energy);
            Alien.NewAlien(tile, path, energy, _nextAlien.Killable, _nextAlien.Resilience, _nextAlien.Attacker, _nextAlien.Movable);
            this._nextAlien = MechBlueprint.Alien(_research);
        }
        internal override bool Spend(int energy, int mass)
        {
            this._energy = Game.Rand.Round(this.Energy - energy - mass * Consts.MechMassDiv);
            return true;
        }

        private void PlayTurn(double difficulty)
        {
            Dictionary<Tile, double> playerAttacks = GetPlayerAttacks();
            Dictionary<IKillable, Dictionary<IKillable, int>> allTargets = GetAllTargets();
            double avgHp = 1, avgWeight = 1;
            if (allTargets.Any())
            {
                avgHp = allTargets.Keys.Sum(k => k.AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur)));
                avgWeight = allTargets.Keys.Average(k => GetKillWeight(k, avgHp, null, null));
            }
            HashSet<EnemyPiece> moved = new();
            foreach (var piece in Game.Rand.Iterate(Pieces.Cast<EnemyPiece>()).OrderBy(p => p is Hive ? 1 : 2))
                PlayTurn(piece, Math.Pow(difficulty, Consts.DifficultyAIPow), moved, playerAttacks, allTargets, avgHp, avgWeight);
        }
        private Dictionary<Tile, double> GetPlayerAttacks()
        {
            Dictionary<Tile, double> result = new();
            foreach (var attacker in Game.Player.PiecesOfType<IAttacker>())
                foreach (var t in PlayerAttacks(attacker))
                {
                    result.TryGetValue(t.Item1, out var weight);
                    weight += t.Item2;
                    result[t.Item1] = weight;
                }
            return result;
        }
        private static IEnumerable<Tuple<Tile, double>> PlayerAttacks(IAttacker attacker)
        {
            if (attacker != null)
                foreach (var attack in attacker.Attacks)
                {
                    var tiles = attacker.Piece.Tile.GetTilesInRange(attack);
                    if (attacker.HasBehavior(out IMovable movable) && attack.Range == Attack.MELEE_RANGE)
                        tiles = tiles.Concat(attacker.Piece.Tile.GetTilesInRange(movable).SelectMany(m => m.GetAdjacentTiles())).Distinct();
                    foreach (var tile in tiles)
                        yield return Tuple.Create(tile, Consts.StatValue(attack.AttackCur));
                }
        }

        private Dictionary<IKillable, Dictionary<IKillable, int>> GetAllTargets() =>
            Game.Player.PiecesOfType<IKillable>().ToDictionary(k => k, k => Attack.GetDefenders(this, k.Piece));

        private static void PlayTurn(EnemyPiece piece, double difficulty, HashSet<EnemyPiece> moved,
            Dictionary<Tile, double> playerAttacks, Dictionary<IKillable, Dictionary<IKillable, int>> allTargets, double avgHp, double avgWeight)
        {
            if (piece.Tile.Visible)
                ;

            IKillable killPiece = piece.GetBehavior<IKillable>();
            IAttacker attPiece = piece.GetBehavior<IAttacker>();
            IMovable movePiece = piece.GetBehavior<IMovable>();
            if (movePiece?.MoveCur < 1)
                movePiece = null;

            IEnumerable<Attack> attacks = Enumerable.Empty<Attack>();
            IEnumerable<Attack> melee = Enumerable.Empty<Attack>();
            IEnumerable<Attack> ranged = Enumerable.Empty<Attack>();
            if (attPiece != null)
            {
                attacks = GetAttacks(attPiece).ToList();
                melee = attacks.Where(a => a.Range == Attack.MELEE_RANGE).ToList();
                ranged = attacks.Where(a => a.Range > Attack.MELEE_RANGE).ToList();
            }

            HashSet<Tile> moveTiles = new();
            if (movePiece != null)
                moveTiles = piece.Tile.GetTilesInRange(movePiece, movePiece.MoveCur + (melee.Any() ? Attack.MELEE_RANGE : 0)).ToHashSet();
            bool filteredMoves = false;
            void FilterMoves()
            {
                if (!filteredMoves)
                    moveTiles.RemoveWhere(t => t.Piece != null && t.Piece != piece);
                filteredMoves = true;
            }

            HashSet<IKillable> targets = new();
            if (attacks.Any())
            {
                HashSet<Tile> attTiles = piece.Tile.GetTilesInRange(attPiece, attacks.Max(a => a.Range)).ToHashSet();
                if (movePiece != null && melee.Any())
                {
                    var meleeTiles = moveTiles.ToList();
                    FilterMoves();
                    moveTiles.RemoveWhere(t => t.GetDistance(piece.Tile) > movePiece.MoveCur);
                    attTiles.UnionWith(meleeTiles.Where(t => t.GetTilesInRange(melee.First()).Any(moveTiles.Contains)));
                }
                targets = attTiles.Select(t => t.Piece?.GetBehavior<IKillable>()).Where(k => k != null && k.Piece.IsPlayer && !k.Dead).ToHashSet();
            }
            FilterMoves();

            double maxMoveAttRange = (movePiece?.MoveCur ?? 0) + (attacks.Max(a => a?.Range) ?? 0);
            HashSet<IKillable> extendedTargets = allTargets.Keys.Where(k => k.Piece.Tile.GetDistance(piece.Tile) < maxMoveAttRange).SelectMany(k => allTargets[k].Keys).ToHashSet();

            //if (moveTiles.Any(t => !playerAttacks.ContainsKey(t)))
            //    ;
            //else
            //    ;

            EnemyPiece.AIState state = piece.TurnState(difficulty, playerAttacks, moveTiles, extendedTargets, out List<Point> fullPath);

            IKillable target = null;
            if (attPiece != null && state != EnemyPiece.AIState.Retreat)
                if (targets.Any())
                    target = Game.Rand.SelectValue(targets, GetWeight);
                else if (state == EnemyPiece.AIState.Fight)
                    target = Game.Rand.Iterate((IEnumerable<IKillable>)(extendedTargets.Any() ? extendedTargets : allTargets.Keys)).OrderBy(k =>
                    {
                        double dist = piece.Tile.GetDistance(k.Piece.Tile);
                        double weight = GetWeight(k);
                        return 1 + (avgWeight + Game.Rand.OE(weight)) / dist / dist;
                    }).FirstOrDefault();
            int GetWeight(IKillable killable)
            {
                Tile tile = killable.Piece.Tile;
                bool meleeRange = melee.Any() && tile.GetAdjacentTiles().Any(moveTiles.Contains);
                double attValue = 1 + SumAttacks(attacks, a => a.Range == Attack.MELEE_RANGE ? meleeRange : tile.GetDistance(piece.Tile) <= a.Range);
                return Game.Rand.Round(attValue * GetGroupWeight(killable));
            }
            double GetGroupWeight(IKillable killable)
            {
                var defenders = allTargets[killable];
                return defenders.Sum(p => GetKillWeight(p.Key, avgHp, state, target) * p.Value) / (double)defenders.Values.Sum();
            }

            Tile moveTo = piece.Tile;
            if (movePiece != null)
            {
                List<Tile> pathTiles = new();
                if (fullPath != null)
                {
                    int keepDiv = 0;
                    //Point final = fullPath[^1];
                    for (int a = fullPath.Count; --a >= 0;)
                    {
                        Tile pathTile = piece.Tile.Map.GetTile(fullPath[a]);
                        //diminishing chance of adding each successive tile - we don't want very many since we loop through them for each possible moveTile
                        if (moveTiles.Contains(pathTile) && Game.Rand.Next(++keepDiv) == 0)
                            pathTiles.Add(pathTile);
                    }
                    pathTiles.Reverse();
                }

                static IEnumerable<IKillable> MeleeTargets(Tile tile) => tile.GetAdjacentTiles().Select(t => t.Piece?.GetBehavior<IKillable>()).Where(k => k != null && k.Piece.IsPlayer && !k.Dead);//reuse
                bool hasMeleeTrg = !melee.Any() || MeleeTargets(piece.Tile).Any();
                double attValue = SumAttacks(attacks, _ => true);
                double meleeValue = SumAttacks(melee, _ => true);
                double defValue = 0;
                if (killPiece != null)
                    defValue = DefWeight(killPiece);
                double moveValue = (1 * movePiece.MoveCur + 2 * movePiece.MoveInc) / 3.0;

                killPiece.GetHitsRepair(out double repair, out _);
                var armor = killPiece.Protection.SingleOrDefault(d => d.Type == CombatTypes.DefenseType.Armor && d.DefenseCur < d.DefenseMax);
                if (armor != null)
                    repair += armor.GetRegen() / 2.0;

                Debug.WriteLine(piece);

                //eventually convert to ulong?
                var dict = moveTiles.ToDictionary(t => t, moveTile =>
                {
                    double attWeight = 1;
                    if (attValue > 0)
                    {
                        //can hit with melee attack that cant hit now 
                        double meleeVal = 0; //avg trg weights
                        double meleeAttTrg = 0;
                        if (!hasMeleeTrg)
                            foreach (var attack in melee)
                            {
                                double weight = AttWeight(attack) ?? 0;
                                var trg = MeleeTargets(moveTile);
                                meleeVal += weight * (trg.Average(k => (double?)GetGroupWeight(k)) ?? 0);

                                if (target != null && trg.Contains(target))
                                    meleeAttTrg += weight;
                            }
                        meleeVal /= attValue;
                        meleeAttTrg /= attValue;

                        //enemies in range of atts for next turn 
                        double rangedVal = 0;  //avg trg weights
                        double rangeAttTrg = 0;  //0-1
                        foreach (var attack in ranged)
                        {
                            double weight = AttWeight(attack) ?? 0;
                            rangedVal += weight * (extendedTargets
                                .Select(k => Tuple.Create(k, k.Piece.Tile.GetDistance(moveTile)))
                                .Where(t => t.Item2 <= attack.Range)
                                .Average(t => (double?)GetGroupWeight(t.Item1) * attack.Range / (attack.Range + t.Item2)) ?? 0);

                            if (target != null && target.Piece.Tile.GetDistance(moveTile) <= attack.Range)
                                rangeAttTrg += weight;
                        }
                        rangedVal /= attValue;
                        rangeAttTrg /= attValue;

                        double attPct = (meleeVal + rangedVal) / 2.0 / avgWeight; //centered on 1
                        double trgVal = meleeAttTrg + rangeAttTrg; //0-2

                        if (state == EnemyPiece.AIState.Fight && attPct == 0 && trgVal == 0)
                        {
                            double dist = moveTile.GetDistance(target.Piece.Tile);
                            attWeight = moveValue / (moveValue + dist * dist);
                        }
                        else
                        {
                            attWeight = attPct * Math.Sqrt(attValue + attPct);
                            attWeight = (1 + attWeight) * (1 + trgVal * trgVal);
                        }
                    }

                    double pathWeight = 1;
                    if (pathTiles.Any())
                    {
                        Tile final = pathTiles[^1];
                        double curDist = piece.Tile.GetDistance(final) + 1;
                        double moveTileDist = moveTile.GetDistance(final) + 1;
                        bool moveCloser = moveTileDist <= curDist;
                        double padding = Math.Sqrt(moveValue + 1); ;
                        double mult = curDist + padding;
                        for (int b = 0; b < pathTiles.Count; b++)
                        {
                            var tile = pathTiles[b];
                            double pct = pathTiles.Count == 1 ? 1 : b / (double)(pathTiles.Count - 1);
                            pct *= pct;
                            double dist = tile.GetDistance(moveTile);
                            dist = (dist + 1) * (dist + padding);
                            //dist /= pct;

                            double weight = 1 + (1 + pct * mult) / dist;
                            if (moveCloser)
                                weight *= (weight);
                            else
                                weight = Math.Sqrt(weight);
                            //if (moveTile == tile)
                            //    weight *= Math.Pow(weight, (1 + pct));

                            pathWeight = Math.Max(pathWeight, (weight));
                        }
                        //if (piece.ToString() == "Alien 2")
                        //    Debug.WriteLine((float)pathWeight);
                    }

                    double coreWeight = Consts.CaveDistance / (Consts.CaveDistance + Consts.PathWidth + moveTile.GetDistance(piece.Game.Player.Core.Tile));
                    coreWeight = Math.Pow(coreWeight, difficulty);

                    double playerAttWeight = 1;
                    playerAttacks.TryGetValue(moveTile, out playerAttWeight);
                    playerAttWeight = defValue / (defValue + playerAttWeight);
                    playerAttWeight *= playerAttWeight;

                    double moveWeight = (moveValue / (moveValue + piece.Tile.GetDistance(moveTile)));//Math.Sqrt

                    double repairWeight = 1;
                    if (moveTile == piece.Tile || !moveTile.Visible)
                        repairWeight = repair + 1;
                    if (repairWeight > 1)
                    {
                        repairWeight *= (1 + Math.Sqrt(defValue) + repair);
                        if (moveTile == piece.Tile)
                        {
                            repairWeight *= Math.Sqrt(repairWeight);
                            pathWeight = 1;
                            coreWeight = 1;
                            playerAttWeight *= Math.Sqrt(playerAttWeight);
                        }
                    }

                    double defWeight = 1;
                    if (killPiece != null)
                    {
                        var friendly = moveTile.GetAdjacentTiles().Select(t => (t.Piece?.GetBehavior<IKillable>()))
                            .Where(k => k != null && k.Piece.Side == piece.Side && k.Piece != piece && moved.Contains(k.Piece)).ToList();
                        foreach (var killable in friendly)
                        {
                            var weight = defValue / DefWeight(killable);
                            if (killable.HasBehavior<IAttacker>() && weight < 1)
                                weight = 1 / weight;
                            defWeight *= Math.Sqrt(1 + weight);
                        }
                        if (friendly.OfType<Hive>().Any())
                            defWeight *= Math.Sqrt(defWeight);
                        defWeight = Math.Pow(defWeight, .5 / playerAttWeight);
                    }

                    //double multipliers = 0;
                    void Inc(ref double weight, double pow)
                    {
                        weight = Math.Pow(weight, Math.Sqrt(pow));
                        //if (weight != 1)
                        //    multipliers += (pow - 1);// / 2.0;
                    }
                    switch (state)
                    {
                        case EnemyPiece.AIState.Retreat:
                            Inc(ref attWeight, 1 / 3.5);
                            Inc(ref pathWeight, 3);
                            Inc(ref playerAttWeight, 4);
                            Inc(ref repairWeight, 3);
                            Inc(ref defWeight, 4);
                            break;
                        case EnemyPiece.AIState.Patrol:
                            Inc(ref moveWeight, 2);
                            goto case EnemyPiece.AIState.Fight;
                        case EnemyPiece.AIState.Fight:
                            Inc(ref attWeight, 4);
                            Inc(ref playerAttWeight, 2);
                            Inc(ref defWeight, 3);
                            break;
                        case EnemyPiece.AIState.Rush:
                            Inc(ref pathWeight, 4);
                            Inc(ref coreWeight, 2);
                            goto case EnemyPiece.AIState.Fight;
                    }

                    double[] weights = new double[] { attWeight, pathWeight, coreWeight, playerAttWeight, moveWeight, repairWeight, defWeight, };
                    double result = 1, div = 1;
                    foreach (var w in weights)
                    {
                        //if (w != 1)
                        //    multipliers++;
                        if (double.IsNaN(w) || w < 0)
                            throw new Exception();
                        if (w > ushort.MaxValue)
                            ;
                        if (w > 1)
                            result += w;
                        else
                            div *= w;// * w;
                    }
                    result *= div;// Math.Pow(result, 1.0 / multipliers);
                    int chance = Game.Rand.Round(1 + moveTiles.Count * result);
                    if (chance < 0)
                        throw new Exception();
                    return chance;
                });
                moveTo = Game.Rand.SelectValue(dict);
            }

            if (piece.Tile != moveTo)
            {
                if (attPiece != null)
                {
                    IKillable meleeTrg = target;
                    if (meleeTrg == null)
                    {
                        var meleeTargets = targets.Where(k => Math.Min(k.Piece.Tile.GetDistance(piece.Tile), k.Piece.Tile.GetDistance(moveTo)) <= Attack.MELEE_RANGE);
                        if (meleeTargets.Any())
                            meleeTrg = Game.Rand.SelectValue(meleeTargets, GetWeight);
                    }

                    Fire((meleeTrg?.Piece.Tile.GetDistance(moveTo) ?? 0) > Attack.MELEE_RANGE);
                }

                if (!movePiece.EnemyMove(moveTo))
                    ;
            }

            Fire(true);

            moved.Add(piece);

            void Fire(bool useMelee)
            {
                var attacks = GetAttacks(attPiece).Where(a => useMelee || a.Range > Attack.MELEE_RANGE);
                if (attacks.Any())
                {
                    //foreach (double range in attacks.Where(a => inRetreat || IsFull(a)).Select(a => a.Range).Order())
                    foreach (var attack in Game.Rand.Iterate(attacks))
                    {
                        bool CanTarget(IKillable killable) => attack.GetDefenders(killable.Piece).Any();
                        if (target == null || !CanTarget(target))
                        {
                            var choices = targets.Where(CanTarget);
                            if (choices.Any())
                                target = Game.Rand.SelectValue(choices, GetWeight);
                            else
                                ;
                        }

                        if (target != null && allTargets.TryGetValue(target, out var trgGrp))
                        {
                            if (state != EnemyPiece.AIState.Retreat)
                            {
                                double def = 0;
                                foreach (var pair in trgGrp)
                                {
                                    var defenses = pair.Key.AllDefenses.ToDictionary(d => d, CombatTypes.GetDefenceChance);
                                    double tDef = defenses.Sum(p => Consts.StatValue(p.Key.DefenseCur) * p.Value) / (double)defenses.Values.Sum();
                                    if (!pair.Key.Piece.HasBehavior<IAttacker>())
                                        tDef *= Game.Rand.DoubleHalf();
                                    def += tDef * pair.Value;
                                }
                                int defense = Game.Rand.Round(Consts.StatValueInverse(def / (double)trgGrp.Values.Sum()));
                                if (!(IsFull(attack) || attack.AttackCur > Game.Rand.RangeInt(0, defense)))
                                    continue;
                            }

                            List<Tuple<Tile, double>> trgAttacks = new();
                            if (CanTarget(target))
                                trgAttacks = trgGrp.Keys.SelectMany(k => PlayerAttacks(k.GetBehavior<IAttacker>())).ToList();

                            if (attPiece.EnemyFire(target, attack))
                            {
                                //update targets, player attacks 
                                foreach (var pair in trgAttacks)
                                    playerAttacks[pair.Item1] -= pair.Item2;
                                foreach (var killable in trgGrp.Keys)
                                    if (killable.Dead)
                                    {
                                        allTargets.Remove(killable);
                                        foreach (var v in allTargets.Values)
                                            if (v.Remove(killable))
                                                ;
                                    }
                                    else
                                    {
                                        foreach (var t in PlayerAttacks(killable.GetBehavior<IAttacker>()))
                                            playerAttacks[t.Item1] += t.Item2;
                                    }
                                //unecessary - we never loop through all playerAttacks
                                //foreach (var pair in trgAttacks)
                                //    if (playerAttacks[pair.Item1] <= 0)
                                //        playerAttacks.Remove(pair.Item1);
                            }
                            else if (CanTarget(target))
                            { }
                        }
                        else if (piece is Alien)
                        { }
                    }
                }
            }
        }

        private static IEnumerable<Attack> GetAttacks(IAttacker attacker) =>
            attacker?.Attacks.Where(a => a.CanAttack()) ?? Enumerable.Empty<Attack>();
        private static double SumAttacks(IEnumerable<Attack> attacks, Func<Attack, bool> Predicate) => attacks?.Where(Predicate).Sum(AttWeight) ?? 0;
        private static double? AttWeight(Attack a) => Consts.StatValue(a.AttackCur) * Math.Sqrt(a.AttackCur / (double)a.AttackMax) * (IsFull(a) ? 2 : 1);
        private static bool IsFull(Attack a) => a.AttackCur == a.AttackMax || (a.Reload < 1 && Game.Rand.Bool()) //??
            || a.AttackCur + Game.Rand.RangeInt(1, Game.Rand.Round(a.Reload)) > a.AttackMax;
        private static double DefWeight(IKillable k) => k?.AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur)) ?? 0;

        private static double GetKillWeight(IKillable killable, double avgHp, EnemyPiece.AIState? state, IKillable target)
        {
            double attacks = 0, repair = 0;

            if (killable.Piece.HasBehavior(out IAttacker attacker))
                attacks += attacker.Attacks.Sum(a => Consts.StatValue(a.AttackCur)
                    * (Math.Max(a.Range, Attack.MIN_RANGED) + Attack.MIN_RANGED) / Attack.MIN_RANGED / 2.0);
            if (killable.Piece.HasBehavior(out IRepair repairs))
                repair += 26 + avgHp * repairs.Rate * (repairs.Range + 3.9);
            if (killable.Piece.HasBehavior(out IBuilder builder))
                repair += 13 + avgHp * (builder.Range + 13) / 130.0;

            double mass = 0;
            if (killable.Piece is PlayerPiece playerPiece)
            {
                double e, r;
                e = r = 0;
                playerPiece.GenerateResources(ref e, ref mass, ref r);
                double ratio = Math.Sqrt(Consts.EnergyForFabricateMass * Consts.BurnMassForEnergy);
                mass += e * Consts.MechMassDiv + r / (ratio * Consts.MassForScrapResearch);
            }
            mass *= avgHp / 39.0;

            double shieldFactor = 1;
            foreach (var def in killable.AllDefenses)
            {
                double pow = def.Type switch
                {
                    CombatTypes.DefenseType.Hits => .75,
                    CombatTypes.DefenseType.Shield => 1.5,
                    _ => 1,
                };
                shieldFactor *= Math.Pow(Consts.StatValue(def.DefenseMax + 1) / Consts.StatValue(def.DefenseCur + 1), pow / 2.0);
            }
            shieldFactor = Math.Pow(shieldFactor, 1.0 / killable.AllDefenses.Count);

            double defCur = killable.AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur));
            double defMax = killable.AllDefenses.Sum(d => Consts.StatValue(d.DefenseMax));
            if (!killable.HasBehavior<IAttacker>())
            {
                defCur /= 3.9;
                defMax /= 3.9;
            }

            double gc = (1 + attacks + repair + mass) / defCur;
            double damagePct = 2 - defCur / (double)defMax;

            double trg = killable == target ? 3.9 : 1;
            if (state == EnemyPiece.AIState.Rush && killable.Piece is Core)
                trg *= 2.6;

            return Math.Sqrt(1 + gc * gc * damagePct * shieldFactor * trg);
        }
    }
}
