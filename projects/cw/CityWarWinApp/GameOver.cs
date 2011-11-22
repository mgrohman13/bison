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
    partial class GameOver : Form
    {
        public GameOver()
        {
            InitializeComponent();
        }

        private void gameOver_Load(object sender, EventArgs e)
        {
            Dictionary<Player, int> won = Map.game.GetWon();
            Player[] defeatedPlayers = Map.game.GetDefeatedPlayers();
            Dictionary<Player, int> points = new Dictionary<Player, int>(won.Count + defeatedPlayers.Length);

            //adds in (x^2+x)/2 points, where x is the inverse index
            int cur = 0, add = -1;
            for (int b = defeatedPlayers.Length ; --b > -1 ; )
            {
                points.Add(defeatedPlayers[b], cur += ( ++add ));
            }
            while (won.Count > 0)
            {
                Player next = null;
                int turn = Map.game.Turn;
                foreach (KeyValuePair<Player, int> p in won)
                    if (p.Value < turn)
                    {
                        next = p.Key;
                        turn = p.Value;
                    }
                points.Add(next, Game.Random.Round(666.0 / turn) + ( cur += ( ++add ) ));
                won.Remove(next);
            }

            this.textBox1.Clear();
            foreach (KeyValuePair<Player, int> p in Game.Random.Iterate(points))
                this.textBox1.Text += string.Format("{0} - {1}\r\n", p.Key.Name, p.Value);
        }
    }
}