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
	abstract class Weapons : powerUp
	{
		new public static void NewGame()
		{
			allEight.ammo = new refInt(0);
			anglePair.ammo = new refInt(0);
			fourCorners.ammo = new refInt(0);
			pair.ammo = new refInt(0);
			randomLasers.ammo = new refInt(0);
			topFour.ammo = new refInt(0);
		}

		static Texture texture;
		refInt ammo;
		int reload;
		bool firstCollected = true;

		public Weapons(int frame)
			: base(26)
		{
			this.curFrame = frame;
			animate = false;
			this.width = 2;
			this.height = 3;
			this.pic = texture;
			this.size = new Size(42, 42);
			this.x = Data.Random.Next(Data.Width);
			this.y = -size.Height / 2;

			powerUp.all.Add(this);

			if (this is allEight)
				ammo = allEight.ammo;
			else if (this is anglePair)
				ammo = anglePair.ammo;
			else if (this is fourCorners)
				ammo = fourCorners.ammo;
			else if (this is pair)
				ammo = pair.ammo;
			else if (this is randomLasers)
				ammo = randomLasers.ammo;
			else if (this is topFour)
				ammo = topFour.ammo;
		}

		public override void Inc()
		{
			if (collected)
			{
				//when the power up is first collected
				if (firstCollected)
				{
					firstCollected = false;

					//if the same type has already been collected, there is no need to keep the second one
					if (ammo.val > 0)
						powerUp.all.Remove(this);

					//reset the reload and add more ammo
					reload = player.reloadTime;
					ammo.val += GetAmmo();
				}

				//fire when the spacebar is pressed
				KeyboardState state = mainForm.state;
				if (--reload < 0 && !Data.player.Dead && state[Key.Space])
				{
					Fire();

					//remove when out of ammo
					if (--ammo.val < 1)
						powerUp.Remove(this);
					else
						reload = player.reloadTime;
				}

			}
		}

		//required methods for each subclass
		protected abstract int GetAmmo();
		protected abstract void Fire();

		public static void loadTexture()
		{
			//pictures for each type of weapon power up are included in this file
			texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\weapons.bmp", 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Red.ToArgb());
		}

		new public static void disposeTextures()
		{
			texture.Dispose();
		}

		new public static void create()
		{
			//new nuke();
			//return;
			//create a random weapon power up
			switch (Data.Random.Next(6))
			{
				case 0:
					new pair();
					break;

				case 1:
					new allEight();
					break;

				case 2:
					new anglePair();
					break;

				case 3:
					new fourCorners();
					break;

				case 4:
					new randomLasers();
					break;

				case 5:
					new topFour();
					break;
			}
		}
	}

	//an integer that is passed by reference
	class refInt
	{
		public refInt(int val)
		{
			this.val = val;
		}

		public int val;
	}
}