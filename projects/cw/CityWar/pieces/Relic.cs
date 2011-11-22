using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

            InitUnits(tile.Terrain);

            tile.Add(this);
            owner.Add(this);
        }

        private void InitUnits(Terrain terrain)
        {
            units = new List<string>();
            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
                {
                    //the chance of being able to build is based on the cost type
                    float val;
                    Unit unit = Unit.CreateTempUnit(u);
                    if (unit.costType == CostType.Production) //7.67
                        val = .3f;
                    else if (unit.costType == CostType.Death) //1.67
                        val = .7f;
                    else if (( terrain == Terrain.Forest && unit.costType == CostType.Nature ) //1.92
                        || ( terrain == Terrain.Mountain && unit.costType == CostType.Earth )
                        || ( terrain == Terrain.Plains && unit.costType == CostType.Air )
                        || ( terrain == Terrain.Water && unit.costType == CostType.Water ))
                        val = .4f;
                    else //5.75
                        val = .2f;

                    if (Game.Random.Bool(val))
                        units.Add(u);
                }
        }
        #endregion //fields and constructors

        #region overrides
        public override bool CapableBuild(string name)
        {
            if (!raceCheck(name))
                return false;
            if (name == "Wizard")
                return true;
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
