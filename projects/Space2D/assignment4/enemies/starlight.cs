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
    class starlight : enemy
    {
        static Texture texture;

        Vector3 heading;

        float rotation;
        int reloadLeft, reloadRight;
        const int reloadMin = 6, reloadMax = 21;//13

        public starlight()
            : base(33)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(21, .087f, 16.9f));
            hits = maxHits;

            heading = new Vector3(0, Data.Random.GaussianCapped(9, .3f, 6), 0);

            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(46, 52);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            rotation = 0;

            //fire left and right guns separately
            reloadLeft = getReload();
            reloadRight = getReload();

            score = Data.Random.Round(hits / 6.66f / Data.hitMult);

            enemy.all.Add(this);
        }

        int getReload()
        {
            return Data.Random.GaussianCappedInt(13, .39f, 6);
        }

        public override void Inc()
        {
            randTurn();

            const float offRotationMult = 0.013f;
            if (( x += heading.X ) > Data.Width)
                heading = Data.rotateTowards(heading, new Vector3(-1, 0, 0), offRotationMult * heading.Length());
            else if (x < 0)
                heading = Data.rotateTowards(heading, new Vector3(1, 0, 0), offRotationMult * heading.Length());
            if (( y += heading.Y ) < 0)
                heading = Data.rotateTowards(heading, new Vector3(0, 1, 0), offRotationMult * heading.Length());
            else if (y > Data.Height)
                //starlight never dissapears off bottom
                heading = Data.rotateTowards(heading, new Vector3(0, -1, 0), offRotationMult * heading.Length());

            //fire left and right guns separately
            if (--reloadRight < 0)
                fire(true);
            if (--reloadLeft < 0)
                fire(false);
        }

        private void fire(bool right)
        {
            if (right)
                reloadRight = getReload();
            else
                reloadLeft = getReload();

            //vector logic to get the right position for each gun
            Vector3 pos = new Vector3(x, y, 0), temp = heading;
            temp.Normalize();
            temp.Scale(10);
            pos.Add(temp);
            temp.TransformCoordinate(Matrix.RotationZ(( right ? .5f : -.5f ) * (float)Math.PI));
            temp.Scale(1.3f);
            pos.Add(temp);

            new weapons.pellet(pos.X, pos.Y, friendly, heading);
        }

        private void randTurn()
        {
            rotation += Data.Random.Gaussian(.001f) * heading.Length();
            rotation -= rotation / 130f;

            heading.TransformCoordinate(Matrix.RotationZ(rotation));
        }

        public override void draw(Microsoft.DirectX.Direct3D.Sprite sprite)
        {
            DrawRotate(sprite, heading);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\starlight.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}