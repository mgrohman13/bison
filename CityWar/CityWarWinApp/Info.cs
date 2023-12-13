using CityWar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CityWarWinApp
{
    partial class Info : Form
    {
        private static Info theForm = new();
        private readonly ListBox[] units;
        private readonly ListBox[] have;
        private readonly ListBox[] needed;

        new public static void ShowDialog()
        {
            theForm.RefreshStuff();
            ((Form)theForm).ShowDialog();
        }

        private void RefreshStuff()
        {
            RefreshStuff(true);
        }

        private void RefreshStuff(bool players)
        {
            int playersHeight = 4 + this.lbxOrder.ItemHeight * Map.Game.GetPlayers().Length;
            AdjustPlayerLbx(this.lbxOrder, this.label10, playersHeight);
            AdjustPlayerLbx(this.lbxScore, this.label12, playersHeight);
            AdjustPlayerLbx(this.lbxUnits, this.label4, playersHeight);
            AdjustPlayerLbx(this.lbxRelics, this.label9, playersHeight);
            AdjustPlayerLbx(this.lbxCities, this.label7, playersHeight);
            AdjustPlayerLbx(this.lbxWizards, this.label8, playersHeight);
            AdjustPlayerLbx(this.lbxPortals, this.label6, playersHeight);
            AdjustPlayerLbx(this.lbxResources, this.label5, playersHeight);
            AdjustPlayerLbx(this.lbxMagic, this.label11, playersHeight);

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
            this.lbxScore.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxUnits.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxRelics.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxCities.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxWizards.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxPortals.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxResources.DrawMode = DrawMode.OwnerDrawFixed;
            this.lbxMagic.DrawMode = DrawMode.OwnerDrawFixed;

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

                List<string> raceUnits = new(race);
                raceUnits.Sort((name1, name2) => Unit.CreateTempUnit(name1).BaseTotalCost.CompareTo(Unit.CreateTempUnit(name2).BaseTotalCost));
                foreach (string unit in raceUnits)
                {
                    units[arrayIndex].Items.Add(unit);
                    needed[arrayIndex].Items.Add(Map.Game.GetUnitNeeds(unit));
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
            lblTitle.Location = new Point(x, y);
            lblTitle.Name = "lblRace" + arrayIndex;
            lblTitle.Size = new Size(270, 18);
            lblTitle.Text = raceName;

            y += 20;

            ListBox lbxUnit = CreateListBox();
            lbxUnit.Location = new Point(x, y);
            lbxUnit.Name = "lbxUnit" + arrayIndex.ToString();
            lbxUnit.Size = new Size(130, 21);
            units[arrayIndex] = lbxUnit;

            ListBox lbxCurrent = CreateListBox();
            lbxCurrent.Location = new Point(x + 136, y);
            lbxCurrent.Name = "lbxCurrent" + arrayIndex.ToString();
            lbxCurrent.Size = new Size(73, 21);
            lbxCurrent.Tag = lbxUnit;
            have[arrayIndex] = lbxCurrent;

            ListBox lbxNeeded = CreateListBox();
            lbxNeeded.Location = new Point(x + 215, y);
            lbxNeeded.Name = "lbxNeeded" + arrayIndex.ToString();
            lbxNeeded.Size = new Size(65, 21);
            needed[arrayIndex] = lbxNeeded;
        }

        private Label CreateLabel()
        {
            Label lbl = new()
            {
                Font = new Font("Arial", 11.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                BackColor = Color.Black,
                ForeColor = Color.Silver,
                TabIndex = 0,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            this.Controls.Add(lbl);
            return lbl;
        }

        private ListBox CreateListBox()
        {
            ListBox lbx = new()
            {
                Font = new Font("Lucida Console", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                BackColor = Color.Silver,
                ForeColor = Color.Black,
                FormattingEnabled = true,
                ItemHeight = 17,
                DrawMode = DrawMode.OwnerDrawFixed,
            };
            lbx.DrawItem += new DrawItemEventHandler(this.Lbx_DrawItem);
            this.Controls.Add(lbx);
            return lbx;
        }

        private void RefreshHave()
        {
            foreach (ListBox lbxCurrent in have)
            {
                lbxCurrent.Items.Clear();
                foreach (string unit in ((ListBox)lbxCurrent.Tag).Items)
                    lbxCurrent.Items.Add(Map.Game.GetUnitHas(unit));
            }
        }

        private void GetPlayers()
        {
            this.lbxOrder.Items.Clear();
            this.lbxScore.Items.Clear();
            this.lbxUnits.Items.Clear();
            this.lbxRelics.Items.Clear();
            this.lbxCities.Items.Clear();
            this.lbxWizards.Items.Clear();
            this.lbxPortals.Items.Clear();
            this.lbxResources.Items.Clear();
            this.lbxMagic.Items.Clear();

            Player[] players = Map.Game.GetPlayers();
            var info = players.Select(p =>
            {
                int airInc = 0, deathInc = 0, earthInc = 0, natureInc = 0, prodInc = 0, waterInc = 0, magicInc = 0, popInc = 0;
                p.GenerateIncome(ref airInc, ref deathInc, ref earthInc, ref natureInc, ref prodInc, ref waterInc, ref magicInc, ref popInc);
                int sum = airInc + deathInc + earthInc + natureInc + prodInc + waterInc + popInc;
                return new string[] { $"{p.Score:0}", $"{p.GetArmyStrength():0}", $"+{sum}", $"+{magicInc}" };
            }).ToArray();
            for (int b = 0; b < 4; b++)
            {
                int max = -1;
                for (int a = 0; a < players.Length; a++)
                    max = Math.Max(max, info[a][b].Length);
                for (int a = 0; a < players.Length; a++)
                    info[a][b] = info[a][b].PadLeft(max);
            }
            for (int c = 0; c < players.Length; c++)
            {
                Player p = players[c];
                p.GetCounts(out int wizards, out int portals, out int cities, out int relics, out int units);

                this.lbxOrder.Items.Add(p);

                this.lbxScore.Items.Add($"({p.Score - p.LastRelicScore:0}) {info[c][0]}");
                this.lbxUnits.Items.Add($"({units}) {info[c][1]}");
                this.lbxResources.Items.Add($"{p.CountTradeableResources():0} {info[c][2]}");
                this.lbxMagic.Items.Add($"{p.Magic} {info[c][3]}");

                this.lbxRelics.Items.Add(relics);
                this.lbxCities.Items.Add(cities);
                this.lbxWizards.Items.Add(wizards);
                this.lbxPortals.Items.Add(portals);
            }
        }

        private void LbxOrder_DrawItem(object sender, DrawItemEventArgs e)
        {
            // check if the item exists
            if (e.Index >= 0 && e.Index < this.lbxOrder.Items.Count)
            {
                Player p = ((Player)this.lbxOrder.Items[e.Index]);

                // Draw the background of the ListBox control for each item. 
                using SolidBrush playerBrush = new(p.Color);
                e.Graphics.FillRectangle(playerBrush, e.Bounds);

                // Draw the current item text based on the current Font and the custom brush settings.
                float width = e.Graphics.MeasureString(p.Name, e.Font).Width;
                using SolidBrush fontBrush = new(p.InverseColor);
                e.Graphics.DrawString(p.Name, e.Font, fontBrush, e.Bounds.X + (e.Bounds.Width - width), e.Bounds.Y + 1, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        private void LbxCapturables_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox box = (ListBox)sender;

            // check if the item exists
            if (e.Index >= 0 && e.Index < this.lbxOrder.Items.Count)
            {
                Player p = ((Player)this.lbxOrder.Items[e.Index]);

                // Draw the background of the ListBox control for each item. 
                using SolidBrush playerBrush = new(p.Color);
                e.Graphics.FillRectangle(playerBrush, e.Bounds);

                // Draw the current item text based on the current Font and the custom brush settings.
                float width = e.Graphics.MeasureString(box.Items[e.Index].ToString(), e.Font).Width;
                using SolidBrush fontBrush = new(p.InverseColor);
                e.Graphics.DrawString(box.Items[e.Index].ToString(), e.Font, fontBrush, e.Bounds.X + (e.Bounds.Width - width), e.Bounds.Y + 1, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        private void Lbx_DrawItem(object sender, DrawItemEventArgs e)
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
                SolidBrush backBrush = new(back);
                e.Graphics.FillRectangle(backBrush, e.Bounds);

                // Draw the current item text based on the current Font and the custom brush settings.
                float width = e.Graphics.MeasureString(box.Items[e.Index].ToString(), e.Font).Width;
                SolidBrush foreBrush = new(fore);
                e.Graphics.DrawString(box.Items[e.Index].ToString(), e.Font, foreBrush, e.Bounds.X + (e.Bounds.Width - width), e.Bounds.Y + 1, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        public static void ClearDialog()
        {
            theForm = new();
        }
    }
}