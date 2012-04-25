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
        private Dictionary<Point, float[]> buffer;

        public Map()
        {
            this.final = new Dictionary<Point, Tile>();
            this.buffer = new Dictionary<Point, float[]>();

            this.types = new TileValue[3];
            this.types[0] = new Height(this);
            this.types[1] = new Temperature(this);
            this.types[2] = new Rainfall(this);
        }

        public void Explore(Point p)
        {
            if (!this.final.ContainsKey(p))
            {
                Tile t = Get(p);
                if (!t.IsWater)
                    FillFlow(p);
            }
        }

        private void FillFlow(Point orig)
        {
            HashSet<Point> done = new HashSet<Point>();

            SortedDictionary<float, Point> queue = new SortedDictionary<float, Point>();
            AddToQueue(queue, orig);

            Dictionary<Point, Point> flow = new Dictionary<Point, Point>();

            while (queue.Count > 0)
            {
                KeyValuePair<float, Point> pair = ReadQueue(queue);
                Point p1 = pair.Value;

                bool stop = true;
                for (int dir = 0 ; dir < 4 ; ++dir)
                {
                    Point p2 = GetNeighbor(p1, dir);
                    float[] val;
                    if (!done.Contains(p2) && buffer.TryGetValue(p2, out val) && val[0] > pair.Key)
                    {
                        Point? down = GetFlowsDown(flow, p2);
                        if (down.Value == p1)
                        {
                            AddToQueue(queue, p2);
                            stop = false;
                        }
                        AddFlow(flow, p2, down.Value);
                    }
                }

                if (stop)
                {
                    done.Add(p1);
                    Point? p2 = GetFlowsDown(flow, p1);
                    if (p2.HasValue)
                    {
                        if (!Get(p2.Value).IsWater)
                        {
                            AddFlow(p1, p2.Value);
                            AddFlow(flow, p1, p2.Value);
                            AddToQueue(queue, p2.Value);
                        }
                    }
                    else
                    {
                        if (queue.Count > 0)
                            throw new Exception();

                        //fill lake
                        if (buffer.ContainsKey(p1) && buffer[p1][2] / buffer[p1][1] * Math.Sqrt(buffer[p1][1] / 500) > 1.69)
                        {
                            AddToQueue(queue, p1, -1);
                            while (queue.Count > 0)
                            {
                                KeyValuePair<float, Point> pair2 = ReadQueue(queue);
                                Point p3 = pair2.Value;
                                Point p4;
                                while (flow.TryGetValue(p3, out p4))
                                    p3 = p4;
                                if (p1 == p3)
                                {
                                    p3 = pair2.Value;
                                    final[p3] = new Tile(Terrain.FreshWater);
                                    buffer.Remove(p3);
                                    done.Add(p3);
                                    for (int dir = 0 ; dir < 4 ; ++dir)
                                    {
                                        Point p5 = GetNeighbor(p3, dir);
                                        if (buffer.ContainsKey(p5) && this.buffer[p5][0] > this.buffer[p3][0])
                                            AddToQueue(queue, p5, -1);
                                    }
                                }
                                else
                                {
                                    AddFlow(p1, p3);
                                    AddFlow(flow, p1, p3);
                                    queue.Clear();
                                    AddToQueue(queue, p3);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach (Point p in done)
            {
                if (!this.final[p].IsWater)
                {
                    //river
                    if (buffer[p][2] / buffer[p][1] * Math.Sqrt(buffer[p][1] / 500) > 1.69 && final[p].Terrain != Terrain.Sea)
                        final[p] = new Tile(Terrain.FreshWater);

                    buffer.Remove(p);
                }
            }
        }

        private static KeyValuePair<float, Point> ReadQueue(SortedDictionary<float, Point> queue)
        {
            IEnumerator<KeyValuePair<float, Point>> enumerator = queue.GetEnumerator();
            enumerator.MoveNext();
            KeyValuePair<float, Point> pair = enumerator.Current;
            queue.Remove(pair.Key);
            return pair;
        }

        private void AddFlow(Dictionary<Point, Point> flow, Point from, Point to)
        {
            if (!flow.ContainsKey(from))
            {
                Point val;
                if (flow.TryGetValue(to, out val))
                    to = val;
                flow.Add(from, to);
            }
        }

        private Point AddFlow(Point from, Point to)
        {
            double rain = buffer[from][2] / buffer[from][1] * Math.Sqrt(buffer[from][1] / 500);
            if (rain < 1)
                rain *= rain * rain;
            if (rain > 0)
                buffer[to][2] += (float)( rain * buffer[to][1] );
            return from;
        }

        private void AddToQueue(SortedDictionary<float, Point> queue, Point p)
        {
            AddToQueue(queue, p, 1);
        }

        private void AddToQueue(SortedDictionary<float, Point> queue, Point p, float mult)
        {
            if (this.buffer.ContainsKey(p))
                queue[this.buffer[p][0] * mult] = p;
        }

        private Point? GetFlowsDown(Dictionary<Point, Point> flow, Point p1)
        {
            //Point p3;
            //if (flow.TryGetValue(p1, out p3))
            //    return p3;

            float val = float.MaxValue;
            Point lowest = p1;
            for (int dir = 0 ; dir < 4 ; ++dir)
            {
                Point p2 = GetNeighbor(p1, dir);
                float[] v2;
                if (buffer.TryGetValue(p2, out v2) && v2[0] < val)
                {
                    val = buffer[p2][0];
                    lowest = p2;
                }
            }
            if (val < buffer[p1][0])
                return lowest;
            return null;
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

        private Tile Get(Point p)
        {
            if (!this.final.ContainsKey(p))
            {
                float height = types[0].Get(p);
                float temp = types[1].Get(p);
                float rain = types[2].Get(p);
                if (height > .39 && height < .5)
                {
                    rain = (float)Math.Pow(rain, 1 + ( height - .5 ) * 7.8);
                }
                else if (height > .75 && height < .87)
                {
                    rain = (float)Math.Pow(rain, 1 + ( .75 - height ) * 3.9);
                    temp -= .13f;
                    if (temp > 0)
                        temp = (float)Math.Pow(temp, 1 + ( height - .75 ) * 6.5);
                    temp += .13f;
                }
                if (temp < 0)
                    temp = 0;
                else if (temp > 1)
                    temp = 1;
                temp *= 500;
                rain *= temp;

                Tile tile = new Tile(height, temp, rain, p);
                this.final[p] = tile;

                if (!tile.IsWater)
                    this.buffer.Add(p, new float[] { height, temp, rain });
            }

            return this.final[p];
        }

        public void DrawAll(Point topLeft, int width, int height)
        {
            for (int y = topLeft.Y ; y < topLeft.Y + height ; ++y)
                for (int x = topLeft.X ; x < topLeft.X + width ; ++x)
                {
                    Tile t;
                    if (this.final.TryGetValue(new Point(x, y), out t))
                        switch (t.Terrain)
                        {
                        case Terrain.Glacier:
                            Console.BackgroundColor = ConsoleColor.White;
                            break;
                        case Terrain.DeepSea:
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            break;
                        case Terrain.Sea:
                            Console.BackgroundColor = ConsoleColor.Blue;
                            break;
                        case Terrain.AlpineGlacier:
                            Console.BackgroundColor = ConsoleColor.White;
                            break;
                        case Terrain.Mountain:
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            break;
                        case Terrain.AlpineTundra:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            break;
                        case Terrain.SubDesert:
                            Console.BackgroundColor = ConsoleColor.Red;
                            break;
                        case Terrain.TempGrassDesert:
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            break;
                        case Terrain.TropSeasForestSavannah:
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            break;
                        case Terrain.TropRainForest:
                            Console.BackgroundColor = ConsoleColor.Cyan;
                            break;
                        case Terrain.WoodlandShrubland:
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            break;
                        case Terrain.TempDecForest:
                            Console.BackgroundColor = ConsoleColor.Green;
                            break;
                        case Terrain.TempRainForest:
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            break;
                        case Terrain.Taiga:
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                            break;
                        case Terrain.Tundra:
                            Console.BackgroundColor = ConsoleColor.Gray;
                            break;
                        case Terrain.FreshWater:
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            break;
                        default:
                            throw new Exception();
                        }
                    else
                        Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(' ');
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
            private float freqMult, ampMult, smooth;
            private float avg;
            private float topLevel;

            private uint[] seeds;
            private float[] rotate;

            private Dictionary<Point, float> values;

            private Map map;

            protected TileValue(byte numLevels, float freqMult, float ampMult, float smooth, byte smoothDist, Map map)
            {
                this.map = map;

                this.numLevels = numLevels;
                this.smoothDist = smoothDist;
                this.freqMult = freqMult;
                this.ampMult = ampMult;
                this.smooth = smooth;

                this.seeds = new uint[numLevels];
                this.rotate = new float[numLevels];

                this.values = new Dictionary<Point, float>();

                double amp = 1, avg = amp;
                for (int level = 0 ; level < numLevels ; ++level)
                {
                    this.seeds[level] = Game.Random.NextUInt();
                    this.rotate[level] = Game.Random.DoubleFull((float)Math.PI);
                    avg += ( amp *= ampMult );
                }

                this.avg = (float)( avg );
                this.topLevel = (float)Math.Pow(ampMult, numLevels);
            }

            private void Set(Point p)
            {
                double amp = 1;
                double freq = 1;
                double value = Game.Random.DoubleHalf(2 * amp);
                for (int level = 0 ; level < numLevels ; ++level)
                {
                    amp *= ampMult;
                    freq *= freqMult;
                    double s = Math.Sin(this.rotate[level]);
                    double c = Math.Cos(this.rotate[level]);
                    value += Noise.GetNoise(this.seeds[level], ( p.X * c - p.Y * s ) / freq, amp);
                    value += Noise.GetNoise(this.seeds[level], ( p.X * s + p.Y * c ) / freq, amp);
                }

                value -= ( this.avg - topLevel );
                value /= this.topLevel * 2;

                this.values[p] = (float)( value );
            }

            public float Get(Point p1)
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

                return (float)( tot / div );
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
                : base(12, 1.6f, 1.5f, 2.6f, 4, map)
            {
            }
        }

        private class Temperature : TileValue
        {
            public Temperature(Map map)
                : base(7, 2.4f, 1.8f, .39f, 2, map)
            {
            }
        }

        private class Rainfall : TileValue
        {
            public Rainfall(Map map)
                : base(9, 1.8f, 1.5f, .13f, 1, map)
            {
            }
        }
    }
}
