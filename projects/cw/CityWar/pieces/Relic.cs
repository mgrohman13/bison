using System;
using System.Collections.Generic;

namespace CityWar
{
    [Serializable]
    public class Relic : Capturable
    {
        #region fields and constructors
        private List<string> units;

        internal Relic(Player owner, Tile tile)
            : base(0, owner, tile)
        {
            ability = Abilities.AircraftCarrier;
            name = "Relic";

            this.units = InitUnits(tile.Terrain);

            tile.Add(this);
            owner.Add(this);
        }

        private const float matchChance = .6f, unmatchChance = .3f;
        private static List<string> InitUnits(Terrain terrain)
        {
            List<string> units = new List<string>();
            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
                {
                    float val;
                    Unit unit = Unit.CreateTempUnit(u);
                    if (unit.costType == CostType.Production)
                        val = .2f;
                    else if (unit.costType == CostType.Death)
                        val = .7f;
                    else if (( terrain == Terrain.Forest && unit.costType == CostType.Nature )
                        || ( terrain == Terrain.Mountain && unit.costType == CostType.Earth )
                        || ( terrain == Terrain.Plains && unit.costType == CostType.Air )
                        || ( terrain == Terrain.Water && unit.costType == CostType.Water ))
                        val = matchChance;
                    else
                        val = unmatchChance;

                    if (Game.Random.Bool(val))
                        units.Add(u);
                }

            if (units.Count == 0)
                units = InitUnits(terrain);
            return units;
        }

        internal void ChangedTerrain(Terrain newTerrain)
        {
            //chance to remove units matching the old terrain
            foreach (string u in this.units.ToArray())
            {
                Unit unit = Unit.CreateTempUnit(u);
                if (( ( tile.Terrain == Terrain.Forest && unit.costType == CostType.Nature )
                    || ( tile.Terrain == Terrain.Mountain && unit.costType == CostType.Earth )
                    || ( tile.Terrain == Terrain.Plains && unit.costType == CostType.Air )
                    || ( tile.Terrain == Terrain.Water && unit.costType == CostType.Water ) ) &&
                    Game.Random.Bool(1 - ( unmatchChance / matchChance )))
                    this.units.Remove(u);
            }
            //chance to add units matching the new terrain
            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
                {
                    Unit unit = Unit.CreateTempUnit(u);
                    if (!this.units.Contains(u) &&
                        ( ( newTerrain == Terrain.Forest && unit.costType == CostType.Nature )
                        || ( newTerrain == Terrain.Mountain && unit.costType == CostType.Earth )
                        || ( newTerrain == Terrain.Plains && unit.costType == CostType.Air )
                        || ( newTerrain == Terrain.Water && unit.costType == CostType.Water ) )
                        && Game.Random.Bool(( matchChance - unmatchChance ) / ( 1 - unmatchChance )))
                        this.units.Add(u);
                }
        }
        #endregion //fields and constructors

        #region overrides
        public override bool CapableBuild(string name)
        {
            if (name == "Wizard")
                return true;
            if (!raceCheck(name))
                return false;

            return ( units.Contains(name) );
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

        #region internal methods
        internal int CanBuildCount
        {
            get
            {
                return units.Count;
            }
        }
        #endregion //internal methods
    }
}
