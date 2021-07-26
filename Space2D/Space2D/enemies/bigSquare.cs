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
    class bigSquare : enemy
    {
        int reloadTime;
        int reload;

        static Texture texture;

        public bigSquare()
            : base(40)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(390, .1f, 260));
            hits = maxHits;

            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(75, 85);
            this.x = Data.Random.Next(Data.Width);
            this.y = size.Height / 2;

            score = Data.Random.Round(hits / 21f / Data.hitMult);

            enemy.all.Add(this);

            reloadTime = Data.Random.Round(Data.Height / Data.scrollSpeed / 13f);
            reload = reloadTime;
        }

        public override bool Capture()
        {
            score = Data.Random.Round(score / 6.66f);
            return base.Capture();
        }

        public override void Inc()
        {
            if (y > size.Height / 2 + Data.Height)
                if (friendly)
                {
                    //the enemy.Remove() method automatically takes away from your score
                    //if you have captured it, insead we want to add to your score
                    Data.score += Data.Random.Round(score * hits / (float)maxHits);
                    enemy.all.Remove(this);
                }
                else
                    enemy.Remove(this);

            if (--reload < 3)
            {
                fire();
                reload = reloadTime;
            }
        }

        private void fire()
        {
            new enemies.squareWeapon(x, y, friendly);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\bigSquare.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}