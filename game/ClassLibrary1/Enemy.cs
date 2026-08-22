using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Enemy : Side
    {
        private readonly EnemyResearch _research;
        private MechBlueprint _nextAlien;
        private MechBlueprint NextAlien => _nextAlien;
        private double _portalSpawn, _aggression, _debt, _payment;

        internal IResearch Research => _research;

        public IEnumerable<Piece> VisiblePieces => _pieces.Where(p => p.Tile.Visible);

        public IEnumerable<Tuple<Tile, Tile>> LastAttacks => PiecesOfType<EnemyPiece>().SelectMany(a => a.LastAttacks);
        public IEnumerable<Tuple<Tile, Tile>> LastMoves => PiecesOfType<EnemyPiece>().Where(a => a.LastMove != null).Select(a => Tuple.Create(a.Tile, a.LastMove));

        internal Enemy(Game game)
            : base(game, Game.Rand.Round(game.Consts.EnemyStartEnergy), 0)
        {
            this._research = new EnemyResearch(game);
            this._nextAlien = MechBlueprint.Alien(game.Consts, _research);
            this._portalSpawn = 0;
            this._aggression = 1;
            this._debt = 0;
            this._payment = 0;
        }
        internal void NewGame()
        {
            SpawnAlien();
        }

        internal void PlayTurn(Action<Tile, double> UpdateProgress, double playerIncome)
        {
            PayDebt();

            bool portal = false;
            if (this._research.TypeVailable(EnemyResearch.PortalType))
                portal = BuildPortals();

            double difficulty = GetDifficulty();
            EnemyMovement.PlayTurn(Game, Math.Pow(difficulty, Game.Consts.DifficultyAIPow), _aggression, portal, UpdateProgress);
            DecAggression();

            base.EndTurn(out double energyUpk, out double massUpk);

            Debug.WriteIf(this.Mass != 0, "Enemy mass: " + this.Mass);
            AddResources((this.Mass - massUpk) * Game.Consts.EnergyMassRatio - energyUpk);
            this._mass = 0;

            Income(GetEnergyIncome());
            Income(GetPlayerIncMatch(playerIncome));

            RandIncome();

            double avg = Energy / Game.Consts.SpawnEnergyDiv
                + Math.Sqrt(Game.Turn + Game.Consts.SpawnTurnAdd) / Game.Consts.SpawnTurnDiv - Game.Consts.SpawnNeg;
            int spawns = Game.Rand.OEInt(avg);
            Debug.WriteLine($"Turn {Game.Turn}, Energy: {_energy}, Spawns: {spawns} ({(float)avg})");
            for (int a = 0; a < spawns && NextAlien.EnergyEquivalent(Game.Consts) + 13 < this.Energy; a++)
                SpawnAlien();


            _research.EndTurn(Math.Pow(difficulty, Game.Consts.DifficultyResearchPow));

            //we start turn here so the player sees things in the correct state for the enemy's next moves
            base.StartTurn();
        }

        private bool BuildPortals()
        {
            PortalIncome();

            int researchLevel = Game.Rand.Round((_research.GetBlueprintLevel() + Game.Player.Research.GetBlueprintLevel()) / 2.0);

            Game.Player.GetIncome(out double energyInc, out double massInc, out int researchInc);
            double pInc = EnergyEquivalent(energyInc, massInc, researchInc);
            double pRes = Game.Player.Energy + Game.Player.Mass * Game.Consts.EnergyMassRatio;
            double pStr = Game.Player.Pieces.Sum(p => p.Strength(researchLevel, false));

            double eInc = GetEnergyIncome() + GetPlayerIncMatch(pInc) - GetPayment();
            double eRes = this.Energy + this.Mass * Game.Consts.EnergyMassRatio - _debt;
            double eStr = this.Pieces.Sum(p => p.Strength(researchLevel, false));

            pStr += pRes;
            eStr += eRes;

            eInc *= Game.Consts.DifficultySetting;
            pStr *= Game.Consts.PortalSpawnStrMult;

            eStr = Math.Max(0, eStr);
            pStr = Math.Max(0, pStr);
            eInc = Math.Max(0, eInc);
            pInc = Math.Max(0, pInc);

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

            var portals = PiecesOfType<Portal>();
            bool hasEntrance = portals.Any(p => !p.Exit);
            bool hasExit = portals.Any(p => p.Exit);
            double count = portals.Sum(p => Consts.StatValue(p.GetBehavior<IKillable>().Hits.DefenseCur)) / Game.Consts.PortalExitDef + 1;
            if (count > 2)
                count *= count - 1;
            inc /= count;

            this._portalSpawn += Game.Rand.OE(inc / Game.Consts.PortalSpawnTime);

            bool portal = false;
            double needed = 1;
            if (hasExit && !hasEntrance)
                needed = .5 - Game.Rand.OE();
            else if (hasEntrance && !hasExit)
                needed = Game.Rand.DoubleHalf();
            if (_portalSpawn > needed)
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
            Player player = Game.Player;
            Core core = player.Core;
            Tile coreTile = core.Tile;
            var pieces = Game.AllPieces;
            double difficulty = GetDifficulty();
            if (exit)
            {
                Map.Map map = Game.Map;

                //exits place near core, avoiding stronger immediate player attacks and potential turret range
                double turretRange = (new[] { UpgType.TurretRange, UpgType.TurretLaserRange, UpgType.TurretExplosivesRange, })
                    .Max(u => Game.ResearchUpgValues.Calc(u, ClassLibrary1.Research.GetResearchMult(Game.Consts, player.Research.ResearchCur)));
                IEnumerable<FoundationPiece> turrets = player.PiecesOfType<Turret>();
                if (turrets.Any())
                    turretRange = Math.Max(turretRange,
                         turrets.Max(t => t.GetBehavior<IAttacker>().Attacks.Max((Func<Attack, double>)(att => att.RangeBase))));

                double dev = core.GetBehavior<IRepair>().Range + Game.Rand.Range(Attack.MELEE_RANGE, Attack.MIN_RANGED);

                double portalDef = Portal.GetDefAvg(Game.Consts, difficulty, exit);
                var avoid = EnemyMovement.GetPlayerAttacks(Game)
                        .Where(p => Game.Rand.DoubleHalf(portalDef) < Game.Rand.DoubleFull(p.Value)).Select(p => p.Key)
                    .Concat(coreTile.GetPointsInRange(dev)
                        .Concat(player.PiecesOfType<FoundationPiece>().Select(t => t.Tile)
                            .Concat(pieces.OfType<Foundation>().Select(f => f.Tile))
                            .SelectMany(t => t.GetPointsInRange(turretRange)))
                        .Concat(PiecesOfType<Hive>().SelectMany(h => h.Tile.GetPointsInRange(
                            h.GetBehavior<IAttacker>().Attacks.Max(a => a.Range * Game.Rand.Range(1, 2)))))
                        .Select(map.GetTile))
                    .Where(t => t != null).ToHashSet();

                dev += Game.Rand.DoubleHalf(Math.Sqrt(Game.Consts.PathWidth));
                tile = map.RandTile(coreTile.LocationD, dev, Valid: t => !avoid.Remove(t));
            }
            else
            {
                //pieces.Select(p => p.Strength());

                //entrances chosen based on prioximity to aliens and distance from player pieces or resources
                static bool CanPlace(Tile t) => t.Piece == null;
                Dictionary<Piece, int> select = [];
                foreach (EnemyPiece piece in Game.Rand.Iterate(PiecesOfType<EnemyPiece>()))
                    if (piece is not Portal && piece.HasBehavior<IMovable>())
                    {
                        Tile portalTile = piece.Tile;
                        if (portalTile.GetAdjacentTiles().Any(CanPlace)
                            && PiecesOfType<Portal>().All(p => p.Tile.GetDistance(portalTile) > Game.Rand.GaussianCapped(Game.Consts.PortalMinDist, .13, Portal.AvgRange * 3.9)))
                        {
                            double mult = 2.6, div = 1;
                            if (piece.State == EnemyPiece.AIState.Rush)
                                mult *= mult;
                            foreach (var check in Game.Rand.Iterate(pieces))
                                if (piece != check)
                                {
                                    double factor = 2.1 * Game.Consts.PathWidth / (Game.Consts.CavePathWidth + portalTile.GetDistance(check.Tile));
                                    factor *= factor * (check is EnemyPiece enemy && enemy.State == EnemyPiece.AIState.Rush ? factor : 1);
                                    if (check.IsEnemy && check is not Portal)
                                        mult += factor;
                                    else
                                        div += factor;
                                }
                            mult /= div;
                            mult *= portalTile.GetDistance(coreTile) / Game.Consts.PortalMinDist;
                            mult *= mult;
                            select.Add(piece, Game.Rand.Round(mult + 1));
                        }
                    }
                if (select.Count > 0)
                    tile = Game.Rand.SelectValue(Game.Rand.SelectValue(select).Tile.GetAdjacentTiles().Where(CanPlace));
                else
                    return false;
            }

            Portal portal = Portal.NewPortal(tile, difficulty, exit, out double cost);
            AddDebt(cost);
            Loan(Game.Rand.GaussianOE(IncomeReference() * Game.Consts.PortalLoan + cost * 1.69, .26, .13));

            double spawnCost = GetPct(exit);
            this._portalSpawn -= spawnCost;
            IncAggression(spawnCost / 2.1);

            foreach (Alien alien in Game.Rand.Iterate(PiecesOfType<Alien>()))
            {
                alien.RecoverMorale();
                if (exit)
                    alien.RecoverMorale();
            }

            return true;
        }
        private double GetPct(bool exit)
        {
            double total = 2 * Game.Consts.PortalEntranceDef + Game.Consts.PortalExitDef;
            return (exit ? Game.Consts.PortalExitDef : Game.Consts.PortalEntranceDef) / total;
        }

        private void PortalIncome()
        {
            var portals = PiecesOfType<Portal>();
            double energy = portals.Any(p => p.Exit) ? IncomeReference() : 0;
            Loan(Math.Sqrt(portals.Count() / 2.0) * energy);
        }
        private void IncPortals(double inc)
        {
            if (inc > 0)
            {
                this._portalSpawn += Game.Rand.Gaussian(inc, .039 / Math.Sqrt(inc));
                Loan(26 * inc * IncomeReference());
            }
        }

        internal void VictoryPoint() => IncPortals(1.0 / Game.POINTS_TO_WIN);

        private double GetDifficulty() =>
            (Game.Turn + Game.Consts.DifficultyIncTurns) / Game.Consts.DifficultyIncTurns;

        private double GetEnergyIncome() =>
            Math.Pow(GetDifficulty(), Game.Consts.DifficultyEnergyPow) * Game.Consts.EnemyEnergy * Math.Min(Game.Turn / Game.Consts.EnemyEnergyRampTurns, 1);
        private double GetPlayerIncMatch(double playerIncome) =>
            playerIncome * playerIncome / (playerIncome + Game.Consts.EnemyIncomeMatchFactor);

        internal double IncomeReference()
        {
            double energyInc, massInc, researchInc;
            energyInc = massInc = researchInc = 0;
            Game.Player.Core.GenerateResources(ref energyInc, ref massInc, ref researchInc);
            double energy = GetEnergyIncome() + GetPlayerIncMatch(EnergyEquivalent(energyInc, massInc, researchInc));
            return energy * Game.Consts.DifficultySetting;
        }
        internal double EnergyEquivalent(double energyInc, double massInc, double researchInc) =>
            energyInc + Game.Consts.EnergyMassRatio * (massInc + researchInc * Game.Consts.MassPerResearchConversion);

        private void Loan(double energy)
        {
            int loan = Game.Rand.GaussianOEInt(energy, .13, .13);
            AddResources(loan);
            AddDebt(loan);
        }
        private void AddDebt(double loan)
        {
            this._debt += loan;
        }
        private void PayDebt()
        {
            double inc = Math.Sqrt(IncomeReference());

            double interest = Math.Sqrt(_debt + 1) - 1;
            AddDebt(interest);
            double payment = GetPayment();
            if (Math.Min(interest, payment) > Game.Rand.DoubleFull(inc))
            {
                AddResources(-payment);
                AddDebt(-payment);
            }

            double trgPayment = (1 + interest) * inc;
            this._payment = Math.Max(0, _payment + Game.Rand.DoubleHalf(_payment < trgPayment ? inc : -inc));
        }
        private double GetPayment() => Math.Min(_debt, _payment);

        internal void HiveDamaged(Hive hive, Tile defTile, Map.Map.SpawnChance spawn, ref double energy,
            int hits, double hitsPct, double dev)
        {
            if (!Game.GameOver)
            {
                double inc = Game.Victory + PiecesOfType<Hive>().Select(h => h.GetBehavior<IKillable>())
                    .Sum(k => (k.MaxDefenseValue - k.CurDefenseValue) / k.MaxDefenseValue);
                double ramp = Game.POINTS_TO_WIN / 2.1;
                if (inc > ramp)
                {
                    ramp = 1 + (inc - ramp) * 1.69 / (Game.POINTS_TO_WIN - ramp);
                    inc *= ramp *= ramp;
                }
                double div = hive.Dead ? 1 : hive.GetBehavior<IKillable>().AllDefenses.Sum(d => d.DefenseMax);
                inc /= Game.POINTS_TO_WIN * div;

                IncAggression(inc);
                IncPortals(inc);
            }

            hitsPct = 1 - hitsPct;
            int xfer;
            if (hive.Dead)
            {
                xfer = Game.Rand.Round(energy);
                hitsPct = 1;
            }
            else
            {
                xfer = Game.Rand.GaussianInt(energy * hitsPct, 1);
                hitsPct /= Math.Sqrt(hits);
            }
            AddResources(xfer);
            energy -= xfer;
            Debug.WriteLine($"Enemy energy: {_energy} ({(xfer > 0 ? "+" : "")}{xfer})");

            if (this.Energy > 0 && Game.Rand.Bool(hitsPct / 2.0))
            {
                SpawnAlien(() =>
                {
                    Tile tile = Game.Map.RandTile(defTile.LocationD, dev);

                    while (Alien.GetPathFindingMovement(NextAlien.Movable) < Game.Map.GetMinSpawnMove(tile))
                        this._nextAlien = MechBlueprint.Alien(Game.Consts, _research);

                    return tile;
                });
                spawn.Spawned();
            }
            else
            {
                spawn.Mult(1 + hitsPct);
            }
        }
        private void IncAggression(double inc)
        {
            foreach (Alien alien in Game.Rand.Iterate(PiecesOfType<Alien>()))
            {
                int morale = Game.Rand.Round(inc);
                for (int a = 0; a < morale; a++)
                    alien.RecoverMorale();
            }
            _aggression += Game.Rand.GaussianCapped(inc * Math.Sqrt(Game.Consts.AgressionTurns), .13);
        }
        private void DecAggression()
        {
            double dec = 1 - (Game.Consts.AgressionTurns - 1) / Game.Consts.AgressionTurns;
            dec = Game.Rand.GaussianCapped(dec, .13);
            dec = 1 - dec;

            _aggression--;
            _aggression *= dec;
            _aggression++;
        }

        internal void Income(double energy) => AddResources(energy * Game.Consts.DifficultySetting);
        private void RandIncome()
        {
            double modify = Math.Min(Math.Max(0, Energy), IncomeReference());
            AddResources(Game.Rand.OEInt(modify) - modify);
        }

        private void SpawnAlien() => SpawnAlien(() => Game.Map.GetEnemyTile(Alien.GetPathFindingMovement(NextAlien.Movable)));
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
                this._nextAlien = MechBlueprint.Alien(Game.Consts, research);
            }

            if (value.HasValue)
                GenAlien();

            Tile tile;
            List<Point> path;
            while (true)
            {
                tile = GetTile();
                path = tile.Map.PathFindCore(tile, Alien.GetPathFindingMovement(NextAlien.Movable), blocked => blocked.Count == 0);
                if (path == null)
                    GenAlien();
                else
                    break;
            }

            double energy = NextAlien.EnergyEquivalent(Game.Consts);
            AddResources(-energy);
            Alien.NewAlien(tile, path, energy, NextAlien.ResearchLevel, NextAlien.Killable, NextAlien.Resilience, NextAlien.Attacker, NextAlien.Movable);
            value = null;
            GenAlien();

            return energy;
        }
        internal override void AddResources(double energy, double mass = 0) =>
            this._energy += Game.Rand.Round(energy + mass * Game.Consts.EnergyMassRatio);
    }
}
