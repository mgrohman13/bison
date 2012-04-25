using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Tile
    {
        public readonly Terrain Terrain;

        public bool IsWater
        {
            get
            {
                return ( Terrain == Terrain.Sea || Terrain == Terrain.DeepSea || Terrain == Terrain.FreshWater );
            }
        }

        public Tile(Terrain terrain)
        {
            this.Terrain = terrain;
        }

        public Tile(float height, float temperature, float rainfall, Point p)
        {
            this.Terrain = GetTerrain(height, temperature, rainfall);
        }

        private static Terrain GetTerrain(float height, float temp, float rain)
        {
            if (rain < 0)
                rain = 0;
            else if (rain > 500)
                rain = 500;

            Terrain t;

            if (temp < 39)
                t = Terrain.Glacier;
            else if (height < .169)
                t = Terrain.DeepSea;
            else if (height < .39)
                t = Terrain.Sea;
            else if (height > .97)
                t = Terrain.AlpineGlacier;
            else if (height > .87)
                if (height % .02f < ( height - .87f ) / 5f)
                    t = Terrain.Mountain;
                else
                    t = Terrain.AlpineTundra;
            else if (( temp > 100 ) && ( ( rain < ( temp - 100 ) * ( temp - 100 ) / 1000f ) ))
                if (rain < temp - 313)
                    t = Terrain.SubDesert;
                else
                    t = Terrain.TempGrassDesert;
            else if (( temp > 300 ) && ( rain < 25 + 490 / ( 1.0 + Math.Pow(Math.E, ( 390 - temp ) / 26.0) ) ))
                if (rain < 260 + ( temp - 333 ) * ( temp - 333 ) / 260f)
                    t = Terrain.TropSeasForestSavannah;
                else
                    t = Terrain.TropRainForest;
            else if (rain < ( temp - 65 ) / 2.1f)
                t = Terrain.WoodlandShrubland;
            else if (( temp > 200 ) || ( ( temp > 130 ) && ( rain < 21 + 196 / ( 1.0 + Math.Pow(Math.E, ( 169 - temp ) / 13.0) ) ) ))
                if (( temp < 169 ) || rain < 125 + 10 * Math.Sqrt(temp - 169))
                    t = Terrain.TempDecForest;
                else
                    t = Terrain.TempRainForest;
            else if (( temp > 100 ) || ( rain < 120 / ( 1.0 + Math.Pow(Math.E, ( 78 - temp ) / 13.0) ) ))
                t = Terrain.Taiga;
            else
                t = Terrain.Tundra;

            return t;
        }
    }

    public enum Terrain : byte
    {
        FreshWater,
        Glacier,
        DeepSea,
        Sea,
        AlpineGlacier,
        Mountain,
        AlpineTundra,
        SubDesert,
        TempGrassDesert,
        TropSeasForestSavannah,
        TropRainForest,
        WoodlandShrubland,
        TempDecForest,
        TempRainForest,
        Taiga,
        Tundra,
    }
}
