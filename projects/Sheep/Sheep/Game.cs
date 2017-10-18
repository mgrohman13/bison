using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MattUtil;
using MattUtil.RealTimeGame;

namespace Sheep
{
    class Game : MattUtil.RealTimeGame.Game
    {
        public static MTRandom rand = new MTRandom();

        private const string PicLocation = "..\\..\\..\\pics\\";

        private int score;
        private float width, height, yOffset;

        private Player player;
        private Target target;
        private HashSet<Sheep> sheeps;
        private HashSet<Wolf> wolves;

        public Game(bool scoring, GameTicker.EventDelegate RefreshGame, float width, float height, float yOffset) : base(1000 / 52f, RefreshGame)
        {
            this.Scoring = scoring;
            this.score = 0;
            this.width = width;
            this.height = height;
            this.yOffset = yOffset;

            this.player = new Player(width / 2f, height * .91f);
            this.target = new Target(width / 2f, height / 2f);
            this.sheeps = new HashSet<Sheep>();
            this.wolves = new HashSet<Wolf>();

            for (int a = 0 ; a < 13 ; ++a)
                sheeps.Add(new Sheep(rand.Gaussian(width / 2f, .13f), rand.Gaussian(height * .78f, .065f)));
        }

        public override decimal Score
        {
            get
            {
                return score;
            }
        }

        public override string ScoreFile
        {
            get
            {
                return PicLocation + "pics.dat";
            }
        }

        public void SetTarget(float x, float y)
        {
            target.SetTarget(x, y);
        }

        public override void Draw(Graphics graphics)
        {
            player.Draw(graphics, yOffset);
            target.Draw(graphics, yOffset);
            foreach (Sheep sheep in sheeps)
                sheep.Draw(graphics, yOffset);
            foreach (Wolf wolf in wolves)
                wolf.Draw(graphics, yOffset);
        }

        public override bool GameOver()
        {
            return ( sheeps.Count == 0 );
        }

        public override void Step()
        {
            player.Step();
            target.Step();
            foreach (Sheep sheep in sheeps)
                sheep.Step();
            foreach (Wolf wolf in wolves)
                wolf.Step();
        }

        protected override void OnEnd()
        {
        }

        internal static Image LoadImage(string name)
        {
            return SetTransparentBackground(new Bitmap(PicLocation + name));
        }
        internal static Image SetTransparentBackground(Bitmap image)
        {
            image.MakeTransparent(Color.FromArgb(163, 73, 164));
            return image;
        }
    }
}
