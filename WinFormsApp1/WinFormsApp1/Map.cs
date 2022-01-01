using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using Tile = ClassLibrary1.Map.Tile;
using Point = MattUtil.Point;
using DPoint = System.Drawing.Point;

namespace WinFormsApp1
{
    public partial class Map : UserControl
    {
        private const float padding = 1f, scrollTime = 26f, scrollSpeed = 52f;

        private float xStart, yStart, scale;
        private Tile _selected, _moused;
        private bool viewAttacks;

        private readonly Timer timer;
        //private readonly Stopwatch watch;
        private bool scrollDown, scrollLeft, scrollUp, scrollRight;

        public Tile SelTile
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    Center();
                    this.Invalidate();
                    if (Program.Form != null)
                    {
                        Program.Form.Info.Selected = SelTile;
                        Program.RefreshSelected();
                    }
                }
            }
        }
        public Tile MouseTile
        {
            get { return _moused; }
            set
            {
                if (_moused != value)
                {
                    _moused = value;
                    ShowMouseInfo();
                    this.Invalidate();
                }
            }
        }
        public bool ViewAttacks
        {
            get { return viewAttacks; }
            set
            {
                if (viewAttacks != value)
                {
                    viewAttacks = value;
                    RefreshRanges();
                }
            }
        }

        public Map()
        {
            InitializeComponent();
            lblMouse.Text = "";

            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            this.ResizeRedraw = true;

            this.MouseWheel += Map_MouseWheel;

            timer = new();
            timer.Interval = Game.Rand.Round(scrollTime);
            timer.Tick += Timer_Tick;
            //watch = new();
        }

        private void ShowMouseInfo()
        {
            this.lblMouse.Text = "";
            if (MouseTile != null)
            {
                this.lblMouse.Text = string.Format("({0}, {1})", MouseTile.X, MouseTile.Y);
                if (SelTile != null)// && SelTile.Piece != null && SelTile.Piece.HasBehavior(out IMovable movable))// && movable.MoveCur >= 1)
                {
                    double distance = MouseTile.GetDistance(SelTile);
                    //if (distance <= movable.MoveCur)
                    this.lblMouse.Text = distance.ToString("0.0");
                }
            }
        }

        private void Map_Load(object sender, EventArgs e)
        {
            Rectangle gameRect = Program.Game.Map.GameRect();
            scale = Math.Min((this.Width - 1 - padding * 2) / (float)gameRect.Width, (this.Height - 1 - padding * 2) / (float)gameRect.Height);
            xStart = GetX(gameRect.X);
            yStart = GetY(gameRect.Y);

            RefreshRanges();
        }

        public bool Center()
        {
            return Center(SelTile);
        }
        public bool Center(Tile tile)
        {
            Rectangle mapCoords = Rectangle.Inflate(GetMapCoords(), -1, -1);
            if (tile != null && !mapCoords.Contains(tile.X, tile.Y))
            {
                xStart += GetX(tile.X - mapCoords.Width / 2 - 1) + scale / 2;
                yStart += GetY(tile.Y - mapCoords.Height / 2 - 1) + scale / 2;
                this.Invalidate();
                return true;
            }
            return false;
        }

        //private readonly float count = 0f, total = 0f;
        protected override void OnPaint(PaintEventArgs e)
        {
            //watch.Reset();
            //watch.Start();

            //e.Graphics.Clear(Color.White);

            if (Program.Game != null)
            {
                Tiles(e);
                Ranges(e);
            }

            base.OnPaint(e);

            //watch.Stop();
            //float time = watch.ElapsedTicks * 1000f / Stopwatch.Frequency;
            //count++;
            //total += time;
            //Debug.WriteLine(time.ToString("0.0"));
            //Debug.WriteLine(total / count);
        }
        private void Tiles(PaintEventArgs e)
        {
            List<RectangleF> rectangles = new();
            List<RectangleF> ellipses = new();
            List<PointF[]> polygons = new();
            List<Tuple<PointF, PointF>> lines = new();
            Dictionary<int, Brush> hitsBrushes = new();
            Dictionary<Brush, List<RectangleF>> fill = new();
            void AddFill(Brush b, RectangleF r)
            {
                if (!fill.TryGetValue(b, out List<RectangleF> l))
                    fill.Add(b, l = new());
                l.Add(r);
            }
            void AddFillRect(Brush b, float x, float y, float w, float h) => AddFill(b, new(x, y, w, h));

            Rectangle mapCoords = GetMapCoords();
            mapCoords = Rectangle.Intersect(mapCoords, Program.Game.Map.GameRect());

            foreach (Piece piece in Program.Game.Map.GetVisiblePieces())
                if (mapCoords.Contains(piece.Tile.X, piece.Tile.Y))
                {
                    int x = piece.Tile.X, y = piece.Tile.Y;
                    RectangleF rect = new(GetX(x), GetY(y), scale, scale);

                    const float ellipseInflate = -.13f;
                    RectangleF ellipse = RectangleF.Inflate(rect, rect.Width * ellipseInflate, rect.Height * ellipseInflate);

                    Resource resource = piece as Resource;
                    Extractor extractor = piece as Extractor;
                    if (resource == null && extractor != null)
                        resource = extractor.Resource;
                    if (piece is Alien)
                        AddFill(Brushes.Red, rect);
                    else if (piece is Mech)
                        AddFill(Brushes.Green, rect);
                    else if (piece is Constructor)
                        AddFill(Brushes.LightGreen, rect);
                    else if (piece is Core || piece is Factory)
                        AddFill(Brushes.Blue, rect);
                    else if (piece is Foundation)
                        AddFill(Brushes.Black, rect);
                    else if (piece is Turret)
                        ellipses.Add(ellipse);
                    else if (resource != null)
                    {
                        if (resource is Biomass)
                            AddFill(Brushes.Orange, rect);
                        else if (resource is Metal)
                            AddFill(Brushes.Gray, rect);
                        else if (resource is Artifact)
                            AddFill(Brushes.Magenta, rect);
                        if (extractor != null)
                            ellipses.Add(ellipse);
                    }

                    if (Info.HasAnyUpgrade(piece.Tile))
                        polygons.Add(new PointF[] { new(rect.X + rect.Width / 2f, rect.Y), new(rect.Right, rect.Y), new(rect.Right, rect.Y + rect.Height / 2f) });

                    if (piece != null && piece.HasBehavior(out IKillable killable))
                    {
                        float barSize = .169f * rect.Height;
                        RectangleF hitsBar = new(rect.X, rect.Bottom - barSize, rect.Width, barSize);
                        rectangles.Add(hitsBar);

                        float hitsPct = killable.HitsCur / (float)killable.HitsMax;
                        int color = Game.Rand.Round(255 * .65f * killable.Armor);
                        if (!hitsBrushes.TryGetValue(color, out Brush hitsBrush))
                            hitsBrushes.Add(color, hitsBrush = new SolidBrush(Color.FromArgb(color, color, color)));
                        AddFillRect(hitsBrush, hitsBar.X, hitsBar.Y, hitsBar.Width * hitsPct, hitsBar.Height);
                        AddFillRect(Brushes.White, hitsBar.X + hitsBar.Width * hitsPct, hitsBar.Y, hitsBar.Width * (1 - hitsPct), hitsBar.Height);

                        if (killable.ShieldInc > 0)
                        {
                            RectangleF shieldBar = new(hitsBar.X, hitsBar.Y - barSize, hitsBar.Width, barSize);
                            rectangles.Add(shieldBar);

                            float shieldPct = (float)(killable.ShieldCur / killable.ShieldLimit);
                            AddFillRect(Brushes.Purple, shieldBar.X, shieldBar.Y, shieldBar.Width * shieldPct, shieldBar.Height);
                            AddFillRect(Brushes.White, shieldBar.X + shieldBar.Width * shieldPct, shieldBar.Y, shieldBar.Width * (1 - shieldPct), shieldBar.Height);
                            float max = (float)(shieldBar.X + shieldBar.Width * killable.ShieldMax / killable.ShieldLimit);
                            lines.Add(new(new(max, shieldBar.Y), new(max, shieldBar.Bottom)));
                        }
                    }
                }

            for (int a = 0; a < mapCoords.Width; a++)
            {
                int x = mapCoords.X + a;
                for (int b = 0; b < mapCoords.Height; b++)
                {
                    int y = mapCoords.Y + b;
                    if (Program.Game.Map.GetVisibleTile(x, y) != null)
                        rectangles.Add(new(GetX(x), GetY(y), scale, scale));
                }
            }

            HashSet<Brush> afterBrushes = hitsBrushes.Values.Append(Brushes.White).Append(Brushes.Purple).ToHashSet();
            foreach (var p in fill.OrderBy(p => afterBrushes.Contains(p.Key)))
                e.Graphics.FillRectangles(p.Key, p.Value.ToArray());
            foreach (var ellipse in ellipses)
                e.Graphics.FillEllipse(Brushes.Blue, ellipse);
            foreach (var p in polygons)
                e.Graphics.FillPolygon(Brushes.Black, p);

            if (rectangles.Count > 0)
                e.Graphics.DrawRectangles(Pens.Black, rectangles.ToArray());
            foreach (var t in lines)
                e.Graphics.DrawLine(Pens.Black, t.Item1.X, t.Item1.Y, t.Item2.X, t.Item2.Y);
            if (SelTile != null)
            {
                using Pen sel = new(Color.Black, 5f);
                e.Graphics.DrawRectangle(sel, GetX(SelTile.X), GetY(SelTile.Y), scale, scale);
            }

            if (viewAttacks && scale > 21)
            {
                using Font f = new(FontFamily.GenericMonospace, scale / 5.6f);
                foreach (var p in attacks)
                    if (mapCoords.Contains(p.Key.X, p.Key.Y))
                        e.Graphics.DrawString(p.Value, f, Brushes.Red, new PointF(GetX(p.Key.X) + scale - e.Graphics.MeasureString(p.Value, f).Width, GetY(p.Key.Y) + scale - f.Size * 2 - 2));
            }

            foreach (IDisposable d in hitsBrushes.Values)
                d.Dispose();
        }

        private readonly static Pen Red = new(Color.Red, 3f), Green = new(Color.Green, 3f), Blue = new(Color.Blue, 3f), Black = new(Color.Black, 3f);
        private readonly Dictionary<Pen, List<HashSet<Point>>> ranges = new Pen[] { Blue, Red, Green, Black }.ToDictionary(p => p, p => new List<HashSet<Point>>());
        private readonly Dictionary<Point, string> attacks = new();
        public void RefreshRanges()
        {
            foreach (var pair in ranges)
                pair.Value.Clear();

            ShowMouseInfo();

            HashSet<Point> edges = new();
            ranges[Black].Add(edges);
            Rectangle rect = Program.Game.Map.GameRect();
            for (int a = 0; a < rect.Width; a++)
            {
                int x = rect.X + a;
                for (int b = 0; b < rect.Height; b++)
                {
                    int y = rect.Y + b;
                    if (Program.Game.Map.Visible(x, y) && Program.Game.Map.GetVisibleTile(x, y) == null)
                        edges.Add(new(x, y));
                }
            }

            foreach (IBuilder b in Program.Game.Player.PiecesOfType<IBuilder>())
                ranges[Blue].Add(GetPoints(b.Piece.Tile.GetVisibleTilesInRange(b.Range)));

            if (viewAttacks)
            {
                Dictionary<Tile, double> attStr = new();
                void AddAttStr(IEnumerable<Tile> range, double damage)
                {
                    foreach (Tile t in range)
                        if (t.Piece == null || !t.Piece.IsEnemy)
                        {
                            attStr.TryGetValue(t, out double total);
                            attStr[t] = total + damage;
                        }
                }

                IEnumerable<Point> allAttacks = Enumerable.Empty<Point>();
                foreach (IAttacker enemy in Program.Game.Enemy.VisiblePieces.Select(e => e.GetBehavior<IAttacker>()).Where(b => b != null))
                {
                    IEnumerable<Tile> attackerTiles = new Tile[] { enemy.Piece.Tile };
                    if (enemy.HasBehavior(out IMovable enemyMovable))
                        attackerTiles = enemyMovable.Piece.Tile.GetVisibleTilesInRange(enemyMovable.MoveCur);
                    allAttacks = allAttacks.Union(AddAttacks(enemy, attackerTiles, AddAttStr).SelectMany(hs => hs));
                }
                ranges[Red].Add(allAttacks.ToHashSet());

                attacks.Clear();
                foreach (var p in attStr)
                    attacks.Add(new Point(p.Key.X, p.Key.Y), p.Value.ToString("0"));
            }
            this.Invalidate();
        }
        private void Ranges(PaintEventArgs e)
        {
            Dictionary<Pen, List<HashSet<Point>>> ranges = this.ranges;
            if (SelTile != null)
            {
                //clone
                ranges = ranges.ToDictionary(p => p.Key, p => p.Value.ToList());

                IEnumerable<Tile> moveTiles = Enumerable.Empty<Tile>();
                if (SelTile.Piece != null && SelTile.Piece.HasBehavior(out IMovable movable) && movable.MoveCur >= 1)
                {
                    moveTiles = movable.Piece.Tile.GetVisibleTilesInRange(movable.MoveCur)
                        .Where(t => t.Piece == null || (t.Piece.Side == movable.Piece.Side && t.Piece.HasBehavior<IMovable>()));
                    ranges[Green].Add(GetPoints(moveTiles));
                    if (SelTile.Piece.IsPlayer && movable.MoveCur + movable.MoveInc > movable.MoveMax)
                        ranges[Green].Add(GetPoints(moveTiles.Where(t => Math.Min(movable.MoveCur - 1, movable.MoveCur + movable.MoveInc - movable.MoveMax) > t.GetDistance(SelTile))));
                }

                if (SelTile.Piece != null && SelTile.Piece.HasBehavior(out IAttacker attacker))
                    ranges[Red].AddRange(AddAttacks(attacker, moveTiles, null));

                if (SelTile.Piece != null && SelTile.Piece.HasBehavior(out IBuilder b) && moveTiles.Contains(MouseTile))
                    ranges[Blue].Add(GetPoints(MouseTile.GetVisibleTilesInRange(b.Range)));
            }

            List<Pen> dipose = new();
            Dictionary<LineSegment, Pen> lines = new();
            foreach (var pair in ranges)
                foreach (var tiles in pair.Value)
                    foreach (Point t in tiles)
                    {
                        bool Show(Point p) => !tiles.Contains(p);
                        void AddLine(int x1, int y1, int x2, int y2)
                        {
                            LineSegment l = new(x1, y1, x2, y2);
                            if (lines.TryGetValue(l, out Pen oth))
                            {
                                if (pair.Key != oth)
                                {
                                    lines[l] = Combine(pair.Key, oth);
                                    dipose.Add(lines[l]);
                                }
                            }
                            else
                                lines.Add(l, pair.Key);
                        };
                        if (Show(new(t.X - 1, t.Y)))
                            AddLine(t.X, t.Y, t.X, t.Y + 1);
                        if (Show(new(t.X + 1, t.Y)))
                            AddLine(t.X + 1, t.Y, t.X + 1, t.Y + 1);
                        if (Show(new(t.X, t.Y - 1)))
                            AddLine(t.X, t.Y, t.X + 1, t.Y);
                        if (Show(new(t.X, t.Y + 1)))
                            AddLine(t.X, t.Y + 1, t.X + 1, t.Y + 1);
                    }

            //foreach (var t in lines)
            //    e.Graphics.DrawLine(t.Value, GetX(t.Key.x1), GetY(t.Key.y1), GetX(t.Key.x2), GetY(t.Key.y2));

            Dictionary<Pen, Range> edges = new();
            foreach (var t in lines)
            {
                if (!edges.TryGetValue(t.Value, out Range r))
                    edges.Add(t.Value, r = new());
                r.AddSegment(t.Key);
            }

            //int calls = 0;
            Rectangle mapCoords = GetMapCoords();
            PointF[] points;
            foreach (var p in edges)
                do
                {
                    points = p.Value.GetNext(mapCoords, GetX, GetY);
                    if (points.Length > 0)
                    {
                        //calls++;
                        e.Graphics.DrawLines(p.Key, points);
                    }
                } while (points.Length > 0);
            //Debug.WriteLine("draws: " + calls);

            foreach (var d in dipose)
                d.Dispose();
        }
        private IEnumerable<HashSet<Point>> AddAttacks(IAttacker attacker, IEnumerable<Tile> moveTiles, Action<IEnumerable<Tile>, double> AddAttStr)
        {
            List<HashSet<Point>> retVal = new();
            var ar = attacker.Attacks.Where(a => !a.Attacked);
            if (attacker.Piece.IsEnemy && moveTiles.Any())
            {
                //var moveEdge = moveTiles.Where(t => t.GetDistance(attacker.Piece.Tile) > attacker.Piece.GetBehavior<IMovable>().MoveCur - 1);
                foreach (var a in ar)
                {
                    IEnumerable<Tile> e = moveTiles.SelectMany(t => t.GetVisibleTilesInRange(a.Range)).Union(moveTiles);
                    AddAttStr?.Invoke(e, a.Damage);
                    retVal.Add(GetPoints(e));
                }
            }
            else
            {
                foreach (var a in ar)
                    if (moveTiles.Contains(MouseTile))
                        retVal.Add(GetPoints(MouseTile.GetVisibleTilesInRange(a.Range)));
                    else
                    {
                        IEnumerable<Tile> e = SelTile.GetVisibleTilesInRange(a.Range);
                        retVal.Add(GetPoints(e));
                    }
            }
            return retVal;
        }
        private static HashSet<Point> GetPoints(IEnumerable<Tile> ts) => ts.Select(t => new Point(t.X, t.Y)).ToHashSet();
        private static Pen Combine(Pen pen, Pen oth)
        {
            const int factor = 2;
            return new Pen(Color.FromArgb((pen.Color.R + oth.Color.R) / factor, (pen.Color.G + oth.Color.G) / factor, (pen.Color.B + oth.Color.B) / factor), (pen.Width + oth.Width) / 2f);
        }
        private class Range
        {
            private int count = 1;
            private readonly Dictionary<Point, List<Point>> range = new();
            public void AddSegment(LineSegment key)
            {
                count++;
                Point p1 = new(key.x1, key.y1);
                Point p2 = new(key.x2, key.y2);
                for (int a = 0; a < 2; a++)
                {
                    if (a == 1)
                    {
                        Point temp = p1;
                        p1 = p2;
                        p2 = temp;
                    }
                    if (!range.TryGetValue(p1, out List<Point> list))
                        range.Add(p1, list = new(4));
                    list.Add(p2);
                }
            }
            public PointF[] GetNext(Rectangle mapCoords, Func<int, float> GetX, Func<int, float> GetY)
            {
                List<PointF> all = new(count);
                Point start = range.Keys.FirstOrDefault(p => OnMap(mapCoords, p));
                //Debug.WriteLine(start);
                Point cur = start;
                while (range.TryGetValue(cur, out List<Point> list))
                {
                    Point next = list[0];

                    list.Remove(next);
                    if (list.Count == 0)
                        range.Remove(cur);

                    List<Point> other = range[next];
                    other.Remove(cur);
                    if (other.Count == 0)
                        range.Remove(next);

                    if (OnMap(mapCoords, cur) || OnMap(mapCoords, next))
                    {
                        all.Add(GetPoint(GetX, GetY, cur));
                        cur = next;
                        //all.Add(GetPoint(GetX, GetY, next));
                    }
                }
                if (all.Count > 0)
                    all.Add(GetPoint(GetX, GetY, cur));
                //if (all.Count > 0 && (cur == start || Math.Abs(start.X - cur.X) + Math.Abs(start.Y - cur.Y) == 1))
                //    all.Add(GetPoint(GetX, GetY, start));
                //Debug.WriteLine("count: " + all.Count);
                //Debug.WriteLine("");
                //Debug.WriteLine(count);
                //Debug.WriteLine(all.Capacity);
                //Debug.WriteLine(all.Count);
                if (all.Count > count || all.Capacity > count)
                    ;
                //else if (all.Count == count)
                //    Debug.WriteLine("Capacity");
                return all.ToArray();
            }
            private static PointF GetPoint(Func<int, float> GetX, Func<int, float> GetY, Point p)
            {
                return new(GetX(p.X), GetY(p.Y));
            }
            private static bool OnMap(Rectangle mapCoords, Point p)
            {
                //return true;
                return mapCoords.Contains(new System.Drawing.Point(p.X, p.Y));
            }
        }
        private class LineSegment
        {
            public int x1, y1, x2, y2;
            public LineSegment(int x1, int y1, int x2, int y2)
            {
                if (x1 < x2 || x1 == x2 && y1 < y2)
                {
                    this.x1 = x1;
                    this.y1 = y1;
                    this.x2 = x2;
                    this.y2 = y2;
                }
                else
                {
                    this.x1 = x2;
                    this.y1 = y2;
                    this.x2 = x1;
                    this.y2 = y1;
                }
            }
            public override int GetHashCode()
            {
                int result = 0;
                int mult = 1 << 8;
                result = mult * result + x1;
                result = mult * result + x2;
                result = mult * result + y1;
                result = mult * result + y2;
                return result;
            }
            public override bool Equals(object obj)
            {
                return obj is LineSegment o && x1 == o.x1 && x2 == o.x2 && y1 == o.y1 && y2 == o.y2;
            }
        }

        private float GetX(int x)
        {
            return GetCoord(x, xStart);
        }
        private float GetY(int y)
        {
            return GetCoord(y, yStart);
        }
        private float GetCoord(int val, float start)
        {
            return val * scale - start + padding;
        }

        private int GetMapX(float x)
        {
            return GetMapCoord(x, xStart);
        }
        private int GetMapY(float y)
        {
            return GetMapCoord(y, yStart);
        }
        private int GetMapCoord(float val, float start)
        {
            return (int)Math.Floor((start + val - padding) / scale);
        }

        private Rectangle GetMapCoords()
        {
            int x = GetMapX(0), y = GetMapY(0), w = GetMapX(Width), h = GetMapY(Height);
            return new Rectangle(x, y, w - x + 1, h - y + 1);
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            this.MouseTile = Program.Game.Map.GetVisibleTile(GetMapX(e.X), GetMapY(e.Y));
        }

        public void Map_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                scrollDown = true;
            else if (e.KeyCode == Keys.A)
                scrollLeft = true;
            else if (e.KeyCode == Keys.S)
                scrollUp = true;
            else if (e.KeyCode == Keys.D)
                scrollRight = true;
            if (scrollDown || scrollLeft || scrollUp || scrollRight)
                timer.Start();
        }

        public void Map_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                scrollDown = false;
            else if (e.KeyCode == Keys.A)
                scrollLeft = false;
            else if (e.KeyCode == Keys.S)
                scrollUp = false;
            else if (e.KeyCode == Keys.D)
                scrollRight = false;
            if (!(scrollDown || scrollLeft || scrollUp || scrollRight))
                timer.Stop();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            float xs = xStart, ys = yStart;

            float scrollAmt = Game.Rand.Gaussian(scrollSpeed, .013f);
            if (scrollDown && !scrollUp)
                this.yStart -= scrollAmt;
            if (scrollUp && !scrollDown)
                this.yStart += scrollAmt;
            if (scrollLeft && !scrollRight)
                this.xStart -= scrollAmt;
            if (scrollRight && !scrollLeft)
                this.xStart += scrollAmt;

            CheckBounds(xs, ys);
            Invalidate();
        }
        public void Map_MouseWheel(object sender, MouseEventArgs e)
        {
            float mult = e.Delta / Game.Rand.GaussianCapped(91f, .026f, 1);
            if (mult < 0)
                mult = -1 / mult;

            Tile sel = MouseTile ?? SelTile;
            Point anchor;
            if (sel != null && GetMapCoords().Contains(sel.X, sel.Y))
                anchor = new Point(sel.X, sel.Y);
            else
                anchor = new Point(GetMapX(Width / 2f), GetMapY(Height / 2f));
            float selX = GetX(anchor.X) + scale / 2f;
            float selY = GetY(anchor.Y) + scale / 2f;

            this.scale *= mult;
            if (scale < 3)
                scale = 3;
            else if (scale > 169)
                scale = 169;
            if (this.CheckBounds(xStart, yStart))
                this.scale /= mult;

            xStart += GetX(anchor.X) + scale / 2f - selX;
            yStart += GetY(anchor.Y) + scale / 2f - selY;

            this.Invalidate();
        }
        private bool CheckBounds(float xs, float ys)
        {
            bool retVal = false;
            Rectangle mapCoords = GetMapCoords();
            Rectangle gameRect = Program.Game.Map.GameRect();
            if (mapCoords.Right - 1 <= gameRect.Left || mapCoords.Left >= gameRect.Right - 1)
            {
                retVal = true;
                this.xStart = xs;
            }
            if (mapCoords.Bottom - 1 <= gameRect.Top || mapCoords.Top >= gameRect.Bottom - 1)
            {
                retVal = true;
                this.yStart = ys;
            }
            return retVal;
        }

        private void Map_MouseClick(object sender, MouseEventArgs e)
        {
            scrollDown = false;
            scrollLeft = false;
            scrollUp = false;
            scrollRight = false;

            int x = GetMapX(e.X), y = GetMapY(e.Y);

            Tile clicked = Program.Game.Map.GetVisibleTile(x, y);
            Tile orig = SelTile;

            if (e.Button == MouseButtons.Left)
            {
                SelTile = clicked;
            }
            else if (e.Button == MouseButtons.Right && SelTile != null && SelTile.Piece != null && clicked != null)
            {
                if (SelTile.Piece.HasBehavior(out IMovable movable))
                {
                    if (movable.Move(clicked))
                    {
                        SelTile = clicked;
                        Program.RefreshChanged();
                    }
                }
                if (SelTile.Piece.HasBehavior(out IAttacker attacker) && clicked.Piece != null && clicked.Piece.HasBehavior(out IKillable killable))
                {
                    if (attacker.Fire(killable))
                    {
                        SelTile = clicked;
                        Program.RefreshChanged();
                    }
                }
            }

            if (SelTile == orig)
            {
                if (SelTile != null && SelTile == clicked)
                    Program.Hold();
                else
                    Program.Next(true);
            }
        }
    }
}
