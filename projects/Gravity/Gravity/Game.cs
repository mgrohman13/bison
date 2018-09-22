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
        public const int dTime = 1800;
        const float powerUpChance = .001f;

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
        private float difficulty, dMeter, dInc, dTick;

        private Rectangle drawRectangle;

        public Game(bool scoring, float gameTick, MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame) : base(gameTick, RefreshGame)
        {
            this.Scoring = scoring;
            this.score = 0;
            this.difficulty = 1f;
            this.dMeter = -2f;
            this.dInc = difficulty;
            this.dTick = rand.Round(dTime / 2f);

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

            Func<bool, bool> none = ( type => !pieces.OfType<PowerUp>().Where(pu => ( type == pu.Type )).Any() );
            while (none(true) || none(false) || rand.Bool())
                NewPowerUp(1.05f);

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

        internal void AddScore(float amt, Piece p)
        {
            float mult = 100f * scoreSize / avgSize;
            AddMeter(amt * avgSize / scoreSize / 3f);

            int old = (int)Math.Round(this.score);
            this.score += (decimal)amt * 100m;
            int text = (int)Math.Round(this.score) - old;
            AddText(p, "+" + text.ToString(), text / mult, true, false, true);
        }
        internal void ScoreMeter(float amt, Piece p)
        {
            float old = this.dMeter;
            AddMeter((float)( -Math.Sqrt(amt * avgSize / scoreSize) ));
            float text = old - this.dMeter;
            AddText(p, "+" + rand.Round(text * 100f).ToString(), text, false, true, false);
        }
        internal void AddText(Piece p, string text, float sizeFactor, bool r, bool g, bool b)
        {
            pieces.Add(new Text(this, p.X, p.Y, text, (float)( 10f * Math.Pow(sizeFactor, .15f) ), r, g, b));
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
        private void NewPowerUp(float div)
        {
            const float dist = gameSize / 7.5f;
            if (rand.Bool())
            {
                PowerUp pu = new PowerUp(this, false, rand.Gaussian(dist), rand.Gaussian(dist), rand.GaussianOE(scoreSize, .1f, .15f), rand.OE(rand.Weighted(scoreDensity)));
                this.pieces.Add(pu);
                AddMeter((float)( Math.Sqrt(pu.Size / scoreSize) / div ));
            }
            if (rand.Bool())
            {
                PowerUp su = new PowerUp(this, true, rand.Gaussian(dist), rand.Gaussian(dist), rand.GaussianOE(scoreSize, .1f, .15f), rand.OE(rand.Weighted(scoreDensity)));
                this.pieces.Add(su);
            }
        }
        private void AddMeter(float amt)
        {
            bool neg = ( amt < 0 );
            if (neg)
                amt = -amt;
            else
                amt *= .975f;
            amt = rand.GaussianOE(amt, .25f, .15f);
            if (neg)
                amt = -amt;
            this.dMeter += amt;
        }

        public static float GetDensity(float size)
        {
            return rand.GaussianOE((float)Math.Sqrt(size / avgSize), .15f, .35f);
        }

        public float DifficultyMeter
        {
            get
            {
                return dMeter;
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
                return "inf.dat";
            }
        }


        private static Font info = new Font("Lucida Console", 15f);
        public override void Draw(Graphics graphics)
        {
            //graphics.DrawString(( this.difficulty * 100f ).ToString("0"), info, Brushes.Black, 0, 25);
            //graphics.DrawString(( this.dMeter * 100f ).ToString("0"), info, Brushes.Black, 0, 45);

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
                if (!( pieces[a] is Text ))
                    for (int b = a + 1 ; b < pieces.Count && ( exists = this.pieces.Contains(pieces[a]) ) ; ++b)
                        if (!( pieces[b] is Text ) && this.pieces.Contains(pieces[b]))
                            pieces[a].Interact(pieces[b]);
                if (exists && this.pieces.Contains(pieces[a]))
                    pieces[a].Step(enemyCount);
            }

            if (ExistChance(enemyCount, (float)( Math.Pow(Difficulty, .75f) * Game.enemyMult ), 1))
                NewEnemy();


            if (--this.dTick < 0)
            {
                float diff = this.difficulty - this.dInc;
                AddText(player, "-" + rand.Round(diff * 500f).ToString(), diff * 5f, true, true, true);
                this.dInc = this.difficulty;
                this.dTick = Game.dTime;
            }

            float factor = 1f;
            if (this.dMeter > 0)
                factor *= ( 2f + dMeter ) / 2f;
            else
                factor /= ( 2f - dMeter );

            this.difficulty += rand.OE(factor * .00125f / ( difficulty + 1f ));
            //Console.WriteLine(difficulty);

            float f = ( this.dMeter > 0 ? .0005f : .0001f );
            this.dMeter *= ( 1 - f );

            float pc = (float)( Game.powerUpChance * Math.Sqrt(Difficulty) );
            if (rand.Bool(pc * 2f))
                NewPowerUp(3f);
            AddMeter(rand.OE(pc / 3f));
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
