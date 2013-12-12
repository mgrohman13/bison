using System;
using System.Drawing;

namespace SpaceRunner
{
    internal class Asteroid : GameObject, IDisposable
    {
        //load all possible different images, though each asteroid instance will have its own sized copy
        private static readonly Image[] Images;

        static Asteroid()
        {
            Images = new Image[Game.NumAsteroidImages];
            for (int idx = 0 ; idx < Game.NumAsteroidImages ; idx++)
                Images[idx] = Game.LoadImage("asteroids\\" + idx.ToString() + ".bmp");
        }

        //dispose all static images, not to be confused with IDisposable implementation
        internal static void Dispose()
        {
            foreach (Image image in Images)
                image.Dispose();
        }

        internal static Asteroid NewAsteroid(Game game)
        {
            PointF point = game.RandomEdgePoint();
            return NewAsteroid(game, point.X, point.Y);
        }

        internal static Asteroid NewAsteroid(Game game, float x, float y)
        {
            float xDir, yDir, speed = game.GameRand.Gaussian(Game.AsteroidInitialSpeed);
            game.GetRandomDirection(out xDir, out yDir, speed);
            float size = game.GameRand.GaussianCapped(Game.AsteroidAverageSize, Game.AsteroidSizeRandomness, Game.AsteroidAverageSize * Game.AsteroidSizeCap);
            return new Asteroid(game, x, y, size, xDir, yDir);
        }

        internal static Asteroid NewAsteroid(Game game, float x, float y, float size, float xDir, float yDir)
        {
            //used when an old asteroid breaks up
            return new Asteroid(game, x, y, size, xDir, yDir);
        }

        //pass in xDir and yDir total distance so it only needs to be calculated once
        private Asteroid(Game game, float x, float y, float size, float xDir, float yDir) :
            base(game, x, y, xDir, yDir, size, GetImage(size), Game.GetDistance(xDir, yDir) * Game.AsteroidRotateMult + Game.AsteroidRotateConst)
        {
        }

        private static Image GetImage(float size)
        {
            return Game.ResizeImage(Game.Random.SelectValue(Images), size, false);
        }

        internal override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        internal override void Die()
        {
            base.Die();

            //random number of pieces, but always have at least one
            int numPieces = Game.GameRand.GaussianCappedInt(Game.AsteroidPieces, Game.AsteroidPiecesRandomness, 1);
            //maintain total area with the formula: Area=numPieces*pieceArea | pieceArea=Math.PI*pieceSize*pieceSize
            float pieceSize = Game.GetSize(Area / numPieces);
            float pieceSpeed = Game.GameRand.GaussianCapped(Game.AsteroidPieceSpeed, Game.AsteroidPieceSpeedRandomness);
            //space pieces out evenly in all directions
            float angle = Game.GetRandomAngle();
            float angleStep = Game.TwoPi / numPieces;
            float spacing = Game.GetRingSpacing(numPieces, pieceSize);
            for (int idx = 0 ; idx < numPieces ; idx++)
            {
                if (Game.GameRand.Bool((float)( Math.Pow(Size / Game.AsteroidMaxSize, Game.AsteroidPieceChancePower) )))
                {
                    float addX, addY;
                    Game.GetDirs(out addX, out addY, angle);
                    float addXDir = addX * pieceSpeed;
                    float addYDir = addY * pieceSpeed;
                    addX *= spacing;
                    addY *= spacing;
                    NewAsteroid(Game, x + addX, y + addY, pieceSize, xDir + addXDir, yDir + addYDir);
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
            if (Game.GameRand.Gaussian(Size, Game.AsteroidHitPowerUpRandomness) > Game.PowerUpSize)
            {
                obj.Die();
                this.Die();
            }
            else
            {
                Game.RemoveObject(this);
            }
        }

        private void HitAsteroid(Asteroid asteroid)
        {
            //critical size check for each asteroid
            bool thisPassed = ( this.Area > Game.GameRand.OE(Game.AsteroidCollisionCriticalArea) );
            bool otherPassed = ( asteroid.Area > Game.GameRand.OE(Game.AsteroidCollisionCriticalArea) );

            //asteroids close to the same size as each other will be more likely to explode
            if (thisPassed && otherPassed && Game.AsteroidCollisionChance > Game.GameRand.OE(Math.Abs(this.Area - asteroid.Area)))
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
                    Game.RemoveObject(this);
                    Game.RemoveObject(asteroid);
                }
                else
                {
                    //destroy the smaller one
                    if (this.Size < asteroid.Size)
                        Game.RemoveObject(this);
                    else
                        Game.RemoveObject(asteroid);
                }
            }
        }

        protected override void HitPlayer()
        {
            base.HitPlayer();

            float damage = Area / Game.AsteroidAreaToDamageRatio;
            Explosion.NewExplosion(Game, damage, Game.BulletDamage, this, Game.GetPlayerObject());
            Game.HitPlayer(damage);
        }

        void IDisposable.Dispose()
        {
            //dispose instance image
            image.Dispose();
        }
    }
}
