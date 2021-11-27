using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;


namespace ClassLibrary1.Pieces
{
    [Serializable]
    public class Core : IPlayerPiece, IBuilding
    {
        private readonly IPlayerPiece piece;
        private readonly IBuilding building;

        private Core(Player player)
        {
            piece = new PlayerPiece(player.Game, player.Game.Map.GetTile(0, 0), 3.9);
            building = new Building();
        }
        internal static Core NewCore(Player player)
        {
            Core obj = new Core(player);
            player.Game.Map.AddPiece(obj);
            return obj;
        }

        public void EndTurn()
        {
            piece.EndTurn();
            building.EndTurn();
        }

        #region IPlayerPiece

        public ISide Side => piece.Side;
        public Map.Tile Tile => piece.Tile;
        public double Vision => piece.Vision;

        public bool IsPlayer => piece.IsPlayer;

        void IPiece.SetTile(Map.Tile tile)
        {
            SetTile(tile);
        }
        internal void SetTile(Map.Tile tile)
        {
            piece.SetTile(tile);
        }

        #endregion IPlayerPiece

        #region IBuilding

        public void Build(Player player, Map.Tile tile, double vision, double moveInc, double moveMax, double moveLimit)
        {
            building.Build(player, tile, vision, moveInc, moveMax, moveLimit);
        }

        #endregion IBuilding
    }
}
