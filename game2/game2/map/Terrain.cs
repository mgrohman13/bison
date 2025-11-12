namespace game2.map
{
    public enum Terrain
    {
        Sand,
        Plains,
        Forest,
        Hills,
        Mountains,
        Sea,
        Kelp,
        Glacier,
        _MAX,
    }

    public enum VisionType
    {
        Flat,
        Normal,
        Tall,
    }

    public static class TerrainExtensions
    {
        public static int MoveCost(this Terrain terrain) => terrain switch
        {
            Terrain.Sand => 2,
            Terrain.Plains => 1,
            Terrain.Forest => 2,
            Terrain.Hills => 2,
            Terrain.Mountains => 3,
            Terrain.Sea => 1,
            Terrain.Kelp => 2,
            Terrain.Glacier => 3,
            _ => throw new NotImplementedException(),
        };
        public static int VisionCost(this Terrain terrain) => terrain switch
        {
            Terrain.Sand => 1,
            Terrain.Plains => 1,
            Terrain.Forest => 2,
            Terrain.Hills => 2,
            Terrain.Mountains => 3,
            Terrain.Sea => 1,
            Terrain.Kelp => 1,
            Terrain.Glacier => 2,
            _ => throw new NotImplementedException(),
        };
        public static VisionType VisionType(this Terrain terrain) => terrain switch
        {
            Terrain.Sand => map.VisionType.Normal,
            Terrain.Plains => map.VisionType.Normal,
            Terrain.Forest => map.VisionType.Normal,
            Terrain.Hills => map.VisionType.Normal,
            Terrain.Mountains => map.VisionType.Tall,
            Terrain.Sea => map.VisionType.Flat,
            Terrain.Kelp => map.VisionType.Normal,
            Terrain.Glacier => map.VisionType.Tall,
            _ => throw new NotImplementedException(),
        };

        public static int AttMod(this Terrain terrain) => terrain switch
        {
            Terrain.Plains => 0,
            Terrain.Sand => -2,
            Terrain.Forest => 0,
            Terrain.Hills => 1,
            Terrain.Mountains => 1,
            Terrain.Sea => 0,
            Terrain.Kelp => 0,
            Terrain.Glacier => 0,
            _ => throw new NotImplementedException(),
        };
        public static int DefMod(this Terrain terrain) => terrain switch
        {
            Terrain.Sand => -1,
            Terrain.Plains => 0,
            Terrain.Forest => 2,
            Terrain.Hills => 2,
            Terrain.Mountains => 3,
            Terrain.Sea => 0,
            Terrain.Kelp => 1,
            Terrain.Glacier => 0,
            _ => throw new NotImplementedException(),
        };
        public static int VisionMod(this Terrain terrain) => terrain switch
        {
            Terrain.Sand => 0,
            Terrain.Plains => 0,
            Terrain.Forest => -1,
            Terrain.Hills => 1,
            Terrain.Mountains => 2,
            Terrain.Sea => 0,
            Terrain.Kelp => -1,
            Terrain.Glacier => 0,
            _ => throw new NotImplementedException(),
        };
    }
}
