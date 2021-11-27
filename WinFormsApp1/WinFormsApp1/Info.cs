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
using ClassLibrary1.Pieces;
using Tile = ClassLibrary1.Map.Tile;

namespace WinFormsApp1
{
    public partial class Info : UserControl
    {
        private Tile selected;

        public Info()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.Game.Player.EndTurn();
            Program.Form.Refresh();
        }

        internal void SetSelected(Tile selected)
        {
            this.selected = selected;
        }

        public override void Refresh()
        {
            if (selected != null)
            {
                IMovable movable = selected.Piece as IMovable;

                if (movable != null)
                {
                    this.lbl1.Show();
                    this.lblInf1.Show();

                    this.lbl1.Text = "Move";
                    this.lblInf1.Text = string.Format("{0} / {1} / {2} +{3}",
                        movable.MoveCur.ToString("0.0"), movable.MoveMax.ToString("0.0"), movable.MoveLimit.ToString("0.0"), movable.MoveInc.ToString("0.0"));
                }
                else
                {
                    this.lbl1.Hide();
                    this.lblInf1.Hide();
                }
            }

            base.Refresh();
        }
    }
}
