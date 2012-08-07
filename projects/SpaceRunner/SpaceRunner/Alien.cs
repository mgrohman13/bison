using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    class Alien : GameObject
    {
        private static readonly Image AlienImage;

        static Alien()
        {
            AlienImage = Game.LoadImage("alien.bmp", Game.AlienSize);
        }

        public static void Dispose()
        {
            AlienImage.Dispose();
        }

        private float fireRate;
        private int ammo, life, fuel;
        private PowerUp droppedLife;

        public static void NewAlien()
        {
            PointF point = Game.RandomEdgePoint();
            NewAlien(point.X, point.Y);
        }

        public static void NewAlien(float x, float y)
        {
            new Alien(x, y);
        }

        Alien(float x, float y, int life, PowerUp droppedLife)
            : this(x, y)
        {
            this.life = life;
            this.droppedLife = droppedLife;
        }

        Alien(float x, float y)
            : base(x, y, GetStartSpeed(), Game.AlienSize, AlienImage, Game.PowerUpRotate)
        {
            fireRate = 0;
            ammo = 0;
            life = 0;
            fuel = 0;
            droppedLife = null;
        }

        public override decimal Score
        {
            get
            {
                return ( (decimal)Game.GetDistance(xDir, yDir) + (decimal)speed ) * Game.AlienSpeedScoreMult + (decimal)fireRate * Game.AlienFireRateScoreMult;
            }
        }

        private static float GetStartSpeed()
        {
            return Game.Random.GaussianCapped(Game.AlienSpeed, Game.AlienSpeedRandomness);
        }

        protected override void Collide(GameObject obj)
        {
            PowerUp powerUp;
            Asteroid asteroid;
            if (( powerUp = obj as PowerUp ) != null)
                CollectPowerUp(powerUp);
            else if (( asteroid = obj as Asteroid ) != null)
                HitAsteroid(asteroid);
            else if (obj is Alien)
                BumpCollision(obj);
            else if (obj is LifeDust)
                CollectLifeDust(obj);
#if DEBUG
            else
                throw new Exception();
#endif
        }

        private void CollectPowerUp(PowerUp powerUp)
        {
            if (powerUp != droppedLife)
            {
                switch (powerUp.Type)
                {
                case PowerUp.PowerUpType.Firework:
                case PowerUp.PowerUpType.Ammo:
                    AddAmmo();
                    break;
                case PowerUp.PowerUpType.Fuel:
                    AddFuel();
                    break;
                case PowerUp.PowerUpType.Life:
                    AddLife();
                    break;
#if DEBUG
                default:
                    throw new Exception();
#endif
                }
                Forms.GameForm.Game.RemoveObject(powerUp);
            }
        }

        private void AddAmmo()
        {
            if (ammo > 0)
                fireRate += RandVal(Game.AlienFireRateInc);
            else
                fireRate += RandVal(Game.AlienFireRate);

            ++ammo;
        }

        private void AddFuel()
        {
            float inc1 = RandVal(Game.AlienSpeedInc);
            float inc2 = RandVal(Game.AlienSpeedInc);
            if (inc1 > inc2 && Game.Random.Bool())
            {
                float temp = inc1;
                inc1 = inc2;
                inc2 = temp;
            }

            float xd, yd;
            Game.GetRandomDirection(out xd, out yd, inc1);
            xDir += xd;
            yDir += yd;
            speed += inc2;

            ++fuel;
        }

        private void AddLife()
        {
            ++life;
        }

        private void HitAsteroid(Asteroid asteroid)
        {
            float damage = asteroid.Area / Game.AsteroidAreaToAlienDamageRatio;

            if (xDir != 0 || yDir != 0)
            {
                damage = ReduceConstSpeed(damage);
                //use const speed to save from getting destroyed
                while (speed < damage && ( xDir != 0 || yDir != 0 ))
                    damage = ReduceConstSpeed(damage);
            }

            if (speed > damage)
            {
                speed -= damage;
                AddScore((decimal)damage * Game.AlienSpeedScoreMult);
                Forms.GameForm.Game.RemoveObject(asteroid);
            }
            else
            {
                this.Die();
                asteroid.Die();
            }
        }

        private float ReduceConstSpeed(float damage)
        {
            float constSpeed = Game.GetDistance(xDir, yDir);
            if (constSpeed > 0)
            {
                float constDmg = Game.Random.Weighted(damage, Game.AlienConstSpeedReduceWeight);
                damage -= constDmg;
                constSpeed -= constDmg;
                decimal score;
                if (constSpeed > 0)
                {
                    Game.NormalizeDirs(ref xDir, ref yDir, constSpeed);
                    score = (decimal)constDmg;
                }
                else
                {
                    damage += -constSpeed;
                    xDir = yDir = 0;
                    score = (decimal)constSpeed + (decimal)constDmg;
                }
                AddScore(score * Game.AlienSpeedScoreMult);
            }
            return damage;
        }

        private void CollectLifeDust(GameObject obj)
        {
            speed += RandVal(Game.AlienSpeedInc / Game.LifeDustAmtToHeal);
            obj.Die();
        }

        private static float RandVal(float value)
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
                    image = PowerUp.LifeImage;
                else if (ammo > 0)
                    image = PowerUp.AmmoImage;
                else if (fuel > 0)
                    image = PowerUp.FuelImage;
#if DEBUG
                else
                    throw new Exception();
#endif
                DrawImage(graphics, image, centerX, centerY, 0, x, y, curAngle);
            }
        }

        protected override void OnStep()
        {
            if (Game.Random.Bool(fireRate))
            {
                float towardsPlayer = speed;
                if (xDir != 0 || yDir != 0)
                {
                    float xMove, yMove;
                    GetTotalMove(out xMove, out yMove);
                    towardsPlayer = Game.GetDistance(x, y) - Game.GetDistance(x + xMove, y + yMove);
                }
                Game.ShootAtPlayer(towardsPlayer, x, y, size);
            }
        }

        public override void Die()
        {
            base.Die();

            DropPowerUps();
            Explosion.NewExplosion(x, y, speed);
        }

        private void DropPowerUps()
        {
            while (--ammo > -1)
                PowerUp.NewPowerUp(OffsetPowerUp(x), OffsetPowerUp(y), PowerUp.PowerUpType.Ammo);
            while (--fuel > -1)
                PowerUp.NewPowerUp(OffsetPowerUp(x), OffsetPowerUp(y), PowerUp.PowerUpType.Fuel);

            if (--life > -1)
            {
                droppedLife = PowerUp.NewPowerUp(OffsetPowerUp(x), OffsetPowerUp(y), PowerUp.PowerUpType.Life);
                new Alien(x, y, life, droppedLife);
            }
        }

        private static float OffsetPowerUp(float val)
        {
            return val + Game.Random.GaussianFloat();
        }

        protected override float HitPlayer()
        {
            base.HitPlayer();

            DropPowerUps();
            //always kill player
            return Game.PlayerLife;
        }
    }
}
