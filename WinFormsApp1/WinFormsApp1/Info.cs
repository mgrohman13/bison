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
using ClassLibrary1.Pieces.Terrain;
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

        public void SetSelected(Tile selected)
        {
            this.selected = selected;
        }

        public override void Refresh()
        {
            ShowAll(false);
            lblTurn.Text = Program.Game.Turn.ToString();

            if (selected != null && selected.Piece != null)
            {
                if (selected.Piece is IBuilder)
                    btnBuild.Show();
                else
                    btnBuild.Hide();

                lblHeading.Show();
                lblHeading.Text = selected.Piece.ToString();

                PlayerPiece playerPiece = selected.Piece as PlayerPiece;

                if (selected.Piece is IKillable killable)
                {
                    double repairInc = 0;
                    if (playerPiece != null)
                        repairInc = playerPiece.GetRepairInc();

                    lbl1.Show();
                    lblInf1.Show();
                    lbl1.Text = "Hits";
                    lblInf1.Text = string.Format("{0} / {1}{2}{3}",
                        Format(killable.HitsCur), Format(killable.HitsMax),
                        killable.Armor > 0 ? string.Format(" ({0})", FormatPct(killable.Armor)) : "",
                        repairInc != 0 ? string.Format(" +{0}", Format(repairInc)) : "");

                    if (killable.ShieldInc > 0)
                    {
                        lbl2.Show();
                        lblInf2.Show();
                        lbl2.Text = "Shield";
                        lblInf2.Text = string.Format("{0} / {1} / {2} +{3}",
                            Format(killable.ShieldCur), Format(killable.ShieldMax), Format(killable.ShieldLimit), Format(killable.GetInc()));
                    }
                }
                if (selected.Piece is IMovable movable)
                {
                    lbl3.Show();
                    lblInf3.Show();
                    lbl3.Text = "Move";
                    lblInf3.Text = string.Format("{0} / {1} / {2} +{3}",
                            Format(movable.MoveCur), Format(movable.MoveMax), Format(movable.MoveLimit), Format(movable.GetInc()));
                }
                if (playerPiece != null)
                {
                    lbl4.Show();
                    lblInf4.Show();
                    lbl4.Text = "Vision";
                    lblInf4.Text = string.Format("{0}", Format(playerPiece.Vision));

                    if (!(playerPiece is Extractor))
                    {
                        double a, energyUpk, b, massUpk, c, d;
                        a = energyUpk = b = massUpk = c = d = 0;
                        playerPiece.GenerateResources(ref a, ref energyUpk, ref b, ref massUpk, ref c, ref d);
                        if (energyUpk != 0)
                        {
                            lbl5.Show();
                            lblInf5.Show();
                            lbl5.Text = "Energy";
                            lblInf5.Text = string.Format("{1}{0}", Format(energyUpk), energyUpk < 0 ? "+" : "-");
                        }
                        if (massUpk != 0)
                        {
                            lbl6.Show();
                            lblInf6.Show();
                            lbl6.Text = "Mass";
                            lblInf6.Text = string.Format("{1}{0}", Format(massUpk), massUpk < 0 ? "+" : "-");
                        }
                        //if (researchInc != 0)
                        //{
                        //    lbl7.Show();
                        //    lblInf7.Show();
                        //    lbl7.Text = "Research";
                        //    lblInf7.Text = string.Format("{1}{0}", Format(researchInc), researchInc > 0 ? "+" : "");
                        //}
                    }
                }
                if (selected.Piece is IRepair repair)
                {
                    lbl7.Show();
                    lblInf7.Show();
                    lbl7.Text = "Repairs";
                    lblInf7.Text = string.Format("{0}", FormatPct(repair.Rate));
                }

                Resource resource = selected.Piece as Resource;
                Extractor extractor = selected.Piece as Extractor;
                if (resource == null && extractor != null)
                    resource = extractor.Resource;
                if (resource != null)
                {
                    if (extractor == null)
                    {
                        Extractor.Cost(out double energy, out double mass, resource);
                        lbl2.Show();
                        lblInf2.Show();
                        lbl2.Text = "Build Cost";
                        lblInf2.Text = string.Format("{0} : {1}", Format(energy), Format(mass));
                    }

                    double energyInc, energyUpk, massInc, massUpk, researchInc, researchUpk;
                    energyInc = energyUpk = massInc = massUpk = researchInc = researchUpk = 0;
                    resource.GenerateResources(selected.Piece, ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
                    energyInc -= energyUpk;
                    massInc -= massUpk;
                    researchInc -= researchUpk;

                    if (energyInc != 0)
                    {
                        lbl5.Show();
                        lblInf5.Show();
                        lbl5.Text = "Energy";
                        lblInf5.Text = string.Format("{1}{0}", Format(energyInc), energyInc > 0 ? "+" : "");
                    }
                    if (massInc != 0)
                    {
                        lbl6.Show();
                        lblInf6.Show();
                        lbl6.Text = "Mass";
                        lblInf6.Text = string.Format("{1}{0}", Format(massInc), massInc > 0 ? "+" : "");
                    }
                    if (researchInc != 0)
                    {
                        lbl7.Show();
                        lblInf7.Show();
                        lbl7.Text = "Research";
                        lblInf7.Text = string.Format("{1}{0}", Format(researchInc), researchInc > 0 ? "+" : "");
                    }

                    lbl8.Show();
                    lblInf8.Show();
                    lbl8.Text = "Sustainability";
                    lblInf8.Text = string.Format("{0}", FormatPct(resource.Sustain));
                }

                if (selected.Piece is IAttacker attacker)
                {
                    dgvAttacks.Show();

                    dgvAttacks.DataSource = attacker.Attacks.OrderByDescending(a => a.Range).ToList();
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

                    int labelsY = this.Controls.OfType<Label>().Where(lbl => lbl.Visible && lbl.Parent != this.panel1).Max(lbl => lbl.Location.Y + lbl.Height);
                    dgvAttacks.MaximumSize = new Size(this.Width, this.panel1.Location.Y - labelsY);
                    dgvAttacks.Size = dgvAttacks.PreferredSize;
                    dgvAttacks.Location = new Point(0, this.panel1.Location.Y - dgvAttacks.Height);
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
            lblTurn.Show();
        }

        public void BtnBuild_Click(object sender, EventArgs e)
        {
            if (selected != null && selected.Piece is IBuilder builder)
            {
                Piece result = Program.DgvForm.BuilderDialog(builder);
                if (result != null)
                    Program.Form.MapMain.SelTile = result.Tile;
            }
        }

        public void BtnViewAtt_Click(object sender, EventArgs e)
        {
            Program.Form.MapMain.ViewAttacks = !Program.Form.MapMain.ViewAttacks;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Program.Game.SaveGame();
        }
    }
}
