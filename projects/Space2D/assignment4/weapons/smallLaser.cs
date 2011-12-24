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
    class regLaser : weapon
    {
        const float speed = 9f;

        static Texture texture;

        Vector3 heading;

        public regLaser(float x, float y, bool friendly, Vector3 heading)
            : base(5)
        {
            width = 3;
            height = 1;

            //keep speed consistant
            heading.Normalize();
            heading.Scale(speed);

            InitStuff(x, y, friendly, heading);
        }

        public regLaser(float x, float y, bool friendly)
            : base(5)
        {
            width = 3;
            height = 1;

            //automatically set heading based on friendliness
            InitStuff(x, y, friendly, new Vector3(0, friendly ? -speed : speed, 0));
        }

        void InitStuff(float x, float y, bool friendly, Vector3 heading)
        {
            damage = Data.Random.GaussianCappedInt(13, .06f, 6);

            this.pic = texture;
            this.size = new Size(6, 12);
            this.x = x;
            this.y = y;
            this.friendly = friendly;

            curFrame = Data.Random.Next(width);
            this.animate = false;

            weapon.all.Add(this);
            this.heading = heading;
        }

        public override void Inc()
        {
            //remove at screen edges
            if (( y += heading.Y ) > Data.Height || y < 0 || ( x += heading.X ) > Data.Width || x < 0)
                weapon.all.Remove(this);
        }

        public override void draw(Sprite sprite)
        {
            DrawRotate(sprite, heading);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\6x12 blue laser.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.White.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}