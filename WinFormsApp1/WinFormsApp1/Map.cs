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
        private Rectangle mapCoords;

        private float xScale, yScale;
        private Tile _selected;
        public Tile SelTile
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (Program.Form != null)
                {
                    Program.Form.Info.SetSelected(SelTile);
                    mapCoords = Program.Game.Map.GameRect();
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

            if (Program.Game != null)
                mapCoords = Program.Game.Map.GameRect();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (Program.Game != null)
            {
                Scale();
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
                        RectangleF rect = new(GetX(x), GetY(y), xScale, yScale);

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

            float left = 0;
            if (hasLeft)
                left = GetX(Program.Game.Map.left);
            else
            {
                for (int x = mapCoords.Left; x <= mapCoords.Right; x++)
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
                for (int x = mapCoords.Right; x >= mapCoords.Left; x--)
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
                for (int y = mapCoords.Top; y <= mapCoords.Bottom; y++)
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
                for (int y = mapCoords.Bottom; y >= mapCoords.Top; y--)
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
            return GetCoord(x, mapCoords.X, xScale);
        }
        private float GetY(int y)
        {
            return GetCoord(y, mapCoords.Y, yScale);
        }
        private static float GetCoord(int val, int mapCoord, float scale)
        {
            return (val - mapCoord) * scale;
        }

        private void Scale()
        {
            xScale = (this.Width - 1 - padding * 2) / (float)mapCoords.Width;
            yScale = (this.Height - 1 - padding * 2) / (float)mapCoords.Height;
            xScale = yScale = Math.Min(xScale, yScale);
        }

        private void Map_MouseClick(object sender, MouseEventArgs e)
        {
            int x = (int)((e.X - padding) / xScale) + mapCoords.X;
            int y = (int)((e.Y - padding) / yScale) + mapCoords.Y;

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
