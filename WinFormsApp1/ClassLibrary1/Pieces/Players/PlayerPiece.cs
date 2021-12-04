using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public abstract class PlayerPiece : Piece
    {
        private readonly double _vision;
        public double Vision => _vision;

        internal PlayerPiece(Map.Tile tile, double vision)
            : base(tile.Map.Game.Player, tile)
        {
            this._vision = vision;
        }

        public virtual void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            GetUpkeep(ref energyUpk, ref massUpk);
        }

        public virtual double GetRepairInc()
        {
            if (this is IKillable killable)
            {
                var repars = Game.Player.Pieces.OfType<IRepair>();
                return repars.Sum(r => r.GetRepairInc(killable));
            }
            return 0;
        }
    }
}
