using System;
using System.Data;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class DgvForm : Form
    {
        private readonly static DgvForm Singleton = new();

        public DgvForm()
        {
            InitializeComponent();
            dataGridView1.PreviewKeyDown += DataGridView1_PreviewKeyDown;
            button1.Click += Button1_Click;
            FormClosing += DgvForm_FormClosing;
        }

        private void DataGridView1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            dataGridView1.EndEdit();
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DgvForm_FormClosing(object sender, EventArgs e)
        {
            dataGridView1.EndEdit();
        }

        private void DgvForm_AdjustWidth(object sender, EventArgs e)
        {
            this.Width = dataGridView1.PreferredSize.Width + 13;
            this.Height = dataGridView1.PreferredSize.Height + 26;
        }

        public static void ShowData(string name, DataTable data)
        {
            Singleton.Text = name;
            Singleton.dataGridView1.DataSource = data;
            for (int a = 1; a < Singleton.dataGridView1.Columns.Count; a++)
                Singleton.dataGridView1.Columns[a].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            Singleton.ShowDialog();
        }
    }
}
