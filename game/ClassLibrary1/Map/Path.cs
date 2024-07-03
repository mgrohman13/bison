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
            public readonly PointD Left, Right;

            private readonly SpawnChance _spawn = new();
            //public readonly double EnemyMult, EnemyPow;

            public SpawnChance Spawner => _spawn;

            public int ResourceNum { get; private set; }
            public double ExploredDist { get; private set; }
            public double NextResourceDist { get; private set; }

            public Path(double angle)//, double enemyMult, double enemyPow)
            {
                Angle = angle;
                //this.EnemyMult = enemyMult;
                //this.EnemyPow = enemyPow;

                static double Dist() => Game.Rand.GaussianCapped(Consts.PathWidth, Consts.PathWidthDev, Consts.PathWidthMin);
                PointD GetOrigin(int sign)
                {
                    double dist = Dist();
                    double dir = angle + HALF_PI * sign;
                    return GetPoint(dir, dist);
                }
                Left = GetOrigin(1);
                Right = GetOrigin(-1);

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

                    int energy = Game.Rand.Round(mult * Consts.ExploreEnergy);
                    Debug.WriteLine($"ExploreEnergy: {energy}");
                    map.Game.Enemy.AddEnergy(energy);

                    ExploredDist = dist;

                    CreateResources(map);
                }
            }
            private double GetExploredDist(Point point, double vision)
            {
                PointD closest = GetClosestPoint(point.X, point.Y);
                return Math.Sqrt(closest.X * closest.X + closest.Y * closest.Y) + vision;
            }
            public PointD GetClosestPoint(double x, double y)
            {
                CalcLine(new PointD(0, 0), out double a, out double b, out double c);

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
            public void CalcLine(PointD start, out double a, out double b, out double c)
            {
                a = Math.Tan(Angle);
                b = -1;
                c = start.Y - a * start.X;
            }

            //public void Turn(int turn) => _spawn.Turn(turn);
            public int SpawnChance(int turn, double? enemyMove) => _spawn.Chance;
            public Tile SpawnTile(Map map, ResourceType? type, double deviationMult = 1)
                => map.SpawnTile(ExploredPoint(), Consts.PathWidth * deviationMult, !type.HasValue);
            public PointD ExploredPoint(double buffer = 0) => GetPoint(Angle, ExploredDist + buffer);

            public override string ToString() => "Path " + (float)Angle;
        }
    }
}
