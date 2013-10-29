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
        private int max;
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

        private bool SetupStuff(Tile tile, Tile moveTo, bool fire, int max)
        {
            this.button1.Text = ( fire ? "Fire!" : "OK" );

            this.tile = tile;
            this.moveTo = moveTo;
            this.fire = fire;
            this.max = max;

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
            units = tile.GetUnits(tile.Game.GetCurrentPlayer(), !this.cbxForce.Checked, unitType);
            if (this.cbxForce.Checked)
                units = units.Where((u) => ( u.Movement + u.ReserveMove > 0 )).ToList();
            if (fire)
                units.Sort(Tile.UnitDamageComparison);

            if (units.Count == 0 && !fire)
                if (this.cbxForce.Checked)
                {
                    return false;
                }
                else
                {
                    this.cbxForce.Checked = true;
                    return SetupStuff(tile, moveTo, fire, max);
                }

            int[] counts = new int[types.Capacity];

            for (int a = 0 ; a < types.Capacity ; a++)
            {
                types.Add(new List<Unit>());
                counts[a] = 0;
            }

            foreach (Unit u in units)
            {
                switch (u.Type)
                {
                case UnitType.Archer:
                    types[1].Add(u);
                    if (u.Healed)
                    {
                        counts[1]++;
                        move.Add(u);
                    }
                    break;

                case UnitType.Daemon:
                    types[3].Add(u);
                    if (u.Healed)
                    {
                        counts[3]++;
                        move.Add(u);
                    }
                    break;

                case UnitType.Infantry:
                    types[0].Add(u);
                    if (u.Healed)
                    {
                        counts[0]++;
                        move.Add(u);
                    }
                    break;

                case UnitType.Knight:
                    types[2].Add(u);
                    if (u.Healed)
                    {
                        counts[2]++;
                        move.Add(u);
                    }
                    break;

                default:
                    throw new Exception("");
                }
            }

            Label[] pics = new Label[4];
            pics[0] = this.lblUnit1;
            pics[1] = this.lblUnit2;
            pics[2] = this.lblUnit3;
            pics[3] = this.lblUnit4;

            Label[] infos = new Label[4];
            infos[0] = this.lblInf1;
            infos[1] = this.lblInf2;
            infos[2] = this.lblInf3;
            infos[3] = this.lblInf4;

            int num = 0;

            for (int a = 0 ; a < types.Count ; a++)
                if (types[a].Count > 0)
                {
                    infos[num].Text = string.Format("{0}({1})", types[a].Count, counts[a]);
                    pics[num++].Image = types[a][0].GetPic();
                }

            for (int a = types.Count - 1 ; a >= 0 ; a--)
                if (types[a].Count <= 0)
                    types.RemoveAt(a);

            foreach (Control c in Controls)
                if (c.Tag is int)
                    c.Visible = ( (int)c.Tag != num );

            this.Height = 462;
            for (int a = num ; a < 4 ; a++)
                this.Height -= 90;

            RefreshSection();

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (fire)
                Unit.Fire(move, moveTo);
            else
                foreach (Unit u in move)
                    u.Move(moveTo);
        }

        private void btnNone_Click(object sender, EventArgs e)
        {
            List<Unit> units = types[(int)( (Control)sender ).Tag];
            foreach (Unit u in units)
                if (move.Contains(u))
                    move.Remove(u);
            RefreshSection((int)( (Control)sender ).Tag);
        }

        private void btnMax_Click(object sender, EventArgs e)
        {
            List<Unit> units = types[(int)( (Control)sender ).Tag];
            foreach (Unit u in units)
                if (!move.Contains(u))
                    move.Add(u);
            RefreshSection((int)( (Control)sender ).Tag);
        }

        private void btnHealed_Click(object sender, EventArgs e)
        {
            List<Unit> units = types[(int)( (Control)sender ).Tag];
            foreach (Unit u in units)
            {
                if (move.Contains(u))
                    move.Remove(u);
                if (u.Healed)
                    move.Add(u);
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
            if (fire)
                while (move.Count > max)
                    move.RemoveAt(move.Count - 1);

            if (section < types.Count)
            {
                List<Unit> units = types[section];
                int count = 0;
                foreach (Unit u in units)
                    if (move.Contains(u))
                        count++;
                foreach (Control c in Controls)
                    if (c is TextBox && (int)c.Tag == section)
                    {
                        ( (TextBox)c ).Text = count.ToString();
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
        public static DialogResult ShowDialog(Tile tile, Tile moveTo, bool fire, int max)
        {
            events = false;
            form.cbxForce.Visible = !fire;
            form.cbxForce.Checked = false;
            events = true;

            if (form.SetupStuff(tile, moveTo, fire, max))
                return form.ShowDialog();
            else
                return System.Windows.Forms.DialogResult.Cancel;
        }

        private void cbxForce_CheckedChanged(object sender, EventArgs e)
        {
            if (events)
                form.SetupStuff(tile, moveTo, fire, max);
        }
    }
}