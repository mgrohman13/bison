using System;
using System.Windows.Forms;

namespace NCWMap
{
    public partial class Balance : Form
    {
        public Balance()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Bal.GetOutput(this.textBox1.Text);
        }
    }
}
