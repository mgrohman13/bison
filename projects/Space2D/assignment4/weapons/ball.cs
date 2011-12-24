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

namespace assignment4.weapons
{
    class ball : weapon
    {
        const float speed = 13f;

        static Texture texture;

        Vector3 heading;
        float rotation;

        public ball(float x, float y, Vector3 heading, bool friendly)
            : base(7)
        {
            curFrame = 0;

            damage = Data.Random.OEInt(16.9);

            this.pic = texture;
            this.size = new Size(15, 15);
            this.width = 6;
            this.height = 6;
            this.x = x;
            this.y = y;
            this.friendly = friendly;

            //keep speed constant
            heading.Normalize();
            heading.Scale(speed);

            this.heading = heading;
            rotation = 0;

            weapon.all.Add(this);
        }

        public override void Inc()
        {
            //randomly speed up/slow down
            heading.Scale(Data.Random.GaussianCapped(1, .06f, .87f));

            //randomly rotate
            rotation += Data.Random.Gaussian(.00666f) / speed;
            rotation -= rotation / 169f;
            heading.TransformCoordinate(Matrix.RotationZ(rotation));

            if (( y += heading.Y ) > Data.Height || y < 0 || ( x += heading.X ) < 0 || x > Data.Width)
                weapon.all.Remove(this);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\ball.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}