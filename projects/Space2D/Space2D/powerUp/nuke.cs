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
	class nuke : powerUp
	{
		static Texture texture;

		public static bool used;
		public const int nukeTime = 1300;
		public static int time;

		public nuke()
			: base(29)
		{
			this.curFrame = 0;
			this.width = 5;
			this.height = 6;
			this.pic = texture;
			this.size = new Size(48, 48);
			this.x = Data.Random.Next(Data.Width);
			this.y = -size.Height / 2;

			powerUp.all.Add(this);
		}

		public override void Inc()
		{
			if (collected && !used)
			{
				//fire when n is pressed
				KeyboardState state;
				if (!Data.player.Dead && (state = mainForm.state)[Key.N])
				{
					used = true;
					time = nukeTime;

					new weapons.nuke(Data.player.X, Data.player.Y, true);

					powerUp.Remove(this);
				}
			}

			//slow down animation...
			animate = !animate;
		}

		public static void loadTexture()
		{
			texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\nuke.bmp", 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Magenta.ToArgb());
		}

		new public static void disposeTextures()
		{
			texture.Dispose();
		}
	}
}