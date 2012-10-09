using System;
using System.Drawing;
using SpaceRunner.Images;

namespace SpaceRunner
{
    internal class LifeDust : GameObject
    {
        private const float StartSizeImage = .5f;
        private static int ImageCount;
        private static float SizeIncImage;
        private static Image[] Images;

        internal static void InitImages()
        {
            ImageCount = Game.Random.GaussianOEInt(169, .13, .13, 130);
            SizeIncImage = ( Game.LifeDustSize * 2f - StartSizeImage ) / ( ImageCount - 1 );

            Images = new Image[ImageCount];

            float size = StartSizeImage;
            for (int idx = 0 ; idx < ImageCount ; ++idx)
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
                    Images[Game.Random.Round(Math.Max(0, ( size - StartSizeImage ) / SizeIncImage))])
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
            if (Game.GameRand.Bool(Game.LifeDustHitChance * GetSizePct(this)))
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
                float sizes = ( this.size + obj.Size );
                lifeDust.xDir = xDir = ( xDir * this.size + lifeDust.xDir * obj.Size ) / sizes;
                lifeDust.yDir = yDir = ( yDir * this.size + lifeDust.yDir * obj.Size ) / sizes;
            }
            else
            {
                AdjustMove(obj);
            }
            BumpCollision(obj, isLifeDust);
        }

        protected override float HitPlayer()
        {
            Game.AddLife(GetHeal(Game.PlayerLife, this), false);

            return base.HitPlayer();
        }

        internal static float GetHeal(float amt, GameObject lifeDust)
        {
            return ( amt / Game.LifeDustAmtToHeal * GetSizePct(lifeDust) );
        }

        internal static float GetSizePct(GameObject lifeDust)
        {
            return ( lifeDust.Size / Game.LifeDustSize );
        }
    }
}
