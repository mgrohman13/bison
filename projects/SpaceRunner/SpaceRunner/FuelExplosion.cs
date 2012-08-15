using System;
using System.Drawing;

namespace SpaceRunner
{
    internal class FuelExplosion : GameObject, Game.IChecksExtraSectors
    {
        private static readonly int NumImages;
        private static Image[] Images;

        static FuelExplosion()
        {
            NumImages = Game.Random.Round(Game.FuelExplosionTime * Game.GameTick / 1000f * Game.FuelExplosionImagesPerSecond);
            Images = new Image[NumImages];

            float size = Game.PowerUpSize;
            for (int i = 0 ; i < NumImages ; ++i)
            {
                Images[i] = Game.LoadImage("fuelExps\\" + Game.Random.Next(Game.NumFuelExplosionImages) + ".bmp", size);
                size += ( Game.FuelExplosionSize - Game.PowerUpSize ) / ( NumImages - 1 );
            }
        }

        internal static void Dispose()
        {
            foreach (Image i in Images)
                i.Dispose();
        }

        private int time = 0;

        internal static FuelExplosion NewFuelExplosion(float x, float y)
        {
            return new FuelExplosion(x, y);
        }

        private FuelExplosion(float x, float y)
            : base(x, y, Game.PowerUpSize, Images[0], Game.ExplosionRotate)
        {
            this.time = Game.Random.Round(Game.FuelExplosionTime);
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
            size += Game.ExplosionSpeed;

            if (--time > 0)
                this.image = Images[(int)( ( Game.FuelExplosionTime - time ) / Game.FuelExplosionTime * NumImages )];
            else
                this.Die();
        }

        protected override void Collide(GameObject obj)
        {
            //only hit objects whose center is within the explosion
            if (!( obj is FuelExplosion ) && Game.GetDistanceSqr(x, y, obj.X, obj.Y) < size * size)
            {
                LifeDust lifeDust = obj as LifeDust;
                if (lifeDust == null || lifeDust.HitBy(this))
                    obj.Die();
            }
        }

        protected override float HitPlayer()
        {
            return GetDamage(0, 0);
        }

        internal float GetDamage(float x, float y)
        {
            //do more damage closer to center
            return Game.FuelExplosionDamage * this.size / ( Game.GetDistance(this.x, this.y, x, y) + Game.FuelExplosionDamageStartDist );
        }

        int Game.IChecksExtraSectors.DistanceChecked
        {
            get
            {
                return (int)Math.Ceiling(( size + Game.AsteroidMaxSize ) / Game.SectorSize);
            }
        }
    }
}
