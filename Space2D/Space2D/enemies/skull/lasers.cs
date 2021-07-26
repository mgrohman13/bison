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
    class lasers : Skull
    {
        static Texture texture;

        public lasers()
            : base(28)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(39, .06f, 30));
            hits = maxHits;
            this.size = new Size(42, 59);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 7.8f / Data.hitMult);
            this.pic = texture;
            reload = ReloadTime;
        }

        protected override int ReloadTime
        {
            get
            {
                return Data.Random.OEInt(21 * Skull.mult);
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
                return Data.Random.GaussianCapped(100, .06f, 87);
            }
        }

        protected override float getXVel()
        {
            return Data.Random.Gaussian(0.1f);
        }

        protected override void Fire()
        {
            //randomly fire burst or beam laser
            switch (Data.Random.Next(3))
            {
            case 0:
                base.Fire();
                break;
            case 1:
            case 2:
                new weapons.regLaser(x - 13, y, friendly);
                break;
            }
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\skulls\\lasers.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}
