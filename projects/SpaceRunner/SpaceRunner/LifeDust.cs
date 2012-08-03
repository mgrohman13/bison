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
            float oeAmt = amt * Game.LifeDustClumpOEPct;
            amt -= oeAmt;
            int i = Game.Random.OEInt(oeAmt) + Game.Random.GaussianCappedInt(amt, Game.LifeDustAmtRandomness, Game.Random.Round(amt * Game.LifeDustAmtCap));
            float xDir = RandVal(Game.LifeDustClumpSpeed);
            float yDir = RandVal(Game.LifeDustClumpSpeed);
            for ( ; i > 0 ; --i)
            {
                //new LifeDust(x + RandVal(Game.LifeDustSpacing), y + RandVal(Game.LifeDustSpacing), xDir, yDir, Game.Random.Next(Game.NumLifeDustImages));
                new LifeDust(x + RandVal(Game.LifeDustSpacing), y + RandVal(Game.LifeDustSpacing),
                    xDir + RandVal(Game.LifeDustIndividualSpeed), yDir + RandVal(Game.LifeDustIndividualSpeed),
                    Game.Random.Next(Game.NumLifeDustImages));
            }
        }

        static float RandVal(float avg)
        {
            return Game.Random.Gaussian(avg);
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
            bool adjustOther;
            LifeDust lifeDust;
            if (( lifeDust = obj as LifeDust ) != null)
            {
                xDir = ( xDir + lifeDust.xDir ) / 2f;
                yDir = ( yDir + lifeDust.yDir ) / 2f;
                lifeDust.xDir = xDir;
                lifeDust.yDir = yDir;
                adjustOther = true;
            }
            else
            {
                AdjustMove(obj);
                adjustOther = false;
            }
            BumpCollision(obj, adjustOther);
        }

        protected override float HitPlayer()
        {
            Forms.GameForm.Game.AddLife(Game.PlayerLife / Game.LifeDustAmtToHeal, false);
            base.HitPlayer();
            return 0;
        }
    }
}
