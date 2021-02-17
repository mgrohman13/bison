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
    class pellets : Skull
    {
        static Texture texture;
        int cur;
        bool dir;

        public pellets()
            : base(22)
        {
            maxHits = Data.Random.Round(Data.hitMult * Data.Random.GaussianCapped(30, .087f, 21));
            hits = maxHits;
            this.size = new Size(50, 32);
            this.y = -size.Height / 2;

            score = Data.Random.Round(hits / 7.8f / Data.hitMult);
            this.pic = texture;
            reload = ReloadTime;

            cur = 2;
            dir = Data.Random.Bool();
        }

        protected override int ReloadTime
        {
            get
            {
                return 3;
            }
        }

        protected override float ySpeed
        {
            get
            {
                return .9f;
            }
        }

        protected override float Damage
        {
            get
            {
                throw new Exception();
            }
        }

        protected override float getXVel()
        {
            return Data.Random.Gaussian(.03f);
        }

        protected override void Fire()
        {
            Vector3 heading = new Vector3(0, 1, 0);
            if (friendly)
                heading = new Vector3(0, -1, 0);

            //fire pellet
            switch (cur)
            {
            case 2:
                new weapons.pellet(x, y, friendly, heading);
                break;
            case 3:
                new weapons.pellet(x + 6, y, friendly, heading);
                break;
            case 1:
                new weapons.pellet(x - 6, y, friendly, heading);
                break;
            case 4:
                new weapons.pellet(x + 13, y, friendly, heading);
                break;
            case 0:
                new weapons.pellet(x - 13, y, friendly, heading);
                break;
            }

            if (dir)
                ++cur;
            else
                --cur;
            if (cur == 4 || cur == 0)
                dir = !dir;
        }

        public static void loadTexture()
        {
            texture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\skulls\\pellets.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
        }

        new public static void disposeTextures()
        {
            texture.Dispose();
        }
    }
}