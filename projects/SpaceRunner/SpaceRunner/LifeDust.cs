using System;
using System.Drawing;

namespace SpaceRunner
{
    class LifeDust : GameObject
    {
        static readonly Image[] LifeDustImage = LoadImages();

        public static void Dispose()
        {
            for (int i = 0 ; i < Game.NumLifeDustImages ; ++i)
                LifeDustImage[i].Dispose();
        }

        static Image[] LoadImages()
        {
            Image[] retVal = new Image[Game.NumLifeDustImages];
            for (int i = 1 ; i <= Game.NumLifeDustImages ; ++i)
                retVal[i - 1] = Game.LoadImage(@"lifedust\" + i.ToString() + ".bmp", Game.LifeDustSize);
            return retVal;
        }

        public override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        LifeDust(float x, float y, float xDir, float yDir, int imageIndex)
            : base(x, y, xDir, yDir, Game.LifeDustSize, LifeDustImage[imageIndex])
        {
        }

        public static void NewLifeDust()
        {
            PointF point = Game.RandomEdgePoint();
            NewLifeDust(point.X, point.Y, Game.LifeDustClumpAmt);
        }
        public static void NewLifeDust(float x, float y, float amt)
        {
            int i = Game.Random.GaussianOEInt(amt, Game.LifeDustAmtRandomness, Game.LifeDustClumpOEPct, 1);
            float xDir = Game.Random.Gaussian(Game.LifeDustClumpSpeed);
            float yDir = Game.Random.Gaussian(Game.LifeDustClumpSpeed);
            for ( ; i > 0 ; --i)
            {
                new LifeDust(x + Game.Random.Gaussian(Game.LifeDustSpacing), y + Game.Random.Gaussian(Game.LifeDustSpacing),
                        xDir + Game.Random.Gaussian(Game.LifeDustIndividualSpeed), yDir + Game.Random.Gaussian(Game.LifeDustIndividualSpeed),
                        Game.Random.Next(Game.NumLifeDustImages));
            }
        }

        void AdjustMove(GameObject obj)
        {
            xDir += Game.Random.OE(( x - obj.X ) * Game.LifeDustAdjustSpeed);
            yDir += Game.Random.OE(( y - obj.Y ) * Game.LifeDustAdjustSpeed);
        }

        public bool HitBy(GameObject obj)
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

        //public override void Step()
        //{
        //    xDir += RandVal(Game.LifeDustDriftSpeed);
        //    yDir += RandVal(Game.LifeDustDriftSpeed);
        //    base.Step();
        //}

        protected override void Collide(GameObject obj)
        {
            LifeDust lifeDust = ( obj as LifeDust );
            bool adjustOther = ( lifeDust != null );
            if (adjustOther)
            {
                lifeDust.xDir = xDir = ( xDir + lifeDust.xDir ) / 2f;
                lifeDust.yDir = yDir = ( yDir + lifeDust.yDir ) / 2f;
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
