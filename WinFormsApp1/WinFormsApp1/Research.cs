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
            this.Height = 600;
        }

        public static void ShowForm()
        {
            if (ResearchForm == null)
                ResearchForm = new Research();

            if (ResearchForm.ShowDialog() == DialogResult.OK && ResearchForm.GetSelected().HasValue)
            {
                Type selected = ResearchForm.GetSelected().Value;
                if (Program.Game.Player.Research.Available.Contains(selected))
                    Program.Game.Player.Research.Researching = selected;
            }
        }

        public override void Refresh()
        {
            this.lblTotal.Text = Program.Game.Player.Research.ResearchCur.ToString();

            Type? s = GetSelected() ?? GetSelected(lbxDone);
            if (s.HasValue)
            {
                Type selected = s.Value;

                this.lblName.Text = selected.ToString();
                this.lblLast.Text = Program.Game.Player.Research.GetLast(selected).ToString();
                this.lblProgress.Text = Program.Game.Player.Research.GetProgress(selected).ToString();

                if (Program.Game.Player.Research.Available.Contains(selected))
                {
                    this.lblCost.Text = Program.Game.Player.Research.GetCost(selected).ToString();
                    this.lblCost.Show();
                    this.label6.Show();
                }
                else
                {
                    this.lblCost.Hide();
                    this.label6.Hide();
                }

                IEnumerable<Type> unlocks = ClassLibrary1.Research.GetUnlocks(selected);
                if (unlocks.Any())
                {
                    bool done = Program.Game.Player.Research.Done.Contains(selected);
                    this.label3.Text = done ? "Unlocked" : "Unlocks";
                    this.label3.Font = new Font(this.label3.Font, done ? FontStyle.Italic : FontStyle.Regular);
                    this.lbxUnlocks.DataSource = unlocks.ToArray();
                    this.label3.Show();
                    this.lbxUnlocks.Show();
                }
                else
                {
                    this.lbxUnlocks.Hide();
                    this.label3.Hide();
                    this.lvwAlso.Hide();
                    this.label8.Hide();
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
            Type[] available = Program.Game.Player.Research.Available.OrderBy(t => Program.Game.Player.Research.GetCost(t) - Program.Game.Player.Research.GetProgress(t)).ToArray();
            this.lbxAvailable.DataSource = available;
            this.lbxDone.DataSource = Program.Game.Player.Research.Done.OrderByDescending(Program.Game.Player.Research.GetLast).ToArray();

            this.lbxAvailable.SetSelected(Array.IndexOf(available, Program.Game.Player.Research.Researching), true);

            this.Refresh();
        }

        private void Research_Shown(object sender, EventArgs e)
        {
            this.RefreshLB();
        }
        private void LB_SelectedValueChanged(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedValue != null)
                if (sender == lbxAvailable)
                    this.lbxDone.ClearSelected();
                else
                    this.lbxAvailable.ClearSelected();

            this.Refresh();
        }

        private Type? GetSelected()
        {
            return GetSelected(lbxAvailable);
        }
        private static Type? GetSelected(ListBox box)
        {
            return box.SelectedValue == null ? null : (Type)box.SelectedValue;
        }

        private void LbAvailable_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void LbxUnlocks_SelectedValueChanged(object sender, EventArgs e)
        {
            Type? main = GetSelected() ?? GetSelected(lbxDone);
            Type? selected = GetSelected(lbxUnlocks);

            this.lvwAlso.Hide();
            this.label8.Hide();
            if (selected.HasValue)
            {
                IEnumerable<Type> types = ClassLibrary1.Research.GetDependencies(selected.Value);
                if (main.HasValue)
                    types = types.Where(t => t != main.Value).ToArray();
                if (types.Any())
                {
                    this.lvwAlso.Clear();
                    int idx = 0;
                    foreach (Type type in types)
                    {
                        this.lvwAlso.Items.Add(type.ToString());
                        if (Program.Game.Player.Research.Done.Contains(type))
                            this.lvwAlso.Items[idx].Font = new Font(lvwAlso.Font, FontStyle.Italic);
                        idx++;
                    }
                    this.lvwAlso.Show();
                    this.label8.Show();
                }
            }
        }
    }
}
