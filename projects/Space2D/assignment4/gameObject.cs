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
	abstract class gameObject
	{
		public gameObject(int radius)
		{
			this.radius = radius;
		}

		protected Size size;
		protected float x, y;
		protected Texture pic;
		protected bool animate = true;
		protected int radius;

		//graphics stuff
		protected int curFrame, width, height;

		public abstract void Inc();

		public float X
		{
			get
			{
				return x;
			}
		}

		public float Y
		{
			get
			{
				return y;
			}
		}

		public Size ObjectSize
		{
			get
			{
				return size;
			}
		}

		//public abstract bool Turns { get; }
		public float Radius { get { return radius; } }

		//public Rectangle rect
		//{
		//    get
		//    {
		//        if (Turns)
		//            throw new Exception();
		//        return new Rectangle(new Point((int)(x - size.Width / 2), (int)(y - size.Height / 2)), size);
		//    }
		//}

		public virtual void draw(Sprite sprite)
		{
			ActualDraw(sprite);
		}

		void ActualDraw(Sprite sprite)
		{
			int picX, picY;
			picX = curFrame % width;
			picY = curFrame / width;
			picX *= size.Width;
			picY *= size.Height;

			sprite.Draw(pic, new Rectangle(picX, picY, size.Width, size.Height),
				new Vector3(size.Width / 2, size.Height / 2, 1), new Vector3(x, y, 1),
				Data.player.Dead && Data.player.Lives > -1 ? Color.Red : Color.White);

			if (animate && !mainForm.paused && Data.player.Lives > -1)
				++curFrame;

			if (curFrame >= width * height)
				curFrame = 0;

#if TRACE
			DrawTrace(sprite);
#endif
		}

		/// <summary>
		/// draw at a specific rotation
		/// </summary>
		/// <param name="sprite">the sprite to use</param>
		/// <param name="heading">the rotation</param>
		protected void DrawRotate(Sprite sprite, Vector3 heading)
		{
			if (heading.Y < 0 && Math.Abs(heading.X) < .000666)
			{
				ActualDraw(sprite);
				return;
			}

			Vector3 up = new Vector3(0, -1, 0);
			Vector3 head = heading;
			head.Normalize();
			float dotProd = Vector3.Dot(up, head);
			if (dotProd > -1 && dotProd < 1)
			{
				float angle = (float)Math.Acos(dotProd);

				if (head.X < 0)
					angle = (float)(Math.PI * 2) - angle;

				//fix the x and y coordinates so it draws at the correct place
				float xOld = x, yOld = y;
				Vector3 xy = new Vector3(x, y, 0);
				xy.TransformCoordinate(Matrix.RotationZ(-angle));
				x = xy.X;
				y = xy.Y;

				//draw the sprites already in the buffer
				sprite.Flush();
				//transform the world at the rotation
				mainForm.device.Transform.World = Matrix.RotationZ(angle);
				//draw this object
				ActualDraw(sprite);
				sprite.Flush();
				//reset the world transform
				mainForm.device.Transform.World = Matrix.RotationZ(0);

				//reset the x and y
				x = xOld;
				y = yOld;
			}
			else
				ActualDraw(sprite);

#if TRACE
			DrawTrace(sprite);
#endif
		}

#if TRACE
		private void DrawTrace(Sprite sprite)
		{
			sprite.Flush();
			float scaleFactor = radius / 49.5f;
			mainForm.device.Transform.World = Matrix.Scaling(scaleFactor, scaleFactor, 1);
			sprite.Draw(Data.tracer, Rectangle.Empty, new Vector3(49.5f, 49.5f, 1), new Vector3(x / scaleFactor, y / scaleFactor, 1), Color.White);
			sprite.Flush();
			mainForm.device.Transform.World = Matrix.Scaling(1, 1, 1);
		}
#endif
	}
}