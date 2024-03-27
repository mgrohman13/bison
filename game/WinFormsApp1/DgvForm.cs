using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Tile;

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
            dataGridView1.PreviewKeyDown += DataGridView1_PreviewKeyDown;
        }

        private void DataGridView1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            dataGridView1.EndEdit();
            if (e.KeyCode == Keys.Escape)
                this.Close();
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
                dataGridView1.Columns["Build"].Visible = true;
                dataGridView1.Columns["Notify"].Visible = true;

                dataGridView1.Columns["Upgraded"].Visible = rows.Any(r => r.Upgraded != null);
                dataGridView1.Columns["Resilience"].Visible = rows.Count > 1;
                dataGridView1.Columns["Weapons"].Visible = CheckStats(b => b.Attacker, a => a.Type != AttackType.Kinetic);
                dataGridView1.Columns["Range"].Visible = CheckStats(b => b.Attacker, a => a.Range > Attack.MELEE_RANGE);
                dataGridView1.Columns["Shield"].Visible = CheckStats(b => b.Killable, k => k.Type == DefenseType.Shield);
                dataGridView1.Columns["Armor"].Visible = CheckStats(b => b.Killable, k => k.Type == DefenseType.Armor);

                dataGridView1.Columns["Builder"].Visible = false;
                dataGridView1.Columns["Resilience"].DefaultCellStyle.Format = "P0";

                dataGridView1.Columns["Build"].HeaderText = "";
                dataGridView1.Columns["Build"].DefaultCellStyle.Font = new Font(dataGridView1.DefaultCellStyle.Font, FontStyle.Bold);

                bool CheckStats<T>(Func<MechBlueprint, IEnumerable<T>> Get, Func<T, bool> Predicate) =>
                    rows.Any(r => r.Blueprint != null && Get(r.Blueprint).Any(Predicate));

                BuildRow.DataGridView1 = dataGridView1;
                dataGridView1.Show();
            }
        }
        public static T GetBuilder<T>(Tile selected) where T : class, IBuilder
        {
            //check blocks
            return Program.Game.Player.PiecesOfType<T>().FirstOrDefault(b => selected.GetDistance(b.Piece.Tile) <= b.Range);
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if (selected != null
                && e.ColumnIndex >= 0 && e.ColumnIndex < senderGrid.Columns.Count
                && e.RowIndex >= 0 && e.RowIndex < rows.Count
                && senderGrid.Columns[e.ColumnIndex].Name == "Build")
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

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void DgvForm_FormClosing(object sender, EventArgs e)
        {
            dataGridView1.EndEdit();
        }

        public class BuildRow
        {
            public static DataGridView DataGridView1 { private get; set; }

            public IBuilder Builder { get; }

            public string Name { get; }
            public double Energy { get; }
            public double Mass { get; }
            public bool Notify
            {
                get
                {
                    if (Blueprint is not null)
                        return Program.GetNotify(Blueprint);
                    if (Builder is IBuilder.IBuildConstructor)
                        return Program.NotifyConstructor;
                    return true;
                }
                set
                {
                    if (Blueprint is not null)
                        Program.SetNotify(Blueprint, value);
                    if (Builder is IBuilder.IBuildConstructor)
                        Program.NotifyConstructor = value;
                }
            }
            public MechBlueprint Blueprint { get; }
            public MechBlueprint Upgraded => Blueprint?.UpgradeFrom; public int? Research => Blueprint?.ResearchLevel;
            public double? Resilience => Blueprint?.Resilience;
            public string Vision => Info.FormatDown(Blueprint?.Vision);
            public string Movement => Blueprint == null ? null : string.Format("{1} / {2} +{0:0.0}",
                Blueprint.Movable.MoveInc, Blueprint.Movable.MoveMax, Blueprint.Movable.MoveLimit);
            public string Weapons => Blueprint == null ? null : FormatAtt(a => a.Type.ToString());
            public string Range => Blueprint == null ? null :
                FormatAtt(a => a.Range > ClassLibrary1.Pieces.Attack.MELEE_RANGE ? a.Range.ToString("0.0") : "0");
            public string Attack => Blueprint == null ? null : FormatAtt(a => a.Attack.ToString());
            public int? Defense => FormatDef(DefenseType.Hits);
            public int? Shield => FormatDef(DefenseType.Shield);
            public int? Armor => FormatDef(DefenseType.Armor);

            public string Build
            {
                get => Program.Game.Player.Has(Energy, Mass) ? "BUILD" : "";
                set => DataGridView1.Invalidate();
            }

            private int? FormatDef(DefenseType type)
            {
                return Blueprint?.Killable.Where(k => k.Type == type).SingleOrDefault().Defense;
            }
            private string FormatAtt(Func<IAttacker.Values, string> Get)
            {
                var data = Blueprint.Attacker.Select(Get);
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
    }
}
