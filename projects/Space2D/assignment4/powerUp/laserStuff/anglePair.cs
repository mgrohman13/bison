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
    class anglePair : Weapons
    {
        public static refInt ammo;

        public anglePair()
            : base(1)
        {
        }

        protected override void Fire()
        {
            //fire 2 lasers
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(-1, -1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(1, -1, 0));
        }

        protected override int GetAmmo()
        {
            return Data.Random.GaussianCappedInt(39, .13f, 21);
        }
    }
}