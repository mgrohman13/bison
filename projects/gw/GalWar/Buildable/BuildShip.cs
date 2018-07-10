using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    internal class BuildShip : Buildable
    {
        private ShipDesign design;

        internal BuildShip(Colony colony, ShipDesign design)
            : base(colony)
        {
            this.design = design;
        }
        internal BuildShip(Colony colony, ShipDesign design, int production)
            : base(colony)
        {
            this.design = design;
            this.Production = production;
        }

        public ShipDesign ShipDesign
        {
            get
            {
                return design;
            }
        }

        public override int? Cost
        {
            get
            {
                return null;
            }
        }

        internal override bool Build(IEventHandler handler, double production)
        {
            bool retVal = false;

            this.Production += Game.Random.Round(production);
            while (this.Production >= this.Cost.Value)
            {
                Tile tile = null;
                foreach (Tile neighbor in Tile.GetNeighbors(colony.Tile))
                    if (neighbor.SpaceObject == null)
                    {
                        //only ask for a tile if there is one available
                        tile = handler.getBuildTile(colony);
                        break;
                    }
                //null means to not actually produce the ship
                if (tile == null)
                    break;
                //invalid selection; just ask again
                if (tile.SpaceObject != null || !Tile.IsNeighbor(colony.Tile, tile))
                    continue;
                retVal = true;

                Ship ship = colony.Player.NewShip(handler, tile, design);

                int max = Math.Min(colony.AvailablePop, ship.FreeSpace);
                if (max > 0)
                {
                    max = handler.MoveTroops(colony, max, colony.Population, colony.Soldiers, true);
                    if (max > 0)
                    {
                        colony.MovePop(handler, max, ship);
                        //costs more than a standard move
                        colony.Player.GoldIncome(-max * Consts.Income);
                        //troops can be moved again next turn
                        ship.ResetMoved();
                    }
                }

                this.Production -= this.Cost.Value;
            }

            return retVal;
        }

        public override string ToString()
        {
            return design.ToString();
        }
    }
}
