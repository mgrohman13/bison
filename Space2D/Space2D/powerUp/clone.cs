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

namespace assignment4.powerUp
{
    class clone : powerUp
    {
        static Texture texture;

        public clone()
            : base(22)
        {
            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(36, 36);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            powerUp.all.Add(this);
        }

        public override void Inc()
        {
            if (collected)
            {
                //immediately create clones and remove the power up
                int amt = Data.Random.Round(1.3f + Data.Random.OE(3));
                for (int i = -1 ; ++i < amt ; )
                    new player();

                powerUp.Remove(this);
            }
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\clone.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }
        new public static void disposeTextures()
        {
            texture.Dispose();
        }

    }
}