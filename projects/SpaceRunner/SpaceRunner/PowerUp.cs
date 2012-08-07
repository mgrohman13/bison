using System;
using System.Drawing;

namespace SpaceRunner
{
    class PowerUp : GameObject
    {
        public enum PowerUpType
        {
            Life,
            Ammo,
            Fuel,
            Firework,
        }

        public static readonly Image LifeImage, AmmoImage, FuelImage, BlankImage;

        static PowerUp()
        {
            LifeImage = Game.LoadImage("life.bmp", Game.PowerUpSize);
            AmmoImage = Game.LoadImage("ammo.bmp", Game.PowerUpSize);
            FuelImage = Game.LoadImage("fuel.bmp", Game.PowerUpSize);
            BlankImage = Game.LoadImage("fireworks\\blank.bmp", 1);
        }

        public static void Dispose()
        {
            AmmoImage.Dispose();
            FuelImage.Dispose();
            LifeImage.Dispose();
            BlankImage.Dispose();
        }

        public readonly PowerUpType Type;

        public static PowerUp NewFirework()
        {
            PointF point = Game.RandomEdgePoint();
            return NewPowerUp(point.X, point.Y, PowerUpType.Firework);
        }

        public static PowerUp NewPowerUp()
        {
            PointF point = Game.RandomEdgePoint();
            return NewPowerUp(point.X, point.Y);
        }

        public static PowerUp NewPowerUp(float x, float y)
        {
            var types = new System.Collections.Generic.Dictionary<PowerUpType, int>();
            types.Add(PowerUpType.Ammo, Game.PowerUpAmmoChance);
            types.Add(PowerUpType.Fuel, Game.PowerUpFuelChance);
            types.Add(PowerUpType.Life, Game.PowerUpLifeChance);

            return NewPowerUp(x, y, Game.Random.SelectValue<PowerUpType>(types));
        }

        public static PowerUp NewPowerUp(float x, float y, PowerUpType type)
        {
            return new PowerUp(x, y, type);
        }

        private PowerUp(float x, float y, PowerUpType type)
            : base(x, y, Game.PowerUpSize,
                    type == PowerUpType.Ammo ? AmmoImage : type == PowerUpType.Fuel ? FuelImage :
                    type == PowerUpType.Life ? LifeImage : BlankImage, Game.PowerUpRotate)
        {
            this.Type = type;
        }

        public override decimal Score
        {
            get
            {
                switch (this.Type)
                {
                case PowerUpType.Firework:
                case PowerUpType.Ammo:
                    return Game.IncAmmo;
                case PowerUpType.Fuel:
                    return (decimal)Game.IncFuel / (decimal)Game.FuelMult;
                case PowerUpType.Life:
                    return Game.ScoreToDamageRatio * (decimal)Game.PlayerLife;
                }
#if DEBUG
                throw new Exception();
#endif
            }
        }

        protected override void Collide(GameObject obj)
        {
            if (obj is PowerUp)
                BumpCollision(obj);
#if DEBUG
            //lowest type priority, never hits anything but itself
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
                Forms.GameForm.Game.AddLife(Game.PlayerLife, true);
                break;
            case PowerUpType.Fuel:
                Forms.GameForm.Game.AddFuel();
                break;
#if DEBUG
            default:
                throw new Exception();
#endif
            }

            return base.HitPlayer();
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
