using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GalWar;

namespace GalWarWin
{
    public partial class ResearchForm : Form
    {
        private static ResearchForm form = new ResearchForm();

        private ResearchForm()
        {
            InitializeComponent();
        }

        private void SetObsolete(HashSet<ShipDesign> obsolete)
        {
            ShipDesign[] items = new ShipDesign[obsolete.Count];
            obsolete.CopyTo(items, 0);
            this.lbxDesigns.Items.Clear();
            this.lbxDesigns.Items.AddRange(items);
        }

        private void lbxDesigns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.Width < 400)
                this.Width += 200;

            Buildable buildable = (Buildable)this.lbxDesigns.SelectedItem;
            sdObsolete.SetBuildable(buildable);
        }

        public static void ShowForm(ShipDesign newDesign, HashSet<ShipDesign> obsolete)
        {
            if (form.Width > 400)
                form.Width -= 200;

            MainForm.GameForm.SetLocation(form);

            form.shipDesignForm1.SetBuildable(newDesign);
            form.SetObsolete(obsolete);
            form.ShowDialog();
        }
    }
}
