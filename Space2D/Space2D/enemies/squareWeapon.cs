using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DInput = Microsoft.DirectX.DirectInput;
using dSound = Microsoft.DirectX.DirectSound;

namespace assignment4.enemies
{
    class squareWeapon : enemy
    {
        float xVel, yVel;

        static Texture texture;

        public squareWeapon()
            : base(20)
        {
            Vector3 vel = getVel();
            Init(Data.Random.Next(Data.Width), Data.Random.Next(Data.Height), vel.X, vel.Y, false);
        }

        public squareWeapon(float x, float y, bool friendly)
            : base(20)
        {
            Vector3 vel = getVel();
            Init(x, y, vel.X, vel.Y, friendly);
        }

        public override bool Capture()
        {
            //dont capture it
            return false;
        }

        private Vector3 getVel()
        {
            float angle = Data.Random.DoubleHalf(4 * (float)Math.PI);
            Vector3 vel = new Vector3(Data.Random.OE(5), 0, 0);
            vel.TransformCoordinate(Matrix.RotationZ(angle));
            return vel;
        }

        public squareWeapon(float x, float y, float xVel, float yvel, bool friendly)
            : base(20)
        {
            Init(x, y, xVel, yvel, friendly);
        }

        void Init(float x, float y, float xVel, float yvel, bool friendly)
        {
            this.friendly = friendly;

            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(( friendly ? 78 : 30 ), .1f, 21));
            hits = maxHits;

            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(39, 39);
            this.x = x;
            this.y = y;
            this.xVel = xVel;
            this.yVel = yvel;

            animate = false;

            score = Data.Random.Round(hits / 169f / Data.hitMult);

            enemy.all.Add(this);
        }

        public override void Inc()
        {
            //bounce off walls
            if (( x += xVel ) < size.Width / 2)
            {
                xVel = -xVel;
                x = size.Width / 2;
            }
            else if (x > Data.Width - size.Width / 2)
            {
                xVel = -xVel;
                x = Data.Width - size.Width / 2;
            }
            if (( y += yVel - Data.scrollSpeed ) < size.Height / 2)
            {
                yVel = -yVel;
                y = size.Height / 2;
            }
            else if (y > Data.Height - size.Height / 2)
            {
                yVel = -yVel;
                y = Data.Height - size.Height / 2;
            }
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\squareWeapon.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}