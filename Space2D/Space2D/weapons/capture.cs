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
	class capture : weapon
	{
		const float speed = 6.66f;

		static Texture texture;

		Vector3 heading;

		public capture(float x, float y, bool friendly, Vector3 heading)
			: base(47)
		{
			damage = 0;

			curFrame = 0;
			this.width = 10;
			this.height = 6;
			this.pic = texture;
			this.size = new Size(94, 94);
			this.x = x;
			this.y = y;
			this.friendly = friendly;

			weapon.all.Add(this);

			//keep speed constant
			heading.Normalize();
			heading.Scale(speed);
			this.heading = heading;
		}

		public override void collisions(enemies.enemy[] temp)
		{
			for (int i = temp.Length; --i > -1; )
			{
				enemies.enemy e = temp[i];
				if (e.Alive && (friendly != e.Friendly) && Data.Collide(this, e))
				{
					//capture on contact
					if (e.Capture() && stops)
					{
						all.Remove(this);
						break;
					}
				}
			}
		}

		public override void Inc()
		{
			//bounce off walls
			if ((y += heading.Y - Data.scrollSpeed) > Data.Height || y < 0)
				heading.Y = -heading.Y;
			if ((x += heading.X) > Data.Width || x < 0)
				heading.X = -heading.X;
		}

		public static void loadTexture()
		{
			texture = TextureLoader.FromFile(mainForm.device, Data.path + "weapon\\capture.bmp", 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
		}

		new public static void disposeTextures()
		{
			texture.Dispose();
		}
	}
}