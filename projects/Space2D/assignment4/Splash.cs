using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace assignment4
{
    public partial class Splash : Form
    {
        mainForm mainForm;

        public Splash()
        {
            InitializeComponent();

            Bounds = Screen.PrimaryScreen.Bounds;
            WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            lblStart.Text = "Loading...";
            mainForm = new mainForm(progressBar1);//load textures
            lblStart.Text = "Press any key to start!";
        }

        private void splash_KeyDown(object sender, KeyEventArgs e)
        {
            //show the main form
            mainForm.timer1.Enabled = true;
            mainForm.ShowDialog();
            mainForm.Dispose();
            this.Close();
            this.Dispose();
        }
    }
}