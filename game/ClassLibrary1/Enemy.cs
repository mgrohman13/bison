using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1
{
    [Serializable]
    public class Enemy : Side
    {
        private readonly EnemyResearch _research;
        private MechBlueprint _nextAlien;
        private MechBlueprint NextAlien => _nextAlien;
        private double _portalSpawn, _debt, _payment;

        public IEnumerable<Piece> VisiblePieces => _pieces.Where(p => p.Tile.Visible);

        public IEnumerable<Tuple<Tile, Tile>> LastAttacks => PiecesOfType<EnemyPiece>().SelectMany(a => a.LastAttacks);
        public IEnumerable<Tuple<Tile, Tile>> LastMoves => PiecesOfType<EnemyPiece>().Where(a => a.LastMove != null).Select(a => Tuple.Create(a.Tile, a.LastMove));

        internal Enemy(Game game)
            : base(game, Game.Rand.Round(Consts.EnemyStartEnergy), 0)
        {
            this._research = new EnemyResearch(game);
            this._nextAlien = MechBlueprint.Alien(_research);
            this._portalSpawn = 0;
            this._debt = 0;
            this._payment = 0;
        }

        internal void PlayTurn(Action<Tile, double> UpdateProgress)
        {
            PayDebt();

            double difficulty = GetDifficulty(Game);

            bool portal = false;
            if (this._research.TypeVailable(EnemyResearch.PortalType))
                portal = BuildPortals();

            EnemyMovement.PlayTurn(Game, Math.Pow(difficulty, Consts.DifficultyAIPow), portal, UpdateProgress);

            base.EndTurn(out double energyUpk, out double massUpk);
            double energy = GetEneryIncome(Game);
            if (Game.Turn < Consts.EnemyEnergyRampTurns)
                energy *= Game.Turn / Consts.EnemyEnergyRampTurns;
            AddEnergy(Game.Rand.OEInt(energy) + Game.Rand.Round((this.Mass - massUpk) * Consts.EnergyMassRatio - energyUpk));
            this._mass = 0;

            int spawns = Game.Rand.OEInt(Game.Turn / 13.0);
            for (int a = 0; a < spawns && NextAlien.EnergyEquivalent() + 13 < this.Energy; a++)
                SpawnAlien(() => Game.Map.GetEnemyTile(Alien.GetPathFindingMovement(NextAlien.Movable)));

            Debug.WriteLine($"Enemy energy: {_energy}");

            _research.EndTurn(Math.Pow(difficulty, Consts.DifficultyResearchPow));

            //we start turn here so the player sees things in the correct state for the enemy's next moves
            base.StartTurn();
        }

        private bool BuildPortals()
        {
            PortalIncome();

            int researchLevel = Game.Rand.Round((_research.GetBlueprintLevel() + Game.Player.Research.GetBlueprintLevel()) / 2.0);

            Game.Player.GetIncome(out double energyInc, out double massInc, out double researchInc);
            double pInc = energyInc + Consts.EnergyMassRatio * (massInc + researchInc * Consts.ResearchMassConversion);
            double pRes = Game.Player.Energy + Game.Player.Mass * Consts.EnergyMassRatio;
            double pStr = Game.Player.Pieces.Sum(p => p.Strength(researchLevel, false));

            double eInc = GetEneryIncome(Game);
            double eRes = this.Energy + this.Mass * Consts.EnergyMassRatio;
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

            this._portalSpawn += Game.Rand.OE(inc);

            bool portal = false;
            double needed = hasEntrance && !hasExit ? Game.Rand.DoubleHalf() : 1;
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
            var pieces = Game.AllPieces;
            double difficulty = GetDifficulty(Game);
            if (exit)
            {
                Map.Map map = Game.Map;
                Player player = Game.Player;
                Core core = player.Core;

                //exits place near core, avoiding stronger immediate player attacks and potential turret range
                double turretRange = (new[] { UpgType.TurretRange, UpgType.TurretLaserRange, UpgType.TurretExplosivesRange, })
                    .Max(u => ResearchUpgValues.Calc(u, player.Research.ResearchCur));
                IEnumerable<FoundationPiece> turrets = player.PiecesOfType<Turret>();
                if (turrets.Any())
                    turretRange = Math.Max(turretRange,
                         turrets.Max(t => t.GetBehavior<IAttacker>().Attacks.Max(att => att.RangeBase)));

                double deviation = core.GetBehavior<IRepair>().Range + Game.Rand.Range(Attack.MELEE_RANGE, Attack.MIN_RANGED);

                double portalDef = Portal.GetDefAvg(difficulty, exit);
                var avoid = EnemyMovement.GetPlayerAttacks(Game)
                        .Where(p => Game.Rand.DoubleHalf(portalDef) < Game.Rand.DoubleFull(p.Value)).Select(p => p.Key)
                    .Concat(core.Tile.GetAllPointsInRange(deviation)
                        .Concat(player.PiecesOfType<FoundationPiece>().Select(t => t.Tile)
                            .Concat(pieces.OfType<Foundation>().Select(f => f.Tile))
                            .SelectMany(t => t.GetAllPointsInRange(turretRange)))
                        .Select(map.GetTile))
                    .Where(t => t is not null).ToHashSet();

                do
                {
                    deviation += Game.Rand.DoubleFull(Math.Sqrt(Consts.PathWidth));
                    tile = map.GetTile(core.Tile.X + Game.Rand.GaussianInt(deviation), core.Tile.Y + Game.Rand.GaussianInt(deviation));
                }
                while (tile is null || tile.Piece is not null || avoid.Remove(tile));
            }
            else
            {
                //pieces.Select(p => p.Strength());

                //entrances chosen based on prioximity to aliens and distance from player pieces or resources
                static bool CanPlace(Tile t) => t.Piece is null;
                var enemies = Pieces.Where(p => p.HasBehavior<IMovable>() && p.Tile.GetAdjacentTiles().Any(CanPlace));
                if (enemies.Any())
                {
                    Dictionary<Piece, int> select = new();
                    foreach (var piece in Game.Rand.Iterate(enemies))
                    {
                        double mult = 3.9, div = 1;
                        foreach (var check in Game.Rand.Iterate(pieces))
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

            Portal portal = Portal.NewPortal(tile, difficulty, exit, out double cost);
            AddDebt(cost);
            this._portalSpawn -= GetPct(exit);
            return true;
        }
        private static double GetPct(bool exit)
        {
            const double total = 2 * Consts.PortalEntranceDef + Consts.PortalExitDef;
            return (exit ? Consts.PortalExitDef : Consts.PortalEntranceDef) / total;
        }

        private void PortalIncome()
        {
            var portals = PiecesOfType<Portal>();
            double energy = portals.Any(p => p.Exit) ? Math.Sqrt(GetEneryIncome(Game)) : 0;
            Loan(portals.Count() * energy);
        }
        private void IncPortals(Hive hive)
        {
            const double amt = 1 / 4.0;
            double inc = 0;
            if (hive == null)
                inc = amt;
            else if (!hive.Dead)
                inc = amt / PiecesOfType<Hive>().Average(h =>
                    h.GetBehavior<IKillable>().AllDefenses.Sum(d => d.DefenseMax));
            else
                ;
            this._portalSpawn += Game.Rand.Gaussian(inc, .039 / Math.Sqrt(inc));

            Loan(13 / amt * inc * GetEneryIncome(Game));
        }
        internal void VictoryPoint() => IncPortals(null);

        internal static double GetDifficulty(Game game) =>
            (game.Turn + Consts.DifficultyIncTurns) / Consts.DifficultyIncTurns;
        internal static double GetEneryIncome(Game game) =>
            Math.Pow(GetDifficulty(game), Consts.DifficultyEnergyPow) * Consts.EnemyEnergy;

        private void Loan(double energy)
        {
            int loan = Game.Rand.GaussianOEInt(energy, .13, .13);
            AddEnergy(loan);
            AddDebt(loan);
        }
        private void AddDebt(double loan)
        {
            this._debt += loan;
        }
        private void PayDebt()
        {
            double inc = Math.Sqrt(GetEneryIncome(Game));

            double interest = Math.Sqrt(_debt + 1) - 1;
            AddDebt(interest);

            double payment = Math.Min(_debt, _payment);
            ;
            if (Math.Min(interest, payment) > Game.Rand.DoubleFull(inc))
            {
                Spend(Game.Rand.Round(payment), 0);
                AddDebt(-payment);
            }

            double trgPayment = (1 + interest) * inc;
            this._payment = Math.Max(0, _payment + Game.Rand.DoubleHalf(_payment < trgPayment ? inc : -inc));
        }

        internal void HiveDamaged(Hive hive, Tile defTile, Map.Map.SpawnChance spawn, ref double energy,
            int hits, double hitsPct, double dev)
        {
            IncPortals(hive);

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
                    int RandCoord(double coord) => Game.Rand.Round(coord + Game.Rand.Gaussian(dev));
                    do
                    {
                        tile = Game.Map.GetTile(RandCoord(defTile.X), RandCoord(defTile.Y));
                        dev += Game.Rand.DoubleFull(Consts.CavePathSize);
                    }
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
            }

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

            double energy = NextAlien.EnergyEquivalent();
            this._energy -= Game.Rand.Round(energy);
            Alien.NewAlien(tile, path, energy, NextAlien.Killable, NextAlien.Resilience, NextAlien.Attacker, NextAlien.Movable);
            value = null;
            GenAlien();

            return energy;
        }
        internal override bool Spend(int energy, int mass)
        {
            this._energy = Game.Rand.Round(this.Energy - energy - mass * Consts.EnergyMassRatio);
            return true;
        }
    }
}
