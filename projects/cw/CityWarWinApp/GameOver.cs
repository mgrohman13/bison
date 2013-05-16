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
            Dictionary<Player, int> points = new Dictionary<Player, int>();

            SortedList<int, Player> lost = Map.game.GetLost(), won = Map.game.GetWon();
            int cur = 0, add = -1, min = int.MaxValue;
            foreach (var pair in lost)
            {
                int val = Game.Random.Round(-26.0 / pair.Key) + ( cur += ( ++add ) );
                points.Add(pair.Value, val);
                min = Math.Min(min, val);
            }
            for (int idx = won.Count - 1 ; idx >= 0 ; --idx)
                points.Add(won.Values[idx], Game.Random.Round(780.0 / won.Keys[idx]) + ( cur += ( ++add ) ));

            this.textBox1.Clear();
            foreach (var pair in Game.Random.Iterate(points))
                this.textBox1.Text += string.Format("{0} - {1}\r\n", pair.Key.Name, pair.Value - min);
        }
    }
}