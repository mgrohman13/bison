using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
    internal abstract class GameObject
    {
        protected readonly Game Game;

        protected Image image;
        protected float x, y;
        protected float speed;
        protected float size;
        protected float xDir, yDir;
        protected float curAngle;
        protected float rotate;

        protected GameObject(Game game, float x, float y, float size, Image image) :
            this(game, x, y, 0, 0, 0, size, image, float.NaN)
        {
        }

        protected GameObject(Game game, float x, float y, float size, Image image, float rotateSpeed) :
            this(game, x, y, 0, 0, 0, size, image, rotateSpeed)
        {
        }

        protected GameObject(Game game, float x, float y, float xDir, float yDir, float size, Image image) :
            this(game, x, y, xDir, yDir, 0, size, image, float.NaN)
        {
        }

        protected GameObject(Game game, float x, float y, float xDir, float yDir, float size, Image image, float rotateSpeed) :
            this(game, x, y, xDir, yDir, 0, size, image, rotateSpeed)
        {
        }

        protected GameObject(Game game, float x, float y, float speed, float size, Image image, float rotateSpeed) :
            this(game, x, y, 0, 0, speed, size, image, rotateSpeed)
        {
        }

        private GameObject(float xDir, float yDir, float size)
            : this(0, 0, xDir, yDir, size)
        {
        }

        private GameObject(float x, float y, float xDir, float yDir, float size)
            : this(null, x, y, xDir, yDir, 0, size, null, float.NaN, false)
        {
        }

        private GameObject(Game game, float x, float y, float xDir, float yDir, float speed, float size, Image image, float rotateSpeed)
            : this(game, x, y, xDir, yDir, speed, size, image, rotateSpeed, true)
        {
        }

        private GameObject(Game game, float x, float y, float xDir, float yDir, float speed, float size, Image image, float rotateSpeed, bool add)
        {
            this.Game = game;

            this.x = x;
            this.y = y;
            this.xDir = xDir;
            this.yDir = yDir;
            this.speed = speed;
            this.size = size;
            this.image = image;

            if (float.IsNaN(rotateSpeed))
            {
                curAngle = float.NaN;
            }
            else
            {
                curAngle = Game.GetImageAngle();
                if (rotateSpeed != 0)
                    rotate = Game.Random.Gaussian(rotateSpeed);
            }

            if (add)
                game.AddObject(this);
        }

        internal float X
        {
            get
            {
                return x;
            }
        }

        internal float Y
        {
            get
            {
                return y;
            }
        }

        internal float XDir
        {
            get
            {
                return xDir;
            }
        }

        internal float YDir
        {
            get
            {
                return yDir;
            }
        }

        internal float Speed
        {
            get
            {
                return speed;
            }
        }

        internal float Size
        {
            get
            {
                return size;
            }
        }

        internal float Area
        {
            get
            {
                return Game.GetArea(this.Size);
            }
        }

        internal abstract decimal Score
        {
            get;
        }

        internal virtual void Draw(Graphics graphics, int centerX, int centerY)
        {
#if TRACE
            graphics.DrawEllipse(Pens.White, centerX + x - size, centerY + y - size, size * 2 - 1, size * 2 - 1);
#endif
            DrawImage(graphics, image, centerX, centerY, speed, x, y, Size, curAngle);
        }

        internal static void DrawImage(Graphics graphics, Image image, int centerX, int centerY, float speed, float x, float y, float size, float curAngle)
        {
            if (ShouldDraw(x, y, size))
                lock (image)
                {
#if DEBUG
                    if (image.Width != image.Height)
                        throw new Exception();
#endif
                    float objectX = centerX + x;
                    float objectY = centerY + y;

                    if (speed > 0)
                        curAngle = Game.GetAngleImageAdjusted(-x, -y);

                    if (!float.IsNaN(curAngle))
                    {
                        graphics.TranslateTransform(objectX, objectY);
                        graphics.RotateTransform(curAngle * Game.RadToDeg);
                        graphics.TranslateTransform(-objectX, -objectY);
                    }

                    float offset = image.Width / 2f;
                    graphics.DrawImage(image, objectX - offset, objectY - offset);

                    if (!float.IsNaN(curAngle))
                    {
                        graphics.Transform.Dispose();
                        graphics.ResetTransform();
                    }
                }
        }

        protected static bool ShouldDraw(float x, float y, float size)
        {
            return ( Game.GetDistance(x, y) - size < Game.MapSize );
        }

        protected virtual void OnStep()
        {
        }

        internal void Step(float playerXMove, float playerYMove)
        {
            if (!float.IsNaN(curAngle))
                curAngle += rotate;

            float xMove, yMove;
            GetTotalMove(out xMove, out yMove);
            Move(xMove - playerXMove, yMove - playerYMove);

            float dist = Game.GetDistance(x, y) - Size, checkDist = dist - Game.CreationDist;
            if (checkDist > 0 && Game.GameRand.Bool((float)( 1.0 - Math.Pow(1.0 - checkDist / ( checkDist + Game.RemovalDist ), Game.TotalSpeed) )))
            {
                //Game.PrintObjInfo(this, true);
                Game.RemoveObject(this);
            }
            else if (dist < Game.PlayerSize)
                HitPlayer();

            OnStep();
        }

        internal void GetTotalMove(out float xMove, out float yMove)
        {
            //move towards the player
            xMove = -x;
            yMove = -y;
            Game.NormalizeDirs(ref xMove, ref yMove, speed);

            //directional object movement
            xMove += xDir;
            yMove += yDir;
        }

        protected virtual void HitPlayer()
        {
            Game.AddScore(Score);
            Game.RemoveObject(this);
        }

        internal virtual void Die()
        {
            AddScore(Score);
            Game.RemoveObject(this);
        }

        protected void AddScore(decimal score)
        {
            //only add score if the center of the object is within the visible portion of the map
            if (score != 0m && Game.GetDistanceSqr(x, y) < Game.MapSize * Game.MapSize)
                Game.AddScore(score);
        }

        protected void BumpCollision(GameObject obj)
        {
            BumpCollision(obj, true);
        }

        protected void BumpCollision(GameObject obj, bool adjustOther)
        {
            float moveDist = ( Size + obj.Size - Game.GetDistance(x, y, obj.x, obj.y) ) / ( adjustOther ? 2 : 1 );
            float xDif = x - obj.x, yDif = y - obj.y;
            Game.NormalizeDirs(ref xDif, ref yDif, moveDist);

            Move(xDif, yDif);
            if (adjustOther)
                obj.Move(-xDif, -yDif);
        }

        internal void Move(float xDir, float yDir)
        {
            x += xDir;
            y += yDir;
        }

        //actual collision logic
        protected abstract void Collide(GameObject obj);

        internal bool CheckCollision(GameObject obj)
        {
            float sizes = this.Size + obj.Size;
            if (Game.GetDistanceSqr(this.x, this.y, obj.x, obj.y) < sizes * sizes)
            {
                Collide(this, obj);
                return true;
            }
            return false;
        }

        private static void Collide(GameObject obj1, GameObject obj2)
        {
            //run the correct object's Collide method
            if (GetCollidePriority(obj2) > GetCollidePriority(obj1))
                obj2.Collide(obj1);
            else
                obj1.Collide(obj2);
        }

        private static int GetCollidePriority(GameObject obj)
        {
            //determines which object's Collide method is called when two objects collide
            //the object with the higher return value has its method called
            if (obj is LifeDust)
                return 3;
            if (obj is Bullet)
                return 4;
            if (obj is Explosion)
                return 8;
            if (obj is FuelExplosion)
                return 6;
            if (obj is Asteroid)
                return 2;
            if (obj is Alien)
                return 5;
            if (obj is PowerUp)
                return 1;
            if (obj is AlienShip)
                return 7;
#if DEBUG
            throw new Exception();
#else
            return 0;
#endif
        }

        internal class DummyObject : GameObject
        {
            internal DummyObject(float x, float y, float xDir, float yDir)
                : base(x, y, xDir, yDir, 0f)
            {
            }
            internal DummyObject(float xDir, float yDir, float size)
                : base(xDir, yDir, size)
            {
            }
            internal override decimal Score
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            protected override void Collide(GameObject obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
