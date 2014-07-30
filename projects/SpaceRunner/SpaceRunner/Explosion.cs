using System;
using System.Drawing;
using SpaceRunner.Images;

namespace SpaceRunner
{
    internal class Explosion : GameObject
    {
        private static Image[][] Images;

        internal static void InitImages()
        {
            lock (typeof(Explosion))
            {
                StaticDispose();

                int numExplosions = Game.Random.GaussianOEInt(9.1f, .065f, .065f, 6);

                Images = new Image[numExplosions][];
                for (int explosion = 0 ; explosion < numExplosions ; ++explosion)
                {
                    int numImages = Game.Random.GaussianOEInt(16.9f, .039f, .091f, 13);
                    Images[explosion] = new Image[numImages * 2 - 1];
                    const float avgWidth = 52;
                    int width = Game.Random.GaussianOEInt(avgWidth, .091f, .078f, Game.Random.Round(2 * Game.ExplosionSize));
                    Bitmap[] b = ExplosionGenerator.GenerateExplosion(width, numImages, Game.Random.GaussianCapped(13f * width / avgWidth, .078f));
                    for (int number = 0 ; number < numImages * 2 - 1 ; ++number)
                        Images[explosion][number] = Game.LoadImage(b[number], Game.ExplosionSize);
                }
            }
        }

        internal static void StaticDispose()
        {
            if (Images != null)
                foreach (Image[] exp in Images)
                    foreach (Image image in exp)
                        lock (image)
                            image.Dispose();
        }

        private int time = 0;
        private int expNum;

        internal static Explosion NewExplosion(Game game, float damage, float cutoff, params GameObject[] objs)
        {
            if (game.GameRand.Gaussian(damage, Game.ExplosionAppearanceRandomness) > cutoff)
                return NewExplosion(game, objs);
            return null;
        }
        internal static Explosion NewExplosion(Game game, params GameObject[] objs)
        {
#if DEBUG
            if (objs.Length == 0)
                throw new System.Exception();
#endif
            float xDir = 0, yDir = 0, count = 0;
            foreach (GameObject obj in objs)
            {
                float x = obj.X, y = obj.Y;
                game.NormalizeDirs(ref x, ref y, obj.Speed);
                xDir += obj.XDir - x;
                yDir += obj.YDir - y;
                ++count;
            }
            xDir /= count;
            yDir /= count;
            return new Explosion(game, objs[0].X, objs[0].Y, xDir, yDir, Game.Random.Next(Images.Length));
        }

        private Explosion(Game game, float x, float y, float xDir, float yDir, int expNum)
            : base(game, x, y, Game.ExplosionSize, Images[expNum][0], Game.ExplosionRotate)
        {
            this.xDir = xDir;
            this.yDir = yDir;
            this.expNum = expNum;
            this.time = Game.GameRand.Round(Game.ExplosionTime);
        }

        internal override decimal Score
        {
            get
            {
                return 0;
            }
        }
        internal int Time
        {
            get
            {
                return time;
            }
        }

        protected override void OnStep()
        {
            Game.NormalizeDirs(ref this.xDir, ref this.yDir, Game.GetDistance(this.xDir, this.yDir) * Game.ExplosionSpeedMult);

            if (--time > 0)
                this.image = Images[expNum][(int)( ( Game.ExplosionTime - time ) / Game.ExplosionTime * Images[expNum].Length )];
            else
                this.Die();
        }

        protected override void Collide(GameObject obj)
        {
            LifeDust ld = obj as LifeDust;
            if (ld != null && ld.HitBy(this))
                ld.Die();

            if (obj is Explosion)
            {
            }
        }

        protected override void HitPlayer()
        {
        }
    }
}
