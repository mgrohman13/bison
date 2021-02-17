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

namespace assignment4.weapons
{
	class laserBeam : weapon
	{
		static Texture texture, ftex;
		int frame;
		float dMult;
		float xVel;

		public laserBeam(float x, bool friendly, float damage, float xVel)
			: base(13)
		{
			//make the damage passed the total damage that will be inflicted over the entire laser's life
			dMult = damage / 1000f;

			frame = 0;
			curFrame = 0;
			this.width = 31;
			this.height = 1;
			//different texture for friendly laser beams
			if (friendly)
				this.pic = ftex;
			else
				this.pic = texture;
			this.size = new Size(17, 1300);
			this.x = x;
			this.y = Data.Height / 2;
			this.friendly = friendly;

			this.xVel = xVel;

			//don't remove on first enemy contact
			stops = false;

			weapon.all.Add(this);
		}

		public override void Inc()
		{
			//move x
			x += xVel;
			//y stays still
			y -= Data.scrollSpeed;

			//slow down animation
			animate = !animate;

			//increased damage towards center of laser's life
			if (frame < (width * height))
				damage = Data.Random.Round(dMult * (frame + 1));
			else
				damage = Data.Random.Round(dMult * ((width * height * 2) - frame));

			//remove when finished
			if (++frame > width * height * 2)
				weapon.all.Remove(this);
		}

		public override void collisions(assignment4.enemies.enemy[] temp)
		{
			for (int i = temp.Length; --i > -1; )
			{
				enemies.enemy e = temp[i];
				if (e.Alive && (friendly != e.Friendly) && (Math.Abs(x - e.X) < e.Radius + 13))
				{
					explosionManager.createDamage(damage, x, e.Y);

					e.hit(damage);

					if (stops)
					{
						all.Remove(this);
						break;
					}
				}
			}
		}

		public static void loadTexture()
		{
			//different texture for friendly laser beams
			texture = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\vLaserBeam.bmp", 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
			ftex = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\beam2.bmp", 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
		}

		new public static void disposeTextures()
		{
			texture.Dispose();
			ftex.Dispose();
		}
	}
}