using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MattUtil.RealTimeGame
{
    public class GameTicker : IDisposable
    {
        public static readonly float TicksPerMilisecond = Stopwatch.Frequency / 1000f;

        public delegate void EventDelegate();

        private double gameTick;

        private Game game;
        private Timer timer;

        private EventDelegate Refresh;

        private bool paused, started, running;

        public GameTicker(Game game, double GameTick, EventDelegate Refresh)
        {
            this.game = game;

            this.gameTick = GameTick;

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

        public double GameTick
        {
            get
            {
                return gameTick;
            }
            internal set
            {
                gameTick = value;
            }
        }

        public void Start()
        {
            Thread refresh = new Thread(RefreshGame);
            refresh.IsBackground = true;
            refresh.Start();

            Thread tick = new Thread(RunGame);
            tick.IsBackground = true;
            tick.Start();
        }

        //Stopwatch stopwatch = new Stopwatch();
        private void RunGame()
        {
            Stopwatch stopwatch = new Stopwatch();
            long ticks = 0;
            double offset = 0;

            lock (this)
                stopwatch.Start();

            while (Running)
            {
                lock (this)
                {
                    bool gameOver = game.GameOver();
                    if (( !paused || gameOver ) && started)
                    {
                        //WriteLine("   step " + stopwatch.ElapsedMilliseconds);
                        if (gameOver)
                            End();
                        else
                            game.Step();
                        //WriteLine("endstep " + stopwatch.ElapsedMilliseconds);

                        if (Running)
                        {
                            long elapsedTicks = stopwatch.ElapsedTicks;
                            long timeDiff = elapsedTicks - ticks;
                            ticks = elapsedTicks;

                            offset += gameTick - timeDiff / TicksPerMilisecond;
                        }
                    }
                    else
                    {
                        ticks = 0;
                        offset = gameTick;

                        stopwatch.Restart();
                    }
                }

                lock (timer)
                    Monitor.Pulse(timer);

                int sleep = Game.Random.Round(Math.Max(0.0, ( offset + gameTick ) / 2.0));
                //WriteLine("sleep:" + sleep);
                Thread.Sleep(sleep);
            }

            lock (this)
                stopwatch.Stop();
        }

        public void End()
        {
            if (Running)
            {
                game.OnEnd();

                Running = false;

                //get critical information while we still have the lock
                bool saveScore = game.Scoring;
                decimal newScore = game.Score;

                //release the lock so that the form can draw everything on the refresh
                Monitor.Exit(this);
                try
                {
                    Refresh();

                    Thread.Sleep(Game.Random.Round(gameTick));

                    //we don't need to be locked for this
                    if (saveScore)
                        HighScores.SaveScore(game, true, newScore);

                    Thread.Sleep(Game.Random.Round(gameTick));
                }
                finally
                {
                    //reaquire the lock since we're inside a lock block
                    Monitor.Enter(this);
                }
            }
        }

        private void RefreshGame()
        {
            while (Running)
            {
                //WriteLine("   wait " + stopwatch.ElapsedMilliseconds);
                lock (timer)
                    Monitor.Wait(timer);
                //WriteLine("endwait " + stopwatch.ElapsedMilliseconds);

                Refresh();
            }
        }

        //private int index = -1;
        //private string[] output = new string[10000];
        //private void WriteLine(object s)
        //{
        //    lock (output)
        //    {
        //        if (++index >= 10000)
        //        {
        //            foreach (string s2 in output)
        //                Console.WriteLine(s2);
        //            index = 0;
        //        }
        //        output[index] = s.ToString();
        //    }
        //}

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
