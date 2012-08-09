using System;
using System.Drawing;

namespace SpaceRunner
{
    internal class LifeDust : GameObject
    {
        private static readonly Image[] LifeDustImage;

        static LifeDust()
        {
            LifeDustImage = new Image[Game.NumLifeDustImages];
            for (int i = 1 ; i <= Game.NumLifeDustImages ; ++i)
                LifeDustImage[i - 1] = Game.LoadImage(@"lifedust\" + i.ToString() + ".bmp", Game.LifeDustSize);
        }

        internal static void Dispose()
        {
            for (int i = 0 ; i < Game.NumLifeDustImages ; ++i)
                LifeDustImage[i].Dispose();
        }

        internal static void NewLifeDust()
        {
            PointF point = Game.RandomEdgePoint();
            NewLifeDust(point.X, point.Y, Game.LifeDustClumpAmt);
        }

        internal static void NewLifeDust(float x, float y, float amt)
        {
            int i = Game.Random.GaussianOEInt(amt, Game.LifeDustAmtRandomness, Game.LifeDustClumpOEPct, 1);
            float xDir = Game.Random.Gaussian(Game.LifeDustClumpSpeed);
            float yDir = Game.Random.Gaussian(Game.LifeDustClumpSpeed);
            for ( ; i > 0 ; --i)
                new LifeDust(x + Game.Random.Gaussian(Game.LifeDustSpacing), y + Game.Random.Gaussian(Game.LifeDustSpacing),
                        xDir + Game.Random.Gaussian(Game.LifeDustIndividualSpeed), yDir + Game.Random.Gaussian(Game.LifeDustIndividualSpeed),
                        Game.Random.Next(Game.NumLifeDustImages));
        }

        private LifeDust(float x, float y, float xDir, float yDir, int imageIndex)
            : base(x, y, xDir, yDir, Game.LifeDustSize, LifeDustImage[imageIndex])
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
            float objXDir = obj.XDir, objYDir = obj.YDir;
            if (obj is Explosion || obj is FuelExplosion)
            {
                float expX = x - obj.X, expY = y - obj.Y;
                Game.NormalizeDirs(ref expX, ref expY, Game.ExplosionSpeed);
                objXDir += expX;
                objYDir += expY;
            }
            Mod(ref xDir, objXDir);
            Mod(ref yDir, objYDir);
        }

        private void Mod(ref float dir, float objDir)
        {
            dir = ( dir + Game.ReduceWithPower(objDir, Game.LifeDustObjSpeedPower) ) / 2;
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
            bool adjustOther = ( lifeDust != null );
            if (adjustOther)
            {
                lifeDust.xDir = xDir = ( xDir + lifeDust.xDir ) / 2;
                lifeDust.yDir = yDir = ( yDir + lifeDust.yDir ) / 2;
            }
            else
            {
                AdjustMove(obj);
            }
            BumpCollision(obj, adjustOther);
        }

        protected override float HitPlayer()
        {
            Forms.GameForm.Game.AddLife(Game.PlayerLife / Game.LifeDustAmtToHeal, false);

            return base.HitPlayer();
        }
    }
}
