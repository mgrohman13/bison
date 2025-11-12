using game2.game;
using game2.map;
using game2.pieces;
using game2.pieces.behavior;
using game2.pieces.player;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace game2Forms
{
    public partial class MapCtrl : UserControl
    {
        private const float padding = 1f, scrollTime = 39f, scrollSpeed = 13f;

        private readonly Timer timer;

        private Tile? _selected;

        private float _xPan, _yPan, _scale;
        private bool _scrollDown, _scrollLeft, _scrollUp, _scrollRight;

        private Dictionary<Brush, List<List<PointF>>>? _terrainCache;

        //// debug: hit-test visualization
        //private bool _debugHitTest = false;
        //private PointF? _lastClick;
        //private readonly List<DebugCandidate> _debugCandidates = new();
        //private record DebugCandidate(int X, int Y, PointF Hex, PointF Click, float DistSq, bool Contains);

        //private bool _shift = false;

        public Tile? Selected => _selected;

        private float XPan
        {
            get => _xPan;
            set => _xPan = ClearCache(_xPan, value);
        }
        private float YPan
        {
            get => _yPan;
            set => _yPan = ClearCache(_yPan, value);
        }
        private float Zoom
        {
            get => _scale;
            set => _scale = ClearCache(_scale, value);
        }
        private float ClearCache(float current, float value)
        {
            if (current != value)
                ClearCache();
            return value;
        }
        public void ClearCache()
        {
            _terrainCache = null;
        }

        public MapCtrl()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            this.ResizeRedraw = true;

            timer = new()
            {
                Interval = Game.Rand.Round(scrollTime)
            };
            timer.Tick += Timer_Tick;

            MouseWheel += Map_MouseWheel;
        }
        private void Map_Load(object sender, EventArgs e)
        {
            //if (Screen.PrimaryScreen != null)
            //    this.Bounds = Screen.PrimaryScreen.WorkingArea;

            _selected = Program.Game.Player.Core.Tile;

            _scale = Game.Rand.GaussianOE(Game.Rand.Range(39, 52), .26f, .13f, 1);
            _xPan = Rand(Width / -2f);
            _yPan = Rand(Height / -2f);

            float Rand(float x) => x + Game.Rand.Gaussian(_scale);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //float xs = _xPan, ys = _yPan;

            float scrollAmt = (float)(scrollSpeed * Math.Sqrt(Zoom));
            //scrollAmt *= (scrollTime + paintTime) / scrollTime;
            scrollAmt = Game.Rand.Gaussian(scrollAmt, .013f);

            if (_scrollDown && !_scrollUp)
                YPan -= scrollAmt;
            if (_scrollUp && !_scrollDown)
                YPan += scrollAmt;
            if (_scrollLeft && !_scrollRight)
                XPan -= scrollAmt;
            if (_scrollRight && !_scrollLeft)
                XPan += scrollAmt;

            CheckBounds();
            Invalidate();
            //Map_MouseMove(sender, null);

            //timer.Interval = Math.Max(1, Game.Rand.Round(scrollTime - paintTime));
        }

        public void Map_KeyDown(object? sender, KeyEventArgs e)
        {
            //_shift = false;
            //if (e.Shift)
            //{
            //    _shift = true;
            //    ClampPath();
            //}
            //else 

            if (e.KeyCode == Keys.W)
                _scrollDown = true;
            else if (e.KeyCode == Keys.A)
                _scrollLeft = true;
            else if (e.KeyCode == Keys.S)
                _scrollUp = true;
            else if (e.KeyCode == Keys.D)
                _scrollRight = true;

            //// toggle debug hit-test overlay
            //else if (e.KeyCode == Keys.F3)
            //{
            //    _debugHitTest = !_debugHitTest;
            //    _terrainCache = null;
            //    Invalidate();
            //    Debug.WriteLine($"Debug hit-test: {_debugHitTest}");
            //}

            if (_scrollDown || _scrollLeft || _scrollUp || _scrollRight)
                timer.Start();
        }
        public void Map_KeyUp(object? sender, KeyEventArgs e)
        {
            //if (shift)
            //{
            //    shift = false;
            //    var old = MouseTile;
            //    MouseTile = null;
            //    MouseTile = old;
            //}

            //if (e.KeyCode == Keys.Escape)
            //{
            //    Program.Game.EndTurn();
            //    _terrainCache = null;
            //    Invalidate();
            //    return;
            //}

            if (e.KeyCode == Keys.W)
                _scrollDown = false;
            else if (e.KeyCode == Keys.A)
                _scrollLeft = false;
            else if (e.KeyCode == Keys.S)
                _scrollUp = false;
            else if (e.KeyCode == Keys.D)
                _scrollRight = false;
            if (!(_scrollDown || _scrollLeft || _scrollUp || _scrollRight))
                timer.Stop();
        }

        private void Map_MouseClick(object? sender, MouseEventArgs e)
        {
            _scrollDown = false;
            _scrollLeft = false;
            _scrollUp = false;
            _scrollRight = false;

            int y = GetMapY(e.X, e.Y);
            int x = GetMapX(e.X, y);

            Tile? clicked = Program.Game.Map.GetVisibleTile(x, y);
            //Tile? orig = _selected;

            if (clicked != null)
                if (e.Button == MouseButtons.Left)
                {
                    _selected = clicked;

                    //Program.Game.Map.ForceLand.Add(new MattUtil.Point(x, y));
                    //_terrainCache = null;
                    Debug.WriteLine($"Selected tile: {_selected}");
                    Program.Form.RefreshInfo();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (_selected?.Piece is PlayerPiece piece && piece.HasBehavior(out Movable? movable))
                        if (movable.Move(clicked))
                        {
                            _terrainCache = null;
                            _selected = clicked;

                            //Debug.WriteLine($"Moved piece to: {clicked}");
                            Program.Form.RefreshAll();
                        }
                }

            Invalidate();
        }

        private void Map_MouseWheel(object? sender, MouseEventArgs e)
        {
            Tile? sel = _selected;// MouseTile ?? SelTile;
            PointF anchor;
            if (sel != null && IsInBounds(GetX(sel.Point.X), GetY(sel.Point.Y)))
            {
                anchor = sel.Point;
            }
            else
            {
                int y = GetMapY(Width / 2f, Height / 2f);
                int x = GetMapX(Width / 2f, y);
                anchor = Tile.GetPoint(x, y);
            }
            float selX = GetX(anchor.X) + Zoom / 2f;
            float selY = GetY(anchor.Y) + Zoom / 2f;

            float mult = e.Delta / Game.Rand.GaussianCapped(91f, .026f, 1);
            if (mult < 0)
                mult = -1 / mult;
            else if (mult == 0)
                mult = 1;
            this.Zoom *= mult;
            if (Zoom < 1)
                Zoom = 1;
            else if (Zoom > 169)
                Zoom = 169;

            _xPan += GetX(anchor.X) + Zoom / 2f - selX;
            _yPan += GetY(anchor.Y) + Zoom / 2f - selY;

            CheckBounds();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                e.Graphics.Clear(Color.Black);

                if (Program.Game != null)
                {
                    PaintTiles(e);
                    PaintPieces(e);
                }

                //// draw debug overlays after main paint
                //if (_debugHitTest)
                //    DrawHitTestDebug(e);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
        }

        //private void DrawHitTestDebug(PaintEventArgs e)
        //{
        //    if (_lastClick is null) return;
        //    var g = e.Graphics;

        //    // draw click cross
        //    var click = _lastClick.Value;
        //    using (var clickPen = new Pen(Color.Cyan, 2))
        //    {
        //        g.DrawLine(clickPen, click.X - 8, click.Y, click.X + 8, click.Y);
        //        g.DrawLine(clickPen, click.X, click.Y - 8, click.X, click.Y + 8);
        //    }

        //    // draw candidates: contained polygons in green, others in orange; centers as small dots
        //    foreach (var c in  Game.Rand.Iterate(_debugCandidates))
        //    {
        //        // polygon outline
        //        using (var pen = new Pen(c.Contains ? Color.Lime : Color.Orange, c.Contains ? 2f : 1f))
        //        {
        //            pen.DashStyle = c.Contains ? DashStyle.Solid : DashStyle.Dot;
        //            g.DrawLine(pen, c.Click, c.Hex);
        //        }



        //        //// center dot (filled)
        //        //using (var brush = new SolidBrush(c.Contains ? Color.Lime : Color.Orange))
        //        //{
        //        //    var r = new RectangleF(c.CenterScreen.X - 3, c.CenterScreen.Y - 3, 6, 6);
        //        //    g.FillEllipse(brush, r);
        //        //}
        //    }

        //    //// highlight nearest center
        //    //var nearest = _debugCandidates.OrderBy(c => c.DistSq).FirstOrDefault();
        //    //if (nearest != null)
        //    //{
        //    //    using var pen = new Pen(Color.Magenta, 2);
        //    //    g.DrawEllipse(pen, nearest.CenterScreen.X - 6, nearest.CenterScreen.Y - 6, 12, 12);
        //    //}
        //}

        private void PaintTiles(PaintEventArgs e)
        {
            if (_terrainCache == null)
            {
                _terrainCache = Program.Game.Map.VisibleTiles.Where(t =>
                        IsInBounds(GetX(t.Point.X), GetY(t.Point.Y))).GroupBy(GetBrush)
                    .ToDictionary(group => group.Key, group => HexUnion.MergeHexes(
                        Game.Rand.Iterate(group.Select(t => new MattUtil.Point(t.XIndex, t.YIndex)))));
                _terrainCache.Add(new SolidBrush(Color.FromArgb(20, 20, 20)),
                    HexUnion.MergeHexes(Program.Game.Map.NotTallTiles.Where(idx =>
                    {
                        PointF p = Tile.GetPoint((int)idx.X, (int)idx.Y);
                        return IsInBounds(GetX(p.X), GetY(p.Y));
                    })));
            }

            foreach (var group in Game.Rand.Iterate(_terrainCache))
            {
                //Brush brush = GetBrush(group);

                // Build a single GraphicsPath for this terrain, add all polygons (outer + holes),
                // and fill using Alternate fill mode so fully enclosed gaps (holes) are subtracted.
                using var path = new GraphicsPath();
                path.FillMode = FillMode.Alternate;

                foreach (var polygon in Game.Rand.Iterate(group.Value))
                {
                    var points = polygon.Select(p => new PointF(GetX(p.X), GetY(p.Y))).ToArray();
                    if (points.Length > 2)
                        path.AddPolygon(points);
                }

                if (path.PointCount > 0)
                {
                    if (group.Key != Brushes.Black)
                        e.Graphics.FillPath(group.Key, path);
                    e.Graphics.DrawPath(Pens.Black, path);
                }
            }

            if (_selected != null)
                using (Pen p = new(Color.Black, 3))
                    e.Graphics.DrawPolygon(p, GetDrawHex(GetX(_selected.Point.X), GetY(_selected.Point.Y)));
        }
        private static Brush GetBrush(Tile t) => t.Terrain switch
        {
            Terrain.Sand => Brushes.Tan,
            Terrain.Plains => Brushes.Gray,
            Terrain.Forest => Brushes.Green,
            Terrain.Hills => Brushes.DarkGoldenrod,
            Terrain.Mountains => Brushes.Gold,
            Terrain.Sea => Brushes.Blue,
            Terrain.Kelp => Brushes.SeaGreen,
            Terrain.Glacier => Brushes.White,
            _ => Brushes.Magenta,
        };

        private void PaintPieces(PaintEventArgs e)
        {
            using Font font = new("Cascadia Code", Zoom / 3f, FontStyle.Bold, GraphicsUnit.Pixel);

            Dictionary<Brush, List<RectangleF>> rects = new()
            {
                { Brushes.Magenta, [] },
                { Brushes.PaleGreen, [] },
                { Brushes.Silver, [] },
                { Brushes.Purple, [] },
                { Brushes.DarkCyan, [] },
                { Brushes.Cyan, [] },
                { Brushes.DarkTurquoise, [] },
                { Brushes.IndianRed, [] },
            };
            Dictionary<Brush, List<(string, PointF)>> strings = new()
            {
                { Brushes.Black, [] },
            };
            List<(Image, RectangleF)> images = [];

            foreach (var piece in Game.Rand.Iterate(Program.Game.Map.GetVisiblePieces()))
            {
                GetHexLengths(out float side, out float mid);
                float x = GetX(piece.Tile.Point.X) + mid * 2;
                float y = GetY(piece.Tile.Point.Y) + side;

                if (IsInBounds(x, y))
                {
                    RectangleF rect = new(x - Zoom / 4f, y - Zoom / 4f, Zoom / 2f, Zoom / 2f);
                    if (piece is Resource resource)
                    {
                        rect.Offset(Zoom / -4f, 0f);
                        rect.Inflate(Zoom / -24f, Zoom / -24f);

                        int idx = Game.Rand.Iterate(Enumerable.Range(0, Resources.NumResources)
                            .Select(a => (a, resource.Generate[a]))).OrderByDescending(p => p.Item2).First().Item1;

                        images.Add((Images.Resources[idx], rect));

                        string str = resource.Generate[idx].ToString();
                        var size = e.Graphics.MeasureString(str, font);
                        strings[Brushes.Black].Add((str, new PointF(rect.Right + Zoom / 24f, y - size.Height / 2f)));
                    }
                    else
                    {
                        rect.Inflate(Zoom / 12f, Zoom / 12f);

                        Brush b = Brushes.Magenta;
                        if (piece is Core core)
                            b = Brushes.Cyan;
                        else if (piece is PlayerPiece playerPiece)
                            b = Brushes.DarkTurquoise;
                        else
                            b = Brushes.IndianRed;
                        rects[b].Add(rect);

                        //if (piece.HasBehavior(out Movable? movable))
                        //{
                        //    string str = movable.MoveCur.ToString();
                        //    var size = e.Graphics.MeasureString(str, font);
                        //    strings[Brushes.Black].Add((str, new PointF(x - size.Width / 2f, y - size.Height / 2f)));
                        //}
                    }
                }
            }

            foreach (var pair in Game.Rand.Iterate(rects))
                if (pair.Value.Count > 0)
                {
                    var a = pair.Value.ToArray();
                    e.Graphics.FillRectangles(pair.Key, a);
                    e.Graphics.DrawRectangles(Pens.Black, a);
                }
            foreach (var pair in Game.Rand.Iterate(images))
                e.Graphics.DrawImage(pair.Item1, pair.Item2);
            foreach (var pair in Game.Rand.Iterate(strings))
                foreach (var str in Game.Rand.Iterate(pair.Value))
                    e.Graphics.DrawString(str.Item1, font, pair.Key, str.Item2);
        }

        private void CheckBounds()
        {
            if (!Program.Game.Map.VisibleTiles.Any(t => IsInBounds(GetX(t.Point.X), GetY(t.Point.Y))))
            {
                XPan = Width / -2f;
                YPan = Height / -2f;
            }
        }
        private bool IsInBounds(float x, float y) =>
            x + Zoom >= 0 && x <= Width && y + Zoom >= 0 && y - Zoom / 4f <= Height;

        private float GetX(float x) => GetCoord(x, XPan);
        private float GetY(float y)
        {
            GetHexLengths(out float side, out _);
            return GetCoord(y, YPan) + side;
        }
        private float GetCoord(float coord, float start) => coord * Zoom - start + padding;

        private int GetMapX(float x, int mapY) => GetMapCoord(x - Zoom / 2f * (mapY & 1), XPan, 1);
        //private int GetMapY(float x, float y)
        //{
        //    GetHex(out _, out float side, out _);
        //
        //    int y1 = GetMapCoord((y - side) / Tile.YRatio, YPan);
        //    int x1 = GetMapX(x, y1);
        //    int y2 = y1 + 1;
        //    int x2 = GetMapX(x, y2);
        //
        //    PointF p1 = Tile.GetPoint(x1, y1), p2 = Tile.GetPoint(x2, y2);
        //
        //    if (DistanceSqr(GetX(p1.X), GetY(p1.Y), x, y) > DistanceSqr(GetX(p2.X), GetY(p2.Y), x, y))
        //        return y2;
        //    else
        //        return y1;
        //}
        private int GetMapY(float x, float y)
        {
            GetHexLengths(out float side, out float mid);

            int approxY = GetMapCoord(y, YPan - side, Tile.YRatio);

            //// debug init
            //if (_debugHitTest)
            //{
            //    _debugCandidates.Clear();
            //    _lastClick = new PointF(x, y);
            //}

            int bestY = approxY;
            float bestDist = float.MaxValue;

            // search a small neighborhood around the approximate row to handle parity/corner cases. 
            for (int candY = approxY; candY <= approxY + 1; candY++)
            {
                int candX = GetMapX(x, candY);
                PointF topLeft = Tile.GetPoint(candX, candY);
                float centerX = GetX(topLeft.X) + mid * 2;
                float centerY = GetY(topLeft.Y) + side;
                //var poly = GetDrawHex(cx, cy);
                //bool contains = PointInPolygon(poly, x, y);

                float d = DistanceSqr(centerX, centerY, x, y);
                //Debug.WriteLine($"{candX},{candY}: {d} ({cx},{cy})");
                if (d < bestDist)
                {
                    bestDist = d;
                    bestY = candY;
                }

                //if (_debugHitTest)
                //    _debugCandidates.Add(new DebugCandidate(candX, candY, new PointF(cx, cy), new PointF(x, y), d, contains));

                //if (contains)
                //    bestY = candY;
            }

            return bestY;

            //Debug.WriteLine(bestY);
            //if (bestY.HasValue)
            //    return bestY.Value;

            //throw new Exception("No containing hex found in neighborhood");
        }
        private int GetMapCoord(float pixel, float start, float div) => (int)Math.Floor((pixel + start - padding) / Zoom / div);

        private static float DistanceSqr(float x1, float y1, float x2, float y2) =>
            (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);

        private PointF[] GetDrawHex(float xVal, float yVal)
        {
            GetHexLengths(out float side, out float mid);
            PointF[] points =
            [
                new PointF(xVal, yVal),
                new PointF(xVal + mid * 2, yVal - side),
                new PointF(xVal + mid * 4, yVal),
                new PointF(xVal + mid * 4, yVal + side * 2),
                new PointF(xVal + mid * 2, yVal + side * 3),
                new PointF(xVal, yVal + side * 2),
            ];
            return points;
        }
        private void GetHexLengths(out float side, out float mid)
        {
            side = Zoom * 1f / 4f / Tile.YRatio;
            mid = Zoom / 4f;
        }

        //// standard ray-casting point-in-polygon test
        //private static bool PointInPolygon(PointF[] polygon, float testX, float testY)
        //{
        //    bool inside = false;
        //    if (polygon == null || polygon.Length < 3) return false;
        //    for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        //    {
        //        float xi = polygon[i].X, yi = polygon[i].Y;
        //        float xj = polygon[j].X, yj = polygon[j].Y;

        //        bool intersect = ((yi > testY) != (yj > testY)) &&
        //                         (testX < (xj - xi) * (testY - yi) / ((yj - yi) == 0 ? float.Epsilon : (yj - yi)) + xi);
        //        if (intersect) inside = !inside;
        //    }
        //    return inside;
        //}

        /// <summary>
        /// Utilities to compute the polygonal union of multiple hex tiles.
        /// The algorithm:
        /// 1. Generate the 6 corners for each hex in integer space.
        /// 2. Add each hex edge to a dictionary; if an edge appears twice it is internal and removed.
        /// 3. Remaining edges form the outer boundaries. Walk them to produce polygons.
        /// 4. Uses an X multiplier of 2 and Y of 4*Tile.YRatio to work with integer points.
        /// </summary>
        private static class HexUnion
        {
            // Precomputed corner offsets in integer space matching hexagon geometry.
            private static readonly Point[] HexOffsets = [
                new(0, 0),
                new(1, -1),
                new(2, 0),
                new(2, 2),
                new(1, 3),
                new(0, 2),
            ];

            // Restore actual floating-point coordinates from integer space.
            private static PointF ResultPoint(Point p) =>
                new(p.X / 2f, p.Y / (4f * Tile.YRatio));

            private static (int X1, int Y1, int X2, int Y2) MakeOrderedEdgeKey(Point a, Point b)
            {
                // Order endpoints so identical undirected edge produces same key regardless of direction
                if (a.X < b.X || (a.X == b.X && a.Y <= b.Y))
                    return (a.X, a.Y, b.X, b.Y);
                return (b.X, b.Y, a.X, a.Y);
            }

            /// <summary>
            /// Merge the given tiles into a list of polygon boundaries.
            /// Returned polygons are in tile-coordinate space (same coordinate system as Tile.Point).
            /// </summary>
            public static List<List<PointF>> MergeHexes(IEnumerable<MattUtil.Point> tiles)
            {
                // Edge dictionary: key => count (we will remove edges that appear twice)
                var edgeSeen = new Dictionary<(int X1, int Y1, int X2, int Y2), int>();

                foreach (var t in Game.Rand.Iterate(tiles))
                {
                    var origin = new Point(2 * t.X + (t.Y & 1), 3 * t.Y);

                    // compute corners
                    Point[] corners = [.. HexOffsets.Select(off => new Point(origin.X + off.X, origin.Y + off.Y))];

                    for (int i = 0; i < corners.Length; i++)
                    {
                        var p = corners[i];
                        var q = corners[(i + 1) % corners.Length];

                        var edgeKey = MakeOrderedEdgeKey(p, q);

                        // If edge previously seen remove it (internal). Otherwise add it.
                        if (!edgeSeen.Remove(edgeKey))
                            edgeSeen[edgeKey] = 1;
                    }
                }

                // Build adjacency from remaining edges.
                var adj = new Dictionary<Point, List<Point>>();
                foreach (var (X1, Y1, X2, Y2) in Game.Rand.Iterate(edgeSeen.Keys))
                {
                    Point a = new(X1, Y1);
                    Point b = new(X2, Y2);
                    if (!adj.TryGetValue(a, out var la)) { la = []; adj[a] = la; }
                    la.Add(b);
                    if (!adj.TryGetValue(b, out var lb)) { lb = []; adj[b] = lb; }
                    lb.Add(a);
                }

                var polygons = new List<List<PointF>>();
                var visitedEdges = new HashSet<(int X1, int Y1, int X2, int Y2)>();

                // Walk cycles
                while (adj.Count > 0)
                {
                    // pick a start node
                    var start = adj.Keys.First();
                    var cycle = new List<PointF>();

                    var current = start;
                    Point? previous = null;

                    //// to guard against pathological cases limit steps to adjacency size * 4
                    //int safetyLimit = adj.Count * 4;
                    //int steps = 0;

                    while (true)
                    {
                        //steps++;
                        //if (steps > safetyLimit)
                        //    break;

                        // add current point to polygon 
                        cycle.Add(ResultPoint(new Point(current.X, current.Y)));

                        // pick next neighbor that is not previous
                        var neighbors = adj[current];
                        Point next;
                        if (neighbors.Count == 0)
                            break;
                        else if (neighbors.Count == 1)
                            next = neighbors[0];
                        else // choose neighbor that's not the node we came from; deterministic tie-break by ordering
                            next = previous.HasValue ? neighbors.FirstOrDefault(n => n != previous.Value) : neighbors.OrderBy(n => (n.X, n.Y)).First();

                        // mark edge visited and remove it from adjacency
                        var ekey = MakeOrderedEdgeKey(current, next);
                        visitedEdges.Add(ekey);

                        // remove the undirected edge
                        adj[current].Remove(next);
                        if (adj[current].Count == 0) adj.Remove(current);
                        if (adj.TryGetValue(next, out var nextList))
                        {
                            nextList.Remove(current);
                            if (nextList.Count == 0) adj.Remove(next);
                        }

                        previous = current;
                        current = next;

                        // closed loop?
                        if (current == start)
                            break;
                    }

                    if (cycle.Count > 2)
                        polygons.Add(cycle);
                }

                return polygons;
            }
        }
    }
}
