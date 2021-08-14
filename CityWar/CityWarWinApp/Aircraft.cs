using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    partial class Aircraft : Form
    {
        public Aircraft(Piece p)
        {
            InitializeComponent();
            this.pictureBox1.Image = p.Owner.GetConstPic(p.ToString());
            this.lblText.Text = "This action may cause your " + p.ToString() +
                " to die!  Continue anyways?";
        }
    }
}