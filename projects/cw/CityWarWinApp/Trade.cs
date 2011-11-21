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
		Player p;
		List<string> trades = new List<string>();

		public Trade(Player p)
		{
			InitializeComponent();
			this.p = p;
			RefVals();

			int air = 0, earth = 0, nature = 0, production = 0, death = 0,
				water = 0, work = 0, wizard = 0, relic = 0, people = 0;
			p.GenerateIncome(ref air, ref death, ref earth, ref nature, ref production, ref water, ref work, ref wizard, ref relic, ref people, false);

			this.lblWorkInc.Text += Math.Ceiling(p.GetTurnUpkeep()).ToString("0");
			this.lblAirInc.Text += ((double)((double)air)).ToString("0");
			this.lblDeathInc.Text += ((double)((double)death)).ToString("0");
			this.lblEarthInc.Text += ((double)((double)earth)).ToString("0");
			this.lblWizInc.Text += ((double)((double)wizard)).ToString("0");
			this.lblNatureInc.Text += ((double)((double)nature)).ToString("0");
			this.lblProdInc.Text += ((double)((double)production)).ToString("0");
			this.lblWaterInc.Text += ((double)((double)water)).ToString("0");
			this.lblRelicInc.Text += ((double)((double)relic)).ToString("0");
			this.lblPplInc.Text += ((double)((double)people)).ToString("0");
		}

		void RefVals()
		{
			this.lblAir.Text = p.GetResource("Air").ToString("0");
			this.lblDeath.Text = p.GetResource("Death").ToString("0");
			this.lblEarth.Text = p.GetResource("Earth").ToString("0");
			this.lblWizard.Text = p.GetResource("Magic").ToString("0");
			this.lblNature.Text = p.GetResource("Nature").ToString("0");
			this.lblWork.Text = p.GetResource("Work").ToString("0");
			this.lblProd.Text = p.GetResource("Production").ToString("0");
			this.lblWater.Text = p.GetResource("Water").ToString("0");
			this.lblRelic.Text = p.GetResource("Relic").ToString("0");
			this.lblPpl.Text = p.GetResource("Population").ToString("0");
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
	}
}