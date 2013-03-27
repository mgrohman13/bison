using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Tetwist
{
	class Block
	{
		public Block(Point p)
		{
			this.Point = p;
			Game.AddBlock(p, this);
		}

		static double fallBlockChance, fallBlockInner, explosiveBlockChance, settleBlockChance, infectedCountBase;
		public static void NewActiveBlock()
		{
			explosiveBlockChance = 1 - Math.Pow(.994, Math.Pow(infectedCountBase, InfectBlock.InfectedCount));
			fallBlockChance = Game.Random.OE(fallBlockInner);
		}
		public static void NewLevel()
		{
			int level = Game.Level + 1;
			fallBlockInner = .666 * Math.Pow(.87, level);
			settleBlockChance = .0666 * (1 - Math.Pow(.9, level));
			infectedCountBase = 13.0 / level;
		}

		public static Block NewInstance(Point p)
		{
			if (Game.Level > 1 && Game.Random.Bool(fallBlockChance))
				return new FallBlock(p);
			if (Game.Level > 2 && Game.Random.Bool(settleBlockChance))
				return new SettleBlock(p);
			if (Game.Level > 3 && Game.Random.Bool(explosiveBlockChance))
				return new ExplosiveBlock(p);
			if (Game.Level > 4 && Game.Random.Bool(.00666))
				return new InfectBlock(p);
			return new Block(p);
		}

		protected void RealMove(Point newPoint)
		{
			Game.RemoveBlock(Point);
			Game.AddBlock(newPoint, this);
			Point = newPoint;
		}

		public static bool MoveBlocks(IEnumerable<Block> blocks, Dictionary<Block, Point> newPoints)
		{
			List<Block> move = blocks as List<Block> ?? new List<Block>(blocks);

			foreach (Block b in newPoints.Keys)
			{
				Point p = newPoints[b];
				if ((Game.HasBlock(p) && !move.Contains(Game.GetBlock(p))) ||
					(p.X < 0) || (p.X >= Game.Width) || (p.Y < 0) || (p.Y >= Game.Height))
					return false;
			}

			foreach (Block b in move)
				Game.RemoveBlock(b.Point);
			foreach (Block b in move)
			{
				if (!b.Dead)
				{
					b.Point = newPoints[b];
					Game.AddBlock(b.Point, b);
				}
			}

			return true;
		}
		public delegate bool CheckBlock(Block b);
		public static bool MoveBlocks(IEnumerable<Block> blocks, int xMod, int yMod, CheckBlock checkCondition)
		{
			if (checkCondition != null)
			{
				List<Block> temp = new List<Block>();
				foreach (Block b in blocks)
					if (checkCondition(b))
						temp.Add(b);

				blocks = temp;
			}

			Dictionary<Block, Point> newPoints = new Dictionary<Block, Point>();
			foreach (Block b in blocks)
				newPoints.Add(b, new Point(b.Point.X + xMod, b.Point.Y + yMod));
			return MoveBlocks(blocks, newPoints);
		}
		public static bool MoveBlocks(IEnumerable<Block> blocks, int xMod, int yMod)
		{
			return MoveBlocks(blocks, xMod, yMod, null);
		}

		public bool MoveBlock(int xMod, int yMod)
		{
			return MoveBlock(new Point(Point.X + xMod, Point.Y + yMod));
		}
		public bool MoveBlock(Point newPoint)
		{
			Dictionary<Block, Point> newPoints = new Dictionary<Block, Point>();
			newPoints.Add(this, newPoint);
			return MoveBlocks(new Block[] { this }, newPoints);
		}

		public void Destroy()
		{
			Game.RemoveBlock(Point);
			dead = true;
			OnDestroy();
		}
		protected virtual void OnDestroy() { }

		protected virtual Brush Brush { get { return Brushes.DarkGray; } }

		bool dead = false;
		public bool Dead
		{
			get
			{
				return dead;
			}
		}

		Point p;
		public Point Point
		{
			get { return p; }
			set { this.p = value; }
		}

		public virtual void Notify(Notifications.Type type, object information)
		{
			if (type != Notifications.Type._Solidification)
				throw new Exception();
		}

		public virtual void Draw(Graphics graphics, Rectangle r)
		{
			graphics.FillRectangle(Brush, r);
			graphics.DrawRectangle(Pens.Black, r);
		}
	}

	class GameOverBlock : Block
	{
		public GameOverBlock(Point p) : base(p) { }

		protected override Brush Brush { get { return Brushes.Black; } }
	}
}
