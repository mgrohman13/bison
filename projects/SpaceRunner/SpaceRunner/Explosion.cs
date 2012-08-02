using System.Drawing;

namespace SpaceRunner
{
	class Explosion : GameObject
	{
		static Image[,] images = LoadImages();
		static Image[,] LoadImages()
		{
			Image[,] retVal = new Image[Game.NumExplosions, Game.NumExplosionImages];
			for (int a = 0; a < Game.NumExplosions; ++a)
			{
				for (int b = 0; b < Game.NumExplosionImages; )
				{
					retVal[a, b] = LoadImage(a + 1, ++b);
				}
			}
			return retVal;
		}
		static Image LoadImage(int explosion, int number)
		{
			return Game.LoadImage("explosion\\" + explosion.ToString() + "\\" + number.ToString() + ".bmp", Color.White, Game.AlienSize * 1.26f);
		}

		public static void Dispose()
		{
			foreach (Image i in images)
				i.Dispose();
		}

		float cachedSpeed;
		int alt = 0;
		int time = 0;
		int expNum;

		public override decimal Score
		{
			get { return 0; }
		}

		Explosion(float x, float y, float speed, int expNum)
			: base(x, y, Game.AlienSize * 1.3f, images[expNum, 0])
		{
			//keep direction constant, no longer chase player
			Game.NormalizeDirs(ref x, ref y, speed);
			this.xDir = -x;
			this.yDir = -y;
			this.cachedSpeed = speed;
			this.expNum = expNum;
		}
		public static void NewExplosion(float x, float y, float speed)
		{
			new Explosion(x, y, speed, Game.Random.Next(Game.NumExplosions));
		}

		protected override void OnStep()
		{
			//dont run every frame
			const int Frames = (int)(.5f + Game.Framerate * 2.6f / Game.GameTick);
			if (++alt % Frames < 1)
			{
				//reduce speed each iteration
				Game.NormalizeDirs(ref this.xDir, ref this.yDir, cachedSpeed /= 1.13f);

				if (++time < Game.NumExplosionImages)
					this.image = images[expNum, time];
				else
				{
					//die when out of images
					this.Die();
				}
			}
		}

		//dont hit anything
		protected override void Collide(GameObject obj)
		{
			LifeDust ld;
			if ((ld = obj as LifeDust) != null)
				ld.HitBy(this);
		}

		protected override float HitPlayer()
		{
			//no damage
			return 0f;
		}
	}
}
