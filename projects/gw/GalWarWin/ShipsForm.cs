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

namespace GalWarWin
{
    public partial class ShipsForm : Form
    {
        private static ShipsForm form = new ShipsForm();

        private static Dictionary<Ship, int> researchCache = new Dictionary<Ship, int>();

        private string sort;
        private bool reverse;

        public ShipsForm()
        {
            InitializeComponent();

            this.dataGridView1.Columns.Clear();
            this.dataGridView1.AutoGenerateColumns = true;

            DataGridViewCellStyle style = new DataGridViewCellStyle();
            style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.DefaultCellStyle = style;
        }

        public static void ShowForm()
        {
            form.LoadData();

            MainForm.GameForm.SetLocation(form);

            form.ShowDialog();
        }

        private void LoadData()
        {
            researchCache.Clear();

            this.sort = "Location";
            this.reverse = false;

            LoadData(ShipInfo.SortLocation);
        }
        private void LoadData(Func<IOrderedEnumerable<Ship>, IOrderedEnumerable<Ship>> Sort)
        {
            var items = Sort(MainForm.Game.CurrentPlayer.GetShips().OrderBy(ship => 0))
                    .ThenBy(ship => ship.Tile.Y).ThenBy(ship => ship.Tile.X)
                    .Select(ship => new ShipInfo(ship));
            if (reverse)
                items = items.Reverse();
            this.dataGridView1.DataSource = items.ToList();
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int idx = e.ColumnIndex;
            if (idx > -1)
            {
                var column = this.dataGridView1.Columns[idx].Name;
                if (column != null && column.Length > 0)
                {
                    if (sort == column)
                    {
                        reverse = !reverse;
                    }
                    else
                    {
                        sort = column;
                        reverse = false;
                    }

                    var method = typeof(ShipInfo).GetMethod("Sort" + column);
                    if (method != null)
                    {
                        var fucType = typeof(Func<IOrderedEnumerable<Ship>, IOrderedEnumerable<Ship>>);
                        var func = (Func<IOrderedEnumerable<Ship>, IOrderedEnumerable<Ship>>)
                                Delegate.CreateDelegate(fucType, null, method);
                        LoadData(func);
                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1)
            {
            }
        }

        private static int CalcResearch(Ship ship)
        {
            int research;
            if (!researchCache.TryGetValue(ship, out research))
            {
                research = CostCalculatorForm.CalcResearch(ship);
                researchCache.Add(ship, research);
            }
            return research;
        }

        private class ShipInfo
        {
            private Ship ship;

            public ShipInfo(Ship ship)
            {
                this.ship = ship;
            }

            public string Location
            {
                get
                {
                    return MainForm.GetLoction(ship.Tile);
                }
            }
            public static IOrderedEnumerable<Ship> SortLocation(IOrderedEnumerable<Ship> items)
            {
                return items.OrderBy(ship => 0);
            }

            public string Type
            {
                get
                {
                    return ship.ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortType(IOrderedEnumerable<Ship> items)
            {
                return items.ThenBy(ship => ship.ToString());
            }

            public string Att
            {
                get
                {
                    return ship.Att.ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortAtt(IOrderedEnumerable<Ship> items)
            {
                return SortHP(items.ThenByDescending(ship => ship.Att));
            }

            public string Def
            {
                get
                {
                    return ship.Def.ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortDef(IOrderedEnumerable<Ship> items)
            {
                return SortHP(items.ThenByDescending(ship => ship.Def));
            }

            public string HP
            {
                get
                {
                    return ship.HP.ToString() + " / " + ship.MaxHP.ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortHP(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ship.MaxHP).ThenByDescending(ship => ship.HP)
                        .ThenByDescending(ship => ShipDesign.GetStatValue(ship.Att) + ShipDesign.GetStatValue(ship.Def));
            }

            public string Pct
            {
                get
                {
                    return MainForm.FormatPctWithCheck(ship.HP / (double)ship.MaxHP);
                }
            }
            public static IOrderedEnumerable<Ship> SortPct(IOrderedEnumerable<Ship> items)
            {
                return SortHP(items.ThenByDescending(ship => ship.HP / (double)ship.MaxHP)
                        .ThenByDescending(ship => ship.HP));
            }

            public string Speed
            {
                get
                {
                    return ship.CurSpeed.ToString() + " / " + ship.MaxSpeed.ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortSpeed(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ship.MaxSpeed).ThenByDescending(ship => ship.CurSpeed);
            }

            public string Upk
            {
                get
                {
                    return ship.BaseUpkeep.ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortUpk(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ship.BaseUpkeep).ThenByDescending(ship => ship.Upkeep);
            }

            public string Exp
            {
                get
                {
                    return ship.GetTotalExp().ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortExp(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ship.GetTotalExp()).ThenBy(ship => ship.NextExpType.ToString());
            }

            public string Next
            {
                get
                {
                    return ship.NextExpType.ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortNext(IOrderedEnumerable<Ship> items)
            {
                return SortExp(items.ThenBy(ship => ship.NextExpType.ToString()));
            }

            public string Troops
            {
                get
                {
                    if (ship.MaxPop > 0)
                        return ship.Population.ToString() + " / " + ship.MaxPop.ToString();
                    return string.Empty;
                }
            }
            public static IOrderedEnumerable<Ship> SortTroops(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ship.Population).ThenByDescending(ship => ship.MaxPop)
                        .ThenByDescending(ship => ship.Soldiers);
            }

            public string Sldrs
            {
                get
                {
                    if (ship.Population > 0)
                        return MainForm.FormatPct(ship.GetSoldierPct());
                    return string.Empty;
                }
            }
            public static IOrderedEnumerable<Ship> SortSldrs(IOrderedEnumerable<Ship> items)
            {
                return SortTroops(items.ThenByDescending(ship => ship.Soldiers));
            }

            public string Info
            {
                get
                {
                    string info = string.Empty;
                    if (ship.Colony)
                    {
                        info = "Colony Ship";
                        if (ship.Player.IsTurn)
                        {
                            double colonizationValue = ship.ColonizationValue;
                            string repair = string.Empty;
                            Colony repairedFrom = ship.GetRepairedFrom();
                            if (repairedFrom != null)
                                repair = " +" + MainForm.FormatDouble(ship.GetColonizationValue(ship.GetHPForProd(repairedFrom.GetProductionIncome())) - colonizationValue);

                            info += " (" + MainForm.FormatDouble(colonizationValue) + repair + ")";

                        }
                    }
                    else if (ship.DeathStar)
                    {
                        info = "Death Star (" + MainForm.FormatInt(ship.BombardDamage) + ")";
                    }
                    return info;
                }
            }
            public static IOrderedEnumerable<Ship> SortInfo(IOrderedEnumerable<Ship> items)
            {
                return items.ThenBy(ship => ship.Colony ? 0 : 1)
                        .ThenByDescending(ship => ship.Colony ? ship.ColonizationValue : ship.BombardDamage * ship.MaxSpeed);
            }

            public string Prod
            {
                get
                {
                    double prod = ship.GetProdForHP(ship.MaxHP) / Consts.RepairCostMult;
                    return MainForm.FormatDouble(prod);
                }
            }
            public static IOrderedEnumerable<Ship> SortProd(IOrderedEnumerable<Ship> items)
            {
                return SortUpk(items.ThenByDescending(ship => ship.GetProdForHP(ship.MaxHP)));
            }

            public string Research
            {
                get
                {
                    return CalcResearch(ship).ToString();
                }
            }
            public static IOrderedEnumerable<Ship> SortResearch(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => CalcResearch(ship));
            }

            public string Cost
            {
                get
                {
                    return MainForm.FormatDouble(ShipDesign.GetTotCost(ship.Att, ship.Def, ship.MaxHP,
                            ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, CalcResearch(ship)));
                }
            }
            public static IOrderedEnumerable<Ship> SortCost(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ShipDesign.GetTotCost(ship.Att, ship.Def, ship.MaxHP,
                        ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, CalcResearch(ship)));
            }

            public string Strength
            {
                get
                {
                    return GraphsForm.GetArmadaString(ShipDesign.GetStrength(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed));
                }
            }
            public static IOrderedEnumerable<Ship> SortStrength(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ShipDesign.GetStrength(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed));
            }

            public string Value
            {
                get
                {
                    return GraphsForm.GetArmadaString(ShipDesign.GetValue(ship.Att, ship.Def, ship.MaxHP,
                            ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, CalcResearch(ship)));
                }
            }
            public static IOrderedEnumerable<Ship> SortValue(IOrderedEnumerable<Ship> items)
            {
                return items.ThenByDescending(ship => ShipDesign.GetValue(ship.Att, ship.Def, ship.MaxHP,
                        ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, CalcResearch(ship)));
            }
        }
    }
}
