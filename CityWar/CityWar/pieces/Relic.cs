using System;
using System.Collections.Generic;

namespace CityWar
{
    [Serializable]
    public class Relic : Capturable
    {
        #region fields and constructors

        private readonly List<string> units = new List<string>();

        internal Relic(Player owner, Tile tile)
            : base(0, owner, tile, "Relic", CityWar.Ability.AircraftCarrier)
        {
            this.units = InitUnits(tile.Game, tile.Terrain);
        }

        private const double matchChance = .5, unmatchChance = .2;
        private static List<string> InitUnits(Game game, Terrain terrain)
        {
            List<string> units = new List<string>();
            foreach (string[] race in Game.Races.Values)
            {
                bool any = false;
                foreach (string u in race)
                {
                    double val;
                    Unit unit = Unit.CreateTempUnit(game, u);
                    if (unit.CostType == CostType.Production)
                        val = .3;
                    else if (unit.CostType == CostType.Death)
                        val = .7;
                    else if (Tile.MatchesTerrain(unit.CostType, terrain))
                        val = matchChance;
                    else
                        val = unmatchChance;

                    if (Game.Random.Bool(val))
                    {
                        units.Add(u);
                        any = true;
                    }
                }
                if (!any)
                    return InitUnits(game, terrain);
            }
            return units;
        }

        internal void ChangedTerrain(Terrain newTerrain)
        {
            //chance to remove units matching the old terrain
            foreach (string u in this.units.ToArray())
            {
                Unit unit = Unit.CreateTempUnit(owner.Game, u);
                if (tile.MatchesTerrain(unit.CostType) && Game.Random.Bool(1 - ( unmatchChance / matchChance )))
                    this.units.Remove(u);
            }
            //chance to add units matching the new terrain
            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
                {
                    Unit unit = Unit.CreateTempUnit(owner.Game, u);
                    if (!this.units.Contains(u) && Tile.MatchesTerrain(unit.CostType, newTerrain)
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

        protected override bool DoMove(Tile t, bool gamble, out bool canUndo)
        {
            canUndo = true;
            return false;
        }

        internal override void ResetMove()
        {
            base.ResetMove();
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
