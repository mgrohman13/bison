using System;
using System.Drawing;
using Form = SpaceRunner.Forms.GameForm;

namespace SpaceRunner
{
    internal class Asteroid : GameObject, IDisposable
    {
        //load all possible different images, though each asteroid instance will have its own sized copy
        private static readonly Image[] Images;

        static Asteroid()
        {
            Images = new Image[Game.NumAsteroidImages];
            for (int i = 0 ; i < Game.NumAsteroidImages ; i++)
                Images[i] = Game.LoadImage("asteroids\\" + i.ToString() + ".bmp");
        }

        //dispose all static images, not to be confused with IDisposable implementation
        internal static void Dispose()
        {
            foreach (Image i in Images)
                i.Dispose();
        }

        internal static Asteroid NewAsteroid()
        {
            PointF point = Game.RandomEdgePoint();
            return NewAsteroid(point.X, point.Y);
        }

        internal static Asteroid NewAsteroid(float x, float y)
        {
            float xDir, yDir, speed = Game.Random.Gaussian(Game.AsteroidInitialSpeed);
            Game.GetRandomDirection(out xDir, out yDir, speed);
            float size = Game.Random.GaussianCapped(Game.AsteroidAverageSize, Game.AsteroidSizeRandomness, Game.AsteroidAverageSize * Game.AsteroidSizeCap);
            return new Asteroid(x, y, size, xDir, yDir, speed);
        }

        internal static Asteroid NewAsteroid(float x, float y, float size, float xDir, float yDir)
        {
            //used when an old asteroid breaks up
            return new Asteroid(x, y, size, xDir, yDir, Game.GetDistance(xDir, yDir));
        }

        //pass in xDir and yDir total distance so it only needs to be calculated once
        private Asteroid(float x, float y, float size, float xDir, float yDir, float speed) :
            base(x, y, xDir, yDir, size, GetImage(size), speed * Game.AsteroidRotateMult + Game.AsteroidRotateConst)
        {
        }

        private static Image GetImage(float size)
        {
            return Game.ResizeImage(Images[Game.Random.Next(Game.NumAsteroidImages)], size, false);
        }

        internal override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        internal float Area
        {
            get
            {
                return (float)( Math.PI * size * size );
            }
        }

        internal override void Die()
        {
            base.Die();

            //random number of pieces, but always have at least one
            int numPieces = Game.Random.GaussianCappedInt(Game.AsteroidPieces, Game.AsteroidPiecesRandomness, 1);
            //maintain total area with the formula: Area=numPieces*pieceArea | pieceArea=Math.PI*pieceSize*pieceSize
            float pieceSize = (float)Math.Sqrt(Area / ( Math.PI * numPieces ));
            float pieceSpeed = Game.Random.GaussianCapped(Game.AsteroidPieceSpeed, Game.AsteroidPieceSpeedRandomness);
            //space pieces out evenly in all directions
            float angle = Game.GetRandomAngle();
            float angleStep = Game.TwoPi / numPieces;
            float spacing = Game.GetRingSpacing(numPieces, pieceSize);
            for (int i = 0 ; i < numPieces ; i++)
            {
                if (Game.Random.Bool(Math.Pow(size / Game.AsteroidMaxSize, Game.AsteroidPieceChancePower)))
                {
                    float addX, addY;
                    Game.GetDirs(out addX, out addY, angle);
                    float addXDir = addX * pieceSpeed;
                    float addYDir = addY * pieceSpeed;
                    addX *= spacing;
                    addY *= spacing;
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
                Form.Game.RemoveObject(this);
            }
        }

        private void HitAsteroid(Asteroid asteroid)
        {
            //critical size check for each asteroid
            bool thisPassed = ( this.Area > Game.Random.OE(Game.AsteroidCollisionCriticalArea) );
            bool otherPassed = ( asteroid.Area > Game.Random.OE(Game.AsteroidCollisionCriticalArea) );

            //asteroids close to the same size as each other will be more likely to explode
            if (thisPassed && otherPassed && Game.AsteroidCollisionChance > Game.Random.OE(Math.Abs(this.Area - asteroid.Area)))
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
                    Form.Game.RemoveObject(this);
                    Form.Game.RemoveObject(asteroid);
                }
                else
                {
                    //destroy the smaller one
                    if (this.size < asteroid.size)
                        Form.Game.RemoveObject(this);
                    else
                        Form.Game.RemoveObject(asteroid);
                }
            }
        }

        protected override float HitPlayer()
        {
            base.HitPlayer();

            float damage = Area / Game.AsteroidAreaToDamageRatio;
            if (damage > Game.BulletDamage)
                Explosion.NewExplosion(this, Form.Game.GetPlayerObject());
            return damage;
        }

        void IDisposable.Dispose()
        {
            //dispose instance image
            image.Dispose();
        }
    }
}
