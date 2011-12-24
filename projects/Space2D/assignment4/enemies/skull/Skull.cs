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

namespace assignment4.enemies
{
	abstract class Skull : enemy
	{
		protected int reload;
		public const int mult = 3;

		float xvel;

		public Skull(int radius)
			: base(radius)
		{
			//stats consistant for all skulls
			this.curFrame = 0;
			this.height = 1;
			this.width = 1;
			this.x = Data.Random.Next(Data.Width);

			xvel = 0;

			enemy.all.Add(this);
		}

		public override void Inc()
		{
			//random x movement
			if ((x += (xvel += getXVel())) > Data.Width - size.Width / 2)
			{
				x = Data.Width - size.Width / 2;
				xvel = 0;
			}
			else if (x < 0 + size.Width / 2)
			{
				x = 0 + size.Width / 2;
				xvel = 0;
			}

			if (friendly)
				y -= Data.scrollSpeed;
			else
				y += ySpeed;

			//remove if it goes off the bottom
			if (y > size.Height / 2 + Data.Height)
				enemy.Remove(this);

			if (--reload < 1)
			{
				Fire();
				reload = ReloadTime;
			}
		}

		protected virtual void Fire()
		{
			new weapons.laserBeam(x, friendly, Damage, xvel);
		}

		//stats required for each type of skull
		protected abstract float getXVel();
		protected abstract float ySpeed { get; }
		protected abstract float Damage { get; }
		protected abstract int ReloadTime { get; }

		public override void draw(Sprite sprite)
		{
			if (friendly)
				DrawRotate(sprite, new Vector3(0, 1, 0));
			else
				base.draw(sprite);
		}

		public static void loadTexture(System.Windows.Forms.ProgressBar pb)
		{
			//load subclass textures
			skull.avg.loadTexture();
			skull.fast.loadTexture();
			++pb.Value;
			skull.power.loadTexture();
			skull.weak.loadTexture();
			++pb.Value;
			skull.pellets.loadTexture();
			skull.lasers.loadTexture();
			pb.Value += 2;
		}

		new public static void disposeTextures()
		{
			skull.avg.disposeTextures();
			skull.fast.disposeTextures();
			skull.power.disposeTextures();
			skull.weak.disposeTextures();
			skull.pellets.disposeTextures();
			skull.lasers.disposeTextures();
		}

		new public static void create()
		{
			//create a random subclass
			switch (Data.Random.Next(6))
			{
				case 0:
					new skull.avg();
					break;
				case 1:
					new skull.fast();
					break;
				case 2:
					new skull.power();
					break;
				case 3:
					new skull.weak();
					break;
				case 4:
					new skull.pellets();
					break;
				case 5:
					new skull.lasers();
					break;
			}
		}
	}
}