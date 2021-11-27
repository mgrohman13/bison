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
using Tile = ClassLibrary1.Map.Tile;

namespace WinFormsApp1
{
    public partial class Map : UserControl
    {
        const float padding = 1;
        public Rectangle mapCoords;

        private Tile _selected;
        public Tile Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                Program.Form.Info.SetSelected(Selected);
                mapCoords = Program.Game.Map.GameRect();
                Program.Form.Refresh();
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
                Pen thick = new(Color.Black, 3f);

                Scale(out float xScale, out float yScale);

                List<RectangleF> grid = new();
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
                            RectangleF rect = rectangleF;
                            if (tile.Piece != null)
                                e.Graphics.FillRectangle(Brushes.Blue, rect);
                            grid.Add(rect);
                        }
                        if (Selected != null)
                        {
                            if (Selected.Piece is IMovable movable)
                            {
                                if (Selected.Piece.Tile.GetTilesInRange(movable.MoveCur).Contains(tile))
                                    e.Graphics.DrawRectangles(thick, new RectangleF[] { rectangleF });
                            }
                            else if (tile == Selected)
                            {
                                e.Graphics.DrawRectangles(thick, new RectangleF[] { rectangleF });
                            }
                        }
                    }
                }

                e.Graphics.DrawRectangles(Pens.Black, grid.ToArray());

                float left = (Program.Game.Map.left - mapCoords.X) * xScale;
                float right = (Program.Game.Map.right - mapCoords.X + 1) * xScale;
                float up = (Program.Game.Map.up - mapCoords.Y + 1) * yScale;
                float down = (Program.Game.Map.down - mapCoords.Y) * yScale;

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
                Selected = clicked;
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (Selected != null && Selected.Piece is IMovable movable)
                    if (movable.Move(clicked))
                        Selected = clicked;
            }
        }
    }
}
