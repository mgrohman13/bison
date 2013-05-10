using System;
using System.Collections.Generic;

namespace CityWar
{
    [Serializable]
    public class Wizard : Capturable
    {
        #region fields and constructors
        private List<string> units;

        internal Wizard(Player owner, Tile tile, out bool canUndo)
            : base(1, owner, tile)
        {
            name = "Wizard";

            this.units = InitUnits();

            canUndo = tile.Add(this);
            owner.Add(this);

            canUndo = false;
        }

        private static List<string> InitUnits()
        {
            List<string> units = new List<string>();
            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
                {
                    double chance;
                    switch (Unit.CreateTempUnit(u).costType)
                    {
                    case CostType.Death:
                        chance = .6;
                        break;
                    default:
                        chance = .4;
                        break;
                    case CostType.Production:
                        continue;
                    }
                    if (Game.Random.Bool(chance))
                        units.Add(u);
                }

            if (units.Count == 0)
                units = InitUnits();
            return units;
        }
        #endregion //fields and constructors

        #region overrides
        public override bool CapableBuild(string name)
        {
            if (name == "Wizard" || name.EndsWith(" Portal"))
                return true;

            Unit unit = Unit.CreateTempUnit(name);
            if (!raceCheck(unit))
                return false;
            if (units.Contains(name))
                return true;

            //can always build magic units when on the correct terrain
            CostType costType = unit.costType;
            return ( costType == CostType.Air && tile.Terrain == Terrain.Plains ||
                costType == CostType.Earth && tile.Terrain == Terrain.Mountain ||
                costType == CostType.Nature && tile.Terrain == Terrain.Forest ||
                costType == CostType.Water && tile.Terrain == Terrain.Water );
        }

        internal override double Heal()
        {
            if (movement > 0)
            {
                owner.AddMagic(tile.Terrain, 10);
                --movement;
                return 1;
            }
            return -1;
        }
        internal override void UndoHeal(double oldMove)
        {
            owner.AddMagic(tile.Terrain, -10);
            ++movement;
        }

        protected override bool CanMoveChild(Tile t)
        {
            Player p;
            return ( !( t.Occupied(out p) && p != owner ) );
        }

        protected override bool DoMove(Tile t, bool gamble, out bool canUndo)
        {
            if (movement > 0)
            {
                //cant undo if you collect any wizard points
                canUndo = t.CollectWizPts(owner);
                tile.Remove(this);
                t.Add(this);
                this.tile = t;
                tile.CurrentGroup = group;
                --movement;
                return true;
            }

            canUndo = true;
            return false;
        }

        internal override void ResetMove()
        {
            while (movement > 0)
                Heal();

            movement = MaxMove;
        }
        #endregion //overrides

        #region internal methods
        internal void ChangeTerrain(Terrain t)
        {
            if (movement > 0)
            {
                tile.Terrain = t;
                --movement;
            }
        }
        internal void UndoChangeTerrain(int oldMovement, Terrain t)
        {
            tile.Terrain = t;
            movement = oldMovement;
        }

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
