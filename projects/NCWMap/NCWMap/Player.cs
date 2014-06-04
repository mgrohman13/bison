using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCWMap
{
    public class Player
    {
        private static readonly string[] units = new string[] { "1Z", "1H", "1E", "1C", "2A", "2T", "2H", "2D", "3D", "3W", "3E", "3B" };

        public string Name;
        public string Unit;
        public int[,] Resources;

        public Player(string name, Tile tile, int order)
        {
            this.Name = name;
            this.Resources = new int[3, 2];
            InitPlayer(tile, order);
        }

        private void InitPlayer(Tile tile, int order)
        {
            bool land = tile.GetNeighbors().Any(t => !t.Water);
            bool water = tile.GetNeighbors().Any(t => t.Water);

            int tier = Program.Random.Next(3);
            int unit;
            if (land && water)
                unit = Program.Random.Next(4);
            else if (land)
                unit = Program.Random.Next(3);
            else
                unit = Program.Random.Next(2) * 3;

            this.Unit = units[4 * tier + unit];

            int resources = 12 - ( tier + 1 ) * 2;
            while (resources >= 6)
            {
                int t = Program.Random.SelectValue(new Dictionary<int, int> { { 0, 6 }, { 1, 3 }, { 2, 2 } });
                int amt = ( t + 1 ) * 2;
                resources -= amt;
                this.Resources[t, 0] += amt;
            }
            while (resources-- > 0)
                ++this.Resources[Program.Random.Next(3), 0];

            order *= 2;
            while (order-- > 0)
                ++this.Resources[Program.Random.Next(3), 1];
        }
    }
}
