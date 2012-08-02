using System;
using System.Collections.Generic;
using System.Text;

namespace MattUtil.RealTimeGame
{
    public abstract class Game
    {
        public static MTRandom Random;

        static Game()
        {
            Random = new MTRandom();
            Random.StartTick();
        }

        public MattUtil.RealTimeGame.GameTicker gameTicker;

        private bool scoring;

        public Game(int GameTick, int Framerate, GameTicker.EventDelegate RefreshGame)
        {
            gameTicker = new GameTicker(this, GameTick, Framerate, RefreshGame);
            //start game
            gameTicker.Start();
        }

        public bool Running
        {
            get
            {
                return gameTicker.Running;
            }
            set
            {
                gameTicker.Running = value;
            }
        }
        public bool Paused
        {
            get
            {
                return gameTicker.Paused;
            }
            set
            {
                gameTicker.Paused = value;
            }
        }
        public bool Scoring
        {
            get
            {
                return scoring;
            }
            protected set
            {
                scoring = value;
            }
        }
        public bool Started
        {
            get
            {
                return gameTicker.Started;
            }
            set
            {
                gameTicker.Started = value;
            }
        }

        public abstract decimal Score
        {
            get;
        }
        public abstract String ScoreFile
        {
            get;
        }
        public abstract bool GameOver();

        public abstract void Step();

        public abstract void Draw(System.Drawing.Graphics graphics);
    }
}
