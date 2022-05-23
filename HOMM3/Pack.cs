using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    class Pack
    {
        private readonly FieldCount Field_count;
        private readonly Options options;
        public Pack()
        {
            Field_count = new();
            options = new();
        }
        class FieldCount
        {
            private readonly int Town = 10;
            private readonly int Terrain = 10;
            private readonly int Zone_type = 4;
            private readonly int Pack_new = 7;
            private readonly int Map_new = 10;
            private readonly int Zone_new = 12;
            private readonly int Connection_new = 4;

            public void Output(List<List<string>> output, ref int x, ref int y)
            {
                int sx = x;
                Program.Output(output, x++, y, "Town");
                Program.Output(output, x++, y, "Terrain");
                Program.Output(output, x++, y, "Zone type");
                Program.Output(output, x++, y, "Pack new");
                Program.Output(output, x++, y, "Map new");
                Program.Output(output, x++, y, "Zone new");
                Program.Output(output, x++, y, "Connection new");
                x = sx;
                y++;
                Program.Output(output, x++, y, Town);
                Program.Output(output, x++, y, Terrain);
                Program.Output(output, x++, y, Zone_type);
                Program.Output(output, x++, y, Pack_new);
                Program.Output(output, x++, y, Map_new);
                Program.Output(output, x++, y, Zone_new);
                Program.Output(output, x++, y, Connection_new);
            }
        }
        class Options
        {
            private readonly string Name;
            private readonly string Description;
            private readonly string Town_selection;
            private readonly string Heroes;
            private readonly string Mirror;
            private readonly string Tags;
            private readonly int Max_Battle_Rounds = -1;
            public Options()
            {
                Name = Program.Name;
                Max_Battle_Rounds = Program.rand.GaussianCappedInt(100, .13, 52);
            }

            public void Output(List<List<string>> output, ref int x, ref int y)
            {
                int sx = x;
                Program.Output(output, x++, y, "Name");
                Program.Output(output, x++, y, "Description");
                Program.Output(output, x++, y, "Town selection");
                Program.Output(output, x++, y, "Heroes");
                Program.Output(output, x++, y, "Mirror");
                Program.Output(output, x++, y, "Tags");
                Program.Output(output, x++, y, "Max Battle Rounds");
                x = sx;
                y++;
                Program.Output(output, x++, y, Name);
                Program.Output(output, x++, y, Description);
                Program.Output(output, x++, y, Town_selection);
                Program.Output(output, x++, y, Heroes);
                Program.Output(output, x++, y, Mirror);
                Program.Output(output, x++, y, Tags);
                Program.Output(output, x++, y, Max_Battle_Rounds);
            }
        }

        public void Output(List<List<string>> output, ref int x, ref int y)
        {
            Program.Output(output, x, 1, "Field count");
            y = 2;
            Field_count.Output(output, ref x, ref y);

            Program.Output(output, x, 1, "Options");
            y = 2;
            options.Output(output, ref x, ref y);
        }
    }
}
