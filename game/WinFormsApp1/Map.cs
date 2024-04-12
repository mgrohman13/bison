using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using DPoint = System.Drawing.Point;
using Point = MattUtil.Point;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace WinFormsApp1
{
    public partial class Map : UserControl
    {
        private const float padding = 1f, scrollTime = 39f, scrollSpeed = 13f;

        private float xStart, yStart, _scale;
        private Tile _selected, _moused;
        private bool viewAttacks;

        private readonly Timer timer;
        private readonly Stopwatch watch = new();
        private bool scrollDown, scrollLeft, scrollUp, scrollRight;

        private static Pen Red, Green, Blue, White;
        private Pen[] rgbw = new[] { Red, Green, Blue, White };
        private Dictionary<Pen, List<HashSet<Point>>> ranges;
        private readonly Dictionary<Point, float> attacks = new();

        private new float Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    float size = Game.Rand.Round(Math.Max(3, Math.Sqrt(Scale) - 1));
                    if (Red?.Width != size)
                    {
                        Red = new(Color.Red, size);
                        Green = new(Color.Green, size);
                        Blue = new(Color.Blue, size);
                        White = new(Color.White, size);
                        Pen[] rgbwNew = new Pen[] { Red, Green, Blue, White };
                        if (ranges != null)
                            ranges = ranges.ToDictionary(p => rgbwNew[Array.IndexOf(rgbw, p.Key)], p => p.Value);
                        rgbw = rgbwNew;
                        ranges ??= new Pen[] { Blue, Red, Green, White }.ToDictionary(p => p, p => new List<HashSet<Point>>());
                    }
                }

                _scale = value;
            }
        }

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
                float range = -1;
                bool HasSel() => !scrollDown && !scrollLeft && !scrollUp && !scrollRight && (SelTile?.Piece?.IsPlayer).GetValueOrDefault();
                //check blocks
                bool InRange() => HasSel() && _moused != null && _moused.GetDistance(SelTile) <= range;
                if (HasSel())
                {
                    if (SelTile.Piece.HasBehavior(out IBuilder b))
                        range = (float)Math.Max(range, b.Range);
                    if (SelTile.Piece.HasBehavior(out IMovable m))
                        range = (float)Math.Max(range, m.MoveCur);
                }

                if (_moused != value)
                {
                    bool invalidate = InRange();
                    _moused = value;

                    ShowMouseInfo();
                    if (invalidate || InRange())
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
            //lblMouse.AutoSize\
            //lblMouse.

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            this.ResizeRedraw = true;

            this.MouseWheel += Map_MouseWheel;

            timer = new()
            {
                Interval = Game.Rand.Round(scrollTime)
            };
            timer.Tick += Timer_Tick;
            //watch = new();
        }

        private void ShowMouseInfo()
        {
            this.lblMouse.Text = "";
            if (MouseTile != null)
            {
                int sign = Game.TEST_MAP_GEN.HasValue ? 1 : -1;
                Tile center = Game.TEST_MAP_GEN.HasValue ? Program.Game.Map.GetVisibleTile(0, 0) : Program.Game.Player.Core.Tile;
                this.lblMouse.Text = string.Format("({0}, {1})", MouseTile.X - center.X, sign * (MouseTile.Y - center.Y));
                if (SelTile != null)// && SelTile.Piece != null && SelTile.Piece.HasBehavior(out IMovable movable))// && movable.MoveCur >= 1)
                {
                    float distance = (float)MouseTile.GetDistance(SelTile);
                    //if (distance <= movable.MoveCur)
                    this.lblMouse.Text = distance.ToString("0.0");
                    this.lblMouse.Refresh();
                }
            }
        }

        private void Map_Load(object sender, EventArgs e)
        {
            ClassLibrary1.Map.Map.LogEvalTime();

            Rectangle gameRect = Program.Game.Map.GameRect();
            Scale = Math.Min((this.Width - 1 - padding * 2) / gameRect.Width, (this.Height - 1 - padding * 2) / gameRect.Height);
            xStart = GetX(gameRect.X);
            yStart = GetY(gameRect.Y);

            RefreshRanges();

            lblMouse.Location = new DPoint(0, this.ClientSize.Height - lblMouse.Height);

            ClassLibrary1.Map.Map.LogEvalTime();
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
                xStart += GetX(tile.X - mapCoords.Width / 2 - 1) + Scale / 2;
                yStart += GetY(tile.Y - mapCoords.Height / 2 - 1) + Scale / 2;
                this.Invalidate();
                return true;
            }
            return false;
        }

        //private readonly float count = 0f, total = 0f;
        private float paintTime;
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                watch.Reset();
                watch.Start();

                e.Graphics.Clear(Color.Black);

                if (Program.Game != null)
                {
                    Tiles(e);
                    Ranges(e);
                }

                base.OnPaint(e);
                paintTime = watch.ElapsedTicks * 1000f / Stopwatch.Frequency;

                Debug.WriteLine("OnPaint: " + paintTime);
                watch.Stop();

                ClassLibrary1.Map.Map.LogEvalTime();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
        }
        private void Tiles(PaintEventArgs e)
        {
            const float defHeight = .169f, statBarPow = .39f;

            List<RectangleF> rectangles = new();
            List<RectangleF> ellipses = new();
            List<PointF[]> polygons = new();
            List<Tuple<PointF, PointF>> lines = new();
            Dictionary<int, Brush> hitsBrushes = new();
            Dictionary<Brush, List<RectangleF>> fill = new();
            Dictionary<RectangleF, string> letters = new();
            void AddFill(Brush b, RectangleF r)
            {
                if (!fill.TryGetValue(b, out List<RectangleF> l))
                    fill.Add(b, l = new());
                l.Add(r);
            }
            //void AddFillRect(Brush b, float x, float y, float w, float h) => AddFill(b, new(x, y, w, h));

            Rectangle mapCoords = GetMapCoords();
            mapCoords = Rectangle.Intersect(mapCoords, Program.Game.Map.GameRect());

            static IEnumerable<T> GetPieces<T>() where T : class, IBehavior => Program.Game.Map.GetVisiblePieces().SelectMany(p => p.GetBehaviors<T>());
            //static float? SumAttacksCur(IAttacker attacker) => SumAttacks(attacker, a => a.AttackCur);
            static float? SumAttacksMax(IAttacker attacker) => SumAttacks(attacker, a => a.AttackMax);
            static float? SumAttacks(IAttacker attacker, Func<Attack, int> Stat) => attacker.Attacks.Sum(a => StatValue(Stat(a)));
            static float StatValue(float stat) => (float)Consts.StatValue(stat);
            const float padding = 1.69f;
            float attackMax = (GetPieces<IAttacker>().Where(k => k.Piece.HasBehavior<IKillable>() && k.Piece.HasBehavior<IMovable>())
                .Max<IAttacker>(SumAttacksMax) ?? 1) * padding;
            float defenseMax = (GetPieces<IKillable>().Where(k => k.Piece.HasBehavior<IAttacker>() && k.Piece.HasBehavior<IMovable>())
                .Max(k => k.TotalDefenses.Sum(d => (float?)StatValue(d.DefenseMax))) ?? 1) * padding;
            attackMax = defenseMax = Math.Max(attackMax, defenseMax);

            foreach (Piece piece in Program.Game.Map.GetVisiblePieces())
                if (mapCoords.Contains(piece.Tile.X, piece.Tile.Y))
                {
                    int x = piece.Tile.X, y = piece.Tile.Y;
                    RectangleF rect = new(GetX(x), GetY(y), Scale, Scale);

                    const float ellipseInflate = -.13f;
                    RectangleF ellipse = RectangleF.Inflate(rect, rect.Width * ellipseInflate, rect.Height * ellipseInflate);

                    Resource resource = piece as Resource;
                    Extractor extractor = piece as Extractor;
                    if (resource == null && extractor != null)
                        resource = extractor.Resource;
                    if (piece is EnemyPiece)
                    {
                        if (piece is Hive)
                            AddFill(Brushes.IndianRed, rect);
                        else
                            AddFill(Brushes.Red, rect);
                        if (piece is Alien alien && alien.LastMove != null)
                        {
                            PointF GetCenter(Tile p) => new(GetX(p.X) + Scale / 2f, GetY(p.Y) + Scale / 2f);
                            var p1 = GetCenter(alien.Tile);
                            var p2 = GetCenter(alien.LastMove);
                            lines.Add(new(p1, p2));
                        }
                    }
                    else if (piece is Mech mech)
                    {
                        AddFill(Brushes.Green, rect);
                        letters.Add(rect, mech.Blueprint.BlueprintNum);
                    }
                    else if (piece is Constructor)
                        AddFill(Brushes.LightGreen, rect);
                    else if (piece is Core || piece is Factory)
                        AddFill(Brushes.Blue, rect);
                    else if (piece is Foundation)
                        AddFill(Brushes.Aqua, rect);
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
                        float barSize = defHeight * rect.Height;
                        //statBarPow smaller bars bigger which helps visual clarity 
                        float widthTotal = (float)Math.Pow(killable.TotalDefenses.Sum(k => StatValue(k.DefenseMax)) / defenseMax, statBarPow);
                        if (widthTotal > 1)
                            widthTotal = 1;
                        widthTotal *= rect.Width;

                        Func<Defense, int?> Get = d => d.DefenseCur;
                        float cur1 = GetValue(DefenseType.Hits, Get);
                        float cur2 = GetValue(DefenseType.Armor, Get);
                        float cur3 = GetValue(DefenseType.Shield, Get);
                        Get = d => d.DefenseMax;
                        float max1 = GetValue(DefenseType.Hits, Get);
                        float max2 = GetValue(DefenseType.Armor, Get);
                        float max3 = GetValue(DefenseType.Shield, Get);
                        float GetValue(DefenseType type, Func<Defense, int?> GetStat) =>
                            killable.TotalDefenses.Where(d => d.Type == type).Sum(a => StatValue(GetStat(a) ?? 0));//StatValue?

                        DrawBar(1, new float[] { cur1, cur2, cur3 }, new float[] { max1, max2, max3 },
                            new Brush[] { Brushes.DarkGray, Brushes.LightGray, Brushes.SkyBlue, }, barSize, widthTotal);
                    }
                    if (piece != null && piece.HasBehavior(out IAttacker attacker))
                    {
                        float barSize = defHeight * rect.Height;
                        float widthTotal = (float)Math.Pow((SumAttacksMax(attacker) ?? 0) / attackMax, statBarPow);
                        if (widthTotal > 1)
                            widthTotal = 1;
                        widthTotal *= rect.Width;

                        Func<Attack, int?> Get = a => a.AttackCur;
                        float cur1 = GetValue(AttackType.Kinetic, Get);
                        float cur2 = GetValue(AttackType.Energy, Get);
                        float cur3 = GetValue(AttackType.Explosive, Get);
                        Get = a => a.AttackMax;
                        float max1 = GetValue(AttackType.Kinetic, Get);
                        float max2 = GetValue(AttackType.Energy, Get);
                        float max3 = GetValue(AttackType.Explosive, Get);
                        float GetValue(AttackType type, Func<Attack, int?> GetStat) =>
                            attacker.Attacks.Where(d => d.Type == type).Sum(a => StatValue(GetStat(a) ?? 0));//StatValue?

                        DrawBar(2, new float[] { cur1, cur2, cur3 }, new float[] { max1, max2, max3 },
                            new Brush[] { Brushes.Silver, Brushes.SandyBrown, Brushes.MediumPurple, }, barSize, widthTotal);
                    }

                    void DrawBar(int barNum, float[] curs, float[] maxes, Brush[] brushes, float barSize, float widthTotal)
                    {
                        RectangleF defBar = new(rect.X, rect.Bottom - barSize * barNum, widthTotal, barSize);
                        rectangles.Add(defBar);
                        float curX = defBar.X;

                        float total = maxes.Sum();
                        for (int a = 0; a < curs.Length; a++)
                        {
                            float mult = defBar.Width / total;
                            float cur = curs[a] * mult;
                            float max = maxes[a] * mult;
                            AddFill(brushes[a], new(curX, defBar.Y, cur, defBar.Height));
                            curX += cur;
                            if (cur < max)
                                AddFill(Brushes.White, new(curX, defBar.Y, max - cur, defBar.Height));
                            curX += max - cur;
                            if (a + 1 < curs.Length)
                                lines.Add(new(new(curX, defBar.Y), new(curX, defBar.Bottom)));
                        }
                    }

                    //if (piece != null && piece.HasBehavior(out IAttacker attacker))
                    //{
                    //    float attacks = SumAttacksMax(attacker) ?? 0;
                    //    float barSize = defHeight * rect.Height;
                    //    float width = rect.Width * (float)Math.Pow(attacks / attackMax, statBarPow);
                    //    RectangleF hitsBar = new(rect.X, rect.Bottom - barSize * 2, width, barSize);
                    //    rectangles.Add(hitsBar);

                    //    float attPct = (SumAttacksCur(attacker) ?? 0) / attacks;

                    //    RectangleF filled = new(hitsBar.X, hitsBar.Y, hitsBar.Width * attPct, hitsBar.Height);
                    //    AddFill(Brushes.Purple, filled);
                    //    RectangleF empty = new(hitsBar.X + hitsBar.Width * attPct, hitsBar.Y, hitsBar.Width * (1 - attPct), hitsBar.Height);
                    //    AddFill(Brushes.White, empty);
                    //}
                }

            for (int b = 0; b < mapCoords.Width; b++)
            {
                int x = mapCoords.X + b;
                for (int c = 0; c < mapCoords.Height; c++)
                {
                    int y = mapCoords.Y + c;
                    RectangleF rect = new(GetX(x), GetY(y), Scale, Scale);
                    if (Program.Game.Map.Visible(x, y))
                    {
                        Tile tile = Program.Game.Map.GetVisibleTile(x, y);
                        Terrain terrain = tile?.Terrain;
                        if (terrain != null)
                        {
                            int color = 169;
                            if (terrain is Block block)
                                color = Game.Rand.Round(color * block.Value);
                            //Color.Brown
                            AddFill(new SolidBrush(Color.FromArgb(130, color, 26)), rect);
                        }
                        else if (tile != null)
                            rectangles.Add(rect);
                        else if (!Game.TEST_MAP_GEN.HasValue)
                            AddFill(Brushes.DarkGray, rect);
                    }
                }
            }

            if (viewAttacks)// && scale > 16.9f)
            {
                using Font f = new(FontFamily.GenericMonospace, Scale / 3.9f);
                foreach (var p in attacks)
                {
                    int x = p.Key.X, y = p.Key.Y;
                    if (mapCoords.Contains(p.Key.X, p.Key.Y))
                    {
                        RectangleF rect = new(GetX(x), GetY(y), Scale, Scale);
                        float barSize = defHeight * rect.Height;

                        float widthTotal = (float)Math.Pow(StatValue(p.Value) / defenseMax, statBarPow);
                        if (widthTotal > 1)
                            widthTotal = 1;
                        widthTotal *= rect.Width;

                        //new(rect.X, rect.Y, cur, defBar.Height));

                        RectangleF attBar = new(rect.X, rect.Bottom - barSize * 1, widthTotal, barSize);
                        if (Program.Game.Map.GetVisibleTile(x, y).Piece != null)
                        {
                            //AddFill(Brushes.PaleVioletRed, attBar);
                        }
                        else
                        {
                            AddFill(Brushes.LightPink, attBar);
                            lines.Add(new(new(attBar.Right, attBar.Y), new(attBar.Right, attBar.Bottom)));
                        }
                        //SizeF size = e.Graphics.MeasureString(p.Value, f);
                        //e.Graphics.DrawString(p.Value, f, Brushes.Red, new PointF(GetX(p.Key.X) + scale - size.Width, GetY(p.Key.Y) + scale - size.Height));
                    }
                }
            }

            RectangleF[] rects = rectangles.ToArray();
            if (rects.Length > 0)
                e.Graphics.FillRectangles(Brushes.White, rects);

            HashSet<Brush> afterBrushes = hitsBrushes.Values.Concat(new[] { Brushes.White,
                Brushes.DarkGray, Brushes.LightGray, Brushes.SkyBlue, Brushes.Silver, Brushes.SandyBrown , Brushes.MediumPurple,
            }).ToHashSet();
            foreach (var p in fill)
                if (!afterBrushes.Contains(p.Key))
                    e.Graphics.FillRectangles(p.Key, p.Value.ToArray());
            foreach (var ellipse in ellipses)
                e.Graphics.FillEllipse(Brushes.Blue, ellipse);
            foreach (var p in fill)
                if (afterBrushes.Contains(p.Key))
                    e.Graphics.FillRectangles(p.Key, p.Value.ToArray());
            foreach (var p in polygons)
                e.Graphics.FillPolygon(Brushes.Black, p);


            //foreach (var found in Program.Game.Map.FoundPaths.Values.Distinct())
            //{
            //    var path = found.Path;
            //    for (int d = 1; d < path.Count; d++)
            //    {
            //        PointF GetCenter(Point p) => new(GetX(p.X) + Scale / 2f, GetY(p.Y) + Scale / 2f);
            //        var p1 = GetCenter(path[d - 1]);
            //        var p2 = GetCenter(path[d]);
            //        e.Graphics.DrawLine(new Pen(Color.DarkGreen, 2f), p1.X, p1.Y, p2.X, p2.Y);
            //    }
            //}

            if (rects.Length > 0)
                e.Graphics.DrawRectangles(Pens.Black, rects);
            foreach (var t in lines)
                e.Graphics.DrawLine(Pens.Black, t.Item1.X, t.Item1.Y, t.Item2.X, t.Item2.Y);
            if (SelTile != null)
            {
                using Pen sel = new(Color.Black, 5f);
                e.Graphics.DrawRectangle(sel, GetX(SelTile.X), GetY(SelTile.Y), Scale, Scale);
            }

            if (viewAttacks && Scale > 16.9f)
            {
                using Font f = new(FontFamily.GenericMonospace, Scale / 3.9f);
                foreach (var p in attacks)
                    if (mapCoords.Contains(p.Key.X, p.Key.Y))
                    {
                        string value = p.Value.ToString("0");
                        SizeF size = e.Graphics.MeasureString(value, f);
                        e.Graphics.DrawString(value, f, Brushes.Red, new PointF(GetX(p.Key.X) + Scale - size.Width, GetY(p.Key.Y) + Scale - size.Height));
                    }
            }
            foreach (var p in letters)
            {
                using Font f = new(FontFamily.GenericMonospace, Scale / 2.6f);
                SizeF size = e.Graphics.MeasureString(p.Value, f);
                e.Graphics.DrawString(p.Value, f, Brushes.Black, p.Key.X + (Scale - size.Width) / 2f, p.Key.Y);
            }

            foreach (IDisposable d in hitsBrushes.Values)
                d.Dispose();
        }

        public void RefreshRanges()
        {
            //watch.Reset();
            //watch.Start();

            foreach (var pair in ranges)
                pair.Value.Clear();

            ShowMouseInfo();

            //Debug.WriteLine("1 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            HashSet<Point> edges = new();
            ranges[White].Add(edges);
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

            //Debug.WriteLine("2 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            foreach (IBuilder b in Program.Game.Player.PiecesOfType<IBuilder>())
                ranges[Blue].Add(b.Piece.Tile.GetPointsInRange(b).ToHashSet());

            //Debug.WriteLine("3 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            if (viewAttacks)
            {
                Dictionary<Tile, float> attStr = new();
                void AddAttStr(IEnumerable<Point> range, float damage)
                {
                    foreach (Tile t in range.Select(Program.Game.Map.GetVisibleTile).Where(t => t != null))
                        if (t.Piece == null || !t.Piece.IsEnemy)
                        {
                            attStr.TryGetValue(t, out float total);
                            attStr[t] = (float)(total + Consts.StatValue(damage));
                        }
                }

                IEnumerable<Point> allAttacks = Enumerable.Empty<Point>();
                foreach (IAttacker enemy in Program.Game.Enemy.VisiblePieces.Select(e => e.GetBehavior<IAttacker>()).Where(b => b != null))
                    allAttacks = allAttacks.Union(AddAttacks(enemy, AddAttStr).SelectMany(hs => hs));
                ranges[Red].Add(allAttacks.ToHashSet());

                attacks.Clear();
                foreach (var p in attStr)
                    attacks.Add(new Point(p.Key.X, p.Key.Y), (float)Consts.StatValueInverse(p.Value));
            }

            //Debug.WriteLine("4 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            this.Invalidate();

            //Debug.WriteLine("RefreshRanges: " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);
            //watch.Stop();
        }
        private void Ranges(PaintEventArgs e)
        {
            //watch.Reset();
            //watch.Start();

            Dictionary<Pen, List<HashSet<Point>>> ranges = this.ranges;
            if (SelTile != null)
            {
                //clone
                ranges = ranges.ToDictionary(p => p.Key, p => p.Value.ToList());

                //Debug.WriteLine("1 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

                HashSet<Point> moveTiles = new();
                IMovable movable = SelTile.Piece?.GetBehavior<IMovable>();
                if (SelTile.Piece != null && movable != null && movable.MoveCur >= 1)
                {

                    //Debug.WriteLine("2 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

                    //if ( SelTile.Piece.IsEnemy)
                    Tile t;
                    moveTiles = movable.Piece.Tile.GetPointsInRange(movable)
                        .Where(p => !Program.Game.Map.Visible(p) ? !movable.Piece.IsPlayer : ((t = Program.Game.Map.GetVisibleTile(p)) != null && (t.Piece == null || (t.Piece.Side == movable.Piece.Side && t.Piece.HasBehavior<IMovable>()))))
                        .ToHashSet();
                    ranges[Green].Add(moveTiles);

                    //Debug.WriteLine("3 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

                    if (SelTile.Piece.IsPlayer && movable.MoveCur + movable.MoveInc > movable.MoveMax)
                        //check blocks
                        ranges[Green].Add(moveTiles.Where(t => Math.Min(movable.MoveCur - 1, movable.MoveCur + movable.MoveInc - movable.MoveMax) > SelTile.GetDistance(t)).ToHashSet());
                }

                //Debug.WriteLine("4 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

                if (SelTile.Piece != null && SelTile.Piece.HasBehavior(out IAttacker attacker))
                    ranges[Red].AddRange(AddAttacks(attacker, null));


                //Debug.WriteLine("5 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

                if (SelTile.Piece != null && SelTile.Piece.HasBehavior(out IBuilder b) && MouseTile != null && moveTiles.Contains(new(MouseTile.X, MouseTile.Y)))
                    ranges[Blue].Add(MouseTile.GetPointsInRange(b).ToHashSet());
            }

            Rectangle mapCoords = GetMapCoords();

            //Debug.WriteLine("6 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            List<Pen> dipose = new();
            Dictionary<LineSegment, Tuple<Pen, int>> lines = new();
            foreach (var pair in ranges)
                foreach (var tiles in pair.Value)
                    foreach (Point t in tiles)
                        if (mapCoords.Contains(new DPoint(t.X, t.Y)))
                        {
                            bool Show(Point p) => !tiles.Contains(p) && mapCoords.Contains(new DPoint(p.X, p.Y));
                            void AddLine(int x1, int y1, int x2, int y2)
                            {
                                LineSegment l = new(x1, y1, x2, y2);
                                if (lines.TryGetValue(l, out Tuple<Pen, int> oth))
                                {
                                    Pen combined;
                                    if (pair.Key != oth.Item1)
                                    {
                                        combined = Combine(pair.Key, oth);
                                        dipose.Add(combined);
                                    }
                                    else
                                        combined = pair.Key;
                                    lines[l] = new(combined, oth.Item2 + 1);
                                }
                                else
                                    lines.Add(l, new(pair.Key, 1));
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

            //Debug.WriteLine("7 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            //foreach (var t in lines)
            //    e.Graphics.DrawLine(t.Value, GetX(t.Key.x1), GetY(t.Key.y1), GetX(t.Key.x2), GetY(t.Key.y2));

            Dictionary<Pen, Range> edges = new();
            foreach (var t in lines)
            {
                if (!edges.TryGetValue(t.Value.Item1, out Range r))
                    edges.Add(t.Value.Item1, r = new());
                r.AddSegment(t.Key);
            }

            //Debug.WriteLine("8 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            //int calls = 0;
            PointF[] points;
            foreach (var p in edges)
                do
                {
                    points = p.Value.GetNext(GetX, GetY);
                    if (points.Length > 0)
                    {
                        //calls++;
                        e.Graphics.DrawLines(p.Key, points);
                    }
                } while (points.Length > 0);
            //Debug.WriteLine("draws: " + calls);

            //Debug.WriteLine("9 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            foreach (var d in dipose)
                d.Dispose();

            //Debug.WriteLine("10 " + watch.ElapsedTicks * 1000f / Stopwatch.Frequency);

            //watch.Stop();
        }
        private IEnumerable<HashSet<Point>> AddAttacks(IAttacker attacker, Action<IEnumerable<Point>, float> AddAttStr)
        {
            HashSet<Point> moveTiles = (attacker.HasBehavior(out IMovable movable) ? movable.Piece.Tile.GetPointsInRange(movable) : new Point[] { new(attacker.Piece.Tile.X, attacker.Piece.Tile.Y) }).ToHashSet();

            List<HashSet<Point>> retVal = new();
            var ar = attacker.Attacks.Where(a => a.CanAttack());
            if (attacker.Piece.IsEnemy && ar.Any())
            {
                HashSet<Point> moveEdge;
                if (movable != null)
                {
                    moveEdge = new HashSet<Point>(moveTiles.Count);
                    foreach (Point edge in GetEdge(attacker, moveTiles, movable))
                        CheckMovable(edge);
                    void CheckMovable(Point edge)
                    {
                        if (moveTiles.Contains(edge) && !moveEdge.Contains(edge))
                        {
                            Tile edgeTile = Program.Game.Map.GetVisibleTile(edge);
                            if (Program.Game.Map.Visible(edge) && (edgeTile == null || (edgeTile.Piece != null
                                && (SelTile != edgeTile || !SelTile.Piece.IsPlayer || !(SelTile.Piece.HasBehavior(out IMovable selMove) && selMove.MoveCur >= 1))
                                && (!edgeTile.Piece.IsEnemy || !edgeTile.Piece.HasBehavior<IMovable>()))))
                            {
                                moveTiles.Remove(edge);
                                CheckMovable(new Point(edge.X - 1, edge.Y));
                                CheckMovable(new Point(edge.X + 1, edge.Y));
                                CheckMovable(new Point(edge.X, edge.Y - 1));
                                CheckMovable(new Point(edge.X, edge.Y + 1));
                            }
                            else
                            {
                                moveEdge.Add(edge);
                            }
                        }
                    }
                }
                else
                {
                    moveEdge = moveTiles;
                }

                var points = attacker.Piece.Tile.GetAllPointsInRange((movable?.MoveCur ?? 0) + ar.Max(a => a.Range)).ToArray();
                foreach (var a in ar)
                {
                    List<Point> attPts = new(points.Length);
                    foreach (var point in points)
                        //check blocks
                        if (moveEdge.Any(mt => Tile.GetDistance(mt.X, mt.Y, point.X, point.Y) <= a.Range))
                            attPts.Add(point);
                    HashSet<Point> result = attPts.Union(moveTiles.Select(t => new Point(t.X, t.Y))).ToHashSet();
                    AddAttStr?.Invoke(result, a.AttackCur);
                    retVal.Add(result);
                }
            }
            else
            {
                foreach (var a in ar)
                    if (MouseTile != null && moveTiles.Contains(new Point(MouseTile.X, MouseTile.Y)))
                        retVal.Add(MouseTile.GetPointsInRange(a).ToHashSet());
                    else
                        retVal.Add(SelTile.GetPointsInRange(a).ToHashSet());
            }

            foreach (var result in retVal)
                result.RemoveWhere(p => Program.Game.Map.Visible(p) && Program.Game.Map.GetVisibleTile(p) == null);

            return retVal;
        }

        private static IEnumerable<Point> GetEdge(IAttacker attacker, HashSet<Point> moveTiles, IMovable movable)
        {
            //check blocks
            return moveTiles.Where(t => attacker.Piece.Tile.GetDistance(t) > movable.MoveCur - 1);
        }

        //private static HashSet<Point> GetPoints(IEnumerable<Tile> ts) => ts.Select(t => new Point(t.X, t.Y)).ToHashSet();
        private static Pen Combine(Pen pen, Tuple<Pen, int> tuple)
        {
            Pen oth = tuple.Item1;
            float factor = tuple.Item2 + 1;
            return new Pen(Color.FromArgb(Game.Rand.Round((pen.Color.R + tuple.Item2 * oth.Color.R) / factor),
                Game.Rand.Round((pen.Color.G + tuple.Item2 * oth.Color.G) / factor),
                Game.Rand.Round((pen.Color.B + tuple.Item2 * oth.Color.B) / factor)),
                (pen.Width + tuple.Item2 * oth.Width) / factor);
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
                        (p2, p1) = (p1, p2);
                    }
                    if (!range.TryGetValue(p1, out List<Point> list))
                        range.Add(p1, list = new(4));
                    list.Add(p2);
                }
            }
            public PointF[] GetNext(Func<int, float> GetX, Func<int, float> GetY)
            {
                List<PointF> all = new(count);
                Point start = range.Keys.FirstOrDefault();// p => OnMap(mapCoords, p));
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

                    //if (OnMap(mapCoords, cur) || OnMap(mapCoords, next))
                    //{
                    all.Add(GetPoint(GetX, GetY, cur));
                    cur = next;
                    //all.Add(GetPoint(GetX, GetY, next));
                    //}
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
            //private static bool OnMap(Rectangle mapCoords, Point p)
            //{
            //    //return true;
            //    return mapCoords.Contains(new Point(p.X, p.Y));
            //}
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

        //private PointF GetXY(float x, int y)
        //{
        //    if (1 == y1 - y2 % 2)
        //        x += .5f;
        //    float pX = GetCoord(x, xStart);
        //    float pY = GetCoord((float)(y * Math.Sqrt(3) / 2.0), yStart);
        //    return new PointF(pX, pY);
        //}
        private float GetX(int x)
        {
            return GetCoord(x, xStart);
        }
        private float GetY(int y)
        {
            return GetCoord(y, yStart);
        }
        private float GetCoord(float val, float start)
        {
            return val * Scale - start + padding;
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
            return (int)Math.Floor((start + val - padding) / Scale);
        }

        private Rectangle GetMapCoords()
        {
            int x = GetMapX(0), y = GetMapY(0), w = GetMapX(Width), h = GetMapY(Height);
            return new Rectangle(x, y, w - x + 1, h - y + 1);
        }

        private MouseEventArgs lastMouse;
        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            if (e == null)
                e = lastMouse;
            else
                lastMouse = e;
            if (e != null)
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

            float scrollAmt = (float)(scrollSpeed * Math.Sqrt(Scale));
            scrollAmt *= (scrollTime + paintTime) / scrollTime;
            scrollAmt = Game.Rand.Gaussian(scrollAmt, .013f);

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
            Map_MouseMove(sender, null);

            timer.Interval = Math.Max(1, Game.Rand.Round(scrollTime - paintTime));
        }
        public void Map_MouseWheel(object sender, MouseEventArgs e)
        {
            float mult = e.Delta / Game.Rand.GaussianCapped(91f, .026f, 1);
            if (mult < 0)
                mult = -1 / mult;
            else if (mult == 0)
                mult = 1;

            Tile sel = MouseTile ?? SelTile;
            Point anchor;
            if (sel != null && GetMapCoords().Contains(sel.X, sel.Y))
                anchor = new Point(sel.X, sel.Y);
            else
                anchor = new Point(GetMapX(Width / 2f), GetMapY(Height / 2f));
            float selX = GetX(anchor.X) + Scale / 2f;
            float selY = GetY(anchor.Y) + Scale / 2f;

            this.Scale *= mult;
            if (Scale < 3)
                Scale = 3;
            else if (Scale > 169)
                Scale = 169;
            if (this.CheckBounds(xStart, yStart))
                this.Scale /= mult;

            xStart += GetX(anchor.X) + Scale / 2f - selX;
            yStart += GetY(anchor.Y) + Scale / 2f - selY;

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
