using System;
using System.Collections.Generic;
using System.Linq;

namespace NCWMap
{
    public class Player
    {
        private static readonly string[] units = new string[] { "1Z", "1H", "1E", "1C", "2A", "2T", "2H", "2D", "3D", "3W", "3E", "3B" };

        public readonly string Name;
        public readonly Tile Tile;
        public string Unit { get; private set; }
        public readonly int[,] Resources;
        public readonly int Order, Add;
        public int Relic { get; private set; }

        public Player(string name, Tile tile, int order, int add)
        {
            this.Name = name;
            this.Tile = tile;
            this.Unit = null;
            this.Resources = new int[3, 2];
            this.Order = order;
            this.Add = add;

            int tier = SelectUnit(tile);
            DistResources(GetTierCost(tier));
        }
        public void DoMore()
        {
            BalanceOrder(Order, Add);
            for (int a = 0; a < 3; ++a)
                AddOne(a);
            Relic = AddRelic();
        }
        public bool GetPlayerExtra(int a)
        {
            int e = this.Resources[a, 0] % (2 * (a + 1));
            if (a == 0 && e == 0 && this.Resources[a, 0] > 0 && Program.Random.Next(6) == 0)
                e = 1;
            if (e > 0 && this.Resources[a, 0] == 1 && this.Resources[a, 1] == 0)
                e = 0;
            return e > 0;
        }
        public void Outpost()
        {
            HashSet<Tile> neighbors = Tile.GetNeighbors().ToHashSet();
            HashSet<Tile> twoAway = neighbors.SelectMany(t => t.GetNeighbors()).ToHashSet();
            neighbors.Add(Tile);
            twoAway.ExceptWith(neighbors);

            Program.Random.SelectValue(twoAway.Where(t => t.Inf == null)).Outpost(this);
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
                    Program.Log("T1: " + amt);
                    AddResource(0, 0, amt, ref resources);
                }
            }
            else
            {
                if (Program.Random.Next(3) == 0)
                {
                    Program.Log("T2: 4");
                    AddResource(1, 0, 4, ref resources);
                }
            }

            //give in chunks of whole units while possible
            while (resources >= 6)
            {
                Program.Log("resources: " + resources);
                int tier = Program.Random.SelectValue(new Dictionary<int, int> { { 0, 6 }, { 1, 3 }, { 2, 2 } });
                AddResource(tier, 0, GetTierCost(tier), ref resources);
            }
            //remaining are evenly distributed
            Program.Log("last: " + resources);
            while (resources >= 1)
                AddResource(Program.Random.Next(3), 0, 1, ref resources);
        }

        private int BalanceOrder(int order, int add)
        {
            //turn order balancing 
            order += add;// + Program.Random.Next(2);
            while (order > 0)
                AddResource(Program.Random.Next(3), 1, 1, ref order);
            return order;
        }

        private int AddRelic()
        {
            int relic = 6;
            var types = Program.Random.Iterate(3);
            if (Program.Random.Bool() && types.All(GetPlayerExtra))
            {
                int inc = Program.Random.RangeInt(0, 3);

                relic += inc;
                //cost 4 resources per relic
                inc = 18 - inc * 4;

                foreach (int a in types)
                    Resources[a, 0]--;
                while (inc > 0)
                {
                    Program.Log("AddRelic: " + inc);
                    AddResource(Program.Random.Next(3), 1, Program.Random.RangeInt(1, inc), ref inc);
                }
            }
            return relic;
        }

        private static int GetTierCost(int tier)
        {
            return (tier + 1) * 2;
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
