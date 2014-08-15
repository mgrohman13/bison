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

        private IOrderedEnumerable<ShipInfo> items;
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
            form.dataGridView1.DataSource = null;
        }

        private void LoadData()
        {
            this.items = MainForm.Game.CurrentPlayer.GetShips().Select(ship => new ShipInfo(ship)).ToList().OrderBy(ship => 0);

            this.sort = "Location";
            this.reverse = false;
            SortData(ShipInfo.SortLocation);
        }
        private void SortData(Func<IOrderedEnumerable<ShipInfo>, IOrderedEnumerable<ShipInfo>> Sort)
        {
            IEnumerable<ShipInfo> dataSource = ShipInfo.SortLocation(Sort(this.items));
            if (reverse)
                dataSource = dataSource.Reverse();
            this.dataGridView1.DataSource = dataSource.ToList();
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
                        var fucType = typeof(Func<IOrderedEnumerable<ShipInfo>, IOrderedEnumerable<ShipInfo>>);
                        var func = (Func<IOrderedEnumerable<ShipInfo>, IOrderedEnumerable<ShipInfo>>)
                                Delegate.CreateDelegate(fucType, null, method);
                        SortData(func);
                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int columnIdx = e.ColumnIndex, rowIdx = e.RowIndex;
            if (columnIdx > -1 && rowIdx > -1)
            {
                string column = this.dataGridView1.Columns[columnIdx].Name;
                if (column != null && column.Length > 0)
                {
                    Ship row = ( (IList<ShipInfo>)this.dataGridView1.DataSource )[rowIdx].Class;

                    Console.WriteLine(column);
                    Console.WriteLine(row);

                    //"Location";
                    //"HP";
                    //"Pct";
                    //"Repair";
                    //"Disband?";
                }
            }
        }

        private class ShipInfo
        {
            private Ship ship;

            public ShipInfo(Ship ship)
            {
                this.ship = ship;
                //ensure we calculate research now
                string research = this.Research;
            }

            private string location = null;
            public string Location
            {
                get
                {
                    if (location == null)
                        location = MainForm.GetLoction(ship.Tile);
                    return location;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortLocation(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenBy(info => info.ship.Tile.Y).ThenBy(info => info.ship.Tile.X);
            }

            private string className = null;
            public Ship Class
            {
                get
                {
                    if (className == null)
                        className = ship.ToString();
                    return ship;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortClass(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenBy(info => info.className);
            }

            private string att = null;
            public string Att
            {
                get
                {
                    if (att == null)
                        att = ship.Att.ToString();
                    return att;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortAtt(IOrderedEnumerable<ShipInfo> items)
            {
                return SortHP(items.ThenByDescending(info => info.ship.Att));
            }

            private string def = null;
            public string Def
            {
                get
                {
                    if (def == null)
                        def = ship.Def.ToString();
                    return def;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortDef(IOrderedEnumerable<ShipInfo> items)
            {
                return SortHP(items.ThenByDescending(info => info.ship.Def));
            }

            private string hp = null;
            private double sortHP = double.NaN;
            public string HP
            {
                get
                {
                    if (hp == null)
                    {
                        sortHP = ShipDesign.GetStatValue(ship.Att) + ShipDesign.GetStatValue(ship.Def);
                        hp = ship.HP.ToString() + " / " + ship.MaxHP.ToString();
                    }
                    return hp;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortHP(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.MaxHP).ThenByDescending(info => info.ship.HP)
                        .ThenByDescending(info => info.sortHP);
            }

            private string pct = null;
            private double sortPct = double.NaN;
            public string Pct
            {
                get
                {
                    if (pct == null)
                    {
                        sortPct = ship.HP / (double)ship.MaxHP;
                        pct = MainForm.FormatPctWithCheck(sortPct);
                    }
                    return pct;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortPct(IOrderedEnumerable<ShipInfo> items)
            {
                return SortHP(items.ThenByDescending(info => info.sortPct).ThenByDescending(info => info.ship.HP));
            }

            private string repair = null;
            private double sortRepair = double.NaN;
            public string Repair
            {
                get
                {
                    if (repair == null)
                    {
                        sortRepair = ship.AutoRepair;
                        repair = string.Empty;
                        if (ship.HP < ship.MaxHP)
                            if (double.IsNaN(sortRepair))
                            {
                                sortRepair = double.PositiveInfinity;
                                repair = "M";
                            }
                            else if (sortRepair != 0)
                            {
                                repair = MainForm.FormatDouble(sortRepair);
                            }
                        Colony repairedFrom = ship.GetRepairedFrom();
                        if (repairedFrom != null)
                            repair = string.Format("(+{1}) {0}", repair,
                                    MainForm.FormatDouble(ship.GetHPForProd(repairedFrom.GetProductionIncome())));
                    }
                    return repair;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortRepair(IOrderedEnumerable<ShipInfo> items)
            {
                return SortPct(items.ThenByDescending(info => info.sortRepair));
            }

            private string speed = null;
            public string Speed
            {
                get
                {
                    if (speed == null)
                        speed = ship.CurSpeed.ToString() + " / " + ship.MaxSpeed.ToString();
                    return speed;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortSpeed(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.MaxSpeed).ThenByDescending(info => info.ship.CurSpeed);
            }

            private string upk = null;
            private double sortUpk = double.NaN;
            public string Upk
            {
                get
                {
                    if (upk == null)
                    {
                        sortUpk = ship.Upkeep;
                        upk = ship.BaseUpkeep.ToString();
                    }
                    return upk;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortUpk(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.BaseUpkeep).ThenByDescending(info => info.sortUpk);
            }

            private string exp = null;
            private int sortExp = int.MinValue;
            public string Exp
            {
                get
                {
                    if (exp == null)
                    {
                        sortExp = ship.GetTotalExp();
                        exp = sortExp.ToString();
                    }
                    return exp;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortExp(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.sortExp).ThenBy(info => info.next);
            }

            private string next = null;
            public string Next
            {
                get
                {
                    if (next == null)
                        next = ship.NextExpType.ToString();
                    return next;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortNext(IOrderedEnumerable<ShipInfo> items)
            {
                return SortExp(items.ThenBy(info => info.next));
            }

            private string troops = null;
            public string Troops
            {
                get
                {
                    if (troops == null)
                        if (ship.MaxPop > 0)
                            troops = ship.Population.ToString() + " / " + ship.MaxPop.ToString();
                        else
                            troops = string.Empty;
                    return troops;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortTroops(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.ship.Population).ThenByDescending(info => info.ship.MaxPop)
                        .ThenByDescending(info => info.ship.Soldiers);
            }

            private string sldrs = null;
            public string Sldrs
            {
                get
                {
                    if (sldrs == null)
                        if (ship.Population > 0)
                            sldrs = MainForm.FormatPct(ship.GetSoldierPct());
                        else
                            sldrs = string.Empty;
                    return sldrs;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortSldrs(IOrderedEnumerable<ShipInfo> items)
            {
                return SortTroops(items.ThenByDescending(info => info.ship.Soldiers));
            }

            private string info = null;
            private double sortInfo = double.NaN;
            public string Info
            {
                get
                {
                    if (info == null)
                    {
                        sortInfo = ( ship.Colony ? ship.ColonizationValue : ship.BombardDamage * ship.MaxSpeed );
                        if (ship.Colony)
                            info = "Colony Ship (" + MainForm.FormatDouble(sortInfo) + ")";
                        else if (ship.DeathStar)
                            info = "Death Star (" + MainForm.FormatInt(sortInfo) + ")";
                        else
                            info = string.Empty;
                    }
                    return info;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortInfo(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenBy(info => info.ship.Colony ? 0 : 1).ThenByDescending(info => info.sortInfo);
            }

            private string prod = null;
            private double sortProd = double.NaN;
            public string Prod
            {
                get
                {
                    if (prod == null)
                    {
                        sortProd = ship.GetProdForHP(ship.MaxHP);
                        prod = MainForm.FormatDouble(sortProd / Consts.RepairCostMult);
                    }
                    return prod;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortProd(IOrderedEnumerable<ShipInfo> items)
            {
                return SortUpk(items.ThenByDescending(info => info.sortProd));
            }

            private string research = null;
            private int sortResearch = int.MinValue;
            public string Research
            {
                get
                {
                    if (research == null)
                    {
                        sortResearch = CostCalculatorForm.CalcResearch(ship);
                        research = sortResearch.ToString();
                    }
                    return research;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortResearch(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.sortResearch);
            }

            private string cost = null;
            private double sortCost = double.NaN;
            public string Cost
            {
                get
                {
                    if (cost == null)
                    {
                        sortCost = ShipDesign.GetTotCost(ship.Att, ship.Def, ship.MaxHP,
                                ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, sortResearch);
                        cost = MainForm.FormatDouble(sortCost);
                    }
                    return cost;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortCost(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.sortCost);
            }

            private string strength = null;
            private double sortStrength = double.NaN;
            public string Strength
            {
                get
                {
                    if (strength == null)
                    {
                        sortStrength = ShipDesign.GetStrength(ship.Att, ship.Def, ship.MaxHP, ship.MaxSpeed);
                        strength = GraphsForm.GetArmadaString(sortStrength);
                    }
                    return strength;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortStrength(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.sortStrength);
            }

            private string value = null;
            private double sortValue = double.NaN;
            public string Value
            {
                get
                {
                    if (value == null)
                    {
                        sortValue = ShipDesign.GetValue(ship.Att, ship.Def, ship.MaxHP,
                                ship.MaxSpeed, ship.MaxPop, ship.Colony, ship.BombardDamage, sortResearch);
                        value = GraphsForm.GetArmadaString(sortValue);
                    }
                    return value;
                }
            }
            public static IOrderedEnumerable<ShipInfo> SortValue(IOrderedEnumerable<ShipInfo> items)
            {
                return items.ThenByDescending(info => info.sortValue);
            }
        }
    }
}
