using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    class AlienShip : GameObject
    {
        private static readonly Image AlienShipImage;

        static AlienShip()
        {
            AlienShipImage = Game.LoadImage("alienship.bmp", Game.AlienShipSize);
        }

        public static void Dispose()
        {
            AlienShipImage.Dispose();
        }

        private float life, baseLife, fireRate, speedMult;

        public static void NewAlienShip()
        {
            PointF point = Game.RandomEdgePoint();
            new AlienShip(point.X, point.Y);
        }

        private AlienShip(float x, float y)
            : base(x, y, Game.AlienShipSize, AlienShipImage)
        {
            life = baseLife = RandVal(Game.AlienShipLife);
            fireRate = RandVal(Game.AlienShipFireRate);
            speedMult = RandVal(Game.AlienShipSpeedMult);
        }

        public override decimal Score
        {
            get
            {
                return GetScore(baseLife, Game.AlienShipDeathScoreMult);
            }
        }

        private void Damage(float amt)
        {
            amt = Game.Random.GaussianOE(amt, Game.AlienShipDamageRandomness, Game.AlienShipDamageOEPct, 0);
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

        public override void Die()
        {
            base.Die();

            Bullet.BulletExplosion(x, y, Game.AlienShipExplosionBullets);
        }

        protected override void OnStep()
        {
            //speed adjusts based on distance from player to keep from running into the player
            speed = Game.GetDistance(x, y) / Game.MapSize * speedMult * Game.BasePlayerSpeed;

            //fire at the player
            if (Game.Random.Bool(fireRate))
                Game.ShootAtPlayer(speed, x, y, size);
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
                Damage(fuelExplosion.GetDamage(Game.GetDistance(x, y, obj.X, obj.Y)) * Game.AlienShipFuelExplosionDamageMult);
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
            Damage(asteroid.Area / Game.AsteroidAreaToDamageRatio);
            if (life > 0)
                Forms.GameForm.Game.RemoveObject(asteroid);
            else
                asteroid.Die();
        }

        private void HitBullet(Bullet bullet)
        {
            //bullets fired by aliens or alien ships wont hit alien ships
            if (bullet.Friendly != Bullet.FriendlyStatus.Enemy)
            {
                Damage(Game.BulletDamage * ( bullet.Friendly == Bullet.FriendlyStatus.Friend ? Game.AlienShipFriendlyBulletDamageMult : Game.AlienShipNeutralBulletDamageMult ));
                bullet.Die();
            }
        }

        private void CollectPowerUp(PowerUp powerUp)
        {
            //collect the power up
            switch (powerUp.Type)
            {
            case PowerUp.PowerUpType.Firework:
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
            Forms.GameForm.Game.RemoveObject(powerUp);
        }

        private void CollectLifeDust(GameObject obj)
        {
            AddLife(Game.AlienShipLifeInc / Game.LifeDustAmtToHeal);
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
            if (!Forms.GameForm.Game.Dead)
            {
                //either kill the ship or the player
                damage = Math.Min(Forms.GameForm.Game.CurrentLifePart, life);
                Damage(damage);
            }
            return damage;
        }

        private static float RandVal(float value)
        {
            return Game.Random.GaussianCapped(value, Game.AlienShipStatRandomness, value * Game.AlienShipStatCap);
        }

        public override void Draw(Graphics graphics, int centerX, int centerY)
        {
            base.Draw(graphics, centerX, centerY);

            Game.DrawHealthBar(graphics, this, life / baseLife);
        }
    }
}
