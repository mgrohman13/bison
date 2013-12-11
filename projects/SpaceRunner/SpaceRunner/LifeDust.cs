using System;
using System.Collections.Generic;
using System.Drawing;
using SpaceRunner.Images;
using MattUtil;

namespace SpaceRunner
{
    internal class LifeDust : GameObject
    {
        //smallest size that will actually show up in an image
        private static readonly float StartSizeImage = Game.AddBit(.5f / Game.SqrtTwo);
        private static float SizeIncImage;
        private static Image[] Images;

        internal static void InitImages()
        {
            int numImages = Game.Random.GaussianOEInt(169, .065f, .091f, 130);
            SizeIncImage = ( Game.LifeDustSize * 2f - StartSizeImage ) / ( numImages - 1 );

            Images = new Image[numImages];

            float size = StartSizeImage;
            for (int idx = 0 ; idx < numImages ; ++idx)
            {
                Images[idx] = Game.LoadImageRotated(LifeDustGenerator.GenerateLifeDust(), size);
                size += SizeIncImage;
            }
        }

        internal static void Dispose()
        {
            foreach (Image image in Images)
                image.Dispose();
        }

        internal static void NewLifeDust(Game game)
        {
            PointF point = game.RandomEdgePoint();
            NewLifeDust(game, point.X, point.Y, Game.LifeDustClumpAmt);
        }

        internal static void NewLifeDust(Game game, float x, float y, float amt)
        {
            int num = game.GameRand.GaussianOEInt(amt, Game.LifeDustAmtRandomness, Game.LifeDustClumpOEPct, 1);
            float xDir = game.GameRand.Gaussian(Game.LifeDustClumpSpeed);
            float yDir = game.GameRand.Gaussian(Game.LifeDustClumpSpeed);
            for ( ; num > 0 ; --num)
                new LifeDust(game, x + game.GameRand.Gaussian(Game.LifeDustSpacing), y + game.GameRand.Gaussian(Game.LifeDustSpacing),
                        xDir + game.GameRand.Gaussian(Game.LifeDustIndividualSpeed), yDir + game.GameRand.Gaussian(Game.LifeDustIndividualSpeed),
                        game.GameRand.GaussianCapped(Game.LifeDustSize, Game.LifeDustSizeRandomness));
        }

        private LifeDust(Game game, float x, float y, float xDir, float yDir, float size)
            : base(game, x, y, xDir, yDir, size,
                //use the smallest image even if the object size is smaller than the image so that it is still visible
                Images[Game.Random.Round(Math.Max(0f, ( size - StartSizeImage ) / SizeIncImage))])
        {
        }

        internal override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        internal static List<Tuple<float, float, float>> bonds = new List<Tuple<float, float, float>>();
        protected override void OnStep()
        {
            //find a nearby bond if one exists
            Tuple<float, float, float> bond = null;
            int idx = -1;
            float distance = float.NaN;
            if (bonds.Count > 0)
                foreach (int findIdx in Game.GameRand.Iterate(bonds.Count))
                {
                    var find = bonds[findIdx];
                    distance = Game.GetDistance(this.x, this.y, GetBondX(find), GetBondY(find));
                    if (Game.GameRand.Gaussian(Game.LifeDustBondDistance, Game.LifeDustBondRandomness) > distance)
                    {
                        bond = find;
                        idx = findIdx;
                        break;
                    }
                }

            if (idx < 0)
            {
                bond = Tuple.Create(0f, 0f, 0f);
            }
            else if (distance > this.size + Game.GetSize(bond.Item3))
            {
                //move towards the bond
                float xDir = GetBondX(bond) - this.x;
                float yDir = GetBondY(bond) - this.y;
                Game.NormalizeDirs(ref xDir, ref yDir, (float)( Game.LifeDustBondAcceleration / GetSizePct(this)
                        * Math.Sqrt(bond.Item3 / ( bond.Item3 + Game.LifeDustClumpAmt / 2f * Game.GetArea(Game.LifeDustSize) )) ));
                this.xDir += xDir;
                this.yDir += yDir;
            }

            //add to the bond
            bond = Tuple.Create(bond.Item1 + this.x * this.Area,
                    bond.Item2 + this.y * this.Area, bond.Item3 + this.Area);
            if (idx < 0)
                bonds.Add(bond);
            else
                bonds[idx] = bond;
        }
        private static float GetBondX(Tuple<float, float, float> bond)
        {
            return bond.Item1 / bond.Item3;
        }
        private static float GetBondY(Tuple<float, float, float> bond)
        {
            return bond.Item2 / bond.Item3;
        }

        private void AdjustMove(GameObject obj)
        {
            float objXDir = obj.XDir, objYDir = obj.YDir, xDist = this.x - obj.X, yDist = this.y - obj.Y;
            Game.NormalizeDirs(ref xDist, ref yDist, 1);
            if (obj is Explosion || obj is FuelExplosion)
            {
                //explosion shockwave acts as outward speed
                objXDir += GetExplosionSpeed(xDist);
                objYDir += GetExplosionSpeed(yDist);
            }

            this.xDir = AvgWith(this.xDir, objXDir);
            this.yDir = AvgWith(this.yDir, objYDir);

            //retain whatever the final total speed would have been, but half of it ends up being in the direction away from the other object
            float totalSpeed = Game.GetDistance(this.xDir, this.yDir);
            this.xDir = AvgWith(this.xDir, totalSpeed, xDist);
            this.yDir = AvgWith(this.yDir, totalSpeed, yDist);
            Game.NormalizeDirs(ref this.xDir, ref this.yDir, totalSpeed);
        }

        private static float GetExplosionSpeed(float dist)
        {
            //inverse the LifeDustObjSpeedPower so explosions convey the total ExplosionSpeed
            return Game.VectorExponent(dist * Game.ExplosionSpeed, 1 / Game.LifeDustObjSpeedPower);
        }

        private static float AvgWith(float dir, float objDir)
        {
            //average the current speed with the maximum speed the other object can convey
            return ( dir + Game.VectorExponent(objDir, Game.LifeDustObjSpeedPower) ) / 2;
        }

        private static float AvgWith(float dir, float totalSpeed, float dist)
        {
            return dir / totalSpeed + dist;
        }

        internal bool HitBy(GameObject obj)
        {
            if (Game.GameRand.Bool((float)( Math.Pow(Game.LifeDustHitChance, 1.0 / GetSizePct(this)) )))
            {
                return true;
            }
            else
            {
                AdjustMove(obj);
                BumpCollision(obj, false);
                return false;
            }
        }

        protected override void Collide(GameObject obj)
        {
            LifeDust lifeDust = ( obj as LifeDust );
            bool isLifeDust = ( lifeDust != null );
            if (isLifeDust)
            {
                float areas = ( this.Area + obj.Area );
                lifeDust.xDir = xDir = ( xDir * this.Area + lifeDust.xDir * obj.Area ) / areas;
                lifeDust.yDir = yDir = ( yDir * this.Area + lifeDust.yDir * obj.Area ) / areas;
            }
            else
            {
                AdjustMove(obj);
            }
            BumpCollision(obj, isLifeDust);
        }

        protected override void HitPlayer()
        {
            base.HitPlayer();

            Game.AddLife(GetHeal(Game.PlayerLife, this), false);
        }

        internal static float GetHeal(float amt, GameObject lifeDust)
        {
            return ( amt / Game.LifeDustAmtToHeal * GetSizePct(lifeDust) );
        }

        internal static float GetSizePct(GameObject lifeDust)
        {
            return ( lifeDust.Area / Game.GetArea(Game.LifeDustSize) );
        }
    }
}
