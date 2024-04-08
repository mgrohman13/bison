using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassLibrary1;
using ClassLibrary1.Pieces.Players;

namespace WinFormsApp1
{
    public partial class Details : Form
    {
        private static readonly Details form = new();

        public Details()
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
            Constructor.Cost(Program.Game, out int ce, out int cm);
            Factory.Cost(Program.Game, out int fe, out int fm);
            Turret.Cost(Program.Game, out int te, out int tm);

            this.lblCE.Text = ce.ToString();
            this.lblCM.Text = cm.ToString();
            this.lblFE.Text = fe.ToString();
            this.lblFM.Text = fm.ToString();
            this.lblTE.Text = te.ToString();
            this.lblTM.Text = tm.ToString();

            this.txtVictory.Text = $"{Program.Game.Victory}/{Game.POINTS_TO_WIN}";

            base.Refresh();
        }

        internal static void ShowForm()
        {
            form.Refresh();
            form.ShowDialog();
        }
    }
}
