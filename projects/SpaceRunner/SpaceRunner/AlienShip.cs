using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    class AlienShip : GameObject
    {
        static Image AlienShipImage = Game.LoadImage("alienship.bmp", Game.AlienShipSize);

        public static void Dispose()
        {
            AlienShipImage.Dispose();
        }

        float life, fireRate, speedMult;
        float baseLife;
        void Damage(float amt)
        {
            amt = Game.Random.GaussianCapped(amt, Game.AlienShipDamageRandomness);
            if (amt > life)
                amt = life;
            Forms.GameForm.Game.AddScore((decimal)amt / (decimal)Game.AlienShipLife * GetScore() * (decimal)Game.AlienShipScoreMult);
            life -= amt;
        }
        public override decimal Score
        {
            get
            {
                return (decimal)baseLife / (decimal)Game.AlienShipLife * GetScore() * (decimal)Game.AlienShipDeathScoreMult;
            }
        }
        decimal GetScore()
        {
            return (decimal)fireRate / (decimal)Game.AlienShipFireRate * (decimal)speedMult / (decimal)Game.AlienShipSpeedMult;
        }

        AlienShip(float x, float y)
            : base(x, y, Game.AlienShipSize, AlienShipImage)
        {
            life = baseLife = RandVal(Game.AlienShipLife);
            fireRate = RandVal(Game.AlienShipFireRate);
            speedMult = RandVal(Game.AlienShipSpeedMult);
        }
        public static void NewAlienShip()
        {
            PointF point = Game.RandomEdgePoint();
            new AlienShip(point.X, point.Y);
        }

        public override void Die()
        {
            Bullet.BulletExplosion(x, y, Game.AlienShipExplosionBullets);
            base.Die();
        }

        protected override void OnStep()
        {
            //speed adjusts based on distance from player to keep from running into the player
            speed = Game.GetDistance(x, y) / Game.MapRadius * speedMult * Game.BasePlayerSpeed;
            ////move towards player
            //Game.AdjustForPlayerSpeed(ref xDir, ref  yDir, speed * 3.9f, x, y, size);
            //Game.NormalizeDirs(ref xDir, ref  yDir, speed);
            //speed = 0f;

            //fire at the player
            if (Game.Random.Bool(fireRate))
                Game.ShootAtPlayer(speed, x, y, size);
        }

        protected override void Collide(GameObject obj)
        {
            //lose life based on the type and stats of the opposing object

            PowerUp powerUp;
            Bullet bullet;
            FuelExplosion fuelExplosion;
            Asteroid asteroid;

            if (( asteroid = obj as Asteroid ) != null)
            {
                //lose life based on the area of the asteroid
                Damage(asteroid.Area / Game.AsteroidAreaToDamageRatio);
                if (life <= 0)
                    obj.Die();
                else
                    Forms.GameForm.Game.RemoveObject(obj);
            }
            else if (null != ( bullet = obj as Bullet ))
            {
                //bullets fired by aliens or alien ships wont hit alien ships
                if (bullet.Friendly != Bullet.FriendlyStatus.Enemy)
                {
                    float mult = 0;
                    switch (bullet.Friendly)
                    {
                    case Bullet.FriendlyStatus.Friend:
                        mult = Game.AlienShipFriendlyBulletDamageMult;
                        break;
                    case Bullet.FriendlyStatus.Neutral:
                        mult = Game.AlienShipNeutralBulletDamageMult;
                        break;
#if DEBUG
                    default:
                        throw new Exception();
#endif
                    }
                    Damage(Game.BulletDamage * mult);
                    obj.Die();
                }
            }
            else if (null != ( powerUp = obj as PowerUp ))
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
                    float newLife = RandVal(Game.AlienShipLifeInc);
                    life += newLife;
                    baseLife += newLife;
                    break;
#if DEBUG
                default:
                    throw new Exception();
#endif
                }
                Forms.GameForm.Game.RemoveObject(obj);
            }
            else if (( fuelExplosion = obj as FuelExplosion ) != null)
            {
                Damage(Game.AlienShipFuelExplosionDamageMult * fuelExplosion.GetDamage(Game.GetDistance(x, y, obj.X, obj.Y)));
            }
            else if (obj is AlienShip || obj is Alien)
            {
                BumpCollision(obj);
            }
            else if (obj is LifeDust)
            {
                float addlife = RandVal(Game.AlienShipLifeInc / Game.LifeDustAmtToHeal);
                life += addlife;
                baseLife += addlife;
                Forms.GameForm.Game.RemoveObject(obj);
            }
#if DEBUG
            else
            {
                throw new Exception();
            }
#endif

            //check if the ship is dead
            if (life <= 0)
            {
                Die();
            }
        }

        protected override float HitPlayer()
        {
            float retVal = 0f;
            if (!Forms.GameForm.Game.Dead)
            {
                //either kill the ship or the player
                retVal = Math.Min(Forms.GameForm.Game.CurrentLifePart, life);

                Damage(retVal);
                if (life <= 0)
                {
                    Die();
                }
            }
            return retVal;
        }

        static float RandVal(float value)
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
