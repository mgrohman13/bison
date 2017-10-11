using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MattUtil;
using MattUtil.RealTimeGame;
using Point = System.Drawing.Point;
using BaseForm = MattUtil.RealTimeGame.GameForm;
using BaseGame = MattUtil.RealTimeGame.Game;

namespace Gravity
{
    public partial class Form1 : BaseForm
    {
        public Form1()
        {
            InitializeComponent();
            this.ClientSize = new Size(300, 300);

            game = GetNewGame(false);
        }

        protected override BaseGame GetNewGame(bool scoring)
        {
            return ( game = new Game(1000 / 39f, base.RefreshGame, this.ClientRectangle) );
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (game != null)
            {
                ( (Game)game ).setTarget(e.X, e.Y);
                if (!game.Started)
                    game.Start();
            }
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            if (game != null)
            {
                ( (Game)game ).setClientRectangle(this.ClientRectangle);
                Invalidate(ClientRectangle, false);

                Text = ClientSize.ToString();
            }
        }
    }
}
