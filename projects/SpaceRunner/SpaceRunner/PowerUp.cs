using System;
using System.Drawing;
using System.Collections.Generic;

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

        internal static void StaticDispose()
        {
            LifeImage.Dispose();
            AmmoImage.Dispose();
            FuelImage.Dispose();
        }

        private static readonly Dictionary<PowerUpType, int> types = new Dictionary<PowerUpType, int> {
            { PowerUpType.Ammo, Game.PowerUpAmmoChance },
            { PowerUpType.Fuel, Game.PowerUpFuelChance },
            { PowerUpType.Life, Game.PowerUpLifeChance },
        };

        internal readonly PowerUpType Type;

        internal static PowerUp NewPowerUp(Game game)
        {
            PointF point = game.RandomEdgePoint();
            return NewPowerUp(game, point.X, point.Y);
        }

        internal static PowerUp NewPowerUp(Game game, float x, float y)
        {
            return NewPowerUp(game, x, y, game.GameRand.SelectValue<PowerUpType>(types));
        }

        internal static PowerUp NewPowerUp(Game game, float x, float y, PowerUpType type)
        {
            return new PowerUp(game, x, y, type);
        }

        private PowerUp(Game game, float x, float y, PowerUpType type)
            : base(game, x, y, Game.PowerUpSize,
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

        protected override void HitPlayer()
        {
            switch (Type)
            {
            case PowerUpType.Ammo:
                Game.AddAmmo();
                break;
            case PowerUpType.Life:
                Game.AddLife(Game.PlayerLife, true);
                break;
            case PowerUpType.Fuel:
                Game.AddFuel();
                break;
#if DEBUG
            default:
                throw new Exception();
#endif
            }

            Game.RemoveObject(this);
        }

        internal override void Die()
        {
            base.Die();

            switch (Type)
            {
            case PowerUpType.Ammo:
                Bullet.BulletExplosion(Game, x, y, Game.PowerUpAmmoExplosionBullets);
                break;
            case PowerUpType.Fuel:
                FuelExplosion.NewFuelExplosion(Game, x, y);
                break;
            case PowerUpType.Life:
                LifeDust.NewLifeDust(Game, x, y, Game.LifeDustAmtToHeal);
                break;
#if DEBUG
            default:
                throw new Exception();
#endif
            }
        }
    }
}
