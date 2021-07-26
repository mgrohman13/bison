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
    class spinner : enemy
    {
        float reloadTime, rotateSpeed;
        int reload;
        float rotate;

        static Texture texture;

        float xvel, yvel;
        bool spinDir;

        public spinner()
            : base(30)
        {
            if (Data.Random.Bool())
            {
                reloadTime = 13;
                rotateSpeed = .013f;
            }
            else
            {
                reloadTime = 16.9f;
                rotateSpeed = .169f;
            }

            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(39, .3f, 21));
            hits = maxHits;

            this.curFrame = 0;
            this.height = 6;
            this.pic = texture;
            this.size = new Size(60, 60);
            this.width = 6;
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 1.3f / Data.hitMult);

            xvel = 0;
            yvel = 0;

            rotate = Data.Random.DoubleHalf(4 * (float)Math.PI);

            enemy.all.Add(this);

            reload = Data.Random.GaussianCappedInt(reloadTime, .3f, 3);
            spinDir = Data.Random.Bool();
        }

        public override void Inc()
        {
            //randomly drift around
            const float speed = .39f;
            if (( x += ( xvel += Data.Random.Gaussian(speed) ) ) > Data.Width - size.Width / 2)
            {
                x = Data.Width - size.Width / 2;
                xvel = 0;
            }
            else if (x < 0 + size.Width / 2)
            {
                x = 0 + size.Width / 2;
                xvel = 0;
            }
            if (( y += ( yvel += Data.Random.Gaussian(speed) ) - Data.scrollSpeed ) > Data.Height - size.Height / 2)
            {
                y = Data.Height - size.Height / 2;
                yvel = 0;
            }
            else if (friendly)
            {
                if (y < 0 + size.Height / 2)
                {
                    y = 0 + size.Height / 2;
                    yvel = 0;
                }
            }
            else
            {
                if (y < 0 - size.Height / 2)
                {
                    y = 0 - size.Height / 2;
                    yvel = 0;
                }
            }

            if (--reload < 1)
            {
                fire();
                reload = Data.Random.GaussianCappedInt(reloadTime, .3f, 3);
            }

            //rotate firing direction
            if (spinDir)
            {
                rotate -= Data.Random.Gaussian(rotateSpeed, .013f);
                if (rotate < Math.PI * 2f)
                    rotate -= (float)Math.PI * 2;
            }
            else
            {
                rotate += Data.Random.Gaussian(rotateSpeed, .013f);
                if (rotate > -Math.PI * 2f)
                    rotate += (float)Math.PI * 2;
            }
        }

        private void fire()
        {
            Vector3 vel = new Vector3(1, 0, 0);
            vel.TransformCoordinate(Matrix.RotationZ(rotate));
            new weapons.ball(x, y, vel, friendly);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\ball.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}