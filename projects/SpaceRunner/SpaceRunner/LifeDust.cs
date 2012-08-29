using System;
using System.Drawing;

namespace SpaceRunner
{
    internal class LifeDust : GameObject
    {
        private static readonly Image[] Images;

        static LifeDust()
        {
            Images = new Image[Game.NumLifeDustImages];
            for (int i = 1 ; i <= Game.NumLifeDustImages ; ++i)
                Images[i - 1] = Game.LoadImage(@"lifedust\" + i.ToString() + ".bmp", Game.LifeDustSize);
        }

        internal static void Dispose()
        {
            foreach (Image i in Images)
                i.Dispose();
        }

        internal static void NewLifeDust(Game game)
        {
            PointF point = Game.RandomEdgePoint();
            NewLifeDust(game, point.X, point.Y, Game.LifeDustClumpAmt);
        }

        internal static void NewLifeDust(Game game, float x, float y, float amt)
        {
            int i = Game.Random.GaussianOEInt(amt, Game.LifeDustAmtRandomness, Game.LifeDustClumpOEPct, 1);
            float xDir = Game.Random.Gaussian(Game.LifeDustClumpSpeed);
            float yDir = Game.Random.Gaussian(Game.LifeDustClumpSpeed);
            for ( ; i > 0 ; --i)
                new LifeDust(game, x + Game.Random.Gaussian(Game.LifeDustSpacing), y + Game.Random.Gaussian(Game.LifeDustSpacing),
                        xDir + Game.Random.Gaussian(Game.LifeDustIndividualSpeed), yDir + Game.Random.Gaussian(Game.LifeDustIndividualSpeed),
                        Game.Random.Next(Game.NumLifeDustImages));
        }

        private LifeDust(Game game, float x, float y, float xDir, float yDir, int imageIndex)
            : base(game, x, y, xDir, yDir, Game.LifeDustSize, Images[imageIndex])
        {
        }

        internal override decimal Score
        {
            get
            {
                return 0m;
            }
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
            if (Game.Random.Bool(Game.LifeDustHitChance))
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
                lifeDust.xDir = xDir = ( xDir + lifeDust.xDir ) / 2;
                lifeDust.yDir = yDir = ( yDir + lifeDust.yDir ) / 2;
            }
            else
            {
                AdjustMove(obj);
            }
            BumpCollision(obj, isLifeDust);
        }

        protected override float HitPlayer()
        {
            Game.AddLife(Game.PlayerLife / Game.LifeDustAmtToHeal, false);

            return base.HitPlayer();
        }
    }
}
