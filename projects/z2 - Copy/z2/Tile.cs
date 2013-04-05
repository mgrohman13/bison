using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Tile
    {
        public readonly Point Point;

        public readonly Terrain Terrain;
        public Feature Feature;

        private readonly double Pop;
        public readonly Classification Class;

        public bool IsWater
        {
            get
            {
                return ( Terrain == Terrain.SaltWater || Terrain == Terrain.FreshWater );
            }
        }

        public Tile(double height, double temp, double rain, double pop, Point p)
        {
            this.Point = p;

            temp *= 500;
            rain *= temp;
            this.Class = GetClassification(height, temp, rain);

            Terrain terrain;
            double popMult;
            Feature tree = Feature.None;
            Feature thickTree = Feature.None;
            switch (Class)
            {
            case Classification.DeepSea:
                popMult = 0;
                terrain = Terrain.SaltWater;
                break;
            case Classification.Sea:
                popMult = 0;
                terrain = Terrain.SaltWater;
                break;
            case Classification.Beach:
                popMult = 2.6f / ( 1 + Math.Abs(250 - temp) );
                terrain = DirtSnow(height, temp, rain);
                break;
            case Classification.AlpineGlacier:
                popMult = 0;
                terrain = Terrain.ThickSnow;
                break;
            case Classification.Glacier:
                popMult = 0;
                terrain = Terrain.ThickSnow;
                break;
            case Classification.Mountain:
                popMult = .13f;
                if (Game.Random.Bool(( height % .006f ) / .006f))
                    terrain = Terrain.SteepCliff;
                else
                    terrain = Terrain.Cliff;
                break;
            case Classification.AlpineTundra:
                popMult = .21f;
                terrain = DirtGrassSnow(height, temp, rain);
                break;
            case Classification.Tundra:
                popMult = .169f;
                terrain = DirtGrassSnow(height, temp, rain);
                break;
            case Classification.AlpineTaiga:
                popMult = .65f;
                terrain = DirtGrassSnow(height, temp, rain);
                tree = thickTree = Feature.EvergreenTrees;
                break;
            case Classification.Taiga:
                popMult = .52f;
                terrain = DirtGrassSnow(height, temp, rain);
                tree = Feature.EvergreenTrees;
                thickTree = Feature.ThickEvergreenTrees;
                break;
            case Classification.TropRainForest:
                popMult = .52f;
                terrain = Terrain.ThickGrass;
                tree = Feature.DeciduousTrees;
                thickTree = Feature.ThickDeciduousTrees;
                break;
            case Classification.TempRainForest:
                popMult = .65f;
                terrain = GrassThickGrass(height, temp, rain);
                tree = Feature.DeciduousTrees;
                thickTree = Feature.ThickDeciduousTrees;
                break;
            case Classification.TempDecForest:
                popMult = 2.1f;
                terrain = DirtGrass(height, temp, rain);
                tree = Feature.DeciduousTrees;
                thickTree = Feature.ThickDeciduousTrees;
                break;
            case Classification.TropSeasForestSavannah:
                popMult = 1.04f;
                terrain = DirtGrassThickGrass(height, temp, rain);
                tree = Feature.DeciduousTrees;
                thickTree = Feature.ThickDeciduousTrees;
                break;
            case Classification.WoodlandShrubland:
                popMult = 1.3f;
                terrain = DirtGrassThickGrass(height, temp, rain);
                tree = thickTree = Feature.DeciduousTrees;
                break;
            case Classification.TempGrassDesert:
                popMult = 1.04f;
                terrain = DirtGrassThickGrass(height, temp, rain);
                break;
            case Classification.SubDesert:
                popMult = .39f;
                terrain = Terrain.Dirt;
                break;
            default:
                throw new Exception();
            }

            this.Pop = pop * popMult;
            this.Terrain = terrain;

            rain /= temp;
            if (temp < 125)
                rain = 1 - ( ( 1 - rain ) / ( 130 / ( 130 - temp ) ) );
            else if (temp < 250)
                rain = 1 - ( ( 1 - rain ) / ( 130 / ( temp - 120 ) ) );
            rain *= 500;
            this.Feature = CreateForest(rain, tree, thickTree);
        }

        private Terrain DirtGrassThickGrass(double height, double temp, double rain)
        {
            double chance = Math.Abs(rain - temp + 250);
            if (chance < 39)
            {
                chance = ( 39 - chance ) / 39;
                if (Game.Random.Bool(chance * chance))
                    return Terrain.ThickGrass;
            }
            return DirtGrass(height, temp, rain);
        }
        private Terrain GrassThickGrass(double height, double temp, double rain)
        {
            if (rain > Game.Random.GaussianOE(333, .13, .169))
                return Terrain.ThickGrass;
            return Terrain.Grass;
        }
        private Terrain DirtSnow(double height, double temp, double rain)
        {
            Terrain terrain = DirtGrassSnow(height, temp, rain);
            if (terrain == Terrain.Grass)
                terrain = Terrain.Dirt;
            return terrain;
        }
        private Terrain DirtGrassSnow(double height, double temp, double rain)
        {
            if (height > .91 || ( height > .85 && ( Game.Random.Bool(Math.Sqrt(( height - .85 ) / .15))
                    || ( temp < 250 && Game.Random.Bool(( 250 - temp ) / 250) ) ) ))
                return Terrain.Snow;
            temp = 2 * temp - rain;
            if (temp * temp < Game.Random.GaussianOE(130 * 130, .078, .13, 39))
                return Terrain.Snow;
            return DirtGrass(height, temp, rain);
        }
        private Terrain DirtGrass(double height, double temp, double rain)
        {
            if (rain > Game.Random.GaussianOE(169, .091, .21))
                return Terrain.Grass;
            return Terrain.Dirt;
        }

        private Feature CreateForest(double rain, Feature tree, Feature thickTree)
        {
            rain *= rain;
            double chance = rain / ( rain + 390 * 390 );
            if (Game.Random.Bool(chance))
                if (Game.Random.Bool(chance * chance))
                    return thickTree;
                else
                    return tree;
            return Feature.None;
        }

        private Classification GetClassification(double height, double temp, double rain)
        {
            Classification t;

            if (height % .006f < ( height - .8f ) / 52f)
                t = Classification.Mountain;
            else if (temp < 39)
                t = Classification.Glacier;
            else if (height < .169)
                t = Classification.DeepSea;
            else if (height < .39)
                t = Classification.Sea;
            else if (height < .392)
                t = Classification.Beach;
            else if (height > .97)
                t = Classification.AlpineGlacier;
            else if (height > .87)
                t = Classification.AlpineTundra;
            else if (height > .8)
                t = Classification.AlpineTaiga;
            else if (( temp > 100 ) && ( ( rain < ( temp - 100 ) * ( temp - 100 ) / 1000f ) ))
                if (rain < temp - 313)
                    t = Classification.SubDesert;
                else
                    t = Classification.TempGrassDesert;
            else if (( temp > 300 ) && ( rain < 25 + 490 / ( 1.0 + Math.Pow(Math.E, ( 390 - temp ) / 26.0) ) ))
                if (rain < 260 + ( temp - 333 ) * ( temp - 333 ) / 260f)
                    t = Classification.TropSeasForestSavannah;
                else
                    t = Classification.TropRainForest;
            else if (rain < ( temp - 65 ) / 2.1f)
                t = Classification.WoodlandShrubland;
            else if (( temp > 200 ) || ( ( temp > 130 ) && ( rain < 21 + 196 / ( 1.0 + Math.Pow(Math.E, ( 169 - temp ) / 13.0) ) ) ))
                if (( temp < 169 ) || rain < 125 + 10 * Math.Sqrt(temp - 169))
                    t = Classification.TempDecForest;
                else
                    t = Classification.TempRainForest;
            else if (( temp > 100 ) || ( rain < 120 / ( 1.0 + Math.Pow(Math.E, ( 78 - temp ) / 13.0) ) ))
                t = Classification.Taiga;
            else
                t = Classification.Tundra;

            return t;
        }

        public enum Classification
        {
            Glacier,
            DeepSea,
            Sea,
            Beach,
            AlpineGlacier,
            AlpineTaiga,
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

    public enum Terrain
    {
        SaltWater,
        FreshWater,
        SteepCliff,
        ThickSnow,
        Snow,
        Cliff,
        ThickGrass,
        Grass,
        Dirt,
        Floor,
    }

    public enum Feature
    {
        None,
        ThickDeciduousTrees,
        ThickEvergreenTrees,
        DeciduousTrees,
        EvergreenTrees,
    }
}
