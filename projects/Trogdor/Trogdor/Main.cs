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

            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Game.Paused)
                using (Font font = new Font("Arial", 30f))
                    e.Graphics.DrawString("PAUSED", font, Brushes.Black, new PointF(Game.Width / 2f - 75f, Game.Height / 2f - 15f));

            foreach (Piece piece in Game.Pieces)
                piece.Draw(e.Graphics, this.menuStrip1.Height);

            this.Text = string.Format("Trogdor - {0:f0}", Game.Score);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
            case Keys.Down:
                Game.Down = true;
                break;
            case Keys.Left:
                Game.Left = true;
                break;
            case Keys.Right:
                Game.Right = true;
                break;
            case Keys.Up:
                Game.Up = true;
                break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
            case Keys.Down:
                Game.Down = false;
                break;
            case Keys.Left:
                Game.Left = false;
                break;
            case Keys.Right:
                Game.Right = false;
                break;
            case Keys.Up:
                Game.Up = false;
                break;
            case Keys.P:
            case Keys.Pause:
                Game.Pause();
                this.Refresh();
                break;
            case Keys.S:
                Game.ShowScores();
                break;
            case Keys.N:
                if (Game.GameOver)
                {
                    Game.NewGame();
                    this.ResizeGame();
                }
                else if (MessageBox.Show("Are you sure you want to start a new game?\nThe current game will be lost!",
                        "New Game", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Game.EndGame();
                    Game.NewGame();
                    this.ResizeGame();
                }
                break;
            }
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Game.GameOver)
            {
                Game.NewGame();
                this.ResizeGame();
            }
            else if (MessageBox.Show("Are you sure you want to start a new game?\nThe current game will be lost!",
                    "New Game", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Game.EndGame();
                Game.NewGame();
                this.ResizeGame();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Game.EndGame();
        }

        private void scoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.ShowScores();
        }

        internal void ResizeGame()
        {
            Size size = this.Size;
            size.Width -= this.ClientSize.Width;
            size.Height -= this.ClientSize.Height - this.menuStrip1.Height;
            size.Width += Game.Width;
            size.Height += Game.Height;

            this.MinimumSize = this.MaximumSize = this.Size = size;
        }
    }
}