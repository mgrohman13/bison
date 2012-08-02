using System;
using System.Drawing;

namespace SpaceRunner
{
    class Asteroid : GameObject, IDisposable
    {
        public const float AsteroidMaxSize = Game.AsteroidAverageSize * ( 2f - Game.AsteroidSizeCap );

        //load all possible different images, though each asteroid instance will have its own sized copy
        static Image[] images = LoadImages();
        static Image[] LoadImages()
        {
            Image[] retVal = new Image[Game.NumAsteroidImages];
            for (int i = 0 ; i < Game.NumAsteroidImages ; i++)
            {
                retVal[i] = LoadImage(i);
            }
            return retVal;
        }
        static Image LoadImage(int number)
        {
            return Game.LoadImage("asteroids\\" + number.ToString() + ".bmp", Game.AsteroidMaxImageSizeHalf);
        }

        static Image GetImage(float size)
        {
            int imageSize;
            if (( imageSize = Game.Random.Round(size * 2f) ) < 1)
                imageSize = 1;
            return ( new Bitmap(images[Game.Random.Next(Game.NumAsteroidImages)], new Size(imageSize, imageSize)) );
        }

        //dispose all static images, not to be confused with IDisposable implementation
        public static void Dispose()
        {
            foreach (Image i in images)
                i.Dispose();
        }

        //cached area
        float area;
        public float Area
        {
            get
            {
                return area;
            }
        }

        public override decimal Score
        {
            get
            {
                return (decimal)area * Game.AsteroidAreaScoreMult;
            }
        }

        //pass in xDir and yDir total distance so it only needs to be calculated once
        Asteroid(float x, float y, float size, float xDir, float yDir, float speed)
            : base(x, y, xDir, yDir, size, GetImage(size), speed / Game.AsteroidPieceSpeed * .6f)
        {
            //cache area (size is the radius)
            area = (float)( Math.PI * size * size );
        }
        public static void NewAsteroid()
        {
            PointF point = Game.RandomEdgePoint();
            NewAsteroid(point.X, point.Y);
        }
        public static void NewAsteroid(float x, float y)
        {
            //used when new asteroids appear on the edge
            double angle = Game.Random.DoubleFull(Math.PI);
            float speed = Game.Random.Gaussian(Game.AsteroidInitialSpeed);
            float xDir = (float)( Math.Cos(angle) * speed );
            float yDir = (float)( Math.Sin(angle) * speed );
            new Asteroid(x, y, Game.Random.GaussianCapped(Game.AsteroidAverageSize, Game.AsteroidSizeRandomness, Game.AsteroidAverageSize * Game.AsteroidSizeCap),
                xDir, yDir, speed);
        }
        public static void NewAsteroid(float x, float y, float size, float xDir, float yDir)
        {
            //used when an old asteroid breaks up
            new Asteroid(x, y, size, xDir, yDir, Game.GetDistance(xDir, yDir));
        }

        public override void Die()
        {
            //random number of pieces, but always have at least one
            int numPieces = Game.Random.GaussianCappedInt(Game.AsteroidPieces, Game.AsteroidPiecesRandomness, 1);
            //maintain total area with the formula: numPieces*pieceArea=area | pieceArea=Math.PI*pieceSize*pieceSize
            //i.e. pieceSize=Math.Sqrt(area/(Math.PI*numPieces))
            float pieceSize = (float)Math.Sqrt(area / ( Math.PI * numPieces ));

            float angle = Game.Random.DoubleFull((float)Math.PI);
            float step = Game.TwoPi / numPieces;
            //speed of new pieces
            float newSpeed = Game.Random.GaussianCapped(Game.AsteroidPieceSpeed, Game.AsteroidPieceSpeedRandomness);

            //space pieces out evenly in all directions
            float spacing = (float)( ( pieceSize + Forms.GameForm.Game.TotalSpeed / 2.0 ) * ( numPieces < 3 ? 1.0 : 1.0 / Math.Sin(Math.PI / numPieces) ) );
            for (int i = 0 ; i < numPieces ; i++)
            {
                angle += step;
                if (Game.Random.Bool(Math.Pow(size / AsteroidMaxSize, Game.AsteroidPieceChancePower)))
                {
                    float tempX, tempY;
                    float newXDir = tempX = (float)Math.Cos(angle);
                    float newYDir = tempY = (float)Math.Sin(angle);
                    tempX *= spacing;
                    tempY *= spacing;
                    newXDir *= newSpeed;
                    newYDir *= newSpeed;
                    NewAsteroid(x + tempX, y + tempY, pieceSize, xDir + newXDir, yDir + newYDir);
                }
            }

            base.Die();
        }

        protected override void Collide(GameObject obj)
        {
            Asteroid asteroid;
            if (obj is PowerUp)
            {
                //always kill the asteroid
                //kill the power up if the asteroid is bigger
                if (size > Game.PowerUpSize)
                {
                    //kill the power up
                    obj.Die();
                    //if the power up dies, the asteroid explodes
                    this.Die();
                }
                else
                {
                    //otherwise remove the asteroid uneventfully
                    Forms.GameForm.Game.RemoveObject(this);
                }
            }
            else if (( asteroid = obj as Asteroid ) != null)
            {
                //critical size check for each asteroid
                bool thisPassed, otherPassed;

                //check if the two asteroids should explode or if either should be destroyed uneventfully
                //asteroids close to the same size as each other will be more likely to explode
                //the smaller the asteroids the less likely to explode
                if (( ( thisPassed = area > Game.Random.OE(Game.AsteroidCollisionCriticalArea) ) &
                    ( otherPassed = asteroid.area > Game.Random.OE(Game.AsteroidCollisionCriticalArea) ) )
                    && Game.Random.OE(Math.Abs(area - asteroid.area)) < Game.AsteroidCollisionChance)
                {
                    //both asteroids explode
                    this.Die();
                    obj.Die();
                }
                else
                {
                    //if both asteroids failed the critical size check, destroy them both 
                    //else destroy the smaller one
                    if (!thisPassed && !otherPassed)
                    {
                        Forms.GameForm.Game.RemoveObject(this);
                        Forms.GameForm.Game.RemoveObject(obj);
                    }
                    else if (size < asteroid.size)
                    {
                        Forms.GameForm.Game.RemoveObject(this);
                    }
                    else
                    {
                        Forms.GameForm.Game.RemoveObject(obj);
                    }
                }
            }
#if DEBUG
            else
            {
                throw new Exception();
            }
#endif
        }

        protected override float HitPlayer()
        {
            //call HitPlayer to do cleanup stuff and add to score, but return damage based on size
            base.HitPlayer();
            return area / Game.AsteroidAreaToDamageRatio;
        }

        void IDisposable.Dispose()
        {
            //dispose instance image
            image.Dispose();
        }
    }
}
