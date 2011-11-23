using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Trogdor
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();

			this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (Game.paused)
				e.Graphics.DrawString("PAUSED", new Font("Arial", 30), Brushes.Black,
					new PointF((float)(Game.MaxWidth / 2.0 - 100.0),
					(float)(Game.MaxHeight / 2.0 - 30.0)));

			ArrayList temp = (ArrayList)Game.pieces.Clone();

			foreach (Piece piece in temp)
				piece.draw(e.Graphics);

			this.Text = string.Format("Trogdor - {0:f0}", Game.score);
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
				Game.down = true;
			if (e.KeyCode == Keys.Up)
				Game.up = true;
			if (e.KeyCode == Keys.Left)
				Game.left = true;
			if (e.KeyCode == Keys.Right)
				Game.right = true;
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
				Game.down = false;
			if (e.KeyCode == Keys.Up)
				Game.up = false;
			if (e.KeyCode == Keys.Left)
				Game.left = false;
			if (e.KeyCode == Keys.Right)
				Game.right = false;

			if (e.KeyCode == Keys.P || e.KeyCode == Keys.Pause)
			{
				Game.Pause();
				this.Refresh();
			}

			if (e.KeyCode == Keys.S)
				Game.ShowScores();

			if (e.KeyCode == Keys.N)
			{
				if (Game.gameOver)
				{
					Game.NewGame();
				}
				else if (MessageBox.Show("Are you sure you want to start a new game?\n" +
					"The current game will be lost!", "New Game", MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					Game.GameOver();
					Game.NewGame();
				}
			}
		}

		private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Game.gameOver)
			{
				Game.NewGame();
			}
			else if (MessageBox.Show("Are you sure you want to start a new game?\n" +
					"The current game will be lost!", "New Game", MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				Game.GameOver();
				Game.NewGame();
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			Game.GameOver();
		}

		private void scoresToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Game.ShowScores();
		}
	}
}