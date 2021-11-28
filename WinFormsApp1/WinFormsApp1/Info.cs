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
            Program.Form.Refresh();
        }

        internal void SetSelected(Tile selected)
        {
            this.selected = selected;
        }

        public override void Refresh()
        {
            if (selected != null)
            {
                ShowAll(false);

                if (selected.Piece is IKillable killable)
                {
                    lbl1.Show();
                    lblInf1.Show();
                    lbl1.Text = "Hits";
                    lblInf1.Text = string.Format("{0} / {1} ({2})",
                        Format(killable.HitsCur), Format(killable.HitsMax), FormatPct(killable.Armor));

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
                if (selected.Piece is IAttacker attacker)
                {
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

                    dgvAttacks.MaximumSize = new Size(this.Width, this.Height - this.lbl3.Location.Y - this.lbl3.Height);
                    dgvAttacks.Size = dgvAttacks.PreferredSize;
                    dgvAttacks.Location = new Point(0, this.Height - dgvAttacks.Height);

                    //Debug.WriteLine(dgvAttacks.MaximumSize);
                    //Debug.WriteLine(dgvAttacks.Location);
                    //Debug.WriteLine(dgvAttacks.Height);
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
            foreach (Label label in this.Controls.OfType<Label>())
                if (show)
                    label.Show();
                else
                    label.Hide();
        }
    }
}
