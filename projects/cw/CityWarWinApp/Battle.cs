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
        private Unit _selected = null, mouseOver = null;
        private UnitInfo unitInfo = null;

        private HashSet<Unit> hideUnits = new HashSet<Unit>();
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

        private void RefreshSelected()
        {
            if (selected != null)
            {
                //lbAttacks.ClearSelected();
                lbAttacks.Items.Clear();
                ValidAttacks(selected, delegate(Attack attack)
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

        public Battle(CityWar.Battle battle)
        {
            this.battle = battle;

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

            MouseWheel += new MouseEventHandler(Battle_MouseWheel);
            lbAttacks.MouseWheel += new MouseEventHandler(listBox_MouseWheel);
            lbAtt.MouseWheel += new MouseEventHandler(listBox_MouseWheel);
            lbDef.MouseWheel += new MouseEventHandler(listBox_MouseWheel);
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

            Attack retalliation = GetSelectedRetalliation();

            if (attacker)
            {
                if (selected == unit)
                    result.Add(PiecesPanel.DrawFlags.Frame);

                if (retalliation != null && retalliation.CanAttack(unit))
                    result.Add(PiecesPanel.DrawFlags.Background);
            }
            else
            {
                if (retalliation != null && retalliation.Owner == unit)
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

            int unused = 0, total = 0;
            HashSet<Attack> attacks;
            validAttacks.TryGetValue(unit, out attacks);
            foreach (Attack attack in unit.Attacks)
            {
                bool used = attack.Used, valid = ( attacks != null && attacks.Contains(attack) );
                if (!used && valid)
                    ++unused;
                if (used || valid)
                    ++total;
            }
            string left = string.Format("{0} / {1}", unused, total);
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
                if (selected != null && mouseOver != null && attack != null && attack.CanAttack(mouseOver))
                {
                    double killPct, avgRelic;
                    double avgDamage = attack.GetAverageDamage(mouseOver, out killPct, out avgRelic);
                    txtTarget.Text = mouseOver.Name;
                    txtArmor.Text = mouseOver.Armor.ToString();
                    txtTargDmg.Text = string.Format("{0}({1})", avgDamage.ToString("0.00"), attack.GetMinDamage(mouseOver));
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
                if (clicked != null && attack != null && Map.CheckAircraft(attack.Owner, 1))
                {
                    int oldHits = clicked.Hits;
                    int damage = Map.game.AttackUnit(battle, attack, clicked);
                    if (damage > -1)
                        Log.LogAttack(attack.Owner, attack, clicked, damage, oldHits);

                    if (CheckUnits())
                        return;

                    RefreshSelected();
                    panelAttackers.Invalidate();
                    panelDefenders.Invalidate();
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

        private bool CheckUnits()
        {
            ClearTarget();

            Unit[] attackers = battle.GetAttackers();
            Unit[] defenders = battle.GetDefenders();

            bool doAttacks = ( validAttacks == null );
            if (doAttacks)
                validAttacks = new Dictionary<Unit, HashSet<Attack>>();

            bool anyHave = false, selectedHas = false;

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
                                anyHave = true;
                                if (attacker == selected)
                                    selectedHas = true;
                                hideUnits.Remove(attacker);
                                hideUnits.Remove(defender);
                                if (!doAttacks)
                                    break;
                            }
                            AddValidAttack(doAttacks, attacker, attAtt);
                        }

            foreach (Unit attacker in attackers)
            {
                int minLength = attacker.Length;
                ValidAttacks(attacker, delegate(Attack attack)
                {
                    minLength = Math.Min(minLength, attack.Length);
                });
                foreach (Unit defender in defenders)
                    foreach (Attack defAtt in defender.Attacks)
                        if (defAtt.CanAttack(attacker, minLength))
                        {
                            if (battle.CanRetalliate)
                            {
                                hideUnits.Remove(attacker);
                                hideUnits.Remove(defender);
                                if (!doAttacks)
                                    break;
                            }
                            AddValidAttack(doAttacks, defender, defAtt);
                        }
            }

            if (!anyHave)
            {
                btnEnd_Click(null, null);
                return true;
            }

            if (!selectedHas)
                foreach (Unit attacker in attackers)
                    ValidAttacks(attacker, delegate(Attack attack)
                    {
                        bool unused = !attack.Used;
                        if (unused)
                            selected = attacker;
                        return unused;
                    });
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
            ValidAttacks(unit, delegate(Attack attack)
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

        private void btnEnd_Click(object sender, EventArgs e)
        {
            if (Map.game.EndBattle(battle))
            {
                validAttacks = null;
                if (CheckUnits())
                    return;

                cbAttAll.Checked = true;
                RefreshLBAtt();
                lbDef.ClearSelected();

                cbAttAll.Visible = false;
                cbDefAll.Visible = false;
                lbDef.Visible = false;

                RefreshSelected();
                panelAttackers.Invalidate();
                panelDefenders.Invalidate();
            }
            else
            {
                Close();
            }
        }

        private void lbAttacks_SelectedIndexChanged(object sender, EventArgs e)
        {
            Attack attack = GetSelectedAttack();
            if (attack != null)
            {
                txtAP.Text = attack.ArmorPiercing.ToString();
                txtDam.Text = attack.Damage.ToString("0.0");
                txtTargets.Text = attack.GetTargetString();
                txtLength.Text = attack.Length.ToString();

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
            }
        }

        private void RefreshLBDef()
        {
            object selectedItem = lbDef.SelectedItem;
            //lbDef.ClearSelected();
            lbDef.Items.Clear();

            Unit attacker = selected;
            Attack attack = GetSelectedAttack();

            foreach (Unit defender in battle.GetDefenders())
                ValidAttacks(defender, delegate(Attack retalliation)
                {
                    if (cbDefAll.Checked || ShowAttack(retalliation, attacker, attack))
                        lbDef.Items.Add(retalliation);
                });

            lbDef.SelectedItem = selectedItem;
        }
        private void RefreshLBAtt()
        {
            object selectedItem = lbAtt.SelectedItem;
            //lbAtt.ClearSelected();
            lbAtt.Items.Clear();

            Attack retalliation = GetSelectedRetalliation();
            if (retalliation != null || cbAttAll.Checked)
                foreach (Unit attacker in battle.GetAttackers())
                    ValidAttacks(attacker, delegate(Attack attack)
                    {
                        if (!attack.Used && ( cbAttAll.Checked || ShowAttack(retalliation, attacker, attack) ))
                            lbAtt.Items.Add(attack);
                    });

            lbAtt.SelectedItem = selectedItem;
        }
        private static bool ShowAttack(Attack retalliation, Unit attacker, Attack attack)
        {
            return ( !retalliation.CanAttack(attacker) && retalliation.CanAttack(attacker, attack.Length) );
        }

        private Attack GetSelectedAttack()
        {
            return lbAttacks.SelectedItem as Attack;
        }
        private Attack GetSelectedRetalliation()
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
            panelAttackers.Invalidate();
        }

        private void sbDefenders_Scroll(object sender, ScrollEventArgs e)
        {
            panelDefenders.Invalidate();
        }

        private void Battle_Load(object sender, EventArgs e)
        {
            CheckUnits();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            new Log().ShowDialog();
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {

        }
    }
}
