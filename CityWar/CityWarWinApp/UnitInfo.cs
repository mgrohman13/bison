using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    partial class UnitInfo : Form
    {
        Font f1 = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        Font f2 = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        public UnitInfo(Piece piece, Point location, int currentMove)
        {
            InitializeComponent();

            Rectangle bounds = Map.GetScreenBounds();
            if (location.X + this.Width > bounds.Width)
                location.X = bounds.Width - this.Width;
            if (location.Y + this.Height > bounds.Height)
                location.Y = bounds.Height - this.Height;
            this.Location = location;

            this.txtUnit.Text = piece.ToString();

            Unit unit = piece as Unit;
            City city = piece as City;
            Portal portal = piece as Portal;
            Relic relic = piece as Relic;
            Wizard wizard = piece as Wizard;
            if (unit != null)
            {
                this.txtArmor.Text = GetModString(unit.Armor.ToString(), unit.BaseArmor.ToString());
                this.txtCost.Text = unit.RandedCost.ToString("0");
                this.txtHits.Text = string.Format("{0} / {1}", unit.Hits, unit.MaxHits);
                this.txtMove.Text = GetMoveString(currentMove, unit);
                int regen = unit.Regen;
                this.txtRegen.Font = (unit.IsAir() && !unit.Tile.HasCarrier()) ? f2 : f1;
                this.txtRegen.Text = GetModString(regen.ToString(), unit.MaxRegen.ToString());
                if (!unit.RegenRecover)
                    this.txtRegen.Text += " -";
                this.txtType.Text = unit.Type.ToString();

                this.lbAttacks.Items.AddRange(unit.Attacks);
            }
            else
            {
                this.txtType.Text = "Passive";
                if (wizard != null)
                {
                    this.txtCost.Text = Player.WizardCost.ToString();
                    this.txtMove.Text = GetMoveString(currentMove, wizard);
                }
                else
                {
                    this.txtMove.Text = "0";
                    if (portal != null)
                    {
                        this.txtCost.Text = portal.TotalCost.ToString();

                        this.lblHits.Text = "Pts / Turn";
                        this.txtHits.Text = portal.GetTurnInc().ToString("0.0");

                        this.lblRegen.Text = "Income";
                        this.txtRegen.Text = portal.Income.ToString();

                        this.lblAttacks.Text = "Summons";
                        this.lbAttacks.Items.AddRange(portal.GetUnitValues().OrderBy(
                                pair => Unit.CreateTempUnit(Map.Game, pair.Key).BaseTotalCost).Select(
                                pair => string.Format("{0:000} / {1:000}   {2}", pair.Value, Unit.CreateTempUnit(Map.Game, pair.Key).BaseTotalCost, pair.Key)).ToArray());

                        this.lblArmor.Visible = false;
                        this.txtArmor.Visible = false;
                        this.lblMove.Visible = false;
                        this.txtMove.Visible = false;
                    }
                    else if (relic != null)
                    {
                        this.txtCost.Text = Player.RelicCost.ToString();
                    }
                }
            }

            this.Height = piece.Abilities == Ability.None ? 180 : 250;
            this.txtSpecial.Clear();
            foreach (Ability a in piece.Abilities)
                if (a != Ability.None)
                {
                    if (this.txtSpecial.Text.Length > 0)
                        this.txtSpecial.Text += "\r\n";
                    switch (a)
                    {
                        case Ability.Aircraft:
                            this.txtSpecial.Text += string.Format("Aircraft - Fuel {0} / {1}", unit.Fuel, unit.MaxFuel);
                            break;
                        case Ability.AircraftCarrier:
                            this.txtSpecial.Text += "Aircraft Carrier";
                            break;
                        case Ability.Shield:
                            this.txtSpecial.Text += string.Format("Shield - {0}%", unit.Shield);
                            break;
                        case Ability.Regen:
                            this.txtSpecial.Text += "Regenerates";
                            break;
                        default:
                            throw new Exception();
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

        private static string GetMoveString(int currentMove, Piece piece)
        {
            if (currentMove == -1)
                currentMove = piece.Movement;
            return (currentMove != piece.MaxMove || piece.Owner == piece.Owner.Game.CurrentPlayer)
                    ? string.Format("{0} / {1}", currentMove, piece.MaxMove) : piece.MaxMove.ToString();
        }
    }
}