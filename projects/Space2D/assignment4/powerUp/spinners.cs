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
    class spinners : powerUp
    {
        static Texture texture;

        const int reloadTime = 6;
        int ammo, reload;

        public spinners()
            : base(18)
        {
            this.curFrame = 0;
            this.width = 6;
            this.height = 6;
            this.pic = texture;
            this.size = new Size(30, 30);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            ammo = Data.Random.OEInt(90);

            reload = reloadTime;

            powerUp.all.Add(this);
        }

        public override void Inc()
        {
            if (collected)
            {
                //fire when space is pressed
                KeyboardState state;
                if (--reload < 0 && !Data.player.Dead && ( state = mainForm.state )[Key.Space])
                {
                    //fire in a random direction
                    Vector3 vel = new Vector3(1, 0, 0);
                    vel.TransformCoordinate(Matrix.RotationZ(Data.Random.DoubleHalf((float)Math.PI * 4f)));
                    new weapons.ball(Data.player.X, Data.player.Y, vel, true);

                    //remove when out of ammo
                    if (--ammo < 1)
                        powerUp.Remove(this);
                    else
                        reload = reloadTime;
                }
            }
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\ball.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}