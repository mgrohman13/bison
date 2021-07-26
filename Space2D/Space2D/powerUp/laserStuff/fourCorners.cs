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
    class fourCorners : Weapons
    {
        public static refInt ammo;

        public fourCorners()
            : base(4)
        {
        }

        protected override void Fire()
        {
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(1, -1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(-1, -1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(-1, 1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(1, 1, 0));
        }

        protected override int GetAmmo()
        {
            return Data.Random.GaussianCappedInt(26, .13f, 13);
        }
    }
}