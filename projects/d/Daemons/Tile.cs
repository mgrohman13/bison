using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons
{
    [Serializable]
    public class Tile
    {
        public readonly Game Game;

        public readonly int X, Y;

        private readonly List<Unit> units = new List<Unit>(), attackers = new List<Unit>();

        [NonSerialized]
        private HashSet<Tile> sideNeighbors, cornerNeighbors;

        public Tile(Game game, int x, int y)
        {
            this.X = x;
            this.Y = y;

            this.Game = game;
        }

        public int NumUnits
        {
            get
            {
                return units.Count;
            }
        }

        public int NumAttackers
        {
            get
            {
                return attackers.Count;
            }
        }

        public List<ProductionCenter> GetProduction()
        {
            return Game.GetProduction(X, Y);
        }

        public List<ProductionCenter> GetProduction(bool unused)
        {
            return Game.GetProduction(X, Y, unused);
        }

        public System.Drawing.Bitmap GetBestUnit()
        {
            return GetBestPic(units);
        }

        public System.Drawing.Bitmap GetBestAttacker()
        {
            attackers.Sort(Unit.UnitComparison);
            return GetBestPic(attackers);
        }

        private static System.Drawing.Bitmap GetBestPic(IEnumerable<Unit> list)
        {
            foreach (Unit unit in list)
                if (unit.Type == UnitType.Daemon)
                    return unit.GetPic();
            foreach (Unit unit in list)
                if (unit.Type == UnitType.Knight)
                    return unit.GetPic();
            foreach (Unit unit in list)
                if (unit.Type == UnitType.Archer)
                    return unit.GetPic();
            foreach (Unit unit in list)
                if (unit.Type == UnitType.Infantry)
                    return unit.GetPic();
            foreach (Unit unit in list)
                if (unit.Type == UnitType.Indy)
                    return unit.GetPic();
            return null;
        }

        public bool Occupied()
        {
            Player player;
            return Occupied(out player);
        }

        public bool Occupied(out Player occupying)
        {
            occupying = null;
            foreach (Unit unit in units)
            {
                occupying = unit.Owner;
                return true;
            }
            return false;
        }

        internal void Add(Unit unit)
        {
            Player occupying;
            if (Occupied(out occupying) && occupying != unit.Owner)
                attackers.Add(unit);
            else
                units.Add(unit);
        }

        internal void Remove(Unit unit)
        {
            units.Remove(unit);
            attackers.Remove(unit);
            GetNewAttackers();
        }

        public List<Unit> GetUnits(Player player)
        {
            return GetUnits(player, false);
        }

        public List<Unit> GetUnits(Player player, bool hasMove)
        {
            return GetUnits(player, hasMove, null);
        }

        public List<Unit> GetUnits(Player player, bool hasMove, UnitType? unitType)
        {
            return GetUnits(player, hasMove, false, unitType);
        }

        public List<Unit> GetUnits(Player player, bool hasMove, bool healed)
        {
            return GetUnits(player, hasMove, healed, null);
        }

        public List<Unit> GetUnits(Player player, bool hasMove, bool healed, UnitType? unitType)
        {
            List<Unit> result = new List<Unit>();
            foreach (Unit unit in GetAllUnits())
                if (unit.Owner == player && ( !unitType.HasValue || unit.Type == unitType.Value )
                        && ( !hasMove || unit.Movement > 0 ) && ( !healed || unit.Healed ))
                    result.Add(unit);
            return result;
        }

        public Unit[] GetUnits()
        {
            return units.ToArray();
        }

        public Unit[] GetAttackers()
        {
            return attackers.ToArray();
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            foreach (Unit unit in units)
                yield return unit;
            foreach (Unit unit in attackers)
                yield return unit;
        }

        public bool FightBattle()
        {
            if (this.NumAttackers > 0)
                foreach (Unit unit in GetAllUnits())
                    if (unit.Owner == Game.GetCurrentPlayer())
                    {
                        ProcessBattle();
                        return true;
                    }
            return false;
        }

        private void ProcessBattle()
        {
            Game.Log("----------------------------------------------------");

            foreach (Unit unit in GetAllUnits())
                unit.OnBattle();

            List<Unit> attDaemons = new List<Unit>();
            foreach (Unit unit in this.attackers)
                if (unit.Type == UnitType.Daemon)
                    attDaemons.Add(unit);
            List<Unit> defDaemons = new List<Unit>();
            foreach (Unit unit in this.units)
                if (unit.Type == UnitType.Daemon)
                    defDaemons.Add(unit);
            List<Unit> allUnits = new List<Unit>();
            AddUnits(allUnits, attDaemons, defDaemons, .5);
            foreach (Unit unit in Game.Random.Iterate<Unit>(allUnits))
                unit.Attack();

            bool fight = true;
            while (fight && GetNewAttackers() > 0)
            {
                Game.Log("------------- " + this.NumAttackers + " : " + this.NumUnits);

                allUnits.Clear();
                AddUnits(allUnits, this.attackers, this.units, 1);
                foreach (Unit unit in Game.Random.Iterate<Unit>(allUnits))
                    unit.Attack();

                if (this.attackers.Count > 0 && this.units.Count > 0)
                    CheckMorale();

                fight = false;
                foreach (Unit unit in GetAllUnits())
                    if (unit.Owner == Game.GetCurrentPlayer())
                    {
                        fight = true;
                        break;
                    }
            }
        }

        private void CheckMorale()
        {
            double attMorale = GetMorale(this.attackers);
            double defMorale = GetMorale(this.units);

            double mult = Math.Pow(GetAttackerStr() / GetArmyStr(), .21);
            attMorale = Mult(attMorale, mult);
            defMorale = Mult(defMorale, 1 / mult);

            bool side = ( defMorale > attMorale );
            if (side)
            {
                double temp = defMorale;
                defMorale = attMorale;
                attMorale = temp;
            }

            double chance = Math.Pow(attMorale / defMorale, .52) * Math.Pow(1 - defMorale, .91);
            if (chance > .5)
                chance /= ( chance + .5 );
            if (Game.Random.Bool(chance * chance))
            {
                if (side)
                    Retreat(this.attackers, false);
                else
                    Retreat(this.units, false);
            }
            else if (Game.Random.Bool())
            {
                if (Game.Random.Bool())
                    Retreat(this.attackers.Where((unit) => ( Game.Random.Bool() && unit.Morale < Game.Random.GaussianCapped(.169, .52) )), true);
                if (Game.Random.Bool())
                    Retreat(this.units.Where((unit) => ( Game.Random.Bool() && unit.Morale < Game.Random.GaussianCapped(.169, .52) )), true);
            }
        }
        private double Mult(double morale, double mult)
        {
            if (mult > 1)
                morale = 1 - ( 1 - morale ) / mult;
            else
                morale *= mult;
            return morale;
        }

        private static double GetMorale(IEnumerable<Unit> units)
        {
            double morale = 0, tot = 0;
            foreach (Unit unit in units)
            {
                morale += unit.Morale * unit.MaxStrength;
                tot += unit.MaxStrength;
            }
            return morale / tot;
        }

        private void Retreat(IEnumerable<Unit> units, bool force)
        {
            Tile t = null;
            foreach (Unit unit in Game.Random.Iterate(units))
                t = unit.Retreat(t, force);
        }

        private int GetNewAttackers()
        {
            Player owner;
            if (!Occupied(out owner))
                if (this.NumAttackers > 0)
                    owner = Game.GetRandom(this.attackers).Owner;
                else
                    return 0;

            foreach (Unit unit in this.GetAttackers())
                if (unit.Owner == owner)
                {
                    this.units.Add(unit);
                    this.attackers.Remove(unit);
                }

            return this.NumAttackers;
        }

        private void AddUnits(List<Unit> fightList, List<Unit> attackers, List<Unit> defenders, double dmgMult)
        {
            double attTot = GetDamage(attackers) * dmgMult;
            double defTot = GetDamage(defenders) * dmgMult;

            AddUnits(fightList, attackers, attTot);
            AddUnits(fightList, defenders, defTot);
        }

        private double GetDamage(List<Unit> units)
        {
            units.Sort(UnitDamageComparison);
            double total = 0;
            int count = -1;
            foreach (Unit unit in units)
                total += GetDamage(unit) / ( 1 + ++count / 2.6 );
            return total;
        }

        public static int UnitDamageComparison(Unit unit1, Unit unit2)
        {
            return Math.Sign(unit2.Tile.GetDamage(unit2) - unit1.Tile.GetDamage(unit1));
        }

        private void AddUnits(List<Unit> fightList, List<Unit> available, double damTot)
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

        public double GetDamage(Unit attacker)
        {
            double avg = 0, tot = 0;
            foreach (Unit defender in GetAllUnits())
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
            foreach (Unit u in GetAllUnits())
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

        public double GetArmyStr()
        {
            return GetArmyStr(this.units);
        }

        public double GetAttackerStr()
        {
            return GetArmyStr(this.attackers);
        }

        public static double GetArmyStr(List<Unit> units)
        {
            double retVal = 0;
            foreach (Unit unit in units)
                if (unit.Hits > 0)
                    retVal += unit.Strength;
            return retVal;
        }

        internal int GetRetreatValue(Player player)
        {
            double friend = GetArmyStr(GetUnits(player));
            double total = GetArmyStr() + GetAttackerStr();

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