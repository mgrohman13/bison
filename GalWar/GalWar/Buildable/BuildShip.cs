using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class BuildShip : Buildable
    {
        private ShipDesign design;
        private bool _pause;

        internal BuildShip(Colony colony, ShipDesign design)
            : base(colony)
        {
            this.design = design;
        }

        public ShipDesign ShipDesign
        {
            get
            {
                TurnException.CheckTurn(colony.Player);

                return design;
            }
        }
        public bool Pause
        {
            get
            {
                TurnException.CheckTurn(colony.Player);

                return this._pause;
            }
            set
            {
                TurnException.CheckTurn(colony.Player);

                this._pause = value;
            }
        }


        public override int? Cost
        {
            get
            {
                TurnException.CheckTurn(colony.Player);

                return design.Cost;
            }
        }

        internal override List<Ship> BuildShips(IEventHandler handler)
        {
            var retVal = new List<Ship>();

            while (this.production >= this.Cost.Value && !this.Pause)
            {
                Tile tile = null;
                foreach (Tile neighbor in Tile.GetNeighbors(colony.Tile))
                    if (neighbor.SpaceObject == null)
                    {
                        //only ask for a tile if there is one available
                        tile = handler.GetBuildTile(colony, design);
                        break;
                    }
                //null means to not actually produce the ship
                if (tile == null)
                    break;
                //invalid selection; just ask again
                if (tile.SpaceObject != null || !Tile.IsNeighbor(colony.Tile, tile))
                    continue;

                Ship ship = colony.Player.NewShip(handler, tile, design);
                retVal.Add(ship);

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

                this.production -= this.Cost.Value;
            }

            return retVal;
        }

        public override string ToString()
        {
            TurnException.CheckTurn(colony.Player);

            return design.ToString();
        }
    }
}
