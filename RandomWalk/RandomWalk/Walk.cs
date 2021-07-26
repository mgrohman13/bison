using System;
using System.Diagnostics;
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
        public static ulong ID;
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
                lock (this)
                    return points.Count;
            }
        }

        public Walk(Action Invalidate, Color color, int size, bool singleDimension, double tension, double deviation, double interval, double intDevPct, double intOePct, double max, double x, double y)
        {
            string info = string.Format("{9}\t{0}\tsize: {1}\tsingleDimension: {2}\ttension:{3}\tdeviation:{4}\tinterval:{5}\tintDevPct:{6}\tintOePct:{7}\tmax:{8}\tx:{10}\ty:{11}",
                    string.Format("R: {0} G: {1} B: {2}", color.R.ToString().PadLeft(3), color.G.ToString().PadLeft(3), color.B.ToString().PadLeft(3)),
                    size, singleDimension,
                    PadDouble(tension), PadDouble(deviation), PadDouble(interval, 4), PadDouble(intDevPct, 0), PadDouble(intOePct, 0), PadDouble(max, 4),
                    DateTime.Now.TimeOfDay, PadDouble(x, 2), PadDouble(y, 2));
            WriteLine(info);

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
            Restart(x, y);
        }

        public static void WriteLine(string info)
        {
            lock (typeof(Walk))
            {
                Console.WriteLine(info);
                using (var fileStream = new System.IO.StreamWriter("walks_" + ID + ".txt", true))
                {
                    fileStream.WriteLine(info);
                    fileStream.Flush();
                }
            }
        }

        private string PadDouble(double val)
        {
            return PadDouble(val, 1);
        }
        private string PadDouble(double val, int digits)
        {
            return val.ToString(
                //(digits == 0 ? "" : "0") +
                "." + new string('0', 11 - digits)).PadLeft(13);
        }

        public void Deactivate()
        {
            if (active)
            {
                Walk.WriteLine("deactivate");
                this.active = false;
            }
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

        public void Restart(double x, double y)
        {
            this.x = x;
            this.y = y;
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
                val = Count / ( Count + val );
                if (active)
                    val *= val;
                while (rand.Bool(val) && ( !active || Count > 1 ))
                    Decay();

                Invalidate();

                Thread.Sleep(rand.GaussianOEInt(interval, IntDevPct, IntOePct));
            }
            Deactivate();
            thread = null;
        }

        private void Decay()
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
