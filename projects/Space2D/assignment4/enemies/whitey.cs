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
    class whitey : enemy
    {
        const float speed = 3;
        const int reloadTime = 130;
        int reload;

        static Texture texture;

        int dir;

        public whitey()
            : base(21)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(60, .087f, 39));
            hits = maxHits;

            this.curFrame = 0;
            this.width = 3;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(43, 55);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 10f / Data.hitMult);

            enemy.all.Add(this);

            animate = false;
            reload = Data.Random.Next(reloadTime);

            dir = Data.Random.Next(3);
            FixFrame();
        }

        private void FixFrame()
        {
            //show proper sprite for direction
            if (dir < 1)
                curFrame = 0;
            else if (friendly)
            {
                if (dir < 2)
                    curFrame = 1;
                else
                    curFrame = 2;
            }
            else
            {
                if (dir < 2)
                    curFrame = 2;
                else
                    curFrame = 1;
            }
        }

        public override void Inc()
        {
            if (Data.Random.Bool(.00333f))
            {
                dir = Data.Random.Next(3);
                FixFrame();
            }

            if (dir == 1)
                x += speed;
            else if (dir == 2)
                x -= speed;

            if (x > Data.Width - size.Width / 2)
            {
                x = Data.Width - size.Width / 2;
                dir = 2;
                FixFrame();
            }
            else if (x < 0 + size.Width / 2)
            {
                x = 0 + size.Width / 2;
                dir = 1;
                FixFrame();
            }

            if (friendly)
                y -= Data.scrollSpeed;
            else
                y += .9f;

            //remove when it goes off the bottom
            if (y > size.Height / 2 + Data.Height)
                enemy.Remove(this);

            if (--reload < 0)
            {
                fire();
                reload = reloadTime;
            }
        }

        private void fire()
        {
            new weapons.bomb(x, y, friendly, new Vector3(0, friendly ? -1 : 1, 0));
        }

        public override void draw(Sprite sprite)
        {
            if (friendly)
                DrawRotate(sprite, new Vector3(0, 1, 0));
            else
                base.draw(sprite);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\whitey.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}