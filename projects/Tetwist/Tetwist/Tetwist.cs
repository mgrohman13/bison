using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tetwist
{
	public partial class Tetwist : Form
	{
		public const int SquareSize = 26;
		public static int StartHeight;
		Font font = new Font(FontFamily.GenericMonospace, 30, FontStyle.Bold);
		PointF fontPoint = new PointF();

		public static int levelTimer = -1;

		public Tetwist()
		{
			InitializeComponent();
			this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

			this.musicToolStripMenuItem.Checked = Game.Music;
			StartHeight = this.menuStrip.Height;
			this.ClientSize = new Size(SquareSize * Game.Width + this.panel.Width + 1,
				SquareSize * Game.Height + StartHeight + 1);
		}

		void Pause()
		{
			Game.Paused = !Game.Paused;
			pauseToolStripMenuItem.Checked = Game.Paused;
			Refresh();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			this.lblLevel.Text = Game.Level.ToString();
			this.lblLines.Text = Game.Lines.ToString();
			this.lblNext.Text = Game.Next.ToString();
			this.lblScore.Text = Game.Score.ToString();

			if (!Game.Paused)
			{
				Game.Draw(e.Graphics);

				if (levelTimer > -1)
				{
					const string level = "LEVEL ";
					string levelString = level + Game.Level.ToString();
					PointF temp = GetPoint(e.Graphics, levelString);
					e.Graphics.DrawString(levelString, font, Brushes.Black, temp);
				}
			}
			else if (Game.Running)
			{
				const string paused = "PAUSED";
				if (fontPoint.IsEmpty)
					fontPoint = GetPoint(e.Graphics, paused);
				e.Graphics.DrawString(paused, font, Brushes.Black, fontPoint);
			}

			base.OnPaint(e);
		}

		private PointF GetPoint(Graphics graphics, string text)
		{
			SizeF size = graphics.MeasureString(text, font);

			PointF point = new PointF((this.ClientSize.Width - this.panel.Width - size.Width) / 2f,
				(this.ClientSize.Height - size.Height) / 2f);

			return point;
		}

		void newGameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Game.NewGame();
			if (Game.Paused)
				Refresh();
		}

		void musicToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Game.Music = this.musicToolStripMenuItem.Checked;
		}

		void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void Tetwist_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Left:
					Game.MoveCurrent(true);//, true);
					break;
				case Keys.Right:
					Game.MoveCurrent(false);//, true);
					break;
				case Keys.Up:
					Game.Rotate();
					break;
				case Keys.Down:
					Game.Drop();
					break;
				case Keys.P:
					Pause();
					break;
			}
		}

		void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Pause();
		}

		void Tetwist_FormClosed(object sender, FormClosedEventArgs e)
		{
			font.Dispose();
		}

		//void Tetwist_KeyUp(object sender, KeyEventArgs e)
		//{
		//    switch (e.KeyCode)
		//    {
		//        case Keys.Left:
		//            Game.MoveCurrent(true, false);
		//            break;
		//        case Keys.Right:
		//            Game.MoveCurrent(false, false);
		//            break;
		//    }
		//}

		//delegate void RefreshCallback();
		//RefreshCallback refreshCallback;
		//public void ThreadSafeRefresh()
		//{
		//    if (this.InvokeRequired)
		//    {
		//        this.refreshCallback = new RefreshCallback(ThreadSafeRefresh);
		//        this.Invoke(refreshCallback);
		//    }
		//    else
		//    {
		//        this.Refresh();
		//    }
		//}
	}
}