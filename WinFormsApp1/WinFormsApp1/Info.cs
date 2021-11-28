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
using Tile = ClassLibrary1.Map.Tile;

namespace WinFormsApp1
{
    public partial class Info : UserControl
    {
        private Tile selected;

        public Info()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Program.EndTurn();
        }

        internal void SetSelected(Tile selected)
        {
            this.selected = selected;
        }

        public override void Refresh()
        {
            ShowAll(false);
            this.lblTurn.Text = Program.Game.Turn.ToString();

            if (selected != null)
            {

                if (selected.Piece is IKillable killable)
                {
                    lbl1.Show();
                    lblInf1.Show();
                    lbl1.Text = "Hits";
                    lblInf1.Text = string.Format("{0} / {1}{2}",
                        Format(killable.HitsCur), Format(killable.HitsMax),
                        killable.Armor > 0 ? string.Format(" ({0})", FormatPct(killable.Armor)) : "");

                    if (killable.ShieldInc > 0)
                    {
                        lbl2.Show();
                        lblInf2.Show();
                        lbl2.Text = "Shield";
                        lblInf2.Text = string.Format("{0} / {1} / {2} +{3}",
                            Format(killable.ShieldCur), Format(killable.ShieldMax), Format(killable.ShieldLimit), Format(killable.ShieldInc));
                    }
                }
                if (selected.Piece is IMovable movable)
                {
                    lbl3.Show();
                    lblInf3.Show();
                    lbl3.Text = "Move";
                    lblInf3.Text = string.Format("{0} / {1} / {2} +{3}",
                            Format(movable.MoveCur), Format(movable.MoveMax), Format(movable.MoveLimit), Format(movable.MoveInc));
                }
                if (selected.Piece is PlayerPiece playerPiece)
                {
                    lbl4.Show();
                    lblInf4.Show();
                    lbl4.Text = "Vision";
                    lblInf4.Text = string.Format("{0}", Format(playerPiece.Vision));
                }
                if (selected.Piece is IAttacker attacker)
                {
                    dgvAttacks.Show();

                    dgvAttacks.DataSource = attacker.Attacks;
                    dgvAttacks.Columns["Range"].DisplayIndex = 0;
                    dgvAttacks.Columns["Range"].HeaderText = "RANGE";
                    dgvAttacks.Columns["Range"].DefaultCellStyle.Format = "0.0";
                    dgvAttacks.Columns["Damage"].DisplayIndex = 1;
                    dgvAttacks.Columns["Damage"].HeaderText = "DMG";
                    dgvAttacks.Columns["Damage"].DefaultCellStyle.Format = "0.0";
                    dgvAttacks.Columns["ArmorPierce"].DisplayIndex = 2;
                    dgvAttacks.Columns["ArmorPierce"].HeaderText = "AP";
                    dgvAttacks.Columns["ArmorPierce"].DefaultCellStyle.Format = "P1";
                    dgvAttacks.Columns["ShieldPierce"].DisplayIndex = 3;
                    dgvAttacks.Columns["ShieldPierce"].HeaderText = "SP";
                    dgvAttacks.Columns["ShieldPierce"].DefaultCellStyle.Format = "P1";
                    dgvAttacks.Columns["Dev"].DisplayIndex = 4;
                    dgvAttacks.Columns["Dev"].HeaderText = "RNG";
                    dgvAttacks.Columns["Dev"].DefaultCellStyle.Format = "P1";
                    dgvAttacks.Columns["Attacked"].DisplayIndex = 5;
                    dgvAttacks.Columns["Attacked"].HeaderText = "USED";

                    int labelsY = this.Controls.OfType<Label>().Select(lbl => lbl.Visible).Max(lbl => this.lbl3.Location.Y + this.lbl3.Height);
                    dgvAttacks.MaximumSize = new Size(this.Width, this.Height - labelsY);
                    dgvAttacks.Size = dgvAttacks.PreferredSize;
                    dgvAttacks.Location = new Point(0, this.Height - dgvAttacks.Height);
                }
            }

            base.Refresh();
        }

        private static string Format(double value)
        {
            return value.ToString("0.0");
        }
        private static string FormatPct(double value)
        {
            return value.ToString("P1");
        }

        private void ShowAll(bool show)
        {
            foreach (Control label in this.Controls.OfType<Label>().OfType<Control>().Concat(new Control[] { dgvAttacks }))
                if (show)
                    label.Show();
                else
                    label.Hide();
            this.lblTurn.Show();
        }
    }
}
