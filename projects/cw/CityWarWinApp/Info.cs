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
    partial class Info : Form
    {
        private static Info theForm = new Info();
        private ListBox[] units;
        private ListBox[] have;
        private ListBox[] needed;

        public static void showDialog()
        {
            theForm.RefreshStuff();
            theForm.ShowDialog();
        }

        private void RefreshStuff()
        {
            RefreshStuff(true);
        }

        private void RefreshStuff(bool players)
        {
            int playersHeight = 4 + this.lbxOrder.ItemHeight * Map.game.GetPlayers().Length;
            AdjustPlayerLbx(this.lbxUnits, this.label4, playersHeight);
            AdjustPlayerLbx(this.lbxOrder, this.label10, playersHeight);
            AdjustPlayerLbx(this.lbxRelics, this.label9, playersHeight);
            AdjustPlayerLbx(this.lbxCities, this.label7, playersHeight);
            AdjustPlayerLbx(this.lbxWizards, this.label8, playersHeight);
            AdjustPlayerLbx(this.lbxPortals, this.label6, playersHeight);
            AdjustPlayerLbx(this.lbxResources, this.label5, playersHeight);

            if (players)
                GetPlayers();
            RefreshHave();
        }

        private void AdjustPlayerLbx(ListBox listBox, Label label, int playersHeight)
        {
            label.Location = new Point(label.Location.X, label.Location.Y + listBox.Height - playersHeight);
            listBox.Location = new Point(listBox.Location.X, listBox.Location.Y + listBox.Height - playersHeight);
            listBox.Height = playersHeight;
        }

        Info()
        {
            InitializeComponent();

            this.lbxOrder.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxUnits.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxRelics.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxCities.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxWizards.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxPortals.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxResources.DrawMode = DrawMode.OwnerDrawFixed;

            GetPlayers();

            int numRaces = Game.Races.Count;
            this.units = new ListBox[numRaces];
            this.have = new ListBox[numRaces];
            this.needed = new ListBox[numRaces];

            int column = numRaces / 2;
            this.Width += 300 * column;

            // 12, 30
            int arrayIndex = -1;
            column = 0;
            bool row = false;
            int y = -1;
            foreach (string raceName in Game.Races.Keys)
            {
                string[] race = Game.Races[raceName];
                CreateRaceBoxes(10 + column * 300, row ? y : 50, raceName, ++arrayIndex);

                List<string> raceUnits = new List<string>(race);
                raceUnits.Sort(delegate(string name1, string name2)
                {
                    Unit unit1 = Unit.CreateTempUnit(name1);
                    Unit unit2 = Unit.CreateTempUnit(name2);
                    return unit1.BaseCost.CompareTo(unit2.BaseCost);
                });
                foreach (string unit in raceUnits)
                {
                    units[arrayIndex].Items.Add(unit);
                    needed[arrayIndex].Items.Add(Map.game.GetUnitNeeds(unit));
                }

                if (!row)
                    y = 0;
                int unitsHeight = 4 + units[arrayIndex].ItemHeight * raceUnits.Count;
                y += 80 + unitsHeight;

                have[arrayIndex].Height = unitsHeight;
                needed[arrayIndex].Height = unitsHeight;
                units[arrayIndex].Height = unitsHeight;

                if (row)
                {
                    this.Height = Math.Max(y - 30, 100 + lbxOrder.Height);

                    ++column;
                    row = false;
                }
                else
                {
                    row = true;
                }
            }

            RefreshStuff(false);
        }

        private void CreateRaceBoxes(int x, int y, string raceName, int arrayIndex)
        {
            Label lblTitle = CreateLabel();
            lblTitle.Location = new System.Drawing.Point(x, y);
            lblTitle.Name = "lblRace" + arrayIndex;
            lblTitle.Size = new System.Drawing.Size(270, 18);
            lblTitle.Text = raceName;

            y += 20;

            ListBox lbxUnit = CreateListBox();
            lbxUnit.Location = new System.Drawing.Point(x, y);
            lbxUnit.Name = "lbxUnit" + arrayIndex.ToString();
            lbxUnit.Size = new System.Drawing.Size(130, 21);
            units[arrayIndex] = lbxUnit;

            ListBox lbxCurrent = CreateListBox();
            lbxCurrent.Location = new System.Drawing.Point(x + 136, y);
            lbxCurrent.Name = "lbxCurrent" + arrayIndex.ToString();
            lbxCurrent.Size = new System.Drawing.Size(73, 21);
            lbxCurrent.Tag = lbxUnit;
            have[arrayIndex] = lbxCurrent;

            ListBox lbxNeeded = CreateListBox();
            lbxNeeded.Location = new System.Drawing.Point(x + 215, y);
            lbxNeeded.Name = "lbxNeeded" + arrayIndex.ToString();
            lbxNeeded.Size = new System.Drawing.Size(65, 21);
            needed[arrayIndex] = lbxNeeded;
        }

        private Label CreateLabel()
        {
            Label label1 = new Label();
            label1.Font = new System.Drawing.Font("Algerian", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            label1.ForeColor = System.Drawing.Color.White;
            label1.TabIndex = 0;
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(label1);
            return label1;
        }

        private ListBox CreateListBox()
        {
            ListBox lbx = new ListBox();
            lbx.Font = new System.Drawing.Font("Engravers", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ));
            lbx.ForeColor = System.Drawing.Color.Black;
            lbx.FormattingEnabled = true;
            lbx.ItemHeight = 17;
            lbx.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbx_DrawItem);
            lbx.DrawMode = DrawMode.OwnerDrawFixed;
            this.Controls.Add(lbx);
            return lbx;
        }

        private void RefreshHave()
        {
            foreach (ListBox lbxCurrent in have)
            {
                lbxCurrent.Items.Clear();
                foreach (string unit in ( (ListBox)lbxCurrent.Tag ).Items)
                    lbxCurrent.Items.Add(Map.game.GetUnitHas(unit));
            }
        }

        private void GetPlayers()
        {
            this.lbxOrder.Items.Clear();
            this.lbxUnits.Items.Clear();
            this.lbxRelics.Items.Clear();
            this.lbxCities.Items.Clear();
            this.lbxWizards.Items.Clear();
            this.lbxPortals.Items.Clear();
            this.lbxResources.Items.Clear();

            foreach (Player p in Map.game.GetPlayers())
            {
                this.lbxOrder.Items.Add(p);
                int wizards, portals, cities, relics, units;
                p.GetCounts(out wizards, out portals, out cities, out relics, out units);
                this.lbxUnits.Items.Add(p.GetArmyStrength().ToString("0"));
                this.lbxRelics.Items.Add(relics);
                this.lbxCities.Items.Add(cities);
                this.lbxWizards.Items.Add(wizards);
                this.lbxPortals.Items.Add(portals);
                this.lbxResources.Items.Add(p.GetTotalResources().ToString("0"));
            }
        }

        private void lbxOrder_DrawItem(object sender, DrawItemEventArgs e)
        {
            // check if the item exists
            if (e.Index >= 0 && e.Index < this.lbxOrder.Items.Count)
            {
                Player p = ( (Player)this.lbxOrder.Items[e.Index] );

                // Draw the background of the ListBox control for each item.
                //e.DrawBackground();
                e.Graphics.FillRectangle(new SolidBrush(p.Color), e.Bounds);

                //cheate a new black brush
                SolidBrush myBrush = new SolidBrush(Color.Black);

                // Draw the current item text based on the current Font and the custom brush settings.
                float width = e.Graphics.MeasureString(p.Name, e.Font).Width;
                e.Graphics.DrawString(p.Name, e.Font, new SolidBrush(p.InverseColor), e.Bounds.X + ( e.Bounds.Width - width ), e.Bounds.Y, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        private void lbxCapturables_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox box = (ListBox)sender;

            // check if the item exists
            if (e.Index >= 0 && e.Index < this.lbxOrder.Items.Count)
            {
                Player p = ( (Player)this.lbxOrder.Items[e.Index] );

                // Draw the background of the ListBox control for each item.
                //e.DrawBackground();
                e.Graphics.FillRectangle(new SolidBrush(p.Color), e.Bounds);

                //cheate a new black brush
                SolidBrush myBrush = new SolidBrush(Color.Black);

                // Draw the current item text based on the current Font and the custom brush settings.
                float width = e.Graphics.MeasureString(box.Items[e.Index].ToString(), e.Font).Width;
                e.Graphics.DrawString(box.Items[e.Index].ToString(), e.Font, new SolidBrush(p.InverseColor), e.Bounds.X + ( e.Bounds.Width - width ), e.Bounds.Y, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        private void lbx_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox box = (ListBox)sender;

            // check if the item exists
            if (e.Index >= 0 && e.Index < box.Items.Count)
            {
                Color fore = Color.White, back = Color.Black;
                if (e.Index % 2 == 0)
                {
                    Color temp = Color.FromArgb(fore.R, fore.G, fore.B);
                    fore = Color.FromArgb(back.R, back.G, back.B);
                    back = Color.FromArgb(temp.R, temp.G, temp.B);
                }

                // Draw the background of the ListBox control for each item.
                //e.DrawBackground();
                e.Graphics.FillRectangle(new SolidBrush(back), e.Bounds);

                //cheate a new black brush
                SolidBrush myBrush = new SolidBrush(Color.Black);

                // Draw the current item text based on the current Font and the custom brush settings.
                float width = e.Graphics.MeasureString(box.Items[e.Index].ToString(), e.Font).Width;
                e.Graphics.DrawString(box.Items[e.Index].ToString(), e.Font, new SolidBrush(fore), e.Bounds.X + ( e.Bounds.Width - width ), e.Bounds.Y, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        public static void clearDialog()
        {
            theForm = new Info();
        }
    }
}