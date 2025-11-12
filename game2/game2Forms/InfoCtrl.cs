using game2.game;
using game2.map;
using game2.pieces;
using game2.pieces.behavior;
using game2.pieces.player;
using System.Diagnostics;

namespace game2Forms
{
    public partial class InfoCtrl : UserControl
    {
        public InfoCtrl()
        {
            InitializeComponent();

            //// If labels change their text internally, notify parent (optional)
            //label1.TextChanged += ChildLabel_TextChanged;
            //label2.TextChanged += ChildLabel_TextChanged;
            //label3.TextChanged += ChildLabel_TextChanged;
            //label4.TextChanged += ChildLabel_TextChanged;

            //tableLayoutPanel.ColumnStyles[1].
        }

        private void ChildLabel_TextChanged(object? sender, EventArgs e)
        {
            // Inform parent that layout may need updating.
            // We do not resize here because only the form knows the available max width.
            Parent?.PerformLayout();
        }

        public bool RefreshInfo(int topSpacing, int minWidth, int maxWidth)
        {
            lblHeader.Text = "";
            this.lblHeader.AutoSize = true;

            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowCount = 0;
            tableLayoutPanel.RowStyles.Clear();

            Tile? selected = Program.Form?.Map.Selected;
            if (selected != null)
            {
                Piece? piece = selected.Piece;
                if (piece != null)
                {
                    lblHeader.Text = piece.ToString();
                    SkipRow();

                    PlayerPiece? playerPiece = piece as PlayerPiece;
                    if (playerPiece != null)
                        AddRow(Images.Vision, playerPiece.Vision, playerPiece.VisionBase);
                    if (piece.HasBehavior(out Movable? movable))
                    {
                        AddRow(Images.Move, movable.MoveCur, movable.MoveMax);
                        int moveInc = movable.MoveInc;// Math.Min(movable.MoveInc, movable.MoveMax - movable.MoveCur);
                                                      //if (moveInc > 0)
                        AddRow("", FormatInc(moveInc), moveInc == movable.MoveBase ? null : movable.MoveBase);
                        SkipRow();
                    }
                    if (piece.HasBehavior(out Combatant? combatant))
                    {
                        AddRow(Images.Attack, combatant.AttCur, combatant.AttBase);
                        AddRow(Images.Defense, combatant.DefCur, combatant.DefBase);
                        AddRow(Images.HP, combatant.HPCur, combatant.HPMax);
                        SkipRow();
                    }
                    if (playerPiece != null)
                    {
                        Resources r = playerPiece.GetTurnEnd();
                        for (int a = 0; a < Resources.NumResources; a++)
                        {
                            int value = r[a];
                            if (value != 0)
                                AddRow(Images.Resources[a], FormatInc(value));
                        }
                    }
                }
                else
                {
                    Terrain terrain = selected.Terrain;
                    lblHeader.Text = terrain.ToString();

                    AddRow(Images.Move, terrain.MoveCost());
                    AddRow(Images.Vision, terrain.VisionCost());

                    int att = terrain.AttMod();
                    if (att != 0)
                        AddRow(Images.Attack, FormatInc(att));
                    int def = terrain.DefMod();
                    if (def != 0)
                        AddRow(Images.Defense, FormatInc(def));
                    int vision = terrain.VisionMod();
                    if (vision != 0)
                        AddRow(Images.Vision, FormatInc(vision));
                }
            }

            void SkipRow() => AddRow(" ", "");
            void AddRow(object label, object value, object? baseValue = null)
            {
                int row = tableLayoutPanel.RowCount;
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                Control? col1 = null;
                string? lblStr = label.ToString();
                if (label is Image image)
                {
                    PictureBox pbLabel = CreatePictureBox();
                    pbLabel.Image = image;
                    col1 = pbLabel;
                }
                else if (lblStr?.Length > 0)
                {
                    Label lblLabel = CreateLabel();
                    lblLabel.Text = lblStr;
                    col1 = lblLabel;
                }

                Label? col2 = null;
                string? valueStr = value.ToString();
                if (valueStr?.Length > 0)
                {
                    col2 = CreateLabel();
                    col2.Text = valueStr;

                    string? baseStr = baseValue?.ToString();
                    if (baseStr != null && baseStr != valueStr)
                        col2.Text += $" / {baseStr}";
                }

                if (col1 != null)
                    tableLayoutPanel.Controls.Add(col1, 0, row);
                if (col2 != null)
                    tableLayoutPanel.Controls.Add(col2, 1, row);
            }

            tableLayoutPanel.ResumeLayout();

            return EnsureSize(topSpacing, minWidth, maxWidth);
        }
        public static string FormatInc(int value) => $"{(value > 0 ? "+" : "")}{value}";

        private PictureBox CreatePictureBox()
        {
            PictureBox targetControl = new();
            PictureBox sourceControl = pbTemplate;
            CopyControl(sourceControl, targetControl);

            targetControl.SizeMode = sourceControl.SizeMode;

            return targetControl;
        }
        private Label CreateLabel()
        {
            Label targetControl = new();
            Label sourceControl = lblTemplate;
            CopyControl(sourceControl, targetControl);

            targetControl.TextAlign = sourceControl.TextAlign;
            targetControl.Font = sourceControl.Font;

            targetControl.AutoSize = true;

            return targetControl;
        }
        private static void CopyControl(Control sourceControl, Control targetControl)
        {
            targetControl.Anchor = sourceControl.Anchor;
            targetControl.Size = sourceControl.Size;
            sourceControl.Visible = false;
            targetControl.Visible = true;
        }

        /// <summary>
        /// Ensures the Info control is wide enough to display its table contents up to maxWidth.
        /// If allowShrink is false, Info will only grow, never shrink.
        /// </summary>
        public bool EnsureSize(int topSpacing, int minWidth, int maxWidth)
        {
            lblHeader.Location = new(0, topSpacing);
            tableLayoutPanel.Location = new(0, lblHeader.Bottom);

            // Ask the internal TableLayoutPanel what it prefers when constrained to maxWidth.
            Size preferred = tableLayoutPanel.GetPreferredSize(new Size(maxWidth, 0));
            int targetWidth = Math.Max(preferred.Width, lblHeader.PreferredWidth);
            Debug.WriteLine($"tableLayoutPanel1.GetPreferredSize: {targetWidth}");

            // Keep a little breathing room for padding/borders
            targetWidth += Padding.Horizontal;

            // Respect the min/max
            if (targetWidth < minWidth)
                targetWidth = minWidth;
            else if (targetWidth > maxWidth)
                targetWidth = maxWidth;

            bool changed = Width != targetWidth;
            this.Width = targetWidth;

            int h = lblHeader.Height;
            this.lblHeader.AutoSize = false;
            this.lblHeader.Width = targetWidth;
            this.lblHeader.Height = h;

            return changed;
        }
    }
}
