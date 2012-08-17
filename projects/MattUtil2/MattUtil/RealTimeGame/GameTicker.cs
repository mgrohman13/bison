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

        private Game game;
        private Timer timer;

        private EventDelegate Refresh;

        private bool paused, started, running;

        public GameTicker(Game game, int GameTick, EventDelegate Refresh)
        {
            this.game = game;

            this.GameTick = GameTick;

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
            Thread refresh = new Thread(RefreshGame);
            refresh.IsBackground = true;
            refresh.Start();

            Thread tick = new Thread(RunGame);
            tick.Priority = ThreadPriority.AboveNormal;
            tick.IsBackground = true;
            tick.Start();
        }

        private void RunGame()
        {
            Stopwatch stopwatch = new Stopwatch();
            double offset = GameTick;
            //WriteLine(offset);

            lock (this)
                stopwatch.Start();

            while (Running)
            {
                //int millisecondsTimeout = -1;

                lock (this)
                {
                    if (( !paused || game.GameOver() ) && started)
                    {
                        //WriteLine("   step " + Environment.TickCount);
                        if (game.GameOver())
                            End();
                        else
                            game.Step();
                        //WriteLine("endstep " + Environment.TickCount);

                        if (Running)
                        {
                            long timeDiff = stopwatch.ElapsedTicks;
                            stopwatch.Restart();

                            offset += GameTick - timeDiff / TicksPerMilisecond;
                            //WriteLine(offset);
                            //millisecondsTimeout = (int)offset;
                            //offset = ( offset - GameTick ) * ( 1 - 1 / GameTick ) + GameTick;
                        }
                    }
                    else
                    {
                        stopwatch.Restart();

                        offset = GameTick;
                        //WriteLine(offset);
                        //millisecondsTimeout = GameTick;
                    }
                }

                if (offset > 0)
                {
                    lock (timer)
                        Monitor.Pulse(timer);

                    //WriteLine("sleep:" + (int)offset);
                    System.Threading.Thread.Sleep((int)offset);
                }
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
                    Thread.Sleep(26);

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

        private void RefreshGame()
        {
            while (Running)
            {
                //WriteLine("   wait " + Environment.TickCount);
                lock (timer)
                    Monitor.Wait(timer);
                //WriteLine("endwait " + Environment.TickCount);

                Refresh();

                //WriteLine("   sleep" + Environment.TickCount);
                Thread.Sleep(GameTick);
                //WriteLine("endsleep" + Environment.TickCount);
            }
        }

        //private int index = -1;
        //private string[] output = new string[10000];
        //private void WriteLine(object s)
        //{
        //    if (++index >= 10000)
        //    {
        //        foreach (string s2 in output)
        //            Console.WriteLine(s2);
        //        index = 0;
        //    }
        //    output[index] = s.ToString();
        //}

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
