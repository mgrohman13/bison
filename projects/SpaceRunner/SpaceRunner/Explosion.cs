using System.Drawing;

namespace SpaceRunner
{
    internal class Explosion : GameObject
    {
        private static Image[,] images;
        static Explosion()
        {
            images = new Image[Game.NumExplosions, Game.NumExplosionImages];
            for (int explosion = 1 ; explosion <= Game.NumExplosions ; ++explosion)
                for (int number = 1 ; number <= Game.NumExplosionImages ; ++number)
                    images[explosion - 1, number - 1] = Game.LoadImage("explosion\\" + explosion.ToString() + "\\" + number.ToString() + ".bmp", Color.White, Game.ExplosionSize);
        }

        internal static void Dispose()
        {
            foreach (Image i in images)
                i.Dispose();
        }

        private int time = 0;
        private int expNum;

        internal static Explosion NewExplosion(params GameObject[] objs)
        {
#if DEBUG
            if (objs.Length == 0)
                throw new System.Exception();
#endif
            float xDir = 0, yDir = 0, count = 0;
            foreach (GameObject obj in objs)
            {
                float x = obj.X, y = obj.Y;
                Game.NormalizeDirs(ref x, ref y, obj.Speed);
                xDir += obj.XDir - x;
                yDir += obj.YDir - y;
                ++count;
            }
            xDir /= count;
            yDir /= count;
            return new Explosion(objs[0].X, objs[0].Y, xDir, yDir, Game.Random.Next(Game.NumExplosions));
        }

        private Explosion(float x, float y, float xDir, float yDir, int expNum)
            : base(x, y, Game.ExplosionSize, images[expNum, 0], Game.ExplosionRotate)
        {
            this.xDir = xDir;
            this.yDir = yDir;
            this.expNum = expNum;
            this.time = Game.Random.Round(Game.ExplosionTime);
        }

        internal override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        protected override void OnStep()
        {
            if (time > 0)
            {
                Game.NormalizeDirs(ref this.xDir, ref this.yDir, Game.GetDistance(xDir, yDir) * Game.ExplosionSpeedMult);
                this.image = images[expNum, (int)( ( Game.ExplosionTime - time ) / Game.ExplosionTime * Game.NumExplosionImages )];
                --time;
            }
            else
            {
                this.Die();
            }
        }

        protected override void Collide(GameObject obj)
        {
            LifeDust ld = obj as LifeDust;
            if (ld != null && ld.HitBy(this))
                ld.Die();
        }

        protected override float HitPlayer()
        {
            return 0f;
        }
    }
}
