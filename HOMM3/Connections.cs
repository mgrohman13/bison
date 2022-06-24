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

        public bool Ground => ground;
        public int Strength => strength;

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
            this.borderGuard = !this.wide && canBorderGuard && Program.rand.Bool(borderGuardChance);
            this.road = road;
            this.strength = Program.rand.GaussianOEInt(strength, deviation, .013);

            zones = new();
            options = new();
            restrictions = new();
        }

        private static double GenerateStrongest()
        {
            return Program.rand.GaussianOE(Program.rand.Range(26000, 52000), .091, .169);
        }
        public static List<Connections> InitConnections(Player[] players, double size, double numZones)
        {
            double playerCount = players.Length;
            List<Connections> connections = new();

            double range = Program.rand.Range(16900, 39000);
            double[] strengths = new double[] {
                Program.rand.GaussianOE( 9100, .26 , .078),
                Program.rand.GaussianOE(26000, .13 , .13 ),
                Program.rand.GaussianOE(range, .169, .091),
                GenerateStrongest(),
            };
            Log.Out(0, "Connections strengths: {0}", strengths.ToList());
            var temp = strengths.ToArray();
            Array.Sort(strengths);
            if (!temp.SequenceEqual(strengths))
                Log.Out(0, "strengths: {0}", strengths.ToList());
            double baseInternalStr = strengths[0];
            bool select = strengths[1] > strengths[0] * Program.rand.Range(2.1, Math.E);
            Log.Out("select: {0}", select);
            double baseExternalStr = strengths[select ? 1 : 2];
            double otherInternalStr = strengths[select ? 2 : 1];
            double otherExternalStr = strengths[3];
            double extraStr = Program.rand.Range(baseInternalStr, baseExternalStr * .78);
            Log.Out(0, "baseInternalStr, baseExternalStr, otherInternalStr, otherExternalStr, extraStr: {0}, {1}, {2}, {3}, {4}",
                baseInternalStr, baseExternalStr, otherInternalStr, otherExternalStr, extraStr);

            double wideWeight = (.65 + Program.NumPlayers) / (1.69 + numZones);
            Log.Out("wideWeight: {0}", wideWeight);
            wideWeight = Program.rand.GaussianCapped(wideWeight, .169, Math.Max(0, 2 * wideWeight - 1));
            Log.Out("wideWeight: {0}", wideWeight);

            static bool? Road(double chanceDefault, double roadChance) =>
                Program.rand.Bool(chanceDefault) ? null : Program.rand.Bool(roadChance);

            //primary internal connections
            int min = players.Min(p => p.Zones.Count);
            int max = players.Max(p => p.Zones.Count);
            for (int a = 1; a < max; a++)
            {
                bool primary = true;
                bool ground = Program.rand.Bool(.91);
                double wide = Program.rand.Weighted(wideWeight);
                bool canBorderGuard = false;
                bool? road = Road(.78, .78);
                double deviation = .13;
                double strength = Program.GaussianOEWithMax(baseInternalStr * (1 - wide), .39, .039, baseExternalStr);

                if (a >= min)
                {
                    //extra zones 
                    ground &= Program.rand.Bool(.78);
                    wide = 0;
                    strength = Program.GaussianOEWithMax(extraStr, .13, .052, baseExternalStr);
                }

                foreach (Player player in Program.rand.Iterate(players))
                    if (a < player.Zones.Count)
                    {
                        Zone z1 = player.Zones[a - 1], z2 = player.Zones[a];
                        AddConnection(connections, z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
                    }
            }

            //extra internal connections 
            double zonesPerPlayer = numZones / playerCount;
            double avgInternalConnections = Math.Sqrt(zonesPerPlayer - 1.69);
            Log.Out("avg internalConnections: {0}", avgInternalConnections);
            int internalConnections = zonesPerPlayer > 2 ? Program.rand.GaussianCappedInt(avgInternalConnections, .26) : 0;
            Log.Out("internalConnections: {0}", internalConnections);
            double[] wides = new double[internalConnections + 1];
            for (int b = 0; b < internalConnections + 1; b++)
                wides[b] = Program.rand.Weighted(.39, .21);
            Log.Out("wides: {0}", wides.ToList());
            foreach (Player player in Program.rand.Iterate(players))
            {
                int addConnections = internalConnections + Program.rand.Next(2);
                if (player.Zones.Count > 1)
                {
                    wides = Program.rand.Iterate(wides).ToArray();
                    for (int c = 0; c < addConnections; c++)
                        if (Program.rand.Bool())
                        {
                            bool primary = false;
                            bool ground = false;
                            double wide = wides[c];
                            bool canBorderGuard = Program.rand.Bool(.39);
                            bool? road = Road(.39, .65);
                            double deviation = 0;
                            double strength = Program.rand.GaussianOE(Program.rand.Range(baseInternalStr * (1 - wide), otherInternalStr * 1.3), .26, .104);

                            var zones = Program.rand.Iterate(player.Zones).Take(2).ToList();
                            Zone z1 = zones[0];
                            Zone z2 = zones[1];
                            AddConnection(connections, z1, z2, primary, ground, wide, canBorderGuard, road, deviation, strength);
                        }
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
                        bool? road = Road(.65, .52);
                        double deviation = 0;
                        double strength = Program.GaussianOEWithMax(baseExternalStr, .078, .065, otherExternalStr);

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
            double avgExternalConnections = Math.Sqrt(numZones - 1.69) / 2.6;
            Log.Out("avg externalConnections: {0}", avgExternalConnections);
            int externalConnections = Program.rand.GaussianOEInt(avgExternalConnections, .39, .065);
            Log.Out("externalConnections: {0}", externalConnections);
            if (Program.PairPlayer)
            {
                externalConnections -= Program.rand.RangeInt(0, 2);
                Log.Out("externalConnections: {0}", externalConnections);
            }
            HashSet<Zone> tempZones = players.SelectMany(p => p.Zones).ToHashSet();
            for (int c = 0; c < externalConnections; c++)
            {
                bool primary = false;
                bool ground = false;
                double wide = 0;
                bool canBorderGuard = true;
                bool? road = Road(.26, .26);
                double deviation = .065;
                double strength = Program.rand.GaussianOE(otherExternalStr, .078, .091);

                foreach (Player player in Program.rand.Iterate(players))
                    if (tempZones.Any(z => z.player != player) && Program.rand.Bool(1 / Math.Sqrt(playerCount)))
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

            //paired connections
            double avgPairConnections = Math.Sqrt(zonesPerPlayer);
            Log.Out("avg pairConnections: {0}", avgPairConnections);
            int pairConnections = Program.rand.GaussianOEInt(avgPairConnections, .26, .13, 1);
            Log.Out("pairConnections: {0}", pairConnections);
            for (int d = 0; d < pairConnections; d++)
            {
                bool primary = true;
                double ground = .65;
                double wide = 0;
                bool canBorderGuard = false;
                bool? road = Road(.52, .39);
                double deviation = .091;
                double strength = Program.GaussianOEWithMax(extraStr, .078, .078, baseExternalStr);

                tempPlayers = players.Where(p => p.Paired != null).ToHashSet();
                while (tempPlayers.Any())
                {
                    Player p1 = Program.rand.SelectValue(tempPlayers);
                    tempPlayers.Remove(p1);
                    Player p2 = p1.Paired;
                    tempPlayers.Remove(p2);

                    Zone z1 = Program.rand.SelectValue(p1.Zones);
                    Zone z2 = Program.rand.SelectValue(p2.Zones);
                    AddConnection(connections, z1, z2, primary, Program.rand.Bool(ground), wide, canBorderGuard, road, deviation, strength);
                }
            }

            //don't randomize connection order so primary connections are always first
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
            Log.Out("IncreaseStrength ({0})-({1}): {2}", zone1.Id, zone2.Id, strength);
            strength = Math.Max(strength, Program.rand.GaussianOEInt(strength * 1.3, .13, .13));
            Log.Out("IncreaseStrength: {0}", strength);
            strength = Math.Max(strength, Program.rand.Round(GenerateStrongest()));
            Log.Out("IncreaseStrength: {0}", strength);
        }

        public static void Generate(List<Connections> allConnections, List<Connections> connections)
        {
            if (connections.Count > 1)
                TrimConnections(allConnections, connections);
            foreach (Connections connection in Program.rand.Iterate(connections))
                connection.Generate();
        }
        private static void TrimConnections(List<Connections> allConnections, List<Connections> connections)
        {
            HashSet<Connections> keep = new();

            var wide = connections.Where(c => c.wide);
            if (wide.Any())
            {
                Log.Out("wide ({0})-({1}): {2}", connections.First().zone1.Id, connections.First().zone2.Id, connections.Count);
                //only one wide connection makes sense
                keep.Add(Program.rand.SelectValue(wide));
                //if any wide, keep only one of the others 
                var others = connections.Where(c => !c.wide && !c.borderGuard);
                if (others.Any())
                {
                    Log.Out("wide: additional");
                    Connections connection = Program.rand.SelectValue(others);
                    if (connection.road.HasValue ? !connection.road.Value : Program.rand.Bool())
                    {
                        connection.road = Program.rand.Bool(.78);
                        Log.Out("wide: road {0}", connection.road);
                    }
                    keep.Add(connection);
                }
            }
            else
            {
                bool primary = connections.Any(c => c.primary);

                var borderGuards = connections.Where(c => c.borderGuard);
                //if this is a primary connection, dont keep any border guards
                bool borderGuard = borderGuards.Any();
                if (borderGuard)
                    Log.Out("borderGuards ({0})-({1}): {2}", connections.First().zone1.Id, connections.First().zone2.Id, borderGuards.Count());
                borderGuard &= !primary;
                if (borderGuards.Any())
                    Log.Out("borderGuard: {0}", borderGuard);
                //only one border guard makes sense
                if (borderGuard)
                {
                    if (borderGuards.Count() > 1)
                        ;
                    keep.Add(Program.rand.SelectValue(borderGuards));
                }

                var others = connections.Where(c => !c.borderGuard);
                if (others.Any())
                {
                    Log.Out("({0})-({1}): maximum", connections.First().zone1.Id, connections.First().zone2.Id);

                    var max = others.Where(c => c.strength == others.Max(c => c.strength));
                    //if a border guard, only keep the strongest other connection and ensure it is sufficiently high as to have a purpose
                    Connections strongest = Program.rand.SelectValue(max);
                    if (borderGuard)
                        strongest.IncreaseStrength();
                    keep.Add(strongest);

                    if (!borderGuard)
                    {
                        Log.Out("minimum");
                        //keep the minimum and maximum strength connections, and a small chance to keep each additional one
                        var min = others.Where(c => c.strength == others.Min(c => c.strength));
                        keep.Add(Program.rand.SelectValue(min));
                        if (connections.Count > 2)
                        {
                            double chance = 1 / (connections.Count - 1.3);
                            Log.Out("chance: {0}", chance);
                            foreach (Connections connection in others)
                                if (Program.rand.Bool(chance))
                                    keep.Add(connection);
                        }
                    }
                }
                else
                    ;
            }

            allConnections.RemoveAll(c => connections.Contains(c) && !keep.Contains(c));
            connections.RemoveAll(c => !keep.Contains(c));

            //if any are ground, set all to ground
            bool ground = connections.Any(c => c.ground);
            connections.ForEach(c => c.ground = ground);
        }

        private void Generate()
        {
            zones.Generate(zone1.Id, zone2.Id);
            options.Generate(ground, borderGuard, wide, road, strength);
            restrictions.Generate();
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
            private readonly string Fictive;
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

            public void Generate()
            {
                Minimum_human_positions = Maximum_human_positions = Program.NumPlayers;
                Minimum_total_positions = Maximum_total_positions = Program.NumPlayers;
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
