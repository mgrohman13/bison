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

namespace assignment4
{
	class explosion : gameObject
	{
		public Size Size
		{
			get { return size; }
		}

		public override void Inc() { }

		public explosion(Texture t, Size size, int curFrame, int width, int height)
			: base(0)
		{
			this.pic = t;
			this.size = size;
			this.curFrame = curFrame;
			this.width = width;
			this.height = height;
		}

		public void setXY(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		//create an identical explosion
		public explosion clone()
		{
			explosion result = new explosion(pic, size, curFrame, width, height);
			result.setXY(x, y);
			return result;
		}

		public bool CheckFrame()
		{
			return curFrame == 0;
		}

		public void disposeTextures()
		{
			pic.Dispose();
		}
	}
}