using System;
using System.Collections.Generic;
using System.Drawing;
using SpaceRunner.Images;

namespace SpaceRunner
{
    internal class FuelExplosion : GameObject, Game.IChecksExtraSectors
    {
        private static List<Bitmap> generated = new List<Bitmap>();
        private static Image[] Images;

        static FuelExplosion()
        {
            GenerateOne();
        }
        internal static void InitImages()
        {
            lock (typeof(FuelExplosion))
            {
                DisposeImages();

                GenerateOne();

                int numImages = Game.Random.GaussianInt(Game.FuelExplosionTime * Game.GameTick / 1000f * Game.FuelExplosionImagesPerSecond, .026f);
                Images = new Image[numImages];

                float size = Game.PowerUpSize;
                float sizeInc = ( Game.FuelExplosionSize - Game.PowerUpSize ) / numImages;
                for (int idx = 0 ; idx < numImages ; ++idx)
                    Images[idx] = Game.LoadImage(Game.Random.SelectValue(generated), size += sizeInc, false);
            }
        }
        private static void GenerateOne()
        {
            generated.Add(FuelExplosionGenerator.GenerateFuelExplosion(Game.Random.GaussianOEInt(195f, .13f, .13f, 104)));
        }

        internal static void StaticDispose()
        {
            foreach (Image image in generated)
                image.Dispose();
            DisposeImages();
        }
        internal static void DisposeImages()
        {
            if (Images != null)
                foreach (Image image in Images)
                    lock (image)
                        image.Dispose();
        }

        private int time = 0;

        internal static FuelExplosion NewFuelExplosion(Game game, float x, float y)
        {
            return new FuelExplosion(game, x, y);
        }

        private FuelExplosion(Game game, float x, float y)
            : base(game, x, y, Game.PowerUpSize, Images[0], Game.ExplosionRotate)
        {
            this.time = Game.GameRand.Round(Game.FuelExplosionTime);
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
                this.image = Images[(int)( ( Game.FuelExplosionTime - time ) / Game.FuelExplosionTime * Images.Length )];
            else
                this.Die();
        }

        protected override void Collide(GameObject obj)
        {
            //only hit objects whose center is within the explosion
            if (!( obj is FuelExplosion ) && Game.GetDistanceSqr(this.x, this.y, obj.X, obj.Y) < this.Size * this.Size)
            {
                LifeDust lifeDust = obj as LifeDust;
                if (lifeDust == null || lifeDust.HitBy(this))
                {
                    if (obj is Bullet)
                        Bullet.BulletExplosion(Game, obj.X, obj.Y, 1);
                    else if (!( obj is Alien ))
                        Explosion.NewExplosion(Game, obj.Size, Game.ExplosionSize, obj);
                    obj.Die();
                }
            }
        }

        protected override void HitPlayer()
        {
            Game.HitPlayer(GetDamage(0, 0));
        }

        internal float GetDamage(float x, float y)
        {
            //do more damage closer to center
            return Game.FuelExplosionDamage * this.Size / ( Game.GetDistance(this.x, this.y, x, y) + Game.FuelExplosionDamageStartDist );
        }

        int Game.IChecksExtraSectors.CheckSectors
        {
            get
            {
                return (int)Math.Ceiling(( Size + Game.AsteroidMaxSize ) / Game.SectorSize);
            }
        }
    }
}
