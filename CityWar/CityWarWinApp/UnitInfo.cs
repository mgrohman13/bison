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
        private readonly Font f1 = new("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        private readonly Font f2 = new("Arial", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        public UnitInfo(Piece piece, Point location)
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
            if (unit != null)
            {
                this.txtArmor.Text = GetModString(unit.Armor.ToString(), unit.BaseArmor.ToString());
                this.txtCost.Text = unit.RandedCost.ToString("0");
                this.txtHits.Text = string.Format("{0} / {1}", unit.Hits, unit.MaxHits);
                this.txtMove.Text = GetMoveString(unit);
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
                this.txtCost.Text = "";
                this.txtType.Text = "Passive";
                if (piece is Wizard wizard)
                {
                    this.txtCost.Text = Player.WizardCost.ToString();
                    this.txtMove.Text = GetMoveString(wizard);
                }
                else
                {
                    this.txtMove.Text = "0";
                    if (piece is Portal portal)
                    {
                        this.txtCost.Text = portal.Cost.ToString();

                        this.lblHits.Text = "Unit / Turn";
                        this.txtHits.Text = portal.UnitInc.ToString();

                        this.lblRegen.Text = "Income";
                        this.txtRegen.Text = portal.Income.ToString();

                        this.lblAttacks.Text = "Progress";
                        this.lbAttacks.Items.AddRange(portal.GetUnitValues().OrderBy(
                                pair => Unit.CreateTempUnit(pair.Key).BaseTotalCost).Select(
                                pair => string.Format("{0:000} / {1:000}   {2}", pair.Value, Unit.CreateTempUnit(pair.Key).BaseTotalCost, pair.Key)).ToArray());

                        this.lblArmor.Visible = false;
                        this.txtArmor.Visible = false;
                        this.lblMove.Visible = false;
                        this.txtMove.Visible = false;
                    }
                    else if (piece is Relic relic)
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
                            int range = (int)(Math.Ceiling(unit.MaxFuel * unit.MaxMove / (1.0 + unit.MaxMove) + 1.0) / 2.0);
                            //TODO: ?
                            int range1 = (int)(Math.Ceiling(unit.MaxFuel * unit.MaxMove / (1.0 + unit.MaxMove) + 1.0));
                            this.txtSpecial.Text += string.Format("Aircraft - Fuel {0} / {1} ({2})", unit.Fuel, unit.MaxFuel, range, range1);
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
                        case Ability.Submerged:
                            this.txtSpecial.Text += "Submerge";
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

        private static string GetMoveString(Piece piece)
        {
            int currentMove = piece.Movement;
            return (currentMove != piece.MaxMove || piece.Owner == piece.Owner.Game.CurrentPlayer)
                    ? string.Format("{0} / {1}", currentMove, piece.MaxMove) : piece.MaxMove.ToString();
        }
    }
}