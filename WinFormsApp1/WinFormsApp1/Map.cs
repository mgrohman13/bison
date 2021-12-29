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
        private const float padding = 1;

        private float xStart, yStart, scale;
        private Tile _selected, _moused;
        private bool viewAttacks;

        private readonly Timer timer;
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
            timer.Interval = 13;
            timer.Tick += Timer_Tick;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (Program.Game != null)
            {
                Tiles(e);
                Ranges(e);
                Border(e);
            }

            base.OnPaint(e);
        }
        private void Tiles(PaintEventArgs e)
        {
            Pen reg = Pens.Black;
            using Pen sel = new(Color.Black, 3f);
            Pen[] pens = new Pen[] { reg, sel };

            Rectangle mapCoords = GetMapCoords();
            Dictionary<Pen, List<RectangleF>> grid = new();
            using Font f = new(FontFamily.GenericMonospace, scale / 5.6f);
            for (int a = 0; a < mapCoords.Width; a++)
            {
                int x = mapCoords.X + a;
                for (int b = 0; b < mapCoords.Height; b++)
                {
                    int y = mapCoords.Y + b;
                    Tile tile = Program.Game.Map.GetVisibleTile(x, y);
                    if (tile != null)
                    {
                        Pen pen = reg;
                        RectangleF rect = new(GetX(x), GetY(y), scale, scale);

                        Resource resource = tile.Piece as Resource;
                        Extractor extractor = tile.Piece as Extractor;
                        if (resource == null && extractor != null)
                            resource = extractor.Resource;
                        if (tile.Piece is Alien)
                            e.Graphics.FillRectangle(Brushes.Red, rect);
                        else if (tile.Piece is Mech)
                            e.Graphics.FillRectangle(Brushes.Green, rect);
                        else if (tile.Piece is Constructor)
                            e.Graphics.FillRectangle(Brushes.LightGreen, rect);
                        else if (tile.Piece is Core || tile.Piece is Factory)
                            e.Graphics.FillRectangle(Brushes.Blue, rect);
                        else if (tile.Piece is Foundation)
                            e.Graphics.FillRectangle(Brushes.Black, rect);
                        else if (tile.Piece is Turret)
                            e.Graphics.FillEllipse(Brushes.Blue, RectangleF.Inflate(rect, -2.5f, -2.5f));
                        else if (resource != null)
                        {
                            if (resource is Biomass)
                                e.Graphics.FillRectangle(Brushes.Orange, rect);
                            else if (resource is Metal)
                                e.Graphics.FillRectangle(Brushes.Gray, rect);
                            else if (resource is Artifact)
                                e.Graphics.FillRectangle(Brushes.Magenta, rect);
                            if (extractor != null)
                                e.Graphics.FillEllipse(Brushes.Blue, RectangleF.Inflate(rect, -2.5f, -2.5f));
                        }

                        if (viewAttacks && scale > 21 && this.attacks.TryGetValue(new Point(x, y), out string attack))
                            e.Graphics.DrawString(attack, f, Brushes.Red, new PointF(
                                GetX(x) + scale - e.Graphics.MeasureString(attack, f).Width, GetY(y) + scale - f.Size * 2 - 2));

                        if (tile == SelTile)
                            pen = sel;

                        if (!grid.TryGetValue(pen, out List<RectangleF> list))
                            grid.Add(pen, list = new List<RectangleF>());
                        list.Add(rect);
                    }
                }
            }

            foreach (Pen pen in pens)
                if (grid.ContainsKey(pen))
                    e.Graphics.DrawRectangles(pen, grid[pen].ToArray());
        }

        private readonly static Pen range = new(Color.Red, 3f), move = new(Color.Green, 3f), repair = new(Color.Blue, 3f);
        private readonly static Pen[] pens = new Pen[] { repair, range, move };
        private readonly Dictionary<Pen, List<HashSet<Point>>> ranges = pens.ToDictionary(p => p, p => new List<HashSet<Point>>());
        private readonly Dictionary<Point, string> attacks = new();
        public void RefreshRanges()
        {
            ShowMouseInfo();

            foreach (var pair in ranges)
                pair.Value.Clear();

            foreach (IBuilder b in Program.Game.Player.PiecesOfType<IBuilder>())
                ranges[repair].Add(GetPoints(b.Piece.Tile.GetVisibleTilesInRange(b.Range)));

            if (viewAttacks)
            {
                Dictionary<Tile, double> attStr = new();
                void AddAttStr(IEnumerable<Tile> range, double damage)
                {
                    foreach (Tile t in range)
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
                ranges[range].Add(allAttacks.ToHashSet());

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
                    moveTiles = movable.Piece.Tile.GetVisibleTilesInRange(movable.MoveCur);
                    ranges[move].Add(GetPoints(moveTiles));
                    if (SelTile.Piece.IsPlayer && movable.MoveCur + movable.MoveInc > movable.MoveMax)
                        ranges[move].Add(GetPoints(moveTiles.Where(t => Math.Min(movable.MoveCur - 1, movable.MoveCur + movable.MoveInc - movable.MoveMax) > t.GetDistance(SelTile))));
                }

                if (SelTile.Piece != null && SelTile.Piece.HasBehavior(out IAttacker attacker))
                    ranges[range].AddRange(AddAttacks(attacker, moveTiles, null));

                if (SelTile.Piece != null && SelTile.Piece.HasBehavior(out IBuilder b) && moveTiles.Contains(MouseTile))
                    ranges[repair].Add(GetPoints(MouseTile.GetVisibleTilesInRange(b.Range)));
            }

            Dictionary<LineSegment, Pen> lines = new();
            foreach (Pen pen in pens)
            {
                if (ranges.ContainsKey(pen))
                    foreach (var tiles in ranges[pen])
                        foreach (Point t in tiles)
                        {
                            bool Show(Point p) => !tiles.Contains(p);
                            void AddLine(int x1, int y1, int x2, int y2)
                            {
                                LineSegment l = new(x1, y1, x2, y2);
                                if (lines.TryGetValue(l, out Pen oth))
                                {
                                    if (pen != oth)
                                        lines[l] = Combine(pen, oth);
                                }
                                else
                                    lines.Add(l, pen);
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
            }

            foreach (var t in lines)
                e.Graphics.DrawLine(t.Value, GetX(t.Key.x1), GetY(t.Key.y1), GetX(t.Key.x2), GetY(t.Key.y2));
        }
        private IEnumerable<HashSet<Point>> AddAttacks(IAttacker attacker, IEnumerable<Tile> moveTiles, Action<IEnumerable<Tile>, double> AddAttStr)
        {
            List<HashSet<Point>> retVal = new();
            var ar = attacker.Attacks.Where(a => !a.Attacked);
            if (attacker.Piece.IsEnemy && moveTiles.Any())
            {
                var moveEdge = moveTiles.Where(t => t.GetDistance(attacker.Piece.Tile) > attacker.Piece.GetBehavior<IMovable>().MoveCur - 1);
                foreach (var a in ar)
                {
                    IEnumerable<Tile> e = moveEdge.SelectMany(t => t.GetVisibleTilesInRange(a.Range)).Union(moveTiles);
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
            const int factor = 3;
            return new Pen(Color.FromArgb((pen.Color.R + oth.Color.R) / factor, (pen.Color.G + oth.Color.G) / factor, (pen.Color.B + oth.Color.B) / factor), (pen.Width + oth.Width) / 2f);
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

        private void Border(PaintEventArgs e)
        {
            using Pen thick = new(Color.Black, 3f);

            bool hasLeft = (Program.Game.Map.Visible(Program.Game.Map.left, Program.Game.Map.down - 1))
                || (Program.Game.Map.Visible(Program.Game.Map.left, Program.Game.Map.up + 1))
                || Program.Game.Map.ExploredLeft.Min() < Program.Game.Map.left;
            bool hasRight = (Program.Game.Map.Visible(Program.Game.Map.right, Program.Game.Map.down - 1))
                || (Program.Game.Map.Visible(Program.Game.Map.right, Program.Game.Map.up + 1))
                || Program.Game.Map.ExploredRight.Max() > Program.Game.Map.right;
            bool hasDown = (Program.Game.Map.Visible(Program.Game.Map.left - 1, Program.Game.Map.down))
                || (Program.Game.Map.Visible(Program.Game.Map.right + 1, Program.Game.Map.down))
                || Program.Game.Map.ExploredDown.Min() < Program.Game.Map.down;
            bool hasUp = (Program.Game.Map.Visible(Program.Game.Map.left - 1, Program.Game.Map.up))
                || (Program.Game.Map.Visible(Program.Game.Map.right + 1, Program.Game.Map.up))
                || Program.Game.Map.ExploredUp.Max() > Program.Game.Map.up;

            Rectangle gameRect = Program.Game.Map.GameRect();

            float left = 0;
            if (hasLeft)
                left = GetX(Program.Game.Map.left);
            else
            {
                for (int x = gameRect.Left; x <= gameRect.Right; x++)
                    if (Program.Game.Map.Visible(x, Program.Game.Map.up + 1)
                        || Program.Game.Map.Visible(x, Program.Game.Map.down - 1))
                    {
                        left = GetX(x);
                        break;
                    }
            }
            float right = 0;
            if (hasRight)
                right = GetX(Program.Game.Map.right + 1);
            else
            {
                for (int x = gameRect.Right; x >= gameRect.Left; x--)
                    if (Program.Game.Map.Visible(x, Program.Game.Map.up + 1)
                        || Program.Game.Map.Visible(x, Program.Game.Map.down - 1))
                    {
                        right = GetX(x + 1);
                        break;
                    }
            }
            float down = 0;
            if (hasDown)
                down = GetY(Program.Game.Map.down);
            else
            {
                for (int y = gameRect.Top; y <= gameRect.Bottom; y++)
                    if (Program.Game.Map.Visible(Program.Game.Map.right + 1, y)
                        || Program.Game.Map.Visible(Program.Game.Map.left - 1, y))
                    {
                        down = GetY(y);
                        break;
                    }
            }
            float up = 0;
            if (hasUp)
                up = GetY(Program.Game.Map.up + 1);
            else
            {
                for (int y = gameRect.Bottom; y >= gameRect.Top; y--)
                    if (Program.Game.Map.Visible(Program.Game.Map.right + 1, y)
                        || Program.Game.Map.Visible(Program.Game.Map.left - 1, y))
                    {
                        up = GetY(y + 1);
                        break;
                    }
            }

            if (hasLeft)
                e.Graphics.DrawLine(thick, left, 0, left, down);
            if (hasLeft)
                e.Graphics.DrawLine(thick, left, Height, left, up);
            if (hasRight)
                e.Graphics.DrawLine(thick, right, 0, right, down);
            if (hasRight)
                e.Graphics.DrawLine(thick, right, Height, right, up);
            if (hasDown)
                e.Graphics.DrawLine(thick, 0, down, left, down);
            if (hasDown)
                e.Graphics.DrawLine(thick, Width, down, right, down);
            if (hasUp)
                e.Graphics.DrawLine(thick, 0, up, left, up);
            if (hasUp)
                e.Graphics.DrawLine(thick, Width, up, right, up);
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

            float scrollAmt = Game.Rand.Gaussian(16.9f, .013f);
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

            Point anchor;
            if (SelTile != null && GetMapCoords().Contains(SelTile.X, SelTile.Y))
                anchor = new Point(SelTile.X, SelTile.Y);
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
