using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClassLibrary1.Map
{
    public partial class Map
    {

        [Serializable]
        private class Path : IEnemySpawn
        {

            public readonly double Angle;
            public readonly PointD Start;//, Right;
            public readonly double Width;

            private readonly SpawnChance _spawn = new();
            //public readonly double EnemyMult, EnemyPow;

            public SpawnChance Spawner => _spawn;

            public int ResourceNum { get; private set; }
            public double ExploredDist { get; private set; }
            public double NextResourceDist { get; private set; }

            private double K;

            public Path(double angle)//, double enemyMult, double enemyPow)
            {
                Angle = angle;
                //this.EnemyMult = enemyMult;
                //this.EnemyPow = enemyPow;

                Width = Game.Rand.GaussianCapped(Consts.PathWidth, Consts.PathWidthDev, Consts.PathWidthMin);
                PointD GetOrigin(int sign)
                {
                    double dist = Width;
                    double dir = angle + Game.Rand.GaussianCapped(HALF_PI, Consts.PathWidthDev) * sign;
                    return GetPoint(dir, dist);
                }
                Start = GetOrigin(Game.Rand.Bool() ? 1 : -1);
                //Right = GetOrigin(-1);

                K = Game.Rand.GaussianOE(1, 1 / Math.PI, .5);
                Debug.WriteLine("Angle: " + Angle);
                Debug.WriteLine("K: " + K);
                Debug.WriteLine("Width: " + Width);

                //Debug.WriteLine(Left);
                //Debug.WriteLine(Right);

                ResourceNum = 0;
                ExploredDist = 0;
                NextResourceDist = 0;
                GetNextDist();
            }
            private void GetNextDist()
            {
                double avg = ResourceNum * Consts.ResourceAvgDist + Consts.PathWidth;
                double inc = Math.Max(avg - NextResourceDist, 0) + Consts.ResourceAvgDist / 2.0;
                double oe = Math.Min(inc, Consts.ResourceAvgDist);
                inc -= oe;
                NextResourceDist += Game.Rand.DoubleFull(inc) + Game.Rand.OE(oe);
            }

            public void Explore(Map map, Point point, double vision)
            {
                Explore(map, GetExploredDist(point, vision));
            }
            public void Explore(Map map, double dist)
            {
                if (dist > ExploredDist)
                {
                    double mult = (dist - ExploredDist) * dist / Consts.CaveDistance / Consts.ResourceAvgDist;

                    _spawn.Mult(1 + mult);

                    double energy = mult * Consts.ExploreEnergy;
                    Debug.WriteLine($"ExploreEnergy: {energy}");
                    map.Game.Enemy.Income(energy);

                    ExploredDist = dist;

                    CreateResources(map);
                }
            }
            private double GetExploredDist(Point point, double vision)
            {
                PointD closest = GetClosestPoint(point.X, point.Y);
                return Math.Sqrt(closest.X * closest.X + closest.Y * closest.Y) + vision; //centered on (0,0)
            }
            public PointD GetClosestPoint(double x, double y)
            {
                CalcLine(new PointD(0, 0), out double a, out double b, out double c); //centered on (0,0)

                double div = a * a + b * b;
                double lineX = (b * (+b * x - a * y) - a * c) / div;
                double lineY = (a * (-b * x + a * y) - b * c) / div;
                double path = GetAngle(lineX, lineY);
                //check can be againsts any arbitrarily small value - angle will either be equal or opposite
                if (GetAngleDiff(path, Angle) < HALF_PI)
                    return new(lineX, lineY);
                return new(0, 0);
            }
            private void CreateResources(Map map)
            {
                double generationBuffer = Consts.ResourceAvgDist;
                if (Game.TEST_MAP_GEN.HasValue)
                    generationBuffer = Game.TEST_MAP_GEN.Value;

                List<double> create = new();
                while (ExploredDist + generationBuffer > NextResourceDist)
                {
                    map.GenResources(_ => map.SpawnTile(GetPoint(Angle, NextResourceDist), Consts.PathWidth, false));

                    ResourceNum++;
                    GetNextDist();
                }
            }

            //calculates the line equation in the format ax + by + c = 0 
            public void CalcLine(PointD start, out double a, out double b, out double c) =>
                CalcLine(start, Angle, out a, out b, out c);
            public static void CalcLine(PointD start, double angle, out double a, out double b, out double c)
            {
                a = Math.Tan(angle);
                b = -1;
                c = start.Y - a * start.X;
            }

            //public void Turn(int turn) => _spawn.Turn(turn);
            public int SpawnChance(int turn, double? enemyMove) => _spawn.Chance;
            public Tile SpawnTile(Map map)
                => map.SpawnTile(ExploredPoint(), Consts.PathWidth * 1.69, true);
            public PointD ExploredPoint(double buffer = 0) => GetPoint(Angle, ExploredDist + buffer);

            public override string ToString() => "Path " + (float)Angle;

            internal double Evaluate(Point p)
            {
                double Logistic(double dist) => Map.Logistic(dist, K, Width);

                double backMult = 1;
                double direction = PointLineDistanceSigned(Start, Angle + Map.HALF_PI, p);
                if (direction < 0)
                    backMult = Logistic(1 + Math.Abs(direction));

                double dist = PointLineDistanceSigned(Start, Angle, p);
                double mult = Logistic(Math.Abs(dist)) * backMult;
                if (mult > 1)
                    mult = Math.Pow(mult, Math.Log(Math.E) / Math.Log(Logistic(0)));
                //if (mult < 1)
                //{
                //    Math.Log(Math.E) / Math.Log(Logistic(2))
                //}
                return mult;
            }
        }
    }
}
