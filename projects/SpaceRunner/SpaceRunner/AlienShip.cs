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

        internal static void StaticDispose()
        {
            AlienShipImage.Dispose();
        }

        private float life, baseLife, fireRate, speedMult;
        private int coolDown;

        internal static AlienShip NewAlienShip(Game game)
        {
            PointF point = game.RandomEdgePoint();
            return new AlienShip(game, point.X, point.Y);
        }

        private AlienShip(Game game, float x, float y)
            : base(game, x, y, Game.AlienShipSize, AlienShipImage)
        {
            this.life = this.baseLife = RandVal(Game.AlienShipLife);
            this.fireRate = RandVal(Game.AlienShipFireRate);
            this.speedMult = RandVal(Game.AlienShipSpeedMult, 1);
            this.coolDown = -1;
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

            if (!( objs[0] is FuelExplosion ))
                Explosion.NewExplosion(Game, amt, Game.PlayerLife, objs);
            else if (Game.GameRand.Bool(amt / Game.FuelExplosionDamage / Game.AlienShipFuelExplosionDamageMult / Game.ExplosionTime))
                Explosion.NewExplosion(Game, this);

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
            return (decimal)GetStrMult(life) * mult;
        }

        internal float GetLifePct()
        {
            return life / baseLife;
        }
        internal float GetStrMult()
        {
            return GetStrMult(baseLife);
        }
        private float GetStrMult(float life)
        {
            return life / Game.AlienShipLife * fireRate / Game.AlienShipFireRate * speedMult / Game.AlienShipSpeedMult;
        }

        internal override void Die()
        {
            base.Die();

            Bullet.BulletExplosion(Game, x, y, GetStrMult() * Game.AlienShipExplosionBullets);
        }

        protected override void OnStep()
        {
            //speed adjusts based on distance from player to keep from running into the player
            speed = Game.GetDistance(x, y) / Game.MapSize * speedMult * Game.BasePlayerSpeed;

            Game.ShootAtPlayer(fireRate, ref coolDown, speed, x, y, Size);
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

        protected override void HitPlayer()
        {
            if (!Game.Dead)
            {
                //either kill the ship or the player
                float damage = Math.Min(Game.CurrentLifePart, life);
                Damage(damage, Game.GetPlayerObject(), this);
                Game.HitPlayer(damage, false);
            }
        }

        private float RandVal(float value, float? lowerCap = null)
        {
            lowerCap = lowerCap ?? value * Game.AlienShipStatCap;
            return Game.GameRand.GaussianCapped(value, Game.AlienShipStatRandomness, lowerCap.Value);
        }

        internal override void Draw(Graphics graphics, int centerX, int centerY)
        {
            base.Draw(graphics, centerX, centerY);

            if (Game.GetDistance(x, y) - size < Game.MapSize)
                Game.DrawHealthBar(graphics, this, GetLifePct());
        }
    }
}
