using System;
using System.Collections.Generic;
using System.Linq;

namespace CityWar
{
    [Serializable]
    public class City : Capturable
    {
        #region fields and constructors
        private List<string> units;

        internal City(Player owner, Tile tile)
            : base(0, owner, tile)
        {
            ability = Abilities.AircraftCarrier;
            name = "City";

            this.units = InitUnits(tile);

            tile.Add(this);
            owner.Add(this);
        }

        private const float avgChance = .8f;
        private static List<string> InitUnits(Tile tile)
        {
            var neighbors = Enumerable.Range(0, 6).Select(dir => tile.GetNeighbor(dir));
            Func<Func<Terrain, bool>, int> CountNeighbors = Predicate => neighbors
                    .Count(neighbor => neighbor != null && Predicate(neighbor.Terrain));

            double baseWaterChance = Math.Pow(1.3 + CountNeighbors(terrain => terrain == Terrain.Water), tile.Terrain == Terrain.Water ? 1.69 : 1.3);
            baseWaterChance = 1 - 1 / ( 1.0 + 13 * baseWaterChance / ( 5.2 + CountNeighbors(terrain => true) ) );

            IEnumerable<string> units = Enumerable.Empty<string>();
            foreach (IEnumerable<Unit> race in Game.Races.Select(pair => pair.Value.Select(name => Unit.CreateTempUnit(name))))
            {
                Func<Func<UnitType, bool>, int> CountUnits = Predicate => race
                        .Count(unit => unit.costType == CostType.Production && Predicate(unit.Type));

                int waterCount = CountUnits(unitType => unitType == UnitType.Water || unitType == UnitType.Amphibious);
                int otherCount = CountUnits(unitType => unitType != UnitType.Water && unitType != UnitType.Amphibious);
                double waterChance, otherChance;
                GetChances(waterCount, otherCount, baseWaterChance, out waterChance, out otherChance);

                Func<Unit, double> GetChance = unit =>
                {
                    if (unit.costType != CostType.Production)
                        return avgChance;
                    switch (unit.Type)
                    {
                    case UnitType.Air:
                    case UnitType.Ground:
                    case UnitType.Immobile:
                        return otherChance;
                    case UnitType.Amphibious:
                    case UnitType.Water:
                        return waterChance;
                    default:
                        throw new Exception();
                    }
                };

                var addUnits = race.Where(unit => Game.Random.Bool(GetChance(unit))).ToList();
                if (!addUnits.Any(unit => unit.costType == CostType.Production))
                    return InitUnits(tile);
                units.Concat(addUnits.Select(unit => unit.Name));
            }

            return new List<string>(Game.Random.Iterate(units));
        }
        private static void GetChances(int count1, int count2, double baseChance1, out double chance1, out double chance2)
        {
            if (count1 > 0 && count2 > 0)
            {
                chance2 = GetTargetPct(avgChance, baseChance1, count1, count2);
                if (chance2 < 0)
                    chance2 = 0;
                else if (chance2 > 1)
                    chance2 = 1;
                chance1 = GetTargetPct(avgChance, chance2, count2, count1);
            }
            else
            {
                chance1 = avgChance;
                chance2 = avgChance;
            }
        }
        private static double GetTargetPct(double avgPct, double havePct, int haveCount, int targetCount)
        {
            return ( ( avgPct * ( haveCount + targetCount ) ) - ( havePct * haveCount ) ) / targetCount;
        }
        #endregion //fields and constructors

        #region internal methods
        internal int CanBuildCount
        {
            get
            {
                return units.Count;
            }
        }
        #endregion //internal methods

        #region overrides
        public override bool CapableBuild(string name)
        {
            return CapableBuild(name, false);
        }
        private bool CapableBuild(string name, bool raw)
        {
            if (name == "Wizard")
                return true;

            Unit unit = Unit.CreateTempUnit(name);
            if (!RaceCheck(unit))
                return false;
            if (!units.Contains(name))
                return false;

            //can only build elemental units when on the correct terrain
            CostType costType = unit.costType;
            if (tile.MatchesTerrain(costType))
                return true;

            if (costType == CostType.Production)
            {
                bool ground = false, water = false;
                for (int i = 0 ; i < 6 ; ++i)
                {
                    Tile neighbor = tile.GetNeighbor(i);
                    if (neighbor != null)
                        if (neighbor.Terrain == Terrain.Water)
                            water = true;
                        else
                            ground = true;
                }
                bool can;
                switch (unit.Type)
                {
                case UnitType.Air:
                    //can only build air when on or next to ground
                    can = ( ground || tile.Terrain != Terrain.Water );
                    break;
                case UnitType.Amphibious:
                    //can only build amphibious when on or next to water
                    can = ( water || tile.Terrain == Terrain.Water );
                    break;
                case UnitType.Ground:
                    //can only build ground when next to ground
                    can = ground;
                    break;
                case UnitType.Immobile:
                    //can only build immobile when on ground or next to water
                    can = ( water || tile.Terrain != Terrain.Water );
                    break;
                case UnitType.Water:
                    //can only build water when next to water
                    can = water;
                    break;
                default:
                    throw new Exception();
                }
                if (can)
                    return true;
            }

            if (raw)
                return false;

            bool prod = true, death = true;
            UnitSchema us = UnitTypes.GetSchema();
            foreach (UnitSchema.UnitRow r in us.Unit.Rows)
                if (CapableBuild(r.Name, true))
                {
                    if (r.CostType == "")
                        prod = false;
                    else
                        death = false;
                }

            //if no production units could otherwise be built, a random half of those in the list can be built (rounded up)
            if (prod && costType == CostType.Production)
            {
                bool can = true;
                foreach (string available in units)
                    if (RaceCheck(unit) && Unit.CreateTempUnit(available).costType == CostType.Production)
                    {
                        if (can && available == name)
                            return true;
                        can = !can;
                    }
            }

            //can only build death units if no other magic units can be built
            return ( death && costType == CostType.Death );
        }

        protected override bool CanMoveChild(Tile t)
        {
            return false;
        }

        protected override bool DoMove(Tile t, bool gamble, out bool canUndo)
        {
            canUndo = true;
            return false;
        }

        internal override void ResetMove()
        {
        }

        internal override double Heal()
        {
            return -1;
        }
        internal override void UndoHeal(double healInfo)
        {
            throw new Exception();
        }
        #endregion //overrides
    }
}
