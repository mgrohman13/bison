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
    class laserShip : enemy
    {
        static Texture texture;

        Vector3 heading;

        const float reloadTime = 39;
        const float speed = 3.9f;
        int reload;

        float rotation;

        public laserShip()
            : base(28)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(60, .26f, 30));
            hits = maxHits;

            heading = new Vector3(0, speed, 0);

            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(52, 58);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 6.66f / Data.hitMult);

            enemy.all.Add(this);

            animate = false;

            //fire at random intervals
            reload = Data.Random.OEInt(reloadTime);

            rotation = 0;
        }

        public override void Inc()
        {
            //random rotation
            rotation += (float)Data.Random.Gaussian(.00087f) * heading.Length();
            rotation -= rotation / 130f;
            heading.TransformCoordinate(Matrix.RotationZ(rotation));

            const float offRotationAmt = 0.013f * speed;
            if (( x += heading.X ) > Data.Width)
                heading = Data.rotateTowards(heading, new Vector3(-1, 0, 0), offRotationAmt);
            else if (x < 0)
                heading = Data.rotateTowards(heading, new Vector3(1, 0, 0), offRotationAmt);
            if (( y += heading.Y ) < 0)
                heading = Data.rotateTowards(heading, new Vector3(0, 1, 0), offRotationAmt);
            else if (y > Data.Height + size.Height / 2)
                if (friendly) //if it's friendly, dont remove it when it reaches the bottom
                    heading = Data.rotateTowards(heading, new Vector3(0, -1, 0), offRotationAmt);
                else
                    enemy.Remove(this);

            if (--reload < 0)
            {
                fire();
                //fire at random intervals
                reload = Data.Random.OEInt(reloadTime);
            }
        }

        private void fire()
        {
            new weapons.regLaser(x, y, friendly, heading);
        }

        public override void draw(Sprite sprite)
        {
            DrawRotate(sprite, heading);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\pixelship_1-1.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}