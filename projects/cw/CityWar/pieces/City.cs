using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

            InitUnits(tile);

            tile.Add(this);
            owner.Add(this);
        }

        private void InitUnits(Tile tile)
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
            units = new List<string>();
            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
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
                        units.Add(u);
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

            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
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
                            units.Add(u);
                    }
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
            if (name == "Wizard")
                return true;

            if (!units.Contains(name))
                return false;

            //can only build elemental units when on the correct terrain
            Unit unit = Unit.CreateTempUnit(name);
            if (!raceCheck(unit))
                return false;
            if (unit.costType != CostType.Production)
            {
                if (tile.Terrain == Terrain.Forest && unit.costType == CostType.Nature)
                    return true;
                else if (tile.Terrain == Terrain.Mountain && unit.costType == CostType.Earth)
                    return true;
                else if (tile.Terrain == Terrain.Plains && unit.costType == CostType.Air)
                    return true;
                else if (tile.Terrain == Terrain.Water && unit.costType == CostType.Water)
                    return true;
                else if (unit.costType == CostType.Death)
                {
                    //can only build death units if no other magic units can be built
                    bool build = true;
                    UnitSchema us = UnitTypes.GetSchema();
                    foreach (UnitSchema.UnitRow r in us.Unit.Rows)
                    {
                        if (owner.Race == r.Race
                            && ( ( tile.Terrain == Terrain.Forest && r.CostType == "N" )
                            || ( tile.Terrain == Terrain.Mountain && r.CostType == "E" )
                            || ( tile.Terrain == Terrain.Plains && r.CostType == "A" )
                            || ( tile.Terrain == Terrain.Water && r.CostType == "W" ) )
                            && ( units.Contains(r.Name) ))
                        {
                            build = false;
                            break;
                        }
                    }
                    return build;
                }

                return false;
            }

            return true;
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
