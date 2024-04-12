using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Point = MattUtil.Point;

namespace ClassLibrary1.Map
{
    public partial class Map
    {
        [Serializable]
        private class Cave : IEnemySpawn
        {
            public readonly PointD Center;
            private readonly double[] shape;

            private readonly PointD seg1, seg2;
            private readonly double segSize;

            private readonly SpawnChance spawn = new();
            private readonly List<Hive> hives = new();

            private bool explored;//, spawned;

            private double minSpawnMove = double.NaN;
            //private List<Point> pathToCore = null;

            //PathCenter should handle edge distances better (e.g. segment on other side of cave/path)
            public PointD PathCenter => new((seg1.X + seg2.X) / 2.0, (seg1.Y + seg2.Y) / 2.0);
            public double PathLength => Math.Sqrt(GetDistSqr(seg1, seg2));

            public double MinSpawnMove => minSpawnMove;
            //public Point PathFindStart => pathToCore.First();
            //public IReadOnlyList<Point> PathToCore => pathToCore.AsReadOnly();

            public Cave(PointD center, PointD connectTo, bool connectCave = false)
            {
                double off2 = connectCave ? Consts.CaveSize : 1.3 * Consts.PathWidth;
                static double Offset(double amt) => Game.Rand.Gaussian(amt / 2.1);

                Center = center;
                seg1 = new(center.X + Offset(Consts.CaveSize), center.Y + Offset(Consts.CaveSize));
                seg2 = new(connectTo.X + Offset(off2), connectTo.Y + Offset(off2));
                segSize = Game.Rand.GaussianOE(Consts.CavePathSize, .13, .13, 1.3);

                shape = new[] { Game.Rand.GaussianOE(1.69, .39, .13), 1 + Game.Rand.GaussianOEInt(1.3, .26, .26), Game.Rand.NextDouble() * TWO_PI,
                    Game.Rand.GaussianCapped(1.3, .26), Game.Rand.GaussianOE(6.5, .39, .13), Game.Rand.GaussianCapped(1, .13, .5) };
                if (shape[1] != (int)shape[1])
                    throw new Exception();
            }

            public void Explore(Tile tile, double vision)
            {
                explored |= GetDistSqr(new(tile.X, tile.Y), Center) < vision * vision;
            }

            public double GetMult(int x, int y)
            {
                double offset = 1.3 + shape[0];
                double s = offset + Math.Sin((GetAngle(Center.X - x, Center.Y - y) + Math.PI) * shape[1] + shape[2]);
                s *= s;
                double distance = GetDistSqr(x, y, Center) / s;
                double centerMult = GetMult(distance, shape[5] * Consts.CaveSize / offset, shape[3]);

                double connection = GetMult(PointLineDistSqr(seg1, seg2, new(x, y)), segSize, shape[4]);

                return centerMult + connection;

                static double GetMult(double distSqr, double size, double o) =>
                    (Math.Pow(size, Consts.CaveDistPow) + o) / (Math.Pow(distSqr, Consts.CaveDistPow / 2.0) + o);
            }
            private static double PointLineDistSqr(PointD v, PointD w, PointD p)
            {
                var l2 = GetDistSqr(v, w);
                if (l2 == 0) return GetDistSqr(p, v);
                var t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
                t = Math.Max(0, Math.Min(1, t));
                return GetDistSqr(p, new(v.X + t * (w.X - v.X), v.Y + t * (w.Y - v.Y)));
            }

            public void AddHive(Hive hive)
            {
                hives.Add(hive);
            }
            public void PathFind(Map map)//, Tile to)
            {
                this.minSpawnMove = 1;
                List<Point> path;
                while (true)
                {
                    Tile from = map.SpawnTile(Center, Math.Sqrt(2), true);
                    path = map.PathFind(from, minSpawnMove, blocked => //, to
                    {
                        if (minSpawnMove > Constructor.BASE_MOVE_MAX)
                            return true;
                        double penalty = 1;
                        foreach (var p in blocked)
                        {
                            Tile tile = map.GetTile(p);
                            double div = 1;
                            if (tile == null)
                                div = 1 + Consts.CaveSize * Consts.CaveSize;
                            else if (tile.Piece is Terrain)
                                div = 1 + Consts.CaveSize / minSpawnMove;
                            if (div > 1)
                                penalty += div;
                        }
                        return Game.Rand.Bool(1 / penalty);
                    });
                    if (path == null)
                        this.minSpawnMove += Math.Sqrt(1 + minSpawnMove) - 1 + Game.Rand.OE();
                    else
                        break;
                }
                //this.pathToCore = path;
            }

            public void Turn(int turn)
            {
                spawn.Turn(turn);
            }
            public int SpawnChance(int turn, double? enemyMove)
            {
                if (enemyMove.HasValue && enemyMove.Value < minSpawnMove)
                    return 0;
                if (!explored || hives.Any(h => !h.Dead))// && spawned)
                    return Game.Rand.Round(spawn.Chance * Math.Sqrt(2.1 + hives.Count));
                return 0;
            }
            public Tile SpawnTile(Map map, bool isEnemy, double deviationMult = 1)
            {
                //this.spawned = true;

                bool inPath = !isEnemy && Game.Rand.Bool(.169);
                PointD spawnCenter = inPath ? PathCenter : Center;
                double deviation = deviationMult * (inPath ? PathLength / 6.5 : Consts.CaveSize);
                Tile tile = map.SpawnTile(spawnCenter, deviation, isEnemy);
                if (!isEnemy)
                    Debug.WriteLine($"Cave resource ({inPath}): {tile} ({Math.Sqrt(GetDistSqr(spawnCenter, new(tile.X, tile.Y)))})");
                return tile;
            }

            public override string ToString()
            {
                return "Cave " + Center;
            }
        }
    }
}
