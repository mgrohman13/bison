using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Core : PlayerPiece, IBuilding
    {
        private readonly IBuilding building;

        private Core(Player player) : base(player.Game, player.Game.Map.GetTile(0, 0), 3.9)
        {
            building = new Building(this);
        }
        internal static Core NewCore(Player player)
        {
            Core obj = new(player);
            player.Game.AddPiece(obj);
            return obj;
        }

        void IBuilding.EndTurn()
        {
            EndTurn();
        }
        internal override void EndTurn()
        {
            base.EndTurn();
            building.EndTurn();
        }

        #region IBuilding

        public void Build(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit)
        {
            building.Build(player, tile, vision, moveInc, moveMax, moveLimit);
        }


        #endregion IBuilding
    }
}
