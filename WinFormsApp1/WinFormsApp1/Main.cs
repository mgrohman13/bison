using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        public Map MapMain => mapMain;
        public Map MapMini => mapMini;
        public Info Info => infoMain;

        public override void Refresh()
        {
            Info.Refresh();
            base.Refresh();
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Program.Game.Player.EndTurn();
                Refresh();
            }
        }
    }
}
