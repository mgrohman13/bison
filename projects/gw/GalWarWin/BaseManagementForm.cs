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
    public abstract partial class BaseManagementForm<Info> : Form
    {
        private List<Info> items;
        private Func<IOrderedEnumerable<Info>, IOrderedEnumerable<Info>> defaultSort;
        private string sort;
        private bool reverse;

        protected abstract void ClickCell(string column, Info row);
        protected abstract Info CreateNewFrom(Info info);

        protected BaseManagementForm()
        {
            InitializeComponent();

            this.dataGridView1.Columns.Clear();
            this.dataGridView1.AutoGenerateColumns = true;

            DataGridViewCellStyle style = new DataGridViewCellStyle();
            style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.DefaultCellStyle = style;
        }

        protected void ShowManagementForm(IEnumerable<Info> infoItems, Func<IOrderedEnumerable<Info>, IOrderedEnumerable<Info>> defaultSort, string initialSort)
        {
            this.items = infoItems.ToList();
            this.defaultSort = defaultSort;
            this.sort = initialSort;
            this.reverse = false;

            dataGridView1.DataSource = null;
            SortData();

            MainForm.GameForm.SetLocation(this);
            ShowDialog();
        }

        protected void RefreshData(Info info)
        {
            MainForm.GameForm.RefreshAll();

            this.items.Remove(info);
            info = CreateNewFrom(info);
            if (info != null)
                this.items.Add(info);
            SortData();
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

                    SortData();
                }
            }
        }
        private void SortData()
        {
            var method = typeof(Info).GetMethod("Sort" + sort);
            if (method != null)
            {
                var funcType = typeof(Func<IOrderedEnumerable<Info>, IOrderedEnumerable<Info>>);
                var func = (Func<IOrderedEnumerable<Info>, IOrderedEnumerable<Info>>)Delegate.CreateDelegate(funcType, null, method);
                IEnumerable<Info> dataSource = defaultSort(func((IOrderedEnumerable<Info>)this.items.OrderBy(a => 0)));
                if (reverse)
                    dataSource = dataSource.Reverse();
                this.items = dataSource.ToList();
                this.dataGridView1.DataSource = this.items;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int columnIdx = e.ColumnIndex, rowIdx = e.RowIndex;
            if (columnIdx > -1 && rowIdx > -1)
            {
                string column = this.dataGridView1.Columns[columnIdx].Name;
                if (column != null && column.Length > 0)
                    ClickCell(column, this.items[rowIdx]);
            }
        }
    }
}
