using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Daemons;

namespace DaemonsWinApp
{
    public partial class MoveForm : Form
    {
        private static MoveForm form = new MoveForm();

        private Tile tile, moveTo;
        private bool fire;
        private List<List<Unit>> types = new List<List<Unit>>(4);
        private List<Unit> move = new List<Unit>(), units = new List<Unit>();

        private MoveForm()
        {
            InitializeComponent();

            this.lblInf1.Tag = 0;
            this.lblUnit1.Tag = 0;
            this.txtNum0.Tag = 0;
            this.btnCustomize0.Tag = 0;
            this.btnHealed0.Tag = 0;
            this.btnMax0.Tag = 0;
            this.btnNone0.Tag = 0;
            this.lblInf2.Tag = 1;
            this.lblUnit2.Tag = 1;
            this.txtNum1.Tag = 1;
            this.btnCustomize1.Tag = 1;
            this.btnHealed1.Tag = 1;
            this.btnMax1.Tag = 1;
            this.btnNone1.Tag = 1;
            this.lblInf3.Tag = 2;
            this.lblUnit3.Tag = 2;
            this.txtNum2.Tag = 2;
            this.btnCustomize2.Tag = 2;
            this.btnHealed2.Tag = 2;
            this.btnMax2.Tag = 2;
            this.btnNone2.Tag = 2;
            this.lblInf4.Tag = 3;
            this.lblUnit4.Tag = 3;
            this.txtNum3.Tag = 3;
            this.btnCustomize3.Tag = 3;
            this.btnHealed3.Tag = 3;
            this.btnMax3.Tag = 3;
            this.btnNone3.Tag = 3;
        }

        private bool SetupStuff(Tile tile, Tile moveTo, bool fire)
        {
            this.pbArrows.Visible = fire;
            this.lblArrows.Visible = fire;
            if (fire)
            {
                int arrows = tile.Game.GetCurrentPlayer().Arrows;
                if (tile.IsCornerNeighbor(moveTo))
                    arrows /= 2;
                this.lblArrows.Text = arrows.ToString();
            }

            this.button1.Text = ( fire ? "Fire!" : "OK" );
            this.Text = ( fire ? "Fire" : "Move Troops" );

            this.tile = tile;
            this.moveTo = moveTo;
            this.fire = fire;

            this.types = new List<List<Unit>>(4);
            this.move = new List<Unit>();
            this.units = new List<Unit>();

            UnitType? unitType;
            if (fire)
                unitType = UnitType.Archer;
            else if (tile.IsSideNeighbor(moveTo))
                unitType = null;
            else
                unitType = UnitType.Daemon;
            units = tile.GetUnits(tile.Game.GetCurrentPlayer(), !this.cbxForce.Checked, false, unitType).ToList();
            if (this.cbxForce.Checked)
                units = units.Where(unit => ( unit.Movement + unit.ReserveMovement > 0 )).ToList();
            if (fire)
                units.Sort(UnitDamageComparison);

            if (units.Count == 0)
                if (this.cbxForce.Checked || fire)
                {
                    return false;
                }
                else
                {
                    this.cbxForce.Checked = true;
                    return SetupStuff(tile, moveTo, fire);
                }

            for (int a = 0 ; a < 4 ; a++)
                types.Add(new List<Unit>());

            foreach (Unit unit in units)
            {
                switch (unit.Type)
                {
                case UnitType.Archer:
                    types[1].Add(unit);
                    if (unit.Healed)
                        move.Add(unit);
                    break;

                case UnitType.Daemon:
                    types[3].Add(unit);
                    if (unit.Healed)
                        move.Add(unit);
                    break;

                case UnitType.Infantry:
                    types[0].Add(unit);
                    if (unit.Healed)
                        move.Add(unit);
                    break;

                case UnitType.Knight:
                    types[2].Add(unit);
                    if (unit.Healed)
                        move.Add(unit);
                    break;

                default:
                    throw new Exception("");
                }
            }

            Label[] pics = new[] { this.lblUnit1, this.lblUnit2, this.lblUnit3, this.lblUnit4 };
            Label[] infos = new[] { this.lblInf1, this.lblInf2, this.lblInf3, this.lblInf4 };

            int num = 0;

            for (int a = 0 ; a < types.Count ; a++)
                if (types[a].Count > 0)
                {
                    infos[num].Text = string.Format("{0} / {1} / {2}", types[a].Count(unit => unit.Healed),
                            types[a].Count(unit => ( unit.Movement > 0 )), types[a].Count());
                    pics[num++].Image = types[a][0].GetPic();
                }

            for (int a = types.Count - 1 ; a >= 0 ; a--)
                if (types[a].Count <= 0)
                    types.RemoveAt(a);

            foreach (Control control in Controls)
                if (control.Tag is int)
                    control.Visible = ( (int)control.Tag != num );

            this.Height = 462;
            for (int a = num ; a < 4 ; a++)
                this.Height -= 90;

            RefreshSection();

            return true;
        }
        private static int UnitDamageComparison(Unit unit1, Unit unit2)
        {
            return Math.Sign(unit2.Tile.GetDamage(unit2) - unit1.Tile.GetDamage(unit1));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (fire)
                Unit.Fire(move, moveTo);
            else
                foreach (Unit unit in move)
                    unit.Move(moveTo);
        }

        private void btnNone_Click(object sender, EventArgs e)
        {
            List<Unit> units = types[(int)( (Control)sender ).Tag];
            foreach (Unit unit in units)
                if (move.Contains(unit))
                    move.Remove(unit);
            RefreshSection((int)( (Control)sender ).Tag);
        }

        private void btnMax_Click(object sender, EventArgs e)
        {
            List<Unit> units = types[(int)( (Control)sender ).Tag];
            foreach (Unit unit in units)
                if (!move.Contains(unit))
                    move.Add(unit);
            RefreshSection((int)( (Control)sender ).Tag);
        }

        private void btnHealed_Click(object sender, EventArgs e)
        {
            List<Unit> units = types[(int)( (Control)sender ).Tag];
            foreach (Unit unit in units)
            {
                if (move.Contains(unit))
                    move.Remove(unit);
                if (unit.Healed)
                    move.Add(unit);
            }
            RefreshSection((int)( (Control)sender ).Tag);
        }

        private void btnCustomize_Click(object sender, EventArgs e)
        {
            List<Unit> temp = types[(int)( (Control)sender ).Tag];
            InfoForm.ShowDialog(ref units, ref temp, ref move, UseType.Move);
            types[(int)( (Control)sender ).Tag] = temp;

            RefreshSection();
        }

        private void RefreshSection()
        {
            for (int a = 0 ; a < types.Count ; a++)
                RefreshSection(a);
        }

        private void RefreshSection(int section)
        {
            if (section < types.Count)
            {
                List<Unit> units = types[section];
                int count = 0;
                foreach (Unit unit in units)
                    if (move.Contains(unit))
                        count++;
                foreach (Control control in Controls)
                    if (control is TextBox && (int)control.Tag == section)
                    {
                        ( (TextBox)control ).Text = count.ToString();
                        break;
                    }
            }
        }

        private void MoveForm_KeyDown(object sender, KeyEventArgs e)
        {
            MainForm.shift = e.Shift;
        }

        private void MoveForm_KeyUp(object sender, KeyEventArgs e)
        {
            MainForm.shift = e.Shift;
        }

        private static bool events = true;
        public static DialogResult ShowDialog(Tile tile, Tile moveTo, bool fire)
        {
            events = false;
            form.cbxForce.Visible = !fire;
            form.cbxForce.Checked = false;
            events = true;

            if (form.SetupStuff(tile, moveTo, fire))
                return form.ShowDialog();
            else
                return System.Windows.Forms.DialogResult.Cancel;
        }

        private void cbxForce_CheckedChanged(object sender, EventArgs e)
        {
            if (events)
                form.SetupStuff(tile, moveTo, fire);
        }
    }
}