using System;
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

        private bool active = true;
        private double x, y;
        private double interval, max;
        private Thread thread;
        private Action Invalidate;
        private List<PointD> points;

        public bool Active
        {
            get
            {
                return active;
            }
        }

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

        public int Count
        {
            get
            {
                return points.Count;
            }
        }

        public Walk(Action Invalidate, Color color, int size, bool singleDimension, double tension, double deviation, double interval, double intDevPct, double intOePct, double max)
        {
            string info = string.Format("color:{0}\tsize:{1}\tsingleDimension:{2}\ttension:{3}\tdeviation:{4}\tinterval:{5}\tIntDevPct:{6}\tIntOePct:{7}\tmax:{8}",
                    color, size, singleDimension, tension, deviation, interval, intDevPct, intOePct, max);
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
            this.max = max;

            this.thread = null;
            this.Invalidate = Invalidate;

            this.points = new List<PointD>();
            Restart();
        }

        public void Deactivate()
        {
            this.active = false;
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
            while (this.Count > 0)
            {
                if (this.active)
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
                }

                double val = active ? max : Math.Sqrt(max);
                if (rand.Bool(Count / ( Count + val )))
                    Decay();

                Invalidate();

                Thread.Sleep(rand.GaussianOEInt(interval, IntDevPct, IntOePct));
            }
            Deactivate();
            thread = null;
        }

        public void Decay()
        {
            lock (this)
                if (points.Count > 0)
                    points.RemoveAt(rand.Next(points.Count));
        }

        private void Mod(ref double value)
        {
            value += rand.Gaussian(Deviation);
        }
    }
}
