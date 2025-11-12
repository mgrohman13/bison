using game2.map;
using System.Diagnostics;

namespace game2.game
{
    public class Consts
    {
        public const float AvgTotalPerResource = 150f; // + > ~ 30
        public const float AvgTileResources = 5f; //avg total minable resources per explored tile

        private static float Deviation;
        static Consts() => SetDeviation();
        private static void SetDeviation() => Deviation = GetDeviation();
        internal static float GetDeviation() => Game.Rand.GaussianOE(Game.Rand.Range(.13f, .169f), .169f, .065f);

        internal readonly float WoundDeviation;

        internal readonly float LandWidth = Game.Rand.GaussianCapped(.1f, Deviation / 3f, .05f);
        internal readonly float Forests = Game.Rand.GaussianCapped(.425f, Deviation, .2f);
        internal readonly float Kelp = Game.Rand.GaussianCapped(.375f, Deviation, .2f);
        internal readonly float WaterNoiseMult = Game.Rand.GaussianCapped(1.25f, Deviation, .5f);
        internal readonly float ForestNoiseMult = Game.Rand.GaussianOE(.5f, Deviation * 4f, Deviation, .01f);
        internal readonly float MountainNoiseMult = Game.Rand.GaussianCapped(1f, Deviation, .5f);
        internal readonly float HillsWidth = Game.Rand.GaussianCapped(.04f, Deviation / 2f, .01f);

        internal readonly float HillsDepthMod = Game.Rand.GaussianOE(1.25f, Deviation * 2.25f, Deviation / 1.25f, .01f);
        internal readonly float ForestMountainMod = Game.Rand.GaussianOE(1f, Deviation * 2.75f, Deviation, .01f);
        internal readonly float MountainSeaGlacierMod = Game.Rand.GaussianOE(.8f, Deviation * 2.5f, Deviation * 1.5f, .01f);
        internal readonly float ForestGlacierSeaMod = Game.Rand.GaussianOE(.5f, Deviation * 2f, Deviation * 2f, .01f);
        internal readonly float DepthKelpMod = Game.Rand.GaussianOE(.6f, Deviation * 3f, Deviation * 1.75f, .01f);

        internal readonly float DepthGlacier, Mountains, Sand;

        //ResourcePrimarySecondaryRatio
        internal readonly float ResourceTypeConsistency, ResourceDev, ResourceOE, ResourceRateDev, ResourceRateOE;

        private readonly float ResearchValue, UpkeepValue;

        private readonly float BasicValue, AdvancedValue, MobilityValue, SpecialValue;
        private readonly float BasicRateDiv, AdvancedRateDiv, MobilityRateDiv, SpecialRateDiv;
        private readonly float BasicFreq, AdvancedFreq, MobilityFreq, SpecialFreq;
        private readonly float BasicRichness, AdvancedRichness, MobilityRichness, SpecialRichness;

        internal readonly float CoreTotalIncome, CoreIncReduction;
        private readonly int CoreBasicInc, CoreAdvancedInc, CoreMobilityInc, CoreSpecialInc;

        private readonly float[][] TerrainResourceMult;

        internal readonly float TilesPerResource;


        internal Consts()
        {
            SetDeviation();

            WoundDeviation = Game.Rand.GaussianOE(1 / 4f, Deviation, Deviation / 3f);

            DepthGlacier = Game.Rand.GaussianCapped(.275f, Deviation * 3f, LandWidth + .05f);
            Mountains = Game.Rand.GaussianCapped(LandWidth, Deviation * 3f) - LandWidth;
            Sand = Game.Rand.GaussianCapped(.75f, Deviation / 2f, Math.Max(.5f, Forests));

            SetDeviation();

            //ResourcePrimarySecondaryRatio = Game.Rand.GaussianOE(3f, Deviation, Deviation / 3f, 1f);
            ResourceTypeConsistency = 1f + Game.Rand.OEFloat();

            ResourceDev = GetDeviation() * 3f;
            ResourceOE = GetDeviation();
            ResourceRateDev = GetDeviation() / 2f;
            ResourceRateOE = GetDeviation();

            ResearchValue = Game.Rand.GaussianOE(7.5f, Deviation / 2f, Deviation, .5f);
            UpkeepValue = Game.Rand.GaussianOE(1f / 5f, Deviation, Deviation / 2f, .05f);

            BasicValue = Game.Rand.GaussianOE(1f, Deviation / 1.5f, Deviation / 3f, .5f);
            AdvancedValue = Game.Rand.GaussianOE(3f, Deviation / 1.5f, Deviation / 2f, .5f);
            MobilityValue = Game.Rand.GaussianOE(2f, Deviation / 1.5f, Deviation / 2f, .5f);
            SpecialValue = Game.Rand.GaussianOE(5f, Deviation / 2f, Deviation / 4f, .5f);
            //Upkeep = ~.1f?

            BasicRateDiv = Game.Rand.GaussianOE(3f, Deviation / 2.5f, Deviation / 1.5f, 1.5f);
            AdvancedRateDiv = Game.Rand.GaussianOE(4f, Deviation / 2.5f, Deviation / 2f, 1.5f);
            MobilityRateDiv = Game.Rand.GaussianOE(6f, Deviation / 2f, Deviation / 2.5f, 1.5f);
            SpecialRateDiv = Game.Rand.GaussianOE(10f, Deviation / 1.5f, Deviation / 2f, 1.5f);

            SetDeviation();

            BasicFreq = Game.Rand.GaussianOE(20f, Deviation, Deviation, 1f);
            AdvancedFreq = Game.Rand.GaussianOE(10f, Deviation, Deviation, 1f);
            MobilityFreq = Game.Rand.GaussianOE(15f, Deviation, Deviation, 1f);
            SpecialFreq = Game.Rand.GaussianOE(5f, Deviation, Deviation, 1f);

            float limit = Math.Max(Math.Max(Math.Max(BasicFreq, AdvancedFreq), MobilityFreq), SpecialFreq);
            float avgFreq = (BasicFreq + AdvancedFreq + MobilityFreq + SpecialFreq) / 4f;

            float resources = Game.Rand.GaussianOE(AvgTotalPerResource * avgFreq + limit, Deviation / 3f, Deviation / 2f, limit);
            BasicRichness = Game.Rand.GaussianOE(resources / BasicFreq, Deviation, Deviation / 2f, 1f);
            AdvancedRichness = Game.Rand.GaussianOE(resources / AdvancedFreq, Deviation * 2f, Deviation / 2f, 1f);
            MobilityRichness = Game.Rand.GaussianOE(resources / MobilityFreq, Deviation * 2f, Deviation / 2f, 1f);
            SpecialRichness = Game.Rand.GaussianOE(resources / SpecialFreq, Deviation, Deviation / 2f, 1f);

            float avgPerResource = (BasicFreq * BasicRichness + AdvancedFreq * AdvancedRichness
                + MobilityFreq * MobilityRichness + SpecialFreq * SpecialRichness)
                / (BasicFreq + AdvancedFreq + MobilityFreq + SpecialFreq);
            TilesPerResource = Game.Rand.GaussianCapped(avgPerResource / AvgTileResources, Deviation / 5f);

            SetDeviation();

            const int NumTerrains = (int)Terrain._MAX;
            TerrainResourceMult = new float[NumTerrains][];
            for (int a = 0; a < NumTerrains; a++)
            {
                TerrainResourceMult[a] = new float[Resources.NumResources];
                for (int b = 0; b < Resources.NumResources; b++)
                    TerrainResourceMult[a][b] = Game.Rand.GaussianOE(10f, Deviation * 2f, Deviation, 1f);
            }

            // Basic
            int c = 0;
            TerrainResourceMult[(int)Terrain.Glacier][c] /= 10f;
            TerrainResourceMult[(int)Terrain.Sand][c] /= 7f;
            TerrainResourceMult[(int)Terrain.Mountains][c] /= 4f;
            TerrainResourceMult[(int)Terrain.Sea][c] /= 3f;
            TerrainResourceMult[(int)Terrain.Hills][c] /= 2f;
            TerrainResourceMult[(int)Terrain.Forest][c] *= 3f;
            TerrainResourceMult[(int)Terrain.Plains][c] *= 4f;
            TerrainResourceMult[(int)Terrain.Kelp][c] *= 5f;

            // Advanced
            c++;
            TerrainResourceMult[(int)Terrain.Forest][c] /= 5f;
            TerrainResourceMult[(int)Terrain.Kelp][c] /= 3f;
            TerrainResourceMult[(int)Terrain.Sea][c] /= 2f;
            TerrainResourceMult[(int)Terrain.Mountains][c] *= 2f;
            TerrainResourceMult[(int)Terrain.Hills][c] *= 4f;
            TerrainResourceMult[(int)Terrain.Glacier][c] *= 5f;

            // Mobility
            c++;
            TerrainResourceMult[(int)Terrain.Sea][c] /= 5f;
            TerrainResourceMult[(int)Terrain.Plains][c] /= 3f;
            TerrainResourceMult[(int)Terrain.Kelp][c] *= 2f;
            TerrainResourceMult[(int)Terrain.Glacier][c] *= 3f;
            TerrainResourceMult[(int)Terrain.Forest][c] *= 4f;
            TerrainResourceMult[(int)Terrain.Sand][c] *= 5f;

            // Special
            c++;
            TerrainResourceMult[(int)Terrain.Plains][c] /= 5f;
            TerrainResourceMult[(int)Terrain.Glacier][c] /= 2f;
            TerrainResourceMult[(int)Terrain.Sea][c] *= 4f;
            TerrainResourceMult[(int)Terrain.Mountains][c] *= 5f;

            SetDeviation();

            //CoreTotalIncome = Game.Rand.GaussianOE(35f, Deviation / 3f, Deviation / 5f, 20f);

            float avgBasicInc = 15f / BasicValue;
            float avgAdvancedInc = 5f / AdvancedValue;
            float avgMobilityInc = 5f / MobilityValue;
            float avgSpecialInc = 10f / SpecialValue;
            CoreSpecialInc += Game.Rand.GaussianCappedInt(1 + avgSpecialInc, Deviation * 2f, 1);
            double mult = avgSpecialInc / CoreSpecialInc;
            CoreAdvancedInc += Game.Rand.GaussianCappedInt(1 + avgAdvancedInc * Math.Sqrt(mult), Deviation * 1.5f, 1);
            mult *= avgAdvancedInc / CoreAdvancedInc;
            CoreBasicInc += Game.Rand.GaussianCappedInt(1 + avgBasicInc * Math.Sqrt(mult), Deviation * 1.5f, 1);
            mult *= avgBasicInc / CoreBasicInc;
            CoreMobilityInc += Game.Rand.GaussianCappedInt(1 + avgMobilityInc * Math.Sqrt(mult), Deviation / 2f, 1);

            CoreTotalIncome = CoreBasicInc * BasicValue + CoreAdvancedInc * AdvancedValue + CoreMobilityInc * MobilityValue + CoreSpecialInc * SpecialValue;
            float coreTurns = Game.Rand.GaussianCapped(5f + 45f * (35f / CoreTotalIncome), Deviation / 5f, 5);
            CoreIncReduction = coreTurns / (coreTurns + 1f);

            SetDeviation();
        }

        internal float[] TerrainResourceMults(Terrain terrain) => TerrainResourceMult[(int)terrain];

        internal float[] ResourceValue => [BasicValue, AdvancedValue, MobilityValue, SpecialValue,];
        internal float[] ResourceRateDiv => [BasicRateDiv, AdvancedRateDiv, MobilityRateDiv, SpecialRateDiv,];
        internal float[] ResourceFreq => [BasicFreq, AdvancedFreq, MobilityFreq, SpecialFreq,];
        internal float[] ResourceTotal => [BasicRichness / BasicValue,
            AdvancedRichness / AdvancedValue, MobilityRichness / MobilityValue, SpecialRichness / SpecialValue,];
        internal int[] CoreIncome => [CoreBasicInc, CoreAdvancedInc, CoreMobilityInc, CoreSpecialInc,];

        internal int Wound(float pct, int cur, int max, int min = 1)
        {
            double avg = min + (cur - min) * (1f - pct);
            double dev = (cur - min) * pct * WoundDeviation / avg;
            int cap = Math.Max(min, (int)Math.Ceiling(avg * 2 - max));
            int res = Game.Rand.GaussianCappedInt(avg, dev, cap);

            if (res > cur)
            {
                Debug.WriteLine($"Wound res > cur ({res} > {cur})");
                res = cur;
            }
            return res;
        }
    }
}
