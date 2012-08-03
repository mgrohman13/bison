using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    class Alien : GameObject
    {
        static readonly Image AlienImage = Game.LoadImage("alien.bmp", Game.AlienSize);

        public static void Dispose()
        {
            AlienImage.Dispose();
        }

        float fireRate;
        int ammo;
        int life;
        int fuel;
        PowerUp droppedLife;

        public override decimal Score
        {
            get
            {
                return (decimal)speed * Game.AlienSpeedScoreMult + (decimal)fireRate * Game.AlienFireRateScoreMult;
            }
        }

        public float Speed
        {
            get
            {
                return speed;
            }
        }

        Alien(float x, float y, int life, PowerUp droppedLife)
            : this(x, y)
        {
            this.life = life;
            this.droppedLife = droppedLife;
        }
        Alien(float x, float y)
            : base(x, y, startSpeed(), Game.AlienSize, AlienImage, PowerUp.Rotate)
        {
            fireRate = 0;
            ammo = 0;
            life = 0;
            fuel = 0;
            droppedLife = null;
        }
        public static void NewAlien()
        {
            PointF point = Game.RandomEdgePoint();
            new Alien(point.X, point.Y);
        }
        public static void NewAlien(float x, float y)
        {
            new Alien(x, y);
        }
        static float startSpeed()
        {
            return Game.Random.GaussianCapped(Game.AlienSpeed, Game.AlienSpeedRandomness);
        }

        protected override void Collide(GameObject obj)
        {
            PowerUp powerUp;
            Asteroid asteroid;
            if (( powerUp = obj as PowerUp ) != null)
            {
                if (powerUp != droppedLife)
                {
                    switch (powerUp.Type)
                    {
                    case PowerUp.PowerUpType.Firework:
                    case PowerUp.PowerUpType.Ammo:
                        if (++ammo > 1)
                            fireRate += RandVal(Game.AlienFireRateInc);
                        else
                            fireRate += RandVal(Game.AlienFireRate);
                        break;
                    case PowerUp.PowerUpType.Fuel:
                        ++fuel;
                        float speedInc = RandVal(Game.AlienSpeedInc);
                        float angle = Game.Random.DoubleFull((float)Math.PI);
                        xDir += (float)( Math.Cos(angle) * speedInc );
                        yDir += (float)( Math.Sin(angle) * speedInc );
                        speed += speedInc;
                        break;
                    case PowerUp.PowerUpType.Life:
                        ++life;
                        break;
#if DEBUG
                    default:
                        throw new Exception();
#endif
                    }
                    Forms.GameForm.Game.RemoveObject(obj);
                }
            }
            else if (( asteroid = obj as Asteroid ) != null)
            {
                float damage = asteroid.Area / Game.AsteroidAreaToAlienDamageRatio;
                if (xDir > 0 || yDir > 0)
                {
                    float constSpeed = Game.GetDistance(xDir, yDir) - damage;
                    if (constSpeed > 0)
                    {
                        damage = 0;
                        Game.NormalizeDirs(ref xDir, ref yDir, constSpeed);
                    }
                    else
                    {
                        damage = -constSpeed;
                        xDir = yDir = 0;
                    }
                }

                if (damage > 0)
                {
                    if (speed > damage)
                    {
                        //reduce speed
                        speed -= damage;
                        Forms.GameForm.Game.AddScore((decimal)damage * Game.AlienSpeedScoreMult);
                        //destroy asteroid
                        Forms.GameForm.Game.RemoveObject(obj);
                    }
                    else
                    {
                        //if speed falls to less than zero, kill the alien and explode the asteroid
                        this.Die();
                        obj.Die();
                    }
                }
                else
                {
                    Forms.GameForm.Game.RemoveObject(obj);
                }
            }
            else if (obj is Alien)
            {
                BumpCollision(obj);
            }
            else if (obj is LifeDust)
            {
                speed += Game.Random.GaussianCapped(Game.AlienSpeed / Game.LifeDustAmtToHeal, Game.AlienSpeedRandomness);
                Forms.GameForm.Game.RemoveObject(obj);
            }
#if DEBUG
            else
            {
                throw new Exception();
            }
#endif
        }

        static float RandVal(float value)
        {
            return Game.Random.GaussianCapped(value, Game.AlienIncRandomness, value * Game.AlienIncCap);
        }

        public override void Draw(Graphics graphics, int centerX, int centerY)
        {
            base.Draw(graphics, centerX, centerY);
            if (fuel > 0 || ammo > 0 || life > 0)
            {
                Image image;
                if (life > 0)
                    image = PowerUp.lifeImage;
                else if (ammo > 0)
                    image = PowerUp.ammoImage;
                else if (fuel > 0)
                    image = PowerUp.fuelImage;
                else
                    throw new Exception();

                float objectX = ( centerX + x ), objectY = ( centerY + y );
                Game.Rotate(graphics, curAngle, objectX, objectY);
                graphics.DrawImageUnscaled(image, Game.Round(objectX - image.Width / 2f), Game.Round(objectY - image.Height / 2f));
            }
        }

        protected override void OnStep()
        {
            if (Game.Random.Bool(fireRate))
                Game.ShootAtPlayer(speed, x, y, size);
        }

        public override void Die()
        {
            DropPowerUps();
            Explosion.NewExplosion(x, y, speed);
            base.Die();
        }

        void DropPowerUps()
        {
            while (--ammo > -1)
                PowerUp.NewPowerUp(x, y, PowerUp.PowerUpType.Ammo);
            while (--fuel > -1)
                PowerUp.NewPowerUp(x, y, PowerUp.PowerUpType.Fuel);

            if (--life > -1)
            {
                droppedLife = PowerUp.NewPowerUp(x, y, PowerUp.PowerUpType.Life);
                new Alien(x, y, life, droppedLife);
            }
        }

        protected override float HitPlayer()
        {
            DropPowerUps();
            return base.HitPlayer();
        }
    }
}
