using System.Drawing;

namespace SpaceRunner
{
    internal class Explosion : GameObject
    {
        private static Image[,] Images;
        static Explosion()
        {
            Images = new Image[Game.NumExplosionImages, Game.NumImagesPerExplosion];
            for (int explosion = 1 ; explosion <= Game.NumExplosionImages ; ++explosion)
                for (int number = 1 ; number <= Game.NumImagesPerExplosion ; ++number)
                    Images[explosion - 1, number - 1] = Game.LoadImage("explosion\\" + explosion.ToString() + "\\" + number.ToString() + ".bmp", Color.White, Game.ExplosionSize);
        }

        internal static void Dispose()
        {
            foreach (Image image in Images)
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
            return new Explosion(game, objs[0].X, objs[0].Y, xDir, yDir, Game.Random.Next(Game.NumExplosionImages));
        }

        private Explosion(Game game, float x, float y, float xDir, float yDir, int expNum)
            : base(game, x, y, Game.ExplosionSize, Images[expNum, 0], Game.ExplosionRotate)
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
                this.image = Images[expNum, (int)( ( Game.ExplosionTime - time ) / Game.ExplosionTime * Game.NumImagesPerExplosion )];
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
