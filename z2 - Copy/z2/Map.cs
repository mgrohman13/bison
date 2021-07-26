using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Map
    {
        private TileValue[] types;

        private Dictionary<Point, Tile> final;

        public Map()
        {
            this.final = new Dictionary<Point, Tile>();

            Noise noise = new Noise();

            this.types = new TileValue[4];
            this.types[0] = new Height(noise);
            this.types[1] = new Temperature(noise);
            this.types[2] = new Rainfall(noise);
            this.types[3] = new Population(noise);
        }

        public Tile Get(Point p)
        {
            if (!this.final.ContainsKey(p))
                this.final[p] = new Tile(this,GetHeight(p), GetTemp(p), GetRain(p), GetPop(p), p);

            return this.final[p];
        }
        public double GetPop(Point p)
        {
            double pop = types[3].Get(p);
            return pop;
        }
        public double GetRain(Point p)
        {
            double rain = types[2].Get(p);
            return rain;
        }
        public double GetTemp(Point p)
        {
            double temp = types[1].Get(p);
            return temp;
        }
        public double GetHeight(Point p)
        {
            double height = types[0].Get(p);
            return height;
        }

        private Point GetNeighbor(Point p, int dir)
        {
            switch (dir)
            {
            case 0:
                p = new Point(p.X - 1, p.Y);
                break;
            case 1:
                p = new Point(p.X + 1, p.Y);
                break;
            case 2:
                p = new Point(p.X, p.Y - 1);
                break;
            case 3:
                p = new Point(p.X, p.Y + 1);
                break;
            default:
                throw new Exception();
            }
            Get(p);
            return p;
        }

        //public void DrawAll(Point topLeft, int width, int height)
        //{
        //    for (int y = topLeft.Y ; y < topLeft.Y + height ; ++y)
        //        for (int x = topLeft.X ; x < topLeft.X + width ; ++x)
        //        {
        //            Tile t;
        //            char c;
        //            if (this.final.TryGetValue(new Point(x, y), out t))
        //            {
        //                switch (t.Class)
        //                {
        //                case Tile.Classification.Glacier:
        //                    Console.BackgroundColor = ConsoleColor.White;
        //                    break;
        //                case Tile.Classification.DeepSea:
        //                    Console.BackgroundColor = ConsoleColor.DarkBlue;
        //                    break;
        //                case Tile.Classification.Sea:
        //                    Console.BackgroundColor = ConsoleColor.Blue;
        //                    break;
        //                case Tile.Classification.Beach:
        //                    Console.BackgroundColor = ConsoleColor.Red;
        //                    break;
        //                case Tile.Classification.AlpineGlacier:
        //                    Console.BackgroundColor = ConsoleColor.White;
        //                    break;
        //                case Tile.Classification.AlpineTaiga:
        //                    Console.BackgroundColor = ConsoleColor.DarkGreen;
        //                    break;
        //                case Tile.Classification.Mountain:
        //                    Console.BackgroundColor = ConsoleColor.DarkGray;
        //                    break;
        //                case Tile.Classification.AlpineTundra:
        //                    Console.BackgroundColor = ConsoleColor.Gray;
        //                    break;
        //                case Tile.Classification.SubDesert:
        //                    Console.BackgroundColor = ConsoleColor.Red;
        //                    break;
        //                case Tile.Classification.TempGrassDesert:
        //                    Console.BackgroundColor = ConsoleColor.DarkRed;
        //                    break;
        //                case Tile.Classification.TropSeasForestSavannah:
        //                    Console.BackgroundColor = ConsoleColor.Yellow;
        //                    break;
        //                case Tile.Classification.TropRainForest:
        //                    Console.BackgroundColor = ConsoleColor.Cyan;
        //                    break;
        //                case Tile.Classification.WoodlandShrubland:
        //                    Console.BackgroundColor = ConsoleColor.DarkYellow;
        //                    break;
        //                case Tile.Classification.TempDecForest:
        //                    Console.BackgroundColor = ConsoleColor.Green;
        //                    break;
        //                case Tile.Classification.TempRainForest:
        //                    Console.BackgroundColor = ConsoleColor.DarkCyan;
        //                    break;
        //                case Tile.Classification.Taiga:
        //                    Console.BackgroundColor = ConsoleColor.DarkGreen;
        //                    break;
        //                case Tile.Classification.Tundra:
        //                    Console.BackgroundColor = ConsoleColor.Gray;
        //                    break;
        //                default:
        //                    throw new Exception();
        //                }
        //                switch (t.Feature)
        //                {
        //                case Feature.DeciduousTrees:
        //                    c = '!';
        //                    Console.ForegroundColor = ConsoleColor.Green;
        //                    break;
        //                case Feature.EvergreenTrees:
        //                    c = '!';
        //                    Console.ForegroundColor = ConsoleColor.DarkGreen;
        //                    break;
        //                case Feature.None:
        //                    c = ' ';
        //                    break;
        //                case Feature.ThickDeciduousTrees:
        //                    c = Convert.ToChar(19);
        //                    Console.ForegroundColor = ConsoleColor.Green;
        //                    break;
        //                case Feature.ThickEvergreenTrees:
        //                    c = Convert.ToChar(19);
        //                    Console.ForegroundColor = ConsoleColor.DarkGreen;
        //                    break;
        //                default:
        //                    throw new Exception();
        //                }
        //                if (t.Terrain == Terrain.ThickGrass)
        //                    c = '#';
        //            }
        //            else
        //            {
        //                Console.BackgroundColor = ConsoleColor.Black;
        //                c = ' ';
        //            }
        //            Console.ForegroundColor = ConsoleColor.Black;
        //            Console.Write(c);
        //        }
        //    Console.SetCursorPosition(0, 0);
        //}
        public void DrawAll(Point topLeft, int width, int height)
        {
            for (int y = topLeft.Y ; y < topLeft.Y + height ; ++y)
                for (int x = topLeft.X ; x < topLeft.X + width ; ++x)
                {
                    char c;
                    Tile t;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (this.final.TryGetValue(new Point(x, y), out t))
                    {
                        switch (t.Terrain)
                        {
                        case Terrain.SaltWater:
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            break;
                        case Terrain.FreshWater:
                            Console.BackgroundColor = ConsoleColor.Blue;
                            break;
                        case Terrain.ThickSnow:
                            Console.BackgroundColor = ConsoleColor.White;
                            break;
                        case Terrain.Snow:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            break;
                        case Terrain.SteepCliff:
                            Console.BackgroundColor = ConsoleColor.White;
                            break;
                        case Terrain.Cliff:
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            break;
                        case Terrain.ThickGrass:
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                            break;
                        case Terrain.Grass:
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case Terrain.Dirt:
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case Terrain.Floor:
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        default:
                            throw new Exception();
                        }
                        switch (t.Feature)
                        {
                        case Feature.DeciduousTrees:
                            c = '!';
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case Feature.EvergreenTrees:
                            c = '!';
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        case Feature.None:
                            c = ' ';
                            break;
                        case Feature.ThickDeciduousTrees:
                            c = Convert.ToChar(19);
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case Feature.ThickEvergreenTrees:
                            c = Convert.ToChar(19);
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        default:
                            throw new Exception();
                        }
                    }
                    else
                    {
                        c = '*';
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    Console.Write(c);
                }
            Console.SetCursorPosition(0, 0);
        }

        internal void ClearCache()
        {
            foreach (TileValue value in this.types)
                value.ClearCache();
        }

        public abstract class TileValue
        {
            private byte numLevels;
            private double freqMult, ampMult, smooth, smoothDist;

            private uint[] xSeeds;
            private uint[] ySeeds;
            private double[] rotate;

            private Noise noise;

            [NonSerialized]
            private Dictionary<Point, double> cache;

            protected TileValue(byte numLevels, double freqMult, double ampMult, double smooth, double smoothDist, Noise noise)
            {
                this.noise = noise;

                this.numLevels = numLevels;
                this.smoothDist = smoothDist;
                this.freqMult = freqMult;
                this.ampMult = ampMult;
                this.smooth = smooth;

                this.xSeeds = new uint[numLevels];
                this.ySeeds = new uint[numLevels];
                this.rotate = new double[numLevels];

                for (int level = 0 ; level < numLevels ; ++level)
                {
                    this.xSeeds[level] = Game.Random.NextUInt();
                    this.ySeeds[level] = Game.Random.NextUInt();
                    this.rotate[level] = ( Game.Random.NextDouble() * 2 * Math.PI );
                }

                this.cache = null;
            }

            private double GetRaw(Point p)
            {
                double retVal;

                if (cache == null)
                    cache = new Dictionary<Point, double>();
                if (!cache.TryGetValue(p, out retVal))
                {

                    uint xSeed = 0, ySeed = 0;
                    double amp = 1, freq = 1, value = 0, avg = 0;
                    for (int level = 0 ; level < numLevels ; ++level)
                    {
                        amp *= ampMult;
                        freq *= freqMult;

                        double s = Math.Sin(this.rotate[level]);
                        double c = Math.Cos(this.rotate[level]);
                        xSeed += Noise.Combine(this.rotate[level], this.xSeeds[level]);
                        ySeed += Noise.Combine(this.rotate[level], this.ySeeds[level]);

                        value += noise.GetNoise(( p.X * c - p.Y * Consts.YMult * s ) / freq, this.xSeeds[level]) * amp;
                        value += noise.GetNoise(( p.X * s + p.Y * Consts.YMult * c ) / freq, this.ySeeds[level]) * amp;
                        avg += amp;
                    }

                    value += noise.GetNoise(p.X, xSeed) + noise.GetNoise(p.Y * Consts.YMult, ySeed);
                    ++avg;

                    retVal = value / avg;

                    cache[p] = retVal;
                }

                return retVal;
            }

            public double Get(Point p1)
            {
                double tot = 0, div = 0;
                int dif = (int)( smoothDist );
                for (int x = p1.X - dif ; x <= p1.X + dif ; ++x)
                    for (int y = p1.Y - dif ; y <= p1.Y + dif ; ++y)
                    {
                        double yDist = ( p1.Y - y ) * Consts.YMult;
                        double dist = ( p1.X - x ) * ( p1.X - x ) + yDist * yDist;
                        if (Math.Round(dist) < smoothDist * smoothDist)
                        {
                            dist = 1 / ( dist + smooth );
                            tot += GetRaw(new Point(x, y)) * dist;
                            div += dist;
                        }
                    }

                double value = tot / div;
                bool neg = ( value > 1 );
                if (neg)
                    value = 2 - value;
                value *= value;
                if (neg)
                    value = 2 - value;
                return value / 2;
            }

            internal void ClearCache()
            {
                if (cache != null)
                    cache.Clear();
            }
        }

        private class Height : TileValue
        {
            public Height(Noise noise)
                : base(3, 5.2f, 3.9f, 1.69f, 3.9f, noise)
            {
            }
        }
        private class Temperature : TileValue
        {
            public Temperature(Noise noise)
                : base(4, 3.9f, 3.0f, 0.78f, 2.6f, noise)
            {
            }
        }
        private class Rainfall : TileValue
        {
            public Rainfall(Noise noise)
                : base(6, 2.1f, 1.8f, 0.52f, 2.1f, noise)
            {
            }
        }
        private class Population : TileValue
        {
            public Population(Noise noise)
                : base(5, 2.6f, 2.1f, 0.13f, 1.69f, noise)
            {
            }
        }
    }
}
