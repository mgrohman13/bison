using LevelGeneration;
using LevelGeneration.WorldMap;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpipsReplayability.Mod
{
    internal class GameRandom
    {
        private static TerritoryInstance[] _territories;
        public static TerritoryInstance[] Territories
        {
            get
            {
                return _territories;
            }
            set
            {
                if (_territories != value)
                {
                    _territories = value;
                    //force recalculation on next request
                    Shuffled = false;
                    _worldMapIndex = -1;
                }
            }
        }
        public static bool Shuffled { get; set; }

        //random calculations are reset whenever the worldMapIndex changes 
        private static int _worldMapIndex = -1;
        public static int WorldMapIndex
        {
            get
            {
                return _worldMapIndex;
            }
        }
        private static bool[] _hiddenRewards;
        public static bool[] HiddenRewards
        {
            get
            {
                CalcRand();
                return _hiddenRewards;
            }
        }
        private static int[] _shuffle;
        public static int[] Shuffle
        {
            get
            {
                CalcRand();
                return _shuffle;
            }
        }

        //TODO: this approach is not worth the effort, especially when it comes to map validation
        //let's just persist a save file...
        private static void CalcRand()
        {
            int mapIndex = Map.MissionManagerAsset.WorldMapIndex;
            if (_worldMapIndex != mapIndex)
            {
                _worldMapIndex = mapIndex;
                //all territory information that is used for deterministic randomness needs to be included in this order by
                var orderedTerritories = _territories.Select(t => t.operation)
                    .OrderBy(o => o.map.GetInstanceID()).ThenBy(o => o.operationName).ThenBy(o => o.itemRewards.Count);

                //we need to seed a constant PRNG that will persist through saving/loading but be different for each playthrough
                //the humorous randomly generated operation names are the perfect cadidate for this
                uint[] seed = orderedTerritories
                    .Select(o => (uint)o.operationName.GetHashCode())
                    .ToArray();
                Plugin.Log.LogInfo($"PRNG seed: {seed.Select(s => s.ToString("X8")).Aggregate("", (a, b) => a + b)}");

                MTRandom seededRand = new(seed);
                SetHiddenRewards(seededRand, orderedTerritories);
                SetShuffle(seededRand);
            }
        }

        private static void SetHiddenRewards(MTRandom seededRand, IEnumerable<Operation> opertaions)
        {
            //each reward has a small chance to remain hidden
            _hiddenRewards = seededRand.Iterate(opertaions).SelectMany(o =>
            {
                int chance = seededRand.RangeInt(2, 11);
                return o.itemRewards.Select(r => seededRand.Next(chance) == 0);
            }).ToArray();
        }
        private static void SetShuffle(MTRandom seededRand)
        {
            _shuffle = Enumerable.Range(0, Territories.Length).ToArray();
            do
                seededRand.Shuffle(_shuffle);
            while (!Map.ValidateShuffle(seededRand, !Shuffled));
        }
    }
}
