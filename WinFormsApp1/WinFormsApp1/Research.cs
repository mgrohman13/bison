using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Type = ClassLibrary1.Research.Type;

namespace WinFormsApp1
{
    public partial class Research : Form
    {
        private static Research ResearchForm;

        public Research()
        {
            InitializeComponent();
        }

        public static void ShowForm(ClassLibrary1.Research.Type? researched)
        {
            if (ResearchForm == null)
                ResearchForm = new Research();

            if (ResearchForm.ShowDialog() == DialogResult.OK)
                if (ResearchForm.GetSelected().HasValue)
                {
                    Type selected = ResearchForm.GetSelected().Value;
                    if (Program.Game.Player.Research.Available.Contains(selected))
                        Program.Game.Player.Research.Researching = selected;
                }
        }

        public override void Refresh()
        {
            Type? s = GetSelected() ?? GetSelected(lbDone);
            if (s.HasValue)
            {
                Type selected = s.Value;

                this.lblName.Text = selected.ToString();
                this.lblLast.Text = Program.Game.Player.Research.GetLast(selected).ToString("0.0");
                this.lblProgress.Text = Program.Game.Player.Research.GetProgress(selected).ToString("0.0");

                if (Program.Game.Player.Research.Available.Contains(selected))
                {
                    this.lblCost.Text = Program.Game.Player.Research.GetCost(selected).ToString("0.0");
                    this.lblCost.Show();
                    this.label6.Show();
                }
                else
                {
                    this.lblCost.Hide();
                    this.label6.Hide();
                }

                this.lblName.Show();
                this.lblLast.Show();
                this.lblProgress.Show();
                this.label4.Show();
                this.label5.Show();
            }
            else
            {
                this.lblName.Hide();
                this.lblLast.Hide();
                this.lblProgress.Hide();
                this.lblCost.Hide();
                this.label4.Hide();
                this.label5.Hide();
                this.label6.Hide();
            }

            base.Refresh();
        }
        public void RefreshLB()
        {
            Type[] available = Program.Game.Player.Research.Available.OrderBy(Program.Game.Player.Research.GetCost).ToArray();
            this.lbAvailable.DataSource = available;
            this.lbDone.DataSource = Program.Game.Player.Research.Done.OrderByDescending(Program.Game.Player.Research.GetLast).ToArray();

            this.lbAvailable.SetSelected(Array.IndexOf(available, Program.Game.Player.Research.Researching), true);

            this.Refresh();
        }

        private void Research_Shown(object sender, EventArgs e)
        {
            this.RefreshLB();
        }
        private void LB_SelectedValueChanged(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedValue != null)
                if (sender == lbAvailable)
                    this.lbDone.ClearSelected();
                else
                    this.lbAvailable.ClearSelected();

            this.Refresh();
        }

        private Type? GetSelected()
        {
            return GetSelected(lbAvailable);
        }
        private static Type? GetSelected(ListBox box)
        {
            return box.SelectedValue == null ? null : (Type)box.SelectedValue;
        }
    }
}
