using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCWMap
{
    public class Player
    {
        private static readonly string[] units = new string[] { "1Z", "1H", "1E", "1C", "2A", "2T", "2H", "2D", "3D", "3W", "3E", "3B" };

        public readonly string Name;
        public string Unit;
        public readonly int[,] Resources;

        public Player(string name, Tile tile, int order, int add)
        {
            this.Name = name;
            this.Unit = null;
            this.Resources = new int[3, 2];

            int tier = SelectUnit(tile);
            DistResources(GetTierCost(tier));
            BalanceOrder(order, add);
            for (int a = 0 ; a < 3 ; ++a)
                AddOne(a);
        }

        private int SelectUnit(Tile tile)
        {
            //determine available units
            bool land = tile.GetNeighbors().Any(t => !t.Water);
            bool water = tile.GetNeighbors().Any(t => t.Water);

            //select random unit from all available
            int unit;
            if (land && water)
                unit = Program.Random.Next(4);
            else if (land)
                unit = Program.Random.Next(3);
            else
                unit = Program.Random.Next(2) * 3;
            int tier = Program.Random.Next(3);
            Unit = units[4 * tier + unit];

            return tier;
        }

        private void DistResources(int unitCost)
        {
            //remaining resources to be given to bank
            int resources = 12 - unitCost;

            //balance amt given to each tier including units
            if (Program.Random.Bool())
            {
                int amt = Program.Random.SelectValue(new Dictionary<int, int> { { 0, 1 }, { 2, 2 }, { 4, 3 } });
                if (amt > 0)
                {
                    Console.WriteLine("T1: " + amt);
                    AddResource(0, 0, amt, ref resources);
                }
            }
            else
            {
                if (Program.Random.Next(3) < 1)
                {
                    Console.WriteLine("T2: 4");
                    AddResource(1, 0, 4, ref resources);
                }
            }

            //give in chunks of whole units while possible
            while (resources >= 6)
            {
                Console.WriteLine("resources: " + resources);
                int tier = Program.Random.SelectValue(new Dictionary<int, int> { { 0, 6 }, { 1, 3 }, { 2, 2 } });
                AddResource(tier, 0, GetTierCost(tier), ref resources);
            }
            //remaining are evenly distributed
            Console.WriteLine("last: " + resources);
            while (resources >= 1)
                AddResource(Program.Random.Next(3), 0, 1, ref resources);
        }

        private int BalanceOrder(int order, int add)
        {
            //turn order balancing
            order *= 1;
            order += add;
            while (order >= 1)
                AddResource(Program.Random.Next(3), 1, 1, ref order);
            return order;
        }

        private static int GetTierCost(int tier)
        {
            return ( tier + 1 ) * 2;
        }
        private void AddOne(int tier)
        {
            int unused = 0;
            AddResource(tier, 1, 1, ref unused);
        }
        private void AddResource(int tier, int mult, int add, ref int total)
        {
            total -= add;
            Resources[tier, mult] += add;
            if (mult == 1)
            {
                Resources[tier, 0] += Resources[tier, 1] / 6;
                Resources[tier, 1] %= 6;
            }
        }
    }
}
