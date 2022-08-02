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
    partial class Battle : Form, IComparer<Attack>
    {
        private Map main;
        private UnitInfo unitInfo = null;

        private CityWar.Battle battle;

        private Unit _selected = null, mouseOver = null;

        private HashSet<Unit> hideUnits = new HashSet<Unit>();
        private HashSet<Attack> hideAttacks = new HashSet<Attack>();
        private Dictionary<Unit, HashSet<Attack>> validAttacks = null;
        private Dictionary<Unit, int> totalCounts = null;

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
                    panelAttackers.ScrollToSelected(false);
                }
            }
        }

        private void RefreshSelected()
        {
            lbAttacks.Items.Clear();
            if (selected != null)
            {
                //lbAttacks.ClearSelected();
                ValidAttacks(selected, attack =>
                {
                    if (!attack.Used)
                        lbAttacks.Items.Add(attack);
                });
                if (lbAttacks.Items.Count > 0)
                    lbAttacks.SelectedIndex = 0;
                else
                    selected = null;
            }

            ClearTarget();
            panelAttackers.Invalidate();
            panelDefenders.Invalidate();
        }

        public Battle(Map main, CityWar.Battle battle)
        {
            this.main = main;
            this.battle = battle;

            InitializeComponent();

            panelAttackers.Initialize(() => GetPieces(true), piece => GetFlags(piece, true), GetAttackerText, () => Brushes.DarkRed);
            panelDefenders.Initialize(() => GetPieces(false), piece => GetFlags(piece, false), GetDefenderText, () => Brushes.DarkBlue);

            MouseWheel += new MouseEventHandler(Battle_MouseWheel);
            lbAttacks.MouseWheel += new MouseEventHandler(listBox_MouseWheel);
            lbAtt.MouseWheel += new MouseEventHandler(listBox_MouseWheel);
            lbDef.MouseWheel += new MouseEventHandler(listBox_MouseWheel);
        }

        public static void StartBattle(Map map, CityWar.Battle battle)
        {
            Battle f1 = new(map, battle);
            f1.ShowDialog();
            f1.Dispose();

            while (Map.Game.EndBattle(battle))
            {
                Battle f2 = new(map, battle);
                f2.StartRetaliation();
                if (!f2.IsDisposed)
                    f2.ShowDialog();
                f2.Dispose();
            }

            Log.Flush();
        }

        private void listBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ListBox box = (ListBox)sender;
            if (!box.HorizontalScrollbar)
                Battle_MouseWheel(this, new MouseEventArgs(e.Button, e.Clicks,
                        e.X + box.Location.X, e.Y + box.Location.Y, e.Delta));
        }

        private void Battle_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((e.X > Width / 2 && panelDefenders.ScrollBarEnabled) || !panelAttackers.ScrollBarEnabled)
                panelDefenders.PiecesPanel_MouseWheel(sender, e);
            else
                panelAttackers.PiecesPanel_MouseWheel(sender, e);
        }

        private Piece[] GetPieces(bool attacker)
        {
            List<Piece> result = new List<Piece>();
            foreach (Unit u in (attacker ? battle.GetAttackers() : battle.GetDefenders()))
                if (!hideUnits.Contains(u))
                    result.Add(u);
            return result.ToArray();
        }

        private MattUtil.EnumFlags<PiecesPanel.DrawFlags> GetFlags(Piece piece, bool attacker)
        {
            Unit unit = (Unit)piece;
            MattUtil.EnumFlags<PiecesPanel.DrawFlags> result = new MattUtil.EnumFlags<PiecesPanel.DrawFlags>(PiecesPanel.DrawFlags.Text);

            Attack retaliation = GetSelectedRetaliation();

            if (attacker)
            {
                if (selected == unit)
                    result.Add(PiecesPanel.DrawFlags.Frame);

                if (retaliation != null && retaliation.CanAttack(unit))
                    result.Add(PiecesPanel.DrawFlags.Background);

                bool unused = false;
                ValidAttacks(unit, attack =>
                {
                    unused = !attack.Used;
                    return unused;
                });
                if (unused)
                    result.Add(PiecesPanel.DrawFlags.Bold);
            }
            else
            {
                if (retaliation != null && retaliation.Owner == unit)
                    result.Add(PiecesPanel.DrawFlags.Frame);

                Attack selectedAttack = GetSelectedAttack();
                if (selectedAttack != null && selectedAttack.CanAttack(unit))
                    result.Add(PiecesPanel.DrawFlags.Background);
            }
            return result;
        }

        private Tuple<string, string> GetAttackerText(Piece piece)
        {
            Unit unit = (Unit)piece;

            int unused = 0, total = totalCounts[unit];
            ValidAttacks(unit, attack =>
            {
                if (!attack.Used)
                    ++unused;
            });

            string left = string.Format("{0} / {1}", unused, total);
            string right = (unit.Length != int.MinValue && unit.Length != int.MaxValue ? unit.Length.ToString() : null);
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
        }

        private void panelDefenders_MouseLeave(object sender, EventArgs e)
        {
            ClearTarget();
        }

        private void panelDefenders_MouseMove(object sender, MouseEventArgs e)
        {
            Unit newMoused = panelDefenders.GetClickedPiece(e) as Unit;
            if (newMoused != mouseOver)
            {
                mouseOver = newMoused;
                Attack attack = GetSelectedAttack();
                if (selected != null && newMoused != null && attack != null && attack.CanAttack(newMoused))
                {
                    double killPct, avgRelic;
                    double avgDamage = attack.GetAverageDamage(newMoused, out killPct, out avgRelic);
                    newMoused.GetCost(out double gc);
                    txtTarget.Text = newMoused.ToString();
                    txtArmor.Text = newMoused.Armor.ToString();
                    txtGC.Text = gc.ToString("0.0");
                    txtTargDmg.Text = string.Format("{0}({1})", avgDamage.ToString("0.00"), attack.GetMinDamage(newMoused));
                    txtChance.Text = killPct.ToString("0") + "%";
                    txtRelic.Text = avgRelic.ToString("0.00");
                }
                else
                {
                    ClearTarget();
                }
            }
        }
        private void ClearTarget()
        {
            mouseOver = null;
            txtTarget.Clear();
            txtArmor.Clear();
            txtGC.Clear();
            txtTargDmg.Clear();
            txtChance.Clear();
            txtRelic.Clear();
        }

        private void panelDefenders_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && selected != null)
            {
                Unit clicked = panelDefenders.GetClickedPiece(e) as Unit;
                Attack attack = GetSelectedAttack();
                if (clicked != null && attack != null && Map.CheckAircraft(attack.Owner, 1, 1))
                {
                    Unit prev = ((Attack)lbAttacks.SelectedItem).Owner;
                    int idx = lbAtt.SelectedIndex;

                    int oldHits = clicked.Hits;
                    int damage = Map.Game.AttackUnit(battle, attack, clicked, out double relic, out Tuple<Unit, int, int, double> splash);
                    if (damage > -1)
                        Log.LogAttack(attack.Owner, attack, clicked, damage, oldHits, relic);
                    if (splash != null)
                        Log.LogAttack(attack.Owner, attack, splash.Item1, splash.Item2, splash.Item3, splash.Item4);

                    if (clicked.Dead)
                        validAttacks = null;
                    if (CheckUnits())
                        return;

                    RefreshAll(prev, idx);
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

        private void RefreshAll(Unit prev, int idx)
        {
            RefreshLBAtt();
            RefreshLBDef();
            RefreshSelected();
            panelAttackers.Invalidate();
            panelDefenders.Invalidate();
            main.RefreshResources();

            if (prev != ((Attack)lbAttacks.SelectedItem).Owner)
            {
                while (idx >= lbAtt.Items.Count)
                    idx--;
                if (idx > -1)
                    lbAtt.SelectedIndex = idx;
            }
        }

        private bool CheckUnits(bool doAttacks = false)
        {
            ClearTarget();

            Unit[] attackers = battle.GetAttackers();
            Unit[] defenders = battle.GetDefenders();

            doAttacks |= (validAttacks == null);
            if (doAttacks)
                validAttacks = new Dictionary<Unit, HashSet<Attack>>();
            bool doCounts = (doAttacks || totalCounts == null);
            if (doCounts)
                totalCounts = new Dictionary<Unit, int>();

            bool anyHave = false, selectedHas = false;

            hideUnits.Clear();
            hideUnits.UnionWith(attackers);
            hideUnits.UnionWith(defenders);

            foreach (Unit attacker in attackers)
                foreach (Attack attAtt in attacker.Attacks)
                    if (cbHidden.Checked || !hideAttacks.Contains(attAtt))
                    {
                        bool validAttack = false;
                        foreach (Unit defender in defenders)
                            if (attAtt.CanAttack(defender))
                            {
                                validAttack = true;
                                if (!attAtt.Used)
                                {
                                    anyHave = true;
                                    if (attacker == selected)
                                        selectedHas = true;
                                    hideUnits.Remove(attacker);
                                    hideUnits.Remove(defender);
                                }
                            }
                        if (validAttack)
                        {
                            AddValidAttack(doAttacks, attacker, attAtt);
                            if (doCounts)
                            {
                                int count;
                                totalCounts.TryGetValue(attacker, out count);
                                totalCounts[attacker] = count + 1;
                            }
                        }
                    }

            if (!anyHave)
            {
                btnEnd_Click(null, null);
                return true;
            }

            foreach (Unit attacker in attackers)
            {
                int minLength = attacker.Length;
                ValidAttacks(attacker, attack =>
                {
                    minLength = Math.Min(minLength, attack.Length);
                });
                foreach (Unit defender in defenders)
                    foreach (Attack defAtt in defender.Attacks)
                        if (defAtt.CanAttack(attacker, minLength))
                        {
                            if (battle.CanRetaliate)
                            {
                                hideUnits.Remove(attacker);
                                hideUnits.Remove(defender);
                            }
                            AddValidAttack(doAttacks, defender, defAtt);
                        }
            }

            if (!selectedHas)
            {
                selected = null;
                foreach (Unit attacker in attackers)
                {
                    ValidAttacks(attacker, attack =>
                    {
                        bool unused = !attack.Used;
                        if (unused)
                            selected = attacker;
                        return unused;
                    });
                    if (selected != null)
                        break;
                }
            }
            lbAttacks.SelectedIndex = 0;

            return false;
        }
        private void AddValidAttack(bool doAttacks, Unit unit, Attack attack)
        {
            if (doAttacks)
            {
                HashSet<Attack> attacks;
                if (!validAttacks.TryGetValue(unit, out attacks))
                    validAttacks[unit] = attacks = new HashSet<Attack>();
                attacks.Add(attack);
            }
        }
        private void ValidAttacks(Unit unit, Action<Attack> Action)
        {
            ValidAttacks(unit, attack =>
            {
                Action(attack);
                return false;
            });
        }
        private void ValidAttacks(Unit unit, Func<Attack, bool> Func)
        {
            HashSet<Attack> attacks;
            if (validAttacks.TryGetValue(unit, out attacks))
                foreach (Attack validAttack in attacks)
                    if (Func(validAttack))
                        return;
        }

        private void StartRetaliation()
        {
            validAttacks = null;
            totalCounts = null;
            if (CheckUnits())
                return;

            cbAttAll.Checked = true;
            RefreshLBAtt();
            lbDef.ClearSelected();

            lblCost.Visible = true;
            txtCost.Visible = true;

            cbAttAll.Visible = false;
            cbDefAll.Visible = false;
            lbDef.Visible = false;

            btnHide.Visible = false;
            cbHidden.Visible = false;

            RefreshSelected();
            panelAttackers.Invalidate();
            panelDefenders.Invalidate();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lbAttacks_SelectedIndexChanged(object sender, EventArgs e)
        {
            Attack attack = GetSelectedAttack();
            if (attack != null)
            {
                lbAtt.SelectedItem = attack;

                txtAP.Text = attack.Pierce.ToString();
                txtDam.Text = attack.Damage.ToString();
                txtTargets.Text = attack.GetTargetString();
                txtLength.Text = attack.Length.ToString();
                txtSpecial.Text = attack.Special == Attack.SpecialType.None ? "" : attack.Special.ToString();
                txtCost.Text = attack.RetaliateCost.ToString("0.0");

                if (!cbDefAll.Checked)
                    RefreshLBDef();
                ClearTarget();
                panelDefenders.Invalidate();
            }
        }

        private void cbDefAll_CheckedChanged(object sender, EventArgs e)
        {
            RefreshLBDef();
        }
        private void cbAttAll_CheckedChanged(object sender, EventArgs e)
        {
            RefreshLBAtt();
        }

        private void lbDef_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!cbAttAll.Checked)
                RefreshLBAtt();
            panelAttackers.Invalidate();
            panelDefenders.Invalidate();
        }
        private void lbAtt_SelectedIndexChanged(object sender, EventArgs e)
        {
            Attack attack = GetSelectedLBAtt();
            if (attack != null)
            {
                selected = attack.Owner;
                lbAttacks.SelectedItem = attack;

                btnHide.Text = hideAttacks.Contains(attack) ? "Show" : "Hide";
            }
        }

        private void RefreshLBDef()
        {
            SortedSet<Attack> showAttacks = new SortedSet<Attack>(this);

            Unit attacker = selected;
            Attack attack = GetSelectedAttack();
            if (attacker != null && attack != null)
                foreach (Unit defender in battle.GetDefenders())
                    ValidAttacks(defender, retaliation =>
                    {
                        if (cbDefAll.Checked || ShowAttack(retaliation, attacker, attack))
                            showAttacks.Add(retaliation);
                    });

            SetItems(lbDef, showAttacks);
        }
        private void RefreshLBAtt()
        {
            SortedSet<Attack> showAttacks = new SortedSet<Attack>(this);

            Attack retaliation = GetSelectedRetaliation();
            if (retaliation != null || cbAttAll.Checked)
                foreach (Unit attacker in battle.GetAttackers())
                    ValidAttacks(attacker, attack =>
                    {
                        if (!attack.Used && (cbAttAll.Checked || ShowAttack(retaliation, attacker, attack)))
                            showAttacks.Add(attack);
                    });

            SetItems(lbAtt, showAttacks);
        }
        private static bool ShowAttack(Attack retaliation, Unit attacker, Attack attack)
        {
            return (!retaliation.CanAttack(attacker) && retaliation.CanAttack(attacker, attack.Length));
        }
        private static void SetItems(ListBox listBox, SortedSet<Attack> showAttacks)
        {
            Attack selectedItem = listBox.SelectedItem as Attack;
            if (!showAttacks.Contains(selectedItem))
                selectedItem = null;

            listBox.Items.Clear();
            foreach (Attack add in showAttacks)
                listBox.Items.Add(add);

            if (listBox.SelectedItem != selectedItem)
                listBox.SelectedItem = selectedItem;
        }

        private Attack GetSelectedAttack()
        {
            return lbAttacks.SelectedItem as Attack;
        }
        private Attack GetSelectedRetaliation()
        {
            return lbDef.SelectedItem as Attack;
        }
        private Attack GetSelectedLBAtt()
        {
            return lbAtt.SelectedItem as Attack;
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
                            battle.CanRetaliate ? clicked.Movement - (used ? 1 : 0) : -1);
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
                            battle.CanRetaliate ? -1 : clicked.Movement - 1);
                    unitInfo.Show();
                }
            }
        }

        private void sbAttackers_Scroll(object sender, ScrollEventArgs e)
        {
            panelAttackers.Invalidate();
        }

        private void sbDefenders_Scroll(object sender, ScrollEventArgs e)
        {
            panelDefenders.Invalidate();
        }

        private void Battle_Load(object sender, EventArgs e)
        {
            CheckUnits();
            RefreshLBAtt();
            lbAtt.SelectedIndex = 0;
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            new Log().ShowDialog();
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            Calculator.ShowForm(battle);
        }

        private void lb_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            Point point = lb.PointToClient(Cursor.Position);
            int index = lb.IndexFromPoint(point);
            if (index < 0) return;

            //
        }

        private void lb_MouseLeave(object sender, EventArgs e)
        {
            // 
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            if (lbAttacks.SelectedItem is Attack attack)
            {
                Unit prev = ((Attack)lbAttacks.SelectedItem).Owner;
                int idx = lbAtt.SelectedIndex;

                if (!hideAttacks.Remove(attack))
                    hideAttacks.Add(attack);
                if (CheckUnits(true))
                    return;

                RefreshAll(prev, idx);
            }
        }

        private void cbHidden_CheckedChanged(object sender, EventArgs e)
        {
            Unit prev = ((Attack)lbAttacks.SelectedItem).Owner;
            int idx = lbAtt.SelectedIndex;

            if (CheckUnits(true))
                return;

            RefreshAll(prev, idx);
        }

        #region IComparer<Attack> Members

        int IComparer<Attack>.Compare(Attack x, Attack y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;
                return 1;
            }
            if (y == null)
                return -1;

            int retVal = (y.Length - x.Length);
            if (retVal == 0)
            {
                retVal = (y.Pierce - x.Pierce);
                if (retVal == 0)
                {
                    retVal = (y.Damage - x.Damage);
                    if (retVal == 0)
                    {
                        retVal = CityWar.Battle.CompareUnits(x.Owner, y.Owner);
                        if (retVal == 0)
                        {
                            if (x.Owner != y.Owner)
                                throw new Exception();
                            retVal = (Array.IndexOf(x.Owner.Attacks, x) - Array.IndexOf(y.Owner.Attacks, y));
                        }
                    }
                }
            }
            return retVal;
        }

        #endregion
    }
}
