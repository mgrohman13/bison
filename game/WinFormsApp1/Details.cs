using ClassLibrary1;
using ClassLibrary1.Pieces.Players;
using System.Windows.Forms;

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
            Drone.Cost(Program.Game, out int de, out int dm);

            this.lblCE.Text = ce.ToString();
            this.lblCM.Text = cm.ToString();
            this.lblFE.Text = fe.ToString();
            this.lblFM.Text = fm.ToString();
            this.lblTE.Text = te.ToString();
            this.lblTM.Text = tm.ToString();
            this.lblDE.Text = de.ToString();
            this.lblDM.Text = dm.ToString();

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
