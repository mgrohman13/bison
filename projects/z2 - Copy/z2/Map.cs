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
        private Noise noise;

        private Dictionary<Point, Tile> final;

        public Map()
        {
            this.final = new Dictionary<Point, Tile>();

            this.types = new TileValue[4];
            this.types[0] = new Height(this);
            this.types[1] = new Temperature(this);
            this.types[2] = new Rainfall(this);
            this.types[3] = new Population(this);

            noise = new Noise();
        }

        public Tile Get(Point p)
        {
            if (!this.final.ContainsKey(p))
            {
                double height = types[0].Get(p);
                double temp = types[1].Get(p);
                double rain = types[2].Get(p);
                double pop = types[3].Get(p);

                if (height < .5)
                {
                    rain = Math.Pow(rain, height * 2);
                }
                else
                {
                    rain = Math.Pow(rain, 2 - height * 2);
                    temp = Math.Pow(temp, height * 2);
                }
                rain *= rain;
                temp = Math.Pow(temp, .91);

                Tile tile = new Tile(height, temp, rain, pop, p);
                this.final[p] = tile;
            }

            return this.final[p];
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

        internal bool Unfinished(int x, int y)
        {
            return ( !final.ContainsKey(new Point(x, y)) );
        }

        public abstract class TileValue
        {
            private byte numLevels, smoothDist;
            private double freqMult, ampMult, smooth;
            private double avg;
            private double topLevel;

            private uint[] seeds;
            private double[] rotate;

            private Dictionary<Point, double> values;

            private Map map;

            protected TileValue(byte numLevels, double freqMult, double ampMult, double smooth, byte smoothDist, Map map)
            {
                this.map = map;

                this.numLevels = numLevels;
                this.smoothDist = smoothDist;
                this.freqMult = freqMult;
                this.ampMult = ampMult;
                this.smooth = smooth;

                this.seeds = new uint[numLevels];
                this.rotate = new double[numLevels];

                this.values = new Dictionary<Point, double>();

                double amp = 1, avg = amp;
                for (int level = 0 ; level < numLevels ; ++level)
                {
                    this.seeds[level] = Game.Random.NextUInt();
                    this.rotate[level] = ( Game.Random.NextDouble() * 2 * Math.PI );
                    avg += ( amp *= ampMult );
                }

                this.avg = avg;
                this.topLevel = Math.Pow(ampMult, numLevels);
            }

            private void Set(Point p)
            {
                double amp = 1;
                double freq = 1;
                double value = Game.Random.DoubleHalf(amp);
                for (int level = 0 ; level < numLevels ; ++level)
                {
                    amp *= ampMult;
                    freq *= freqMult;
                    double s = Math.Sin(this.rotate[level]);
                    double c = Math.Cos(this.rotate[level]);
                    value += map.noise.GetNoise(( p.X * c - p.Y * s ) / freq, this.seeds[level]) * amp;
                    value += map.noise.GetNoise(( p.X * s + p.Y * c ) / freq, this.seeds[level]) * amp;
                }

                value -= ( this.avg - topLevel );
                value /= this.topLevel * 2;

                this.values[p] = value;
            }

            public double Get(Point p1)
            {
                int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;

                double tot = 0, div = 0;
                for (int x = p1.X - smoothDist ; x <= p1.X + smoothDist ; ++x)
                    for (int y = p1.Y - smoothDist ; y <= p1.Y + smoothDist ; ++y)
                    {
                        Point p2 = new Point(x, y);
                        if (!this.values.ContainsKey(p2))
                        {
                            Set(p2);
                            minX = Math.Min(minX, p2.X);
                            maxX = Math.Max(maxX, p2.X);
                            minY = Math.Min(minY, p2.Y);
                            maxY = Math.Max(maxY, p2.Y);
                        }

                        double dist = 1 / ( ( p1.X - x ) * ( p1.X - x ) + ( p1.Y - y ) * ( p1.Y - y ) + smooth );
                        tot += this.values[p2] * dist;
                        div += dist;
                    }

                Remove(minX - smoothDist, maxX + smoothDist, minY - smoothDist, maxY + smoothDist);

                double temp = tot / div;
                tot *= 2 / div;
                bool neg = ( tot > 1 );
                if (neg)
                    tot = 2 - tot;
                tot = Math.Pow(tot, Math.Sqrt(numLevels));
                if (neg)
                    tot = 2 - tot;
                tot /= 2;
                if (tot < .0001)
                    tot = .0001;
                else if (tot > .9999)
                    tot = .9999;
                return tot;
            }

            private void Remove(int minX, int maxX, int minY, int maxY)
            {
                for (int x = minX ; x <= maxX ; ++x)
                    for (int y = minY ; y <= maxY ; ++y)
                    {
                        bool all = true;
                        for (int x2 = x - smoothDist ; x2 <= x + smoothDist ; ++x2)
                            for (int y2 = y - smoothDist ; y2 <= y + smoothDist ; ++y2)
                                if (map.Unfinished(x2, y2))
                                {
                                    all = false;
                                    break;
                                }
                        if (all)
                            this.values.Remove(new Point(x, y));
                    }
            }
        }

        private class Height : TileValue
        {
            public Height(Map map)
                : base(3, 5.2f, 3.9f, 2.10f, 4, map)
            {
            }
        }
        private class Temperature : TileValue
        {
            public Temperature(Map map)
                : base(4, 3.9f, 3.0f, 0.78f, 3, map)
            {
            }
        }
        private class Rainfall : TileValue
        {
            public Rainfall(Map map)
                : base(6, 2.1f, 1.8f, 0.52f, 2, map)
            {
            }
        }
        private class Population : TileValue
        {
            public Population(Map map)
                : base(5, 2.6f, 2.1f, 0.13f, 1, map)
            {
            }
        }
    }
}
