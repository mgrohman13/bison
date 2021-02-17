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
    class laserBeam : powerUp
    {
        new public static void NewGame()
        {
            ammo = 0;
        }

        static Texture texture;

        const int reloadTime = 169;
        static int ammo;
        int reload;
        bool firstCollected = true;

        public laserBeam()
            : base(15)
        {
            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(24, 24);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            reload = reloadTime;

            powerUp.all.Add(this);
            animate = false;
        }

        public override void Inc()
        {
            if (collected)
            {
                if (firstCollected)
                {
                    firstCollected = false;

                    //if a beam has already been collected, no need to show the second one
                    if (ammo > 0)
                        powerUp.all.Remove(this);

                    ammo += GetAmmo();
                }

                //press b to 
                KeyboardState state;
                if (--reload < 0 && !Data.player.Dead && ( state = mainForm.state )[Key.B])
                {
                    new weapons.laserBeam(Data.player.X, true, Data.Random.GaussianCapped(150, .1f, 130), 0);

                    //remove when out of ammo
                    if (--ammo < 1)
                        powerUp.Remove(this);
                    else
                        reload = reloadTime;
                }
            }
        }

        //amount of ammo to add for each collected power up
        private int GetAmmo()
        {
            return Data.Random.GaussianCappedInt(6, .26f, 3);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\vLaserBeam.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}