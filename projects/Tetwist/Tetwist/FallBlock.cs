using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Tetwist
{
	class FallBlock : Block
	{
		public FallBlock(Point p) : base(p) { }

		protected override Brush Brush { get { return Brushes.Green; } }

		public override void Notify(Notifications.Type type, object information)
		{
			if (type == Notifications.Type._Solidification)
			{
				if (Fall())
					Notifications.AddNotification(Notifications.Type.Iteration, this);
			}
			else if (type == Notifications.Type.Iteration)
			{
				if (Fall())
				{
					int newY;
					Block below;
					if ((newY = Point.Y + 1) == Game.Height || !(((below = Game.GetBlock(new Point(Point.X, newY)))
						is FallBlock) || (below is SettleBlock)))
					{
						Notifications.RemoveNotification(Notifications.Type.Iteration, this);
						this.Destroy();
						new Block(this.Point);
					}
				}
			}
			else
				base.Notify(type, information);
		}

		  bool Fall()
		{
			while (this.MoveBlock(0, 1)) { }
			return !Game.CheckLine(Point.Y);
		}
	}
}
