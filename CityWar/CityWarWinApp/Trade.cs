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
    partial class Trade : Form
    {
        bool gamble;
        Player p;
        List<string> trades = new List<string>();

        public Trade(Player p)
        {
            InitializeComponent();

            gamble = false;
            CheckDisplay();

            this.p = p;
            RefVals();

            int air = 0, earth = 0, nature = 0, production = 0, death = 0,
                water = 0, wizard = 0, people = 0;
            p.GenerateIncome(ref air, ref death, ref earth, ref nature, ref production, ref water, ref wizard, ref people);

            this.lblWorkInc.Text += Math.Ceiling(p.GetTurnUpkeep()).ToString("0");

            this.lblAirInc.Text += air.ToString();
            this.lblDeathInc.Text += death.ToString();
            this.lblEarthInc.Text += earth.ToString();
            this.lblWizInc.Text += wizard.ToString();
            this.lblNatureInc.Text += nature.ToString();
            this.lblProdInc.Text += production.ToString();
            this.lblWaterInc.Text += water.ToString();
            this.lblPplInc.Text += people.ToString();
        }

        private void CheckDisplay()
        {
            foreach (Control c in this.Controls)
                if ((string)c.Tag == "gamble")
                    c.Visible = gamble;
                else if ((string)c.Tag == "trade")
                    c.Visible = !gamble;

            if (gamble)
            {
                AdjustMinMax(nudAirMin, nudAirMax, p.Air);
                AdjustMinMax(nudDeathMin, nudDeathMax, p.Death);
                AdjustMinMax(nudEarthMin, nudEarthMax, p.Earth);
                AdjustMinMax(nudNatureMin, nudNatureMax, p.Nature);
                AdjustMinMax(nudPopMin, nudPopMax, p.Population);
                AdjustMinMax(nudProdMin, nudProdMax, p.Production);
                AdjustMinMax(nudWaterMin, nudWaterMax, p.Water);
                AdjustMinMax(nudWorkMin, nudWorkMax, p.Work);
            }
        }
        private static void AdjustMinMax(NumericUpDown nudMin, NumericUpDown nudMax, int value)
        {
            SetValues(nudMin, 0, value, value);
            SetValues(nudMax, value, 1000 + value * 2, value);
        }
        private static void SetValues(NumericUpDown nud, int min, int max, int value)
        {
            nud.Minimum = int.MinValue;
            nud.Maximum = int.MaxValue;

            nud.Value = value;
            nud.Minimum = min;
            nud.Maximum = max;
        }

        void RefVals()
        {
            this.lblAir.Text = p.Air.ToString();
            this.lblDeath.Text = p.Death.ToString();
            this.lblEarth.Text = p.Earth.ToString();
            this.lblWizard.Text = p.Magic.ToString();
            this.lblNature.Text = p.Nature.ToString();
            this.lblWork.Text = p.Work.ToString();
            this.lblProd.Text = p.Production.ToString();
            this.lblWater.Text = p.Water.ToString();
            this.lblRelic.Text = p.RelicProgress.ToString();
            this.lblPpl.Text = p.Population.ToString();
        }

        private void btnProdU_Click(object sender, EventArgs e)
        {
            p.TradeProduction(true);
            RefVals();
        }

        private void btnProdD_Click(object sender, EventArgs e)
        {
            p.TradeProduction(false);
            RefVals();
        }

        private void btnDeathU_Click(object sender, EventArgs e)
        {
            p.TradeDeath(true);
            RefVals();
        }

        private void btnDeathD_Click(object sender, EventArgs e)
        {
            p.TradeDeath(false);
            RefVals();
        }

        private void btnEarthU_Click(object sender, EventArgs e)
        {
            p.TradeEarth(true);
            RefVals();
        }

        private void btnEarthD_Click(object sender, EventArgs e)
        {
            p.TradeEarth(false);
            RefVals();
        }

        private void btnWaterU_Click(object sender, EventArgs e)
        {
            p.TradeWater(true);
            RefVals();
        }

        private void btnWaterD_Click(object sender, EventArgs e)
        {
            p.TradeWater(false);
            RefVals();
        }

        private void btnNatU_Click(object sender, EventArgs e)
        {
            p.TradeNature(true);
            RefVals();
        }

        private void btnNatD_Click(object sender, EventArgs e)
        {
            p.TradeNature(false);
            RefVals();
        }

        private void btnAirU_Click(object sender, EventArgs e)
        {
            p.TradeAir(true);
            RefVals();
        }

        private void btnAirD_Click(object sender, EventArgs e)
        {
            p.TradeAir(false);
            RefVals();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPplU_Click(object sender, EventArgs e)
        {
            p.TradePopulation(true);
            RefVals();
        }

        private void btnPplD_Click(object sender, EventArgs e)
        {
            p.TradePopulation(false);
            RefVals();
        }

        private void btnGamble_Click(object sender, EventArgs e)
        {
            if (gamble)
            {
                p.GambleAir((int)nudAirMin.Value, (int)nudAirMax.Value);
                p.GambleDeath((int)nudDeathMin.Value, (int)nudDeathMax.Value);
                p.GambleEarth((int)nudEarthMin.Value, (int)nudEarthMax.Value);
                p.GambleNature((int)nudNatureMin.Value, (int)nudNatureMax.Value);
                p.GamblePopulation((int)nudPopMin.Value, (int)nudPopMax.Value);
                p.GambleProduction((int)nudProdMin.Value, (int)nudProdMax.Value);
                p.GambleWater((int)nudWaterMin.Value, (int)nudWaterMax.Value);
                p.GambleWork((int)nudWorkMin.Value, (int)nudWorkMax.Value);
                RefVals();
                CheckDisplay();
            }
            else
            {
                gamble = true;
                CheckDisplay();
            }
        }
    }
}