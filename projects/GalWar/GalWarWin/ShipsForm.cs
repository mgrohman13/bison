using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using GalWar;
using GalWarWin.Sliders;

namespace GalWarWin
{
    public class ShipsForm : BaseManagementForm<ShipsForm.ShipInfo>
    {
        private static ShipsForm form = new ShipsForm();

        private ShipsForm()
            : base()
        {
            this.Width = 850;
        }

        public static void ShowForm()
        {
            IEnumerable<ShipInfo> items = MainForm.Game.CurrentPlayer.GetShips().Select(ship => new ShipInfo(ship));
            form.ShowManagementForm(items, ShipInfo.SortLocation, "Location");
        }

        protected override void ClickCell(string column, ShipsForm.ShipInfo row)
        {
            Ship ship = row.Class;
            switch (column)
            {
            case "Location":
                MainForm.GameForm.SelectTile(ship.Tile);
                MainForm.GameForm.Center();
                MainForm.GameForm.RefreshAll();
                this.Close();
                break;
            case "Class":
                string designName = ship.ToString();
                ShipDesign shipDesign = ship.Player.GetShipDesigns().SingleOrDefault(design => designName == design.ToString());
                if (shipDesign != null)
                    CostCalculatorForm.ShowForm(shipDesign);
                break;
            case "HP":
            case "Repair":
                if (ship.HP < ship.MaxHP)
                    if (!ship.HasRepaired && column != "Repair")
                    {
                        int HP = SliderForm.ShowForm(new GoldRepair(ship));
                        if (HP > 0)
                        {
                            ship.GoldRepair(MainForm.GameForm, HP);
                            RefreshData(row);
                        }
                    }
                    else if (AutoRepairForm.ShowForm(ship))
                    {
                        RefreshData(row);
                    }
                break;
            case "Disband":
                if (MainForm.GameForm.DisbandShip(ship))
                    RefreshData(row);
                break;
            default:
                CostCalculatorForm.ShowForm(ship);
                break;
            }
        }
        protected override ShipInfo CreateNewFrom(ShipInfo info)
        {
            Ship ship = info.Class;
            if (!ship.Dead)
                return new ShipInfo(ship);
            return null;
        }

        public class ShipInfo
        {
            private Ship ship;

            public ShipInfo(Ship ship)
            {
                this.ship = ship;

                SetLocation();
                SetClass();
                SetAtt();
                SetDef();
                SetHP();
                SetRepair();
                SetSpeed();
                SetTroops();
                SetSpecial();
                SetUpk();
                SetExp();
                SetNext();
                SetProd();
                SetResearch();
                SetCost();
                SetStrength();
                SetValue();
            }

            public string Location
            {
                get;
                private set;
            }
            private void SetLocation()
            {
                Location = ship.Tile.GetLoction();
            }
            public static IOrderedEnumerable<ShipInfo> SortLocation(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenBy(info => info.ship.Tile.Y).ThenBy(info => info.ship.Tile.X);
            }

            private int sortClass;
            public Ship Class
            {
                get
                {
                    return ship;
                }
            }
            private void SetClass()
            {
                sortClass = ship.GetClassSort();
            }
            public static IOrderedEnumerable<ShipInfo> SortClass(IOrderedEnumerable<ShipInfo> items)
            {
                return SortExp(items.ThenBy(info => info.sortClass));
            }

            public string Att
            {
                get;
                private set;
            }
            private void SetAtt()
            {
                Att = ship.Att.ToString();
            }
            public static IOrderedEnumerable<ShipInfo> SortAtt(IOrderedEnumerable<ShipInfo> items)
            {
                return SortHP(items.ThenByDescending(info => info.ship.Att));
            }

            public string Def
            {
                get;
                private set;
            }
            private void SetDef()
            {
                Def = ship.Def.ToString();
            }
            public static IOrderedEnumerable<ShipInfo> SortDef(IOrderedEnumerable<ShipInfo> items)
            {
                return SortHP(items.ThenByDescending(info => info.ship.Def));
            }

            private double sortHP;
            private double sortPct;
            public string HP
            {
                get;
                private set;
            }
            private void SetHP()
            {
                sortHP = ShipDesign.GetStatValue(ship.Att) + ShipDesign.GetStatValue(ship.Def);
                sortPct = ship.HP / (double)ship.MaxHP;
                HP = ship.HP.ToString() + " / " + ship.MaxHP.ToString();
                if (ship.HP < ship.MaxHP)
                {
                    HP += " (" + MainForm.FormatPctWithCheck(sortPct) + ")";
                    if (!ship.HasRepaired)
                        HP += " +";
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortHP(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.MaxHP).ThenByDescending(info => info.ship.HP).ThenByDescending(info => info.sortHP);
            }
            private static IOrderedEnumerable<ShipInfo> SortPct(IOrderedEnumerable<ShipInfo> items)
            {
                return SortHP(items.ThenByDescending(info => info.sortPct));
            }

            private double sortRepair;
            public string Repair
            {
                get;
                private set;
            }
            private void SetRepair()
            {
                sortRepair = ship.AutoRepair;
                Repair = string.Empty;
                if (ship.HP < ship.MaxHP)
                    if (double.IsNaN(sortRepair))
                    {
                        sortRepair = double.PositiveInfinity;
                        Repair = "M";
                    }
                    else if (sortRepair != 0)
                    {
                        Repair = sortRepair.ToString(Sliders.AutoRepairForm.GetFloatErrorPrecisionFormat());
                    }
                Colony repairedFrom = ship.GetRepairedFrom();
                if (repairedFrom != null)
                    Repair = string.Format("(+{1}) {0}", Repair, MainForm.FormatDouble(ship.GetHPForProd(repairedFrom.GetProductionIncome())));
            }
            public static IOrderedEnumerable<ShipInfo> SortRepair(IOrderedEnumerable<ShipInfo> items)
            {
                return SortPct(items.ThenByDescending(info => info.sortRepair));
            }

            public string Speed
            {
                get;
                private set;
            }
            private void SetSpeed()
            {
                Speed = ship.CurSpeed.ToString() + " / " + ship.MaxSpeed.ToString();
            }
            public static IOrderedEnumerable<ShipInfo> SortSpeed(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.MaxSpeed).ThenByDescending(info => info.ship.CurSpeed);
            }

            private double sortTroops;
            private double sortPop1, sortPop2;
            public string Troops
            {
                get;
                private set;
            }
            private void SetTroops()
            {
                sortTroops = ship.GetSoldierPct();
                sortPop1 = ship.Population * ship.MaxSpeed;
                sortPop2 = ship.MaxPop * ship.MaxSpeed;
                string sldrs;
                if (ship.Population > 0)
                    sldrs = " (" + MainForm.FormatPct(sortTroops) + ")";
                else
                    sldrs = string.Empty;
                if (ship.MaxPop > 0)
                    Troops = ship.Population.ToString() + " / " + ship.MaxPop.ToString() + sldrs;
                else
                    Troops = string.Empty;

            }
            public static IOrderedEnumerable<ShipInfo> SortTroops(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.Population).ThenByDescending(info => info.sortTroops).ThenByDescending(info => info.sortPop1)
                        .ThenByDescending(info => info.sortPop2).ThenByDescending(info => info.ship.MaxPop);
            }

            private double sortSpecial;
            private double bombardDamage;
            public string Special
            {
                get;
                private set;
            }
            private void SetSpecial()
            {
                bombardDamage = ship.BombardDamage;
                sortSpecial = ( ship.Colony ? ship.ColonizationValue : bombardDamage * ship.MaxSpeed );
                if (ship.Colony)
                    Special = "Colony Ship (" + MainForm.FormatDouble(sortSpecial) + ")";
                else if (ship.DeathStar)
                    Special = "Death Star (" + MainForm.FormatInt(bombardDamage) + ")";
                else
                    Special = string.Empty;
            }
            public static IOrderedEnumerable<ShipInfo> SortSpecial(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.Colony).ThenByDescending(info => info.sortSpecial).ThenByDescending(info => info.bombardDamage);
            }

            private double sortUpk;
            public string Upk
            {
                get;
                private set;
            }
            private void SetUpk()
            {
                sortUpk = ship.Upkeep;
                Upk = ship.BaseUpkeep.ToString();
            }
            public static IOrderedEnumerable<ShipInfo> SortUpk(IOrderedEnumerable<ShipInfo> items)
            {
                return SortValue(items.ThenByDescending(info => info.ship.BaseUpkeep).ThenByDescending(info => info.sortUpk));
            }

            private int sortExp;
            public string Exp
            {
                get;
                private set;
            }
            private void SetExp()
            {
                sortExp = ship.GetTotalExp();
                Exp = sortExp.ToString();
            }
            public static IOrderedEnumerable<ShipInfo> SortExp(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.sortExp).ThenBy(info => info.Next);
            }

            public string Next
            {
                get;
                private set;
            }
            private void SetNext()
            {
                Next = ship.NextExpType.ToString();
            }
            public static IOrderedEnumerable<ShipInfo> SortNext(IOrderedEnumerable<ShipInfo> items)
            {
                return SortExp(items.ThenBy(info => info.Next));
            }

            public string Disband
            {
                get
                {
                    return "+";
                }
            }

            private double sortProd;
            public string Prod
            {
                get;
                private set;
            }
            private void SetProd()
            {
                sortProd = ship.GetProdForHP(ship.MaxHP);
                Prod = MainForm.FormatDouble(sortProd / Consts.RepairCostMult);
            }
            public static IOrderedEnumerable<ShipInfo> SortProd(IOrderedEnumerable<ShipInfo> items)
            {
                return SortUpk(items.ThenByDescending(info => info.sortProd));
            }

            private int sortResearch;
            public string Research
            {
                get;
                private set;
            }
            private void SetResearch()
            {
                sortResearch = CostCalculatorForm.CalcResearch(ship);
                Research = sortResearch.ToString();
            }
            public static IOrderedEnumerable<ShipInfo> SortResearch(IOrderedEnumerable<ShipInfo> items)
            {
                return SortValue(items.ThenByDescending(info => info.sortResearch));
            }

            private double sortCost = double.NaN;
            public string Cost
            {
                get;
                private set;
            }
            private void SetCost()
            {
                sortCost = ShipDesign.GetTotCost(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, sortResearch);
                Cost = MainForm.FormatDouble(sortCost);
            }
            public static IOrderedEnumerable<ShipInfo> SortCost(IOrderedEnumerable<ShipInfo> items)
            {
                return SortUpk(items.ThenByDescending(info => info.sortCost));
            }

            private double sortStrength = double.NaN;
            public string Strength
            {
                get;
                private set;
            }
            private void SetStrength()
            {
                sortStrength = ShipDesign.GetStrength(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed);
                Strength = GraphsForm.GetArmadaString(sortStrength);
            }
            public static IOrderedEnumerable<ShipInfo> SortStrength(IOrderedEnumerable<ShipInfo> items)
            {
                return SortValue(items.ThenByDescending(info => info.sortStrength));
            }

            private double sortValue = double.NaN;
            public string Value
            {
                get;
                private set;
            }
            private void SetValue()
            {
                sortValue = ShipDesign.GetValue(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, MainForm.Game);
                Value = GraphsForm.GetArmadaString(sortValue);
            }
            public static IOrderedEnumerable<ShipInfo> SortValue(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.sortValue);
            }
        }
    }
}
