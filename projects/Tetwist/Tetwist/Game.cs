using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using FTimer = System.Windows.Forms.Timer;

namespace Tetwist
{
    static class Game
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Random.StartTick();

            timer = new FTimer();
            timer.Interval = 1;
            timer.Tick += new EventHandler(timer_Tick);

            watch = new Stopwatch();

            //moveThread = new Thread(Move);
            //moveThread.IsBackground = true;

            music = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Tetwist();
            Application.Run(form);

            timer.Stop();
            timer.Dispose();
            watch.Stop();
            form.Dispose();
            Random.Dispose();
        }

        public const int Width = 8;
        public const int Height = 13;
        public const double StartSpeed = 666;
        public const double SpeedIncMult = .87;
        public const double SpeedDev = .03;
        public const double LevelMult = 250;
        public const double LevelDeviation = 6.66;
        public const double LineScore = Width;
        public const double LineScoreMult = .13;
        public const double LevelScore = 169;
        public const double LevelScoreMult = .0039;
        public const double DropScoreMult = 1.3;
        public const double ExplosionScoreMult = 1;
        public const double SpeedInburseVal = 130000;
        public const double TimeBonus = 666000;
        public const double BlockValSize = 3.5;
        public const double BlockValDev = .06;
        public const double BlockOESize = .5;
        public const int NewBlockWaitTime = 3;
        public const int MaxSolidifyTimes = 2;

        public static MattUtil.MTRandom Random = new MattUtil.MTRandom();
        static Tetwist form;
        static FTimer timer;
        static Stopwatch watch;
        static readonly double TicksPerMilisecond = Stopwatch.Frequency / 1000.0;

        static bool paused = false;
        public static bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
            }
        }
        static bool music;
        public static bool Music
        {
            get
            {
                return music;
            }
            set
            {
                music = value;
            }
        }
        public static bool Running
        {
            get
            {
                return timer.Enabled;
            }
        }

        public static int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
                CheckLevel();
            }
        }
        static int CurSpeed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
                Score += Random.Round(SpeedInburseVal / value);
                Tetwist.levelTimer = Game.Random.Round(3333.0 / CurSpeed);
            }
        }

        public static int Level
        {
            get
            {
                return curLevel;
            }
        }
        public static int Next
        {
            get
            {
                return nextLevel;
            }
        }
        public static int Lines
        {
            get
            {
                return numLines;
            }
        }

        static int _speed, _score, curLevel, nextLevel, numLines, solidifyingCount, iterationCount;
        static bool gotcommand, solidifying;
        public static bool Solidifying
        {
            get
            {
                return solidifying;
            }
            set
            {
                solidifying = value;
                gotcommand = false;

                if (value && ++solidifyingCount > MaxSolidifyTimes)
                {
                    solidifyingCount = 0;
                    SolidifyCurBlock();
                }
            }
        }
        //static bool moveLeft = false, moveRight = false;
        //static object moveLock = new object();
        //static Thread moveThread;

        static Dictionary<Point, Block> _blocks = new Dictionary<Point, Block>();
        static ActiveSet curBlock;

        public static void RemoveBlock(Point p)
        {
            _blocks.Remove(p);
        }
        public static void AddBlock(Point p, Block block)
        {
            _blocks.Add(p, block);
        }
        public static bool HasBlock(Point p)
        {
            return _blocks.ContainsKey(p);
        }
        public static Block GetBlock(Point p)
        {
            return _blocks[p];
        }

        public static void NewGame()
        {
            numLines = 0;
            curLevel = 0;
            _score = 0;
            iterationCount = 0;
            nextLevel = GetNextLevel(curLevel, 0, Score, false);
            CurSpeed = Random.GaussianCappedInt(StartSpeed, SpeedDev, 1);
            curLevel = 1;

            solidifyingCount = 0;
            Solidifying = false;
            gotcommand = false;

            Notifications.NewGame();
            InfectBlock.NewGame();
            _blocks.Clear();
            curBlock = new ActiveSet();

            //try { while (true) { blocks.Add(new Point(Random.Next(Width), Random.Next(Height)), new Block()); } }
            //catch { }

            timer.Enabled = true;
        }

        static void CheckLevel()
        {
            if (Score >= nextLevel)
            {
                nextLevel = GetNextLevel(curLevel, nextLevel, Score, true);
                Score += Random.Round(curLevel * TimeBonus / ( iterationCount * CurSpeed ));
                iterationCount = 0;
                CurSpeed = Random.GaussianCappedInt(_speed * SpeedIncMult, SpeedDev, 1);
                ++curLevel;
            }
        }

        static int GetNextLevel(int curNum, int lastScore, int curScore, bool addScore)
        {
            Block.NewLevel();

            int nextNum = curNum + 1;
            double avg = ( nextNum * nextNum + nextNum ) * LevelMult;
            double old = ( curNum * curNum + curNum ) * LevelMult;
            double diffDiv = avg - old;
            double lastDiff = lastScore - old;
            double avgDev = Math.Abs(lastDiff) / LevelMult + LevelDeviation;

            if (addScore)
                curScore += Random.Round(LevelScore + old * LevelScoreMult);

            int next;
            do
            {
                next = Random.GaussianInt(avg + lastDiff, avgDev / 100.0);

                double diff = avg + lastDiff - curScore;
                if (diff > 0)
                    avg -= diff / diffDiv;
            }
            while (next <= curScore);

            return next;
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            watch.Reset();
            watch.Start();

            if (!Paused)
            {
                Run();
                --Tetwist.levelTimer;
                form.Refresh();
            }

            int oldInterval = timer.Interval;
            int newInterval = _speed - 100;
            if (newInterval < 1)
                newInterval = 1;
            if (oldInterval != newInterval)
                timer.Interval = newInterval;

            watch.Stop();
            int timeDiff = _speed - Random.Round(watch.ElapsedTicks / TicksPerMilisecond) - oldInterval;
            if (timeDiff > 0)
                System.Threading.Thread.Sleep(timeDiff);
        }

        static void Run()
        {
            ++iterationCount;
            Notifications.Notify(Notifications.Type.Iteration, null);

            if (Solidifying)
                if (gotcommand)
                    Solidifying = false;
                else
                    SolidifyCurBlock();
            else
                curBlock.Fall();
        }

        public static void SolidifyCurBlock()
        {
            Solidifying = false;

            ++Score;
            CheckLines();

            curBlock.NotifySolidified();
            Notifications.Notify(Notifications.Type.BlockSet, null);

            curBlock = new ActiveSet();
        }

        static void CheckLines()
        {
            for (int y = Height ; --y > -1 ; )
            {
                if (CheckLine(y, false))
                    ++y;
            }
        }

        public static bool CheckLine(int y)
        {
            return CheckLine(y, true);
        }

        static bool CheckLine(int y, bool checksActive)
        {
            for (int x = Width ; --x > -1 ; )
            {
                Point p;
                if (( !HasBlock(p = new Point(x, y)) ) || ( checksActive && curBlock.Contains(GetBlock(p)) ))
                    return false;
            }

            ClearLine(y);
            return true;
        }

        static void ClearLine(int y)
        {
            ++numLines;
            Score += Random.Round(LineScore + LineScoreMult * numLines);

            //lock (Game.blocks)
            for (int x = Width ; --x > -1 ; )
            {
                GetBlock(new Point(x, y)).Destroy();
            }

            Block.MoveBlocks(_blocks.Values, 0, 1, delegate(Block b)
            {
                return b.Point.Y < y;
            });

            Notifications.Notify(Notifications.Type.LineCleared, y);
        }

        public static void GameOver()
        {
            timer.Enabled = false;
        }

        public static void MoveCurrent(bool left)
        {
            if (Game.Running)
            {
                if (curBlock.Move(left ? -1 : 1, 0))
                {
                    gotcommand = true;
                    form.Refresh();
                }
            }
        }

        //public static void MoveCurrent(bool left, bool down)
        //{
        //    lock (moveLock)
        //    {
        //        if (moveThread.IsAlive)
        //            moveThread.Abort();

        //        if (left)
        //            moveLeft = down;
        //        else
        //            moveRight = down;

        //        if (moveLeft ^ moveRight)
        //        {
        //            moveThread = new Thread(Move);
        //            moveThread.IsBackground = true;
        //            moveThread.Start();
        //        }
        //    }
        //}

        //static void Move()
        //{
        //    if (moveLeft ^ moveRight)
        //    {
        //        lock (moveLock)
        //        {
        //            if (moveLeft ^ moveRight)
        //                ActualMove();
        //            else
        //                return;
        //        }
        //        Thread.Sleep(MoveInitialDelay);

        //        while (moveLeft ^ moveRight)
        //        {
        //            lock (moveLock)
        //            {
        //                if (moveLeft ^ moveRight)
        //                    ActualMove();
        //                else
        //                    return;
        //            }
        //            Thread.Sleep(MoveRepeat);
        //        }
        //    }
        //}

        //static void ActualMove()
        //{
        //    curBlock.Move(moveLeft ? -1 : 1, 0);
        //    form.ThreadSafeRefresh();
        //}

        public static void Rotate()
        {
            if (Game.Running)
            {
                if (curBlock.Rotate())
                {
                    gotcommand = true;
                    form.Refresh();
                }
            }
        }

        public static void Drop()
        {
            if (Game.Running)
            {
                if (curBlock.Drop())
                {
                    gotcommand = true;
                    form.Refresh();
                }
            }
        }

        public static void Draw(Graphics graphics)
        {
            foreach (Point p in _blocks.Keys)
            {
                //if ( blocks[p].HasImage)
                Rectangle r = new Rectangle(p.X * Tetwist.SquareSize,
                    p.Y * Tetwist.SquareSize + Tetwist.StartHeight,
                    Tetwist.SquareSize, Tetwist.SquareSize);
                GetBlock(p).Draw(graphics, r);
            }
        }
    }
}