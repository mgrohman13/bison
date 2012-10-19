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

        private MainForm gameForm;
        private Combatant attacker, defender;
        private bool isConfirmation, showConfirmation;

        private CombatForm()
        {
            InitializeComponent();

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

        public void RefreshShips()
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

        public void InitializeWorker()
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

        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            Dictionary<int, double> damageTable = (Dictionary<int, double>)e.Argument;

            int att = (int)this.nudAttack.Value, def = (int)this.nudDefense.Value;
            int attHP = (int)this.nudAttHP.Value, defHP = (int)this.nudDefHP.Value;
            double totalDmgChance = ( att + 1 ) * ( def + 1 );

            int c1 = GetCapacity(att, def, ( att / 2 ) * 2, attHP, defHP),
                c2 = GetCapacity(att, def, ( ( ( att + 1 ) / 2 ) * 2 ) - 1, attHP, defHP);
            var chances = new Dictionary<ResultPoint, double>(c1);
            var oldChances = new Dictionary<ResultPoint, double>(c2);

            FieldInfo buckets = chances.GetType().GetField("buckets", BindingFlags.NonPublic | BindingFlags.Instance);
            Console.WriteLine();
            Console.WriteLine("att {0} def {1} attHP {2} defHP {3}", att, def, attHP, defHP);
            Console.WriteLine("c1: {0} c2: {1}", c1, c2);
            Console.WriteLine("chances: {0} oldChances: {1}", GetCapacity(chances, buckets), GetCapacity(oldChances, buckets));

            ResultPoint rp = new ResultPoint(attHP, defHP);
            chances.Add(rp, 1);

            //the code in this loop should be optimized for performance
            for (int round = -1 ; ++round < att ; )
            {
                Dictionary<ResultPoint, double> temp = oldChances;
                oldChances = chances;
                chances = temp;
                chances.Clear();

                int capacity = GetCapacity(chances, buckets);

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

                if (capacity != GetCapacity(chances, buckets))
                    throw new Exception();

                if (worker.CancellationPending)
                    goto end;
            }

            Console.WriteLine("chances: {0} cap: {1}", chances.Count, GetCapacity(chances, buckets));
            Console.WriteLine("oldChances: {0} cap: {1}", oldChances.Count, GetCapacity(oldChances, buckets));
            Console.WriteLine();

            double total = 0, attDead = 0, defDead = 0, attDmg = 0, defDmg = 0;
            foreach (KeyValuePair<ResultPoint, double> pair in chances)
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
            e.Result = new WorkerResult(attDead, defDead, attDmg, defDmg);

end:
            if (worker.CancellationPending)
                e.Cancel = true;
        }
        private static int GetCapacity(int att, int def, int rounds, int attHP, int defHP)
        {
            return ( ( Math.Min(attHP, def * rounds) + 1 ) * ( Math.Min(defHP, att * rounds) + 1 ) - 1 );
        }
        private static int GetCapacity(Dictionary<ResultPoint, double> chances, FieldInfo buckets)
        {
            return ( (Array)buckets.GetValue(chances) ).Length;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                WorkerResult result = (WorkerResult)e.Result;

                this.lblAttDmg.Text = FormatDmg(result.AttDmg);
                this.lblDefDmg.Text = FormatDmg(result.DefDmg);

                this.lblAttKill.Text = MainForm.FormatPct(result.AttDead);
                this.lblDefKill.Text = MainForm.FormatPct(result.DefDead);
            }
        }

        private string FormatDmg(double dmg)
        {
            return "-" + MainForm.FormatDouble(dmg);
        }

        private void btnAttack_Click(object sender, EventArgs e)
        {
            Ship attShip = this.attacker as Ship;
            Ship defShip = this.defender as Ship;

            logAtt = attacker.Att;
            logAttCurHP = attacker.HP;
            logAttMaxHP = attShip == null ? -1 : attShip.MaxHP;
            logAttExp = attShip == null ? -1 : attShip.GetTotalExp();
            logDef = defender.Def;
            logDefCurHP = defender.HP;
            logDefMaxHP = defShip == null ? -1 : defShip.MaxHP;
            logDefExp = defShip == null ? -1 : defShip.GetTotalExp();

            if (btnAttack.DialogResult == DialogResult.None)
            {
                if (defShip == null)
                    attShip.Bombard(gameForm, ( (Colony)defender ).Planet);
                else
                    attShip.AttackShip(gameForm, defShip);

                RefreshShips();
                gameForm.RefreshAll();

                FlushLog();

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

        public static void Combat(Combatant attacker, Combatant defender, int attack, int defense, int popLoss)
        {
            Form.OnCombat(attacker, defender, attack, defense, popLoss);
        }

        public static void LevelUp(Ship ship, Ship.ExpType expType, double pct, int needExp, int lastExp)
        {
            Form.OnLevel(ship, expType, pct, needExp, lastExp);
        }

        public static void FlushLog(MainForm gameForm)
        {
            Form.gameForm = gameForm;
            Form.FlushLog();
        }

        public static bool ShowDialog(MainForm gameForm)
        {
            return ShowDialog(gameForm, null, null);
        }

        public static bool ShowDialog(MainForm gameForm, Combatant attacker, Combatant defender)
        {
            return ShowDialog(gameForm, attacker, defender, false);
        }

        public static bool ShowDialog(MainForm gameForm, Combatant attacker, Combatant defender, bool isConfirmation)
        {
            Form.gameForm = gameForm;
            gameForm.SetLocation(Form);

            Form.SetCombatants(attacker, defender, isConfirmation);

            if (isConfirmation)
            {
                Form.FlushLog();

                Form.btnAttack.DialogResult = DialogResult.Yes;
                return ( Form.ShowDialog() == DialogResult.Yes );
            }
            else
            {
                Form.btnAttack.DialogResult = DialogResult.None;
                return ( Form.ShowDialog() != DialogResult.Cancel );
            }
        }

        private struct ResultPoint
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
                return ( AttHP ^ ( DefHP << 15 ) );
            }

            public override string ToString()
            {
                return string.Format("( {0} , {1} )", AttHP, DefHP);
            }
        }

        private class WorkerResult
        {
            public readonly double AttDead, DefDead, AttDmg, DefDmg;

            public WorkerResult(double attDead, double defDead, double attDmg, double defDmg)
            {
                this.AttDead = attDead;
                this.DefDead = defDead;
                this.AttDmg = attDmg;
                this.DefDmg = defDmg;
            }
        }

        #region Logging

        private void btnLog_Click(object sender, EventArgs e)
        {
            ShowLog();
        }

        private void ShowLog()
        {
            TextForm.ShowDialog(gameForm);
        }

        private int logAtt;
        private int logAttCurHP;
        private int logAttMaxHP;
        private int logAttExp;
        private int logDef;
        private int logDefCurHP;
        private int logDefMaxHP;
        private int logDefExp;
        private List<CombatType> combat = new List<CombatType>();
        private Dictionary<Ship, List<LevelUpType>> levels = new Dictionary<Ship, List<LevelUpType>>();

        private void OnCombat(Combatant attacker, Combatant defender, int attack, int defense, int popLoss)
        {
            combat.Add(new CombatType(attacker, defender, attack, defense, popLoss));
        }

        private void OnLevel(Ship ship, Ship.ExpType expType, double pct, int needExp, int lastExp)
        {
            List<LevelUpType> list;
            if (!this.levels.TryGetValue(ship, out list))
            {
                list = new List<LevelUpType>();
                this.levels.Add(ship, list);
            }
            list.Add(new LevelUpType(expType, pct, needExp, lastExp));
        }

        private void FlushLog()
        {
            bool show = false;

            if (this.combat.Count > 0)
            {
                show = true;
                LogCombat();
                this.combat.Clear();
            }

            if (this.levels.Count > 0)
            {
                show = true;
                foreach (KeyValuePair<Ship, List<LevelUpType>> pair in Game.Random.Iterate<KeyValuePair<Ship, List<LevelUpType>>>(this.levels))
                    LogLevelUp(pair.Key, pair.Value);
                this.levels.Clear();
            }

            if (show && this.chkLog.Checked)
                ShowLog();
        }

        private void LogCombat()
        {
            CombatType first = combat[0];
            Combatant attacker = first.attacker, defender = first.defender;
            bool attShip = !( attacker is Ship ), defShip = !( defender is Ship );

            gameForm.LogMsg("{10} {0} ({1}, {2}{3}{8}) : {11} {4} ({5}, {6}{7}{9})", attacker.ToString(), logAtt, logAttCurHP,
                    attShip ? string.Empty : ( "/" + logAttMaxHP ), defender.ToString(), logDef, logDefCurHP,
                    defShip ? string.Empty : ( "/" + logDefMaxHP ), attShip ? string.Empty : ( ", " + logAttExp ),
                    defShip ? string.Empty : ( ", " + logDefExp ), attacker.Player.Name, defender.Player.Name);

            int rounds = combat.Count;
            int attDmg = 0, defDmg = 0, attTot = 0, defTot = 0;
            for (int round = 0 ; round < rounds ; )
            {
                CombatType combatType = combat[round];
                int attack = combatType.attack, defense = combatType.defense, popLoss = combatType.popLoss;
                ++round;

                int damage = attack - defense;
                if (damage > 0)
                    attDmg += damage;
                else if (damage < 0)
                    defDmg += damage;
                attTot += attack;
                defTot += defense;

                gameForm.LogMsg("{3}={0} :{1} ({2}){4}", Format(attack, 3), Format(defense, 4), Format(damage, true),
                        Format(round), popLoss > 0 ? ( -popLoss ).ToString().PadLeft(5) : string.Empty);
            }

            double attAdv = attTot - logAtt * rounds / 2.0;
            double defAdv = defTot - logDef * rounds / 2.0;
            gameForm.LogMsg("{0} :{1} ({2} : {3})", Format(attTot, 6), Format(defTot, 4), Format(attDmg, 3, true), Format(defDmg, 3, true));
            gameForm.LogMsg(" {0} : {1} ({2})", Format(attAdv), Format(defAdv), Format(attAdv - defAdv));
            gameForm.LogMsg();

            LogLevelUp(attacker);
            LogLevelUp(defender);
        }

        private void LogLevelUp(Combatant combatant)
        {
            Ship ship = combatant as Ship;
            if (ship != null && this.levels.Count > 1)
            {
                List<LevelUpType> levels;
                if (this.levels.TryGetValue(ship, out levels))
                {
                    LogLevelUp(ship, levels);
                    this.levels.Remove(ship);
                }
            }
        }

        private void LogLevelUp(Ship ship, List<LevelUpType> levels)
        {
            gameForm.LogMsg("{2} {0} - {1}", ship.ToString(), MainForm.FormatInt(levels[0].lastExp), ship.Player.Name);

            foreach (LevelUpType level in levels)
                gameForm.LogMsg(" {0} ({1}) - {2}", level.expType.ToString().PadRight(5, ' '),
                        MainForm.FormatPct(level.pct).PadLeft(4, ' '), MainForm.FormatInt(level.needExp));

            gameForm.LogMsg("({0}) {2}/{3} - {1}", ship.NextExpType.ToString().PadRight(5, ' '), ship.GetTotalExp(), ship.HP, ship.MaxHP);
            gameForm.LogMsg();
        }

        private string Format(int value)
        {
            return Format(value, 2, false);
        }

        private string Format(int value, bool neg)
        {
            return Format(value, 2, neg);
        }

        private string Format(int value, int digits)
        {
            return Format(value, digits, false);
        }

        private string Format(int value, int digits, bool neg)
        {
            bool isNeg = ( neg && ( value < 0 ) );
            if (isNeg)
                value = ( -value );
            string retVal = value.ToString().PadLeft(digits);
            if (neg)
                retVal = ( isNeg ? '-' : value > 0 ? '+' : ' ' ) + retVal;
            return retVal;
        }

        private string Format(double value)
        {
            bool isNeg = value < 0;
            if (isNeg)
                value = -value;
            return ( isNeg ? '-' : value > 0 ? '+' : ' ' ) + MainForm.FormatDouble(value).PadLeft(4);
        }

        private struct CombatType
        {
            public Combatant attacker, defender;
            public int attack, defense, popLoss;

            public CombatType(Combatant attacker, Combatant defender, int attack, int defense, int popLoss)
            {
                this.attacker = attacker;
                this.defender = defender;
                this.attack = attack;
                this.defense = defense;
                this.popLoss = popLoss;
            }
        }

        private struct LevelUpType
        {
            public Ship.ExpType expType;
            public double pct;
            public int needExp, lastExp;

            public LevelUpType(Ship.ExpType expType, double pct, int needExp, int lastExp)
            {
                this.expType = expType;
                this.pct = pct;
                this.needExp = needExp;
                this.lastExp = lastExp;
            }
        }

        #endregion //Logging

    }
}

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
