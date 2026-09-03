using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AIState = ClassLibrary1.Pieces.Enemies.EnemyPiece.AIState;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1
{
    internal static class EnemyMovement
    {
        internal static void PlayTurn(Game game, double difficulty, double aggression, bool clearPaths, Action<Tile, double> UpdateProgress)
        {
            Debug.WriteLine($"EnemyMovement.PlayTurn, difficulty: {(float)difficulty}, aggression: {(float)aggression}");

            double offset = 0;
            offset = game.Enemy.Pieces.Average(Time);
            double totalTime = offset + game.Enemy.Pieces.Sum(Time);
            double progress = offset / totalTime;

            Dictionary<Tile, double> playerAttacks = GetPlayerAttacks(game);
            Dictionary<IKillable, Dictionary<IKillable, int>> allTargets = GetAllTargets(game);
            double avgHp = 1, avgWeight = 1;
            if (allTargets.Count > 0)
            {
                avgHp = allTargets.Keys.Average(DefWeight);
                avgWeight = allTargets.Keys.Average(k => GetKillWeight(k, avgHp, null, null));
            }

            UpdateProgress(null, progress);
            HashSet<EnemyPiece> moved = [];
            foreach (var piece in Game.Rand.Iterate(game.Enemy.Pieces.Cast<EnemyPiece>()).OrderBy(p => p is Hive ? 1 : 2))
            {
                progress += Time(piece) / totalTime;
                allTargets = PlayTurn(piece, difficulty, aggression, clearPaths, moved, playerAttacks, allTargets, avgHp, avgWeight);
                UpdateProgress(piece.Tile.Visible ? piece.Tile : null, Math.Min(progress, 1));
            }
            double Time(Piece enemy) => offset + (enemy.HasBehavior(out IMovable movable) ? movable.MoveCur * movable.MoveCur : 0);
        }
        internal static Dictionary<Tile, double> GetPlayerAttacks(Game game)
        {
            Dictionary<Tile, double> result = [];
            foreach (var attacker in game.Player.PiecesOfType<IAttacker>())
                foreach (var t in PlayerAttacks(attacker))
                {
                    result.TryGetValue(t.Item1, out var weight);
                    weight += t.Item2;
                    result[t.Item1] = weight;
                }
            return result;
        }
        private static IEnumerable<Tuple<Tile, double>> PlayerAttacks(IAttacker attacker) =>
            Tile.GetAttacks(attacker, false).Select(tuple =>
                new Tuple<Tile, double>(attacker.Piece.Tile.Map.GetTile(tuple.Item1), tuple.Item2));

        private static Dictionary<IKillable, Dictionary<IKillable, int>> GetAllTargets(Game game) =>
            game.Player.PiecesOfType<IKillable>().ToDictionary(k => k, k => Attack.GetDefenders(game.Enemy, k.Piece));

        private static Dictionary<IKillable, Dictionary<IKillable, int>> PlayTurn(EnemyPiece piece, double difficulty, double aggression, bool clearPaths, HashSet<EnemyPiece> moved,
            Dictionary<Tile, double> playerAttacks, Dictionary<IKillable, Dictionary<IKillable, int>> allTargets, double avgHp, double avgWeight)
        {
            //if (piece.ToString() == "Alien 28")
            //    ;

            Game game = piece.Game;
            Consts consts = game.Consts;

            IKillable killPiece = piece.GetBehavior<IKillable>();
            IAttacker attPiece = piece.GetBehavior<IAttacker>();
            IMovable movable = piece.GetBehavior<IMovable>();
            if (movable?.MoveCur < 1)
                movable = null;

            IEnumerable<Attack> attacks = [];
            IEnumerable<Attack> melee = [];
            IEnumerable<Attack> ranged = [];
            if (attPiece != null)
            {
                attacks = [.. GetAttacks(attPiece)];
                melee = [.. attacks.Where(a => a.Range == Attack.MELEE_RANGE)];
                ranged = [.. attacks.Where(a => a.Range > Attack.MELEE_RANGE)];
            }

            Tile orig = piece.Tile;
            double mapSize = game.Map.GetMapSize();
            HashSet<Tile> moveTiles = [];
            List<Tile> rawMoves = [];
            if (movable != null)
            {
                rawMoves = [.. orig.GetTilesInRange(movable)];
                moveTiles = [.. rawMoves];
                if (melee.Any())
                    moveTiles = [.. moveTiles.SelectMany(t => t.GetAdjacentTiles())];
            }

            bool usePortal = true;
            bool HasPortal(Tile t) => usePortal && t.Piece is Portal portal && portal.CanPort(movable, out _, out _);
            usePortal = moveTiles.Any(HasPortal);
            bool filteredMoves = false;
            void FilterMoves()
            {
                if (!filteredMoves)
                    moveTiles.RemoveWhere(t => t.Piece != null && t.Piece != piece && !HasPortal(t));
                filteredMoves = true;
            }

            HashSet<IKillable> targets = [];
            if (attacks.Any())
            {
                HashSet<Tile> attTiles = [.. orig.GetTilesInRange(attPiece)];
                if (movable != null && melee.Any())
                {
                    var meleeTiles = moveTiles.ToList();
                    moveTiles = [.. rawMoves];
                    FilterMoves();
                    attTiles.UnionWith(meleeTiles.Where(t => t.GetTilesInRange(melee.First()).Any(moveTiles.Contains)));
                }
                targets = [.. attTiles.Select(t => t.Piece?.GetBehavior<IKillable>()).Where(k => k != null && k.Piece.IsPlayer && !k.Dead)];
            }
            FilterMoves();

            double attValue = SumAttacks(attacks, orig, null);
            double maxMoveAttRange = (movable?.MoveCur ?? 0) + (attacks.Max(a => a?.Range) ?? 0);
            HashSet<IKillable> extendedTargets = [.. allTargets.Keys.Where(k => orig.MoveDistTo(k.Piece.Tile, maxMoveAttRange)).SelectMany(k => allTargets[k].Keys)];

            AIState prev = piece.State;
            AIState state = piece.TurnState(difficulty, aggression, clearPaths, playerAttacks, moveTiles, extendedTargets, out List<Point> fullPath);
            usePortal &= (state == AIState.Fight || state == AIState.Patrol || state == AIState.Rush);

            IKillable target = null;
            if (attPiece != null && state != AIState.Retreat)
                if (targets.Count > 0)
                    target = Game.Rand.SelectValue(targets, GetWeight);
                else if (state == AIState.Fight || state == AIState.Patrol)
                    target = Game.Rand.Iterate((IEnumerable<IKillable>)(extendedTargets.Count > 0 ? extendedTargets : allTargets.Keys)).OrderBy(k =>
                    {
                        double dist = orig.MoveDistTo(k.Piece.Tile);
                        double weight = GetWeight(k);
                        return 1 + (avgWeight + Game.Rand.OE(weight)) / dist / dist;
                    }).FirstOrDefault();
            int GetWeight(IKillable killable)
            {
                Tile tile = killable.Piece.Tile;
                bool meleeRange = melee.Any() && tile.GetAdjacentTiles().Any(moveTiles.Contains);
                double inRange = 1 + SumAttacks(attacks, orig, tile, a => a.Range == Attack.MELEE_RANGE ? meleeRange : orig.GetDistance(tile) <= a.Range);
                return Game.Rand.Round(1 + inRange / (attValue < 1 ? 1 : attValue) * GetGroupWeight(killable));
            }
            double GetGroupWeight(IKillable killable)
            {
                var defenders = allTargets[killable];
                return defenders.Sum(p => GetKillWeight(p.Key, avgHp, state, target) * p.Value) / (double)defenders.Values.Sum();
            }

            double defValue = 0;
            if (killPiece != null)
                defValue = DefWeight(killPiece);

            double innerRange = Consts.LimitedMove(movable, out bool limitMove);

            Tile moveTo = orig;
            if (movable != null && state != AIState.Heal)
            {
                bool seeCore = targets.Any(k => k.Piece is Core);

                List<Tile> pathTiles = [];
                if (fullPath != null)//&& (fullPath.Count > 2 || HasPortal(game.Map.GetTile(fullPath[^1]))))
                {
                    List<Tile> movePath = [];
                    bool? prevContains = null;
                    for (int a = 0; a < fullPath.Count; a++)
                    {
                        Tile pathTile = orig.Map.GetTile(fullPath[a]);
                        bool contains = moveTiles.Contains(pathTile);
                        if (contains || prevContains == true)
                        {
                            if (contains && prevContains == false)
                                movePath.Add(orig.Map.GetTile(fullPath[a - 1]));
                            movePath.Add(pathTile);
                        }
                        prevContains = contains;
                    }

                    if (movePath.Count > 1)
                        for (int b = 1; b < movePath.Count; b++)
                            pathTiles.AddRange(Tile.GetLinePoints(movePath[b - 1].Location, movePath[b].Location)
                                .Skip(b > 1 ? 1 : 0).Select(orig.Map.GetTile).Where(moveTiles.Contains));
                    else
                        pathTiles = movePath;

                    if (pathTiles.Count > 0)
                    {
                        double pathHeight = pathTiles.Max(Tile.Height);
                        pathTiles.Remove(orig);
                        pathTiles.RemoveAll(t => !HasPortal(t) && Tile.Height(t) + Game.Rand.DoubleFull(Math.PI) < Game.Rand.DoubleHalf(pathHeight));
                        //increasing chance of removing each tile further back in the path
                        //we don't want too many since we loop through them for each possible moveTile
                        int c = pathTiles.Count;
                        if (c > 1)
                        {
                            bool flag = true;
                            int d = c;
                            for (int e = c - 2; e >= 0; e--)
                            {
                                if (limitMove && flag && orig.MoveDistTo(pathTiles[e], innerRange))
                                {
                                    flag = false;
                                    d = e + 1;
                                }
                                if (Game.Rand.Next(d - e) > 0)
                                    pathTiles.RemoveAt(e);
                            }
                        }
                    }
                }

                static IEnumerable<IKillable> MeleeTargets(Tile tile) => tile.GetAdjacentTiles()
                    .Select(t => t.Piece?.GetBehavior<IKillable>()).Where(k => k != null && k.Piece.IsPlayer && !k.Dead);//reuse
                bool hasMeleeTrg = !melee.Any() || MeleeTargets(orig).Any();
                double moveValue = (1 * movable.MoveCur + 2 * movable.MoveInc) / 3.0;

                killPiece.GetHitsRepair(out double repair, out _);
                var armor = killPiece.Protection.SingleOrDefault(d => d.Type == CombatTypes.DefenseType.Armor && d.DefenseCur < d.DefenseMax);
                if (armor != null)
                    repair += armor.GetRegen() / 2.0;

                Alien alien = piece as Alien;
                double morale = alien?.Morale ?? 1;

                Debug.WriteLine(piece);

                Dictionary<Tile, double> dictDbl = [];
#pragma warning disable IDE0018 // Inline variable declaration
                foreach (var moveTile in Game.Rand.Iterate(moveTiles))
                {
                    double moveDist = orig.MoveDistTo(moveTile);

                    double attWeight = 1;
                    if (attValue > 0)
                    {
                        //can hit with melee attack that cant hit now 
                        double meleeVal = 0; //avg trg weights
                        double meleeAttTrg = 0;
                        if (!hasMeleeTrg)
                            foreach (var attack in melee)
                            {
                                var trg = MeleeTargets(moveTile);
                                double weight = AttWeight(attack, moveTile, trg.Any() ? Game.Rand.SelectValue(trg).Piece.Tile : null) ?? 0;
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
                            double weight = AttWeight(attack, moveTile, extendedTargets.Count != 0 ? Game.Rand.SelectValue(extendedTargets).Piece.Tile : null) ?? 0;
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

                        if (attPct == 0 && trgVal == 0)
                        {
                            if (state == AIState.Fight || state == AIState.Patrol)
                            {
                                double dist = moveTile.MoveDistTo(target.Piece.Tile);
                                attWeight = moveValue / (moveValue + dist * dist);
                                if (state == AIState.Fight)
                                    attWeight *= attWeight;
                            }
                        }
                        else
                        {
                            attWeight = attPct * Math.Sqrt(attValue + attPct);
                            attWeight = (1 + attWeight) * (1 + trgVal * trgVal);
                        }
                    }

                    bool validRetreat = state == AIState.Retreat && alien.ValidRetreatTile(moveTile, playerAttacks);

                    double pathWeight = 1;
                    double padding = Math.Sqrt(moveValue + 1);
                    if (HasPortal(moveTile))
                    {
                        // consolidate
                        Tile final = moveTile;
                        double curDist = orig.MoveDistTo(final) + 1;
                        double mult = curDist + padding;
                        double pct = 1;
                        double dist = -.5;
                        dist = (dist + 1) * (dist + padding);
                        double weight = 1 + (1 + pct * mult) / dist;
                        weight *= weight;
                        pathWeight = weight;
                        if (state == AIState.Rush)
                            pathWeight *= aggression;
                    }
                    else if (pathTiles.Count > 0)
                    {
                        // consolidate
                        Tile final = pathTiles[^1];
                        double curDist = orig.MoveDistTo(final) + 1;
                        double moveTileDist = moveTile.MoveDistTo(final) + 1;
                        bool moveCloser = moveTileDist <= curDist;
                        double mult = curDist + padding;
                        for (int b = 0; b < pathTiles.Count; b++)
                        {
                            var tile = pathTiles[b];
                            double pct = pathTiles.Count == 1 ? 1 : b / (double)(pathTiles.Count - 1);
                            //pct *= pct;
                            double dist = tile.MoveDistTo(moveTile);
                            dist = (dist + 1) * (dist + padding);

                            double weight = 1 + (1 + pct * mult) / dist;
                            if (moveCloser)
                                weight *= weight;
                            else
                                weight = Math.Sqrt(weight);

                            pathWeight = Math.Max(pathWeight, weight);
                        }

                        if (validRetreat)
                            pathWeight *= pathWeight + 1;

                        if (limitMove && moveDist > innerRange)
                            pathWeight *= Math.Pow(innerRange / (moveDist + moveValue), moveDist / (innerRange + 1) + 1);
                    }

                    double coreWeight = consts.CaveDistance / (consts.CaveDistance + consts.PathWidth + moveTile.MoveDistTo(game.Player.Core.Tile));
                    //if (entrance)
                    //{
                    //    double[] all = new double[] { attWeight, pathWeight, coreWeight, playerAttWeight, moveWeight, // repairWeight,
                    //        defWeight, };
                    //    double w = all.Min();
                    //    if (w < 1)
                    //        w = 1 / w;
                    //    else
                    //        w = all.Max();
                    //    w *= w + 1;
                    //    coreWeight *= moveTiles.Count + w;
                    //}
                    coreWeight = Math.Pow(coreWeight, difficulty);
                    coreWeight *= game.Consts.MapDistMult(moveTile, mapSize);

                    double playerAttWeight = 1;
                    playerAttacks.TryGetValue(moveTile, out playerAttWeight);
                    playerAttWeight = defValue / (defValue + playerAttWeight);
                    playerAttWeight *= playerAttWeight;

                    double moveWeight = (moveValue / (moveValue + moveDist));//Math.Sqrt

                    //double repairWeight = 1;
                    //if (moveTile == orig || !moveTile.Visible)
                    //    repairWeight = repair + 1;
                    //if (repairWeight > 1)
                    //{
                    //    repairWeight *= (1 + Math.Sqrt(defValue) + repair);
                    //    if (moveTile == orig)
                    //    {
                    //        repairWeight *= Math.Sqrt(repairWeight);
                    //        pathWeight = 1;
                    //        coreWeight = 1;
                    //        playerAttWeight *= Math.Sqrt(playerAttWeight);
                    //    }
                    //}

                    //height?
                    double defWeight = 1;
                    if (killPiece != null)
                    {
                        var friendly = moveTile.GetAdjacentTiles().Select(t => (t.Piece?.GetBehavior<IKillable>()))
                            .Where(k => k != null && k.Piece.Side == piece.Side && k.Piece != piece && moved.Contains(k.Piece)
                                && !(piece is Hive && state == AIState.Rush && extendedTargets.Count == 0)).ToList();
                        if (friendly.Count > 0)
                        {
                            int count = 0;
                            foreach (var killable in friendly)
                            {
                                var weight = defValue / DefWeight(killable);
                                if (weight < 1)
                                    if (killable.HasBehavior<IAttacker>())
                                        weight = 1 / weight;
                                    else
                                        weight = 1;
                                defWeight *= weight;
                                count++;
                            }
                            defWeight = (1 + Math.Pow(defWeight, 2.0 / count)) * count;
                            if (!usePortal && (friendly.OfType<Hive>().Any() || friendly.OfType<Portal>().Any()))
                                defWeight *= Math.Sqrt(defWeight);
                            double exponent = .25 / playerAttWeight;
                            if (exponent > 1)
                                exponent = Math.Log(exponent);
                            else
                                exponent = Math.Sqrt(exponent);
                            defWeight = Math.Pow(defWeight, exponent);
                        }
                    }

                    double moveHeight = Tile.Height(moveTile);
                    double terrainWeight = Math.Pow(1 + 3.9 * Math.Sqrt(moveHeight / consts.ElevationHeight), 1 + 1 / difficulty);
                    double origHeight = Tile.Height(orig);
                    if (moveHeight < origHeight)
                    {
                        double offset = consts.ElevationHeight / 5;
                        terrainWeight *= Math.Pow((moveHeight + offset) / (origHeight + offset), 1 + difficulty);
                    }

                    ////debug
                    //if (piece.ToString() == "Alien 56")
                    //{
                    //    string logWeights = string.Format("attWeight:{1}{0}pathWeight:{2}{0}coreWeight:{3}{0}playerAttWeight:{4}{0}moveWeight:{5}{0}repairWeight:{6}{0}defWeight:{7}",
                    //        Environment.NewLine, attWeight, pathWeight, coreWeight, playerAttWeight, moveWeight, repairWeight, defWeight);
                    //    Debug.WriteLine(logWeights);
                    //}

                    void Inc(ref double weight, double pow, bool moraleDir = true)
                    {
                        double moraleMult = moraleDir ? morale : 1 - morale;
                        pow = Math.Sqrt(pow) * (1 + moraleMult) / 1.5;
                        double prev = weight;
                        weight = Math.Pow(weight, pow);
                        if (double.IsInfinity(weight))
                        {
                            Debug.WriteLine("!!! weight overflow");
                            weight = double.MaxValue;
                        }
                    }
                    if (!limitMove)
                        Inc(ref moveWeight, 2, false);
                    Inc(ref attWeight, Math.Sqrt(aggression));
                    Inc(ref pathWeight, Math.Sqrt(aggression));
                    Inc(ref coreWeight, Math.Sqrt(aggression));
                    switch (state)
                    {
                        case AIState.Retreat:
                            Inc(ref attWeight, 1 / 4.5);
                            Inc(ref pathWeight, 3, false);
                            Inc(ref playerAttWeight, 4, false);
                            //Inc(ref repairWeight, 5);
                            Inc(ref defWeight, 4);
                            Inc(ref terrainWeight, 1 / 1.5);
                            break;
                        case AIState.Patrol:
                            Inc(ref moveWeight, 3, false);
                            Inc(ref terrainWeight, 2, false);
                            goto case AIState.Fight;
                        case AIState.Fight:
                        case AIState.Harass:
                            Inc(ref attWeight, 4);
                            Inc(ref playerAttWeight, 2, false);
                            Inc(ref defWeight, 3);
                            break;
                        case AIState.Rush:
                            if (seeCore)
                            {
                                Inc(ref attWeight, 3);
                                Inc(ref playerAttWeight, 1 / 2.5, false);
                                Inc(ref defWeight, 2);
                            }
                            else
                            {
                                Inc(ref pathWeight, 4);
                                Inc(ref coreWeight, 2);
                                Inc(ref terrainWeight, 1 / 3.5, false);
                            }
                            Inc(ref coreWeight, 2);
                            goto case AIState.Fight;
                        default: throw new Exception();
                    }

                    double[] weights = [attWeight, pathWeight, coreWeight, playerAttWeight, moveWeight, defWeight, terrainWeight,];//repairWeight,
                    double result = 1, div = 1;
                    foreach (var w in weights)
                    {

                        if (!double.IsNormal(w))
                            ;

                        double weight = w;
                        if (double.IsNaN(weight) || weight < 0)
                            throw new Exception();
                        if (weight > 1)
                            result += weight;
                        else
                            div *= weight;
                    }
                    result *= div * moveTiles.Count;

                    if (validRetreat)
                        result *= 1 + weights.Max();

                    if (result <= 0 || !double.IsNormal(result))
                        throw new Exception();

                    dictDbl.Add(moveTile, result);
                }
#pragma warning restore IDE0018 // Inline variable declaration

                double multiplier = 1;
                double min = dictDbl.Values.Min();
                if (min * multiplier < 1)
                    multiplier = 1 / min;
                double sum = dictDbl.Values.Sum();
                min = moveTiles.Count * moveTiles.Count;
                if (sum * multiplier < min)
                    multiplier = Math.Max(multiplier, min / sum);
                int max = int.MaxValue - dictDbl.Count;
                if (sum * multiplier > max)
                    multiplier = max / sum;
                Dictionary<Tile, int> dictInt = [];
                foreach (var p in dictDbl)
                    dictInt.Add(p.Key, Game.Rand.Round(p.Value * multiplier));

                moveTo = Game.Rand.SelectValue(dictInt);
            }

            if (moveTo.Piece is Portal portal)
            {
                var ported = movable?.Port(portal);
                if (movable != null && (!ported.HasValue || !ported.Value))
                    ;
            }
            else if (orig != moveTo)
            {
                if (attPiece != null)
                {
                    IKillable meleeTrg = target;
                    if (meleeTrg == null)
                    {
                        var meleeTargets = targets.Where(k => Math.Min(k.Piece.Tile.GetDistance(orig), k.Piece.Tile.GetDistance(moveTo)) <= Attack.MELEE_RANGE);
                        if (meleeTargets.Any())
                            meleeTrg = Game.Rand.SelectValue(meleeTargets, GetWeight);
                    }

                    Fire((meleeTrg?.Piece.Tile.GetDistance(moveTo) ?? 0) > Attack.MELEE_RANGE);
                }

                if (!movable.EnemyMove(moveTo))
                    ;

                playerAttacks.TryGetValue(orig, out double p);
                playerAttacks.TryGetValue(piece.Tile, out double c);
                if (p != c)
                {
                    double moraleMult = MoraleMult(p, c);
                    moraleMult *= moraleMult;
                    Alien.IncMorale(piece, moraleMult, false, 2.6, orig, piece.Tile);
                }
            }

            Fire(true);

            moved.Add(piece);
            Alien.ModState(piece, prev, piece.State, orig, piece.Tile);

            void Fire(bool useMelee)
            {
                var attacks = GetAttacks(attPiece).Where(a => useMelee || a.Range > Attack.MELEE_RANGE);
                if (attacks.Any() && (state != AIState.Heal || Game.Rand.Bool()))
                {
                    foreach (var attack in Game.Rand.Iterate(attacks))
                    {
                        bool CanTarget(IKillable killable) => attack.GetDefenders(killable.Piece).Count > 0;
                        if (target == null || !CanTarget(target))
                        {
                            var choices = targets.Where(CanTarget);
                            if (choices.Any())
                                target = Game.Rand.SelectValue(choices, GetWeight);
                        }

                        if (target != null && allTargets.TryGetValue(target, out var trgGrp) && trgGrp.Count > 0)
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
                            bool retreat = state == AIState.Retreat;
                            int mod = Game.Rand.Round(trgGrp.Sum(p => Attack.TerrainAttMod(attack.Piece.Tile, p.Key.Piece.Tile) * p.Value) / trgGrp.Values.Sum());
                            if (!(IsFull(attack) || attack.AttackCur + mod > Game.Rand.RangeInt(0, defense)
                                    || (playerAttacks.TryGetValue(piece.Tile, out double att)
                                    && att > Game.Rand.Gaussian(retreat ? Game.Rand.DoubleHalf(defValue) : defValue, .169))))
                                continue;

                            double prevDef = trgGrp.Keys.Sum(k => k.CurDefenseValue);
                            List<Tuple<Tile, double>> trgAttacks = [.. trgGrp.Keys.SelectMany(k => PlayerAttacks(k.GetBehavior<IAttacker>()))];
                            foreach (var pair in trgAttacks)
                                playerAttacks[pair.Item1] -= pair.Item2;
                            if (attPiece.EnemyFire(target, attack))
                            {
                                double curDef = trgGrp.Keys.Sum(k => k.CurDefenseValue);
                                bool kill = trgGrp.Keys.Any(k => k.Dead);
                                double moraleMult = MoraleMult(curDef, prevDef);
                                if (kill)
                                    moraleMult *= moraleMult;
                                Alien.IncMorale(piece, moraleMult, false, 1.69, [.. trgGrp.Keys.Select(k => killPiece.Piece.Tile), orig, piece.Tile]);

                                //fully re-load all targets since this kill could affect target grouping
                                if (kill)
                                    allTargets = GetAllTargets(game);
                                else //handle corner case where splash damage kills a passive defender
                                    foreach (var pair in Game.Rand.Iterate(allTargets))
                                        if (pair.Key.Dead)
                                            allTargets.Remove(pair.Key);
                            }
                            else if (CanTarget(target))
                            { }
                            foreach (var killable in trgGrp.Keys)
                                if (!killable.Dead)
                                    foreach (var t in PlayerAttacks(killable.GetBehavior<IAttacker>()))
                                        playerAttacks[t.Item1] += t.Item2;
                        }
                        else if (piece is Alien && target != null && !target.Dead)
                        { }
                    }
                }
            }

            return allTargets;

            double MoraleMult(double prev, double cur)
            {
                double moraleMult = (cur - prev) / Math.Sqrt((cur + prev) / 2.0 * defValue);
                if (moraleMult < 0)
                    moraleMult = 1 / (-moraleMult + 1);
                else
                    moraleMult++;
                return moraleMult;
            }
        }

        private static IEnumerable<Attack> GetAttacks(IAttacker attacker) =>
            attacker?.Attacks.Where(a => a.CanAttack()) ?? [];
        private static double SumAttacks(IEnumerable<Attack> attacks, Tile from, Tile to) => SumAttacks(attacks, from, to, _ => true);
        private static double SumAttacks(IEnumerable<Attack> attacks, Tile from, Tile to, Func<Attack, bool> Predicate) =>
            attacks?.Where(Predicate).Sum(a => AttWeight(a, from, to) ?? 0) ?? 0;
        private static double? AttWeight(Attack a, Tile from, Tile to)
        {
            int mod = Attack.TerrainAttMod(from, to);
            int att = Consts.ModAtt(a.AttackCur, mod);
            double attPct = a.AttackCur / (double)a.AttackMax;
            return Consts.StatValue(att) * Math.Sqrt(attPct) * (IsFull(a) ? 2 : 1);
        }
        private static bool IsFull(Attack a) => a.AttackCur == a.AttackMax || (a.Reload < 1 && Game.Rand.Bool(1 - a.Reload))
            || a.AttackCur + Game.Rand.RangeInt(0, Game.Rand.Round(a.Reload * (a.Type == CombatTypes.AttackType.Energy ? 2 : 1))) > a.AttackMax;
        private static double DefWeight(IKillable k) => k?.CurDefenseValue ?? 0;

        private static double GetKillWeight(IKillable killable, double avgHp, AIState? state, IKillable target)
        {
            bool inFight = state == AIState.Fight;
            double attacks = 1, repair = 1;

            if (killable.Piece.HasBehavior(out IAttacker attacker))
                attacks += attacker.Attacks.Sum(AttValue);
            if (killable.Piece.HasBehavior(out IMissileSilo silo))
                attacks += AttValue(silo.SampleAttack) * Math.Sqrt(silo.NumMissiles) / 3.0;
            static double AttValue(Attack a) => Consts.StatValue(a.AttackCur) * (a.Range == Attack.MELEE_RANGE ? 2 : 1)
                * (Math.Max(a.Range, Attack.MIN_RANGED) + Attack.MIN_RANGED) / Attack.MIN_RANGED / 3.0;

            double ConstructorValue(double range) => avgHp * (range + 21) / 9.1;
            if (killable.Piece.HasBehavior(out IRepair repairs))
                repair += ConstructorValue(repairs.Range) * (repairs.Rate + 1) * (inFight ? 3 : 1);
            if (killable.Piece.HasBehavior(out IBuilder builder))
            {
                double buildTrg = ConstructorValue(builder.Range) * (state == AIState.Harass ? 3 : 1); ;
                if (killable.Piece.HasBehavior<IBuilder.IBuildDrone>())
                    buildTrg *= inFight ? 9 : 3;
                repair += buildTrg;

                if (killable.Piece is Drone drone)
                {
                    double turnMult = (drone.Turns + 2.6) / 16.9;
                    if (turnMult > 1)
                        turnMult = Math.Sqrt(turnMult);
                    else
                        turnMult *= turnMult;
                    repair *= turnMult;
                }
            }

            double mass = 1;
            if (killable.Piece is PlayerPiece playerPiece)
            {
                Consts consts = killable.Piece.Game.Consts;
                double e, r;
                e = r = 0;
                playerPiece.GenerateResources(ref e, ref mass, ref r);
                if (killable.Piece is Core && state == AIState.Rush)
                {
                    const double div = 3;
                    e += consts.CoreEnergy / div;
                    mass += consts.CoreMass / div;
                    r += consts.CoreResearch / div;
                }
                mass += e / consts.EnergyMassRatio + r * consts.MassPerResearchConversion;
            }
            double massDiv = state switch
            {
                AIState.Harass => 9,
                AIState.Rush => 21,
                AIState.Fight => 250,
                _ => 39
            };
            mass = avgHp * (Math.Sqrt(1 + mass / massDiv) - 1);

            double shieldFactor = 1;
            foreach (var def in killable.AllDefenses)
            {
                double pow = def.Type switch
                {
                    CombatTypes.DefenseType.Hits => .75,
                    CombatTypes.DefenseType.Shield => 1.5,
                    _ => 1,
                };
                shieldFactor *= Math.Pow(Consts.StatValue(def.DefenseMax + 1) / Consts.StatValue(def.DefenseCur + 1), pow);
            }
            shieldFactor = Math.Pow(shieldFactor, 1.0 / killable.AllDefenses.Count);

            double defCur = killable.CurDefenseValue;
            double defMax = killable.MaxDefenseValue;
            if (!inFight && !killable.HasBehavior<IAttacker>())
            {
                defCur /= 3.9;
                defMax /= 3.9;
            }

            double gc = (1 + attacks + repair + mass) / (1 + defCur);
            gc *= gc;
            double damagePct = 2 - defCur / defMax;
            damagePct *= damagePct;
            double trg = killable == target ? 6.5 : 1;
            if (killable.Piece is Core)
                if (state == AIState.Rush)
                    trg *= 2.6;
                else if (inFight)
                    trg /= 2.1;

            return Math.Sqrt(shieldFactor * (1 + gc)) * damagePct * trg;
        }
    }
}
