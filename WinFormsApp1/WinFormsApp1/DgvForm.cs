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
        private Tile selected;
        private Piece result;

        public DgvForm()
        {
            InitializeComponent();
            dataGridView1.CellContentClick += DataGridView1_CellClick;
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

            rows = new List<BuildRow>();
            result = null;

            IBuilder.IBuildConstructor buildConstructor = GetBuilder<IBuilder.IBuildConstructor>(selected);
            IBuilder.IBuildMech buildMech = GetBuilder<IBuilder.IBuildMech>(selected);
            IBuilder.IBuildExtractor buildExtractor = GetBuilder<IBuilder.IBuildExtractor>(selected);
            IBuilder.IBuildFactory buildFactory = GetBuilder<IBuilder.IBuildFactory>(selected);
            IBuilder.IBuildTurret buildTurret = GetBuilder<IBuilder.IBuildTurret>(selected);

            if (buildConstructor != null)
            {
                Constructor.Cost(Program.Game, out int energy, out int mass);
                BuildRow row = new(buildConstructor, "Constructor", energy, mass);
                rows.Add(row);
            }
            if (buildMech != null)
            {
                foreach (MechBlueprint blueprint in Program.Game.Player.Research.Blueprints)
                {
                    BuildRow row = new(buildMech, "Mech", blueprint.Energy, blueprint.Mass, blueprint);
                    rows.Add(row);
                }
            }
            if (buildExtractor != null && selected.Piece is Resource resource)
            {
                Extractor.Cost(out int energy, out int mass, resource);
                BuildRow row = new(buildExtractor, "Extractor", energy, mass);
                rows.Add(row);
            }
            Foundation foundation = selected.Piece as Foundation;
            if (buildFactory != null && foundation != null)
            {
                Factory.Cost(Program.Game, out int energy, out int mass);
                BuildRow row = new(buildFactory, "Factory", energy, mass);
                rows.Add(row);
            }
            if (buildTurret != null && foundation != null)
            {
                Turret.Cost(Program.Game, out int energy, out int mass);
                BuildRow row = new(buildTurret, "Turret", energy, mass);
                rows.Add(row);
            }

            Display();

            ShowDialog();
            return result;
        }
        internal bool UpgradeInfo(MechBlueprint blueprint)
        {
            this.selected = null;
            dataGridView1.Hide();

            rows = new List<BuildRow>();
            result = null;

            rows.Add(new(null, null, blueprint.Energy, blueprint.Mass, blueprint));
            MechBlueprint upgrade = blueprint.UpgradeTo;
            while (upgrade != null)
            {
                rows.Add(new(null, null, upgrade.Energy, upgrade.Mass, upgrade));
                upgrade = upgrade.UpgradeTo;
            }

            Display();
            dataGridView1.Columns["Name"].Visible = false;
            dataGridView1.Columns["Upgraded"].Visible = false;
            dataGridView1.Columns["Build"].Visible = false;
            dataGridView1.Columns["Notify"].Visible = false;
            ShowDialog();

            return false;
        }
        private void Display()
        {
            if (rows.Any())
            {
                dataGridView1.DataSource = rows;

                dataGridView1.Columns["Name"].Visible = true;
                dataGridView1.Columns["Upgraded"].Visible = true;

                dataGridView1.Columns["Builder"].Visible = false;

                dataGridView1.Columns["Resilience"].DefaultCellStyle.Format = "P0";
                dataGridView1.Columns["Armor"].DefaultCellStyle.Format = "P0";

                dataGridView1.Columns["Upgraded"].Visible = rows.Any(r => r.Upgraded != null);
                dataGridView1.Columns["Armor"].Visible = rows.Any(r => r.Armor > 0);
                dataGridView1.Columns["Shields"].Visible = rows.Any(r => r.Blueprint?.Killable.ShieldInc > 0);
                dataGridView1.Columns["ArmorPierce"].Visible =
                    rows.SelectMany(r => r.Blueprint?.Attacks ?? Array.Empty<IAttacker.Values>()).Any(a => a.ArmorPierce > 0);
                dataGridView1.Columns["ShieldPierce"].Visible =
                    rows.SelectMany(r => r.Blueprint?.Attacks ?? Array.Empty<IAttacker.Values>()).Any(a => a.ShieldPierce > 0);

                //dataGridView1.Location = new System.Drawing.Point(0, 0);
                //dataGridView1.Size = dataGridView1.PreferredSize;
                //this.ClientSize = dataGridView1.PreferredSize;

                if (!dataGridView1.Columns.Contains("Build"))
                {
                    DataGridViewButtonColumn buildColumn = new DataGridViewButtonColumn();
                    buildColumn.Name = "Build";
                    buildColumn.Text = "Build";
                    buildColumn.UseColumnTextForButtonValue = true;
                    buildColumn.HeaderText = "";
                    dataGridView1.Columns.Add(buildColumn);
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.AutoResizeRows();
                }
                dataGridView1.Columns["Build"].Visible = true;

                if (!dataGridView1.Columns.Contains("Notify"))
                {
                    DataGridViewCheckBoxColumn notifyColumn = new DataGridViewCheckBoxColumn();
                    notifyColumn.Name = "Notify";
                    dataGridView1.Columns.Add(notifyColumn);
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.AutoResizeRows();
                }
                dataGridView1.Columns["Notify"].Visible = true;

                dataGridView1.Show();
            }
        }
        public static T GetBuilder<T>(Tile selected) where T : class, IBuilder
        {
            return Program.Game.Player.PiecesOfType<T>().FirstOrDefault(b => selected.GetDistance(b.Piece.Tile) <= b.Range);
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if (selected != null
                && e.ColumnIndex >= 0 && e.ColumnIndex < senderGrid.Columns.Count
                && e.RowIndex >= 0 && e.RowIndex < rows.Count
                && senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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
                if (result != null)
                    this.Close();
            }
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

            public string Movement => Blueprint == null ? null : string.Format("{1} / {2} +{0:0.0}", Blueprint.Movable.MoveInc, Blueprint.Movable.MoveMax, Blueprint.Movable.MoveLimit);
            public string Vision => Info.FormatDown(Blueprint?.Vision);

            public double? Hits => Blueprint?.Killable.HitsMax;
            public double? Resilience => Blueprint?.Killable.Resilience;
            public double? Armor => Blueprint?.Killable.Armor;
            public string Shields => Blueprint == null ? null : Blueprint.Killable.ShieldInc <= 0 ? "" :
                string.Format("{1} / {2} +{0:0.0}", Blueprint.Killable.ShieldInc, Blueprint.Killable.ShieldMax, Blueprint.Killable.ShieldLimit);

            public int? Attacks => Blueprint?.Attacks.Count;
            public string Range => Blueprint == null ? null : List(a => a.Range.ToString("0.0"));
            public string Damage => Blueprint == null ? null : List(a => a.Damage.ToString("0"));
            public string ArmorPierce => Blueprint == null ? null : List(a => a.ArmorPierce.ToString("P0"));
            public string ShieldPierce => Blueprint == null ? null : List(a => a.ShieldPierce.ToString("P0"));
            public string Randomness => Blueprint == null ? null : List(a => a.Dev.ToString("P0"));

            private string List(Func<IAttacker.Values, string> Get)
            {
                var data = Blueprint.Attacks.Select(Get);
                if (data.All("0%".Equals))
                    return "0%";
                return string.Join(" , ", data);
            }

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
