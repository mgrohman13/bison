using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Sorting
{
    public delegate void RefreshDelegate();

    public partial class Graph : Form
    {
        private float maxElement;
        private Sorter sorter;
        public Graph()
        {
            InitializeComponent();
            //eliminates anooying flickering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            sorter = new Sorter(this.AddSwap, this.RefreshAll);
            //initialize with a random number of elements
            nudAmt.Value = Program.Random.GaussianCappedInt(300, .21f, 10) + Program.Random.OEInt(210);
        }

        protected override void OnLoad(EventArgs e)
        {
            //create a new list
            NewList();
            //initialize sort speed
            tbSpeed_ValueChanged(tbSpeed, null);
            base.OnLoad(e);

            RefreshAll();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //kill the sorting thread when this form is closed
            Stop();
            base.OnClosing(e);
        }

        private void NewList()
        {
            //number of elements
            int length = (int)nudAmt.Value;

            uint div = 0, max = 0;
            if (chbFewUnique.Checked)
            {
                //div is the range of values that will be converted to the same value 
                div = (uint)( Program.Random.GaussianCapped(uint.MaxValue / Math.Sqrt(length), .06, uint.MaxValue / 100000.0) + .5 );
                //maximum value
                max = uint.MaxValue / div * div;
                this.maxElement = max - div;
            }
            else
            {
                this.maxElement = uint.MaxValue;
            }

            Obj[] list;
            if (chbSublists.Checked)
            {
                //create several sublists and then concatenate them
                list = new Obj[length];
                //average length of each sub list
                float avgLength = length / Program.Random.GaussianCapped((float)( Math.Pow(length, .3) + 1.3 ), .039f, 2.1f);
                for (int current = 0 ; current < length ; )
                {
                    int sublist = Program.Random.GaussianCappedInt(avgLength, .666f, 1);
                    int remaining = length - current;
                    if (sublist > remaining)
                    {
                        sublist = remaining;
                    }
                    else
                    {
                        //use a maximum value to reduce the likelihood of the last list being only a couple elements
                        remaining = Program.Random.GaussianCappedInt(remaining / 2.1f, .13f);
                        if (sublist > remaining)
                            sublist = remaining;
                        if (sublist < 1)
                            sublist = 1;
                    }

                    //append the new sublist
                    Obj[] temp = NewList(sublist, div, max);
                    for (int i = 0 ; i < sublist ; )
                        list[current++] = temp[i++];
                }
            }
            else
            {
                list = NewList(length, div, max);
            }

            sorter.List = list;
        }
        private Obj[] NewList(int length, uint div, uint max)
        {
            Obj[] list = new Obj[length];
            for (int i = 0 ; i < length ; ++i)
            {
                uint value = 0;
                //use the selected distribution
                if (rbBell.Checked)
                    value = (uint)( Program.Random.GaussianCapped(uint.MaxValue / 2.0, .21, 1) + .5 );
                else if (rbHigh.Checked)
                    value = uint.MaxValue - GetOE();
                else if (rbLow.Checked)
                    value = GetOE();
                else if (rbNormal.Checked)
                    value = Program.Random.NextUInt();

                if (chbFewUnique.Checked)
                {
                    if (value >= max)
                    {
                        //if the value is above the maximum, retry
                        --i;
                        continue;
                    }
                    //use integer division to floor each value using the div
                    value = value / div * div;
                }
                list[i] = new Obj(value, AddComparison);
            }
            PartialSort(list);
            if (chbReversed.Checked)
                Array.Reverse(list);
            return list;
        }

        private uint GetOE()
        {
            return (uint)( ( Program.Random.OE(uint.MaxValue / 6.66) + .5 ) % uint.MaxValue );
        }

        private void PartialSort(Obj[] list)
        {
            int length = list.Length;
            int count = tbSorted.Value;
            int max = tbSorted.Maximum;
            if (count > 0)
            {
                if (count < max)
                {
                    //partially sort the list, using more iterations for higher counts
                    max = Program.Random.Round(3.9f * count / ( max - 1f ) * length);
                    count = 0;
                    while (true)
                    {
                        bool sorted = true;
                        for (int i = 1 ; i < length ; ++i)
                            if (list[i - 1].Value > list[i].Value)
                            {
                                sorted = false;
                                break;
                            }
                        if (sorted)
                            break;
                        int a = Program.Random.Next(length);
                        int b = Program.Random.Next(length);
                        if (a < b ? list[a].Value > list[b].Value : a > b && list[a].Value < list[b].Value)
                        {
                            Obj temp = list[a];
                            list[a] = list[b];
                            list[b] = temp;
                            if (++count > max)
                                break;
                        }
                    }
                }
                else
                {
                    //if count equals the maximum, fully sort the list
                    int step = length;
                    while (step > 1)
                    {
                        step = Program.Random.Round(step / 2.2f);
                        if (step < 1)
                            step = 1;
                        for (int a = step ; a < length ; a++)
                        {
                            Obj temp = list[a];
                            int b = a;
                            while (b >= step && list[b - step].Value > temp.Value)
                                list[b] = list[b -= step];
                            list[b] = temp;
                        }
                    }
                }
            }
        }

        private Thread sortThread = null;
        private int comparisons, swaps;
        private void StartSort(ThreadStart start)
        {
            Stop();

            //reset counts
            comparisons = swaps = 0;
            lblComps.Text = lblSwaps.Text = string.Empty;
            //the sorting is done in a separate thread so the UI doenst get tied up
            sortThread = new Thread(start);
            sortThread.Start();
            //allow the user to stop the sort
            btnStop.Enabled = true;
        }

        private float speed;
        private void tbSpeed_ValueChanged(object sender, EventArgs e)
        {
            //calculate the sleep time 
            speed = (float)( Math.Pow(2.1, ( 6.66 - tbSpeed.Value ) * 1.69) * 100 / (double)sorter.Length );
        }

        private void Stop()
        {
            if (sortThread != null && sortThread.IsAlive)
                sortThread.Abort();
            btnStop.Enabled = false;
        }

        private void AddComparison()
        {
            ++comparisons;
            //check speed so this doesnt slow things down too much if the speed is insanely fast
            if (Program.Random.Round(speed) > 0)
                this.Invoke((RefreshDelegate)this.RefreshComparisons);
        }
        private void AddSwap()
        {
            ++swaps;
            if (Program.Random.Round(speed) > 0)
                this.Invoke((RefreshDelegate)this.RefreshSwaps);
            RedrawGraph();
        }

        private void RefreshAll()
        {
            this.Invoke((RefreshDelegate)this.RefreshComparisons);
            this.Invoke((RefreshDelegate)this.RefreshSwaps);
            this.Invoke((RefreshDelegate)this.InvalidateGraph);
            this.Invoke((RefreshDelegate)this.Stop);
        }

        private void RefreshComparisons()
        {
            this.lblComps.Text = FormatNum(comparisons);
        }
        private void RefreshSwaps()
        {
            this.lblSwaps.Text = FormatNum(swaps);
        }
        private string FormatNum(int value)
        {
            if (value > 0)
                return value.ToString("0,000,000").TrimStart('0', ',');
            return "0";
        }

        private void RedrawGraph()
        {
            int sleep = Program.Random.Round(speed);
            //dont do anything if we get a 0 sleep time since Thread.Sleep(0) will still sleep for a few miliseconds
            if (sleep > 0)
            {
                this.Invoke((RefreshDelegate)this.InvalidateGraph);
                Thread.Sleep(sleep);
            }
        }

        private void InvalidateGraph()
        {
            //pnlGraph is invisible and is only used to keep track of where the graph is on the form
            Invalidate(pnlGraph.Bounds);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            float width = pnlGraph.Width;
            float height = pnlGraph.Height;
            float length = sorter.Length;

            //calculate the size of each drawn square, based on the number of values and available space to draw
            float offset = width / length * 1.69f;
            if (offset < 1)
                offset = 1;
            else if (offset > 2)
                offset /= (float)Math.Pow(offset, .39);
            int size = (int)Math.Round(offset);
            offset = size / 2f - 1;

            //bring in values so we dont go over the boundary
            float startX = pnlGraph.Location.X + offset;
            float startY = pnlGraph.Location.Y + offset;
            width -= size;
            height -= size;
            --length;

            //draw each element
            Graphics g = e.Graphics;
            for (int i = 0 ; i < sorter.Length ; ++i)
            {
                int x = (int)( startX + width * ( i / length ) - offset );
                int y = (int)( startY + height * ( 1 - sorter.List[i].Value / maxElement ) - offset );
                g.FillRectangle(Brushes.White, x, y, size, size);
            }

            base.OnPaint(e);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            //first stop any previous sort
            Stop();
            NewList();
            InvalidateGraph();
        }

        //sort button events
        private void btnBubble_Click(object sender, EventArgs e)
        {
            StartSort(sorter.BubbleSort);
        }
        private void btnHeap_Click(object sender, EventArgs e)
        {
            StartSort(sorter.HeapSort);
        }
        private void btnQuick_Click(object sender, EventArgs e)
        {
            StartSort(sorter.QuickSort);
        }
        private void btnShell_Click(object sender, EventArgs e)
        {
            StartSort(sorter.ShellSort);
        }
        private void btnCocktail_Click(object sender, EventArgs e)
        {
            StartSort(sorter.CocktailSort);
        }
        private void btnOddEven_Click(object sender, EventArgs e)
        {
            StartSort(sorter.OddEvenSort);
        }
        private void btnGnome_Click(object sender, EventArgs e)
        {
            StartSort(sorter.GnomeSort);
        }
        private void btnStooge_Click(object sender, EventArgs e)
        {
            StartSort(sorter.StoogeSort);
        }
        private void btnBozo_Click(object sender, EventArgs e)
        {
            StartSort(sorter.BozoSort);
        }
        private void btnStrand_Click(object sender, EventArgs e)
        {
            StartSort(sorter.StrandSort);
        }
        private void btnKronrod_Click(object sender, EventArgs e)
        {
            StartSort(sorter.KronrodSort);
        }
        private void btnMerge_Click(object sender, EventArgs e)
        {
            StartSort(sorter.MergeSort);
        }
        private void btnSmooth_Click(object sender, EventArgs e)
        {
            StartSort(sorter.Smoothsort);
        }
        private void btnLSD_Click(object sender, EventArgs e)
        {
            StartSort(sorter.RadixLSDSort);
        }
        private void btnMSD_Click(object sender, EventArgs e)
        {
            StartSort(sorter.RadixMSDSort);
        }
    }
}