using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    public abstract class GameObject
    {
        protected Image image;
        protected float x, y;
        protected float speed;
        protected float size;
        protected float xDir, yDir;
        protected float curAngle;
        protected float rotate;

        public float X
        {
            get
            {
                return x;
            }
        }
        public float Y
        {
            get
            {
                return y;
            }
        }
        public float Size
        {
            get
            {
                return size;
            }
        }

        //score stuff
        public abstract decimal Score
        {
            get;
        }

        //constructor overloads for easy use
        protected GameObject(float x, float y, float size, Image image)
        {
            Init(x, y, 0, 0, 0, size, image, 0);
        }
        protected GameObject(float x, float y, float size, Image image, float rotateSpeed)
        {
            Init(x, y, 0, 0, speed, size, image, rotateSpeed);
        }
        protected GameObject(float x, float y, float xDir, float yDir, float size, Image image)
        {
            Init(x, y, xDir, yDir, 0, size, image, 0);
        }
        protected GameObject(float x, float y, float xDir, float yDir, float size, Image image, float rotateSpeed)
        {
            Init(x, y, xDir, yDir, 0, size, image, rotateSpeed);
        }
        protected GameObject(float x, float y, float speed, float size, Image image)
        {
            Init(x, y, 0, 0, speed, size, image, 0);
        }
        protected GameObject(float x, float y, float speed, float size, Image image, float rotateSpeed)
        {
            Init(x, y, 0, 0, speed, size, image, rotateSpeed);
        }
        protected GameObject(float x, float y, float xDir, float yDir, float speed, float size, Image image)
        {
            Init(x, y, xDir, yDir, speed, size, image, 0);
        }
        void Init(float x, float y, float xDir, float yDir, float speed, float size, Image image, float rotateSpeed)
        {
            this.x = x;
            this.y = y;
            this.xDir = xDir;
            this.yDir = yDir;
            this.speed = speed;
            this.size = size;
            this.image = image;

            curAngle = Game.Random.DoubleHalf(360);
            rotate = Game.Random.Gaussian(Game.GameSpeed * ( rotateSpeed + .3f ));

            Forms.GameForm.Game.AddObject(this);
        }

        public virtual void Draw(Graphics graphics, int centerX, int centerY)
        {
            float objectX = ( centerX + x ), objectY = ( centerY + y );
            if (speed > 0)
                Game.Rotate(graphics, -x, -y, objectX, objectY);
            else
                Game.Rotate(graphics, curAngle, objectX, objectY);
            graphics.DrawImageUnscaled(image, Game.Round(objectX - image.Width / 2f), Game.Round(objectY - image.Height / 2f));

#if TRACE
			graphics.ResetTransform();
			graphics.DrawEllipse(Pens.White, centerX + x - size, centerY + y - size, size * 2, size * 2);
#endif
        }

        protected virtual void OnStep()
        {
        }

        public float Step(float playerXMove, float playerYMove)
        {
            const float EdgeDistSqr = ( Game.MapRadius + Game.ExtraMapDistance ) * ( Game.MapRadius + Game.ExtraMapDistance );

            //constant rotation
            curAngle += rotate;

            //move the object to simulate player movement
            x -= playerXMove;
            y -= playerYMove;

            //directional object movement
            x += xDir;
            y += yDir;

            //move towards the player
            if (speed > 0)
                MoveTowards(-x, -y, speed);

            //do stuff in child classes
            OnStep();

            //check for player hit
            float distSqr, checkDist, damage = 0;
            if (( distSqr = Game.GetDistanceSqr(x, y) ) < ( checkDist = Game.PlayerSize + size ) * ( checkDist ))
            {
                damage = HitPlayer();
            }
            //check game edge
            else if (distSqr > EdgeDistSqr)
            {
                Forms.GameForm.Game.RemoveObject(this);
            }

            //return damage to player
            return damage;
        }

        protected virtual float HitPlayer()
        {
            Forms.GameForm.Game.AddScore(Score);
            Forms.GameForm.Game.RemoveObject(this);
            //defualt to always killing player
            return Game.PlayerLife;
        }

        public virtual void Die()
        {
            Forms.GameForm.Game.AddScore(Score);
            Forms.GameForm.Game.RemoveObject(this);
        }

        protected void BumpCollision(GameObject obj)
        {
            BumpCollision(obj, true);
        }
        protected void BumpCollision(GameObject obj, bool adjustOther)
        {
            float moveDist = ( size + obj.size - Game.GetDistance(x, y, obj.x, obj.y) ) / ( adjustOther ? 2f : 1f );
            float xDif = x - obj.x, yDif = y - obj.y;
            if (xDif == 0 && yDif == 0)
            {
                float angle = Game.Random.DoubleFull((float)Math.PI);
                xDif = (float)Math.Cos(angle);
                yDif = (float)Math.Sin(angle);
            }
            MoveTowards(xDif, yDif, moveDist);
            if (adjustOther)
                obj.MoveTowards(-xDif, -yDif, moveDist);
        }

        void MoveTowards(float xDir, float yDir, float moveDist)
        {
            Game.NormalizeDirs(ref xDir, ref yDir, moveDist);
            x += xDir;
            y += yDir;
        }

        //actual collision logic
        protected abstract void Collide(GameObject obj);

        public void CheckCollision(GameObject obj)
        {
            float sizes;
            //check distance for collision
            if (Game.GetDistanceSqr(x, y, obj.x, obj.y) <
                ( sizes = size + obj.size ) * sizes)
                Collide(this, obj);
        }
        static void Collide(GameObject obj1, GameObject obj2)
        {
            //run the correct objects Collide method
            if (TypePriority(obj2) > TypePriority(obj1))
                obj2.Collide(obj1);
            else
                obj1.Collide(obj2);
        }
        static int TypePriority(GameObject obj)
        {
            //determines which objects Collide method is called when two objects collide
            //the object with the higher return value has its method called

            if (obj is PowerUp)
                return 10;
            if (obj is Asteroid)
                return 20;
            if (obj is LifeDust)
                return 25;
            if (obj is Alien)
                return 30;
            if (obj is Bullet)
                return 50;
            if (obj is FuelExplosion)
                return 60;
            if (obj is AlienShip)
                return 70;
            if (obj is Explosion)
                return 80;
            //if (obj is Mine)
            //    return 90;

            throw new Exception();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1},{2})", GetType(), x, y);
        }
    }
}
