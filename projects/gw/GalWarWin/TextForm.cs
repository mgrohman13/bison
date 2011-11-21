using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class TextForm : Form
    {
        private static TextForm form = new TextForm();

        private TextForm()
        {
            InitializeComponent();

            this.textBox1.GotFocus += new EventHandler(textBox1_GotFocus);
        }

        private void textBox1_GotFocus(object sender, EventArgs e)
        {
            form.textBox1.ScrollToCaret();
        }

        private void TextForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                textBox1.SelectAll();
        }

        public static void ShowDialog(MainForm gameForm)
        {
            form.textBox1.Text = gameForm.GetLog();
            form.textBox1.Select(form.textBox1.Text.Length, 0);

            form.ShowDialog();
        }

        public static void ShowDialog(MainForm gameForm, List<Game.Result> results)
        {
            form.textBox1.Text = string.Empty;
            foreach (Game.Result result in results)
                form.textBox1.Text += result.Player.Name + " - " + result.Points.ToString() + "\r\n";
            form.textBox1.SelectAll();

            form.Size = new Size(300, 300);
            gameForm.SetLocation(form);
            form.ShowDialog();
        }

        private void TextForm_Load(object sender, EventArgs e)
        {
            if (this.Width != 300 || this.Height != 300)
            {
                Rectangle screen = Screen.FromControl(this).WorkingArea;
                this.Location = new Point(( screen.Width - this.Width ) / 2, 0);
                this.Height = screen.Height;
            }
        }
    }
}
