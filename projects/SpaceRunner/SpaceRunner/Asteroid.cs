using System;
using System.Drawing;

namespace SpaceRunner
{
    class Asteroid : GameObject, IDisposable
    {
        //load all possible different images, though each asteroid instance will have its own sized copy
        private static readonly Image[] images;

        static Asteroid()
        {
            images = new Image[Game.NumAsteroidImages];
            for (int i = 0 ; i < Game.NumAsteroidImages ; i++)
                images[i] = Game.LoadImage("asteroids\\" + i.ToString() + ".bmp", Game.AsteroidMaxImageSizeHalf);
        }

        //dispose all static images, not to be confused with IDisposable implementation
        public static void Dispose()
        {
            foreach (Image i in images)
                i.Dispose();
        }

        public static void NewAsteroid()
        {
            PointF point = Game.RandomEdgePoint();
            NewAsteroid(point.X, point.Y);
        }

        public static void NewAsteroid(float x, float y)
        {
            float xDir, yDir, speed = Game.Random.Gaussian(Game.AsteroidInitialSpeed);
            Game.GetRandomDirection(out xDir, out yDir, speed);
            float size = Game.Random.GaussianCapped(Game.AsteroidAverageSize, Game.AsteroidSizeRandomness, Game.AsteroidAverageSize * Game.AsteroidSizeCap);
            new Asteroid(x, y, size, xDir, yDir, speed);
        }

        public static void NewAsteroid(float x, float y, float size, float xDir, float yDir)
        {
            //used when an old asteroid breaks up
            new Asteroid(x, y, size, xDir, yDir, Game.GetDistance(xDir, yDir));
        }

        //pass in xDir and yDir total distance so it only needs to be calculated once
        Asteroid(float x, float y, float size, float xDir, float yDir, float speed) :
            base(x, y, xDir, yDir, size, GetImage(size), speed * Game.AsteroidRotateMult + Game.AsteroidRotateConst)
        {
        }

        private static Image GetImage(float size)
        {
            int imageSize = Game.Random.Round(size * 2);
            if (imageSize < 1)
                imageSize = 1;
            return new Bitmap(images[Game.Random.Next(Game.NumAsteroidImages)], new Size(imageSize, imageSize));
        }

        public override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        public float Area
        {
            get
            {
                return (float)( Math.PI * size * size );
            }
        }

        public override void Die()
        {
            base.Die();

            //random number of pieces, but always have at least one
            int numPieces = Game.Random.GaussianCappedInt(Game.AsteroidPieces, Game.AsteroidPiecesRandomness, 1);
            //maintain total area with the formula: Area=numPieces*pieceArea | pieceArea=Math.PI*pieceSize*pieceSize
            float pieceSize = (float)Math.Sqrt(Area / ( Math.PI * numPieces ));
            float pieceSpeed = Game.Random.GaussianCapped(Game.AsteroidPieceSpeed, Game.AsteroidPieceSpeedRandomness);
            //space pieces out evenly in all directions
            float angle = Game.Random.DoubleFull((float)Math.PI);
            float angleStep = Game.TwoPi / numPieces;
            float spacing = (float)( ( 2 * pieceSize + Forms.GameForm.Game.TotalSpeed ) * ( numPieces < 3 ? 1.0 : 1.0 / Math.Sin(Math.PI / numPieces) ) );
            for (int i = 0 ; i < numPieces ; i++)
            {
                if (Game.Random.Bool(Math.Pow(size / Game.AsteroidMaxSize, Game.AsteroidPieceChancePower)))
                {
                    float addX, addY;
                    float addXDir = addX = (float)Math.Cos(angle);
                    float addYDir = addY = (float)Math.Sin(angle);
                    addX *= spacing;
                    addY *= spacing;
                    addXDir *= pieceSpeed;
                    addYDir *= pieceSpeed;
                    NewAsteroid(x + addX, y + addY, pieceSize, xDir + addXDir, yDir + addYDir);
                }

                angle += angleStep;
            }
        }

        protected override void Collide(GameObject obj)
        {
            Asteroid asteroid;
            if (obj is PowerUp)
                HitPowerUp(obj);
            else if (( asteroid = obj as Asteroid ) != null)
                HitAsteroid(asteroid);
#if DEBUG
            else
                throw new Exception();
#endif
        }

        private void HitPowerUp(GameObject obj)
        {
            if (size > Game.PowerUpSize)
            {
                obj.Die();
                this.Die();
            }
            else
            {
                Forms.GameForm.Game.RemoveObject(this);
            }
        }

        private void HitAsteroid(Asteroid asteroid)
        {
            //critical size check for each asteroid
            bool thisPassed = ( Area > Game.Random.OE(Game.AsteroidCollisionCriticalArea) );
            bool otherPassed = ( asteroid.Area > Game.Random.OE(Game.AsteroidCollisionCriticalArea) );

            //asteroids close to the same size as each other will be more likely to explode
            if (thisPassed && otherPassed && Game.AsteroidCollisionChance > Game.Random.OE(Math.Abs(Area - asteroid.Area)))
            {
                //both asteroids explode
                this.Die();
                asteroid.Die();
            }
            else
            {
                if (!thisPassed && !otherPassed)
                {
                    //if both asteroids failed the critical size check, destroy them both uneventfully
                    Forms.GameForm.Game.RemoveObject(this);
                    Forms.GameForm.Game.RemoveObject(asteroid);
                }
                //destroy the smaller one
                else if (size < asteroid.size)
                    Forms.GameForm.Game.RemoveObject(this);
                else
                    Forms.GameForm.Game.RemoveObject(asteroid);
            }
        }

        protected override float HitPlayer()
        {
            base.HitPlayer();

            return Area / Game.AsteroidAreaToDamageRatio;
        }

        void IDisposable.Dispose()
        {
            //dispose instance image
            image.Dispose();
        }
    }
}
