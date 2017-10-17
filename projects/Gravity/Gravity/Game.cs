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
        public const float offMapPull = .0025f;
        public const float avgSize = 30f;
        public const float scoreSize = avgSize * 1.75f;
        public const float scoreDensity = .25f;
        public const float enemyMult = 5f;

        private HashSet<Piece> pieces;
        private Player player;
        private Target target;

        private decimal score;
        private float difficulty;

        private Rectangle drawRectangle;

        public Game(bool scoring, float gameTick, MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame) : base(gameTick, RefreshGame)
        {
            this.Scoring = scoring;
            this.score = 0;
            this.difficulty = 1f;

            const float playerDist = gameSize / 10f;
            player = new Player(this, rand.Gaussian(playerDist), rand.Gaussian(playerDist), 25f, 1f);
            target = new Target(this, rand.Gaussian(playerDist), rand.Gaussian(playerDist), 15f, 1.125f);
            Piece center = new Center(this, 0, 0, 5f, 10f);

            //Console.WriteLine(Enemy.GetColor(player.Size, player.Density / 2f));
            //Console.WriteLine(Enemy.GetColor(target.Size, target.Density / 4f));
            //Console.WriteLine(Enemy.GetColor(center.Size, center.Density * 2f));

            pieces = new HashSet<Piece>() { center, player, target };

            int amt = rand.GaussianOEInt(Game.enemyMult, .1f, .1f, 3);
            for (int a = 0 ; a < amt ; ++a)
                NewEnemy();

            NewPowerUp();
            NewPowerUp();

            Step();
        }

        public Player Player
        {
            get
            {
                return player;
            }
        }

        public float Difficulty
        {
            get
            {
                return difficulty;
            }
        }

        internal void AddScore(float score)
        {
            this.score += (decimal)score * 100m;
        }

        public void NewEnemy()
        {
            const float enemyDist = gameSize / 5f;
            float size = rand.GaussianOE(avgSize, .25f, .25f, 5);
            float x = rand.Gaussian(enemyDist);
            float y = rand.Gaussian(enemyDist);
            if (player.CheckCourse(x - size / 2f, y - size / 2f, size))
                NewEnemy();
            else
                pieces.Add(rand.Bool(Difficulty / ( Difficulty + 2.5f )) ? new Danger(this, x, y, size, GetDensity(size)) : new Enemy(this, x, y, size, GetDensity(size)));
        }
        private void NewPowerUp()
        {
            const float dist = gameSize / 7.5f;
            this.pieces.Add(new PowerUp(this, rand.Gaussian(dist), rand.Gaussian(dist), rand.GaussianOE(scoreSize, .1f, .15f), rand.OE(rand.Weighted(scoreDensity))));
        }

        public static float GetDensity(float size)
        {
            return rand.GaussianOE((float)Math.Sqrt(size / avgSize), .15f, .35f);
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
                return "inf.dat";
            }
        }

        public override void Draw(Graphics graphics)
        {
            foreach (Piece p in rand.Iterate(pieces))
                p.Draw(graphics, drawRectangle);
        }

        public override bool GameOver()
        {
            return ( player.Shield <= 0 );
        }

        public override void Step()
        {
            List<Piece> pieces = new List<Piece>(rand.Iterate(this.pieces));
            int enemyCount = pieces.OfType<Enemy>().Count();
            for (int a = 0 ; a < pieces.Count ; ++a)
            {
                bool exists = true;
                for (int b = a + 1 ; b < pieces.Count && ( exists = this.pieces.Contains(pieces[a]) ) ; ++b)
                    if (this.pieces.Contains(pieces[b]))
                        pieces[a].Interact(pieces[b]);
                if (exists && this.pieces.Contains(pieces[a]))
                    pieces[a].Step(enemyCount);
            }

            if (ExistChance(enemyCount, (float)( Math.Pow(Difficulty, .75f) * Game.enemyMult ), 1))
                NewEnemy();

            if (rand.Bool(.0015f * Math.Sqrt(Difficulty)))
                NewPowerUp();

            this.difficulty += rand.OE(.0005f / ( difficulty + 1f ));
            //Console.WriteLine(difficulty);
        }


        public static bool ExistChance(float value, float target, float power)
        {
            return rand.Bool(.01 * Math.Pow(target / ( target + value ), 5 * power));
        }

        protected override void OnEnd()
        {
        }

        internal void Remove(Piece piece)
        {
            Console.WriteLine("remove " + piece);
            pieces.Remove(piece);
        }

        internal void setTarget(int x, int y)
        {
            target.setTarget(( x - drawRectangle.X ) * gameSize / drawRectangle.Width - gameSize / 2f, ( y - drawRectangle.Y ) * gameSize / drawRectangle.Height - gameSize / 2f);
        }

        internal void setClientRectangle(Rectangle rectangle)
        {
            drawRectangle = rectangle;
        }
    }
}
