using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GalWarWin
{
    public partial class InputForm : Form
    {
        private static InputForm form = new InputForm();

        public InputForm()
        {
            InitializeComponent();
        }

        private void SetPrompt(string prompt)
        {
            this.lblPrompt.Text = prompt;
        }

        public static double ShowForm(string prompt)
        {
            MainForm.GameForm.SetLocation(form);

            form.SetPrompt(prompt);

            double retVal;
            if (form.ShowDialog() == DialogResult.OK && double.TryParse(form.txtInput.Text, out retVal))
                return retVal;

            return double.NaN;
        }
    }
}
