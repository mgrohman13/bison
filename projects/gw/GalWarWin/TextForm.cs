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
            string format = "#.000000000000E+00";
            Action<Func<ResultPoint, int>> SetText = GetResult =>
            {
                var dict = new SortedDictionary<int, double>();
                foreach (var pair in combatResults)
                {
                    double value;
                    dict.TryGetValue(GetResult(pair.Key), out value);
                    dict[pair.Key.AttHP] = value + pair.Value;
                }
                double max = dict.Values.Max(), tot = dict.Values.Sum();

                form.textBox1.Text += Environment.NewLine;
                foreach (var pair in dict)
                {
                    int val = (int)Math.Ceiling(pair.Value / max * 999 / GalWar.Consts.FLOAT_ERROR_ONE);
                    form.textBox1.Text += string.Format("{0}\t{1}\t{2}" + Environment.NewLine, pair.Key.ToString().PadLeft(5, ' '), val.ToString("000"), ( pair.Value / tot ).ToString(format));
                }
                form.textBox1.Text += Environment.NewLine;
            };

            form.textBox1.Text = "Attacker:";
            SetText(resultPoint => resultPoint.AttHP);
            form.textBox1.Text += "Defender:";
            SetText(resultPoint => resultPoint.DefHP);

            //int minAtt = combatResults.Keys.Min(resultPoint => resultPoint.AttHP);
            //int maxAtt = combatResults.Keys.Max(resultPoint => resultPoint.AttHP);
            //int minDef = combatResults.Keys.Min(resultPoint => resultPoint.DefHP);
            //int maxDef = combatResults.Keys.Max(resultPoint => resultPoint.DefHP);
            //double sum = combatResults.Values.Sum();
            //form.textBox1.Text += Environment.NewLine + Environment.NewLine + "Table:" + Environment.NewLine;
            //for (int att = minAtt ; att <= maxAtt ; att++)
            //    form.textBox1.Text += "\t" + att;
            //for (int def = minDef ; def <= maxDef ; def++)
            //{
            //    form.textBox1.Text += Environment.NewLine + def;
            //    for (int att = minAtt ; att <= maxAtt ; att++)
            //    {
            //        ResultPoint resultPoint = new ResultPoint(att, def);
            //        double chance;
            //        combatResults.TryGetValue(resultPoint, out chance);
            //        form.textBox1.Text += "\t" + ( chance / sum ).ToString(format);
            //    }
            //}

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
                form.textBox1.Text += ( pair.Value / total ).ToString("0.00%").PadLeft(6);
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
