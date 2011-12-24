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
    class allEight : Weapons
    {
        public static refInt ammo;

        public allEight()
            : base(2)
        {
        }

        protected override void Fire()
        {
            //fire eight lasers in separate directions
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(1, 1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(0, -1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(-1, 0, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(0, 1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(1, -1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(-1, -1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(-1, 1, 0));
            new weapons.regLaser(Data.player.X, Data.player.Y, true, new Vector3(1, 0, 0));
        }

        protected override int GetAmmo()
        {
            return Data.Random.GaussianCappedInt(16.9, .13f, 6);
        }
    }
}