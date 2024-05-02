using ClassLibrary1;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace WinFormsApp1
{
    public partial class Info : UserControl
    {
        //private readonly Timer animateTimer;
        public Tile Selected { get; set; }

        public Info()
        {
            InitializeComponent();
            rtbLog.GotFocus += RtbLog_GotFocus;

            //animateTimer = new();
            //animateTimer.Interval = 52;
            //bool dir = true;
            //animateTimer.Tick += (sender, args) =>
            //{
            //    lock (animateTimer)
            //    {
            //        const double step = 16.9;
            //        Color target = dir ? SystemColors.ControlDarkDark : SystemColors.ControlLightLight;
            //        int r = btnBuild.BackColor.R, g = btnBuild.BackColor.G, b = btnBuild.BackColor.B;
            //        int rt = target.R, gt = target.G, bt = target.B;
            //        static int Step(int c, int t)
            //        {
            //            int sign = Math.Sign(t - c);
            //            c += Game.Rand.GaussianInt(sign * step, .13);
            //            if (sign != Math.Sign(t - c))
            //                c = t;
            //            return Math.Max(Math.Min(c, 255), 0);
            //        }
            //        r = Step(r, rt);
            //        g = Step(g, gt);
            //        b = Step(b, bt);
            //        target = Color.FromArgb(r, g, b);
            //        if (btnBuild.BackColor == target)
            //            dir = !dir;
            //        else
            //            btnBuild.BackColor = target;
            //    }
            //};
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Program.EndTurn();
        }

        public override void Refresh()
        {
            logPreventScroll = true;
            UnFocusLog();

            ShowAll(false);
            lblTurn.Text = Program.Game.Turn.ToString();

            //lock (animateTimer)
            //{
            //    animateTimer.Stop();
            //    btnBuild.BackColor = Color.Transparent;
            //}
            if (DgvForm.CanBuild(Selected))
            {
                btnBuild.Text = "Build";
                btnBuild.Show();
            }
            else if (HasUpgrade() || HasConstructorUpgrade())
            {
                btnBuild.Text = "Upgrade";
                btnBuild.Show();
                //animateTimer.Start();
            }
            else if (CanReplace(out _))
            {
                btnBuild.Text = "Replace";
                btnBuild.Show();
            }
            else
            {
                btnBuild.Hide();
            }

            this.btnTrade.Visible = Program.Game.Player.CanBurnMass() || Program.Game.Player.CanFabricateMass() || Program.Game.Player.CanScrapResearch();

            if (Selected != null && Selected.Piece != null)
            {
                lblHeading.Show();
                lblHeading.Text = Selected.Piece.ToString();

                PlayerPiece playerPiece = Selected.Piece as PlayerPiece;

                if (Selected.Piece.HasBehavior(out IKillable killable))
                {
                    double repairInc = 0;
                    if (playerPiece != null)
                        repairInc = playerPiece.GetRepairInc();

                    lbl1.Show();
                    lblInf1.Show();
                    lbl1.Text = "Defense";

                    lblInf1.Text = $"{killable.Hits.DefenseCur}"
                        + (killable.Hits.DefenseCur < killable.Hits.DefenseMax ? $"/{killable.Hits.DefenseMax}" : "")
                        + (repairInc > 0 ? $"+{FormatUsuallyInt(repairInc)}" : "");

                    Defense armor = killable.Protection.Where(d => d.Type == DefenseType.Armor).FirstOrDefault();
                    if (armor != null)
                    {
                        lbl2.Show();
                        lblInf2.Show();
                        lbl2.Text = "Armor";
                        lblInf2.Text = $"{armor.DefenseCur}"
                            + (armor.DefenseCur < armor.DefenseMax ? $"/{armor.DefenseMax}" : "")
                            + (armor.GetRegen() > 0 ? $"+{FormatUsuallyInt(armor.GetRegen())}" : "");
                    }
                    Defense shield = killable.Protection.Where(d => d.Type == DefenseType.Shield).FirstOrDefault();
                    if (shield != null)
                    {
                        lbl3.Show();
                        lblInf3.Show();
                        lbl3.Text = "Shields";
                        lblInf3.Text = $"{shield.DefenseCur}"
                            + (shield.DefenseCur < shield.DefenseMax ? $"/{shield.DefenseMax}" : "")
                            + (shield.GetRegen() > 0 ? $"+{shield.GetRegen()}" : "");
                    }

                    lbl4.Show();
                    lblInf4.Show();
                    lbl4.Text = killable.Hits.DefenseCur < killable.Hits.DefenseMax ? "Efficiency" : "Resilience";
                    lblInf4.Text = string.Format("{0}{1}",
                        FormatPct(killable.Hits.DefenseCur < killable.Hits.DefenseMax ? Consts.GetDamagedValue(killable.Piece, 1, 0) : killable.Resilience),
                        killable.Hits.DefenseCur < killable.Hits.DefenseMax ? string.Format(" ({0})", FormatPct(killable.Resilience)) : "");
                }
                if (Selected.Piece.HasBehavior(out IMovable movable))
                {
                    lbl5.Show();
                    lblInf5.Show();
                    lbl5.Text = "Movement";
                    lblInf5.Text = string.Format("{0} / {1} / {2} +{3}{4}",
                            Format(movable.MoveCur), (movable.MoveMax), (movable.MoveLimit), Format(movable.GetInc()),
                            CheckBase(movable.MoveIncBase, movable.GetInc()));
                }

                if (playerPiece != null)
                {
                    lbl6.Show();
                    lblInf6.Show();
                    lbl6.Text = "Vision";
                    lblInf6.Text = string.Format("{0}{1}", FormatDown(playerPiece.Vision), CheckBase(playerPiece.VisionBase, playerPiece.Vision, FormatDown));

                    if (playerPiece is not Extractor)
                    {
                        double energyInc, massInc, researchInc;
                        energyInc = massInc = researchInc = 0;
                        playerPiece.GetIncome(ref energyInc, ref massInc, ref researchInc);
                        if (energyInc != 0)
                        {
                            lbl7.Show();
                            lblInf7.Show();
                            lbl7.Text = "Energy";
                            lblInf7.Text = FormatInc(energyInc, Consts.CoreEnergy);
                        }
                        if (massInc != 0)
                        {
                            lbl8.Show();
                            lblInf8.Show();
                            lbl8.Text = "Mass";
                            lblInf8.Text = FormatInc(massInc, Consts.CoreMass);
                        }
                        if (researchInc != 0)
                        {
                            lbl9.Show();
                            lblInf9.Show();
                            lbl9.Text = "Research";
                            lblInf9.Text = FormatInc(researchInc, Consts.CoreResearch);
                        }
                        string FormatInc(double inc, double coreValue) =>
                            string.Format("{1}{0}{2}", Format(inc), inc < 0 ? "" : "+", playerPiece is Core core ? $" ({FormatPct(inc / coreValue)})" : "");
                    }
                }
                //if (Selected.Piece is Alien alien)
                //{
                //    lbl6.Show();
                //    lblInf6.Show();
                //    lbl6.Text = "Behavior";
                //    lblInf6.Text = alien.State.ToString();

                //    lbl7.Show();
                //    lblInf7.Show();
                //    lbl7.Text = "Morale";
                //    lblInf7.Text = FormatPct(alien.Morale);
                //}

                var builder = Selected.Piece.GetBehavior<IBuilder>();
                var repair = Selected.Piece.GetBehavior<IRepair>();
                if (repair != null)
                {
                    lbl10.Text = "Repair";
                    lblInf10.Text = string.Format("+{0}{1}", FormatUsuallyInt(repair.Rate), CheckBase(repair.RateBase, repair.Rate, v => FormatUsuallyInt(v.Value)));
                }
                else
                {
                    lbl10.Text = null;
                    lblInf10.Text = null;
                }
                if (builder != null)
                {
                    lbl10.Show();
                    lblInf10.Show();
                    bool noRepair = (lbl10.Text == null || lbl10.Text.Trim().Length == 0);
                    if (noRepair)
                        lbl10.Text = "Range";
                    lblInf10.Text = string.Format(noRepair ? "{0}{1}" : "{2}, {0}{1}",
                        Format(builder.Range), CheckBase(builder.RangeBase, builder.Range), lblInf10.Text);
                }

                Resource resource = Selected.Piece as Resource;
                Extractor extractor = Selected.Piece as Extractor;
                if (resource == null && extractor != null)
                    resource = extractor.Resource;
                if (resource != null)
                {
                    if (extractor == null)
                    {
                        Extractor.Cost(out int energy, out int mass, resource);
                        lbl2.Show();
                        lblInf2.Show();
                        lbl2.Text = "Build Cost";
                        lblInf2.Text = string.Format("{0} : {1}", (energy), (mass));
                    }

                    double energyInc, massInc, researchInc;
                    energyInc = massInc = researchInc = 0;
                    if (extractor == null)
                        resource.GenerateResources(ref energyInc, ref massInc, ref researchInc);
                    else
                        extractor.GetIncome(ref energyInc, ref massInc, ref researchInc);

                    if (energyInc != 0)
                    {
                        lbl7.Show();
                        lblInf7.Show();
                        lbl7.Text = "Energy";
                        lblInf7.Text = string.Format("{1}{0}{2}", Format(energyInc), energyInc > 0 ? "+" : "", CheckBase(resource as Biomass, energyInc));
                    }
                    if (massInc != 0)
                    {
                        lbl8.Show();
                        lblInf8.Show();
                        lbl8.Text = "Mass";
                        lblInf8.Text = string.Format("{1}{0}{2}", Format(massInc), massInc > 0 ? "+" : "", CheckBase(resource as Metal, massInc));
                    }
                    if (researchInc != 0)
                    {
                        lbl9.Show();
                        lblInf9.Show();
                        lbl9.Text = "Research";
                        lblInf9.Text = string.Format("{1}{0}{2}", Format(researchInc), researchInc > 0 ? "+" : "", CheckBase(resource as Artifact, researchInc));
                    }

                    double sustain = extractor?.Sustain ?? resource.Sustain;
                    lbl10.Show();
                    lblInf10.Show();
                    lbl10.Text = "Sustainability";
                    lblInf10.Text = string.Format("{0}{1}", FormatPct(sustain), CheckBase(resource.Sustain, sustain, FormatPct));
                }

                if (Selected.Piece.HasBehavior(out IAttacker attacker))
                {
                    dgvAttacks.Show();

                    //int idx = 0;
                    dgvAttacks.DataSource = attacker.Attacks.OrderByDescending(a => a.Range).Select(a => new
                    {
                        Online = a.CanAttack().ToString(),
                        a.Type,
                        Range = a.Range > Attack.MELEE_RANGE ?
                            $"{a.Range:0.0}" + (a.Range.ToString("0.0") != a.RangeBase.ToString("0.0") ? $" / {a.RangeBase:0.0}" : "") : "Melee",
                        Attack = $"{a.AttackCur}" + (a.AttackCur < a.AttackMax ? $" / {a.AttackMax}" : ""),
                        Reload = $"+{FormatUsuallyInt(a.Reload)}" + (FormatUsuallyInt(a.Reload) != a.ReloadBase.ToString() ? $" / {a.ReloadBase}" : ""),
                    }).ToList();
                    dgvAttacks.Columns["Online"].Visible = attacker.Attacks.Any(a => !a.CanAttack());
                    dgvAttacks.Columns["Type"].Visible = attacker.Attacks.Any(a => a.Type != AttackType.Kinetic);
                    dgvAttacks.Columns["Range"].Visible = attacker.Attacks.Any(a => a.Range > Attack.MELEE_RANGE);
                    //dgvAttacks.Columns["Rounds"].DefaultCellStyle.Format = "0.0";

                    //foreach (DataGridViewColumn c in dgvAttacks.Columns)
                    //    c.Visible = false;

                    //dgvAttacks.Columns["Type"].Visible = true;
                    //dgvAttacks.Columns["AttackCur"].Visible = true;
                    //dgvAttacks.Columns["AttackMax"].Visible = true;
                    //dgvAttacks.Columns["Range"].Visible = true;
                    //dgvAttacks.Columns["Rounds"].Visible = true;
                    //dgvAttacks.Columns["Range"].DefaultCellStyle.Format = "0.0";


                    //dgvAttacks.Columns["Upkeep"].Visible = false;

                    //dgvAttacks.Columns["Range"].DisplayIndex = idx++;
                    //dgvAttacks.Columns["Range"].HeaderText = "RANGE";
                    //dgvAttacks.Columns["Range"].DefaultCellStyle.Format = "0.0";

                    ////if (attacker.Attacks.Any(a => Format(a.Range) != Format(a.RangeBase)))
                    ////{
                    ////    dgvAttacks.Columns["RangeBase"].Visible = true;
                    ////    dgvAttacks.Columns["RangeBase"].DisplayIndex = idx++;
                    ////    dgvAttacks.Columns["RangeBase"].HeaderText = "(base)";
                    ////    dgvAttacks.Columns["RangeBase"].DefaultCellStyle.Format = "0.0";
                    ////}
                    ////else
                    //dgvAttacks.Columns["RangeBase"].Visible = false;

                    //dgvAttacks.Columns["Damage"].DisplayIndex = idx++;
                    //dgvAttacks.Columns["Damage"].HeaderText = "DMG";
                    //dgvAttacks.Columns["Damage"].DefaultCellStyle.Format = attacker.Attacks.Any(a => a.Damage < a.DamageBase) ? "0.0" : "0";

                    ////if (attacker.Attacks.Any(a => Format(a.Damage) != Format(a.DamageBase)))
                    ////{
                    ////    dgvAttacks.Columns["DamageBase"].Visible = true;
                    ////    dgvAttacks.Columns["DamageBase"].DisplayIndex = idx++;
                    ////    dgvAttacks.Columns["DamageBase"].HeaderText = "(base)";
                    ////    dgvAttacks.Columns["DamageBase"].DefaultCellStyle.Format = "0.0";
                    ////}
                    ////else
                    //dgvAttacks.Columns["DamageBase"].Visible = false;

                    //dgvAttacks.Columns["ArmorPierce"].Visible = attacker.Attacks.Any(a => a.ArmorPierce > 0);
                    //dgvAttacks.Columns["ArmorPierce"].DisplayIndex = idx++;
                    //dgvAttacks.Columns["ArmorPierce"].HeaderText = "AP";
                    //dgvAttacks.Columns["ArmorPierce"].DefaultCellStyle.Format = "P0";
                    //dgvAttacks.Columns["ShieldPierce"].Visible = attacker.Attacks.Any(a => a.ShieldPierce > 0);
                    //dgvAttacks.Columns["ShieldPierce"].DisplayIndex = idx++;
                    //dgvAttacks.Columns["ShieldPierce"].HeaderText = "SP";
                    //dgvAttacks.Columns["ShieldPierce"].DefaultCellStyle.Format = "P0";

                    //dgvAttacks.Columns["Dev"].DisplayIndex = idx++;
                    //dgvAttacks.Columns["Dev"].HeaderText = "RNDM";
                    //dgvAttacks.Columns["Dev"].DefaultCellStyle.Format = "P0";
                    //dgvAttacks.Columns["Attacked"].DisplayIndex = idx++;
                    //dgvAttacks.Columns["Attacked"].HeaderText = "USED";

                    int labelsY = this.Controls.OfType<Label>().Where(lbl => lbl.Visible && lbl.Parent != this.panel1).Max(lbl => lbl.Location.Y + lbl.Height);
                    dgvAttacks.MaximumSize = new Size(this.Width, this.panel1.Location.Y - labelsY);
                    dgvAttacks.Size = dgvAttacks.PreferredSize;
                    dgvAttacks.Location = new Point(0, this.panel1.Location.Y - dgvAttacks.Height);
                }
            }

            RefreshLog();

            base.Refresh();

            EnableLogScroll();
        }
        public static string FormatUsuallyInt(double value)
        {
            const string decimals = ".0";
            string format = value.ToString("0" + decimals);
            if (format.EndsWith(decimals))
                format = value.ToString("0");
            return format;
        }

        private static string CheckBase(double orig, double actual)
        {
            return CheckBase(orig, actual, Format);
        }
        private static string CheckBase(double orig, double actual, Func<double?, string> Formatter)
        {
            string origDisp = Formatter(orig);
            return (origDisp != Formatter(actual)) ? " (" + origDisp + ")" : "";
        }
        private static string CheckBase(Resource resource, double energyInc)
        {
            return resource == null ? "" : CheckBase(resource.Value, energyInc);
        }

        public static string Format(double? value)
        {
            if (value.HasValue)
                return value.Value.ToString("0.0");
            return "";
        }
        public static string FormatPct(double? value)
        {
            return FormatPct(value, false);
        }
        public static string FormatPct(double? value, bool digit)
        {
            if (value.HasValue)
                return value.Value.ToString(digit ? "P1" : "P0");
            return "";
        }
        public static string FormatDown(double? value)
        {
            if (value.HasValue)
                return Format(Math.Floor(value.Value * 10) / 10.0);
            return "";
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

        #region Log
        // current log state to support lazy loading on scroll - necessary because the log gets long and RTF text printing is slow
        private bool logPreventScroll, logEnd;
        private int logPage;
        private Log.LogEntry logPrevious;

        private void Info_SizeChanged(object sender, EventArgs e)
        {
            RefreshLog();
        }
        private void RtbLog_VScroll(object sender, EventArgs e)
        {
            // all locking, focusing, and the logPreventScroll flag are to prevent log state from getting mixed up and showing inaccurate info
            UnFocusLog();
            if (!logPreventScroll)
                lock (rtbLog)
                    if (!logPreventScroll)
                    {
                        // append the next page to the current log
                        logPage++;
                        Log();
                    }
        }

        private void RtbLog_GotFocus(object sender, EventArgs e)
        {
            logPreventScroll = true;
            UnFocusLog();
            EnableLogScroll();
        }
        private void RTBLog_MouseClick(object sender, MouseEventArgs e)
        {
            SelectLog(rtbLog.SelectionStart);
        }

        private void RefreshLog()
        {
            //Debug.WriteLine("RefreshLog");
            logPreventScroll = true;
            UnFocusLog();
            lock (rtbLog)
            {
                rtbLog.Height = (dgvAttacks.Visible ? dgvAttacks.Location.Y : panel1.Location.Y) - rtbLog.Location.Y;
                rtbLog.Width = this.ClientSize.Width;

                logEnd = false;
                logPage = 0;
                logPrevious = null;

                rtbLog.ResetText();
                Log();

                rtbLog.Select(0, 0);
            }
            EnableLogScroll();
        }
        private void Log()
        {
            if (Program.Game == null)
                return;

            logPreventScroll = true;
            UnFocusLog();
            lock (rtbLog)
                if (!logEnd)
                {
                    if (logPage > 0)
                    {
                        // fix a weird RichTextBox bug where the color won't show up correctly on the first statement of the next line
                        rtbLog.SelectionColor = Color.White;
                        rtbLog.AppendText(" ");
                        rtbLog.SelectionColor = Color.Black;
                    }

                    logEnd = true;
                    int count = 0;
                    int limit = rtbLog.Height / 30 + 1;
                    int skip = limit * logPage;
                    limit += skip;
                    foreach (var entry in Program.Game.Log.Data(Selected?.Piece))
                    {
                        count++;
                        if (count <= skip)
                            continue;
                        if (count > limit)
                            break;
                        logEnd = false;

                        if (count > 1)
                            rtbLog.AppendText(Environment.NewLine);

                        // turn banner
                        if (entry.Turn != logPrevious?.Turn && entry.Turn != Program.Game.Turn)
                        {
                            void HalfLine()
                            {
                                rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Strikeout);
                                rtbLog.AppendText("".PadLeft(39, ' '));
                                rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);
                                rtbLog.AppendText(" ");
                            };
                            rtbLog.AppendText(Environment.NewLine);
                            HalfLine();
                            rtbLog.AppendText(entry.Turn + " ");
                            HalfLine();
                            rtbLog.AppendText(Environment.NewLine + Environment.NewLine);
                        }

                        // pieces involved
                        void LogPiece(Side side, string name, string type)
                        {
                            if (name != null)
                            {
                                rtbLog.SelectionColor = side == null ? Color.Black : side == side.Game.Player ? Color.Green : Color.Red;
                                rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
                                rtbLog.AppendText(name);
                                rtbLog.SelectionColor = Color.Black;
                                rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);
                                rtbLog.AppendText(type);
                            }
                        }
                        LogPiece(entry.AttackerSide, entry.AttackerName, entry.AttackerType);
                        rtbLog.AppendText(" : ");
                        LogPiece(entry.DefenderSide, entry.DefenderName, entry.DefenderType);
                        rtbLog.AppendText($"  ~  {entry.Attack.Prev} : {FormatDef(s => s.Prev)}");

                        if (entry.Killed)
                        {
                            rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
                            rtbLog.AppendText("  Killed!");
                            rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);
                        }
                        // always leaving a trailing space fixes another weird RichTextBox bug
                        rtbLog.AppendText(Environment.NewLine + "  ");

                        rtbLog.SelectionColor = Color.Purple;
                        rtbLog.AppendText($"{FormatDmg(entry.Attack)}");
                        rtbLog.SelectionColor = Color.Black;
                        rtbLog.AppendText($" : ");
                        rtbLog.SelectionColor = Color.Purple;
                        rtbLog.AppendText($"{FormatDef(FormatDmg)}");
                        rtbLog.SelectionColor = Color.Black;
                        rtbLog.AppendText($"  ->  ");
                        rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
                        rtbLog.AppendText($"{entry.Attack.Cur}");
                        rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);
                        rtbLog.AppendText($"/{entry.Attack.Max}");
                        rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
                        rtbLog.AppendText($" : {FormatDef(s => s.Cur)}");
                        rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);
                        rtbLog.AppendText($"/{FormatDef(s => s.Max)}");

                        ////// damage breakdown
                        ////if (entry.HitsDmg > 0)
                        ////    rtbLog.AppendText(string.Format("{0} -{1} = ", entry.HitsCur + entry.HitsDmg, entry.HitsDmg));
                        ////rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
                        ////rtbLog.AppendText(entry.HitsCur.ToString());
                        ////rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);
                        ////if (entry.ShieldDmg > 0)
                        ////{
                        ////    rtbLog.SelectionColor = Color.Blue;
                        ////    rtbLog.AppendText(string.Format(" ; {0:0.0} -{1:0.0} = ", FormatInt(entry.ShieldCur + entry.ShieldDmg), FormatInt(entry.ShieldDmg)));
                        ////    rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
                        ////    rtbLog.AppendText(FormatInt(entry.ShieldCur));
                        ////    rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);
                        ////    rtbLog.SelectionColor = Color.Black;
                        ////}
                        ////rtbLog.AppendText(" ~ " + FormatInt(entry.RandDmg));
                        //static string FormatResult(int dmg) => (dmg > 0 ? "-" : "") + dmg;
                        //static string FormatDmg(int cur, int max, int dmg)
                        //{
                        //    if (dmg > 0)
                        //        cur += dmg;
                        //    return $"{cur}" + (cur < max ? $"/{max}" : "") + (dmg > 0 ? $" -{dmg}" : "");
                        //}
                        //static string FormatFinal(int cur, int dmg) => (dmg > 0 ? cur + dmg : cur).ToString();
                        //rtbLog.AppendText($"{FormatDmg(entry.attCur, entry.attMax, entry.dmgNeg)} : {FormatDmg(entry.defCur, entry.defMax, entry.dmgPos)}"
                        //    + $"  ~  {FormatResult(entry.dmgNeg)} : {FormatResult(entry.dmgPos)}");
                        ////rtbLog.AppendText();

                        string FormatDmg(Log.LogEntry.Stat stat) => $"{stat.Cur - stat.Prev}";
                        string FormatDef(Func<Log.LogEntry.Stat, object> GetVal) =>
                            string.Join(",", entry.Defense.Select(GetVal).Select(v => v.ToString()));

                        logPrevious = entry;
                    }
                }
            EnableLogScroll();

        }
        //private static string FormatInt(double v)
        //{
        //    //since damage values are frequently integers, only show extra digit when necessary
        //    string result = v.ToString("0.0");
        //    if (result.EndsWith(".0"))
        //        result = v.ToString("0");
        //    return result;
        //}

        private void SelectLog(int position)
        {
            logPreventScroll = true;
            UnFocusLog();
            lock (rtbLog)
                if (position > 0 && position < rtbLog.Text.Length)
                {
                    // find the line that contains piece names for this entry
                    int a = Math.Max(rtbLog.Text.LastIndexOf("\n", position), 0);
                    int b = rtbLog.Text.IndexOf("\n", rtbLog.SelectionStart);
                    if (a >= 0 && a < rtbLog.Text.Length && b >= 0 && b < rtbLog.Text.Length)
                    {
                        string line = rtbLog.Text[a..b];
                        int c = line.IndexOf("~");
                        if (c >= 0 && c < line.Length)
                        {
                            if (line.Contains(':'))
                            {
                                line = line[..c].Trim();
                                // pick the friendly piece if it is still alive, else the enemy
                                Piece select = Program.Game.Player.Pieces.Concat(Program.Game.Enemy.VisiblePieces)
                                    .FirstOrDefault(p => line.StartsWith(p.ToString()) || line.EndsWith(p.ToString()));
                                // select the piece if it is already on screen, otherwise just center it
                                if (select != null && !Program.Form.MapMain.Center(select.Tile))
                                    Program.Form.MapMain.SelTile = select.Tile;
                            }
                            else
                            {
                                SelectLog(a - 1);
                            }
                        }
                    }
                }
            EnableLogScroll();
        }

        private void UnFocusLog()
        {
            // we never want the log to get focus (or it can automatically scroll while being written to)
            //   so focus something else that is always present and doesn't do anything when focused
            this.lblTurn.Focus();
        }
        private bool waiting, triggered;
        private void EnableLogScroll()
        {
            bool DoEnable()
            {
                if (!waiting && triggered && logPreventScroll)
                    lock (rtbLog)
                        if (!waiting && triggered && logPreventScroll)
                            return true;
                return false;
            }
            triggered = true;
            if (DoEnable())
                new Thread(() =>
                {
                    if (DoEnable())
                    {
                        do
                        {
                            waiting = true;
                            triggered = false;
                            // prevent scrolling until form event queue empties
                            Thread.Sleep(169);
                        } while (DoEnable());
                        logPreventScroll = false;
                        waiting = false;
                        //Debug.WriteLine("EnableLogScroll");
                    }
                }).Start();
        }

        #endregion Log

        public void BtnBuild_Click(object sender, EventArgs e)
        {
            static string DispCost(int c) => (c < 0 ? "+" : "") + -c;
            if (DgvForm.CanBuild(Selected))
            {
                Piece result = Program.DgvForm.BuilderDialog(Selected);
                if (result != null)
                    Program.RefreshChanged();
            }
            else if (HasConstructorUpgrade())
            {
                MessageBox.Show("Constructor can be upgraded.", "Upgrade", MessageBoxButtons.OK);
            }
            else if (HasUpgrade(Selected, out MechBlueprint blueprint, out int energy, out int mass))
            {
                Program.DgvForm.UpgradeInfo(((Mech)Selected.Piece).Blueprint);
                bool canUpgrade = CanUpgrade();
                if (canUpgrade && MessageBox.Show(string.Format("{3}pgrade to {0} for {1} energy {2} mass{4}",
                        blueprint, DispCost(energy), DispCost(mass),
                        canUpgrade ? "U" : "Can u", canUpgrade ? "?" : " ."),
                        "Upgrade", canUpgrade ? MessageBoxButtons.YesNo : MessageBoxButtons.OK)
                    == DialogResult.Yes)
                {
                    ((Mech)Selected.Piece).Upgrade();
                    Program.RefreshChanged();
                }
            }
            else if (CanReplace(out Piece builder))
            {
                static bool Replace(string type, ReplaceFunc ReplaceFunc)
                {
                    if (ReplaceFunc(false, out int energy, out int mass)
                        && MessageBox.Show(string.Format("Replace with {0} for {1} energy {2} mass?",
                                type, DispCost(energy), DispCost(mass)), "Replace", MessageBoxButtons.YesNo)
                            == DialogResult.Yes)
                    {
                        if (ReplaceFunc(true, out _, out _))
                        {
                            Program.RefreshChanged();
                            return true;
                        }
                    }
                    return false;
                }
                if (builder.HasBehavior(out IBuilder.IBuildExtractor buildExtractor) && Selected.Piece is Extractor extractor)
                {
                    Replace("Extractor", (bool doReplace, out int energy, out int mass) =>
                        buildExtractor.Replace(doReplace, extractor, out energy, out mass));
                }
                else if (Selected.Piece is FoundationPiece foundationPiece)
                {
                    IBuilder.IBuildFactory buildFactory = builder.GetBehavior<IBuilder.IBuildFactory>();
                    IBuilder.IBuildTurret buildTurret = builder.GetBehavior<IBuilder.IBuildTurret>();
                    bool done = false;
                    for (int a = 0; a < 2 && !done; a++)
                    {
                        if ((a == 0) == (Selected.Piece is Turret))
                            done = buildFactory != null && Replace("Factory", (bool doReplace, out int energy, out int mass) =>
                                buildFactory.Replace(doReplace, foundationPiece, out energy, out mass));
                        else
                            done = buildTurret != null && Replace("Turret", (bool doReplace, out int energy, out int mass) =>
                                buildTurret.Replace(doReplace, foundationPiece, out energy, out mass));
                    }
                }
            }
        }
        private delegate bool ReplaceFunc(bool doReplace, out int energy, out int mass);
        public static bool HasAnyUpgrade(Tile tile)
        {
            return HasUpgrade(tile, out _, out _, out _) || HasConstructorUpgrade(tile);
        }
        public bool HasConstructorUpgrade()
        {
            return HasConstructorUpgrade(Selected);
        }
        public static bool HasConstructorUpgrade(Tile tile)
        {
            return tile != null && tile.Piece is Constructor constructor && constructor.CanUpgrade;
        }
        public bool HasUpgrade()
        {
            return HasUpgrade(Selected, out _, out _, out _);
        }
        private static bool HasUpgrade(Tile tile, out MechBlueprint blueprint, out int energy, out int mass)
        {
            if (tile != null && tile.Piece is Mech mech)
            {
                mech.CanUpgrade(out blueprint, out energy, out mass);
                return blueprint != null;
            }
            blueprint = null;
            energy = mass = 0;
            return false;
        }
        private bool CanUpgrade()
        {
            if (Selected != null && Selected.Piece is Mech mech)
                return mech.CanUpgrade(out _, out _, out _);
            return false;
        }
        private bool CanReplace(out Piece piece)
        {
            IBuilder builder = null;
            if (Selected != null)
            {
                if (Selected.Piece is Extractor)
                {
                    IBuilder.IBuildExtractor buildExtractor = DgvForm.GetBuilder<IBuilder.IBuildExtractor>(Selected);
                    builder = buildExtractor;
                }
                if (Selected.Piece is FoundationPiece)
                {
                    IBuilder.IBuildFactory buildFactory = DgvForm.GetBuilder<IBuilder.IBuildFactory>(Selected);
                    IBuilder.IBuildTurret buildTurret = DgvForm.GetBuilder<IBuilder.IBuildTurret>(Selected);
                    builder = (IBuilder)buildFactory ?? buildTurret;
                }
            }
            piece = builder?.Piece;
            return builder != null;
        }

        public void BtnViewAtt_Click(object sender, EventArgs e)
        {
            Program.Form.MapMain.ToggleViewAttacks();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Program.SaveGame();
        }

        private void BtnResearch_Click(object sender, EventArgs e)
        {
            if (ResearchForm.ShowForm())
                Program.RefreshChanged();
        }

        private void BtnTrade_Click(object sender, EventArgs e)
        {
            if (Trade.ShowTrade())
                Program.RefreshChanged();
        }

        private void BtnInfo_Click(object sender, EventArgs e)
        {
            Details.ShowForm();
        }
    }
}
