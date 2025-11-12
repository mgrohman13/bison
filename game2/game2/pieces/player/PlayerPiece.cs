using game2.map;

namespace game2.pieces.player
{
    [Serializable]
    public abstract class PlayerPiece : Piece
    {
        private int _vision;
        private readonly int _visionBase;

        public int Vision => _vision + Tile.Terrain.VisionMod();
        public int VisionBase => _visionBase;

        internal PlayerPiece(Tile tile, int vision)
            : base(tile.Map.Game.Player, tile)
        {
            _vision = vision;
            _visionBase = vision;
        }

        internal override void Wound(float woundPct)
        {
            _vision = game.Game.Rand.Round(_vision * woundPct);
        }

        //public void GetIncome(ref double energyInc, ref double massInc, ref double researchInc)
        //{
        //    GenerateResources(ref energyInc, ref massInc, ref researchInc);

        //    double energyUpk = 0, massUpk = 0;
        //    GetUpkeep(ref energyUpk, ref massUpk);
        //    energyInc -= energyUpk;
        //    massInc -= massUpk;
        //}
        //internal virtual void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        //{
        //}

        //public double GetRepairInc()
        //{
        //    double result = 0;
        //    if (this.HasBehavior(out Behavior.Combat.IKillable killable))
        //        killable.GetHitsRepair(out result, out _);
        //    return result;
        //}
        //public bool IsRepairing() => HasBehavior(out Behavior.Combat.IKillable killable) && killable.IsRepairing();

        //internal abstract void OnResearch(Research.Type type);
    }
}
