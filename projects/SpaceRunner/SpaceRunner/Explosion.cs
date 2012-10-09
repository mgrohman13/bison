using System;
using System.Drawing;
using SpaceRunner.Images;

namespace SpaceRunner
{
    internal class Explosion : GameObject
    {
        private static int ImageCount;
        private static Image[][] Images;
        internal static void InitImages()
        {
            Dispose();

            ImageCount = Game.Random.GaussianOEInt(6, .13, .13, 3);

            Images = new Image[ImageCount][];
            for (int explosion = 0 ; explosion < ImageCount ; ++explosion)
            {
                int numImages = Game.Random.GaussianOEInt(13, .13, .13, 9);
                Images[explosion] = new Image[numImages * 2 - 1];
                Bitmap[] b = ExplosionGenerator.GenerateExplosion(39, numImages, 16.9);
                for (int number = 0 ; number < numImages * 2 - 1 ; ++number)
                    Images[explosion][number] = Game.LoadImage(b[number], Game.ExplosionSize);
            }
        }

        internal static void Dispose()
        {
            if (Images != null)
                foreach (Image[] exp in Images)
                    foreach (Image image in exp)
                        image.Dispose();
        }

        private int time = 0;
        private int expNum;

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
            return new Explosion(game, objs[0].X, objs[0].Y, xDir, yDir, Game.Random.Next(ImageCount));
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
        }

        protected override float HitPlayer()
        {
            return 0;
        }
    }
}
