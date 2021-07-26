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
    class bomb : weapon
    {
        const float speed = 6f;

        static Texture texture;

        Vector3 heading;

        public bomb(float x, float y, bool friendly, Vector3 heading)
            : base(6)
        {
            curFrame = 0;
            this.width = 4;
            this.height = 3;
            this.pic = texture;
            this.size = new Size(13, 13);
            this.x = x;
            this.y = y;
            this.friendly = friendly;

            weapon.all.Add(this);

            //keep speed constant
            heading.Normalize();
            heading.Scale(speed);
            this.heading = heading;
        }

        public override void Inc()
        {
            if (( y += heading.Y ) > Data.Height || y < 0 || ( x += heading.X ) > Data.Width || x < 0)
                Explode();
        }

        private void Explode()
        {
            //send out random particles
            int amt;
            Vector3 Heading = new Vector3(21, 0, 0);
            Heading.TransformCoordinate(Matrix.RotationZ(Data.Random.DoubleHalf((float)Math.PI * 4)));
            float div = (float)Math.PI * 2 / ( amt = Data.Random.OEInt(60) );
            for (int a = -1 ; ++a < amt ; )
            {
                Heading.TransformCoordinate(Matrix.RotationZ(div));
                float bulletX, buletY;
                if (( bulletX = x + Heading.X ) > 0 && bulletX < Data.Width && ( buletY = y + Heading.Y ) > 0 && buletY < Data.Height)
                    new weapons.pellet(bulletX, buletY, friendly, Heading);
            }

            weapon.all.Remove(this);
        }

        public override void collisions(enemies.enemy[] temp)
        {
            for (int i = temp.Length ; --i > -1 ; )
            {
                enemies.enemy e = temp[i];
                if (( friendly != e.Friendly ) && Data.Collide(this, e))
                    Explode();
            }
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\bomb.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.White.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}