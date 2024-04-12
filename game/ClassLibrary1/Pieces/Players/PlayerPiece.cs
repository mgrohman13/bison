using System;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public abstract class PlayerPiece : Piece
    {
        protected double _vision;
        public double Vision => Consts.GetDamagedValue(this, VisionBase, 0);
        public double VisionBase => _vision;

        internal PlayerPiece(Tile tile, double vision)
            : base(tile.Map.Game.Player, tile)
        {
            this._vision = vision;
        }

        public void GetIncome(ref double energyInc, ref double massInc, ref double researchInc)
        {
            GenerateResources(ref energyInc, ref massInc, ref researchInc);

            double energyUpk = 0, massUpk = 0;
            GetUpkeep(ref energyUpk, ref massUpk);
            energyInc -= energyUpk;
            massInc -= massUpk;
        }
        internal virtual void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
        }

        public double GetRepairInc()
        {
            double result = 0;
            if (this.HasBehavior(out IKillable killable))
                killable.GetHitsRepair(out result, out _);
            return result;
        }

        internal abstract void OnResearch(Research.Type type);
    }
}
