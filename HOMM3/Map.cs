using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    class Map
    {
        private readonly string Name;
        private readonly int Minimum_Size = -1;
        private readonly int Maximum_Size = -1;
        private readonly string Artifacts;
        private readonly string Combo_Arts;
        private readonly string Spells;
        private readonly string Secondary_skills;
        private readonly string Objects;
        private readonly string Rock_blocks;
        private readonly double Zone_sparseness = -1;
        private readonly string Special_weeks_disabled;
        private readonly string Spell_Research;
        private readonly string Anarchy;
        public Map(int sizeInt)
        {
            Name = Program.Name;
            Minimum_Size = Maximum_Size = sizeInt;
            switch (Program.rand.Next(4))
            {
                case 0:
                    Rock_blocks = "x";
                    break;
                case 1:
                    Rock_blocks = Custom().ToString();
                    break;
            }
            Zone_sparseness = Program.rand.Bool() ? -1 : Custom();
            Spell_Research = Program.rand.Bool(.65) ? "x" : null;
            Anarchy = Program.rand.Bool(.26) ? "x" : null;
        }
        private static double Custom()
        {
            return (Program.rand.GaussianOEInt(10, .13, .13) / 10.0);
        }

        public void Output(List<List<string>> output, ref int x, ref int y)
        {
            int sx = x;
            y++;
            Program.Output(output, x++, y, "Name");
            Program.Output(output, x++, y, "Minimum Size");
            Program.Output(output, x++, y, "Maximum Size");
            Program.Output(output, x++, y, "Artifacts");
            Program.Output(output, x++, y, "Combo Arts");
            Program.Output(output, x++, y, "Spells");
            Program.Output(output, x++, y, "Secondary skills");
            Program.Output(output, x++, y, "Objects");
            Program.Output(output, x++, y, "Rock blocks");
            Program.Output(output, x++, y, "Zone sparseness");
            Program.Output(output, x++, y, "Special weeks disabled");
            Program.Output(output, x++, y, "Spell Research");
            Program.Output(output, x++, y, "Anarchy");
            x = sx;
            y++;
            Program.Output(output, x++, y, Name);
            Program.Output(output, x++, y, Minimum_Size);
            Program.Output(output, x++, y, Maximum_Size);
            Program.Output(output, x++, y, Artifacts);
            Program.Output(output, x++, y, Combo_Arts);
            Program.Output(output, x++, y, Spells);
            Program.Output(output, x++, y, Secondary_skills);
            Program.Output(output, x++, y, Objects);
            Program.Output(output, x++, y, Rock_blocks);
            Program.Output(output, x++, y, Zone_sparseness);
            Program.Output(output, x++, y, Special_weeks_disabled);
            Program.Output(output, x++, y, Spell_Research);
            Program.Output(output, x++, y, Anarchy);
        }
    }
}
