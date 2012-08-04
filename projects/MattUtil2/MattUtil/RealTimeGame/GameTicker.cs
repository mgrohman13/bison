using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MattUtil.RealTimeGame
{
    public class GameTicker
    {
        public static readonly float TicksPerMilisecond = Stopwatch.Frequency / 1000f;

        public delegate void EventDelegate();

        private int GameTick;
        private int MaxConsecutive;
        private int MaxConsecutiveSleepTime;
        private int Framerate;

        private Game game;
        private Thread thread;
        private Timer timer;

        private EventDelegate Refresh;

        private bool paused, started, running;

        public GameTicker(Game game, int GameTick, int Framerate, EventDelegate Refresh)
        {
            this.game = game;

            this.GameTick = GameTick;
            this.Framerate = Framerate;
            MaxConsecutive = (int)( .5f + Framerate / (float)GameTick );
            MaxConsecutiveSleepTime = (int)( .5f + Framerate / 1.69 );

            paused = false;
            started = false;
            running = false;

            this.Refresh = Refresh;

            timer = new Timer();
        }

        public bool Running
        {
            get
            {
                return running;
            }
            set
            {
                running = value;
                timer.Enabled = running;
            }
        }

        public bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                if (started && !game.GameOver())
                    paused = value;
            }
        }

        public bool Started
        {
            get
            {
                return started;
            }
            set
            {
                started = value;
            }
        }

        public void Start()
        {
            timer.Dispose();
            timer = new Timer();
            timer.Tick += new EventHandler(RefreshGame);
            timer.Interval = Framerate;
            timer.Start();

            thread = new Thread(RunGame);
            thread.Start();
        }

        private void RunGame()
        {
            Stopwatch stopwatch = new Stopwatch();
            float steps = int.MinValue, consecutiveCount = int.MinValue;

            while (Running)
            {
                int millisecondsTimeout = -1;
                lock (this)
                {
                    if (!Running)
                        break;

                    if (( !paused || game.GameOver() ) && started)
                    {
                        if (game.GameOver())
                            End();
                        else
                            game.Step();

                        if (consecutiveCount < 0)
                            consecutiveCount = 0;
                        else if (steps < 0)
                            steps = 0;
                    }

                    if (Running)
                    {
                        if (steps < 0)
                        {
                            System.Threading.Thread.Sleep(GameTick);
                            if (started)
                            {
                                stopwatch.Reset();
                                stopwatch.Start();
                            }
                        }
                        else
                        {
                            int timeDiff;
                            //long ticks = stopwatch.ElapsedTicks;
                            if (( timeDiff = ( (int)Math.Round(++steps * GameTick - stopwatch.ElapsedTicks / TicksPerMilisecond) ) ) > 0)
                            {
                                millisecondsTimeout = timeDiff;
                                consecutiveCount = 0;
                            }
                            else if (++consecutiveCount > MaxConsecutive)
                            {
                                millisecondsTimeout = MaxConsecutiveSleepTime;
                                consecutiveCount = 0;
                            }
                        }
                    }
                }
                if (millisecondsTimeout > -1)
                    System.Threading.Thread.Sleep(millisecondsTimeout);
            }

            stopwatch.Stop();
        }

        public void End()
        {
            if (Running)
            {
                Running = false;

                //get critical information while we still have the lock
                bool saveScore = GameForm.Game.Scoring;
                decimal newScore = game.Score;

                //release the lock so that the form can draw everything on the refresh
                Monitor.Exit(this);
                try
                {
                    Refresh();
                    Thread.Sleep(Framerate);

                    //we dont need to be locked for this
                    if (saveScore)
                        HighScores.SaveScore(true, newScore);
                }
                finally
                {
                    //reaquire the lock since were inside a lock block
                    Monitor.Enter(this);
                }
            }
        }

        private void RefreshGame(object sender, EventArgs e)
        {
            Refresh();
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
