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
	partial class ChangeTerrain : Form
	{
		Wizard wizard;
		public ChangeTerrain(Wizard wizard, Point location)
		{
			InitializeComponent();

			if (location.X + this.Width > Screen.PrimaryScreen.Bounds.Width)
				location.X = Screen.PrimaryScreen.Bounds.Width - this.Width;
			if (location.Y + this.Height > Screen.PrimaryScreen.Bounds.Height)
				location.Y = Screen.PrimaryScreen.Bounds.Height - this.Height;
			this.Location = location;

			this.wizard = wizard;
		}

		private void btnMountain_Click(object sender, EventArgs e)
		{
			Game.ChangeTerrain(wizard, Terrain.Mountain);
			this.Close();
		}

		private void btnWater_Click(object sender, EventArgs e)
		{
			Game.ChangeTerrain(wizard, Terrain.Water);
			this.Close();
		}

		private void btnAir_Click(object sender, EventArgs e)
		{
			Game.ChangeTerrain(wizard, Terrain.Plains);
			this.Close();
		}

		private void btnForest_Click(object sender, EventArgs e)
		{
			Game.ChangeTerrain(wizard, Terrain.Forest);
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}