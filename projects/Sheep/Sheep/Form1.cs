using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using BaseForm = MattUtil.RealTimeGame.GameForm;
using BaseGame = MattUtil.RealTimeGame.Game;

namespace Sheep
{
    public partial class Form1 : BaseForm
    {
        private const int width = 780, height = 650;

        public Form1()
        {
            InitializeComponent();
            game = GetNewGame(false);
            game.Start();
            SetSize();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (game != null)
            //    ((Game)game).SetTarget(e.X, e.Y - this.menuStrip.Height);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            SetSize();
        }
        private void SetSize()
        {
            this.ClientSize = new Size(width, height + this.menuStrip.Height);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (game != null)
            {
                ((Game)game).SetTarget(e.X, e.Y - this.menuStrip.Height);
                if (!game.Started)
                    //    game.Paused = !game.Paused;
                    //else
                    game.Start();
            }
        }

        protected override BaseGame GetNewGame(bool scoring)
        {
            return new Game(scoring, this.RefreshGame, width, height, this.menuStrip.Height);
        }
    }
}
