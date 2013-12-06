using System;
using System.Drawing;
using SpaceRunner.Images;

namespace SpaceRunner
{
    internal class Bullet : GameObject
    {
        internal enum FriendlyStatus
        {
            //fired by player; gives double score when killing aliens
            Friend,
            //fired by aliens; doesn't hit alien ships and gives no score when killing aliens
            Enemy,
            //from bullet explosions; hits alien ships for reduced damage and gives standard score for killing aliens
            Neutral,
        }

        private static Image[] Images;

        internal static void InitImages()
        {
            int numImages = Game.Random.GaussianOEInt(13f, .13f, .21f, 6);
            Images = new Image[numImages];
            for (int idx = 0 ; idx < numImages ; ++idx)
                Images[idx] = Game.LoadImageRotated(BulletGenerator.GenerateBullet(), Game.BulletSize);
        }

        internal static void Dispose()
        {
            foreach (Image image in Images)
                image.Dispose();
        }

        internal readonly FriendlyStatus Friendly;

        internal static void BulletExplosion(Game game, float x, float y, float numBullets)
        {
            //randomize number of bullets
            int numPieces = game.GameRand.OEInt(numBullets);
            if (numPieces > 0)
            {
                float speed = game.GameRand.Gaussian(Game.BulletExplosionSpeed, Game.BulletExplosionSpeedRandomness);
                float angle = game.GetRandomAngle();
                float angleStep = Game.TwoPi / numPieces;

                //placing all bullets in the same spot will cause collisions and more explosions
                float spacing = -speed;
                //half the time, space bullets out evenly in all directions for a uniform explosion
                if (game.GameRand.Bool())
                    spacing += Game.GetRingSpacing(numPieces, Game.BulletSize);

                //don't add standard bullet speed
                speed -= Game.BulletSpeed;
                for (int idx = 0 ; idx < numPieces ; ++idx)
                {
                    float xDir, yDir;
                    Game.GetDirs(out xDir, out yDir, angle);
                    new Bullet(game, x, y, xDir, yDir, speed, spacing, FriendlyStatus.Neutral);
                    angle += angleStep;
                }
            }
        }

        internal static Bullet NewBullet(Game game, float x, float y, float xDir, float yDir, float speed, float spacing, FriendlyStatus friendly)
        {
            return new Bullet(game, x, y, xDir, yDir, speed, spacing, friendly);
        }

        private Bullet(Game game, float x, float y, float xDir, float yDir, float speed, float spacing, FriendlyStatus friendly)
            : base(game, x, y, xDir, yDir, Game.BulletSize, Game.Random.SelectValue(Images))
        {
            this.Friendly = friendly;
            //space out from whoever fired it
            Game.NormalizeDirs(ref xDir, ref yDir, spacing + Game.BulletSize);
            this.x += xDir;
            this.y += yDir;
            //set bullet speed
            Game.NormalizeDirs(ref this.xDir, ref this.yDir, speed + Game.BulletSpeed);
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
            LifeDust lifeDust = obj as LifeDust;
            if (obj is Bullet)
                BulletExplosion(Game, ( x + obj.X ) / 2, ( y + obj.Y ) / 2, 2);
            else if (lifeDust != null)
                hit = lifeDust.HitBy(this);

            if (hit)
            {
                if (lifeDust == null || Game.GameRand.Bool((float)( Math.Pow(Game.BulletLifeDustDieChance, 1.0 / LifeDust.GetSizePct(lifeDust)) )))
                    this.Die();
                obj.Die();
            }
        }

        protected override float HitPlayer()
        {
            base.HitPlayer();

            GameObject player = Game.GetPlayerObject();
            Explosion.NewExplosion(Game, this, player, player);
            return Game.BulletDamage;
        }
    }
}
