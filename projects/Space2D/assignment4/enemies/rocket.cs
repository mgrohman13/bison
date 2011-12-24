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
    class rocket : enemy
    {
        static Texture texture;

        Vector3 heading;

        //the direction the rocket is rotation
        float rotation;

        public rocket()
            : base(36)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCappedInt(30, .3f, 13));
            hits = maxHits;

            heading = new Vector3(0, 3f + Data.Random.OE(6), 0);

            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(113, 113);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            rotation = 0;

            score = Data.Random.Round(hits / 16f / Data.hitMult);

            enemy.all.Add(this);
        }

        public override void Inc()
        {
            if (friendly)
                hits += Data.Random.Round(Data.hitMult / hits);

            //randomize the rotation and rotate the rocket
            rotation += Data.Random.Gaussian(.0013f) * heading.Length();
            rotation -= rotation / 100f;
            heading.TransformCoordinate(Matrix.RotationZ(rotation));

            const float offRotationMult = 0.013f;
            if (( x += heading.X ) > Data.Width)
                heading = Data.rotateTowards(heading, new Vector3(-1, 0, 0), offRotationMult * heading.Length());
            else if (x < 0)
                heading = Data.rotateTowards(heading, new Vector3(1, 0, 0), offRotationMult * heading.Length());
            if (( y += heading.Y ) < 0)
                heading = Data.rotateTowards(heading, new Vector3(0, 1, 0), offRotationMult * heading.Length());
            else if (y > Data.Height + size.Height / 2)
                if (friendly)
                    heading = Data.rotateTowards(heading, new Vector3(0, -1, 0), offRotationMult * heading.Length());
                else
                    enemy.Remove(this);
        }

        public override void draw(Microsoft.DirectX.Direct3D.Sprite sprite)
        {
            DrawRotate(sprite, heading);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\rocket.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}