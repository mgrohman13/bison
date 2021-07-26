using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MattUtil.RealTimeGame
{
    public partial class GameForm : Form
    {
        protected Game game = null;

        protected void NewGame(bool scoring)
        {
            if (this.game != null)
                lock (this.game.gameTicker)
                    this.game.gameTicker.End();
            this.game = GetNewGame(scoring);
            RefreshGame();
        }

        protected virtual Game GetNewGame(bool scoring)
        {
            throw new Exception("must override this method");
        }

        public GameForm()
        {
            InitializeComponent();

            //eliminate flickering
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (QuitPrompt())
                NewGame(ScoresForm.Scoring);
        }

        protected bool QuitPrompt()
        {
            if (AskQuit())
            {
                bool retVal = false;
                bool wasPaused = this.game.Paused;
                this.game.Paused = true;
                if (MessageBox.Show("Are you sure you want to quit the current game?", "Quit",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    retVal = true;
                this.game.Paused = wasPaused;
                return retVal;
            }
            return true;
        }

        protected virtual bool AskQuit()
        {
            return ( this.game.Running && this.game.Scoring );
        }

        protected virtual void RefreshGame()
        {
            Invalidate(ClientRectangle, false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.game != null)
                this.game.Draw(e.Graphics);
            base.OnPaint(e);
        }

        private void scoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HighScores.ShowScores(this.game);
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !QuitPrompt();
            if (!e.Cancel)
                lock (this.game.gameTicker)
                {
                    //clean up
                    this.game.gameTicker.End();
                }
        }
    }
}
