using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CityWarWinApp
{
	partial class UnitInfo : Form
	{
		public UnitInfo(CityWar.Piece piece, Point location, int currentMove)
		{
			InitializeComponent();

			if (location.X + this.Width > Screen.PrimaryScreen.Bounds.Width)
				location.X = Screen.PrimaryScreen.Bounds.Width - this.Width;
			if (location.Y + this.Height > Screen.PrimaryScreen.Bounds.Height)
				location.Y = Screen.PrimaryScreen.Bounds.Height - this.Height;
			this.Location = location;

			this.txtUnit.Text = piece.Name;

			CityWar.Unit unit = piece as CityWar.Unit;
			CityWar.City city = piece as CityWar.City;
			CityWar.Portal portal = piece as CityWar.Portal;
			CityWar.Relic relic = piece as CityWar.Relic;
			CityWar.Wizard wizard = piece as CityWar.Wizard;
			if (unit != null)
			{
				this.txtArmor.Text = GetModString(unit.Armor.ToString(), unit.BaseArmor.ToString());
				this.txtCost.Text = unit.RandedCost.ToString("0");
				//this.txtCostType.Text = u.costType.ToString();
				this.txtHits.Text = string.Format("{0} / {1}", unit.Hits, unit.maxHits);
				this.txtMove.Text = GetMoveString(currentMove, unit.MaxMove);
				this.txtRegen.Text = GetModString(unit.Regen.ToString(), unit.BaseRegen.ToString());
				this.txtType.Text = unit.Type.ToString();

				this.lbAttacks.Items.AddRange(unit.Attacks);
			}
			else
			{
				this.txtType.Text = "Passive";
				if (wizard != null)
				{
					this.txtCost.Text = CityWar.Player.WizardCost.ToString();
					this.txtMove.Text = GetMoveString(currentMove, wizard.MaxMove);
				}
				else
				{
					this.txtMove.Text = "0";
					if (portal != null)
					{
						this.txtCost.Text = portal.TotalCost.ToString();

						this.lblHits.Text = "Pts / Turn";
						this.txtHits.Text = portal.GetTurnInc().ToString("0");

						this.lblRegen.Text = "Income";
						this.txtRegen.Text = portal.Income.ToString("0");

						Dictionary<string, double> units = portal.getUnitValues();
						foreach (string name in units.Keys)
							this.lbAttacks.Items.Add(name + "   " + units[name].ToString("0") + " / " + CityWar.Unit.CreateTempUnit(name).BaseCost);

						this.lblArmor.Visible = false;
						this.txtArmor.Visible = false;
						this.lblMove.Visible = false;
						this.txtMove.Visible = false;
					}
					else if (relic != null)
					{
						this.txtCost.Text = CityWar.Player.RelicCost.ToString();
					}
				}
			}
		}

		private static string GetModString(string actual, string orig)
		{
			string retVal = orig;
			if (actual != orig)
				retVal += " (" + actual + ")";
			return retVal;
		}

		private static string GetMoveString(int currentMove, int maxMove)
		{
			return (currentMove != -1) ? string.Format("{0} / {1}", currentMove, maxMove) : maxMove.ToString();
		}
	}
}