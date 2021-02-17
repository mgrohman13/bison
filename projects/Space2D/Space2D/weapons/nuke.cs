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
    class nuke : weapon
    {
        const int NUKE_SIZE = 130;

        static Texture texture;

        float scale, rotation, rotChange;
        float scaleInc = .13f, endScale = 26, curFrameInc;

        public nuke(float x, float y, bool friendly)
            : base(-1)
        {
            curFrame = 0;

            damage = 13;
            scale = 1f;

            this.pic = texture;
            this.size = new Size(NUKE_SIZE, NUKE_SIZE);
            this.width = 5;
            this.height = 5;
            this.x = x;
            this.y = y;
            this.friendly = friendly;

            stops = false;

            //animations only for increasing expansion
            animate = false;

            rotation = Data.Random.DoubleHalf((float)Math.PI * 4);

            //randomly rotate
            rotChange += Data.Random.Gaussian(.03f);

            //time between changes in texture
            curFrameInc = endScale / width / height;

            weapon.all.Add(this);
        }

        public override void collisions(enemies.enemy[] temp)
        {
            //destroy enemies
            for (int i = temp.Length ; --i > -1 ; )
            {
                enemies.enemy e = temp[i];
                if (( friendly != e.Friendly ))
                    DoCollision(e);
            }

            //also destroy weapons
            for (int i = all.Count ; --i > -1 ; )
            {
                weapon w;
                if (( friendly != ( w = all[i] ).Friendly ))
                    DoCollision(w);
            }
        }

        //collision for a circular nuke
        private void DoCollision(gameObject gO)
        {
            //int w, h;
            //float xDist, yDist;

            ////close enough to hit
            //if ((xDist = (float)Math.Sqrt((xDist = gO.rect.X - x) * xDist + (yDist = gO.rect.Y - y) * yDist)) <
            //    ((float)this.size.Height * scale / 2.1) + (Math.Sqrt((w = gO.rect.Width) * w + (h = gO.rect.Height) * h) / 2.1))
            //{

            this.radius = Data.Random.Round(NUKE_SIZE * scale / 2.3);
            if (Data.Collide(this, gO))
            {
                weapon weapon;
                explosionManager.createDamage(( ( weapon = ( gO as weapon ) ) == null ) ? damage : weapon.Damage, gO.X, gO.Y);

                if (weapon == null)
                    ( (enemies.enemy)gO ).hit(damage);
                else
                    weapon.all.Remove(weapon);
            }
        }

        public override void Inc()
        {
            //stay still
            y -= Data.scrollSpeed;

            //new frames for increasing expansion
            curFrame = (int)( scale / curFrameInc );

            if (( scale += scaleInc ) > endScale)
                weapon.all.Remove(this);
            else
            {
                //random rotation
                rotation += rotChange;
                rotChange += Data.Random.Gaussian(.01f);
            }
        }

        //draw at different scales
        public override void draw(Microsoft.DirectX.Direct3D.Sprite sprite)
        {
            //decrease the x and y coordinates so it doesn't draw in the wrong place
            float xOld = x, yOld = y;
            Vector3 xy = new Vector3(x, y, 0);
            xy.Scale(1f / scale);
            xy.TransformCoordinate(Matrix.RotationZ(-rotation));
            x = xy.X;
            y = xy.Y;

            //draw everything already in the buffer
            sprite.Flush();
            //transform the world with random rotation and proper scale
            mainForm.device.Transform.World = Matrix.RotationZ(rotation) * Matrix.Scaling(scale, scale, 1);
            base.draw(sprite);
            //draw the nuke
            sprite.Flush();
            //reset the world transform
            mainForm.device.Transform.World = Matrix.RotationZ(0) * Matrix.Scaling(1, 1, 1);

            //reset the x and y coordinates to normal
            x = xOld;
            y = yOld;
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\nuke0.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}