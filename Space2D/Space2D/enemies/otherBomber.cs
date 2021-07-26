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
    class otherBomber : enemy
    {
        static Texture texture;

        Vector3 heading;

        const float rotation = .013f;
        const float speed = 3f;
        const int reloadTime = 169;
        int reload;

        int dir;

        public otherBomber()
            : base(39)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(50, .05f, 39));
            hits = maxHits;

            heading = new Vector3(0, speed, 0);

            this.curFrame = 0;
            this.width = 3;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(64, 64);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 13f / Data.hitMult);

            enemy.all.Add(this);

            animate = false;
            reload = Data.Random.Next(reloadTime);

            dir = Data.Random.Bool() ? 1 : 2;
            FixFrame();
        }

        private void FixFrame()
        {
            curFrame = dir;
            if (curFrame == 4)
                curFrame = 0;
        }

        public override void Inc()
        {
            if (Data.Random.Bool(.0039f))
            {
                dir = Data.Random.Next(4);
                FixFrame();
            }

            if (dir == 1)
                heading.TransformCoordinate(Matrix.RotationZ(-rotation));
            else if (dir == 2)
                heading.TransformCoordinate(Matrix.RotationZ(rotation));

            const float offRotationAmt = 0.013f * speed;
            //if off the edge rotate back towards the screen area
            if (( x += heading.X ) > Data.Width)
                heading = Data.rotateTowards(heading, new Vector3(-1, 0, 0), offRotationAmt);
            else if (x < 0)
                heading = Data.rotateTowards(heading, new Vector3(1, 0, 0), offRotationAmt);
            if (( y += heading.Y ) < 0)
                heading = Data.rotateTowards(heading, new Vector3(0, 1, 0), offRotationAmt);
            else if (y > Data.Height)
                if (friendly)
                    heading = Data.rotateTowards(heading, new Vector3(0, -1, 0), offRotationAmt);
                else
                    enemy.Remove(this);

            if (--reload < 1)
            {
                fire();
                reload = reloadTime;
            }
        }

        private void fire()
        {
            new weapons.bomb(x, y, friendly, heading);
        }

        public override void draw(Sprite sprite)
        {
            DrawRotate(sprite, heading);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\otherBomber.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}