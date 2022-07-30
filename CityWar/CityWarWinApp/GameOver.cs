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
    partial class GameOver : Form
    {
        public GameOver()
        {
            InitializeComponent();
        }

        private void GameOver_Load(object sender, EventArgs e)
        {
            Dictionary<Player, int> points = new();
            Dictionary<Player, double> values = new();
            double rounding = Game.Random.NextDouble();

            Setup(values, Map.Game.GetLost(), false);
            Setup(values, Map.Game.GetWon(), true);

            //give out 21 points based on relative final scoring values
            double mult = 21 / values.Values.Sum();
            foreach (var pair in Game.Random.Iterate(values))
                points.Add(pair.Key, MattUtil.MTRandom.Round(pair.Value * mult, rounding));

            //add in base (x^2-x)/2 points based on rankings 
            int cur = 0, add = -1;
            foreach (Player player in Game.Random.Iterate(values).OrderBy(p => p.Value).Select(p => p.Key))
                points[player] += (cur += (++add));

            //subtract min and display
            int min = points.Values.Min();
            this.textBox1.Clear();
            foreach (var pair in Game.Random.Iterate(points).OrderByDescending(pair => pair.Value))
                this.textBox1.Text += string.Format("{0} ({2}, {4} {3}) - {1}\r\n",
                    pair.Key.Name, pair.Value - min,
                    pair.Key.StartOrder + 1, pair.Key.StartCity ? "City" : "Wizard", pair.Key.Race);
        }

        private void Setup(Dictionary<Player, double> values, IDictionary<Player, int> turns, bool win)
        {
            foreach (var pair in Game.Random.Iterate(turns))
            {
                //small score padding for both winners and losers
                double score = win ? 390 : 169;
                //sqrt the score so that sqrt(score)/turn only decreases with time (incentive to win ASAP not run up score)
                score = Math.Sqrt(score + pair.Key.Score);
                double turn = pair.Value;
                //losing players have the turn they lost inverted (lasting longer is better) 
                // and ensure there is always a gap between the final 2 players winning and losing on the same turn
                if (!win)
                    turn = 13 + Map.Game.Turn * 1.69 - .39 * turn;
                //divide by turns
                score /= (13 + turn);
                //square final result so it's roughly score/(turn^2)
                values.Add(pair.Key, score * score);
            }
        }
    }
}
