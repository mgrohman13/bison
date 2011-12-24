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
    class pellet : weapon
    {
        const float speed = 13f;

        static Texture texture;

        Vector3 heading;

        public pellet(float x, float y, bool friendly, Vector3 heading)
            : base(3)
        {
            damage = Data.Random.GaussianCappedInt(2.1f, 1f);

            curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(5, 5);
            this.x = x;
            this.y = y;
            this.friendly = friendly;

            //no need to animate when there's only one frame
            animate = false;

            weapon.all.Add(this);

            //keep speed consistant
            heading.Normalize();
            heading.Scale(speed);
            this.heading = heading;
        }

        public override void Inc()
        {
            heading.Scale(Data.Random.GaussianCapped(1, .013f, .87f));

            //remove at screen edges
            if (( y += heading.Y ) > Data.Height || y < 0 || ( x += heading.X ) > Data.Width || x < 0)
                weapon.all.Remove(this);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\pellet.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.White.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}