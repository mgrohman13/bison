using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Tetwist
{
	class ExplosiveBlock : Block
	{
		public ExplosiveBlock(Point p) : base(p) { }

		protected override Brush Brush { get { return Brushes.Red; } }

		public override void Notify(Notifications.Type type, object information)
		{
			if (type == Notifications.Type._Solidification)
			{
				int scoreAmt = 1;

				int midX = Point.X, midY = Point.Y;

				for (int x = -1; x < 2; ++x)
					for (int y = -1; y < 2; ++y)
					{
						Point p = new Point(x + midX, y + midY);
						if (Game.HasBlock(p) && !(Game.GetBlock(p) is ExplosiveBlock))
						{
							Game.GetBlock(p).Destroy();
							++scoreAmt;
						}
					}

				this.Destroy();
				Game.Score += Game.Random.Round(scoreAmt * Game.ExplosionScoreMult);
			}
			else
				base.Notify(type, information);
		}
	}
}
