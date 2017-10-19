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

            this.player = new Player(this, width / 2f, height * .91f);
            this.target = new Target(this, width / 2f, height / 2f);
            this.sheeps = new HashSet<Sheep>();
            this.wolves = new HashSet<Wolf>();

            for (int a = 0; a < 13; ++a)
                sheeps.Add(new Sheep(this, rand.Gaussian(width / 2f, .13f), rand.Gaussian(height * .78f, .065f)));
        }

        public Player Player
        {
            get
            {
                return player;
            }
        }
        public Target Target
        {
            get
            {
                return target;
            }
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
            target.Draw(graphics, yOffset);
            foreach (Sheep sheep in sheeps)
                sheep.Draw(graphics, yOffset);
            foreach (Wolf wolf in wolves)
                wolf.Draw(graphics, yOffset);
            player.Draw(graphics, yOffset);
        }

        public override bool GameOver()
        {
            return (sheeps.Count == 0);
        }

        public override void Step()
        {
            float sheepX = sheeps.Average(s => s.X);
            float sheepY = sheeps.Average(s => s.Y);

            player.Step();
            target.Step();
            foreach (Sheep sheep in sheeps)
                sheep.Step(sheepX, sheepY);
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
        internal static Image LoadImage(string name, int x, int y, int width, int height)
        {
            Bitmap src = new Bitmap(PicLocation + name);
            Rectangle cropRect = new Rectangle(x, y, width, height);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            };
            return SetTransparentBackground(target);
        }
        internal static Image SetTransparentBackground(Bitmap image)
        {
            image.MakeTransparent(Color.FromArgb(163, 73, 164));
            return image;
        }
    }
}
