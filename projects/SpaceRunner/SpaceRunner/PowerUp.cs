using System;
using System.Drawing;

namespace SpaceRunner
{
    class PowerUp : GameObject
    {
        public const float Rotate = .39f;

        public enum PowerUpType
        {
            Life = 0,
            Ammo = 1,
            Fuel = 2,
            Max = 3,
            Firework = 4,
        }

        public static readonly Image lifeImage = Game.LoadImage("life.bmp", Game.PowerUpSize),
            ammoImage = Game.LoadImage("ammo.bmp", Game.PowerUpSize),
            fuelImage = Game.LoadImage("fuel.bmp", Game.PowerUpSize),
            blankImage = Game.LoadImage("fireworks\\blank.bmp", 1);

        public static void Dispose()
        {
            ammoImage.Dispose();
            fuelImage.Dispose();
            lifeImage.Dispose();
        }

        public readonly PowerUpType Type;
        //bool paid = false;

        public override decimal Score
        {
            get
            {
                return 0m;
            }
        }

        PowerUp(float x, float y, PowerUpType type)
            : base(x, y, Game.PowerUpSize, type == PowerUpType.Ammo ? ammoImage :
            type == PowerUpType.Fuel ? fuelImage :
            type == PowerUpType.Life ? lifeImage :
            type == PowerUpType.Firework ? blankImage :
            null, Rotate)
        {
#if DEBUG
            if (image == null)
                throw new Exception();
#endif
            this.Type = type;
        }

        public static void NewFirework()
        {
            PointF point = Game.RandomEdgePoint();
            new PowerUp(point.X, point.Y, PowerUpType.Firework);
        }

        public static void NewPowerUp()
        {
            PointF point = Game.RandomEdgePoint();
            NewPowerUp(point.X, point.Y);
        }
        public static void NewPowerUp(float x, float y)
        {
            int max = (int)PowerUpType.Max;
            PowerUpType[] types = new PowerUpType[max];
            for (int i = 0 ; i < max ; ++i)
                types[i] = (PowerUpType)i;
            PowerUpType type = Game.Random.SelectValue<PowerUpType>(types, delegate(PowerUpType powerUpType)
            {
                switch (powerUpType)
                {
                case PowerUpType.Ammo:
                    return Game.PowerUpAmmoChance;
                case PowerUpType.Fuel:
                    return Game.PowerUpFuelChance;
                case PowerUpType.Life:
                    return Game.PowerUpLifeChance;
                default:
                    throw new Exception();
                }
            });

            new PowerUp(x, y, type);
        }
        public static PowerUp NewPowerUp(float x, float y, PowerUpType type)
        {
            return new PowerUp(x, y, type);
        }

        //lowest type priority, never hits anything but itself so do nothing
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
            case PowerUpType.Firework:
                Forms.GameForm.Game.AddAmmo();
                break;
            case PowerUpType.Life:
                Forms.GameForm.Game.AddLife(Game.PlayerLife);
                break;
            case PowerUpType.Fuel:
                Forms.GameForm.Game.AddFuel();
                break;
#if DEBUG
            default:
                throw new Exception();
#endif
            }

            //call HitPlayer to do cleanup stuff and add to score, but return 0 to do no damage
            base.HitPlayer();
            return 0f;
        }

        public override void Die()
        {
            switch (Type)
            {
            case PowerUpType.Firework:
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

            base.Die();
        }
    }
}
