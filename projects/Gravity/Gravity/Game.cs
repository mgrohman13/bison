using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MattUtil;

namespace Gravity
{
    class Game : MattUtil.RealTimeGame.Game
    {
        public static MTRandom rand = new MTRandom();

        public const float gameSize = 1500f;
        public const float gravity = .5f;
        public const float offMapPull = .00001f;

        private HashSet<Piece> pieces;
        private Player player;
        private Target target;

        private Rectangle drawRectangle;

        public Game(double gameTick, MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame, Rectangle rectangle) : base(gameTick, RefreshGame)
        {
            drawRectangle = rectangle;

            const float playerDist = gameSize / 10f;
            player = new Player(rand.Gaussian(playerDist), rand.Gaussian(playerDist), 25f, 1);
            target = new Target(rand.Gaussian(playerDist), rand.Gaussian(playerDist), 15f, 1);

            pieces = new HashSet<Piece>() { new Center(0, 0, 5f, 5f), player, target };

            int amt = rand.GaussianOEInt(5, .1f, .1f, 3);
            for (int a = 0; a < amt; ++a)
            {
                const float enemyDist = gameSize / 5f;
                const float avgSize = 30f;
                float size = rand.GaussianOE(avgSize, .2f, .3f, 5);
                pieces.Add(new Enemy(rand.Gaussian(enemyDist), rand.Gaussian(enemyDist), size, rand.GaussianOE(size / avgSize, .2f, .3f)));
            }
        }

        public override decimal Score
        {
            get
            {
                return 0;
            }
        }

        public override string ScoreFile
        {
            get
            {
                return "inf.dat";
            }
        }

        public override void Draw(Graphics graphics)
        {
            foreach (Piece p in pieces)
                p.Draw(graphics, drawRectangle, gameSize, gameSize);
        }

        public override bool GameOver()
        {
            return false;
        }

        public override void Step()
        {
            List<Piece> pieces = new List<Piece>(rand.Iterate(this.pieces));
            for (int a = 0; a < pieces.Count; ++a)
            {
                for (int b = a + 1; b < pieces.Count; ++b)
                    pieces[a].Interact(pieces[b]);
                pieces[a].Step();
            }
        }

        protected override void OnEnd()
        {
        }

        internal void setTarget(int x, int y)
        {
            target.setTarget(x * gameSize / drawRectangle.Width - gameSize / 2f, y * gameSize / drawRectangle.Height - gameSize / 2f);
        }

        internal void setClientRectangle(Rectangle rectangle)
        {
            drawRectangle = rectangle;
        }
    }
}
