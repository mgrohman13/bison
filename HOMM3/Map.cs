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
        private string Objects;
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
            Spell_Research = Program.rand.Bool() ? "x" : null;
            Anarchy = Program.rand.Bool(.13) ? "x" : null;

        }
        public void Generate(double size, int numZones)
        {
            Objects = ObjectSetting.Generate(size, numZones);
        }
        private static double Custom()
        {
            return (Program.rand.GaussianOEInt(10.4, .26, .13) / 10.0);
        }

        private class ObjectSetting
        {
            private readonly string id;
            private readonly int? value;
            private readonly int? frequency;
            private readonly int? maxOnMap;
            private readonly int? maxPerZone;

            public ObjectSetting(string id, int? value = null, int? frequency = null, int? maxOnMap = null, int? maxPerZone = null)
            {
                if (value.HasValue)
                    value = Program.rand.GaussianCappedInt(value.Value, .13, 100);
                if (frequency.HasValue)
                    frequency = Program.rand.GaussianCappedInt(frequency.Value, .13, 1);

                this.id = id;
                this.value = value;
                this.frequency = frequency;
                this.maxOnMap = maxOnMap;
                this.maxPerZone = maxPerZone;

                Verify(this.value, 100);
                Verify(this.frequency);
                Verify(this.maxOnMap);
                Verify(this.maxPerZone);
            }
            private static void Verify(int? test, int min = 1)
            {
                if (test.HasValue && test.Value < min)
                    throw new Exception();
            }

            public static string Generate(double size, int numZones)
            {
                List<ObjectSetting> settings = new();

                static int Range(int a, double b) => Program.rand.RangeInt(a, Program.rand.Round(b));
                static int Weighted(int a, int b) => a + Program.rand.WeightedInt(b - a, Program.rand.DoubleHalf());
                int Max() => 1 + Program.rand.GaussianCappedInt(Math.Sqrt(size) / 1.69, .13);

                //neutral dragon dwellings
                //Frozen Peaks                  +17 62 d 40  
                settings.Add(new ObjectSetting("+17 62", frequency: 480, maxOnMap: 4, maxPerZone: 2));
                //Crystal Cave                  +17 63 d 40 
                settings.Add(new ObjectSetting("+17 63", frequency: 80, maxOnMap: Range(2, 1 + Max()), maxPerZone: Range(1, 2)));
                //Magic Forest                  +17 64 d 40  
                settings.Add(new ObjectSetting("+17 64", frequency: 80, maxOnMap: Range(2, 1 + Max()), maxPerZone: Range(1, 2)));
                //Sulfurous Lair                +17 65 d 40  
                settings.Add(new ObjectSetting("+17 65", frequency: 80, maxOnMap: Range(2, 1 + Max()), maxPerZone: Range(1, 2)));

                //balance tweaks
                //Cover of Darkness             +15 0 500 25 
                settings.Add(new ObjectSetting("+15 0", value: Weighted(500, 5000), frequency: Range(1, 4), maxPerZone: 1));
                //Dragon Utopia                 +25 0 10000 100 
                settings.Add(new ObjectSetting("+25 0", maxOnMap: Max(), maxPerZone: 1));
                int mult = Range(10, 20);
                //Magic Spring                  +48 0 500 50  
                settings.Add(new ObjectSetting("+48 0", value: Weighted(500, 500 * mult)));
                //Magic Well                    +49 0 250 100  
                settings.Add(new ObjectSetting("+49 0", value: Weighted(250, 250 * mult), frequency: Range(100, 200)));
                //Stables                       +94 0 200 40
                settings.Add(new ObjectSetting("+94 0", value: Weighted(200, 4000), maxPerZone: 1));
                //Trailblazer                   +144 11 200 40
                settings.Add(new ObjectSetting("+144 11", value: Weighted(200, 4000), maxPerZone: 1));
                //Trading Post                  +99 0 3000 100 d d 
                settings.Add(new ObjectSetting("+99 0", value: 12000, frequency: 25, maxOnMap: Range(1, 2), maxPerZone: 1));
                //Warlock's Lab                 +144 9 10000 100 
                settings.Add(new ObjectSetting("+144 9", value: 20000, frequency: 10, maxOnMap: 1));

                //enable objects
                //Keymaster's Tent              +10 n 5000 10 
                //Keymaster's Tent              +10 n 20000 10 
                while (Program.rand.Bool(.65))
                    settings.Add(new ObjectSetting("+10 n", value: Range(100, 20000), frequency: Range(1, 39)));
                //Cartographer Sea              +13 0 5000 20 
                settings.Add(new ObjectSetting("+13 0", value: Range(10000, 15000), frequency: 10, maxOnMap: 1));
                //Cartographer Ground           +13 1 10000 20 
                settings.Add(new ObjectSetting("+13 1", value: Range(20000, 30000), frequency: 10, maxOnMap: 1));
                //Cartographer Underground      +13 2 7500 20 
                settings.Add(new ObjectSetting("+13 2", value: Range(15000, 22500), frequency: 10, maxOnMap: 1));
                int eyes = Range(1, Program.rand.GaussianCappedInt(13 / numZones, .13));
                if (eyes > 0)
                {
                    int hut = Range(4000, 4000 + 2000 * eyes * Math.Sqrt(numZones));
                    //Hut of the Magi               +37 0 100 25 
                    settings.Add(new ObjectSetting("+37 0", value: hut, maxOnMap: Range(1, Max()), maxPerZone: 1));
                    //Eye of the Magi               +27 0 100 50 
                    settings.Add(new ObjectSetting("+27 0", value: 1750, frequency: 100, maxPerZone: eyes));
                }
                //Sanctuary                     +80 0 100 50 
                settings.Add(new ObjectSetting("+80 0", value: Weighted(100, 4000), frequency: 25, maxOnMap: Range(1, Max()), maxPerZone: 1));
                //Ancient Lamp                  +145 0 5000 200 
                settings.Add(new ObjectSetting("+145 0"));
                //Hill Fort (traditional)       +35 0 7000 20 
                settings.Add(new ObjectSetting("+35 0", value: Range(14000, 28000), frequency: 10, maxOnMap: 1));
                //Hill Fort(nerfed)            +35 1 7000 20
                settings.Add(new ObjectSetting("+35 1", value: 3500));

                return Program.rand.Iterate(settings).Select(s => s.Output()).Aggregate((a, b) => a + " " + b);
            }

            //may look into in future:
            //  object  id  id  value   freq    map     zone
            //Spell Scroll 1            +93 0 1 500 30 d d 
            //Spell Scroll 2            +93 0 2 2000 30 d d 
            //Spell Scroll 3            +93 0 3 3000 30 d d 
            //Spell Scroll 4            +93 0 4 8000 30 d d 
            //Spell Scroll 5            +93 0 5 10000 30 d d 
            //Spell Scroll (adventure)  +93 0 6 20000 30 d d 
            //Quest Experience  5000    +83 n 1 5000 2000 10 d d 
            //Quest Experience 10000    +83 n 1 10000 5333 10 d d 
            //Quest Experience 15000    +83 n 1 15000 8666 10 d d 
            //Quest Experience 20000    +83 n 1 20000 12000 10 d d 
            //Quest Gold  5000          +83 n 2 5000 2000 10 d d 
            //Quest Gold 10000          +83 n 2 10000 5333 10 d d 
            //Quest Gold 15000          +83 n 2 15000 8666 10 d d 
            //Quest Gold 20000          +83 n 2 20000 12000 10 d d 

            private string Output()
            {
                static string Map(int? v) => v.HasValue ? v.ToString() : "d";
                return string.Format("{0} {1} {2} {3} {4}", id, Map(value), Map(frequency), Map(maxOnMap), Map(maxPerZone));
            }
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
