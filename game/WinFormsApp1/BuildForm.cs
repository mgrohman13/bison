using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AttackType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace WinFormsApp1
{
    public partial class BuildForm : DgvForm
    {
        private List<BuildRow> rows;
        private Tile selected;
        private Piece result;

        public BuildForm()
        {
            InitializeComponent();
            dataGridView1.CellContentClick += DataGridView1_CellClick;
        }

        public static bool CanBuild(Tile selected)
        {
            if (selected != null)
            {
                IBuilder.IBuildMech buildMech = GetBuilder<IBuilder.IBuildMech>(selected);
                IBuilder.IBuildConstructor buildConstructor = GetBuilder<IBuilder.IBuildConstructor>(selected);
                IBuilder.IBuildDrone buildDrone = GetBuilder<IBuilder.IBuildDrone>(selected);
                IBuilder.IBuildExtractor buildExtractor = GetBuilder<IBuilder.IBuildExtractor>(selected);
                IBuilder.IBuildOutpost buildOutpost = GetBuilder<IBuilder.IBuildOutpost>(selected);
                IBuilder.IBuildFactory buildFactory = GetBuilder<IBuilder.IBuildFactory>(selected);
                IBuilder.IBuildTurret buildTurret = GetBuilder<IBuilder.IBuildTurret>(selected);
                IBuilder.IBuildGenerator buildGenerator = GetBuilder<IBuilder.IBuildGenerator>(selected);
                Piece piece = selected.Piece;
                if (piece is null)
                {
                    if (buildMech != null)
                        return true;
                    if (buildConstructor != null)
                        return true;
                    if (buildDrone != null)
                        return true;
                }
                else if (piece is Resource || piece is IBuilder.IReplacer<Extractor>)
                {
                    if (buildExtractor != null)
                        return true;
                }
                else if (piece is Foundation || piece is IBuilder.IReplacer<FoundationPiece>)
                {
                    if (buildOutpost != null)
                        return true;
                    if (buildFactory != null)
                        return true;
                    if (buildTurret != null)
                        return true;
                    if (buildGenerator != null)
                        return true;
                }
            }
            return false;
        }
        public void BuilderDialogMech()
        {
            this.selected = null;
            dataGridView1.Hide();
            rows = [];
            result = null;
            GetBlueprints(Program.Game.Player.Core.GetBehavior<IBuilder.IBuildMech>());
            Display();
            //dataGridView1.Columns["Name"].Visible = false;
            //dataGridView1.Columns["Upgraded"].Visible = false;
            dataGridView1.Columns["Build"].Visible = false;
            //dataGridView1.Columns["Notify"].Visible = false;
            ShowDialog();
            //return result;
        }
        public Piece BuilderDialog(Tile selected)
        {
            this.selected = selected;
            dataGridView1.Hide();

            rows = [];
            result = null;

            IBuilder.IBuildMech buildMech = GetBuilder<IBuilder.IBuildMech>(selected);
            IBuilder.IBuildConstructor buildConstructor = GetBuilder<IBuilder.IBuildConstructor>(selected);
            IBuilder.IBuildDrone buildDrone = GetBuilder<IBuilder.IBuildDrone>(selected);
            IBuilder.IBuildExtractor buildExtractor = GetBuilder<IBuilder.IBuildExtractor>(selected);
            IBuilder.IBuildOutpost buildOutpost = GetBuilder<IBuilder.IBuildOutpost>(selected);
            IBuilder.IBuildFactory buildFactory = GetBuilder<IBuilder.IBuildFactory>(selected);
            IBuilder.IBuildTurret buildTurret = GetBuilder<IBuilder.IBuildTurret>(selected);
            IBuilder.IBuildGenerator buildGenerator = GetBuilder<IBuilder.IBuildGenerator>(selected);

            if (buildConstructor != null && selected.Piece == null)
            {
                Constructor.Cost(Program.Game, out int energy, out int mass);
                BuildRow row = new(buildConstructor, "Constructor", energy, mass);
                rows.Add(row);
            }
            if (buildDrone != null && selected.Piece == null)
            {
                Drone.Cost(Program.Game, out int energy, out int mass);
                BuildRow row = new(buildDrone, "Drone", energy, mass);
                rows.Add(row);
            }
            if (buildMech != null && selected.Piece == null)
            {
                GetBlueprints(buildMech);
            }
            if (buildExtractor != null && selected.Piece is Resource resource)
            {
                Extractor.Cost(out int energy, out int mass, resource);
                BuildRow row = new(buildExtractor, "Extractor", energy, mass);
                rows.Add(row);
            }
            Foundation foundation = selected.Piece as Foundation;
            if (buildOutpost != null && foundation != null)
            {
                Outpost.Cost(Program.Game, out int energy, out int mass);
                BuildRow row = new(buildOutpost, "Outpost", energy, mass);
                rows.Add(row);
            }
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
            if (buildGenerator != null && foundation != null)
            {
                Generator.Cost(Program.Game, out int energy, out int mass);
                BuildRow row = new(buildGenerator, "Generator", energy, mass);
                rows.Add(row);
            }

            Display();

            ShowDialog();
            return result;
        }
        internal Piece ReplaceDialog(Piece builder, Tile selected)
        {
            this.selected = selected;
            dataGridView1.Hide();

            rows = [];
            result = null;

            if (builder.HasBehavior(out IBuilder.IBuildExtractor buildExtractor) && selected.Piece is Extractor extractor)
            {
                buildExtractor.Replace(false, extractor, out int energy, out int mass, out bool couldReplace, out _);
                if (couldReplace)
                    rows.Add(new(buildExtractor, "Extractor", energy, mass));
            }
            else if (selected.Piece is FoundationPiece foundationPiece)
            {
                if (builder.HasBehavior<IBuilder.IBuildOutpost>(out var buildOutpost))
                {
                    buildOutpost.Replace(false, foundationPiece, out int energy, out int mass, out bool couldReplace, out _);
                    if (couldReplace)
                        rows.Add(new(buildOutpost, "Outpost", energy, mass));
                }
                if (builder.HasBehavior<IBuilder.IBuildFactory>(out var buildFactory))
                {
                    buildFactory.Replace(false, foundationPiece, out int energy, out int mass, out bool couldReplace, out _);
                    if (couldReplace)
                        rows.Add(new(buildFactory, "Factory", energy, mass));
                }
                if (builder.HasBehavior<IBuilder.IBuildTurret>(out var buildTurret))
                {
                    buildTurret.Replace(false, foundationPiece, out int energy, out int mass, out bool couldReplace, out _);
                    if (couldReplace)
                        rows.Add(new(buildTurret, "Turret", energy, mass));
                }
                if (builder.HasBehavior<IBuilder.IBuildGenerator>(out var buildGenerator))
                {
                    buildGenerator.Replace(false, foundationPiece, out int energy, out int mass, out bool couldReplace, out _);
                    if (couldReplace)
                        rows.Add(new(buildGenerator, "Generator", energy, mass));
                }
            }

            Display();
            dataGridView1.Columns["Upgraded"].Visible = false;
            dataGridView1.Columns["Blueprint"].Visible = false;
            dataGridView1.Columns["Research"].Visible = false;
            dataGridView1.Columns["Vision"].Visible = false;
            dataGridView1.Columns["Movement"].Visible = false;
            dataGridView1.Columns["Attack"].Visible = false;
            dataGridView1.Columns["Defense"].Visible = false;
            ShowDialog();

            return result;
        }
        internal bool UpgradeDialog(MechBlueprint blueprint)
        {
            this.selected = null;
            dataGridView1.Hide();

            rows = [];
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
            if (rows.Count > 0)
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

                dataGridView1.Columns["Ratio"].DefaultCellStyle.Format = "P0";
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
        public void GetBlueprints(IBuilder.IBuildMech buildMech)
        {
            foreach (MechBlueprint blueprint in Program.Game.Player.Research.Blueprints)
            {
                BuildRow row = new(buildMech, "Mech", blueprint.Energy, blueprint.Mass, blueprint);
                rows.Add(row);
            }
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
                if (builder is IBuilder.IBuildMech buildMech)
                    this.result = buildMech.Build(selected, row.Blueprint);
                if (builder is IBuilder.IBuildConstructor buildConstructor)
                    this.result = buildConstructor.Build(selected);
                if (builder is IBuilder.IBuildDrone buildDrone)
                    this.result = buildDrone.Build(selected);

                if (selected.Piece is Extractor extractor && builder is IBuilder.IReplacer<Extractor> replaceExtractor)
                    this.result = replaceExtractor.Replace(true, extractor, out _, out _, out _, out _);
                else if (builder is IBuilder.IBuildExtractor buildExtractor)
                    this.result = buildExtractor.Build(selected.Piece as Resource);

                if (selected.Piece is FoundationPiece foundationPiece && builder is IBuilder.IReplacer<FoundationPiece>)
                {
                    if (row.Name == "Outpost")
                        this.result = builder.GetBehavior<IBuilder.IBuildOutpost>().Replace(true, foundationPiece, out _, out _, out _, out _);
                    if (row.Name == "Factory")
                        this.result = builder.GetBehavior<IBuilder.IBuildFactory>().Replace(true, foundationPiece, out _, out _, out _, out _);
                    if (row.Name == "Turret")
                        this.result = builder.GetBehavior<IBuilder.IBuildTurret>().Replace(true, foundationPiece, out _, out _, out _, out _);
                    if (row.Name == "Generator")
                        this.result = builder.GetBehavior<IBuilder.IBuildGenerator>().Replace(true, foundationPiece, out _, out _, out _, out _);
                }
                else
                {
                    if (builder is IBuilder.IBuildOutpost buildOutpost)
                        this.result = buildOutpost.Build(selected.Piece as Foundation);
                    if (builder is IBuilder.IBuildFactory buildFactory)
                        this.result = buildFactory.Build(selected.Piece as Foundation);
                    if (builder is IBuilder.IBuildTurret buildTurret)
                        this.result = buildTurret.Build(selected.Piece as Foundation);
                    if (builder is IBuilder.IBuildGenerator buildGenerator)
                        this.result = buildGenerator.Build(selected.Piece as Foundation);
                }

                if (result != null)
                    this.Close();
            }
        }

        public class BuildRow(IBuilder builder, string name, double energy, double mass)
        {
            public static DataGridView DataGridView1 { private get; set; }

            public IBuilder Builder { get; } = builder;

            public string Name { get; } = name;
            public double Energy { get; } = energy;
            public double Mass { get; } = mass;
            public double Ratio => Energy / (double)Mass;
            public bool Notify
            {
                get
                {
                    if (Blueprint is not null)
                        return Program.GetNotify(Blueprint);
                    if (Builder is IBuilder.IBuildConstructor)
                        return Program.NotifyConstructor;
                    if (Builder is IBuilder.IBuildDrone)
                        return Program.NotifyDrone;
                    return true;
                }
                set
                {
                    if (Blueprint is not null)
                        Program.SetNotify(Blueprint, value);
                    if (Builder is IBuilder.IBuildConstructor)
                        Program.NotifyConstructor = value;
                    if (Builder is IBuilder.IBuildDrone)
                        Program.NotifyDrone = value;
                }
            }
            public MechBlueprint Blueprint { get; }
            public MechBlueprint Upgraded => Blueprint?.UpgradeFrom;
            public int? Research => Blueprint?.ResearchLevel;
            public double? Resilience => Blueprint?.Resilience;
            public string Vision => Info.FormatDown(Blueprint?.Vision);
            public string Movement => Blueprint == null ? null : string.Format("{1} / {2} +{0:0.0}",
                Blueprint.Movable.MoveInc, Blueprint.Movable.MoveMax, Blueprint.Movable.MoveLimit);
            public string Weapons => Blueprint == null ? null : FormatAtt(a => a.Type.ToString());
            public string Range => Blueprint == null ? null :
                FormatAtt(a => a.Range > ClassLibrary1.Pieces.Behavior.Combat.Attack.MELEE_RANGE ? a.Range.ToString("0.0") : "M");
            public string Attack => Blueprint == null ? null : FormatAtt(a => a.Attack.ToString() + (a.Reload > 1 ? $"(+{a.Reload})" : ""));
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
                if (data.All("0".Equals))
                    return "0";
                return string.Join(" , ", data);
            }

            public BuildRow(IBuilder builder, string name, double energy, double mass, MechBlueprint blueprint)
                : this(builder, name, energy, mass)
            {
                this.Blueprint = blueprint;
            }
        }
    }
}
