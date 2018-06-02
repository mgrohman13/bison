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

        private float life, baseLife, fireRate, speedMult, targetAngle, moveTypeRatio;
        private int coolDown;

        internal static AlienShip NewAlienShip(Game game)
        {
            PointF point = game.RandomEdgePoint();
            return new AlienShip(game, point.X, point.Y);
        }

        public static readonly Dictionary<int, List<PointF>> trgps = new Dictionary<int, List<PointF>>(), dirs = new Dictionary<int, List<PointF>>(), actMovs = new Dictionary<int, List<PointF>>();
        public static readonly Dictionary<int, List<float>> mtrrs = new Dictionary<int, List<float>>(), spds = new Dictionary<int, List<float>>(), sms = new Dictionary<int, List<float>>();
        private static void Add<T>(Dictionary<int, List<T>> d, int tc, T obj)
        {
            List<T> l;
            if (!d.TryGetValue(tc, out l))
                d.Add(tc, l = new List<T>());
            l.Add(obj);
        }

        private AlienShip(Game game, float x, float y)
            : base(game, x, y, Game.AlienShipSize, AlienShipImage)
        {
            this.life = this.baseLife = RandVal(Game.AlienShipLife);
            this.fireRate = RandVal(Game.AlienShipFireRate);
            this.speedMult = RandVal(Game.AlienShipSpeedMult, 1);
            this.coolDown = -1;

            targetAngle = game.GetRandomAngle();
            moveTypeRatio = game.GameRand.Weighted(game.GameRand.Weighted(game.GameRand.DoubleHalf(1)));

            //Console.WriteLine(speedMult);
            //Console.WriteLine(moveTypeRatio);
            //Console.WriteLine(Game.GetPoint(this.targetAngle, (float)( Game.MapSize / Math.Sqrt(Game.AlienShipSpeedMult) / this.speedMult )));
            Add(trgps, Game.TickCount, Game.GetPoint(this.targetAngle, Game.MapSize / this.speedMult));
            Add(mtrrs, Game.TickCount, moveTypeRatio);
            Add(sms, Game.TickCount, speedMult);
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

            PointF target = Game.GetPoint(this.targetAngle, (float)( Game.MapSize / Math.Sqrt(Game.AlienShipSpeedMult) / this.speedMult ));
            float trgX = target.X, trgY = target.Y;
            float towardsPlayer;

            float distSqr = Game.GetDistanceSqr(x, y, trgX, trgY);
            if (distSqr < speed * speed)
            {
                //Add(dirs, Game.TickCount, new PointF(0, 0));
                //Add(spds, Game.TickCount, 0);
                //Add(actMovs, Game.TickCount, new PointF(trgX - x, trgY - y));
                Console.WriteLine("hold: " + target);
                towardsPlayer = Game.GetDistance(x, y) - Game.GetDistance(trgX, trgY);
                Console.WriteLine("towardsPlayer: " + towardsPlayer);
                Console.WriteLine("tick: " + Game.TickCount);
                //x = trgX;
                //y = trgY;
                //xDir = yDir = 0;
                //speed = float.Epsilon;
            }
            //else
            //{
            xDir = trgX - x;
            yDir = trgY - y;
            Game.NormalizeDirs(ref xDir, ref yDir, speed * moveTypeRatio);

            speed *= ( 1f - moveTypeRatio );
            if (speed < float.Epsilon)
                speed = float.Epsilon;

            float xMove, yMove;
            GetTotalMove(out xMove, out yMove);
            towardsPlayer = Game.GetDistance(x, y) - Game.GetDistance(x + xMove, y + yMove);

            Add(dirs, Game.TickCount, new PointF(xDir, yDir));
            Add(spds, Game.TickCount, speed);
            Add(actMovs, Game.TickCount, new PointF(xMove, yMove));
            //}

            Game.ShootAtPlayer(fireRate, ref coolDown, towardsPlayer, x, y, Size);
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

            if (ShouldDraw(x, y, size))
                Game.DrawHealthBar(graphics, this, GetLifePct());
        }
    }
}
