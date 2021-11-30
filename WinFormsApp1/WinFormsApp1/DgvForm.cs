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
using MattUtil;
using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using Tile = ClassLibrary1.Map.Tile;
using Point = MattUtil.Point;

namespace WinFormsApp1
{
    public partial class DgvForm : Form
    {
        private List<BuildRow> rows;
        //BindingSource binding = new BindingSource();
        private Piece result;

        public DgvForm()
        {
            InitializeComponent();
            //dataGridView1.DataSource = null;
            //dataGridView1.DataSource = rows;
            //dataGridView1.DataSource = binding;
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        public Piece BuilderDialog(IBuilder builder)
        {
            dataGridView1.Hide();
            ////Show();
            //try
            //{
            //    ((CurrencyManager)dataGridView1.BindingContext[dataGridView1.DataSource]).Refresh();
            //}
            //catch (InvalidOperationException e)
            //{
            //}

            rows = new List<BuildRow>();
            result = null;

            if (builder is IBuilder.IBuildConstructor buildConstructor)
            {
                Constructor.Cost(Program.Game, out double energy, out double mass);
                rows.Add(new(buildConstructor, "Constructor", energy, mass));
            }
            if (builder is IBuilder.IBuildMech buildMech)
            {
                foreach (MechBlueprint blueprint in new MechBlueprint[] { Program.Game.Blueprint1, Program.Game.Blueprint2 })
                {
                    Mech.Cost(Program.Game, out double energy, out double mass, blueprint);
                    BuildRow row = new(buildMech, "Mech", energy, mass);
                    row.Blueprint = blueprint;
                    rows.Add(row);
                }
            }

            if (builder is IBuilder.IBuildExtractor buildExtractor)
            {
                int x = builder.Piece.Tile.X;
                int y = builder.Piece.Tile.Y;
                int idx = 0;
                foreach (Point point in new Point[] { new(x, y - 1), new(x, y + 1), new(x - 1, y), new(x + 1, y) })
                {
                    Tile target = Program.Game.Map.GetVisibleTile(point.X, point.Y);
                    if (target != null && target.Piece is Resource resource)
                    {
                        Extractor.Cost(out double energy, out double mass, resource);
                        BuildRow row = new(buildExtractor, "Extractor", energy, mass);
                        rows.Add(row);
                        switch (idx)
                        {
                            case 0:
                                row.Up = true;
                                break;
                            case 1:
                                row.Down = true;
                                break;
                            case 2:
                                row.Left = true;
                                break;
                            case 3:
                                row.Right = true;
                                break;
                        }
                    }
                    idx++;
                }
            }

            if (rows.Any())
            {
                //CurrencyManager cm = (CurrencyManager)this.dataGridView1.BindingContext[binding];
                //if (cm != null)
                //{
                //    cm.Refresh();
                //}

                //try
                //{
                //    ((CurrencyManager)dataGridView1.BindingContext[dataGridView1.DataSource]).Refresh();
                //}
                //catch (Exception e)
                //{
                //}
                //dataGridView1.DataSource = null;
                //dataGridView1.DataSource = rows;

                dataGridView1.DataSource = rows;

                dataGridView1.Columns["Blueprint"].Visible = false;
                dataGridView1.Columns["Builder"].Visible = false;

                dataGridView1.Columns["Energy"].DefaultCellStyle.Format = "0.0";
                dataGridView1.Columns["Mass"].DefaultCellStyle.Format = "0.0";


                dataGridView1.Location = new System.Drawing.Point(0, 0);
                dataGridView1.Size = dataGridView1.PreferredSize;
                this.ClientSize = dataGridView1.PreferredSize;

                //Hide();

                //try
                //{
                dataGridView1.Show();
                ShowDialog();
                //}
                //catch (Exception e)
                //{
                //    Debug.WriteLine(e.StackTrace);
                //    ((CurrencyManager)dataGridView1.BindingContext[dataGridView1.DataSource]).Refresh();
                //    ShowDialog();
                //}
            }

            return result;
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            BuildRow row = rows[e.RowIndex];
            IBuilder builder = row.Builder;
            int x = builder.Piece.Tile.X;
            int y = builder.Piece.Tile.Y;
            Tile tile = null;
            string result = dataGridView1.Columns[e.ColumnIndex].Name;
            switch (result)
            {
                case "Up":
                    tile = Program.Game.Map.GetVisibleTile(x, y - 1);
                    break;
                case "Down":
                    tile = Program.Game.Map.GetVisibleTile(x, y + 1);
                    break;
                case "Left":
                    tile = Program.Game.Map.GetVisibleTile(x - 1, y);
                    break;
                case "Right":
                    tile = Program.Game.Map.GetVisibleTile(x + 1, y);
                    break;
            }
            if (tile != null)
            {
                result = row.Name;
                switch (result)
                {
                    case "Constructor":
                        this.result = ((IBuilder.IBuildConstructor)builder).Build(tile);
                        break;
                    case "Mech":
                        this.result = ((IBuilder.IBuildMech)builder).Build(tile, row.Blueprint);
                        break;
                    case "Extractor":
                        this.result = ((IBuilder.IBuildExtractor)builder).Build(tile.Piece as Resource);
                        break;
                }
                this.Close();
            }
        }

        public class BuildRow
        {
            public MechBlueprint Blueprint { get; set; }
            public IBuilder Builder { get; set; }
            public string Name { get; set; }
            public double Energy { get; set; }
            public double Mass { get; set; }
            public bool Up { get; set; }
            public bool Down { get; set; }
            public bool Left { get; set; }
            public bool Right { get; set; }
            public BuildRow(IBuilder builder, string name, double energy, double mass)
            {
                this.Builder = builder;
                this.Name = name;
                this.Energy = energy;
                this.Mass = mass;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
