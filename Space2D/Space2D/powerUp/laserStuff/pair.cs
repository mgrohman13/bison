using System;
using System.Collections.Generic;
using System.Text;

namespace assignment4.powerUp
{
    class pair : Weapons
    {
        public static refInt ammo;

        public pair() : base(0)
        {
        }

        protected override void Fire()
        {
            new weapons.regLaser(Data.player.X + Data.player.ObjectSize.Width / 2, Data.player.Y, true);
            new weapons.regLaser(Data.player.X - Data.player.ObjectSize.Width / 2, Data.player.Y, true);
        }

        protected override int GetAmmo()
        {
            return Data.Random.GaussianCappedInt(39, .13f, 21);
        }
    }
}