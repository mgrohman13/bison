﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using MattUtil;

namespace RandomWalk
{
    public class Walk
    {
        public static MTRandom rand;
        static Walk()
        {
            rand = new MTRandom();
            rand.StartTick();
        }

        private double x, y;
        private double interval;
        private Thread thread;
        private Action Invalidate;
        private List<PointD> points;

        public Color Color
        {
            get;
            set;
        }
        public int Size
        {
            get;
            set;
        }
        public bool SingleDimension
        {
            get;
            set;
        }
        public double Tension
        {
            get;
            set;
        }
        public double Deviation
        {
            get;
            set;
        }
        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
                Stop();
                Start();
            }
        }
        public double IntDevPct
        {
            get;
            set;
        }
        public double IntOePct
        {
            get;
            set;
        }
        public List<PointD> Points
        {
            get
            {
                lock (this)
                    return points.ToList();
            }
        }

        public Walk(Action Invalidate, Color color, int size, bool singleDimension, double tension, double deviation, double interval, double intDevPct, double intOePct)
        {
            string info = string.Format("color:{0}\tsize:{1}\tsingleDimension:{2}\ttension:{7}\tdeviation:{3}\tinterval:{4}\tIntDevPct:{5}\tIntOePct:{6}",
                    color, size, singleDimension, deviation, interval, intDevPct, intOePct, tension);
            Console.WriteLine(info);
            string today = DateTime.Now.ToString().Replace('/', '_').Replace(":", "_");
            using (var fileStream = new System.IO.StreamWriter("walks_" + today + ".txt", true))
            {
                fileStream.WriteLine(info);
                fileStream.Flush();
            }

            this.Color = color;
            this.Size = size;
            this.SingleDimension = singleDimension;
            this.Tension = tension;
            this.Deviation = deviation;
            this.interval = interval;
            this.IntDevPct = intDevPct;
            this.IntOePct = intOePct;

            this.thread = null;
            this.Invalidate = Invalidate;

            this.points = new List<PointD>();
            Restart();
        }

        public void Start()
        {
            if (thread == null)
            {
                thread = new Thread(Run);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        public void Restart()
        {
            this.x = 0;
            this.y = 0;
            lock (this)
            {
                this.points.Clear();
                this.points.Add(new PointD(x, y));
            }
        }

        private void Run()
        {
            while (true)
            {
                if (SingleDimension)
                {
                    if (rand.Bool())
                        Mod(ref x);
                    else
                        Mod(ref y);
                }
                else
                {
                    Mod(ref x);
                    Mod(ref y);
                }

                lock (this)
                    points.Add(new PointD(x, y));

                Invalidate();

                Thread.Sleep(rand.GaussianOEInt(interval, IntDevPct, IntOePct));
            }
        }
        private void Mod(ref double value)
        {
            value += rand.Gaussian(Deviation);
        }
    }
}