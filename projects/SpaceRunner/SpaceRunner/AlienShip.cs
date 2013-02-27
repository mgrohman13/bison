using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    internal class AlienShip : GameObject
    {
        private static readonly Image AlienShipImage;

        static AlienShip()
        {
            AlienShipImage = Game.LoadImage("alienship.bmp", Game.AlienShipSize);
        }

        internal static void Dispose()
        {
            AlienShipImage.Dispose();
        }

        private float life, baseLife, fireRate, speedMult;

        internal static AlienShip NewAlienShip(Game game)
        {
            PointF point = game.RandomEdgePoint();
            return new AlienShip(game, point.X, point.Y);
        }

        private AlienShip(Game game, float x, float y)
            : base(game, x, y, Game.AlienShipSize, AlienShipImage)
        {
            life = baseLife = RandVal(Game.AlienShipLife);
            fireRate = RandVal(Game.AlienShipFireRate);
            speedMult = RandVal(Game.AlienShipSpeedMult);
        }

        internal override decimal Score
        {
            get
            {
                return GetScore(baseLife, Game.AlienShipDeathScoreMult);
            }
        }

        private void Damage(float amt, params GameObject[] objs)
        {
#if DEBUG
            if (objs.Length == 0)
                throw new Exception();
#endif
            amt = Game.RandDmgToAlien(amt);

            if (objs[0] is FuelExplosion ?
                    Game.GameRand.Bool(amt / Game.FuelExplosionDamage / Game.AlienShipFuelExplosionDamageMult / Game.ExplosionTime)
                    : amt > Game.PlayerLife)
                Explosion.NewExplosion(Game, objs);

            if (amt > life)
                amt = life;
            AddScore(GetScore(amt, Game.AlienShipScoreMult));
            life -= amt;

            //check if the ship is dead
            if (life <= 0)
                Die();
        }

        private decimal GetScore(float life, decimal mult)
        {
            return ( (decimal)life / (decimal)Game.AlienShipLife * (decimal)fireRate / (decimal)Game.AlienShipFireRate
                    * (decimal)speedMult / (decimal)Game.AlienShipSpeedMult * mult );
        }

        internal override void Die()
        {
            base.Die();

            Bullet.BulletExplosion(Game, x, y, Game.AlienShipExplosionBullets);
        }

        protected override void OnStep()
        {
            //speed adjusts based on distance from player to keep from running into the player
            speed = Game.GetDistance(x, y) / Game.MapSize * speedMult * Game.BasePlayerSpeed;

            //fire at the player
            if (Game.GameRand.Bool(fireRate))
                Game.ShootAtPlayer(speed, x, y, Size);
        }

        protected override void Collide(GameObject obj)
        {
            Asteroid asteroid;
            Bullet bullet;
            PowerUp powerUp;
            FuelExplosion fuelExplosion;
            if (( asteroid = obj as Asteroid ) != null)
                HitAsteroid(asteroid);
            else if (( bullet = obj as Bullet ) != null)
                HitBullet(bullet);
            else if (( powerUp = obj as PowerUp ) != null)
                CollectPowerUp(powerUp);
            else if (( fuelExplosion = obj as FuelExplosion ) != null)
                Damage(fuelExplosion.GetDamage(x, y) * Game.AlienShipFuelExplosionDamageMult, fuelExplosion);
            else if (obj is AlienShip || obj is Alien)
                BumpCollision(obj);
            else if (obj is LifeDust)
                CollectLifeDust(obj);
#if DEBUG
            else
                throw new Exception();
#endif
        }

        private void HitAsteroid(Asteroid asteroid)
        {
            //lose life based on the area of the asteroid
            Damage(asteroid.Area / Game.AsteroidAreaToDamageRatio, asteroid, this);
            if (life > 0)
                Game.RemoveObject(asteroid);
            else
                asteroid.Die();
        }

        private void HitBullet(Bullet bullet)
        {
            //bullets fired by aliens or alien ships wont hit alien ships
            if (bullet.Friendly != Bullet.FriendlyStatus.Enemy)
            {
                Damage(( bullet.Friendly == Bullet.FriendlyStatus.Friend ? Game.AlienShipFriendlyBulletDamageMult : Game.AlienShipNeutralBulletDamageMult )
                        * Game.BulletDamage, bullet, this, this);
                bullet.Die();
            }
        }

        private void CollectPowerUp(PowerUp powerUp)
        {
            //collect the power up
            switch (powerUp.Type)
            {
            case PowerUp.PowerUpType.Ammo:
                fireRate += RandVal(Game.AlienShipFireRateInc);
                break;
            case PowerUp.PowerUpType.Fuel:
                speedMult += RandVal(Game.AlienShipSpeedMultInc);
                break;
            case PowerUp.PowerUpType.Life:
                AddLife(Game.AlienShipLifeInc);
                break;
#if DEBUG
            default:
                throw new Exception();
#endif
            }
            Game.RemoveObject(powerUp);
        }

        private void CollectLifeDust(GameObject obj)
        {
            AddLife(LifeDust.GetHeal(Game.AlienShipLifeInc, obj));
            obj.Die();
        }

        private void AddLife(float amt)
        {
            float addlife = RandVal(amt);
            life += addlife;
            baseLife += addlife;
        }

        protected override float HitPlayer()
        {
            float damage = 0;
            if (!Game.Dead)
            {
                //either kill the ship or the player
                damage = Math.Min(Game.CurrentLifePart, life);
                Damage(damage, Game.GetPlayerObject(), this);
            }
            return damage;
        }

        private float RandVal(float value)
        {
            return Game.GameRand.GaussianCapped(value, Game.AlienShipStatRandomness, value * Game.AlienShipStatCap);
        }

        internal override void Draw(Graphics graphics, int centerX, int centerY)
        {
            base.Draw(graphics, centerX, centerY);

            Game.DrawHealthBar(graphics, this, life / baseLife);
        }
    }
}
