using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Tetwist
{
	class InfectBlock : Block
	{
		static int infectedCount;
		public static int InfectedCount { get { return infectedCount; } }
		public static void NewGame()
		{
			infectedCount = 0;
		}
		protected override void OnDestroy()
		{
			--infectedCount;
		}

		public InfectBlock(Point p)
			: base(p)
		{
			++infectedCount;
			Notifications.AddNotification(Notifications.Type.BlockSet, this);
		}

		protected override Brush Brush
		{
			get
			{
				return Brushes.Magenta;
			}
		}

		public override void Notify(Notifications.Type type, object information)
		{
			if (type == Notifications.Type.BlockSet)
			{
				Point newP;
				do
				{
					int dir = Game.Random.Next(4);
					newP = new Point(Point.X + (dir == 0 ? 1 : (dir == 1 ? -1 : 0)),
					   Point.Y + (dir == 2 ? 1 : (dir == 3 ? -1 : 0)));
				}
				while (newP.X < 0 || newP.X >= Game.Width || newP.Y < 0 || newP.Y >= Game.Height);

				if (Game.HasBlock(newP))
				{
					Block other = Game.GetBlock(newP);
					if (!((other is InfectBlock) || (other is SettleBlock)))
					{
						other.Destroy();
						new InfectBlock(newP);
					}
				}
				else
				{
					RealMove(newP);
					if (Game.CheckLine(Point.Y))
						Notifications.RemoveNotification(Notifications.Type.BlockSet, this);
				}
			}
			else
				base.Notify(type, information);
		}
	}
}
