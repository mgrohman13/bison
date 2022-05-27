using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    public class Connections
    {
        private static readonly double borderGuardChance;
        static Connections()
        {
            borderGuardChance = Program.rand.GaussianCapped(.26, .26);
        }

        public readonly Zone zone1;
        public readonly Zone zone2;

        private readonly bool primary;
        private bool ground;
        private readonly bool wide;
        private readonly bool borderGuard;
        private bool? road;
        private int strength;

        //output
        private readonly Zones zones;
        private readonly Options options;
        private readonly Restrictions restrictions;

        public Connections(Zone zone1, Zone zone2, bool primary, bool ground, double wide, bool canBorderGuard, bool? road, double deviation, double strength)
        {
            this.zone1 = zone1;
            this.zone2 = zone2;
            this.primary = primary;
            this.ground = ground;
            this.wide = Program.rand.Bool(wide);
            this.borderGuard = canBorderGuard && Program.rand.Bool(borderGuardChance);
            this.road = road;
            this.strength = Program.rand.GaussianOEInt(strength, deviation, .013);

            zones = new();
            options = new();
            restrictions = new();
        }

        private static double GenerateStrongest()
        {
            return Program.rand.GaussianOE(Program.rand.Range(16900, 26000), .13, .13);
        }
        public static List<Connections> InitConnections(Player[] players, double size, int numZones, bool pairPlayers)
        {
            List<Connections> connections = new();

            double[] strengths = new double[] {
                Program.rand.GaussianOE( 7800, .26, .13),
                Program.rand.GaussianOE(16900, .21, .13),
                Program.rand.GaussianOE(Program.rand.Range(13000, 21000), .169, .13),
                GenerateStrongest(),
            };
            Array.Sort(strengths);
            double baseInternalStr = strengths[0];
            bool select = strengths[1] > strengths[2] || strengths[1] > strengths[0] * Program.rand.Range(1.69, 2.1);
            double baseExternalStr = strengths[select ? 1 : 2];
            double otherInternalStr = strengths[select ? 2 : 1];
            double otherExternalStr = strengths[3];
            double extraStr = Program.rand.Range(baseInternalStr, baseExternalStr * .78);

            double wideWeight = (.65 + players.Length) / (1.69 + (double)numZones);
            wideWeight = Program.rand.GaussianCapped(wideWeight, .169, Math.Max(0, 2 * wideWeight - 1));

            //primary internal connections
            int count = players.Min(p => p.Zones.Count) + 1;
            for (int a = 1; a < count; a++)
            {
                bool primary = true;
                bool ground = Program.rand.Bool(.91);
                double wide = Program.rand.Weighted(wideWeight);
                bool canBorderGuard = false;
                bool? road = null;
                double deviation = .13;
                double strength;
                do
                    strength = Program.rand.GaussianOE(baseInternalStr * (1 - wide), .39, .052);
                while (strength > baseExternalStr);

                if (a == count - 1)
                {
                    //extra zones 
                    ground &= Program.rand.Bool(.78);
                    wide = 0;
                    strength = Program.rand.GaussianOE(extraStr, .13, .039);
                }

                foreach (Player player in Program.rand.Iterate(players))
                    if (a < player.Zones.Count)
                    {
                        Zone z1 = player.Zones[a - 1], z2 = player.Zones[a];
                        AddConnection(connections, z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
                    }
            }

            //extra internal connections 
            double zonesPerPlayer = numZones / (double)players.Length;
            int internalConnections = zonesPerPlayer > 2 ? Program.rand.GaussianCappedInt(Math.Sqrt(zonesPerPlayer - 1.69), .26) : 0;
            foreach (Player player in Program.rand.Iterate(players))
            {
                int addConnections = internalConnections + Program.rand.Next(2);
                if (player.Zones.Count > 1)
                    for (int b = 0; b < addConnections; b++)
                        if (Program.rand.Bool())
                        {
                            bool primary = false;
                            bool ground = false;
                            double wide = 0;
                            bool canBorderGuard = Program.rand.Bool(.39);
                            bool? road = null;
                            double deviation = 0;
                            double strength = Program.rand.GaussianOE(otherInternalStr, .26, .091);

                            var zones = Program.rand.Iterate(player.Zones).Take(2).ToList();
                            Zone z1 = zones[0];
                            Zone z2 = zones[1];
                            AddConnection(connections, z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
                        }
            }

            //primary external connections
            HashSet<Player> tempPlayers = players.ToHashSet();
            tempPlayers.Remove(Program.rand.SelectValue(tempPlayers));
            while (tempPlayers.Any())
                foreach (Player player in Program.rand.Iterate(players))
                    if (tempPlayers.Any() && !tempPlayers.Contains(player))
                    {
                        bool primary = true;
                        bool ground = Program.rand.Bool();
                        double wide = 0;
                        bool canBorderGuard = false;
                        bool? road = null;
                        double deviation = 0;
                        double strength;
                        do
                            strength = Program.rand.GaussianOE(baseExternalStr, .078, .065);
                        while (strength > otherExternalStr);

                        Player p2 = Program.rand.SelectValue(tempPlayers);
                        tempPlayers.Remove(p2);

                        static Zone SelZone(Player p)
                        {
                            Zone zone = p.Zones[^1];
                            if (zone.extra && Program.rand.Bool())
                                zone = p.Zones[^2];
                            return zone;
                        }
                        Zone z1 = SelZone(player);
                        Zone z2 = SelZone(p2);

                        AddConnection(connections, z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
                    }

            //extra external connections
            int externalConnections = Program.rand.GaussianOEInt(Math.Sqrt(numZones - 1.69) / 2.6, .39, .065);
            if (pairPlayers)
                --externalConnections;
            HashSet<Zone> tempZones = players.SelectMany(p => p.Zones).ToHashSet();
            for (int k = 0; k < externalConnections; k++)
            {
                bool primary = false;
                bool ground = false;
                double wide = 0;
                bool canBorderGuard = true;
                bool? road = Program.rand.Bool(.169);
                double deviation = .065;
                double strength = Program.rand.GaussianOE(otherExternalStr, .078, .078);

                foreach (Player player in Program.rand.Iterate(players))
                    if (tempZones.Any() && Program.rand.Bool(1 / Math.Sqrt(players.Length)))
                    {
                        Zone z1 = player.Home;
                        if (player.Zones.Count > 1 && Program.rand.Bool(.91))
                            z1 = Program.rand.SelectValue(player.Zones.Except(Enumerable.Repeat(z1, 1)));
                        Zone z2;
                        do
                        {
                            z2 = Program.rand.SelectValue(tempZones);
                        } while (player.Zones.Contains(z2));
                        tempZones.Remove(z2);
                        AddConnection(connections, z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
                    }
            }

            //pair up players for early combat 
            if (pairPlayers)
            {
                bool primary = true;
                bool ground = Program.rand.Bool(.65);
                double wide = 0;
                bool canBorderGuard = false;
                bool? road = null;
                double deviation = .091;
                double strength;
                do
                    strength = Program.rand.GaussianOE(baseInternalStr, .169, .026);
                while (strength > baseExternalStr);

                tempPlayers = players.ToHashSet();
                while (tempPlayers.Count > 1)
                {
                    Player p1 = Program.rand.SelectValue(tempPlayers);
                    tempPlayers.Remove(p1);
                    Player p2 = Program.rand.SelectValue(tempPlayers);
                    tempPlayers.Remove(p2);
                    Player.SetPair(p1, p2);
                    Zone z1 = Program.rand.SelectValue(p1.Zones);
                    Zone z2 = Program.rand.SelectValue(p2.Zones);
                    AddConnection(connections, z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
                }
            }

            return connections;
        }
        private static void AddConnection(List<Connections> connections, Zone z1, Zone z2, bool primary, bool ground, double wide, bool canBorderGuard, bool? road, double deviation, double strength)
        {
            if (z1 == z2)
                throw new Exception();
            Connections connection = new(z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
            Zone.AddConnection(z1, z2, connection);
            connections.Add(connection);
        }
        private void IncreaseStrength()
        {
            strength = Program.rand.GaussianOEInt(strength * 1.3, .13, .13);
            strength = Math.Max(strength, Program.rand.Round(GenerateStrongest()));
        }

        internal static void Generate(List<Connections> allConnections, List<Connections> connections, int numPlayers)
        {
            if (connections.Count > 1)
                TrimConnections(allConnections, connections);
            foreach (Connections connection in Program.rand.Iterate(connections))
                connection.Generate(numPlayers);
        }
        private static void TrimConnections(List<Connections> allConnections, List<Connections> connections)
        {
            HashSet<Connections> keep = new();

            var wide = connections.Where(c => c.wide);
            if (wide.Any())
            {
                //only one wide connection makes sense
                keep.Add(Program.rand.SelectValue(wide));
                //if any wide, keep only one of the others 
                var others = connections.Where(c => !c.wide && !c.borderGuard);
                if (others.Any())
                {
                    Connections connection = Program.rand.SelectValue(others);
                    connection.road = Program.rand.Bool(.78);
                    keep.Add(connection);
                }
            }
            else
            {
                bool primary = connections.Any(c => c.primary);

                var borderGuards = connections.Where(c => c.borderGuard);
                //if this is a primary connection, dont keep any border guards
                bool borderGuard = !primary && borderGuards.Any();
                //only one border guard makes sense
                if (borderGuard)
                    keep.Add(Program.rand.SelectValue(borderGuards));

                var others = connections.Where(c => !c.borderGuard);
                var max = others.Where(c => c.strength == others.Max(c => c.strength));
                //if a border guard, only keep the strongest other connection and ensure it is sufficiently high as to have a purpose
                Connections strongest = Program.rand.SelectValue(max);
                if (borderGuard)
                    strongest.IncreaseStrength();
                keep.Add(strongest);

                if (!borderGuard)
                {
                    //keep the minimum and maximum strength connections, and a small chance to keep each additional one
                    var min = others.Where(c => c.strength == others.Min(c => c.strength));
                    keep.Add(Program.rand.SelectValue(min));
                    if (connections.Count > 2)
                    {
                        double chance = 1 / (connections.Count - 1.3);
                        foreach (Connections connection in others)
                            if (Program.rand.Bool(chance))
                                keep.Add(connection);
                    }
                }
            }

            allConnections.RemoveAll(c => connections.Contains(c) && !keep.Contains(c));
            connections.RemoveAll(c => !keep.Contains(c));

            //if any are ground, set all to ground
            bool ground = connections.Any(c => c.ground);
            connections.ForEach(c => c.ground = ground);
        }

        private void Generate(int numPlayers)
        {
            zones.Generate(zone1.Id, zone2.Id);
            options.Generate(ground, borderGuard, wide, road, strength);
            restrictions.Generate(numPlayers);
        }

        public class Zones
        {
            private int Zone_1 = -1;
            private int Zone_2 = -1;

            public void Generate(int id1, int id2)
            {
                Zone_1 = id1;
                Zone_2 = id2;
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Connections[] connections)
            {
                int sx = x;
                Program.Output(output, x++, y, "Zone 1");
                Program.Output(output, x++, y, "Zone 2");
                foreach (Connections connection in connections)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, connection.zones.Zone_1);
                    Program.Output(output, x++, y, connection.zones.Zone_2);
                }
            }
        }
        public class Options
        {
            private int Value = -1;
            private string Wide;
            //ignores value
            private string Border_Guard;
            private string Road;
            //(default)
            //ground
            //underground
            //teleport
            //random
            private string Type;
            private string Fictive;
            private string Portal_repulsion;

            public void Generate(bool ground, bool borderGuard, bool wide, bool? road, int strength)
            {
                Value = strength;
                if (Program.rand.Bool(.78))
                    Portal_repulsion = "x";
                if (wide)
                    Wide = "x";
                if (road.HasValue)
                    if (road.Value)
                        Road = "+";
                    else
                        Road = "-";
                if (ground)
                    Type = "ground";
                if (borderGuard)
                    Border_Guard = "x";
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Connections[] connections)
            {
                int sx = x;
                Program.Output(output, x++, y, "Value");
                Program.Output(output, x++, y, "Wide");
                Program.Output(output, x++, y, "Border Guard");
                Program.Output(output, x++, y, "Road");
                Program.Output(output, x++, y, "Type");
                Program.Output(output, x++, y, "Fictive");
                Program.Output(output, x++, y, "Portal repulsion");
                foreach (Connections connection in connections)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, connection.options.Value);
                    Program.Output(output, x++, y, connection.options.Wide);
                    Program.Output(output, x++, y, connection.options.Border_Guard);
                    Program.Output(output, x++, y, connection.options.Road);
                    Program.Output(output, x++, y, connection.options.Type);
                    Program.Output(output, x++, y, connection.options.Fictive);
                    Program.Output(output, x++, y, connection.options.Portal_repulsion);
                }
            }
        }

        public class Restrictions
        {
            private int Minimum_human_positions = -1;
            private int Maximum_human_positions = -1;
            private int Minimum_total_positions = -1;
            private int Maximum_total_positions = -1;

            public void Generate(int numPlayers)
            {
                Minimum_human_positions = Maximum_human_positions = Minimum_total_positions = Maximum_total_positions = numPlayers;
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Connections[] connections)
            {
                int sx = x;
                Program.Output(output, x++, y, "Minimum human positions");
                Program.Output(output, x++, y, "Maximum human positions");
                Program.Output(output, x++, y, "Minimum total positions");
                Program.Output(output, x++, y, "Maximum total positions");
                foreach (Connections connection in connections)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, connection.restrictions.Minimum_human_positions);
                    Program.Output(output, x++, y, connection.restrictions.Maximum_human_positions);
                    Program.Output(output, x++, y, connection.restrictions.Minimum_total_positions);
                    Program.Output(output, x++, y, connection.restrictions.Maximum_total_positions);
                }
            }
        }

        public static void Output(List<List<string>> output, ref int x, ref int y, Connections[] connections)
        {
            int sy = y;
            Program.Output(output, x, y++, "Zones");
            Zones.Output(output, ref x, ref y, connections);

            y = sy;
            Program.Output(output, x, y++, "Options");
            Options.Output(output, ref x, ref y, connections);

            y = sy;
            Program.Output(output, x, y++, "Restrictions");
            Restrictions.Output(output, ref x, ref y, connections);
        }
    }
}
