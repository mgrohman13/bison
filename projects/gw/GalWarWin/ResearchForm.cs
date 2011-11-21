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

        private PlanetDefense newDefense;

        private ResearchForm()
        {
            InitializeComponent();
        }

        private void SetObsolete(HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense)
        {
            ShipDesign[] items = new ShipDesign[obsolete.Count];
            obsolete.CopyTo(items, 0);
            this.lbxDesigns.Items.Clear();
            if (!CompareDefense(oldDefense, newDefense))
                this.lbxDesigns.Items.Add(oldDefense);
            this.lbxDesigns.Items.AddRange(items);
            this.newDefense = newDefense;
        }

        private bool CompareDefense(PlanetDefense oldDefense, PlanetDefense newDefense)
        {
            return ( oldDefense.Att == newDefense.Att && oldDefense.Def == newDefense.Def
                    && MainForm.FormatDouble(oldDefense.HPCost) == MainForm.FormatDouble(newDefense.HPCost) );
        }

        private void lbxDesigns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.Width < 400)
                this.Width += 200;

            Buildable buildable = (Buildable)this.lbxDesigns.SelectedItem;
            sdObsolete.SetBuildable(buildable);

            newPlanetDefense.Visible = ( buildable is PlanetDefense );
            if (newPlanetDefense.Visible)
            {
                sdObsolete.lblName.Text = "Old:";

                newPlanetDefense.SetBuildable(newDefense);
                newPlanetDefense.lblName.Text = "New:";
            }
        }

        public static void ShowDialog(MainForm gameForm, ShipDesign newDesign, HashSet<ShipDesign> obsolete, PlanetDefense oldDefense, PlanetDefense newDefense)
        {
            if (form.Width > 400)
                form.Width -= 200;

            gameForm.SetLocation(form);

            form.shipDesignForm1.SetBuildable(newDesign);
            form.SetObsolete(obsolete, oldDefense, newDefense);
            form.ShowDialog();
        }
    }
}
