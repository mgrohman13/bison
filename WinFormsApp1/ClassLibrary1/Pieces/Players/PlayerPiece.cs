﻿using System;
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
        protected double _vision;
        public double Vision => Consts.GetDamagedValue(this, VisionBase, 0);
        public double VisionBase => _vision;

        internal PlayerPiece(Map.Tile tile, double vision)
            : base(tile.Map.Game.Player, tile)
        {
            this._vision = vision;
        }

        public virtual void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc)
        {
            GetUpkeep(ref energyUpk, ref massUpk);
        }

        public double GetRepairInc()
        {
            double result = 0;
            if (this.HasBehavior(out IKillable killable))
                killable.Repair(false, out result, out _);
            return result;
        }

        internal abstract void OnResearch(Research.Type type);
    }
}
