using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MattUtil.RealTimeGame
{
	public partial class NewHighScoreForm : Form
	{
		public string ScoreName
		{
			get { return this.txtName.Text; }
		}

		public NewHighScoreForm()
		{
			InitializeComponent();
		}
	}
}
