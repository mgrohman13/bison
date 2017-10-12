﻿using System;
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
        public const float scoreSize = 50f;
        public const float scoreDensity = .25f;
        public const float avgNum = 5f;

        private HashSet<Piece> pieces;
        private Player player;
        private Target target;

        private decimal score;

        private Rectangle drawRectangle;

        public Game(double gameTick, MattUtil.RealTimeGame.GameTicker.EventDelegate RefreshGame, Rectangle rectangle) : base(gameTick, RefreshGame)
        {
            drawRectangle = rectangle;

            const float playerDist = gameSize / 10f;
            player = new Player(this, rand.Gaussian(playerDist), rand.Gaussian(playerDist), 25f, 1f);
            target = new Target(this, rand.Gaussian(playerDist), rand.Gaussian(playerDist), 15f, 1f);
            Piece center = new Center(this, 0, 0, 5f, 10f);

            Console.WriteLine(Enemy.GetColor(player.Size, player.Density / 2f));
            Console.WriteLine(Enemy.GetColor(target.Size, target.Density / 4f));
            Console.WriteLine(Enemy.GetColor(center.Size, center.Density * 2f));

            pieces = new HashSet<Piece>() { center, player, target };

            int amt = rand.GaussianOEInt(avgNum, .1f, .1f, 3);
            for (int a = 0; a < amt; ++a)
                NewEnemy();

            NewPowerUp();
            NewPowerUp();

            this.score = 0;
        }

        internal void AddScore(float score)
        {
            this.score += (decimal)score;
        }

        public void NewEnemy()
        {
            const float enemyDist = gameSize / 5f;
            float size = rand.GaussianOE(avgSize, .25f, .25f, 5);
            pieces.Add(new Enemy(this, rand.Gaussian(enemyDist), rand.Gaussian(enemyDist), size, GetDensity(size)));
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
                if (this.pieces.Contains(pieces[a]))
                {
                    for (int b = a + 1; b < pieces.Count; ++b)
                        if (this.pieces.Contains(pieces[b]))
                            pieces[a].Interact(pieces[b]);
                    pieces[a].Step(pieces.Count);
                }

            if (ExistChance(avgNum / (avgNum + this.pieces.OfType<Enemy>().Count())))
                NewEnemy();

            if (rand.Bool(.0015f))
                NewPowerUp();
        }


        public static bool ExistChance(float pct)
        {
            return rand.Bool(.01 * Math.Pow(pct, 5));
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
            target.setTarget(x * gameSize / drawRectangle.Width - gameSize / 2f, y * gameSize / drawRectangle.Height - gameSize / 2f);
        }

        internal void setClientRectangle(Rectangle rectangle)
        {
            drawRectangle = rectangle;
        }
    }
}
