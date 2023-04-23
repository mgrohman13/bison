using System;
using System.Collections.Generic;
using System.Linq;

namespace CityWar
{
    [Serializable]
    public class Wizard : Capturable
    {
        #region fields and constructors

        private readonly List<string> units = new();

        internal Wizard(Player owner, Tile tile, out bool canUndo)
            : base(1, owner, tile, "Wizard")
        {
            this.units = InitUnits(owner.Game);
            canUndo = false;
        }

        private static List<string> InitUnits(Game game)
        {
            List<string> units = new();
            foreach (string[] race in Game.Races.Values)
            {
                bool any = false;
                foreach (string u in race)
                {
                    double chance;
                    switch (Unit.CreateTempUnit(u).CostType)
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
                    {
                        units.Add(u);
                        any = true;
                    }
                }
                if (!any)
                    return InitUnits(game);
            }
            return units;
        }

        public override List<string> GetBuildList()
        {
            return this.units.Where(u => Unit.CreateTempUnit(u).Race == this.owner.Race).ToList();
        }

        #endregion //fields and constructors

        #region overrides

        public override bool CapableBuild(string name)
        {
            if (name == "Wizard" || name.EndsWith(" Portal"))
                return true;

            Unit unit = Unit.CreateTempUnit(name);
            if (!RaceCheck(unit))
                return false;
            if (units.Contains(name))
                return true;

            //can always build magic units when on the correct terrain
            return tile.MatchesTerrain(unit.CostType);
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
            return (!(t.Occupied(out Player p) && p != owner));
        }

        protected override bool DoMove(Tile t, bool gamble, out bool canUndo)
        {
            canUndo = true;

            if (movement > 0)
            {
                tile.Remove(this);
                tile = t;
                canUndo = tile.Add(this);

                tile.CurrentGroup = group;
                --movement;

                return true;
            }

            return false;
        }

        internal override void ResetMove()
        {
            while (movement > 0)
                Heal();

            movement = MaxMove;

            base.ResetMove();
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
