using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Map
    {
        private const int NumLevels = 6;
        private const double BaseDense = 1.0 / 169, DenseDiv = 2.1;
        private const float IndAmp = 1.3f, AmpMult = 1.13f;
        private double CreateDist = 1 / Math.Sqrt(BaseDense / Math.Pow(DenseDiv, NumLevels - 1));
        private const double DistRndm = .21, DenseRndm = .065;

        public double minX, maxX, minY, maxY;
        private readonly VoronoiGraph[] graphs;
        private readonly List<HeightPoint>[] levels;
        private readonly HashSet<PointD>[] completed;
        private readonly Dictionary<Point, float[]> heights;
        private readonly Dictionary<Point, float> final;

        public Map()
        {
            this.graphs = new VoronoiGraph[NumLevels];
            this.heights = new Dictionary<Point, float[]>();
            this.final = new Dictionary<Point, float>();
            this.levels = new List<HeightPoint>[NumLevels];
            this.completed = new HashSet<PointD>[NumLevels];
            for (int level = 0 ; level < NumLevels ; ++level)
            {
                this.levels[level] = new List<HeightPoint>();
                this.completed[level] = new HashSet<PointD>();
            }
            this.minX = -GetCreateDist();
            this.maxX = GetCreateDist();
            this.minY = -GetCreateDist();
            this.maxY = GetCreateDist();

            CreateStartPoints();
            OnMove(new Point(0, 0));
        }

        public void OnMove(Point p1)
        {
            Point p2 = p1;
            while (!final.ContainsKey(p1))
            {
                double x = p2.X + Game.Random.Gaussian(), y = p2.Y + Game.Random.Gaussian();
                double dist = GetCreateDist();
                if (Math.Abs(x) > Math.Abs(y))
                    if (x > 0)
                    {
                        double xx = maxX;
                        maxX += dist;
                        CreatePoints(xx, maxX, minY, maxY);
                    }
                    else
                    {
                        double nx = minX;
                        minX -= dist;
                        CreatePoints(minX, nx, minY, maxY);
                    }
                else if (y > 0)
                {
                    double xy = maxY;
                    maxY += dist;
                    CreatePoints(minX, maxX, xy, maxY);
                }
                else
                {
                    double ny = minY;
                    minY -= dist;
                    CreatePoints(minX, maxX, minY, ny);
                }

                if (p1 == p2)
                    p2 = new Point(0, 0);
                else
                    p2 = p1;
            }
        }

        public void DrawAll(Point topLeft, int width, int height)
        {
            for (int y = topLeft.Y ; y < topLeft.Y + height ; ++y)
                for (int x = topLeft.X ; x < topLeft.X + width ; ++x)
                {
                    float h;
                    if (this.final.TryGetValue(new Point(x, y), out h))
                    {
                        if (++h > 15)
                            h = 15;
                        Console.BackgroundColor = (ConsoleColor)( h );
                    }
                    else
                    {
                        Console.BackgroundColor = (ConsoleColor)( 0 );
                    }
                    Console.Write(' ');
                }
            Console.SetCursorPosition(0, 0);
        }

        private void CreateStartPoints()
        {
            CreatePoints(this.minX, this.maxX, this.minY, this.maxY);
        }

        private double GetCreateDist()
        {
            return Game.Random.GaussianCapped(CreateDist, DistRndm, float.Epsilon);
        }

        private void CreatePoints(double minX, double maxX, double minY, double maxY)
        {
            double dense = ( maxX - minX ) * ( maxY - minY ) * BaseDense;
            float amp = 1;
            for (int level = 0 ; level < NumLevels ; ++level)
            {
                int points = Game.Random.GaussianCappedInt(dense, DenseRndm, 0);
                for (int a = 0 ; a < points ; ++a)
                    levels[level].Add(new HeightPoint(minX, maxX, minY, maxY, amp));

                dense /= DenseDiv;
                amp *= AmpMult;
            }

            CreateVoronoi();
        }

        private void CreateVoronoi()
        {
            for (int level = 0 ; level < NumLevels ; ++level)
            {
                VoronoiGraph g = graphs[level] = Voronoi.GetVoronoiGraph(levels[level], minX, maxX, minY, maxY);
                Dictionary<VoronoiVertex, List<VoronoiEdge>> pointToEdge = GetCompletedPoints(g);
                foreach (KeyValuePair<VoronoiVertex, List<VoronoiEdge>> polygon in pointToEdge)
                    if (!this.completed[level].Contains(polygon.Key.Point))
                    {
                        MapCompletedPolygons(level, polygon.Value);
                        this.completed[level].Add(polygon.Key.Point);
                    }
                RemoveInnerPoints(level, g, pointToEdge);
            }
        }

        private static Dictionary<VoronoiVertex, List<VoronoiEdge>> GetCompletedPoints(VoronoiGraph g)
        {
            //list all points
            Dictionary<VoronoiVertex, List<VoronoiEdge>> pointToEdge = new Dictionary<VoronoiVertex, List<VoronoiEdge>>();
            foreach (VoronoiEdge e in g.Edges)
                if (e.VertexA != null && e.VertexB != null)
                {
                    MapEdge(pointToEdge, e, e.LeftPolygon);
                    MapEdge(pointToEdge, e, e.RightPolygon);
                }
            //eliminate points with unfinished edges
            foreach (VoronoiEdge e in g.Edges)
                if (e.VertexA == null || e.VertexB == null)
                {
                    pointToEdge.Remove(e.LeftPolygon);
                    pointToEdge.Remove(e.RightPolygon);
                }
            return pointToEdge;
        }

        private static void MapEdge(Dictionary<VoronoiVertex, List<VoronoiEdge>> pointToEdge, VoronoiEdge e, VoronoiVertex v)
        {
            List<VoronoiEdge> list;
            if (!pointToEdge.TryGetValue(v, out list))
            {
                list = new List<VoronoiEdge>();
                pointToEdge[v] = list;
            }
            list.Add(e);
        }

        private void MapCompletedPolygons(int level, List<VoronoiEdge> polygon)
        {
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            HashSet<VoronoiVertex> options = new HashSet<VoronoiVertex>();
            foreach (VoronoiEdge e in polygon)
            {
                GetMinMax(ref minX, ref maxX, e.VertexA.Point.X);
                GetMinMax(ref minY, ref maxY, e.VertexA.Point.Y);
                GetMinMax(ref minX, ref maxX, e.VertexB.Point.X);
                GetMinMax(ref minY, ref maxY, e.VertexB.Point.Y);

                options.Add(e.LeftPolygon);
                options.Add(e.RightPolygon);
            }

            for (int y = minY ; y <= maxY ; ++y)
                for (int x = minX ; x <= maxX ; ++x)
                {
                    int count = 0;
                    foreach (VoronoiEdge side in polygon)
                        if (RayIntersectsSegment(new Point(x, y), side))
                            if (++count > 1)
                                break;
                    if (count == 1)
                    {
                        double modX = x + Game.Random.Gaussian(), modY = y + Game.Random.Gaussian();

                        double found = int.MaxValue;
                        VoronoiVertex close = null;
                        foreach (VoronoiVertex p in options)
                        {
                            double distX = ( p.Point.X - modX );
                            double distY = ( p.Point.Y - modY );
                            double dist = distX * distX + distY * distY;
                            if (dist < found)
                            {
                                found = dist;
                                close = p;
                            }
                        }

                        bool any = false;
                        foreach (HeightPoint hp in this.levels[level])
                            if (hp.Point == close.Point)
                            {
                                any = true;
                                Point p = new Point(x, y);
                                float[] l;
                                if (!heights.TryGetValue(p, out l))
                                {
                                    l = new float[NumLevels];
                                    heights.Add(p, l);
                                }
                                //if (l[level] > 0)
                                //    throw new Exception();
                                if (l[level] > 0)
                                    l[level] = ( l[level] + hp.height ) / 2f;
                                else
                                    l[level] = hp.height;
                                float tot = 0;
                                bool comp = true;
                                foreach (float d in l)
                                    if (d > 0)
                                    {
                                        tot += d;
                                    }
                                    else
                                    {
                                        comp = false;
                                        break;
                                    }
                                if (comp)
                                {
                                    heights.Remove(p);
                                    final.Add(p, tot);
                                }
                                break;
                            }
                        if (!any)
                            throw new Exception();
                    }
                }
        }

        private bool RayIntersectsSegment(Point ray, VoronoiEdge segment)
        {
            PointD low, high;
            if (segment.VertexA.Point.Y > segment.VertexB.Point.Y)
            {
                low = segment.VertexB.Point;
                high = segment.VertexA.Point;
            }
            else
            {
                low = segment.VertexA.Point;
                high = segment.VertexB.Point;
            }
            return ( ray.Y > low.Y && ray.Y < high.Y && ray.X < Math.Max(low.X, high.X) && ( ray.X < Math.Min(low.X, high.X)
                    || ( ( ray.Y - low.Y ) / ( ray.X - low.X ) >= ( high.Y - low.Y ) / ( high.X - low.X ) ) ) );
        }

        private void GetMinMax(ref int min, ref int max, double v)
        {
            int n = (int)v, x = n;
            if (v > 0)
                ++n;
            else
                --x;
            min = Math.Min(min, n);
            max = Math.Max(max, x);
        }

        private void RemoveInnerPoints(int level, VoronoiGraph g, Dictionary<VoronoiVertex, List<VoronoiEdge>> pointToEdge)
        {
            //include all edges that have the points on both sides included
            HashSet<VoronoiEdge> edges = new HashSet<VoronoiEdge>();
            foreach (VoronoiEdge e in g.Edges)
                if (pointToEdge.ContainsKey(e.LeftPolygon) && pointToEdge.ContainsKey(e.RightPolygon))
                    edges.Add(e);
            //remove any points next to unincluded edges
            foreach (VoronoiEdge e in g.Edges)
                if (!edges.Contains(e))
                {
                    pointToEdge.Remove(e.LeftPolygon);
                    pointToEdge.Remove(e.RightPolygon);
                }

            //remove included points from the diagram
            this.completed[level].RemoveWhere(delegate(PointD hp)
            {
                foreach (VoronoiVertex v in pointToEdge.Keys)
                    if (hp == v.Point)
                        return true;
                return false;
            });
            levels[level].RemoveAll(delegate(HeightPoint hp)
            {
                foreach (VoronoiVertex v in pointToEdge.Keys)
                    if (hp.Point == v.Point)
                        return true;
                return false;
            });
        }

        private void DoPt(int[,] data, double xx, double yy, int level)
        {
            int x = (int)Math.Round(xx);
            int y = (int)Math.Round(yy);
            if (x >= 0 && x < data.GetLength(0) && y >= 0 && y < data.GetLength(1))
                data[x, y] = level + 1;
        }

        public class HeightPoint
        {
            public PointD Point;
            public float height;
            public HeightPoint(double minX, double maxX, double minY, double maxY, float height)
            {
                this.Point = new PointD(Game.Random.Range(minX, maxX), Game.Random.Range(minY, maxY));
                this.height = height * ( 1 - Game.Random.NextFloat() );
            }
            public override string ToString()
            {
                return string.Format("{0} - {1:F2}", Point, height);
            }
        }
    }
}
