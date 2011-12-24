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
	class capture : powerUp
	{
		static Texture texture;

		public static bool used;
		public const int captTime = 39;
		public static int time;
		int times;

		public capture()
			: base(29)
		{
			this.curFrame = 0;
			this.width = 10;
			this.height = 6;
			this.pic = texture;
			this.size = new Size(47, 47);
			this.x = Data.Random.Next(Data.Width);
			this.y = -size.Height / 2;

			powerUp.all.Add(this);

			times = 13;
		}

		public override void Inc()
		{
			if (collected && !used)
			{
				KeyboardState state;

				//fire when v is pressed
				if (!Data.player.Dead && (state = mainForm.state)[Key.V])
				{
					used = true;
					time = captTime;

					new weapons.capture(Data.player.X, Data.player.Y, true, new Vector3(0, -1, 0));

					//remove the power up when no shots are left
					if (--times < 1)
						powerUp.Remove(this);
				}
			}
		}

		public static void loadTexture()
		{
			texture = TextureLoader.FromFile(mainForm.device, Data.path + "power\\capture.bmp", 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
		}

		new public static void disposeTextures()
		{
			texture.Dispose();
		}
	}
}