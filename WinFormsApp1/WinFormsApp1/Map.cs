using System;
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
            this.ResizeRedraw = true;

            if (Program.Game != null)
                mapCoords = Program.Game.Map.GameRect();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (Program.Game != null)
            {
                //Pen thick = new(Color.Black, 3f);

                Scale(out float xScale, out float yScale);

                Pen[] pens = new Pen[] { Pens.Black, new Pen(Color.Green, 3f), new Pen(Color.Red, 3f), new Pen(Color.Blue, 3f), new Pen(Color.Black, 3f) };
                Dictionary<Pen, List<RectangleF>> grid = new();
                for (int a = 0; a < mapCoords.Width; a++)
                {
                    int x = mapCoords.X + a;
                    for (int b = 0; b < mapCoords.Height; b++)
                    {
                        int y = mapCoords.Y + b;
                        Tile tile = Program.Game.Map.GetTile(x, y);
                        RectangleF rectangleF = new(a * xScale + padding, b * yScale + padding, xScale, yScale);
                        if (Program.Game.Map.Visible(x, y))
                        {
                            Pen pen = pens[0];
                            RectangleF rect = rectangleF;
                            if (tile.Piece is Core)
                                e.Graphics.FillRectangle(Brushes.Blue, rect);
                            else if (tile.Piece is Mech)
                                e.Graphics.FillRectangle(Brushes.Green, rect);
                            else if (tile.Piece is Alien)
                                e.Graphics.FillRectangle(Brushes.Red, rect);

                            if (SelTile != null)
                            {
                                double moveRange = (SelTile.Piece is IMovable movable ? movable.MoveCur : 0);
                                double attRange = (SelTile.Piece is IAttacker attacker ? attacker.Attacks.Max(a => a.Range) : 0);

                                if (tile == SelTile)
                                    pen = pens[4];
                                else if (moveRange > 1 || attRange > 1)
                                    if (SelTile.Piece.Tile.GetTilesInRange(Math.Min(moveRange, attRange)).Contains(tile))
                                        pen = pens[3];
                                    else if (SelTile.Piece.Tile.GetTilesInRange(Math.Max(moveRange, attRange)).Contains(tile))
                                        if (moveRange > attRange)
                                            pen = pen = pens[1];
                                        else
                                            pen = pen = pens[2];
                            }

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

            Tile clicked = Program.Game.Map.GetTile(x, y);

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
        }
    }
}
