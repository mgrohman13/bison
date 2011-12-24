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
    class large : enemy
    {
        const int reloadTime = 60;
        int reload;

        static Texture texture;

        float xvel;

        public large()
            : base(145)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(130, .21f, 87));
            hits = maxHits;

            this.curFrame = 0;
            this.width = 4;
            this.height = 4;
            this.pic = texture;
            this.size = new Size(274, 298);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 10f / Data.hitMult);

            reload = reloadTime;
            xvel = 0;

            enemy.all.Add(this);

            //animation only for when firing
            animate = false;
        }

        public override void Inc()
        {
            //random x movement
            if (( x += ( xvel += Data.Random.Gaussian(.021f) ) ) > Data.Width - size.Width / 2)
            {
                x = Data.Width - size.Width / 2;
                xvel = 0;
            }
            else if (x < 0 + size.Width / 2)
            {
                x = 0 + size.Width / 2;
                xvel = 0;
            }

            if (friendly)
                y -= Data.scrollSpeed;
            else
                y -= .3f;

            if (y > size.Height / 2 + Data.Height)
                enemy.Remove(this);

            if (--reload < 1)
            {
                fire();
                animate = true; //animate when firing
                reload = reloadTime;
            }
            else if (curFrame == 0)
                animate = false; //stop animation when done
        }

        private void fire()
        {
            new weapons.regLaser(x - 70, y, friendly);
            new weapons.regLaser(x + 70, y, friendly);
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
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\matts-ship.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.White.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}