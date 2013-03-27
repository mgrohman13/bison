using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Tetwist
{
	class SettleBlock : Block
	{
		public SettleBlock(Point p) : base(p) { }

		protected override Brush Brush { get { return Brushes.Blue; } }

		public override void Notify(Notifications.Type type, object information)
		{
			if (type == Notifications.Type.Iteration)
			{
				int newX = base.Point.X, newY, curY = Point.Y;
				Point newPoint;
				if ((newY = base.Point.Y + 1) < Game.Height && !Game.HasBlock(newPoint = new Point(newX, newY)))
				{
					this.MoveBlock(newPoint);
					CheckLine(newY);
				}
				else
				{
					bool left = (--newX) > -1 && !Game.HasBlock(new Point(newX, curY));
					bool right = (newX += 2) < Game.Width && !Game.HasBlock(new Point(newX, curY));

					if (left || right)
					{
						if (left && right)
						{
							bool r2 = !Game.HasBlock(new Point(newX, newY));
							bool l2 = !Game.HasBlock(new Point(newX - 2, newY));

							if (r2 ^ l2)
							{
								if (r2)
									left = false;
							}
							else
							{
								if (Game.Random.Bool())
									left = false;
							}
						}

						if (left)
							newX -= 2;

						this.MoveBlock(new Point(newX, curY));
						CheckLine(curY);
					}
				}
			}
			else if (type == Notifications.Type._Solidification)
			{
				Notifications.AddNotification(Notifications.Type.Iteration, this);
			}
			else
				base.Notify(type, information);
		}

		void CheckLine(int y)
		{
			if (Game.CheckLine(y))
				Notifications.RemoveNotification(Notifications.Type.Iteration, this);
		}
	}
}
