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

        //public void GetIncome(ref float energyInc, ref float massInc, ref float researchInc)
        //{
        //    GenerateResources(ref energyInc, ref massInc, ref researchInc);

        //    float energyUpk = 0, massUpk = 0;
        //    GetUpkeep(ref energyUpk, ref massUpk);
        //    energyInc -= energyUpk;
        //    massInc -= massUpk;
        //}
        //internal virtual void GenerateResources(ref float energyInc, ref float massInc, ref float researchInc)
        //{
        //}

        //public float GetRepairInc()
        //{
        //    float result = 0;
        //    if (this.HasBehavior(out Behavior.Combat.IKillable killable))
        //        killable.GetHitsRepair(out result, out _);
        //    return result;
        //}
        //public bool IsRepairing() => HasBehavior(out Behavior.Combat.IKillable killable) && killable.IsRepairing();

        //internal abstract void OnResearch(Research.Type type);
    }
}
