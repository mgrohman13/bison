using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;
using ResultPoint = GalWarWin.CombatForm.ResultPoint;

namespace GalWarWin
{
    public partial class TextForm : Form
    {
        private static TextForm form = new TextForm();

        private TextForm()
        {
            InitializeComponent();

            this.textBox1.GotFocus += new EventHandler(textBox1_GotFocus);
        }

        private void textBox1_GotFocus(object sender, EventArgs e)
        {
            form.textBox1.ScrollToCaret();
        }

        private void TextForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                textBox1.SelectAll();
            else if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        public static void ShowForm()
        {
            form.textBox1.Text = MainForm.GameForm.GetLog();
            form.textBox1.Select(form.textBox1.Text.Length, 0);

            form.ShowDialog();
        }

        internal static void ShowForm(Dictionary<ResultPoint, double> combatResults)
        {
            var att = new SortedDictionary<int, double>();
            var def = new SortedDictionary<int, double>();

            foreach (var pair in combatResults)
            {
                double value;

                att.TryGetValue(pair.Key.AttHP, out value);
                att[pair.Key.AttHP] = value + pair.Value;

                def.TryGetValue(pair.Key.DefHP, out value);
                def[pair.Key.DefHP] = value + pair.Value;
            }

            double maxAtt = 0, maxDef = 0;
            foreach (double v in att.Values)
                maxAtt = Math.Max(maxAtt, v);
            foreach (double v in def.Values)
                maxDef = Math.Max(maxDef, v);

            form.textBox1.Text = "Attacker:" + Environment.NewLine;
            foreach (var pair in att)
            {
                int val = (int)Math.Ceiling(pair.Value / maxAtt * 999 / GalWar.Consts.FLOAT_ERROR_ONE);
                form.textBox1.Text += string.Format("{0} - {1}" + Environment.NewLine, pair.Key.ToString().PadLeft(5, ' '), val.ToString("000"));
            }

            form.textBox1.Text += Environment.NewLine + "Defender:" + Environment.NewLine;
            foreach (var pair in def)
            {
                int val = (int)Math.Ceiling(pair.Value / maxDef * 999 / GalWar.Consts.FLOAT_ERROR_ONE);
                form.textBox1.Text += string.Format("{0} - {1}" + Environment.NewLine, pair.Key.ToString().PadLeft(5, ' '), val.ToString("000"));
            }

            form.ShowDialog();
        }

        internal static void ShowForm(int att, int def)
        {
            double avgAtt, avgDef;
            IDictionary<int, double> table = Consts.GetDamageTable(att, def, out avgAtt, out avgDef);

            form.textBox1.Text = avgAtt.ToString("+0.00").PadLeft(8);
            form.textBox1.Text += Environment.NewLine;
            form.textBox1.Text += avgDef.ToString("-0.00").PadLeft(8);
            form.textBox1.Text += Environment.NewLine + Environment.NewLine;

            double total = table.Values.Sum();
            foreach (var pair in table)
            {
                form.textBox1.Text += pair.Key.ToString("+#;-#;0").PadLeft(5);
                form.textBox1.Text += ": ";
                form.textBox1.Text += MainForm.FormatPct(pair.Value / total, true).PadLeft(4);
                form.textBox1.Text += Environment.NewLine;
            }

            form.ShowDialog();
        }

        public static void ShowForm(List<Game.Result> results)
        {
            form.textBox1.Text = string.Empty;
            foreach (Game.Result result in results)
                form.textBox1.Text += result.Player.Name + " - " + result.Points.ToString() + Environment.NewLine;
            form.textBox1.SelectAll();

            form.Size = new Size(300, 300);
            MainForm.GameForm.SetLocation(form);
            form.ShowDialog();
        }

        private void TextForm_Load(object sender, EventArgs e)
        {
            if (this.Width != 300 || this.Height != 300)
            {
                Rectangle screen = Screen.FromControl(this).WorkingArea;
                this.Location = new Point(( screen.Width - this.Width ) / 2, 0);
                this.Height = screen.Height;
            }
        }
    }
}
