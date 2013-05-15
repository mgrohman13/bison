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
        private Unit _selected = null, _mouseOver = null;
        private UnitInfo unitInfo = null;

        private HashSet<Unit> hideUnits = new HashSet<Unit>(), attackLeft = new HashSet<Unit>();
        private Dictionary<Unit, HashSet<Attack>> validAttacks = null;

        private Unit selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    RefreshSelected();
                }
            }
        }
        private Unit mouseOver
        {
            get
            {
                return _mouseOver;
            }
            set
            {
                if (_mouseOver != value)
                {
                    _mouseOver = value;
                    RefreshMouseOver();
                }
            }
        }

        private void RefreshSelected()
        {
            if (selected != null)
            {
                lbAttacks.Items.Clear();
                foreach (Attack a in validAttacks[selected])
                    lbAttacks.Items.Add(a);
                if (lbAttacks.Items.Count > 0)
                    lbAttacks.SelectedIndex = 0;
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
            foreach (Unit u in ( attacker ? battle.GetAttackers() : battle.GetDefenders() ))
                if (!hideUnits.Contains(u))
                    result.Add(u);
            return result.ToArray();
        }

        private MattUtil.EnumFlags<PiecesPanel.DrawFlags> GetFlags(Piece piece, bool attacker)
        {
            Unit unit = (Unit)piece;
            MattUtil.EnumFlags<PiecesPanel.DrawFlags> result = new MattUtil.EnumFlags<PiecesPanel.DrawFlags>(PiecesPanel.DrawFlags.Text);
            if (attacker)
            {
                if (selected == unit)
                    result.Add(PiecesPanel.DrawFlags.Frame);
                if (mouseOver == null)
                {
                    if (battle.CanRetalliate && attackLeft.Contains(unit))
                        result.Add(PiecesPanel.DrawFlags.Background);
                }
                else
                {
                    foreach (Attack attack in mouseOver.Attacks)
                        if (attack.CanAttack(unit))
                            result.Add(PiecesPanel.DrawFlags.Background);
                }
            }
            else
            {
                Attack selectedAttack = GetSelectedAttack();
                if (selectedAttack != null && selectedAttack.CanAttack(unit))
                    result.Add(PiecesPanel.DrawFlags.Background);
            }
            return result;
        }

        private Tuple<string, string> GetAttackerText(Piece piece)
        {
            Unit unit = (Unit)piece;
            HashSet<Attack> attacks = validAttacks[unit];
            int unused = 0;
            foreach (Attack a in attacks)
                if (!a.Used)
                    ++unused;
            string left = string.Format("{0} / {1}", unused, attacks.Count);
            string right = ( unit.Length != int.MinValue && unit.Length != int.MaxValue ? unit.Length.ToString() : null );
            return new Tuple<string, string>(left, right);
        }

        private Tuple<string, string> GetDefenderText(Piece piece)
        {
            Unit unit = (Unit)piece;
            return new Tuple<string, string>(unit.Hits.ToString(), unit.Armor.ToString());
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
            mouseOver = panelDefenders.GetClickedPiece(e) as Unit;
        }

        private void RefreshMouseOver()
        {
            this.panelAttackers.Refresh();

            Attack attack = GetSelectedAttack();
            if (selected != null && mouseOver != null && attack != null && attack.CanAttack(mouseOver))
            {
                double killPct, avgRelic;
                double avgDamage = attack.GetAverageDamage(mouseOver, out killPct, out avgRelic);
                this.txtTarget.Text = mouseOver.Name;
                this.txtArmor.Text = mouseOver.Armor.ToString();
                this.txtTargDmg.Text = string.Format("{0}({1})", avgDamage.ToString("0.00"), attack.GetMinDamage(mouseOver));
                this.txtChance.Text = killPct.ToString("0") + "%";
                this.txtRelic.Text = avgRelic.ToString("0.00");
            }
            else
            {
                ClearTarget(false);
            }
        }
        private void ClearTarget()
        {
            ClearTarget(true);
        }
        private void ClearTarget(bool clearMouseOver)
        {
            if (clearMouseOver)
                mouseOver = null;
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
                        LogAttack(attack.Owner, attack, clicked, damage, oldHits);

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
            log = string.Format("{6} {0}, {1} -> {7} {2} ({3}, {5}) : {4}{8}\r\n", attacker, attack.GetLogString(), defender,
                oldHits, -damage, defender.Armor, attacker.Owner, defender.Owner, defender.Dead ? ", Killed!" : "") + log;
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

            bool doAttacks = ( validAttacks == null );
            if (doAttacks)
                validAttacks = new Dictionary<Unit, HashSet<Attack>>();

            attackLeft.Clear();
            hideUnits.Clear();
            hideUnits.UnionWith(attackers);
            hideUnits.UnionWith(defenders);

            foreach (Unit attacker in attackers)
                foreach (Unit defender in defenders)
                    foreach (Attack attAtt in attacker.Attacks)
                        if (attAtt.CanAttack(defender))
                        {
                            if (!attAtt.Used)
                            {
                                hideUnits.Remove(attacker);
                                hideUnits.Remove(defender);
                                if (!attAtt.Used)
                                {
                                    attackLeft.Add(attacker);
                                    if (!doAttacks)
                                        break;
                                }
                            }
                            if (doAttacks)
                            {
                                HashSet<Attack> attacks;
                                if (!validAttacks.TryGetValue(attacker, out attacks))
                                    validAttacks[attacker] = attacks = new HashSet<Attack>();
                                attacks.Add(attAtt);
                            }
                        }

            if (battle.CanRetalliate)
                foreach (Unit attacker in attackers)
                {
                    int minLength = int.MaxValue;
                    foreach (Attack validAttack in validAttacks[attacker])
                        minLength = Math.Min(minLength, validAttack.Length);
                    foreach (Unit defender in defenders)
                        foreach (Attack defAtt in defender.Attacks)
                            if (defAtt.CanAttack(attacker, minLength))
                            {
                                hideUnits.Remove(attacker);
                                hideUnits.Remove(defender);
                                break;
                            }
                }

            if (attackLeft.Count == 0)
            {
                btnEnd_Click(null, null);
                return true;
            }

            if (selected == null || !attackLeft.Contains(selected))
                foreach (Unit u in attackers)
                    if (!hideUnits.Contains(u))
                        foreach (Attack a in validAttacks[u])
                        {
                            selected = u;
                            goto selectAttack;
                        }
selectAttack:
            lbAttacks.SelectedIndex = 0;

            return false;
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            if (Map.game.EndBattle(battle))
            {
                validAttacks = null;
                if (CheckUnits())
                    return;

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
                Unit clicked = panelAttackers.GetClickedPiece(e) as Unit;
                if (clicked != null)
                {
                    bool used = false;
                    foreach (Attack a in clicked.Attacks)
                        if (a.Used)
                        {
                            used = true;
                            break;
                        }
                    unitInfo = new UnitInfo(clicked, panelAttackers.PointToScreen(e.Location),
                            battle.CanRetalliate ? clicked.Movement - ( used ? 1 : 0 ) : -1);
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
    }
}
