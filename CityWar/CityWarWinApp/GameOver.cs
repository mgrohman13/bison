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

        private void gameOver_Load(object sender, EventArgs e)
        {
            IDictionary<Player, double> lost = Cleanup(Map.Game.GetLost());
            IDictionary<Player, double> won = Cleanup(Map.Game.GetWon());

            //losing players always get points directly proportional to their score when they lost
            //for winning players:
            //  invert the stored value so lower is actually better
            //  the best winning player (lowest value) always gets 3.9 * the average of all losers on top of the highest loser's points
            //  all other winning players get a fraction of that bonus, still on top of the highest loser's points
            double winAdd = lost.Values.Max();
            double winMult = 3.9 * lost.Values.Average() * won.Values.Min();
            foreach (Player p in Game.Random.Iterate(won.Keys))
                won[p] = winAdd + winMult / won[p];

            Dictionary<Player, int> points = new Dictionary<Player, int>();
            double mult = 39 * (lost.Values.Sum() + won.Values.Sum());
            double rounding = Game.Random.NextDouble();
            SplitPoints(points, lost, mult, rounding);
            SplitPoints(points, won, mult, rounding);

            //int cur = 0, add = -1, min = int.MaxValue;
            //foreach (var pair in lost)
            //{
            //    int val = Game.Random.Round(-26.0 / pair.Key) + ( cur += ( ++add ) );
            //    points.Add(pair.Value, val);
            //    min = Math.Min(min, val);
            //}
            //for (int idx = won.Count - 1 ; idx >= 0 ; --idx)
            //    points.Add(won.Values[idx], Game.Random.Round(780.0 / won.Keys[idx]) + ( cur += ( ++add ) ));

            int min = points.Values.Min();
            this.textBox1.Clear();
            foreach (var pair in Game.Random.Iterate(points).OrderByDescending(pair => pair.Value))
                this.textBox1.Text += string.Format("{0} - {1}\r\n", pair.Key.Name, pair.Value - min);
        }

        private IDictionary<Player, double> Cleanup(IDictionary<Player, double> dict)
        {
            return new Dictionary<Player, double>(dict);

            //dict = new Dictionary<Player, double>(dict);
            //foreach (Player p in Game.Random.Iterate(dict.Keys))
            //    while (dict.Values.Count(v => dict[p] == v) > 1)
            //        dict[p] += Game.Random.Gaussian();
            //double min = dict.Values.Min();
            //if (min < 1)
            //    foreach (Player p in Game.Random.Iterate(dict.Keys))
            //        dict[p] -= min - 1;
            //return dict;
        }

        private void SplitPoints(IDictionary<Player, int> points, IDictionary<Player, double> dict, double mult, double rounding)
        {
            foreach (Player p in Game.Random.Iterate(dict.Keys))
                points.Add(p, MattUtil.MTRandom.Round(dict[p] * mult, rounding));
        }
    }
}