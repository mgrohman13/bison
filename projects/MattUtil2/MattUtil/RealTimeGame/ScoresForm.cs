using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MattUtil.RealTimeGame
{
    public partial class ScoresForm : Form
    {
        internal ScoresForm(HighScores scores)
        {
            InitializeComponent();

            decimal total = scores.total;
            int games = scores.games;
            List<HighScoreEntry> highScores = scores.highScores;

            //put controls into arrays for easy use
            Label[] Names = new Label[] { lblP1, lblP2, lblP3, lblP4, lblP5, lblP6, lblP7, lblP8, lblP9, lblP10 };
            Label[] Scores = new Label[] { lblS1, lblS2, lblS3, lblS4, lblS5, lblS6, lblS7, lblS8, lblS9, lblS10 };

            //fill with high scores
            for (int i = 0 ; i < 10 ; ++i)
            {
                if (i < highScores.Count)
                {
                    Names[i].Text = highScores[i].name;
                    Scores[i].Text = ( (int)( highScores[i].score + .5m ) ).ToString();
                    ++games;
                    total += highScores[i].score;
                }
                else
                    break;
            }

            //show average score
            if (games == 0)
                this.lblAvg.Text = "0";
            else
                this.lblAvg.Text = ( (int)( total / games + .5m ) ).ToString();

            RefreshScoringText();
        }

        void RefreshScoringText()
        {
            this.btnScoring.Text = Scoring ? "Disable Scoring" : "Enable Scoring";
            this.btnScoring.Visible = !GameForm.Game.Running;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the high scores?", this.btnClear.Text,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                HighScores.ClearScores();
                this.Close();
            }
        }

        public static bool Scoring = true;

        void btnScoring_Click(object sender, EventArgs e)
        {
            if (!GameForm.Game.Running)
                Scoring = !Scoring;
            RefreshScoringText();
        }
    }
}
