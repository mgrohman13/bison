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

namespace WinFormsApp1
{
    public partial class Map : UserControl
    {
        const float padding = 1;
        public Rectangle mapCoords;

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
            //int dim = Math.Min(this.Width, this.Height);
            e.Graphics.Clear(Color.White);
            //e.Graphics.SetClip(new RectangleF(0, 0, dim, dim));

            if (Program.Game != null)
            {
                Scale(out float xScale, out float yScale);

                HashSet<Tile> minRange = new(), maxRange = new();
                double moveRange = 0, attRange = 0;
                if (SelTile != null)
                {
                    moveRange = (SelTile.Piece is IMovable movable ? movable.MoveCur : 0);
                    attRange = (SelTile.Piece is IAttacker attacker ? attacker.Attacks.Max(a => a.Attacked ? 0 : a.Range) : 0);
                    if (moveRange > 1 || attRange > 1)
                    {
                        minRange = SelTile.Piece.Tile.GetTilesInRange(Math.Min(moveRange, attRange)).ToHashSet();
                        maxRange = SelTile.Piece.Tile.GetTilesInRange(Math.Max(moveRange, attRange)).ToHashSet();
                    }
                }

                Pen[] pens = new Pen[] { Pens.Black, new Pen(Color.Green, 3f), new Pen(Color.Red, 3f), new Pen(Color.Blue, 3f), new Pen(Color.Black, 3f) };
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
                            Pen pen = pens[0];
                            RectangleF rect = new(a * xScale + padding, b * yScale + padding, xScale, yScale);

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
                            else if (tile.Piece is Core)
                                e.Graphics.FillRectangle(Brushes.Blue, rect);
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
                                pen = pens[4];
                            else if (moveRange > 1 || attRange > 1)
                                if (minRange.Contains(tile))
                                    pen = pens[3];
                                else if (maxRange.Contains(tile))
                                    if (moveRange > attRange)
                                        pen = pens[1];
                                    else
                                        pen = pens[2];

                            if (!grid.TryGetValue(pen, out List<RectangleF> list))
                                grid.Add(pen, list = new List<RectangleF>());
                            list.Add(rect);
                        }
                    }
                }

                foreach (var pair in grid)
                    e.Graphics.DrawRectangles(pair.Key, pair.Value.ToArray());

                float left = (Program.Game.Map.left - mapCoords.X) * xScale;
                float right = (Program.Game.Map.right - mapCoords.X + 1) * xScale;
                float up = (Program.Game.Map.up - mapCoords.Y + 1) * yScale;
                float down = (Program.Game.Map.down - mapCoords.Y) * yScale;

                Pen thick = new(Color.Black, 3f);
                e.Graphics.DrawLine(thick, left, 0, left, down);
                e.Graphics.DrawLine(thick, 0, down, left, down);
                e.Graphics.DrawLine(thick, right, 0, right, down);
                e.Graphics.DrawLine(thick, Width, down, right, down);
                e.Graphics.DrawLine(thick, left, Height, left, up);
                e.Graphics.DrawLine(thick, 0, up, left, up);
                e.Graphics.DrawLine(thick, right, Height, right, up);
                e.Graphics.DrawLine(thick, Width, up, right, up);
            }

            base.OnPaint(e);
        }

        private void Scale(out float xScale, out float yScale)
        {
            xScale = (this.Width - 1 - padding * 2) / (float)mapCoords.Width;
            yScale = (this.Height - 1 - padding * 2) / (float)mapCoords.Height;
            xScale = yScale = Math.Min(xScale, yScale);
        }

        private void Map_MouseClick(object sender, MouseEventArgs e)
        {
            Scale(out float xScale, out float yScale);

            int x = (int)((e.X - 1) / xScale) + mapCoords.X;
            int y = (int)((e.Y - 1) / yScale) + mapCoords.Y;

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
