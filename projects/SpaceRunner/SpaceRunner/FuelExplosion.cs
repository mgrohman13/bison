using System;
using System.Drawing;

namespace SpaceRunner
{
    internal class FuelExplosion : GameObject, Game.IChecksExtraSectors
    {
        //if its size is greater than the sector size minus the maximum object size (for non fuel explosions),
        //check for collisions 2 sectors away
        int Game.IChecksExtraSectors.DistanceChecked
        {
            get
            {
                return size < Game.SectorSize - Game.AsteroidMaxSize ? 1 : 2;
            }
        }

        //the amount image size is incremented each image
        const float ImgStep = ( Game.FuelExplosionSize - Game.PowerUpSize ) / NumImages;

        //GameTick * FuelExplosionSteps / Framerate is the number of frames the explosion lasts, so load one more than that many images
        const int NumImages = (int)( Game.GameTick * Game.FuelExplosionSteps / Game.Framerate + 1.5f );
        static Image[] images = LoadImages();
        static Image[] LoadImages()
        {
            float curSize = Game.PowerUpSize + ImgStep;

            Image[] retVal = new Image[NumImages];
            for (int i = 0 ; i < NumImages ; ++i)
            {
                //load a random one of the available images
                Image baseImage = Game.LoadImage("fuelExps\\" + Game.Random.Next(Game.NumFuelExplosionImages) + ".bmp", Game.FuelExplosionImageSizeHalf);
                //resize image
                int size = (int)( curSize * 2f + .5f );
                retVal[i] = ( new Bitmap(baseImage, new Size(size, size)) );
                curSize += ImgStep;
                baseImage.Dispose();
            }

            return retVal;
        }

        internal static void Dispose()
        {
            foreach (Image i in images)
                i.Dispose();
        }

        internal override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        internal static FuelExplosion NewFuelExplosion(float x, float y)
        {
            return new FuelExplosion(x, y);
        }

        private FuelExplosion(float x, float y)
            : base(x, y, Game.PowerUpSize, images[0], Game.ExplosionRotate)
        {
        }

        protected override void OnStep()
        {
            int img;

            //increment size and select proper image
            if (( img = (int)Math.Ceiling(( ( size += Game.ExplosionSpeed ) - Game.PowerUpSize ) / ImgStep) - 1 ) < NumImages)
                this.image = images[img];
            else
                //die when out of images
                this.Die();
        }

        protected override void Collide(GameObject obj)
        {
            if (!( obj is FuelExplosion ))
            {
                //only kill objects whose center is within the explosion
                if (Game.GetDistanceSqr(x, y, obj.X, obj.Y) < size * size)
                {
                    LifeDust lifeDust;
                    if (( lifeDust = obj as LifeDust ) == null || lifeDust.HitBy(this))
                        obj.Die();
                }
            }
        }

        protected override float HitPlayer()
        {
            return GetDamage(Game.GetDistance(x, y));
        }

        internal float GetDamage(float dist)
        {
            //do more damage closer to center
            return Game.FuelExplosionDamage * size / ( dist + Game.FuelExplosionDamageStartDist );
        }
    }
}
