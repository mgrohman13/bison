using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Map
    {
        private const int NumLevels = 9;
        private const double BaseDense = 1.0 / 169, DenseDiv = 2.1;
        private const float IndAmp = 1.3f, AmpMult = 1.13f;
        private double CreateDist = 1 / Math.Sqrt(BaseDense / Math.Pow(DenseDiv, NumLevels - 1));
        private const double DistRndm = .21, DenseRndm = .065;

        public double minX, maxX, minY, maxY;
        private readonly VoronoiGraph[] graphs;
        private readonly List<HeightPoint>[] levels;
        private readonly HashSet<PointD>[] completed;
        private readonly Dictionary<Point, double[]> heights;
        private readonly Dictionary<Point, double> final;

        public Map()
        {
            this.graphs = new VoronoiGraph[NumLevels];
            this.heights = new Dictionary<Point, double[]>();
            this.final = new Dictionary<Point, double>();
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
            //OnMove(new Point(0, 0));
        }

        public void OnMove(Point point)
        {
            double minX = maxX;
            maxX += GetCreateDist();
            CreatePoints(minX, maxX, minY, maxY);
        }

        public void DrawAll(Point topLeft, int width, int height)
        {
            if (Game.Random.Bool())
            {
                int[,] data = new int[width, height];

                for (int level = 0 ; level < NumLevels ; ++level)
                    foreach (VoronoiEdge e in graphs[level].Edges)
                        if (e.VertexA != null && e.VertexB != null)
                        {
                            PointD r = e.VertexA.Point;
                            PointD l = e.VertexB.Point;
                            double a = Math.Abs(r.X - l.X), b = Math.Abs(r.Y - l.Y);
                            double dist = Math.Sqrt(a * a + b * b);
                            for (int c = 1 ; c < dist ; c++)
                                DoPt(data, ( r.X * c + l.X * ( dist - c ) ) / dist - topLeft.X, ( r.Y * c + l.Y * ( dist - c ) ) / dist - topLeft.Y, level);
                        }

                for (int level = 0 ; level < NumLevels ; ++level)
                    foreach (HeightPoint p in levels[level])
                        DoPt(data, p.point.X - topLeft.X, p.point.Y - topLeft.Y, level);

                Console.SetCursorPosition(0, 0);
                for (int y = topLeft.Y ; y < topLeft.Y + height ; ++y)
                    for (int x = topLeft.X ; x < topLeft.X + width ; ++x)
                    {
                        Console.BackgroundColor = (ConsoleColor)( data[x - topLeft.X, y - topLeft.Y] );
                        Console.Write(' ');
                    }
                Console.SetCursorPosition(0, 0);
            }
            else
            {
                for (int y = topLeft.Y ; y < topLeft.Y + height ; ++y)
                    for (int x = topLeft.X ; x < topLeft.X + width ; ++x)
                    {
                        double h = 0;
                        double[] l;
                        if (this.heights.TryGetValue(new Point(x, y), out l))
                            foreach (double d in l)
                                h += d;
                        Console.BackgroundColor = (ConsoleColor)( h );
                        Console.Write(' ');
                    }
                Console.SetCursorPosition(0, 0);
            }
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
                            ++count;
                    if (count % 2 == 1)
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

                        foreach (HeightPoint hp in this.levels[level])
                            if (hp.point == close.Point)
                            {
                                Point p = new Point(x, y);
                                double[] l;
                                if (!heights.TryGetValue(p, out l))
                                {
                                    l = new double[NumLevels];
                                    heights.Add(p, l);
                                }
                                if (l[level] > 0)
                                    throw new Exception();
                                l[level] = hp.height;
                                double tot = 0;
                                bool comp = true;
                                foreach (double d in l)
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
                            }
                    }
                }
        }

        private bool RayIntersectsSegment(Point P, VoronoiEdge e)
        {
            PointD A, B;
            if (e.VertexA.Point.Y < e.VertexB.Point.Y)
            {
                A = e.VertexA.Point;
                B = e.VertexB.Point;
            }
            else
            {
                A = e.VertexB.Point;
                B = e.VertexA.Point;
            }

            if (P.Y < A.Y || P.Y > B.Y)
                return false;
            if (P.X > Math.Max(A.X, B.X))
                return false;

            if (P.X < Math.Min(A.X, B.X))
                return true;

            double m_red;
            if (A.X != B.X)
                m_red = ( B.Y - A.Y ) / ( B.X - A.X );
            else
                m_red = double.PositiveInfinity;
            double m_blue;
            if (A.X != P.X)
                m_blue = ( P.Y - A.Y ) / ( P.X - A.X );
            else
                m_blue = double.PositiveInfinity;
            if (m_blue >= m_red)
                return true;
            else
                return false;
        }

        private void GetMinMax(ref int min, ref int max, double v)
        {
            min = Math.Min(min, (int)v - 1);
            max = Math.Max(max, (int)v + 1);
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
                    if (hp.point == v.Point)
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
            public PointD point;
            public float height;
            public HeightPoint(double minX, double maxX, double minY, double maxY, float height)
            {
                this.point = new PointD(Game.Random.Range(minX, maxX), Game.Random.Range(minY, maxY));
                this.height = height * ( 1 - Game.Random.NextFloat() );
            }
            public override string ToString()
            {
                return string.Format("{0} - {1:F2}", point, height);
            }
        }
    }
}
