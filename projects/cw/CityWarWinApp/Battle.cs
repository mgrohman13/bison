using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    partial class Battle : Form
    {
        CityWar.Battle b;
        Unit _selected = null;
        UnitInfo InfoForm;

        List<Unit> useless = new List<Unit>();
        Dictionary<Unit, int> usedAttacks = new Dictionary<Unit, int>();

        private Unit selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                RefreshSelected();
            }
        }

        private void RefreshSelected()
        {
            if (selected != null)
            {
                this.lbAttacks.Items.Clear();
                foreach (Attack a in selected.Attacks)
                    if (!a.Used && ValidateAttack(a))
                        this.lbAttacks.Items.Add(a);
                if (lbAttacks.Items.Count > 0)
                    this.lbAttacks.SelectedIndex = 0;
                else
                    selected = null;
            }

            ClearTarget();
        }

        public Battle(CityWar.Battle b)
        {
            InitializeComponent();
            panelAttackers.Initialize(
                delegate()
                {
                    return GetPieces(true);
                },
                delegate(Piece piece)
                {
                    return GetFlags(piece, true);
                },
                GetAttackerText,
                delegate()
                {
                    return Brushes.DarkRed;
                }
            );
            panelDefenders.Initialize(
                delegate()
                {
                    return GetPieces(false);
                },
                delegate(Piece piece)
                {
                    return GetFlags(piece, false);
                },
                GetDefenderText,
                delegate()
                {
                    return Brushes.DarkBlue;
                }
            );
            this.MouseWheel += new MouseEventHandler(Battle_MouseWheel);
            this.lbAttacks.MouseWheel += new MouseEventHandler(listBox_MouseWheel);

            this.b = b;

            foreach (Unit u in b.GetAttackers())
                if (!useless.Contains(u))
                {
                    selected = u;
                    break;
                }
        }

        void listBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ListBox box = (ListBox)sender;
            Battle_MouseWheel(this, new MouseEventArgs(e.Button, e.Clicks,
                e.X + box.Location.X, e.Y + box.Location.Y, e.Delta));
        }

        void Battle_MouseWheel(object sender, MouseEventArgs e)
        {
            if (( e.X > Width / 2 && panelDefenders.ScrollBarEnabled ) || !panelAttackers.ScrollBarEnabled)
                panelDefenders.PiecesPanel_MouseWheel(sender, e);
            else
                panelAttackers.PiecesPanel_MouseWheel(sender, e);
        }

        private Piece[] GetPieces(bool attacker)
        {
            List<Piece> result = new List<Piece>();
            if (attacker)
            {
                foreach (Unit u in b.GetAttackers())
                    if (!useless.Contains(u))
                        result.Add(u);
            }
            else
            {
                foreach (Unit u in b.GetDefenders())
                    result.Add(u);
            }
            return result.ToArray();
        }

        private MattUtil.EnumFlags<PiecesPanel.DrawFlags> GetFlags(Piece piece, bool attacker)
        {
            MattUtil.EnumFlags<PiecesPanel.DrawFlags> result = new MattUtil.EnumFlags<PiecesPanel.DrawFlags>(PiecesPanel.DrawFlags.Text);
            if (attacker)
            {
                if (selected == piece)
                    result.Add(PiecesPanel.DrawFlags.Frame);
            }
            else
            {
                if (( (Attack)lbAttacks.SelectedItem ).CanAttack((Unit)piece))
                    result.Add(PiecesPanel.DrawFlags.Background);
            }
            return result;
        }

        private string GetAttackerText(Piece piece)
        {
            Unit unit = (Unit)piece;
            int validAttacks = ValidAttacks(unit);
            return string.Format("{0} / {1}", validAttacks,
                validAttacks + ( usedAttacks.ContainsKey(unit) ? usedAttacks[unit] : 0 ));
        }

        private string GetDefenderText(Piece piece)
        {
            Unit unit = (Unit)piece;
            return string.Format("{1}", unit.Hits, unit.Armor);
        }

        private void panelAttackers_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Unit clicked = panelAttackers.GetClickedPiece(e) as Unit;
                if (clicked != null)
                    selected = clicked;
            }
            else if (e.Button == MouseButtons.Right)
                try
                {
                    InfoForm.Close();
                }
                catch
                {
                }

            ClearTarget();
            Refresh();
        }

        private void panelDefenders_MouseLeave(object sender, EventArgs e)
        {
            ClearTarget();
        }

        private void panelDefenders_MouseMove(object sender, MouseEventArgs e)
        {
            bool clear = true;
            if (selected != null)
            {
                Unit enemy = panelDefenders.GetClickedPiece(e) as Unit;
                if (enemy != null)
                {
                    Attack attack = ( (Attack)this.lbAttacks.SelectedItem );

                    if (attack.CanAttack(enemy))
                    {
                        double killPct, avgRelic;
                        double avgDamage = attack.GetAverageDamage(enemy, out killPct, out avgRelic);
                        this.txtTarget.Text = enemy.Name;
                        this.txtArmor.Text = enemy.Armor.ToString();
                        this.txtTargDmg.Text = string.Format("{0}({1})", avgDamage.ToString("0.00"), attack.GetMinDamage(enemy));
                        this.txtChance.Text = killPct.ToString("0") + "%";
                        this.txtRelic.Text = avgRelic.ToString("0.00");
                        clear = false;
                    }
                }
            }
            if (clear)
                ClearTarget();
        }

        private void ClearTarget()
        {
            this.txtTarget.Clear();
            this.txtArmor.Clear();
            this.txtTargDmg.Clear();
            this.txtChance.Clear();
            this.txtRelic.Clear();
        }

        private void PanelDefender_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selected != null)
                {
                    Unit clicked = (Unit)panelDefenders.GetClickedPiece(e);
                    if (clicked != null)
                    {
                        if (Map.CheckAircraft(( (Attack)this.lbAttacks.SelectedItem ).Owner, 1))
                        {
                            int damage = Map.game.AttackUnit(b, ( (Attack)this.lbAttacks.SelectedItem ), clicked);
                            if (damage > -1)
                                if (usedAttacks.ContainsKey(selected))
                                    ++usedAttacks[selected];
                                else
                                    usedAttacks.Add(selected, 1);
                        }

                        if (CheckUnits())
                            return;

                        RefreshSelected();
                        Refresh();
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
                try
                {
                    InfoForm.Close();
                }
                catch
                {
                }
        }

        private bool CheckUnits()
        {
            ClearTarget();

            List<Unit> temp = new List<Unit>(b.GetDefenders());
            foreach (Unit def in temp)
            {
                bool needed = false;
                foreach (Unit att in b.GetAttackers())
                    if (needed)
                        break;
                    else
                        foreach (Attack attack in att.Attacks)
                            if (attack.CanAttack(def))
                            {
                                needed = true;
                                break;
                            }

                if (!needed)
                    useless.Add(def);
            }

            if (b.GetDefenders().Length == 0)
            {
                btnEnd_Click(null, null);
                return true;
            }

            bool any = false;
            foreach (Unit u in b.GetAttackers())
                if (!useless.Contains(u))
                    if (ValidAttacks(u) == 0)
                        useless.Add(u);
                    else
                        any = true;

            if (!any)
            {
                btnEnd_Click(null, null);
                return true;
            }

            if (useless.Contains(selected) || selected == null)
                foreach (Unit u in b.GetAttackers())
                    if (!useless.Contains(u))
                    {
                        selected = u;
                        break;
                    }

            return false;
        }

        private int ValidAttacks(Unit u)
        {
            int res = 0;
            foreach (Attack a in u.Attacks)
                if (ValidateAttack(a))
                    ++res;
            return res;
        }

        private bool ValidateAttack(Attack a)
        {
            foreach (Unit u in b.GetDefenders())
                if (a.CanAttack(u))
                    return true;
            return false;
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            if (Map.game.EndBattle(b))
            {
                useless.Clear();

                if (CheckUnits())
                    return;

                //pause p = new pause((( Unit)b.defenders[0]).Owner);
                //p.ShowDialog();

                selected = null;
                foreach (Unit u in b.GetAttackers())
                    if (!useless.Contains(u))
                    {
                        selected = u;
                        break;
                    }
                //if(selected==null)
                //    btnEnd_Click(null, null);
                RefreshSelected();
                Refresh();

                //Point ml = panelAttackers.Location;
                //Point sl = sbAttackers.Location;
                //panelAttackers.Location = new Point(PanelDefender.Location.X, PanelDefender.Location.Y);
                //sbAttackers.Location = new Point(sbDefenders.Location.X, sbDefenders.Location.Y);
                //PanelDefender.Location = new Point(ml.X, ml.Y);
                //sbDefenders.Location = new Point(sl.X, sl.Y);
            }
            else
            {
                this.Close();
            }
        }

        private void lbAttacks_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.txtAP.Text = ( (Attack)this.lbAttacks.SelectedItem ).ArmorPiercing.ToString();
            this.txtDam.Text = ( (Attack)this.lbAttacks.SelectedItem ).Damage.ToString("0.0");
            this.txtTargets.Text = ( (Attack)this.lbAttacks.SelectedItem ).GetTargetString();
            this.txtLength.Text = ( (Attack)this.lbAttacks.SelectedItem ).Length.ToString();

            ClearTarget();
            panelDefenders.Refresh();
        }

        private void panelAttackers_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Piece clicked = panelAttackers.GetClickedPiece(e);
                if (clicked != null)
                {
                    InfoForm = new UnitInfo(clicked, panelAttackers.PointToScreen(e.Location),
                        b.CanRetalliate ? clicked.Movement - ( usedAttacks.ContainsKey((Unit)clicked) ? 1 : 0 ) : -1);
                    InfoForm.Show();
                }
            }
        }

        private void panelDefenders_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Piece clicked = panelDefenders.GetClickedPiece(e);
                if (clicked != null)
                {
                    InfoForm = new UnitInfo(clicked, panelDefenders.PointToScreen(e.Location),
                        b.CanRetalliate ? -1 : clicked.Movement - 1);
                    InfoForm.Show();
                }
            }
        }

        private void sbAttackers_Scroll(object sender, ScrollEventArgs e)
        {
            this.panelAttackers.Refresh();
        }

        private void sbDefenders_Scroll(object sender, ScrollEventArgs e)
        {
            this.panelDefenders.Refresh();
        }

        private void Battle_Load(object sender, EventArgs e)
        {
            CheckUnits();
        }
    }
}
