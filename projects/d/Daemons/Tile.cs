using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Daemons
{
    [Serializable]
    public class Tile
    {
        public readonly Game Game;

        public readonly int X, Y;

        private readonly List<Unit> _units;

        [NonSerialized]
        private HashSet<Tile> sideNeighbors, cornerNeighbors;

        public Tile(Game game, int x, int y)
        {
            this.Game = game;

            this.X = x;
            this.Y = y;

            this._units = new List<Unit>();
        }

        public IEnumerable<ProductionCenter> GetProduction(bool unused = false)
        {
            return this.Game.GetProduction(this.X, this.Y, unused);
        }

        public static System.Drawing.Bitmap GetBestPic(IEnumerable<Unit> list)
        {
            if (list == null)
                return null;
            Unit best = list.FirstOrDefault(unit => unit.Type == UnitType.Daemon)
                    ?? list.FirstOrDefault(unit => unit.Type == UnitType.Knight)
                    ?? list.FirstOrDefault(unit => unit.Type == UnitType.Archer)
                    ?? list.FirstOrDefault(unit => unit.Type == UnitType.Infantry)
                    ?? list.FirstOrDefault(unit => unit.Type == UnitType.Indy);
            if (best == null)
                return null;
            return best.GetPic();
        }

        public bool Unoccupied(Player forPlayer)
        {
            return ( !GetUnits().Any(unit => unit.Owner != forPlayer) );
        }

        internal void Add(Unit unit)
        {
            this._units.Add(unit);
        }
        internal void Remove(Unit unit)
        {
            this._units.Remove(unit);
        }
        public IEnumerable<Unit> GetUnits(Player player = null, bool move = false, bool healed = false, UnitType? type = null)
        {
            return this._units.Where(unit => ( ( player == null || unit.Owner == player ) && ( !move || unit.Movement > 0 ) && ( !healed || unit.Healed )
                    && ( !type.HasValue || unit.Type == type.Value ) ));
        }

        public IEnumerable<IGrouping<Player, Unit>> GetPlayerUnits(Func<Unit, bool> predicate = null)
        {
            IEnumerable<Unit> units = GetUnits();
            if (predicate != null)
                units = units.Where(predicate);
            return units.GroupBy(unit => unit.Owner);
        }

        public bool CanBattle()
        {
            bool player = false;
            int count = 0;
            foreach (IGrouping<Player, Unit> grouping in GetPlayerUnits())
            {
                player |= grouping.Key.IsTurn();
                ++count;
                if (player && count > 1)
                    return true;
            }
            return false;
        }
        public bool FightBattle()
        {
            if (CanBattle())
            {
                this.Game.Log("----------------------------------------------------");

                foreach (Unit unit in GetUnits())
                    unit.OnBattle();

                Fight(GetPlayerUnits(unit => unit.Type == UnitType.Daemon), .5);

                Dictionary<Player, Tile> retreat = new Dictionary<Player, Tile>();

                while (CanBattle())
                {
                    this.Game.Log(GetPlayerUnits().Aggregate("------------- ", (log, group) => ( log +
                            string.Format("{3} {0}/{1} ({2}) : ", GetArmyStr(group).ToString("0"), group.Count(),
                            GetMorale(group).ToString("0%"), group.Key) )).TrimEnd(':', ' '));

                    Fight(GetPlayerUnits(), 1);

                    if (CanBattle())
                        CheckMorale(retreat);
                }

                return true;
            }
            return false;
        }
        private void CheckMorale(Dictionary<Player, Tile> retreat)
        {
            if (Game.Random.Bool(.78))
                foreach (IGrouping<Player, Unit> group in Game.Random.Iterate(GetPlayerUnits()))
                    if (Game.Random.Bool(.78))
                        Retreat(group.Where(unit => ( Game.Random.Bool(.78) && unit.Morale < Game.Random.GaussianCapped(.169, .65) )), retreat);

            double totalStr = GetArmyStr(GetUnits());
            var morale = GetPlayerUnits().Select(group => new Tuple<Player, double>(group.Key, GetMorale(group, totalStr)))
                    .OrderBy(tuple => tuple.Item2).ToList();

            if (morale.Any())
            {
                Tuple<Player, double> high = morale.Last();
                foreach (Tuple<Player, double> low in Game.Random.Iterate(morale))
                {
                    double chance = GetRetreatChance(low.Item2, high.Item2);
                    this.Game.Log(low.Item1 + ": " + chance.ToString("0%") + " (" + low.Item2.ToString("0%") + ")");
                    if (Game.Random.Bool(chance))
                        Retreat(GetUnits(low.Item1), retreat);
                }
            }
        }
        private static double GetRetreatChance(double morale, double high)
        {
            if (morale == high)
                return 0;
            double chance = Math.Pow(high / morale, .52) * Math.Pow(1 - morale, .91);
            if (double.IsInfinity(chance))
                chance = 1;
            else if (chance > .5)
                chance /= ( chance + .5 );
            chance *= chance;
            if (double.IsInfinity(chance) || double.IsNaN(chance) || chance <= 0 || chance > 1)
            {
            }
            return chance;
        }
        private double GetMorale(IGrouping<Player, Unit> g, double totalStr)
        {
            double str = GetArmyStr(g);
            return MultMorale(GetMorale(g), Math.Pow(str / ( totalStr - str ), .21));
        }
        private double MultMorale(double morale, double mult)
        {
            if (mult > 1)
                morale = 1 - ( 1 - morale ) / mult;
            else
                morale *= mult;
            return morale;
        }
        public static double GetMorale(IEnumerable<Unit> units)
        {
            double morale = 0, tot = 0;
            foreach (Unit unit in units)
            {
                morale += unit.Morale * unit.StrengthMax;
                tot += unit.StrengthMax;
            }
            return morale / tot;
        }
        private void Retreat(IEnumerable<Unit> units, Dictionary<Player, Tile> retreat)
        {
            units = units.ToList();

            IEnumerable<Unit> log = units.Where(unit => unit.Movement + unit.ReserveMovement > 0);
            if (log.Any())
                this.Game.Log(log.First().Owner + " retreated " + log.Count());

            if (units.Any())
            {
                Player player = units.First().Owner;
                Tile tile;
                retreat.TryGetValue(player, out tile);
                foreach (Unit unit in Game.Random.Iterate(units))
                    tile = unit.Retreat(tile);
                retreat[player] = tile;
            }
        }

        private void Fight(IEnumerable<IGrouping<Player, Unit>> players, double dmgMult)
        {
            IEnumerable<Unit> fightList = Enumerable.Empty<Unit>();
            foreach (IGrouping<Player, Unit> group in players)
                fightList = fightList.Concat(GetFightUnits(group, GetDamage(group) * dmgMult));
            foreach (Unit unit in Game.Random.Iterate(fightList))
                unit.Attack();
        }
        private IEnumerable<Unit> GetFightUnits(IEnumerable<Unit> available, double damTot)
        {
            foreach (Unit unit in Game.Random.Iterate<Unit>(available))
            {
                double damage = GetDamage(unit);
                if (damTot <= 0 || ( damTot < damage && !Game.Random.Bool(damTot / damage) ))
                    yield break;
                yield return unit;
                damTot -= damage;
            }
            if (damTot > 0)
                throw new Exception();
        }

        private double GetDamage(IEnumerable<Unit> units)
        {
            IEnumerable<double> strengths = units.Select(unit => GetDamage(unit)).OrderByDescending(d => d);
            double total = 0, count = -1;
            foreach (double damage in strengths)
                total += ( damage / ( ++count / 2.6 + 1.0 ) );
            return total;
        }

        public double GetDamage(Unit attacker)
        {
            double avg = 0, tot = 0;
            foreach (Unit defender in GetUnits().Where(unit => attacker.Owner != unit.Owner))
            {
                double mult = Unit.GetDamageMult(attacker.Type, defender.Type);
                tot += mult;
                avg += mult * mult;
            }
            if (tot > 0)
                avg /= tot;
            else
                avg = 1;
            return attacker.Damage * avg;
        }

        internal Unit GetBestTarget(Unit attacker)
        {
            return GetTarget(attacker, 0);
        }

        internal Unit GetTarget(Unit attacker, bool archery)
        {
            return GetTarget(attacker, archery ? .21 : ( attacker.Type == UnitType.Archer ? .26 : .52 ));
        }

        private Unit GetTarget(Unit attacker, double deviation)
        {
            IEnumerable<Unit> targets = GetUnits().Where(unit => unit.Owner != attacker.Owner);
            if (targets.Any())
            {
                var groups = targets.GroupBy(unit => unit.Owner);
                Unit target = null;
                double min = double.MaxValue;
                foreach (Unit defender in targets)
                {
                    double tartgetVal = defender.TargetFactor;
                    if (groups.Skip(1).Any())
                        tartgetVal /= Math.Pow(GetArmyStr(groups.First(group => group.Key == defender.Owner)), .26);
                    tartgetVal /= Unit.GetDamageMult(attacker.Type, defender.Type);
                    tartgetVal = Game.Random.GaussianCapped(tartgetVal, deviation);
                    if (tartgetVal < min)
                    {
                        min = tartgetVal;
                        target = defender;
                    }
                }
                return target;
            }
            return null;
        }

        public static double GetArmyStr(IEnumerable<Unit> units)
        {
            return units.Sum(unit => unit.Strength);
        }

        internal int GetRetreatValue(Player player)
        {
            double friend = GetArmyStr(GetUnits(player)) + 13;
            double total = GetArmyStr(GetUnits()) + 26;

            double amt = 169;
            if (friend + 13 == total)
                amt *= 13;
            amt += friend * friend * Math.Pow(friend / total, 3.9) * 13;
            return Game.Random.Round(amt);
        }

        internal IEnumerable<Tile> GetSideNeighbors()
        {
            if (this.sideNeighbors == null)
                this.Game.CreateNeighborReferences();
            return this.sideNeighbors;
        }
        internal IEnumerable<Tile> GetCornerNeighbors()
        {
            if (this.cornerNeighbors == null)
                this.Game.CreateNeighborReferences();
            return this.cornerNeighbors;
        }

        public bool IsSideNeighbor(Tile tile)
        {
            if (this.sideNeighbors == null)
                this.Game.CreateNeighborReferences();
            return this.sideNeighbors.Contains(tile);
        }

        public bool IsCornerNeighbor(Tile tile)
        {
            if (this.cornerNeighbors == null)
                this.Game.CreateNeighborReferences();
            return this.cornerNeighbors.Contains(tile);
        }

        public bool IsNeighbor(Tile t)
        {
            return ( IsSideNeighbor(t) || IsCornerNeighbor(t) );
        }

        public static void CreateNeighborReferences(Tile[,] map, int width, int height)
        {
            foreach (Tile tile in map)
                tile.SetupNeighbors(map, width, height);
        }

        private void SetupNeighbors(Tile[,] map, int width, int height)
        {
            this.sideNeighbors = new HashSet<Tile>();
            this.cornerNeighbors = new HashSet<Tile>();
            for (int a = 0 ; a < 8 ; a++)
            {
                HashSet<Tile> neighbors = ( a < 4 ? this.sideNeighbors : this.cornerNeighbors );
                Tile tile = GetTileIn(a, map, width, height);
                if (tile != null)
                    neighbors.Add(tile);
            }
        }
        private Tile GetTileIn(int direction, Tile[,] map, int width, int height)
        {
            int x = this.X;
            int y = this.Y;
            switch (direction)
            {
            case 0:
                y--;
                break;
            case 1:
                x--;
                break;
            case 2:
                x++;
                break;
            case 3:
                y++;
                break;
            case 4:
                x--;
                y--;
                break;
            case 5:
                x++;
                y--;
                break;
            case 6:
                x--;
                y++;
                break;
            case 7:
                x++;
                y++;
                break;
            default:
                throw new Exception();
            }
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            else
                return map[x, y];
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", this.X, this.Y);
        }
    }
}