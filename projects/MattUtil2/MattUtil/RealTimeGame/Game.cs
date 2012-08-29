using System;
using System.Collections.Generic;
using System.Text;

namespace MattUtil.RealTimeGame
{
    public abstract class Game
    {
        public static readonly MTRandom Random;

        static Game()
        {
            Random = new MTRandom();
            Random.StartTick();
        }

        protected internal GameTicker gameTicker;

        private bool scoring;

        public Game(double gameTick, GameTicker.EventDelegate RefreshGame)
        {
            gameTicker = new GameTicker(this, gameTick, RefreshGame);
            Started = false;
            Running = true;
        }

        public void Start()
        {
            Started = true;
            Paused = false;
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
        public virtual bool Paused
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
        public double GameTick
        {
            get
            {
                return gameTicker.GameTick;
            }
            protected set
            {
                gameTicker.GameTick = value;
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

        protected internal abstract void OnEnd();
    }
}
