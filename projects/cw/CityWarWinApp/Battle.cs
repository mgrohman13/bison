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
        private CityWar.Battle battle;
        private Unit _selected;
        private UnitInfo unitInfo;

        private HashSet<Unit> noAtt = new HashSet<Unit>(), noDef = new HashSet<Unit>();
        private Dictionary<Unit, int> usedAttacks = new Dictionary<Unit, int>();

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

            this.battle = b;

            foreach (Unit u in b.GetAttackers())
                if (!noAtt.Contains(u))
                {
                    selected = u;
                    break;
                }

            ShowLog();
        }

        private void listBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ListBox box = (ListBox)sender;
            Battle_MouseWheel(this, new MouseEventArgs(e.Button, e.Clicks,
                    e.X + box.Location.X, e.Y + box.Location.Y, e.Delta));
        }

        private void Battle_MouseWheel(object sender, MouseEventArgs e)
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
                foreach (Unit u in battle.GetAttackers())
                    if (!noAtt.Contains(u))
                        result.Add(u);
            }
            else
            {
                foreach (Unit u in battle.GetDefenders())
                {
                    bool show;
                    if (rbTrg.Checked)
                    {
                        if (cbAttack.Checked)
                            show = ( GetSelectedAttack() != null && GetSelectedAttack().CanAttack(u) );
                        else if (cbSelected.Checked)
                            show = CanAttack(selected, u);
                        else
                            show = !noDef.Contains(u);
                    }
                    else if (rbRet.Checked)
                    {
                        if (cbAttack.Checked)
                            show = CouldRetalliate(u);
                        else if (cbSelected.Checked)
                            show = CanAttack(u, selected);
                        else
                            show = !noAtt.Contains(u);
                    }
                    else
                    {
                        show = ( !noDef.Contains(u) || ( !noAtt.Contains(u) && battle.CanRetalliate ) );
                    }
                    if (show)
                        result.Add(u);
                }
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
                Attack selectedAttack = GetSelectedAttack();
                if (selectedAttack != null && selectedAttack.CanAttack((Unit)piece))
                    result.Add(PiecesPanel.DrawFlags.Background);
            }
            return result;
        }

        private string GetAttackerText(Piece piece)
        {
            Unit unit = (Unit)piece;
            int validAttacks = ValidAttacks(unit);
            bool usedAttack = usedAttacks.ContainsKey(unit);
            return string.Format("{0} / {1}{2}", validAttacks,
                    validAttacks + ( usedAttack ? usedAttacks[unit] : 0 ),
                    unit.Length != int.MinValue && unit.Length != int.MaxValue ? string.Format("  ({0})", unit.Length) : "");
        }

        private string GetDefenderText(Piece piece)
        {
            Unit unit = (Unit)piece;
            return unit.Armor.ToString();
        }

        private void panelAttackers_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Unit clicked = panelAttackers.GetClickedPiece(e) as Unit;
                if (clicked != null)
                    selected = clicked;
            }
            else if (e.Button == MouseButtons.Right && unitInfo != null)
            {
                try
                {
                    unitInfo.Close();
                }
                catch
                {
                }
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
                    Attack attack = GetSelectedAttack();
                    if (attack != null && attack.CanAttack(enemy))
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

        private void panelDefenders_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && selected != null)
            {
                Unit clicked = panelDefenders.GetClickedPiece(e) as Unit;
                Attack attack = GetSelectedAttack();
                if (clicked != null && attack != null && Map.CheckAircraft(attack.Owner, 1))
                {
                    int oldHits = clicked.Hits;
                    int damage = Map.game.AttackUnit(battle, attack, clicked);
                    if (damage > -1)
                    {
                        int used;
                        usedAttacks.TryGetValue(attack.Owner, out used);
                        usedAttacks[attack.Owner] = used + 1;

                        LogAttack(attack.Owner, attack, clicked, damage, oldHits);
                    }

                    if (CheckUnits())
                        return;

                    RefreshSelected();
                    Refresh();
                }
            }
            else if (e.Button == MouseButtons.Right && unitInfo != null)
            {
                try
                {
                    unitInfo.Close();
                }
                catch
                {
                }
            }
        }

        private static string log = "";
        private void LogAttack(Unit attacker, Attack attack, Unit defender, int damage, int oldHits)
        {
            log = string.Format("{6} {0} {1} -> {7} {2} ({3},{5}) : {4}{8}\r\n", attacker, attack.GetLogString(), defender,
                oldHits, -damage, defender.Armor, attacker.Owner, defender.Owner, defender.Dead ? " Killed!" : "") + log;
            ShowLog();
        }
        private void ShowLog()
        {
            this.txtLog.Text = log;
        }

        private bool CheckUnits()
        {
            ClearTarget();

            Unit[] attackers = battle.GetAttackers();
            Unit[] defenders = battle.GetDefenders();

            noAtt.Clear();
            noDef.Clear();
            noAtt.UnionWith(attackers);
            noAtt.UnionWith(defenders);
            noDef.UnionWith(noAtt);

            foreach (Unit a in attackers)
                foreach (Unit d in defenders)
                {
                    foreach (Attack aa in a.Attacks)
                        if (aa.CanAttack(d))
                        {
                            noAtt.Remove(a);
                            noDef.Remove(d);
                            break;
                        }
                    foreach (Attack da in d.Attacks)
                        if (da.CanAttack(a))
                        {
                            noAtt.Remove(d);
                            noDef.Remove(a);
                            break;
                        }
                }

            if (noAtt.IsSupersetOf(attackers) || noDef.IsSupersetOf(defenders))
            {
                btnEnd_Click(null, null);
                return true;
            }

            if (selected == null || noAtt.Contains(selected))
                foreach (Unit u in attackers)
                    if (!noAtt.Contains(u))
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
            foreach (Unit u in battle.GetDefenders())
                if (a.CanAttack(u))
                    return true;
            return false;
        }

        private bool CouldRetalliate(Unit u)
        {
            Attack selectedAttack = GetSelectedAttack();
            if (selected != null && selectedAttack != null)
                return CanAttack(u, selected, Math.Min(selected.Length, selectedAttack.Length));
            return false;
        }

        private bool CanAttack(Unit a, Unit d)
        {
            return CanAttack(a, d, d.Length);
        }
        private bool CanAttack(Unit a, Unit d, int length)
        {
            foreach (Attack attack in a.Attacks)
                if (attack.CanAttack(d, length))
                    return true;
            return false;
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            if (Map.game.EndBattle(battle))
            {
                if (CheckUnits())
                    return;

                selected = null;
                foreach (Unit u in battle.GetAttackers())
                    if (!noAtt.Contains(u))
                    {
                        selected = u;
                        break;
                    }

                RefreshSelected();
                Refresh();
            }
            else
            {
                this.Close();
            }
        }

        private void lbAttacks_SelectedIndexChanged(object sender, EventArgs e)
        {
            Attack attack = GetSelectedAttack();
            this.txtAP.Text = attack.ArmorPiercing.ToString();
            this.txtDam.Text = attack.Damage.ToString("0.0");
            this.txtTargets.Text = attack.GetTargetString();
            this.txtLength.Text = attack.Length.ToString();

            ClearTarget();
            panelDefenders.Refresh();
        }

        private Attack GetSelectedAttack()
        {
            Attack selectedAttack = lbAttacks.SelectedItem as Attack;
            return selectedAttack;
        }

        private void panelAttackers_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Piece clicked = panelAttackers.GetClickedPiece(e);
                if (clicked != null)
                {
                    unitInfo = new UnitInfo(clicked, panelAttackers.PointToScreen(e.Location),
                            battle.CanRetalliate ? clicked.Movement - ( usedAttacks.ContainsKey((Unit)clicked) ? 1 : 0 ) : -1);
                    unitInfo.Show();
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
                    unitInfo = new UnitInfo(clicked, panelDefenders.PointToScreen(e.Location),
                            battle.CanRetalliate ? -1 : clicked.Movement - 1);
                    unitInfo.Show();
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

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            panelDefenders.Refresh();

            rbRet.Visible = battle.CanRetalliate;
            if (!rbRet.Visible && rbRet.Checked)
                rbAll.Checked = true;

            cbSelected.Visible = !rbAll.Checked;
            UncheckDisabled(cbSelected);
            cbAttack.Visible = cbSelected.Checked;
            UncheckDisabled(cbAttack);
        }
        private static void UncheckDisabled(CheckBox cb)
        {
            if (!cb.Visible)
                cb.Checked = false;
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {

        }
    }
}
