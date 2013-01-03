using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class CombatForm : Form
    {
        #region Combat

        private static CombatForm form;
        private static CombatForm Form
        {
            get
            {
                if (form == null)
                    form = new CombatForm();
                return form;
            }
        }

        private BackgroundWorker worker;

        private Combatant attacker, defender;
        private bool isConfirmation, showConfirmation;

        private CombatForm()
        {
            InitializeComponent();
            InitRand();
        }

        private void InitRand()
        {
            SetRand(this.nudAttack, false);
            SetRand(this.nudAttHP, true);
            SetRand(this.nudDefense, false);
            SetRand(this.nudDefHP, true);
        }

        private void SetRand(NumericUpDown nud, bool hp)
        {
            double str = ShipDesign.GetAttDefStr(MainForm.Game.CurrentPlayer.GetLastResearched());
            if (hp)
                str = ShipDesign.GetHPStr(ShipDesign.MakeStat(str), ShipDesign.MakeStat(str));
            SetValue(nud, ShipDesign.MakeStat(str));
        }

        private void SetCombatants(Combatant attacker, Combatant defender, bool isConfirmation)
        {
            this.isConfirmation = isConfirmation;
            this.showConfirmation = true;

            this.nudAttack.Visible = false;
            this.nudAttHP.Visible = false;
            this.nudDefense.Visible = false;
            this.nudDefHP.Visible = false;

            this.lblAttPlayer.Visible = true;
            this.lblDefPlayer.Visible = true;

            this.btnEdit.Visible = true;
            this.btnSwap.Visible = true;

            this.attacker = attacker;
            this.defender = defender;

            RefreshShips();
        }

        private void RefreshShips()
        {
            Ship attShip = attacker as Ship;
            Ship defShip = defender as Ship;

            this.btnAttack.Visible = ( isConfirmation ? ( showConfirmation )
                    : ( attShip != null && attShip.CurSpeed > 0 && attacker.Player.IsTurn
                    && !( attShip.DeathStar && defender is Colony ) && Tile.IsNeighbor(attacker.Tile, defender.Tile) ) );

            Form.btnEdit.Visible = !isConfirmation;

            if (attacker == null)
            {
                ShowEdit();
            }
            else
            {
                SetValue(this.nudAttack, attacker.Att);
                SetValue(this.nudAttHP, attacker.HP);
                SetValue(this.nudDefense, defender.Def);
                SetValue(this.nudDefHP, defender.HP);

                this.lblAttack.Text = attacker.Att.ToString();
                this.lblAttHP.Text = attacker.HP.ToString();
                if (attShip != null)
                    this.lblAttHP.Text += " / " + attShip.MaxHP.ToString();
                this.lblDefense.Text = defender.Def.ToString();
                this.lblDefHP.Text = defender.HP.ToString();
                if (defShip != null)
                    this.lblDefHP.Text += " / " + defShip.MaxHP.ToString();

                this.lblAttPlayer.BackColor = attacker.Player.Color;
                this.lblAttPlayer.Text = attacker.Player.Name;
                this.lblDefPlayer.BackColor = defender.Player.Color;
                this.lblDefPlayer.Text = defender.Player.Name;
            }

            CalculateOdds();
        }

        public static void SetValue(NumericUpDown nud, decimal value)
        {
            if (value < nud.Minimum)
                value = nud.Minimum;
            else if (value > nud.Maximum)
                value = nud.Maximum;
            nud.Value = value;
        }

        private void CalculateOdds()
        {
            btnDetails.Visible = false;
            CancelWorker();

            int att = (int)this.nudAttack.Value, def = (int)this.nudDefense.Value;
            int attHP = (int)this.nudAttHP.Value, defHP = (int)this.nudDefHP.Value;

            double avgAtt, avgDef;
            Dictionary<int, double> damageTable = Consts.GetDamageTable(att, def, out avgAtt, out avgDef);

            this.lblAttDmg.Text = FormatDmg(avgDef);
            this.lblDefDmg.Text = FormatDmg(avgAtt);

            if (( att * att >= defHP || att * def >= attHP ) &&
                    ( !( attacker != null && ( attacker.HP == 0 || defender.HP == 0 ) ) || this.nudAttack.Visible ))
            {
                this.lblAttKill.Text = "...";
                this.lblDefKill.Text = "...";

                InitializeWorker();
                worker.RunWorkerAsync(damageTable);
            }
            else
            {
                this.lblAttKill.Text = MainForm.FormatPct(attacker != null && attacker.HP == 0 ? 1 : 0);
                this.lblDefKill.Text = MainForm.FormatPct(defender != null && defender.HP == 0 ? 1 : 0);
            }
        }

        private void InitializeWorker()
        {
            if (worker != null)
                worker.Dispose();

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        private void CancelWorker()
        {
            if (worker != null && !worker.CancellationPending && worker.IsBusy)
                worker.CancelAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            Dictionary<int, double> damageTable = (Dictionary<int, double>)e.Argument;

            int att = (int)this.nudAttack.Value, def = (int)this.nudDefense.Value;
            int attHP = (int)this.nudAttHP.Value, defHP = (int)this.nudDefHP.Value;
            double totalDmgChance = ( att + 1 ) * ( def + 1 );

            //int chancesCap = GetCapacity(att, def, ( att / 2 ) * 2, attHP, defHP),
            //        oldChancesCap = GetCapacity(att, def, ( ( ( att + 1 ) / 2 ) * 2 ) - 1, attHP, defHP);
            int targetCap = GetCapacity(att, def, att, attHP, defHP);
            var chances = new Dictionary<ResultPoint, double>(targetCap);
            var oldChances = new Dictionary<ResultPoint, double>(targetCap);

            ResultPoint rp = new ResultPoint(attHP, defHP);
            chances.Add(rp, double.Epsilon);

            //the code in this loop should be optimized for performance
            for (int round = -1 ; ++round < att ; )
            {
                Dictionary<ResultPoint, double> temp = oldChances;
                oldChances = chances;
                chances = temp;
                chances.Clear();

                //int capacity = GetCapacity(chances, buckets);

                foreach (KeyValuePair<ResultPoint, double> chancePair in oldChances)
                {
                    ResultPoint oldRes = chancePair.Key;
                    double oldChance = chancePair.Value;
                    int ahp = oldRes.AttHP;
                    int dhp = oldRes.DefHP;
                    if (dhp > 0 && ahp > 0)
                    {
                        foreach (KeyValuePair<int, double> damagePair in damageTable)
                        {
                            int dmg = damagePair.Key;
                            ResultPoint res = oldRes;
                            if (dmg > 0)
                            {
                                dmg = dhp - dmg;
                                if (dmg < 0)
                                    dmg = 0;
                                res.DefHP = dmg;
                            }
                            else if (dmg < 0)
                            {
                                dmg = ahp + dmg;
                                if (dmg < 0)
                                    dmg = 0;
                                res.AttHP = dmg;
                            }

                            double val;
                            chances.TryGetValue(res, out val);
                            chances[res] = val + oldChance * damagePair.Value;
                        }
                    }
                    else
                    {
                        double val;
                        chances.TryGetValue(oldRes, out val);
                        chances[oldRes] = val + oldChance * totalDmgChance;
                    }
                }

                //if (capacity != GetCapacity(chances, buckets))
                //    throw new Exception();

                if (worker.CancellationPending)
                    goto end;
            }

            int max = Math.Max(chances.Count, oldChances.Count);
            double p3 = 100.0 * max / targetCap;
            const double trgPct = 21, pctError = .3;
            if (Math.Abs(p3 - trgPct) / trgPct > pctError)
            {
                FieldInfo buckets = chances.GetType().GetField("buckets", BindingFlags.NonPublic | BindingFlags.Instance);
                int chCap = GetCapacity(chances, buckets), oldCap = GetCapacity(oldChances, buckets);
                double p1 = 100.0 * chances.Count / chCap, p2 = 100.0 * oldChances.Count / oldCap;
                Console.WriteLine("att {0} def {1} attHP {2} defHP {3}", att, def, attHP, defHP);
                Console.WriteLine("target {0}", targetCap);
                Console.WriteLine("ch1 {0} pct {1}%", chances.Count, p1.ToString("00.0"));
                Console.WriteLine("ch2 {0} pct {1}%", oldChances.Count, p2.ToString("00.0"));
                Console.WriteLine("max {0} pct {1}%", max, p3.ToString("00.0"));
                Console.WriteLine();
            }

            e.Result = chances;

end:
            if (worker.CancellationPending)
                e.Cancel = true;
        }
        private static int GetCapacity(int att, int def, int rounds, int attHP, int defHP)
        {
            //return ( ( Math.Min(attHP, def * rounds) + 1 ) * ( Math.Min(defHP, att * rounds) + 1 ) - 1 );
            return (int)( GetCapacityMult(att, def, rounds, attHP, defHP) * GetCapacityMult(def, att, rounds, defHP, attHP) * 3.9 );
        }
        private static double GetCapacityMult(int s1, int s2, int rounds, int hp1, int hp2)
        {
            return Math.Min(hp1, s2 * rounds / ( .91 + Math.Min(1.13, hp2 / (double)( s1 * rounds )) * .39 )) + .91;
        }
        private static int GetCapacity(Dictionary<ResultPoint, double> chances, FieldInfo buckets)
        {
            Array array = (Array)buckets.GetValue(chances);
            if (array == null)
                return 0;
            return array.Length;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Dictionary<ResultPoint, double> result = (Dictionary<ResultPoint, double>)e.Result;
                btnDetails.Tag = result;
                btnDetails.Visible = true;

                int attHP = (int)this.nudAttHP.Value, defHP = (int)this.nudDefHP.Value;
                double total = 0, attDead = 0, defDead = 0, attDmg = 0, defDmg = 0;
                foreach (KeyValuePair<ResultPoint, double> pair in result)
                {
                    ResultPoint res = pair.Key;
                    double chance = pair.Value;
                    total += chance;
                    int ahp = res.AttHP;
                    int dhp = res.DefHP;
                    if (dhp == 0)
                        defDead += chance;
                    else if (ahp == 0)
                        attDead += chance;
                    attDmg += ( attHP - ahp ) * chance;
                    defDmg += ( defHP - dhp ) * chance;
                }
                attDead /= total;
                defDead /= total;
                attDmg /= total;
                defDmg /= total;

                this.lblAttDmg.Text = FormatDmg(attDmg);
                this.lblDefDmg.Text = FormatDmg(defDmg);

                this.lblAttKill.Text = MainForm.FormatPct(attDead);
                this.lblDefKill.Text = MainForm.FormatPct(defDead);
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            TextForm.ShowForm((Dictionary<ResultPoint, double>)btnDetails.Tag);
        }

        private string FormatDmg(double dmg)
        {
            return "-" + MainForm.FormatDouble(dmg);
        }

        private void btnAttack_Click(object sender, EventArgs e)
        {
            Ship attShip = this.attacker as Ship;
            Ship defShip = this.defender as Ship;

            if (btnAttack.DialogResult == DialogResult.None)
            {
                if (defShip == null)
                    attShip.Bombard(MainForm.GameForm, ( (Colony)defender ).Planet);
                else
                    attShip.AttackShip(MainForm.GameForm, defShip);

                RefreshShips();
                MainForm.GameForm.RefreshAll();

                if (( (Ship)attShip ).CurSpeed < 1 || attShip.HP < 1 || defender.HP < 1)
                    this.DialogResult = DialogResult.OK;
            }
        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            showConfirmation = !showConfirmation;

            Combatant temp = attacker;
            attacker = defender;
            defender = temp;

            RefreshShips();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            ShowEdit();
        }

        private void ShowEdit()
        {
            this.nudAttack.Visible = true;
            this.nudAttHP.Visible = true;
            this.nudDefense.Visible = true;
            this.nudDefHP.Visible = true;

            this.lblAttPlayer.Visible = false;
            this.lblDefPlayer.Visible = false;

            this.btnEdit.Visible = false;
            this.btnSwap.Visible = false;

            this.btnAttack.Visible = false;
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            if (( (Control)sender ).Visible)
                CalculateOdds();
        }

        private void CombatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CancelWorker();
        }

        public static bool ShowForm()
        {
            return ShowForm(null, null);
        }

        public static bool ShowForm(Combatant attacker, Combatant defender)
        {
            return ShowForm(attacker, defender, false);
        }

        public static bool ShowForm(Combatant attacker, Combatant defender, bool isConfirmation)
        {
            MainForm.GameForm.SetLocation(Form);

            Form.SetCombatants(attacker, defender, isConfirmation);

            if (isConfirmation)
            {
                Form.btnAttack.DialogResult = DialogResult.Yes;
                return ( Form.ShowDialog() == DialogResult.Yes );
            }
            else
            {
                Form.btnAttack.DialogResult = DialogResult.None;
                return ( Form.ShowDialog() != DialogResult.Cancel );
            }
        }

        public struct ResultPoint
        {
            public int AttHP;
            public int DefHP;

            public ResultPoint(int attHP, int defHP)
            {
                this.AttHP = attHP;
                this.DefHP = defHP;
            }

            public override bool Equals(object obj)
            {
                ResultPoint rp = (ResultPoint)obj;
                return ( DefHP == rp.DefHP && AttHP == rp.AttHP );
            }

            public override int GetHashCode()
            {
                return ( AttHP | ( DefHP << 15 ) );
            }

            public override string ToString()
            {
                return string.Format("( {0} , {1} )", AttHP, DefHP);
            }
        }

        #endregion //Combat

        #region Logging

        private List<ILogType> log = new List<ILogType>();

        public static void OnCombat(Combatant attacker, Combatant defender, int attack, int defense)
        {
            Form.Enqueue(new CombatType(attacker, defender, attack, defense));
        }

        public static void OnLevel(Ship ship, double pct, int last, int needed)
        {
            Form.Enqueue(new LevelUpType(ship, pct, last, needed));
        }

        public static void OnBombard(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage)
        {
            Form.Enqueue(new BombardType(ship, planet, freeDmg, colonyDamage, planetDamage));
        }

        public static void OnInvade(Ship ship, Colony colony, int attackers, double attSoldiers, double gold, double attack, double defense)
        {
            Form.Enqueue(new InvadeType(ship, colony, attackers, attSoldiers, gold, attack, defense));
        }

        private void Enqueue(ILogType entry)
        {
            if (!( log.Count > 0 && log[log.Count - 1].Combine(entry) ))
                log.Add(entry);
        }

        public static void FlushLog()
        {
            Form.Flush();
        }

        public static void OnRefresh()
        {
            if (Form.chkLog.Checked && Form.log.Count > 0)
                Form.ShowLog();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            ShowLog();
        }

        private void ShowLog()
        {
            Flush();
            TextForm.ShowForm();
        }

        private void Flush()
        {
            foreach (ILogType type in log)
                type.Log();
            log.Clear();
        }

        private static string Format(int value)
        {
            return Format(value, 2, false);
        }
        private static string Format(int value, bool neg)
        {
            return Format(value, 2, neg);
        }
        private static string Format(int value, int digits)
        {
            return Format(value, digits, false);
        }
        private static string Format(int value, int digits, bool neg)
        {
            bool isNeg = ( neg && ( value < 0 ) );
            if (isNeg)
                value = ( -value );
            string retVal = value.ToString().PadLeft(digits);
            if (neg)
                retVal = ( isNeg ? '-' : value > 0 ? '+' : ' ' ) + retVal;
            return retVal;
        }
        private static string Format(double value)
        {
            bool isNeg = value < 0;
            if (isNeg)
                value = -value;
            return ( isNeg ? '-' : value > 0 ? '+' : ' ' ) + MainForm.FormatDouble(value).PadLeft(4);
        }

        private class CombatType : BaseLogType<CombatType>
        {
            private readonly Combatant attacker, defender;
            private readonly int attack, defense;

            private readonly int att, attCurHP, attMaxHP, attExp, attPop, def, defCurHP, defMaxHP, defExp, defPop;

            public CombatType(Combatant attacker, Combatant defender, int attack, int defense)
            {
                this.attacker = attacker;
                this.defender = defender;
                this.attack = attack;
                this.defense = defense;

                Ship attShip = ( attacker as Ship ), defShip = ( defender as Ship );

                att = attacker.Att;
                attCurHP = attacker.HP;
                attPop = attacker.Population;
                if (attShip != null)
                {
                    attMaxHP = attShip.MaxHP;
                    attExp = attShip.GetTotalExp();
                }

                def = defender.Def;
                defCurHP = defender.HP;
                defPop = defender.Population;
                if (defShip != null)
                {
                    defMaxHP = defShip.MaxHP;
                    defExp = defShip.GetTotalExp();
                }
            }

            protected override bool CanCombine(CombatType other)
            {
                return ( this.attacker == other.attacker && this.defender == other.defender && base.others.Count < this.att );
            }

            public override void Log()
            {
                bool attShip = !( this.attacker is Ship ), defShip = !( this.defender is Ship );

                MainForm.GameForm.LogMsg("{10} {0} ({1}, {2}{3}{8}) : {11} {4} ({5}, {6}{7}{9})", this.attacker.ToString(), this.att, this.attCurHP,
                        attShip ? string.Empty : ( "/" + this.attMaxHP ), this.defender.ToString(), this.def, this.defCurHP,
                        defShip ? string.Empty : ( "/" + this.defMaxHP ), attShip ? string.Empty : ( ", " + this.attExp ),
                        defShip ? string.Empty : ( ", " + this.defExp ), this.attacker.Player.Name, this.defender.Player.Name);

                int rounds = base.others.Count;
                int attDmg = 0, defDmg = 0, attTot = 0, defTot = 0, attPop = this.attPop, defPop = this.defPop;
                for (int round = 0 ; round < rounds ; ++round)
                    Log(base.others[round], round, ref attDmg, ref defDmg, ref attTot, ref defTot, ref attPop, ref defPop);

                double attAdv = attTot - this.att * rounds / 2.0;
                double defAdv = defTot - this.def * rounds / 2.0;
                MainForm.GameForm.LogMsg("{0} :{1} ({2} : {3})", Format(attTot, 6), Format(defTot, 4), Format(attDmg, 3, true), Format(defDmg, 3, true));
                MainForm.GameForm.LogMsg(" {0} : {1} ({2})", Format(attAdv), Format(defAdv), Format(attAdv - defAdv));
                MainForm.GameForm.LogMsg();
            }
            private static void Log(CombatType combatType, int round, ref int attDmg, ref int defDmg, ref int attTot, ref int defTot, ref int attPop, ref int defPop)
            {
                int attack = combatType.attack, defense = combatType.defense;

                int damage = attack - defense, popLoss = 0;
                if (damage > 0)
                {
                    attDmg += damage;
                    popLoss = defPop - combatType.defPop;
                    defPop = combatType.defPop;
                }
                else if (damage < 0)
                {
                    defDmg += damage;
                    popLoss = attPop - combatType.attPop;
                    attPop = combatType.attPop;
                }
                attTot += attack;
                defTot += defense;

                MainForm.GameForm.LogMsg("{3}={0} :{1} ({2}){4}", Format(attack, 3), Format(defense, 4), Format(damage, true),
                        Format(round + 1), popLoss > 0 ? ( -popLoss ).ToString().PadLeft(5) : string.Empty);
            }
        }

        private class LevelUpType : BaseLogType<LevelUpType>
        {
            private readonly Ship ship;
            private readonly double pct;
            private readonly int last, needed;

            private readonly Ship.ExpType expType;
            private readonly int totalExp, curHP, maxHP;

            public LevelUpType(Ship ship, double pct, int last, int needed)
            {
                this.ship = ship;
                this.pct = pct;
                this.last = last;
                this.needed = needed;

                expType = ship.NextExpType;
                totalExp = this.ship.GetTotalExp();
                curHP = this.ship.HP;
                maxHP = this.ship.MaxHP;
            }

            protected override bool CanCombine(LevelUpType other)
            {
                return ( this.ship == other.ship );
            }

            public override void Log()
            {
                MainForm.GameForm.LogMsg("{2} {0} - {1}", this.ship.ToString(), MainForm.FormatInt(this.last), this.ship.Player.Name);

                Ship.ExpType expType = this.expType;
                foreach (LevelUpType level in base.others)
                    Log(level, ref expType);

                LevelUpType last = base.others[base.others.Count - 1];

                MainForm.GameForm.LogMsg("({0}) {2}/{3} - {1}", GetExpType(last.expType), last.totalExp, last.curHP, last.maxHP);
                MainForm.GameForm.LogMsg();
            }
            private static void Log(LevelUpType level, ref Ship.ExpType expType)
            {
                MainForm.GameForm.LogMsg(" {0} ({1}) - {2}", GetExpType(expType),
                        MainForm.FormatPct(level.pct).PadLeft(4, ' '), MainForm.FormatInt(level.needed));
                expType = level.expType;
            }
            private static string GetExpType(Ship.ExpType expType)
            {
                return expType.ToString().PadRight(5, ' ');
            }
        }

        private class BombardType : BaseLogType<BombardType>
        {
            public readonly Ship Ship;

            private readonly Planet planet;
            private readonly int freeDmg, colonyDamage, planetDamage;

            private readonly Colony colony;
            private readonly double bombardDamage;
            private readonly int totalExp, quality, hp, population;
            private readonly bool dead;

            public BombardType(Ship ship, Planet planet, int freeDmg, int colonyDamage, int planetDamage)
            {
                this.Ship = ship;
                this.planet = planet;
                this.freeDmg = freeDmg;
                this.colonyDamage = colonyDamage;
                this.planetDamage = planetDamage;

                colony = planet.Colony;
                bombardDamage = ship.BombardDamage;
                totalExp = ship.GetTotalExp();
                quality = planet.Quality;
                dead = planet.Dead;
                if (colony != null)
                {
                    hp = colony.HP;
                    population = colony.Population;
                }
            }

            protected override bool CanCombine(BombardType other)
            {
                return ( this.Ship == other.Ship && this.planet == other.planet );
            }

            public override void Log()
            {
                if (this.colonyDamage != int.MinValue)
                    throw new Exception();

                if (base.others.Count > 0)
                {
                    MainForm.GameForm.LogMsg("{0} {1} ({2}, {3}) : {4} ({5}{6}{7})", this.Ship.Player.Name, this.Ship.ToString(),
                            MainForm.FormatUsuallyInt(this.bombardDamage), this.totalExp,
                            this.colony == null ? "Uncolonized" : this.colony.Player.Name + " Colony",
                            this.colony == null ? string.Empty : this.hp + ", ", this.quality,
                            this.colony == null ? string.Empty : ", " + this.population);

                    for (int index = 0 ; index < base.others.Count ; ++index)
                        Log(base.others[index]);

                    MainForm.GameForm.LogMsg();
                }
            }
            private static void Log(BombardType bombardType)
            {
                if (bombardType.colonyDamage != int.MinValue)
                {
                    string logMsg = "No Damage";
                    if (bombardType.freeDmg != 0 || bombardType.colonyDamage != 0 || bombardType.planetDamage != 0)
                    {
                        logMsg = Log(string.Empty, bombardType.freeDmg, " HP");
                        logMsg = Log(logMsg, bombardType.planetDamage, " Quality");
                        logMsg = Log(logMsg, bombardType.colonyDamage, " Population");
                        if (bombardType.dead)
                            logMsg += ".  Planet Destroyed!";
                    }
                    MainForm.GameForm.LogMsg(logMsg);
                }
            }
            private static string Log(string logMsg, int amt, string type)
            {
                if (amt != 0)
                {
                    if (logMsg.Length > 0)
                        logMsg += ", ";
                    logMsg += -amt + type;
                }
                return logMsg;
            }
        }

        private class InvadeType : BaseLogType<InvadeType>
        {
            private readonly Ship ship;
            private readonly Colony colony;
            private readonly int attackers;
            private readonly double attSoldiers, gold, attack, defense;

            private readonly int quality, defenders, att, def, hp;
            private readonly double defSoldiers;

            public InvadeType(Ship ship, Colony colony, int attackers, double attSoldiers, double gold, double attack, double defense)
            {
                this.ship = ship;
                this.colony = colony;
                this.attackers = attackers;
                this.attSoldiers = attSoldiers;
                this.gold = gold;
                this.attack = attack;
                this.defense = defense;

                quality = colony.Planet.Quality;
                defenders = colony.Population;
                defSoldiers = colony.TotalSoldiers;
                att = colony.Att;
                def = colony.Def;
                hp = colony.HP;
            }

            protected override bool CanCombine(InvadeType other)
            {
                return ( base.others.Count < 2 );
            }

            public override void Log()
            {
                InvadeType mid = base.others[0];
                InvadeType after = base.others[1];

                MainForm.GameForm.LogMsg("{0} {1} ({2}) : {3} Colony ({4})", this.ship.Player.Name, this.ship.ToString(),
                        MainForm.FormatPct(after.attack, true), this.colony.Player.Name, MainForm.FormatPct(after.defense, true));
                this.LogLine();

                if (this.defenders > 0)
                {
                    mid.LogLine();
                    after.LogLine();

                    if (after.gold > Consts.FLOAT_ERROR)
                        MainForm.GameForm.LogMsg("Gold -{0}{1}", MainForm.FormatUsuallyInt(after.gold),
                                after.gold > this.gold - Consts.FLOAT_ERROR ? string.Empty : "/" + MainForm.FormatInt(this.gold));
                    if (this.quality != after.quality)
                        MainForm.GameForm.LogMsg("Quality {0} -> {1}{2}", this.quality,
                                after.quality < 0 ? "Destroyed" : after.quality.ToString(),
                                after.quality < 0 ? string.Empty : " (" + ( after.quality - this.quality ) + ")");
                    if (( this.att != after.att || this.def != after.def || this.hp != after.hp )
                            && after.attackers == 0 && after.quality >= 0)
                        MainForm.GameForm.LogMsg("Defenses  {0} : {1} ({2})  ->  {3} : {4} ({5})",
                                this.att, this.def, this.hp, after.att, after.def, after.hp);
                }

                MainForm.GameForm.LogMsg();
            }
            private void LogLine()
            {
                MainForm.GameForm.LogMsg("{0} ({1}) : {2} ({3})", GetPop(this.attackers), GetSoldiers(this.attackers, this.attSoldiers),
                        GetPop(this.defenders), GetSoldiers(this.defenders, this.defSoldiers));
            }
            private static string GetPop(int pop)
            {
                return pop.ToString().PadLeft(4, ' ');
            }
            private static string GetSoldiers(int pop, double soldiers)
            {
                return MainForm.FormatPct(PopCarrier.GetSoldiers(pop, soldiers, 1), true).PadLeft(6, ' ');
            }
        }

        private abstract class BaseLogType<T> : ILogType where T : class, ILogType
        {
            protected List<T> others = new List<T>();

            public bool Combine(ILogType other)
            {
                T combine = ( other as T );
                bool combined = ( combine != null && this.CanCombine(combine) );
                if (combined)
                    others.Add(combine);
                return combined;
            }

            protected abstract bool CanCombine(T other);

            public abstract void Log();
        }

        private interface ILogType
        {
            bool Combine(ILogType other);
            void Log();
        }

        #endregion //Logging
    }
}

#region test
//private void DoCalc(bool forceSimulate, ref double attDmg, ref double defDmg, out double attDead, out double defDead, Dictionary<int, double> damageTable)
//{
//    attDead = 0;
//    defDead = 0;

//    bool simulate = forceSimulate;
//    if (!simulate)
//    {
//        int att = attacker.Att, def = defender.Def, attHP = attacker.CurHP, defHP = defender.CurHP;
//        if (!(att * att < defHP && def * att < attHP))
//        {
//            Approximate(out attDmg, out defDmg, out attDead, out defDead, damageTable);
//            simulate = (attDead > .005 || defDead > .005);
//            if (GetComplexity(att, def, attHP, defHP) > acceptComplexity)
//            {
//                this.btnExact.Visible = true;
//                simulate = false;
//            }
//        }
//    }
//    if (simulate)
//        Simulate(out attDmg, out defDmg, out attDead, out defDead, damageTable);
//}

//private void Approximate(out double attDead, out double defDead, Dictionary<int, double> damageTable)
//{
//    int rounds = attacker.Att;
//    int attHP = attacker.CurHP;
//    int defHP = defender.CurHP;

//    double[] attArr = new double[attacker.Att + 1];
//    double[] defArr = new double[defender.Def + 1];
//    foreach (int damage in damageTable.Keys)
//    {
//        int att = damage;
//        int def = damage;
//        if (att < 0)
//            att = 0;
//        if (def > 0)
//            def = 0;
//        def *= -1;

//        double chance = damageTable[damage];
//        attArr[att] += chance;
//        defArr[def] += chance;
//    }


//    attArr = GetArr(attArr, rounds);
//    defDead = 0;
//    double attChances = 0;
//    int attLen = attArr.Length;
//    for (int a = 0; a < attLen; ++a)
//    {
//        attChances += attArr[a];
//        if (a >= defHP)
//            defDead += attArr[a];
//    }
//    defDead /= attChances;

//    defArr = GetArr(defArr, rounds);
//    attDead = 0;
//    double defChances = 0;
//    int defLen = defArr.Length;
//    for (int d = 0; d < defLen; ++d)
//    {
//        defChances += defArr[d];
//        if (d >= attHP)
//            attDead += defArr[d];
//    }
//    attDead /= defChances;
//}

//private double[] GetArr(double[] cur, int amt)
//{
//    int count = cur.Length;
//    int lower = count - 1;
//    for (int b = 2; b <= amt; ++b)
//    {
//        int len = lower * b + 1;
//        double[] prev = new double[len];
//        prev[0] = cur[0];
//        for (int c = 1; c < len; ++c)
//            prev[c] = prev[c - 1] + (c < cur.Length ? cur[c] : 0);
//        cur = new double[len];
//        for (int d = 0; d < len; ++d)
//        {
//            cur[d] = prev[d];
//            if (d > lower)
//                cur[d] -= prev[d - count];
//        }
//    }
//    return cur;
//}

//private double[] GetArr(double[] cur, int amt)
//{
//    int count = cur.Length;
//    int lower = count - 1;
//    for (int b = 2; b <= amt; ++b)
//    {
//        int len = lower * b + 1;
//        int div = (int)Math.Floor((len + 1) / 2.0);
//        double[] prev = new double[div];
//        prev[0] = cur[0];
//        for (int c = 1; c < div; ++c)
//            prev[c] = prev[c - 1] + cur[c];
//        cur = new double[len];
//        for (int d = 0; d < div; ++d)
//        {
//            cur[d] = prev[d];
//            if (d > lower)
//                cur[d] -= prev[d - count];
//        }
//        for (int e = len; e > div; --e)
//            cur[e - 1] = cur[len - e];
//    }
//    return cur;
//}
#endregion //test