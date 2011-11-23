using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Trogdor
{
	enum Type
	{
		Hut,
		Ally,
		Enemy,
		Player
	}

	class Piece
	{
		Type type;
		double x, y, xVel, yVel, size;

		public double Size
		{
			get { return size; }
			set
			{
				double old = Side;

				size = value;

				this.x += (old - Side) / 2.0;
				this.y += (old - Side) / 2.0;
			}
		}

		private double Side
		{
			get
			{
				return 2.0 * Math.Sqrt(Size / Math.PI);
			}
		}

		public Type Type
		{
			get { return type; }
			set { type = value; }
		}

		public Piece(Type type, double Size)
		{
			this.type = type;
			this.size = Size;
			this.x = Game.rand.DoubleHalf((double)Game.MaxWidth - Side);
			this.y = Game.rand.DoubleHalf((double)Game.MaxHeight - Side);
			xVel = 0;
			yVel = 0;
		}

		private void createChild()
		{
			if (Game.rand.Bool(Game.createChildTime))
			{
				double size = Game.rand.DoubleHalf() * this.Size;
				Game.totalAlly += size;
				Piece child = new Piece(Type.Ally, size);
				child.x = this.x + this.Side / 2.0 - child.Side / 2.0;
				child.y = this.y + this.Side / 2.0 - child.Side / 2.0;
				Game.pieces.Add(child);
			}
		}

		public void Accelerate(double xVel, double yVel)
		{
			this.xVel += xVel;
			this.yVel += yVel;
		}

		private void randomMove()
		{
			double offset = Game.otherOffset;
			if (type == Type.Player)
				offset = Game.playerOffset;

			Accelerate(Game.rand.DoubleHalf(offset) - Game.rand.DoubleHalf(offset),
						Game.rand.DoubleHalf(offset) - Game.rand.DoubleHalf(offset));
		}

		public void Increment()
		{
			if (this.Size == 0)
			{
				if (this.type == Type.Player)
					Game.GameOver();
				Game.pieces.Remove(this);
				return;
			}

			if (Game.rand.Bool() && Game.rand.Bool() && Game.rand.Bool() && Game.rand.Bool())
			{
				double decay = Game.rand.OE(Game.death * (3.0 + this.Size / Game.hutSize));
				switch (this.Type)
				{
					case Type.Ally:
						Game.decayAlly += decay;
						break;
					case Type.Enemy:
						Game.decayEnemy += decay;
						break;
					case Type.Hut:
						Game.decayHut += decay;
						break;
					case Type.Player:
						Game.decayPlayer += decay;
						break;
				}
				this.Size -= decay;
			}

			if (this.Size <= 0)
				this.Size = 0;

			switch (this.Type)
			{
				case Type.Ally:
				case Type.Enemy:
					randomMove();

					Accelerate((Game.MaxWidth / 2.0 - this.x) * Game.otherSpeed,
						(Game.MaxHeight / 2.0 - this.y) * Game.otherSpeed);
					break;

				case Type.Player:
					randomMove();

					if (Game.down)
						Accelerate(0, Game.playerSpeed);
					if (Game.up)
						Accelerate(0, Game.playerSpeed * -1.0);
					if (Game.left)
						Accelerate(Game.playerSpeed * -1.0, 0);
					if (Game.right)
						Accelerate(Game.playerSpeed, 0);
					break;
			}

			this.x += xVel;
			this.y += yVel;

			checkSides();

			if (type == Type.Hut)
				createChild();
		}

		private void checkSides()
		{
			bool hit = false;

			if (x + Side > Game.MaxWidth)
			{
				x = Game.MaxWidth - Side;
				hit = true;
			}
			if (x < 0)
			{
				x = 0;
				hit = true;
			}
			if (y + Side > Game.MaxHeight)
			{
				y = Game.MaxHeight - Side;
				hit = true;
			}
			if (y < 24)
			{
				y = 24;
				hit = true;
			}

			if (hit)
			{
				if (this.type == Type.Player)
					this.Size -= Math.Sqrt(xVel * xVel + yVel * yVel) * Game.hitDamage;

				yVel = 0;
				xVel = 0;
			}
		}

		private bool hits(Piece piece)
		{
			return ((this.Side + piece.Side) / 2.0 >= Math.Sqrt(Math.Pow(
				this.x + this.Side / 2.0 - piece.x - piece.Side / 2.0, 2)
				+ Math.Pow(this.y + this.Side / 2.0 - piece.y - piece.Side / 2.0, 2)));
		}

		public void checkCollisions()
		{
			foreach (Piece piece in Game.pieces)
			{
				if (this.hits(piece))
				{
					double death = Math.Min(this.Size, piece.Size);

					switch (piece.type)
					{
						case Type.Ally:
							Game.totalPlayer += piece.Size;
							Game.collectAlly += piece.Size;
							this.Size += piece.Size;
							piece.Size = 0;
							break;

						case Type.Enemy:
							Game.collectEnemy += piece.Size;
							this.Size -= death;
							piece.Size -= death;
							break;

						case Type.Hut:
							Game.collectHut += piece.Size;
							Game.score += death;
							this.Size -= death;
							piece.Size -= death;
							break;
					}
				}
			}
		}

		public void draw(Graphics g)
		{
			Brush brush = GetBrush(type);

			g.FillEllipse(brush, (float)x, (float)y, (float)Side, (float)Side);
		}

		public static Brush GetBrush(Type type)
		{
			Brush brush;
			switch (type)
			{
				case Type.Ally:
					brush = Brushes.Green;
					break;

				case Type.Enemy:
					brush = Brushes.Red;
					break;

				case Type.Hut:
					brush = Brushes.Black;
					break;

				case Type.Player:
					brush = Brushes.Blue;
					break;

				default:
					throw new Exception();
			}
			return brush;
		}
	}
}
