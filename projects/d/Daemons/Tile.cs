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

        public List<ProductionCenter> GetProduction()
        {
            return Game.GetProduction(X, Y);
        }
        public List<ProductionCenter> GetProduction(bool unused)
        {
            return Game.GetProduction(X, Y, unused);
        }

        public static System.Drawing.Bitmap GetBestPic(IEnumerable<Unit> list)
        {
            Unit unit = list.FirstOrDefault((u) => ( u.Type == UnitType.Daemon ))
                    ?? list.FirstOrDefault((u) => ( u.Type == UnitType.Knight ))
                    ?? list.FirstOrDefault((u) => ( u.Type == UnitType.Archer ))
                    ?? list.FirstOrDefault((u) => ( u.Type == UnitType.Infantry ))
                    ?? list.FirstOrDefault((u) => ( u.Type == UnitType.Indy ));
            if (unit == null)
                return null;
            return unit.GetPic();
        }

        public bool Occupied(Player notBy)
        {
            return GetUnits().Any((u) => ( u.Owner != notBy ));
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
            return this._units.Where((u) => ( ( player == null || u.Owner == player ) && ( !move || u.Movement > 0 ) && ( !healed || u.Healed )
                    && ( !type.HasValue || u.Type == type.Value ) ));
        }
        public IEnumerable<IGrouping<Player, Unit>> GetPlayerUnits(Func<Unit, bool> predicate = null)
        {
            IEnumerable<Unit> units = GetUnits();
            if (predicate != null)
                units = units.Where(predicate);
            return units.GroupBy((u) => u.Owner);
        }

        public bool CanBattle()
        {
            bool player = false;
            int count = 0;
            foreach (var grouping in GetPlayerUnits())
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
                Game.Log("----------------------------------------------------");

                foreach (Unit unit in GetUnits())
                    unit.OnBattle();

                foreach (Unit unit in GetFightList(GetPlayerUnits((u) => ( u.Type == UnitType.Daemon )), .5))
                    unit.Attack();

                while (CanBattle())
                {
                    double totalStr = GetArmyStr(GetUnits());
                    Game.Log(GetPlayerUnits().Aggregate("------------- ", (l, g) => l +
                            string.Format("{3} {0}/{1} ({2}) : ", GetArmyStr(g).ToString("0"), g.Count(),
                            GetMorale(g, totalStr).ToString("0%"), g.Key)).TrimEnd(':', ' '));

                    foreach (Unit unit in GetFightList(GetPlayerUnits(), 1))
                        unit.Attack();

                    if (CanBattle())
                        CheckMorale();
                }

                return true;
            }
            return false;
        }
        private void CheckMorale()
        {
            double totalStr = GetArmyStr(GetUnits());
            var morale = GetPlayerUnits().Select((g) => new Tuple<Player, double>(g.Key, GetMorale(g, totalStr))).OrderBy((t) => t.Item2);

            Tuple<Player, double> low = morale.First();
            double chance = Math.Pow(morale.Last().Item2 / low.Item2, .52) * Math.Pow(1 - low.Item2, .91);
            if (chance > .5)
                chance /= ( chance + .5 );

            if (Game.Random.Bool(chance * chance))
                Retreat(GetUnits(low.Item1));
            else if (Game.Random.Bool(.78f))
                foreach (var g in GetPlayerUnits())
                    if (Game.Random.Bool(.78f))
                        Retreat(g.Where((unit) => ( Game.Random.Bool(.78f) && unit.Morale < Game.Random.GaussianCapped(.169, .52) )));
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
                morale += unit.Morale * unit.MaxStrength;
                tot += unit.MaxStrength;
            }
            return morale / tot;
        }
        private void Retreat(IEnumerable<Unit> units)
        {
            Tile t = null;
            foreach (Unit unit in Game.Random.Iterate(units))
                t = unit.Retreat(t);
        }

        private IEnumerable<Unit> GetFightList(IEnumerable<IGrouping<Player, Unit>> players, double dmgMult)
        {
            List<Unit> fightList = new List<Unit>();
            foreach (var group in players)
                AddFightUnits(fightList, group, GetDamage(group) * dmgMult);
            return Game.Random.Iterate(fightList);
        }
        private void AddFightUnits(List<Unit> fightList, IEnumerable<Unit> available, double damTot)
        {
            foreach (Unit unit in Game.Random.Iterate<Unit>(available))
            {
                double damage = GetDamage(unit);
                if (damTot <= 0 || ( damTot < damage && !Game.Random.Bool(damTot / damage) ))
                    return;
                damTot -= damage;
                fightList.Add(unit);
            }
            if (damTot > 0)
                throw new Exception();
        }

        private double GetDamage(IEnumerable<Unit> units)
        {
            double count = -1;
            return units.OrderByDescending((u) => GetDamage(u)).Aggregate<Unit, double>(0, (t, u) => t + ( GetDamage(u) / ( ++count / 2.6 + 1.0 ) ));
        }
        public static int UnitDamageComparison(Unit unit1, Unit unit2)
        {
            return Math.Sign(unit2.Tile.GetDamage(unit2) - unit1.Tile.GetDamage(unit1));
        }

        public double GetDamage(Unit attacker)
        {
            double avg = 0, tot = 0;
            foreach (Unit defender in GetUnits())
                if (attacker.Owner != defender.Owner)
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
            List<Unit> targets = new List<Unit>();
            foreach (Unit u in GetUnits())
                if (u.Owner != attacker.Owner)
                    targets.Add(u);

            if (targets.Count > 1)
            {
                Unit target = null;
                double min = double.MaxValue;
                foreach (Unit defender in targets)
                {
                    double tartgetVal = defender.TargetFactor;
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
            if (targets.Count == 1)
                return targets[0];
            return null;
        }

        public static double GetArmyStr(IEnumerable<Unit> units)
        {
            return units.Where((u) => ( u.Hits > 0 )).Aggregate<Unit, double>(0, (t, u) => ( t + u.Strength ));
        }

        internal int GetRetreatValue(Player player)
        {
            double friend = GetArmyStr(GetUnits(player));
            double total = GetArmyStr(GetUnits());

            double amt = 13;
            if (friend == total)
                amt *= 13;
            if (total > 0)
                amt += friend * friend * Math.Pow(friend / total, 3.9);
            return Game.Random.Round(amt);
        }

        internal IEnumerable<Tile> GetSideNeighbors()
        {
            if (sideNeighbors == null)
                Game.CreateNeighborReferences();
            return sideNeighbors;
        }
        internal IEnumerable<Tile> GetCornerNeighbors()
        {
            if (cornerNeighbors == null)
                Game.CreateNeighborReferences();
            return cornerNeighbors;
        }

        public bool IsSideNeighbor(Tile tile)
        {
            if (sideNeighbors == null)
                Game.CreateNeighborReferences();
            return sideNeighbors.Contains(tile);
        }

        public bool IsCornerNeighbor(Tile tile)
        {
            if (cornerNeighbors == null)
                Game.CreateNeighborReferences();
            return cornerNeighbors.Contains(tile);
        }

        public bool IsNeighbor(Tile t)
        {
            return ( IsSideNeighbor(t) || IsCornerNeighbor(t) );
        }

        public static void CreateNeighborReferences(Tile[,] map, int width, int height)
        {
            for (int x = 0 ; x < width ; x++)
                for (int y = 0 ; y < height ; y++)
                    map[x, y].SetupNeighbors(map, width, height);
        }

        private void SetupNeighbors(Tile[,] map, int width, int height)
        {
            sideNeighbors = new HashSet<Tile>();
            cornerNeighbors = new HashSet<Tile>();
            for (int a = 0 ; a < 8 ; a++)
            {
                HashSet<Tile> neighbors = ( a < 4 ? sideNeighbors : cornerNeighbors );
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
            return string.Format("({0},{1})", X, Y);
        }
    }
}