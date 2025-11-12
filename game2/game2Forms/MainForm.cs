namespace game2Forms
{
    public partial class MainForm : Form
    {
        public MapCtrl Map => map;

        public MainForm()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            this.ResizeRedraw = true;
            //this.AutoScaleDimensions = new SizeF(96F, 96F); 

        }
        //protected override void OnShown(EventArgs e)
        //{
        //    base.OnShown(e);

        //    // Ensure autoscaling uses the current monitor DPI and force layout of children immediately
        //    try
        //    {
        //        PerformAutoScale();
        //    }
        //    catch { /* defensive - should not throw but be tolerant */ }

        //    // Force layout for critical hosted controls
        //    info?.PerformLayout();
        //    runes?.PerformLayout();
        //    resources?.PerformLayout();

        //    this.PerformLayout();
        //}
        //protected override void OnDpiChanged(DpiChangedEventArgs e)
        //{
        //    base.OnDpiChanged(e);

        //    // When DPI actually changes (PerMonitorV2) make sure layout re-applies immediately
        //    try
        //    {
        //        PerformAutoScale();
        //    }
        //    catch { }

        //    info?.PerformLayout();
        //    runes?.PerformLayout();
        //    resources?.PerformLayout();
        //    this.PerformLayout();
        //}

        private void Main_Shown(object? sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Maximized;
            //var screen = Screen.FromControl(this);
            //this.Bounds = screen.WorkingArea;
            //this.PerformLayout(); 

            RefreshAll();
        }
        private void Main_KeyDown(object? sender, KeyEventArgs e)
        {
            map.Map_KeyDown(sender, e);
        }
        public void Main_KeyUp(object? sender, KeyEventArgs e)
        {
            map.Map_KeyUp(sender, e);
            if (e.KeyCode == Keys.Escape)
            {
                Program.Game.EndTurn();
                RefreshAll();
            }
        }

        // It's also helpful to call EnsureWidthForContent on resize and dpi events:
        protected override void OnResize(EventArgs e)
        {
            RefreshInfo();
            base.OnResize(e);
        }
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            RefreshInfo();
            base.OnDpiChanged(e);
        }

        public void RefreshAll()
        {
            RefreshInfo();
            RefreshResources();
            RefreshMap();
        }
        public void RefreshMap()
        {
            map.ClearCache();
            map.Invalidate();
            Invalidate();
        }
        public void RefreshResources()
        {
            resources.RefreshResources();
        }

        public void RefreshInfo()
        {
            int minInfoWidth = 100;
            // recompute max width and allow shrinking on resize 
            int maxInfoWidth = Math.Min(300, (int)(this.ClientSize.Width * 0.35));

            // Allow shrinking so Info stays in sync with smaller content or window resizes.
            if (info.RefreshInfo(this.resources.Height + 13, minInfoWidth, maxInfoWidth))
            {
                // Force layout so the map/control layout updates immediately
                this.PerformLayout();
                map.Invalidate();
            }
        }
    }
}
