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
    class pellets : powerUp
    {
        int strings;
        float relVal;
        static Texture texture;

        int ammo;
        float[] angle, speed;
        int[] reload;

        public pellets()
            : base(15)
        {
            this.curFrame = 0;
            this.width = 1;
            this.height = 1;
            this.pic = texture;
            this.size = new Size(25, 25);
            this.x = Data.Random.Next(Data.Width);
            this.y = -size.Height / 2;

            ammo = Data.Random.GaussianCappedInt(6000, .13f, 3000);

            powerUp.all.Add(this);

            //number of different directions to fire at once
            strings = Data.Random.Next(1, 6);//3
            relVal = (float)Math.Sqrt(strings + 2.6) * 1.3f;

            angle = new float[strings];
            speed = new float[strings];
            reload = new int[strings];

            for (int a = -1 ; ++a < strings ; )
            {
                //angles and speed of rotations for each string
                angle[a] = Data.Random.DoubleHalf((float)Math.PI * 4f);
                speed[a] = Data.Random.Gaussian(.01f);
                reload[a] = a * 13;
            }
        }

        public override void Inc()
        {
            if (collected)
            {
                for (int a = -1 ; ++a < strings ; )
                {
                    if (--reload[a] < 1)
                    {
                        //fire in the appropriate direction
                        Vector3 vel = new Vector3(1, 0, 0);
                        vel.TransformCoordinate(Matrix.RotationZ(angle[a] += speed[a]));
                        new weapons.pellet(Data.player.X, Data.player.Y, true, vel);

                        //remove when out of ammo
                        if (--ammo < 1)
                            powerUp.Remove(this);
                        else
                            reload[a] = Data.Random.GaussianCappedInt(relVal, .21f, 1);
                    }
                }
            }
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\pellets.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}