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
    class randomLasers : Weapons
    {
        public static refInt ammo;

        public randomLasers()
            : base(5)
        {
        }

        protected override void Fire()
        {
            int amt = Data.Random.OEInt(13);
            Vector3 Heading = new Vector3(1, 0, 0);
            Heading.TransformCoordinate(Matrix.RotationZ(Data.Random.DoubleHalf(4 * (float)Math.PI)));
            float div = (float)Math.PI * 2f / (float)amt;
            for (int a = -1 ; ++a < amt ; )
            {
                Heading.TransformCoordinate(Matrix.RotationZ(div));
                new weapons.regLaser(Data.player.X, Data.player.Y, true, Heading);
            }
        }

        protected override int GetAmmo()
        {
            return Data.Random.GaussianCappedInt(13, .13f, 6);
        }
    }
}