using System;
using System.Collections.Generic;

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

        private static List<string> InitUnits(Tile tile)
        {
            double count = 3, watChance = 0;
            for (int i = 0 ; i < 6 ; ++i)
            {
                Tile neighbor = tile.GetNeighbor(i);
                if (neighbor != null)
                {
                    ++count;
                    if (neighbor.Terrain == Terrain.Water)
                        ++watChance;
                }
            }
            const float avgChance = .8f;
            watChance = 1 - 1 / ( 1 + Math.Pow(watChance + 1.3, tile.Terrain == Terrain.Water ? 1.5 : 1.3) / count * 10 );

            int watCount = 0, nonCount = 0;
            List<string> units = new List<string>();
            foreach (string[] race in Game.Races.Values)
                foreach (string u in Game.Random.Iterate(race))
                {
                    Unit unit = Unit.CreateTempUnit(u);
                    if (unit.costType == CostType.Production)
                    {
                        if (unit.Type == UnitType.Water)
                            ++watCount;
                        else
                            ++nonCount;
                    }
                    else if (Game.Random.Bool(avgChance))
                    {
                        units.Add(u);
                    }
                }

            double nonChance;
            if (watCount > 0 && nonCount > 0)
            {
                nonChance = GetTargetPct(avgChance, watChance, watCount, nonCount);
                if (nonChance < 0)
                    nonChance = 0;
                else if (nonChance > 1)
                    nonChance = 1;
                watChance = GetTargetPct(avgChance, nonChance, nonCount, watCount);
            }
            else
            {
                watChance = avgChance;
                nonChance = avgChance;
            }

            int raceCount = 0;
            foreach (string[] race in Game.Races.Values)
            {
                bool anyProd = false;
                foreach (string u in Game.Random.Iterate(race))
                {
                    Unit unit = Unit.CreateTempUnit(u);
                    if (unit.costType == CostType.Production)
                    {
                        double pct;
                        if (unit.Type == UnitType.Water)
                            pct = watChance;
                        else
                            pct = nonChance;
                        if (Game.Random.Bool(pct))
                        {
                            units.Add(u);
                            anyProd = true;
                        }
                    }
                }
                if (anyProd)
                    ++raceCount;
            }

            if (raceCount != Game.Races.Count)
                units = InitUnits(tile);

            return units;
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
            if (!raceCheck(unit))
                return false;
            if (!units.Contains(name))
                return false;

            //can only build elemental units when on the correct terrain
            CostType costType = unit.costType;
            if (tile.Terrain == Terrain.Forest && costType == CostType.Nature)
                return true;
            if (tile.Terrain == Terrain.Mountain && costType == CostType.Earth)
                return true;
            if (tile.Terrain == Terrain.Plains && costType == CostType.Air)
                return true;
            if (tile.Terrain == Terrain.Water && costType == CostType.Water)
                return true;

            if (costType == CostType.Production)
            {
                bool ground = false, water = false;
                for (int i = 0 ; i < 6 ; ++i)
                {
                    Tile neighbor = tile.GetNeighbor(i);
                    if (neighbor != null)
                    {
                        if (neighbor.Terrain == Terrain.Water)
                            water = true;
                        else
                            ground = true;
                    }
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
                    //can always build immobile
                    can = true;
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
                    if (r.CostType == string.Empty)
                        prod = false;
                    else
                        death = false;
                }

            //if no production units could otherwise be built, a random half of those in the list can be built (rounded up)
            if (prod && costType == CostType.Production)
            {
                bool can = true;
                foreach (string available in units)
                    if (raceCheck(unit) && Unit.CreateTempUnit(available).costType == CostType.Production)
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

        protected override bool DoMove(Tile t, out bool canUndo)
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
