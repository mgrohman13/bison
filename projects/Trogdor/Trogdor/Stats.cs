using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Trogdor
{
	public partial class Stats : Form
	{
		float mult;

		public Stats()
		{
			InitializeComponent();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			float diameter = GetDiameter(Game.totalPlayer);
			float length = 50;
			e.Graphics.FillEllipse(Piece.GetBrush(Type.Player), 50, length, diameter, diameter);
			length += diameter;
			diameter = GetDiameter(Game.totalHut);
			e.Graphics.FillEllipse(Piece.GetBrush(Type.Hut), 50, length, diameter, diameter);
			length += diameter;
			diameter = GetDiameter(Game.totalAlly);
			e.Graphics.FillEllipse(Piece.GetBrush(Type.Ally), 50, length, diameter, diameter);
			length += diameter;
			diameter = GetDiameter(Game.totalEnemy);
			e.Graphics.FillEllipse(Piece.GetBrush(Type.Enemy), 50, length, diameter, diameter);
		}

		internal void Init()
		{
			const float maxSize = 3000;
			mult = (float)Math.Max(Math.Max(Math.Max(Game.totalPlayer, Game.totalHut), Game.totalAlly), Game.totalEnemy);
			if (mult > 0)
			{
				mult = mult / maxSize;
				Game.totalPlayer /= mult;
				Game.totalHut /= mult;
				Game.totalAlly /= mult;
				Game.totalEnemy /= mult;
			}

			float length = GetDiameter(Game.totalPlayer);
			length += GetDiameter(Game.totalHut);
			length += GetDiameter(Game.totalAlly);
			length += GetDiameter(Game.totalEnemy);
			this.ClientSize = new Size(300, Game.rand.Round(length) + 100);
		}

		private float GetDiameter(double area)
		{
			return (float)(Math.Sqrt(area / Math.PI)) * 2f;
		}
	}
}