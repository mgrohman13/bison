using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using CityWar;

namespace CityWarWinApp
{
    partial class MainMenu : Form
    {
        public static string SavePath;

        Map main = new Map();

        public MainMenu()
        {
            SavePath = string.Empty;
            try
            {
                if (File.Exists("savepath.txt"))
                {
                    using (StreamReader reader = new StreamReader("savepath.txt"))
                        SavePath = reader.ReadLine();
                    if (!Directory.Exists(SavePath))
                        SavePath = string.Empty;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                SavePath = string.Empty;
            }

            if (SavePath != string.Empty && !( SavePath.EndsWith("\\") || SavePath.EndsWith("/") ))
                SavePath += "\\";

            main.loadGame.InitialDirectory = SavePath;
            main.loadGame.FileName = "c.cws";
            main.saveGame.InitialDirectory = main.loadGame.InitialDirectory;
            main.saveGame.FileName = main.loadGame.FileName;

            this.BackgroundImage = getBackGround();
            InitializeComponent();

            Label[] menuItems = new Label[4];
            menuItems[0] = this.lblNewGame;
            menuItems[1] = this.lblLoadGame;
            menuItems[2] = this.lblOptions;
            menuItems[3] = this.lblQuit;

            for (int i = -1 ; ++i < menuItems.Length ;)
            {
                menuItems[i].BackColor = back;
                menuItems[i].ForeColor = fore;
            }

            this.Bounds = Map.GetScreenBounds();

            int width = this.lblNewGame.Width, items = 4, spacing = 30,
                 height = this.lblNewGame.Height + spacing;
            int x = ( this.Width - width ) / 2,
                y = ( this.Height - height * items + spacing ) / 2;

            for (int i = -1 ; ++i < menuItems.Length ;)
            {
                menuItems[i].Location = new Point(x, y);
                y += height;
            }
        }

        static readonly Color defaultBack = Color.Black;
        static readonly Color defaultFore = Color.White;
        static readonly Color defaultOver = Color.Black;

        public static Color back = defaultBack;
        public static Color fore = defaultFore;
        static Color over = defaultOver;

        public static Image getBackGround()
        {
            string[] pics = System.IO.Directory.GetFiles(Game.ResourcePath + "backs\\", "*.jpg");
            string file = Game.Random.SelectValue(pics);

            try
            {
                string[] pathParts = file.Split('\\');
                string[] color = pathParts[pathParts.Length - 1].Split('.')[0].Split('~');

                back = Color.FromArgb(byte.Parse(color[0]), byte.Parse(color[1]), byte.Parse(color[2]));
                fore = Color.FromArgb(byte.Parse(color[3]), byte.Parse(color[4]), byte.Parse(color[5]));
                over = Color.FromArgb(byte.Parse(color[6]), byte.Parse(color[7]), byte.Parse(color[8]));
            }
            catch
            {
                back = defaultBack;
                fore = defaultFore;
                over = defaultOver;
            }

            return new Bitmap(file);
        }

        void menuItem_MouseEnter(object sender, EventArgs e)
        {
            MenuItem_MouseEnter((Label)sender, e);
        }

        void menuItem_MouseLeave(object sender, EventArgs e)
        {
            MenuItem_MouseLeave((Label)sender, e);
        }

        public static void MenuItem_MouseEnter(Label sender, EventArgs e)
        {
            sender.BackColor = Color.Transparent;
            sender.ForeColor = over;
        }

        public static void MenuItem_MouseLeave(Label sender, EventArgs e)
        {
            sender.BackColor = back;
            sender.ForeColor = fore;
        }

        private void lblNewGame_Click(object sender, EventArgs e)
        {
            new NewGame().Show();
            this.Close();
        }

        private void lblLoadGame_Click(object sender, EventArgs e)
        {
            if (main.loadGame.ShowDialog() == DialogResult.OK)
            {
                main.LoadGame(main.loadGame.FileName);

                main.Show();
                this.Close();

                MessageBox.Show("Game Loaded.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void lblOptions_Click(object sender, EventArgs e)
        {
            this.lblOptions.BackColor = this.lblOptions.ForeColor;
        }

        private void lblQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}