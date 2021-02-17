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
    class bombs : powerUp
    {
        static Texture texture;

        public static bool used;
        public const int bombTime = 99;
        public static int time;
        int times;

        public bombs()
            : base(18)
        {
            this.curFrame = 0;
            this.width = 4;
            this.height = 3;
            this.pic = texture;
            this.size = new Size(30, 30);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            powerUp.all.Add(this);

            //number of bombs for each power up
            times = Data.Random.GaussianCappedInt(16.9f, .06f, 13); //18
        }

        public override void Inc()
        {
            if (collected && !used)
            {
                KeyboardState state;
                //fire when space bar is pressed
                if (!Data.player.Dead && ( state = mainForm.state )[Key.Space])
                {
                    used = true;
                    time = bombTime;

                    new weapons.bomb(Data.player.X, Data.player.Y, true, new Vector3(0, -1, 0));

                    //remove the power up when all bombs are thrown
                    if (--times < 1)
                        powerUp.Remove(this);
                }
            }
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\bombs.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}