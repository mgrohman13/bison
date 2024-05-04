using MattUtil;
using System;
using System.Collections.Generic;

namespace ClassLibrary1.Map
{
    public partial class Map
    {

        [Serializable]
        private class Path : IEnemySpawn
        {

            public readonly double Angle;
            public readonly PointD Left, Right;

            private readonly SpawnChance spawn = new();
            //public readonly double EnemyMult, EnemyPow;

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

            public HashSet<Tile> Explore(Tile tile, double vision)
            {
                return Explore(tile.Map, GetExploredDist(tile, vision));
            }
            public HashSet<Tile> Explore(Map map, double dist)
            {
                ExploredDist = Math.Max(ExploredDist, dist);

                //retry on duplicate tile?
                HashSet<Tile> tiles = new();
                foreach (double distance in Game.Rand.Iterate(CreateResources()))
                    tiles.Add(map.SpawnTile(GetPoint(Angle, distance), Consts.PathWidth, false));
                return tiles;
            }
            private double GetExploredDist(Tile tile, double vision)
            {
                PointD closest = GetClosestPoint(tile.X, tile.Y);
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
            private IEnumerable<double> CreateResources()
            {
                double generationBuffer = 2.1 * (Consts.PathWidth + Consts.ResourceAvgDist);
                if (Game.TEST_MAP_GEN.HasValue)
                    generationBuffer = Game.TEST_MAP_GEN.Value;

                List<double> create = new();
                while (ExploredDist + generationBuffer > NextResourceDist)
                {
                    create.Add(NextResourceDist);
                    ResourceNum++;
                    GetNextDist();
                }
                return create;
            }

            //calculates the line equation in the format ax + by + c = 0 
            public void CalcLine(PointD start, out double a, out double b, out double c)
            {
                a = Math.Tan(Angle);
                b = -1;
                c = start.Y - a * start.X;
            }

            public void Turn(int turn) => spawn.Turn(turn);
            public int SpawnChance(int turn, double? enemyMove) => spawn.Chance * 3;
            public Tile SpawnTile(Map map, bool isEnemy, double deviationMult)
                => map.SpawnTile(ExploredPoint(), Consts.PathWidth * deviationMult, isEnemy);
            public PointD ExploredPoint(double buffer = 0) => GetPoint(Angle, ExploredDist + buffer);

            public override string ToString() => "Path " + (float)Angle;
        }
    }
}
