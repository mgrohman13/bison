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

namespace assignment4.enemies.skull
{
    class fast : Skull
    {
        static Texture texture;

        public fast()
            : base(33)
        {
            maxHits = Data.Random.Round(Data.hitMult * ( Data.Random.GaussianCapped(39, .21f, 16.9f) ));
            hits = maxHits;
            this.size = new Size(64, 48);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 5f / Data.hitMult);
            this.pic = texture;
            reload = Data.Random.RangeInt(0, ReloadTime);
        }

        protected override int ReloadTime
        {
            get
            {
                return Data.Random.GaussianCappedInt(30 * Skull.mult, .26f, 13);
            }
        }

        protected override float ySpeed
        {
            get
            {
                return 1.3f;
            }
        }

        protected override float Damage
        {
            get
            {
                return Data.Random.GaussianCapped(30, .169f, 21);
            }
        }

        protected override float getXVel()
        {
            return Data.Random.Gaussian(.21f);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\skulls\\fast.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.White.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}
