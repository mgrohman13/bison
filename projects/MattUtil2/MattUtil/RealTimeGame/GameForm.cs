using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MattUtil.RealTimeGame
{
    public partial class GameForm : Form
    {
        protected static Game game = null;
        public static Game Game
        {
            get
            {
                return game;
            }
        }

        protected void NewGame(GameTicker.EventDelegate RefreshGame)
        {
            if (game != null)
                lock (game.gameTicker)
                {
                    game.gameTicker.End();
                }
            game = StartNewGame(RefreshGame, ScoresForm.Scoring);
        }

        protected virtual Game StartNewGame(GameTicker.EventDelegate RefreshGame, bool scoring)
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
                NewGame(RefreshGame);
        }

        protected bool QuitPrompt()
        {
            if (AskQuit())
            {
                bool retVal = false;
                bool wasPaused = GameForm.game.Paused;
                GameForm.game.Paused = true;
                if (MessageBox.Show("Are you sure you want to quit the current game?", "Quit",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    retVal = true;
                GameForm.game.Paused = wasPaused;
                return retVal;
            }
            return true;
        }

        protected virtual bool AskQuit()
        {
            return ( game.Running && game.Scoring );
        }

        protected virtual void RefreshGame()
        {
            Invalidate(ClientRectangle, false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (game != null)
                game.Draw(e.Graphics);
            base.OnPaint(e);
        }

        private void scoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HighScores.ShowScores();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !QuitPrompt();
            if (!e.Cancel)
                lock (game.gameTicker)
                {
                    //clean up
                    game.gameTicker.End();
                }
        }
    }
}
