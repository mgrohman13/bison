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
    class avg : Skull
    {
        static Texture texture;

        public avg()
            : base(33)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(39, .21f, 30));
            hits = maxHits;
            this.size = new Size(54, 64);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 6.66f / Data.hitMult);
            this.pic = texture;
            reload = Data.Random.RangeInt(0, ReloadTime);
        }

        protected override int ReloadTime
        {
            get
            {
                return Data.Random.GaussianCappedInt(50 * Skull.mult, .1f, 13);
            }
        }

        protected override float ySpeed
        {
            get
            {
                return .6f;
            }
        }

        protected override float Damage
        {
            get
            {
                return Data.Random.GaussianCapped(75, .1f, 60);
            }
        }

        protected override float getXVel()
        {
            return Data.Random.Gaussian(.1f);
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\skulls\\avg.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.White.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}
