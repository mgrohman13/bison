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

        public override int? Cost
        {
            get
            {
                return null;
            }
        }
        public override bool StoresProduction
        {
            get
            {
                return true;
            }
        }

        internal override bool Build(IEventHandler handler, double production)
        {
            bool retVal = false;

            production += this.Production;
            this.Production = 0;
            while (production >= this.Cost.Value)
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

                production -= this.Cost.Value;
                production *= ( 1 - Consts.CarryProductionLossPct );
            }
            this.Production += Game.Random.Round(production);

            return retVal;
        }

        internal override void GetTurnIncome(ref double production, ref double gold, bool minGold)
        {
            double totalProd = this.Production + production;
            while (totalProd > this.Cost)
            {
                totalProd -= this.Cost.Value;
                double loss = totalProd * Consts.CarryProductionLossPct;
                if (minGold)
                    loss = Math.Floor(loss);
                totalProd -= loss;
                gold += loss / Consts.ProductionForGold - design.Upkeep * Consts.UpkeepUnmovedReturn;
                if (minGold)
                {
                    int thisPop = colony.Population + (int)Math.Floor(colony.GetPopulationGrowth());
                    int move = design.Trans;
                    if (move > thisPop)
                        move = thisPop + 1;
                    gold -= Consts.GetMovePopCost(colony.Player.Game.MapSize, thisPop, colony.Soldiers);
                }
            }
            if (minGold && gold > 0)
                gold = 0;
        }

        public override string ToString()
        {
            return design.ToString();
        }
    }
}
