using System;
using System.Drawing;

namespace SpaceRunner
{
    class Bullet : GameObject
    {
        public enum FriendlyStatus
        {
            //gives double score when killing aliens
            Friend,
            //doesnt hit alien ships and gives no score when killing aliens
            Enemy,
            //hits alien ships for reduced damage and gives standard score for killing aliens
            Neutral,
        }

        static readonly Image BulletImage = Game.LoadImage("bullet.bmp", Game.BulletSize);

        const int NumFireworks = 13;
        static Image[] Fireworks = null;

        static Image[] LoadFireworks()
        {
            Image[] retVal = new Image[NumFireworks];
            for (int i = 0 ; i < NumFireworks ; i++)
            {
                retVal[i] = LoadFirework(i);
            }
            return retVal;
        }
        static Image LoadFirework(int number)
        {
            return Game.LoadImage("fireworks\\" + number.ToString() + ".bmp", Game.BulletSize);
        }

        public static void Dispose()
        {
            BulletImage.Dispose();

            if (Fireworks != null)
                foreach (Image i in Fireworks)
                    i.Dispose();
        }

        FriendlyStatus friendly;
        public FriendlyStatus Friendly
        {
            get
            {
                return friendly;
            }
        }

        public override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        Bullet(float x, float y, float xDir, float yDir, float speed, float spacing, FriendlyStatus friendly, int FireworkIndex)
            : base(x, y, xDir, yDir, Game.BulletSize, Fireworks[FireworkIndex])
        {
            Init(spacing, speed, friendly);
        }
        Bullet(float x, float y, float xDir, float yDir, float speed, float spacing, FriendlyStatus friendly)
            : base(x, y, xDir, yDir, Game.BulletSize, BulletImage)
        {
            Init(spacing, speed, friendly);
        }
        void Init(float spacing, float speed, FriendlyStatus friendly)
        {
            this.friendly = friendly;
            //space out from whoever fired it
            Game.NormalizeDirs(ref this.xDir, ref this.yDir, spacing);
            x += xDir;
            y += yDir;
            //add bullet speed
            Game.NormalizeDirs(ref this.xDir, ref this.yDir, speed + Game.BulletSpeed);
        }
        public static void NewBullet(float x, float y, float xDir, float yDir, float speed, float spacing, FriendlyStatus friendly)
        {
            new Bullet(x, y, xDir, yDir, speed, spacing, friendly);
        }
        public static void BulletExplosion(float x, float y, float numBullets)
        {
            //randomize number of bullets
            int pieces = Game.Random.OEInt(numBullets);
            if (pieces > 0)
            {
                int fireworkIndex = Game.Random.Next(NumFireworks);

                //space bullets out evenly in all directions
                float step = Game.TwoPi / pieces;
                float angle = Game.Random.DoubleFull((float)Math.PI);
                float speed = Game.Random.GaussianCapped(Game.BulletExplosionSpeed, Game.BulletExplosionSpeedRandomness, Game.BulletExplosionSpeed * Game.BulletExplosionSpeedLowerCap);

                float spacing = (float)( Game.BulletSize * ( pieces < 3 ? 1.0 : 1.0 / Math.Sin(Math.PI / pieces) ) );
                for (int i = 0 ; i < pieces ; ++i)
                {
                    float xMove, yMove;
                    if (Forms.GameForm.Game.Fireworks)
                    {
                        if (Fireworks == null)
                            Fireworks = LoadFireworks();
                        new Bullet(x + ( xMove = (float)Math.Cos(angle += step) ) * Game.BulletSize,
                            y + ( yMove = (float)Math.Sin(angle) ) * Game.BulletSize,
                            xMove, yMove, speed, spacing, FriendlyStatus.Neutral, fireworkIndex);
                    }
                    else
                    {
                        new Bullet(x + ( xMove = (float)Math.Cos(angle += step) ) * Game.BulletSize,
                            y + ( yMove = (float)Math.Sin(angle) ) * Game.BulletSize,
                            xMove, yMove, speed, spacing, FriendlyStatus.Neutral);
                    }
                }
            }
        }

        protected override void Collide(GameObject obj)
        {
            bool hit = true;
            LifeDust lifeDust;
            if (obj is Alien)
            {
                if (friendly == FriendlyStatus.Friend)
                    obj.AddScore(obj.Score);
                else if (friendly == FriendlyStatus.Enemy)
                    obj.AddScore(-obj.Score);
            }
            else if (obj is Bullet)
                BulletExplosion(( x + obj.X ) / 2f, ( y + obj.Y ) / 2f, 2);
            else if (( lifeDust = obj as LifeDust ) != null)
                hit = lifeDust.HitBy(this);

            if (hit)
            {
                this.Die();
                obj.Die();
            }
        }

        protected override float HitPlayer()
        {
            if (Forms.GameForm.Game.Fireworks && friendly == FriendlyStatus.Neutral)
                return 0;

            //call HitPlayer to do cleanup stuff and add to score, but return bullet damage
            base.HitPlayer();
            return Game.BulletDamage;
        }
    }
}
