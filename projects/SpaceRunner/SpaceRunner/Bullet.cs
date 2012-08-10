using System;
using System.Drawing;

namespace SpaceRunner
{
    internal class Bullet : GameObject
    {
        internal enum FriendlyStatus
        {
            //fired by player; gives double score when killing aliens
            Friend,
            //fired by aliens; doesnt hit alien ships and gives no score when killing aliens
            Enemy,
            //from bullet explosions; hits alien ships for reduced damage and gives standard score for killing aliens
            Neutral,
        }

        private static readonly Image BulletImage;
        private static Image[] Fireworks;

        static Bullet()
        {
            BulletImage = Game.LoadImage("bullet.bmp", Game.BulletSize);
            Fireworks = null;
        }

        private static void LoadFireworks()
        {
            Fireworks = new Image[Game.NumFireworks];
            for (int i = 0 ; i < Game.NumFireworks ; i++)
                Fireworks[i] = Game.LoadImage("fireworks\\" + i.ToString() + ".bmp", Game.BulletSize);
        }

        internal static void Dispose()
        {
            BulletImage.Dispose();

            if (Fireworks != null)
                foreach (Image i in Fireworks)
                    i.Dispose();
        }

        internal readonly FriendlyStatus Friendly;

        internal static void BulletExplosion(float x, float y, float numBullets)
        {
            //randomize number of bullets
            int numPieces = Game.Random.OEInt(numBullets);
            if (numPieces > 0)
            {
                int fireworkIndex = -1;
                if (Forms.GameForm.Game.Fireworks)
                {
                    if (Fireworks == null)
                        LoadFireworks();
                    fireworkIndex = Game.Random.Next(Game.NumFireworks);
                }

                float speed = Game.Random.Gaussian(Game.BulletExplosionSpeed, Game.BulletExplosionSpeedRandomness);
                float angle = Game.Random.DoubleFull((float)Math.PI);
                float angleStep = Game.TwoPi / numPieces;

                //placing all bullets in the same spot will cause collisions and more explosions
                float spacing = 0;
                //half the time, space bullets out evenly in all directions for a uniform explosion
                if (Game.Random.Bool())
                    spacing = (float)( ( Game.BulletSize + Forms.GameForm.Game.TotalSpeed / 2 ) * ( numPieces < 3 ? 1.0 : 1.0 / Math.Sin(Math.PI / numPieces) ) );
                //we don't want standard spacing for explosions
                spacing -= GetSpacing(speed);

                for (int i = 0 ; i < numPieces ; ++i)
                {
                    new Bullet(x, y, (float)Math.Cos(angle), (float)Math.Sin(angle), speed, spacing, FriendlyStatus.Neutral, fireworkIndex);
                    angle += angleStep;
                }
            }
        }

        internal static Bullet NewBullet(float x, float y, float xDir, float yDir, float speed, float spacing, FriendlyStatus friendly)
        {
            return new Bullet(x, y, xDir, yDir, speed, spacing, friendly, -1);
        }

        private Bullet(float x, float y, float xDir, float yDir, float speed, float spacing, FriendlyStatus friendly, int FireworkIndex)
            : base(x, y, xDir, yDir, Game.BulletSize, FireworkIndex > -1 ? Fireworks[FireworkIndex] : BulletImage)
        {
            this.Friendly = friendly;
            //space out from whoever fired it
            Game.NormalizeDirs(ref xDir, ref yDir, spacing + GetSpacing(speed));
            this.x += xDir;
            this.y += yDir;
            //set bullet speed
            Game.NormalizeDirs(ref this.xDir, ref this.yDir, speed + Game.BulletSpeed);
        }

        private static float GetSpacing(float speed)
        {
            return Game.BulletSize + speed + Forms.GameForm.Game.TotalSpeed;
        }

        internal override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        protected override void Collide(GameObject obj)
        {
            bool hit = true;
            LifeDust lifeDust;
            if (obj is Bullet)
                BulletExplosion(( x + obj.X ) / 2, ( y + obj.Y ) / 2, 2);
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
            if (Forms.GameForm.Game.Fireworks && Friendly == FriendlyStatus.Neutral)
                return 0;

            base.HitPlayer();

            GameObject player = Forms.GameForm.Game.GetPlayerObject();
            Explosion.NewExplosion(this, player, player);
            return Game.BulletDamage;
        }
    }
}
