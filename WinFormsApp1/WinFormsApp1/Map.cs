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

        private float xStart, yStart, scale;//, yScale;
        private Tile _selected;

        private readonly Timer timer;
        private bool scrollDown, scrollLeft, scrollUp, scrollRight;

        public Tile SelTile
        {
            get { return _selected; }
            set
            {
                _selected = value;
                Center();
                if (Program.Form != null)
                {
                    Program.Form.Info.SetSelected(SelTile);
                    Program.Form.Refresh();
                }
            }
        }

        public Map()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            this.ResizeRedraw = true;

            this.MouseWheel += Map_MouseWheel;

            timer = new();
            timer.Interval = 13;
            timer.Tick += Timer_Tick;
        }

        private void Map_Load(object sender, EventArgs e)
        {
            Rectangle gameRect = Program.Game.Map.GameRect();
            scale = Math.Min((this.Width - 1 - padding * 2) / (float)gameRect.Width, (this.Height - 1 - padding * 2) / (float)gameRect.Height);
            xStart = GetX(gameRect.X);
            yStart = GetY(gameRect.Y);
        }

        private void Center()
        {
            Rectangle mapCoords = GetMapCoords();
            if (SelTile != null && !mapCoords.Contains(SelTile.X, SelTile.Y))
            {
                xStart += GetX(SelTile.X - mapCoords.Width / 2);
                yStart += GetY(SelTile.Y - mapCoords.Height / 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //if (this == Program.Form.MapMain)
            //{
            //    Debug.WriteLine(xStart);
            //    Debug.WriteLine(yStart);
            //    Debug.WriteLine(xScale);
            //    Debug.WriteLine(yScale);
            //    Debug.WriteLine("");
            //}

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
            //e.Graphics.SetClip(new RectangleF(0, 0, xScale * mapCoords.Width + 2, yScale * mapCoords.Height + 2));

            Pen reg = Pens.Black, sel = new(Color.Black, 3f);
            Pen[] pens = new Pen[] { reg, sel };

            Rectangle mapCoords = GetMapCoords();
            Dictionary<Pen, List<RectangleF>> grid = new();
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
                            e.Graphics.FillRectangle(Brushes.Blue, RectangleF.Inflate(rect, -2.5f, -2.5f));
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

        private void Ranges(PaintEventArgs e)
        {
            if (SelTile != null)
            {
                Pen range = new(Color.Red, 3f), move = new(Color.Green, 3f), repair = new(Color.Blue, 3f);
                Pen[] pens = new Pen[] { repair, range, move };
                Dictionary<Pen, List<HashSet<Point>>> ranges = new() { { range, new() }, { move, new() }, { repair, new() } };
                HashSet<Point> GetRangePoints(double v) => GetPoints(SelTile.GetVisibleTilesInRange(v));//.Where(t => t.Piece == null)                    
                HashSet<Point> GetPoints(IEnumerable<Tile> ts) => ts
                   .Select(t => new Point(t.X, t.Y)).ToHashSet();

                IMovable movable = SelTile.Piece as IMovable;
                if (movable != null && movable.MoveCur >= 1)
                {
                    var moveTiles = SelTile.GetVisibleTilesInRange(movable.MoveCur);
                    ranges[move].Add(GetPoints(moveTiles));
                    if (SelTile.Piece.IsPlayer && movable.MoveCur + movable.MoveInc > movable.MoveMax)
                        ranges[move].Add(GetPoints(moveTiles.Where(t => Math.Min(movable.MoveCur - 1, movable.MoveCur + movable.MoveInc - movable.MoveMax) > t.GetDistance(SelTile))));
                }

                if (SelTile.Piece is IAttacker attacker)
                {
                    var ar = attacker.Attacks.Where(a => !a.Attacked).Select(a => a.Range);
                    if (SelTile.Piece.IsEnemy && movable != null)
                    {
                        var moveTiles = movable.Piece.Tile.GetVisibleTilesInRange(movable.MoveCur);
                        var moveEdge = moveTiles.Where(t => t.GetDistance(SelTile) > movable.MoveCur - 1);
                        foreach (var a in ar)
                            ranges[range].Add(GetPoints(moveEdge.SelectMany(t => t.GetVisibleTilesInRange(a)).Union(moveTiles)));
                    }
                    else
                        foreach (var a in ar)
                            ranges[range].Add(GetRangePoints(a));
                }

                if (SelTile.Piece is IRepair r)
                    ranges[repair].Add(GetRangePoints(r.Range));

                Dictionary<LineSegment, Pen> lines = new();
                foreach (Pen pen in pens)
                {
                    if (ranges.ContainsKey(pen))
                        foreach (var tiles in ranges[pen])
                            foreach (Point t in tiles)
                            //if (Program.Game.Map.Visible(t.X, t.Y))
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
        }
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
                int mult = 1 << 7;
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
            Pen thick = new(Color.Black, 3f);

            bool hasLeft = (Program.Game.Map.Visible(Program.Game.Map.left, Program.Game.Map.down - 1))
                || (Program.Game.Map.Visible(Program.Game.Map.left, Program.Game.Map.up + 1));
            bool hasRight = (Program.Game.Map.Visible(Program.Game.Map.right, Program.Game.Map.down - 1))
                || (Program.Game.Map.Visible(Program.Game.Map.right, Program.Game.Map.up + 1));
            bool hasDown = (Program.Game.Map.Visible(Program.Game.Map.left - 1, Program.Game.Map.down))
                || (Program.Game.Map.Visible(Program.Game.Map.right + 1, Program.Game.Map.down));
            bool hasUp = (Program.Game.Map.Visible(Program.Game.Map.left - 1, Program.Game.Map.up))
                || (Program.Game.Map.Visible(Program.Game.Map.right + 1, Program.Game.Map.up));

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

        private void Map_KeyDown(object sender, KeyEventArgs e)
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
        private void Map_KeyUp(object sender, KeyEventArgs e)
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

            const float scrollAmt = 16.9f;
            if (scrollDown && !scrollUp)
                this.yStart -= scrollAmt;
            if (scrollUp && !scrollDown)
                this.yStart += scrollAmt;
            if (scrollLeft && !scrollRight)
                this.xStart -= scrollAmt;
            if (scrollRight && !scrollLeft)
                this.xStart += scrollAmt;

            CheckBounds(xs, ys);

            Refresh();
        }
        private void Map_MouseWheel(object sender, MouseEventArgs e)
        {
            float mult = e.Delta / 91f;
            if (mult < 0)
                mult = -1 / mult;
            this.scale *= mult;

            if (scale < 3)
                scale = 3;
            else if (scale > 390)
                scale = 390;

            if (this.CheckBounds(xStart, yStart))
                this.scale /= mult;
            this.Refresh();
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
            else if (e.Button == MouseButtons.Right && SelTile != null && clicked != null)
            {
                if (SelTile.Piece is IMovable movable)
                {
                    if (movable.Move(clicked))
                        SelTile = clicked;
                }
                if (SelTile.Piece is IAttacker attacker && clicked.Piece is IKillable killable)
                {
                    if (attacker.Fire(killable))
                        SelTile = clicked;
                }
            }

            if (SelTile == orig)
            {
                if (SelTile != null && SelTile == clicked)
                    Program.Moved.Add(SelTile.Piece);
                Program.Next(true);
            }
        }
    }
}
