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
        private List<object> rows;
        private Tile selected;
        private Piece result;

        public DgvForm()
        {
            InitializeComponent();
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        public static bool CanBuild(Tile selected)
        {
            if (selected != null)
            {
                IBuilder.IBuildConstructor buildConstructor = GetBuilder<IBuilder.IBuildConstructor>(selected);
                IBuilder.IBuildMech buildMech = GetBuilder<IBuilder.IBuildMech>(selected);
                IBuilder.IBuildExtractor buildExtractor = GetBuilder<IBuilder.IBuildExtractor>(selected);
                IBuilder.IBuildFactory buildFactory = GetBuilder<IBuilder.IBuildFactory>(selected);
                IBuilder.IBuildTurret buildTurret = GetBuilder<IBuilder.IBuildTurret>(selected);
                if (buildConstructor != null && selected.Piece == null)
                    return true;
                if (buildMech != null && selected.Piece == null)
                    return true;
                if (buildExtractor != null && selected.Piece is Resource)
                    return true;
                if (buildFactory != null && selected.Piece is Foundation)
                    return true;
                if (buildTurret != null && selected.Piece is Foundation)
                    return true;
            }
            return false;
        }
        public Piece BuilderDialog(Tile selected)
        {
            this.selected = selected;
            dataGridView1.Hide();

            rows = new List<object>();
            result = null;


            IBuilder.IBuildConstructor buildConstructor = GetBuilder<IBuilder.IBuildConstructor>(selected);
            IBuilder.IBuildMech buildMech = GetBuilder<IBuilder.IBuildMech>(selected);
            IBuilder.IBuildExtractor buildExtractor = GetBuilder<IBuilder.IBuildExtractor>(selected);
            IBuilder.IBuildFactory buildFactory = GetBuilder<IBuilder.IBuildFactory>(selected);
            IBuilder.IBuildTurret buildTurret = GetBuilder<IBuilder.IBuildTurret>(selected);

            if (buildConstructor != null)
            {
                Constructor.Cost(Program.Game, out double energy, out double mass);
                BuildRow row = new(buildConstructor, "Constructor", energy, mass);
                rows.Add(row);
            }
            if (buildMech != null)
            {
                foreach (MechBlueprint blueprint in Program.Game.Player.Research.Blueprints)
                {
                    blueprint.Cost(out double energy, out double mass);
                    BuildRow row = new(buildMech, "Mech", energy, mass, blueprint);
                    rows.Add(row);
                }
            }
            if (buildExtractor != null && selected.Piece is Resource resource)
            {
                Extractor.Cost(out double energy, out double mass, resource);
                BuildRow row = new(buildExtractor, "Extractor", energy, mass);
                rows.Add(row);
            }
            Foundation foundation = selected.Piece as Foundation;
            if (buildFactory != null && foundation != null)
            {
                Factory.Cost(Program.Game, out double energy, out double mass);
                BuildRow row = new(buildFactory, "Factory", energy, mass);
                rows.Add(row);
            }
            if (buildTurret != null && foundation != null)
            {
                Turret.Cost(Program.Game, out double energy, out double mass);
                BuildRow row = new(buildTurret, "Turret", energy, mass);
                rows.Add(row);
            }

            if (rows.Any())
            {
                dataGridView1.DataSource = rows;

                //dataGridView1.Columns["Blueprint"].Visible = false;
                dataGridView1.Columns["Builder"].Visible = false;

                dataGridView1.Columns["Energy"].DefaultCellStyle.Format = "0.0";
                dataGridView1.Columns["Mass"].DefaultCellStyle.Format = "0.0";

                dataGridView1.Columns["Research"].DefaultCellStyle.Format = "0.0";
                dataGridView1.Columns["Vision"].DefaultCellStyle.Format = "0.0";
                dataGridView1.Columns["Hits"].DefaultCellStyle.Format = "0.0";
                dataGridView1.Columns["Resilience"].DefaultCellStyle.Format = "P1";
                dataGridView1.Columns["Armor"].DefaultCellStyle.Format = "P1";

                dataGridView1.Location = new System.Drawing.Point(0, 0);
                dataGridView1.Size = dataGridView1.PreferredSize;
                this.ClientSize = dataGridView1.PreferredSize;

                dataGridView1.Show();
                ShowDialog();
            }

            return result;
        }
        private static T GetBuilder<T>(Tile selected) where T : class, IBuilder
        {
            return Program.Game.Player.PiecesOfType<T>().FirstOrDefault(b => selected.GetDistance(b.Piece.Tile) <= b.Range);
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            BuildRow row = (BuildRow)rows[e.RowIndex];
            IBuilder builder = row.Builder;
            if (builder is IBuilder.IBuildConstructor buildConstructor)
                this.result = buildConstructor.Build(selected);
            if (builder is IBuilder.IBuildMech buildMech)
                this.result = buildMech.Build(selected, row.Blueprint);
            if (builder is IBuilder.IBuildExtractor buildExtractor)
                this.result = buildExtractor.Build(selected.Piece as Resource);
            if (builder is IBuilder.IBuildFactory buildFactory)
                this.result = buildFactory.Build(selected.Piece as Foundation);
            if (builder is IBuilder.IBuildTurret buildTurret)
                this.result = buildTurret.Build(selected.Piece as Foundation);
            this.Close();
        }

        public class BuildRow
        {
            public IBuilder Builder { get; }
            public string Name { get; }
            public double Energy { get; }
            public double Mass { get; }

            public MechBlueprint Blueprint { get; }

            public MechBlueprint Upgraded => Blueprint?.UpgradeFrom;
            public double? Research => Blueprint?.ResearchLevel;

            public string Movement => Blueprint == null ? null : string.Format("{1:0.0} / {2:0.0} +{0:0.0}", Blueprint.Movable.MoveInc, Blueprint.Movable.MoveMax, Blueprint.Movable.MoveLimit);
            public double? Vision => Blueprint?.Vision;

            public double? Hits => Blueprint?.Killable.HitsMax;
            public double? Resilience => Blueprint?.Killable.Resilience;
            public double? Armor => Blueprint?.Killable.Armor;
            public string Shields => Blueprint == null ? null : Blueprint.Killable.ShieldInc <= 0 ? "" :
                string.Format("{1:0.0} / {2:0.0} +{0:0.0}", Blueprint.Killable.ShieldInc, Blueprint.Killable.ShieldMax, Blueprint.Killable.ShieldLimit);

            public int? Attacks => Blueprint?.Attacks.Count;
            public string Range => Blueprint == null ? null : string.Join(" , ", Blueprint.Attacks.Select(a => a.Range.ToString("0.0")));
            public string Damage => Blueprint == null ? null : string.Join(" , ", Blueprint.Attacks.Select(a => a.Damage.ToString("0.0")));
            public string ArmorPierce => Blueprint == null ? null : string.Join(" , ", Blueprint.Attacks.Select(a => a.ArmorPierce.ToString("P1")));
            public string ShieldPierce => Blueprint == null ? null : string.Join(" , ", Blueprint.Attacks.Select(a => a.ShieldPierce.ToString("P1")));
            public string Dev => Blueprint == null ? null : string.Join(" , ", Blueprint.Attacks.Select(a => a.Dev.ToString("P1")));

            public BuildRow(IBuilder builder, string name, double energy, double mass, MechBlueprint blueprint)
                : this(builder, name, energy, mass)
            {
                this.Blueprint = blueprint;
            }
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
