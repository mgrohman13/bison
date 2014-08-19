using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CityWar;

namespace CityWarWinApp
{
    partial class NewGame : Form
    {
        List<Player> players = new List<Player>();

        public NewGame()
        {
            this.BackgroundImage = MainMenu.getBackGround();
            InitializeComponent();

            InitRaces();

            int insideX = this.Bounds.Width, insideY = this.Bounds.Height;

            this.Bounds = Screen.PrimaryScreen.Bounds;

            for (int i = -1 ; ++i < Controls.Count ; )
            {
                Controls[i].Location = new Point(Controls[i].Location.X + ( this.Width - insideX ) / 2,
                    Controls[i].Location.Y + ( this.Height - insideY ) / 2);

                Controls[i].BackColor = MainMenu.back;
                Controls[i].ForeColor = MainMenu.fore;
            }

            players.Add(new Player(Color.DarkBlue, "Black"));
            players.Add(new Player(Color.LightBlue, "Blue"));
            players.Add(new Player(Color.DarkGreen, "Green"));
            players.Add(new Player(Color.Magenta, "Pink"));
            players.Add(new Player(Color.Red, "Red"));
            players.Add(new Player(Color.Yellow, "Yellow"));

            RefreshPlayerList();

            nudSize_ValueChanged(null, null);
        }

        private void InitRaces()
        {
            List<string> races = new List<string>();
            races.Add(Player.Random);
            UnitSchema us = new UnitTypes().GetSchema();
            foreach (UnitSchema.UnitRow row in us.Unit)
            {
                if (!races.Contains(row.Race))
                    races.Add(row.Race);
            }
            this.cbxRace.Items.AddRange(races.ToArray());
        }

        private void RefreshPlayerList()
        {
            this.lbxPlayers.Items.Clear();
            this.lbxPlayers.Items.AddRange(players.ToArray());
        }

        private void lblStart_Click(object sender, EventArgs e)
        {
            try
            {
                CityWar.Player[] realPlayers = new CityWar.Player[players.Count];

                IEnumerable<IGrouping<string, CityWar.Player>> groups;
                do
                {
                    int i = -1;
                    foreach (Player player in players)
                    {
                        string race = player.Race;
                        if (race == Player.Random)
                        {
                            int idx = Game.Random.Next(cbxRace.Items.Count - 1);
                            race = (string)cbxRace.Items[idx + 1];
                        }
                        realPlayers[++i] = new CityWar.Player(race, player.Color, player.Name);
                    }

                    groups = realPlayers.GroupBy(player => player.Race);
                } while (players.All(player => player.Race == Player.Random) && players.Count >= cbxRace.Items.Count - 1 &&
                        ( groups.Count() != cbxRace.Items.Count - 1 || groups.Max(group => group.Count()) - groups.Min(group => group.Count()) > 1 ));

                Map.Game = new Game(realPlayers, Game.Random.GaussianCappedInt((double)this.nudSize.Value, .104, (int)this.nudSize.Minimum));
            }
            catch (ArgumentOutOfRangeException aoore)
            {
                MessageBox.Show(aoore.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            new Map().Show();
            this.Close();
        }

        private void lblCancel_Click(object sender, EventArgs e)
        {
            new MainMenu().Show();
            this.Close();
        }

        private void lbxPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            Player p = ( (Player)this.lbxPlayers.SelectedItem );
            if (p == null)
                return;
            this.lblColor.BackColor = p.Color;
            this.txtName.Text = p.Name;
            this.cbxRace.SelectedItem = p.Race;
            this.cbxRace.Text = p.Race;

            this.lbxPlayers.Refresh();
        }

        private void lbxPlayers_DrawItem(object sender, DrawItemEventArgs e)
        {
            // check if the item exists
            if (e.Index >= 0 && e.Index < this.lbxPlayers.Items.Count)
            {
                Player p = ( (Player)this.lbxPlayers.Items[e.Index] );

                // Draw the background of the ListBox control for each item.
                //e.DrawBackground();
                e.Graphics.FillRectangle(new SolidBrush(p.Color), e.Bounds);

                //create a new black brush
                SolidBrush myBrush = new SolidBrush(Color.Black);

                Color inverse = p.InverseColor;

                // Draw the current item text based on the current Font and the custom brush settings.
                e.Graphics.DrawString(p.Name, e.Font, new SolidBrush(inverse), e.Bounds, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                if (e.Index == this.lbxPlayers.SelectedIndex)
                    e.Graphics.DrawRectangle(new Pen(inverse, 2),
                        e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2);
            }
        }

        private void lblColor_Click(object sender, EventArgs e)
        {
            SelectColor((Label)sender);
        }

        private void SelectColor(Label lblColor)
        {
            this.colorDialog.Color = lblColor.BackColor;
            if (this.colorDialog.ShowDialog() == DialogResult.OK)
                lblColor.BackColor = this.colorDialog.Color;
        }

        private void lblEdit_Click(object sender, EventArgs e)
        {
            Player p = this.lbxPlayers.SelectedItem as Player;
            if (p == null)
                return;
            this.players[players.IndexOf(p)].Name = this.txtName.Text;
            this.players[players.IndexOf(p)].Color = this.lblColor.BackColor;
            this.players[players.IndexOf(p)].Race = (string)this.cbxRace.SelectedItem;

            this.lbxPlayers.Refresh();
        }

        private void lblAddNew_Click(object sender, EventArgs e)
        {
            Player p = new Player(this.lblColor.BackColor, this.txtName.Text, (string)this.cbxRace.SelectedItem);
            this.lbxPlayers.Items.Add(p);
            players.Add(p);

            this.lbxPlayers.Refresh();
        }

        private void lblDelete_Click(object sender, EventArgs e)
        {
            Player p = ( (Player)this.lbxPlayers.SelectedItem );
            players.Remove(p);
            this.lbxPlayers.Items.Remove(p);

            //this.lblColor.BackColor = Color.Black;
            //this.txtName.Text = "";
            //this.lbxPlayers.SelectedIndex = -1;

            this.lbxPlayers.Refresh();
        }

        private void lblStart_MouseEnter(object sender, EventArgs e)
        {
            MainMenu.MenuItem_MouseEnter((Label)sender, e);
        }
        private void lblCancel_MouseEnter(object sender, EventArgs e)
        {
            MainMenu.MenuItem_MouseEnter((Label)sender, e);
        }
        private void lblStart_MouseLeave(object sender, EventArgs e)
        {
            MainMenu.MenuItem_MouseLeave((Label)sender, e);
        }
        private void lblCancel_MouseLeave(object sender, EventArgs e)
        {
            MainMenu.MenuItem_MouseLeave((Label)sender, e);
        }

        private class Player
        {
            public const string Random = "Random";

            public Player(Color color, string name, string race)
            {
                this.color = color;
                this.name = name;
                this.race = race;
            }

            public Player(Color color, string name)
            {
                this.color = color;
                this.name = name;
                this.race = Random;
            }

            private Color color;
            private string name;
            private string race;
            private CityWar.Player player;

            public Color Color
            {
                get
                {
                    return color;
                }
                set
                {
                    color = value;
                    player = null;
                }
            }
            public string Name
            {
                get
                {
                    return name;
                }
                set
                {
                    name = value;
                    player = null;
                }
            }
            public string Race
            {
                get
                {
                    return race;
                }
                set
                {
                    race = value;
                    player = null;
                }
            }

            public Color InverseColor
            {
                get
                {
                    return CityWarPlayer.InverseColor;
                }
            }
            private CityWar.Player CityWarPlayer
            {
                get
                {
                    if (player == null)
                        player = new CityWar.Player(Race, Color, Name);
                    return player;
                }
            }
        }

        private void nudSize_ValueChanged(object sender, EventArgs e)
        {
            this.lblSize.Text = Game.GetNumHexes((double)this.nudSize.Value).ToString("0.0");
        }
    }
}