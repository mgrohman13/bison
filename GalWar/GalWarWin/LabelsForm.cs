using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class LabelsForm : Form
    {
        private static LabelsForm form = new LabelsForm();

        private List<Label> labels;

        private LabelsForm()
        {
            InitializeComponent();

            labels = new List<Label>();
            labels.Add(this.label);
            labels.Add(this.info);
        }

        private void SetData(string[] data)
        {
            int a = -1, length = data.Length - 1;
            while (++a < length)
            {
                if (this.labels.Count <= a)
                {
                    Label label = NewLabel(true);
                    Label info = NewLabel(false);
                    this.labels.Add(label);
                    this.labels.Add(info);
                }

                Label lbl = this.labels[a];
                string text = data[a];
                if (text.Length > 0)
                    text += ":";
                lbl.Text = text;
                lbl.Show();

                lbl = this.labels[++a];
                text = data[a];
                MainForm.ColorForIncome(lbl, text);
                lbl.Show();
            }

            for (int b = 0; b < a; ++b)
            {
                Label lbl = this.labels[b];
                if ((b & 1) == 0)
                    lbl.Width = 65;
                else
                    lbl.Location = new Point(69, lbl.Location.Y);
            }
            int minDiff = int.MaxValue, infoWidth = int.MinValue, valueWidth = int.MinValue;
            for (int b = 0; b < a; ++b)
            {
                Label lbl = this.labels[b];
                int diff = lbl.Width;
                lbl.AutoSize = true;
                int width = lbl.Width;
                lbl.AutoSize = false;

                if ((b & 1) == 0)
                {
                    diff -= width;
                    if (diff < minDiff)
                        minDiff = diff;
                    if (width > infoWidth)
                        infoWidth = width;
                }
                else
                {
                    if (width > valueWidth)
                        valueWidth = width;
                }
            }
            for (int b = 0; b < a; ++b)
            {
                Label lbl = this.labels[b];
                if ((b & 1) == 0)
                {
                    lbl.Width = infoWidth;
                }
                else
                {
                    lbl.Width = valueWidth;
                    lbl.Location = new Point(lbl.Location.X - minDiff, lbl.Location.Y);
                }
            }

            while (a < this.labels.Count)
                this.labels[a++].Hide();
        }

        private Label NewLabel(bool text)
        {
            Label label = new Label();

            label.AutoEllipsis = true;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            label.Location = new System.Drawing.Point((text ? 3 : 69), this.labels[this.labels.Count - 1].Location.Y + 23);
            label.Size = new System.Drawing.Size((text ? 65 : 40), 23);

            this.Controls.Add(label);
            return label;
        }

        public static void ShowColonyIncome(Colony colony)
        {
            double population = 0, production = 0, gold = 0;
            int research = 0, infrastructure = 0;
            colony.GetTurnIncome(ref population, ref production, ref gold, ref research, ref infrastructure);
            colony.GetIncomeAvgs(out double avgGold, out double avgResearch, out double avgProduction, out double avgInfrastructure);
            double actualInfr = Math.Pow(colony.GetTotalIncome() * colony.Planet.InfrastructureInc, Consts.InfrastructurePow) / colony.GetTotalIncome();
            Func<double, string> FormatPctWithPlace = d => MainForm.FormatPct(d, true);

            ShowForm(
                "Income", ShowOrig(colony.GetTotalIncome(), gold + research + production + infrastructure, MainForm.FormatDouble, MainForm.FormatDouble),
                "Upkeep", MainForm.FormatDouble(-colony.Upkeep),
                string.Empty, string.Empty,
                "Gold", ShowOrig(avgGold, gold, MainForm.FormatDouble, MainForm.FormatDouble),
                "Research", ShowOrig(avgResearch, research),
                "Production", ShowOrig(avgProduction, production),
                "Infrastructure", ShowOrig(avgInfrastructure, infrastructure),
                string.Empty, string.Empty,
                "Prod Rate", FormatPctWithPlace(colony.Planet.ProdMult),
                "Auto Infrst", ShowOrig(colony.Planet.InfrastructureInc, actualInfr, FormatPctWithPlace, FormatPctWithPlace));
        }
        private static string ShowOrig(double orig, double mod)
        {
            return ShowOrig(orig, mod, MainForm.FormatDouble, MainForm.FormatUsuallyInt);
        }
        private static string ShowOrig(double orig, double mod, Func<double, string> FormatOrig, Func<double, string> FormatMod)
        {
            string s1 = FormatOrig(orig);
            string retVal = FormatMod(mod);
            if (double.Parse(s1.TrimEnd('%')) != double.Parse(retVal.TrimEnd('%')))
                retVal = string.Format("({0}) {1}", s1, retVal);
            return retVal;
        }

        internal static void ShowShipRepair(Ship ship)
        {
            double repair = ship.MaxHP - ship.HP;
            double prod = ship.GetProdForHP(repair);
            double gold1 = ship.GetGoldForHP(1) * repair;
            double autoHP = ship.GetAutoRepairHP();
            double goldAuto = ship.GetGoldForHP(autoHP) * repair / autoHP;
            ShowForm("Production", MainForm.FormatUsuallyInt(prod),
                    "Min Gold", MainForm.FormatUsuallyInt(gold1),
                    "Auto Gold", MainForm.FormatUsuallyInt(goldAuto));
        }

        public static void ShowForm(params string[] info)
        {
            form.SetData(info);

            MainForm.GameForm.SetLocation(form);
            form.ShowDialog();
        }
    }
}
