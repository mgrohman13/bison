using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceRunner
{
	class Mine : GameObject
	{
		//    public static readonly Image MineImage = Game.LoadImage("mine.bmp", Game.MineSize);

		public static void Dispose()
		{
			//MineImage.Dispose();
			//font.Dispose();
		}

		public override decimal Score
		{
			get { return 0m; }
		}

		//    bool friendly;
		//    public bool Friendly
		//    {
		//        get { return friendly; }
		//    }

		Brush brush; bool main;
		Mine(float x, float y, float xDir, float yDir, bool main)
			: base(x, y, 0, null)
		{
			this.main = main;
			if (main)
				brush = Brushes.White;
			else
				brush = Brushes.Yellow;

			//this.id = main ? ++indexer : indexer;
			this.x = x;
			this.y = y;
			//this.xDir = xDir * Game.MineSpeedMult;
			//this.yDir = yDir * Game.MineSpeedMult;
		}
		public static void NewMine(float x, float y, bool main)
		{
			new Mine(x, y, 0, 0, main);
		}
		//    public static void NewMine(float x, float y, float xDir, float yDir)
		//    {
		//        new Mine(x, y, xDir, yDir);
		//    }

		protected override void Collide(GameObject obj) { }

		//int id;
		//public static int indexer = 0;
		//static Font font = new Font( "Arial", 30f, FontStyle.Regular);
		//public override void Draw(Graphics graphics, int centerX, int centerY)
		//{
		//    if (main)
		//        return;

		//    graphics.ResetTransform();
		//    graphics.DrawString(id.ToString(), font, brush, centerX + x, centerY + y);
		//}
	}
}
