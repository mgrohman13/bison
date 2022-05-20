using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    public class Connections
    {
        public readonly Zones zones;
        public readonly Options options;
        public readonly Restrictions restrictions;
        public Connections(int id1, int id2, bool ground, bool canBG, double wide, bool? road, double value, bool strong)
        {
            zones = new(id1, id2);
            options = new(ground, canBG, wide, road, value, strong);
            restrictions = new();
        }
        public class Zones
        {
            public readonly int Zone_1 = -1;
            public readonly int Zone_2 = -1;
            public Zones(int id1, int id2)
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
            private static readonly double bg;
            static Options()
            {
                bg = Program.rand.GaussianCapped(.26, .26);
            }

            public readonly int Value = -1;
            public readonly string Wide;
            //ignores value
            public readonly string Border_Guard;
            public readonly string Road;
            //(default)
            //ground
            //underground
            //teleport
            //random
            public readonly string Type;
            public readonly string Fictive;
            public readonly string Portal_repulsion;
            public Options(bool ground, bool canBG, double wide, bool? road, double value, bool strong)
            {
                Value = Program.rand.GaussianOEInt(value, strong ? .065 : .13, .078);
                if (Program.rand.Bool(.78))
                    Portal_repulsion = "x";
                if (Program.rand.Bool(wide))
                    Wide = "x";
                if (road.HasValue)
                    if (road.Value)
                        Road = "+";
                    else
                        Road = "-";
                if (ground)
                    Type = "ground";
                if (canBG && Program.rand.Bool(bg))
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
            public readonly int Minimum_human_positions = -1;
            public readonly int Maximum_human_positions = -1;
            public readonly int Minimum_total_positions = -1;
            public readonly int Maximum_total_positions = -1;

            public Restrictions()
            {
                Minimum_human_positions = Maximum_human_positions = Minimum_total_positions = Maximum_total_positions = Program.players;
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
