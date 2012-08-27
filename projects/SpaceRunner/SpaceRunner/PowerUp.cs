using System;
using System.Drawing;
using Form = SpaceRunner.Forms.GameForm;

namespace SpaceRunner
{
    internal class PowerUp : GameObject
    {
        internal enum PowerUpType
        {
            Life,
            Ammo,
            Fuel,
        }

        internal static readonly Image LifeImage, AmmoImage, FuelImage;

        static PowerUp()
        {
            LifeImage = Game.LoadImage("life.bmp", Game.PowerUpSize);
            AmmoImage = Game.LoadImage("ammo.bmp", Game.PowerUpSize);
            FuelImage = Game.LoadImage("fuel.bmp", Game.PowerUpSize);
        }

        internal static void Dispose()
        {
            LifeImage.Dispose();
            AmmoImage.Dispose();
            FuelImage.Dispose();
        }

        internal readonly PowerUpType Type;

        internal static PowerUp NewPowerUp()
        {
            PointF point = Game.RandomEdgePoint();
            return NewPowerUp(point.X, point.Y);
        }

        internal static PowerUp NewPowerUp(float x, float y)
        {
            var types = new System.Collections.Generic.Dictionary<PowerUpType, int>();
            types.Add(PowerUpType.Ammo, Game.PowerUpAmmoChance);
            types.Add(PowerUpType.Fuel, Game.PowerUpFuelChance);
            types.Add(PowerUpType.Life, Game.PowerUpLifeChance);

            return NewPowerUp(x, y, Game.Random.SelectValue<PowerUpType>(types));
        }

        internal static PowerUp NewPowerUp(float x, float y, PowerUpType type)
        {
            return new PowerUp(x, y, type);
        }

        private PowerUp(float x, float y, PowerUpType type)
            : base(x, y, Game.PowerUpSize,
                    type == PowerUpType.Ammo ? AmmoImage : type == PowerUpType.Fuel ? FuelImage :
                    type == PowerUpType.Life ? LifeImage : null, Game.PowerUpRotate)
        {
            this.Type = type;
        }

        internal override decimal Score
        {
            get
            {
                return 0;
            }
        }

        protected override void Collide(GameObject obj)
        {
            if (obj is PowerUp)
                BumpCollision(obj);
#if DEBUG
            else
                throw new Exception();
#endif
        }

        protected override float HitPlayer()
        {
            switch (Type)
            {
            case PowerUpType.Ammo:
                Form.Game.AddAmmo();
                break;
            case PowerUpType.Life:
                Form.Game.AddLife(Game.PlayerLife, true);
                break;
            case PowerUpType.Fuel:
                Form.Game.AddFuel();
                break;
#if DEBUG
            default:
                throw new Exception();
#endif
            }

            Form.Game.RemoveObject(this);
            return 0;
        }

        internal override void Die()
        {
            base.Die();

            switch (Type)
            {
            case PowerUpType.Ammo:
                Bullet.BulletExplosion(x, y, Game.PowerUpAmmoExplosionBullets);
                break;
            case PowerUpType.Fuel:
                FuelExplosion.NewFuelExplosion(x, y);
                break;
            case PowerUpType.Life:
                LifeDust.NewLifeDust(x, y, Game.LifeDustAmtToHeal);
                break;
#if DEBUG
            default:
                throw new Exception();
#endif
            }
        }
    }
}
