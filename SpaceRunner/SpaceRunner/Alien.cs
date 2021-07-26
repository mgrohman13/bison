using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    internal class Alien : GameObject
    {
        private static readonly Image AlienImage;

        static Alien()
        {
            AlienImage = Game.LoadImage("alien.bmp", Game.AlienSize);
        }

        internal static void StaticDispose()
        {
            AlienImage.Dispose();
        }

        private float fireRate;
        private int ammo, life, fuel, coolDown;
        private PowerUp droppedLife;

        internal static Alien NewAlien(Game game)
        {
            PointF point = game.RandomEdgePoint();
            return NewAlien(game, point.X, point.Y);
        }

        internal static Alien NewAlien(Game game, float x, float y)
        {
            return new Alien(game, x, y);
        }

        private Alien(Game game, float x, float y, int life, PowerUp droppedLife)
            : this(game, x, y)
        {
            this.life = life;
            this.droppedLife = droppedLife;
            this.coolDown = -1;
        }

        private Alien(Game game, float x, float y)
            : base(game, x, y, game.GameRand.GaussianCapped(Game.AlienSpeed, Game.AlienSpeedRandomness), Game.AlienSize, AlienImage, Game.PowerUpRotate)
        {
            fireRate = 0;
            ammo = 0;
            life = 0;
            fuel = 0;
            droppedLife = null;
        }

        internal override decimal Score
        {
            get
            {
                return ( (decimal)Game.GetDistance(xDir, yDir) + (decimal)speed ) * Game.AlienSpeedScoreMult + (decimal)fireRate * Game.AlienFireRateScoreMult;
            }
        }

        protected override void Collide(GameObject obj)
        {
            Bullet bullet;
            PowerUp powerUp;
            Asteroid asteroid;
            if (( bullet = obj as Bullet ) != null)
                HitBullet(bullet);
            else if (( powerUp = obj as PowerUp ) != null)
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

        private void HitBullet(Bullet bullet)
        {
            if (bullet.Friendly == Bullet.FriendlyStatus.Friend)
                AddScore(Score);
            else if (bullet.Friendly == Bullet.FriendlyStatus.Enemy)
                AddScore(-Score);

            this.Die();
            bullet.Die();
        }

        private void CollectPowerUp(PowerUp powerUp)
        {
            if (powerUp != droppedLife)
            {
                switch (powerUp.Type)
                {
                case PowerUp.PowerUpType.Ammo:
                    AddAmmo();
                    break;
                case PowerUp.PowerUpType.Fuel:
                    AddFuel();
                    break;
                case PowerUp.PowerUpType.Life:
                    ++life;
                    break;
#if DEBUG
                default:
                    throw new Exception();
#endif
                }
                Game.RemoveObject(powerUp);
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
            if (inc1 > inc2 && Game.GameRand.Bool())
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

        private void HitAsteroid(Asteroid asteroid)
        {
            float damage = Game.RandDmgToAlien(asteroid.Area / Game.AsteroidAreaToAlienDamageRatio);

            if (Game.GetDistance(xDir, yDir) + speed > damage)
            {
                Explosion.NewExplosion(Game, damage, Game.AlienSpeed, asteroid, this);

                if (HasConstSpeed())
                {
                    damage = ReduceConstSpeed(damage);
                    //use const speed to save from getting destroyed
                    while (speed <= damage && HasConstSpeed())
                        damage = ReduceConstSpeed(damage);
                }

                speed -= damage;
                AddScore((decimal)damage * Game.AlienSpeedScoreMult);
                Game.RemoveObject(asteroid);
            }
            else
            {
                this.Die();
                asteroid.Die();
            }
        }

        private bool HasConstSpeed()
        {
            return ( xDir != 0 || yDir != 0 );
        }

        private float ReduceConstSpeed(float damage)
        {
            float constSpeed = Game.GetDistance(xDir, yDir);
            float constDmg = Game.GameRand.Weighted(damage, Game.AlienConstSpeedReduceWeight);
            if (constDmg > constSpeed)
            {
                constDmg = constSpeed;
                xDir = yDir = 0;
            }
            else
            {
                Game.NormalizeDirs(ref xDir, ref yDir, constSpeed - constDmg);
            }
            damage -= constDmg;
            AddScore((decimal)constDmg * Game.AlienSpeedScoreMult);
            return damage;
        }

        private void CollectLifeDust(GameObject obj)
        {
            speed += RandVal(LifeDust.GetHeal(Game.AlienSpeedInc, obj));
            obj.Die();
        }

        private float RandVal(float value)
        {
            return Game.GameRand.GaussianCapped(value, Game.AlienIncRandomness, value * Game.AlienIncCap);
        }

        internal override void Draw(Graphics graphics, int centerX, int centerY)
        {
            base.Draw(graphics, centerX, centerY);

            if (fuel > 0 || ammo > 0 || life > 0)
            {
                Image image = null;
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
                DrawImage(graphics, image, centerX, centerY, 0, x, y, Game.PowerUpSize, curAngle);
            }
        }

        protected override void OnStep()
        {
            if (fireRate > 0)
            {
                float towardsPlayer = speed;
                if (HasConstSpeed())
                {
                    float xMove, yMove;
                    GetTotalMove(out xMove, out yMove);
                    towardsPlayer = Game.GetDistance(x, y) - Game.GetDistance(x + xMove, y + yMove);
                }
                Game.ShootAtPlayer(fireRate, ref coolDown, towardsPlayer, x, y, Size);
            }
        }

        internal override void Die()
        {
            base.Die();

            DropPowerUps();
            Explosion.NewExplosion(Game, this);
        }

        private void DropPowerUps()
        {
            while (--ammo > -1)
                PowerUp.NewPowerUp(Game, OffsetPowerUp(x), OffsetPowerUp(y), PowerUp.PowerUpType.Ammo);
            while (--fuel > -1)
                PowerUp.NewPowerUp(Game, OffsetPowerUp(x), OffsetPowerUp(y), PowerUp.PowerUpType.Fuel);

            if (--life > -1)
            {
                droppedLife = PowerUp.NewPowerUp(Game, OffsetPowerUp(x), OffsetPowerUp(y), PowerUp.PowerUpType.Life);
                new Alien(Game, x, y, life, droppedLife);
            }
        }

        private float OffsetPowerUp(float val)
        {
            return val + Game.GameRand.GaussianFloat();
        }

        protected override void HitPlayer()
        {
            base.HitPlayer();

            DropPowerUps();
            Explosion.NewExplosion(Game, this);
            //always kill player
            Game.HitPlayer(Game.PlayerLife, false);
        }
    }
}
