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
        private readonly List<HeightPoint>[] levels;
        private readonly VoronoiGraph[] graphs;

        public Map()
        {
            this.levels = new List<HeightPoint>[NumLevels];
            this.graphs = new VoronoiGraph[NumLevels];
            for (int level = 0 ; level < NumLevels ; ++level)
                this.levels[level] = new List<HeightPoint>();
            this.minX = -GetCreateDist();
            this.maxX = GetCreateDist();
            this.minY = -GetCreateDist();
            this.maxY = GetCreateDist();

            CreateStartPoints();
            OnMove(new Point(0, 0));
        }

        public void OnMove(Point point)
        {
        }

        public void DrawAll(Point topLeft, int width, int height)
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
                    Console.BackgroundColor = (ConsoleColor)data[x - topLeft.X, y - topLeft.Y];
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
                graphs[level] = Voronoi.GetVoronoiGraph(levels[level], minX, maxX, minY, maxY);
        }

        private void DoPt(int[,] data, double xx, double yy, int level)
        {
            int x = (int)Math.Round(xx);
            int y = (int)Math.Round(yy);
            if (x >= 0 && x < data.GetLength(0) && y >= 0 && y < data.GetLength(1))
                data[x, y] = level;
        }

        public class HeightPoint
        {
            public PointD point;
            public float height;
            public HeightPoint(double minX, double maxX, double minY, double maxY, float height)
            {
                this.point = new PointD(Game.Random.Range(minX, maxX), Game.Random.Range(minY, maxY));
                this.height = Game.Random.DoubleHalf(height);
            }
            public override string ToString()
            {
                return string.Format("{0} - {1:F2}", point, height);
            }
        }
    }
}
