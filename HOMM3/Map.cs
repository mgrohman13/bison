using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    public class Map
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

        public class ObjectSetting : IComparable<ObjectSetting>
        {
            public readonly bool disable;
            public readonly string id;
            public readonly int? value;
            public readonly int? frequency;
            public readonly int? maxOnMap;
            public readonly int? maxPerZone;

            public ObjectSetting(string id, int? value, int? frequency, int? maxPerZone)
                : this(id, false, value, frequency, null, maxPerZone) { }
            private ObjectSetting(string id, bool disable = false, int? value = null, int? frequency = null, int? maxOnMap = null, int? maxPerZone = null)
            {
                if (value.HasValue)
                    value = Program.rand.GaussianCappedInt(value.Value, .13, 100);
                if (frequency.HasValue)
                    frequency = Program.rand.GaussianCappedInt(frequency.Value, .13, Math.Max(1, 2 * frequency.Value - 10000));

                this.disable = disable;
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

                //uniform distribution
                static int Range(int min, double max) => Program.rand.RangeInt(min, Program.rand.Round(max));
                //this generates a distribution skewed away from the center and towards the extremes
                static int Weighted(int start, int mult) => start + Program.rand.WeightedInt(start * (mult - 1), Program.rand.DoubleHalf());
                int Max() => 1 + Program.rand.GaussianCappedInt(Math.Sqrt(size) / 1.69, .13);

                // neutral dragon dwellings

                //Frozen Peaks                  +17 62 d 40  
                // since we go out of our way to generate treasure chunks big enough for these, make it very likely to appear when at all possible, but cap the number
                settings.Add(new("17 62", frequency: 480, maxOnMap: 4, maxPerZone: 2));
                //make the rest of the neutral dragon dwellings a little more frequent, and cap them so we don't spam them out
                //Crystal Cave                  +17 63 d 40 
                settings.Add(new("17 63", frequency: 80, maxOnMap: Range(2, 1 + Max()), maxPerZone: Range(1, 2)));
                //Magic Forest                  +17 64 d 40  
                settings.Add(new("17 64", frequency: 80, maxOnMap: Range(2, 1 + Max()), maxPerZone: Range(1, 2)));
                //Sulfurous Lair                +17 65 d 40  
                settings.Add(new("17 65", frequency: 80, maxOnMap: Range(2, 1 + Max()), maxPerZone: Range(1, 2)));

                // balance tweaks

                //Hill Fort(new)                +35 1 7000 20
                // over-nerfed, reduce value to reflect 
                settings.Add(new("35 1", value: 3500));
                int mult = Range(10, 20);
                ////Magic Spring                +48 0 500 50       (Note: This object cannot be guarded nor be part of a guarded group of objects)
                //// would modify this similar to magic well, but since it can't be guarded there's no real point
                //settings.Add(new("48 0", value: Weighted(500, 500 * mult)));
                //Magic Well                    +49 0 250 100  
                // more likely to appear, but chance they are guarded on some maps
                settings.Add(new("49 0", value: Weighted(250, mult), frequency: Range(100, 200)));
                //Stables                       +94 0 200 40
                // undervalued, chance to increase and be guarded on some maps
                mult = Range(20, 40);
                settings.Add(new("94 0", value: Weighted(200, mult), maxPerZone: 1));
                //Trailblazer                   +144 11 200 40
                // undervalued, chance to increase and be guarded on some maps
                settings.Add(new("144 11", value: Weighted(200, mult), maxPerZone: 1));
                //Trading Post                  +99 0 3000 100 d d 
                // very valuable, especially early on, increase value to reflect
                settings.Add(new("99 0", value: Range(6000, 15000), frequency: Range(25, 50), maxOnMap: Range(1, 2), maxPerZone: 1));
                //Warlock's Lab                 +144 9 10000 100 
                // ends up functionally removing any distinction between the different resources, so make more valuable and much less frequent 
                settings.Add(new("144 9", value: 20000, frequency: 10, maxOnMap: 1));
                //Obelisk                       +57 0 350 200
                // the grail can potentially make a game way too easy (or occasionally too difficult), disable it sometimes
                if (Program.rand.Bool())
                    settings.Add(new("57 0", true));

                // enable objects

                //potentially enable keymaster's tents at different random values
                //Keymaster's Tent              +10 n 5000 10 
                //Keymaster's Tent              +10 n 20000 10 
                while (Program.rand.Bool(.65))
                    settings.Add(new("10 n", value: Range(100, 20000), frequency: Range(1, 39)));
                //cartographers were thankfully nerfed to cost 10k gold, but still incredibly good, increase value and decrease frequency
                //Cartographer Sea              +13 0 5000 20 
                settings.Add(new("13 0", value: Range(10000, 15000), frequency: 10, maxOnMap: 1));
                //Cartographer Ground           +13 1 10000 20 
                // if the upper end of the range is chosen will be very unlikely to generate outside of zones with neutral dragon dwellings, that's fine
                settings.Add(new("13 1", value: Range(20000, 30000), frequency: 10, maxOnMap: 1));
                //Cartographer Underground      +13 2 7500 20 
                settings.Add(new("13 2", value: Range(15000, 22500), frequency: 10, maxOnMap: 1));
                //Cover of Darkness             +15 0 500 25 
                // include in the interest of enabling everything, however it is kind of annoying to play against and also really messes with the AI
                // very rare and chance to be guarded
                settings.Add(new("15 0", value: Weighted(500, Range(10, 40)), frequency: Range(1, 4), maxPerZone: 1));
                int eyes = Range(1, Program.rand.GaussianCappedInt(13 / numZones, .13));
                if (eyes > 0)
                {
                    //Eye of the Magi           +27 0 100 50 
                    // high enough value to appear outside of only zones with tiny treasure ranges, but not high enough to be guarded if by itself
                    int eyeFreq = 100;
                    ObjectSetting eye = new("27 0", value: 1750, frequency: eyeFreq, maxPerZone: eyes);
                    settings.Add(eye);
                    //Hut of the Magi           +37 0 100 25 
                    // higher potential hut value when more eyes
                    double eyeMult = eye.frequency.Value / (double)eyeFreq;
                    int hut = Program.rand.Round(eyeMult * 3900) + Range(0, eyeMult * 2100 * Math.Sqrt(eyes * numZones));
                    settings.Add(new("37 0", value: hut, maxOnMap: Range(1, Max()), maxPerZone: 1));
                }
                //Hill Fort (traditional)       +35 0 7000 20 
                // extremely powerful, very high value to reflect
                settings.Add(new("35 0", value: Range(14000, 28000), frequency: 10, maxOnMap: 1));
                //Sanctuary                     +80 0 100 50 
                // better than advertised and can be irritating, chance to be guarded and limit maximum
                settings.Add(new("80 0", value: Weighted(100, Range(20, 80)), frequency: 25, maxOnMap: Range(1, Max()), maxPerZone: 1));
                //Ancient Lamp                  +145 0 5000 200 
                // default values, just enable
                settings.Add(new("145 0"));

                // randomize otherwise fixed reward values

                HashSet<int> duplicates = new();
                //Prison                        +62 0 0 2500 30 
                //Prison                        +62 0 500000 30000 30
                //exp   =    0, 5000, 15000, 90000, 500000
                //value = 2500, 5000, 10000, 20000,  30000
                int prisons = Program.rand.GaussianOEInt(1.3 * Math.Sqrt(size), .39, .39);
                int prisonFreq = Program.rand.Round(390.0 / prisons);
                HashSet<int> prisonDefaults = new() { 0, 5000, 15000, 90000, 500000 };
                for (int a = 0; a < prisons; a++)
                {
                    int exp = Program.rand.GaussianOEInt(9100, 1, .5);
                    if (duplicates.Contains(exp))
                    {
                        a--;
                        continue;
                    }
                    duplicates.Add(exp);
                    int value = Program.rand.Round(7.8 * Math.Pow(2100 + exp, .78));
                    settings.Add(new("62 0 " + exp, value: value, frequency: prisonFreq, maxOnMap: 1));
                    if (prisonDefaults.Contains(exp))
                        prisonDefaults.Remove(exp);
                }
                foreach (int v in prisonDefaults)
                    settings.Add(new("62 0 " + v, true));
                duplicates.Clear();
                //Pandora's Box (experience)    +6 0 1 5000 6000 20 
                //Pandora's Box (experience)    +6 0 1 20000 24000 20 
                //exp   = 5000, 10000, 15000, 20000
                //value = 6000, 12000, 18000, 24000 
                //Pandora's Box (gold)          +6 0 2 5000 6000 5 
                //Pandora's Box (gold)          +6 0 2 20000 24000 5 
                //gold  = 5000, 10000, 15000, 20000
                //value = 5000, 10000, 15000, 20000 
                int pandorasBoxes = Program.GaussianOEIntWithMax(39, .26, .13, 91, 6);
                double pandoraFreq = (20 * 4 + 5 * 4) / (double)pandorasBoxes;
                HashSet<int> pandoraExps = new() { 5000, 10000, 15000, 20000 };
                HashSet<int> pandoraGolds = new() { 5000, 10000, 15000, 20000 };
                for (int a = 0; a < pandorasBoxes; a++)
                {
                    bool gold = Program.rand.Next(5) == 0;
                    int amount = Program.rand.GaussianOEInt(gold ? 10400 : 7800, 1, .5);
                    if (duplicates.Contains(amount * (gold ? -1 : 1)))
                    {
                        a--;
                        continue;
                    }
                    duplicates.Add(amount * (gold ? -1 : 1));
                    int value = Program.rand.Round(amount * (gold ? 1 : 1.3));
                    settings.Add(new(string.Format("6 0 {0} {1}", gold ? "2" : "1", amount), value: value, frequency: Program.rand.Round(pandoraFreq)));
                    if (gold && pandoraGolds.Contains(amount))
                        pandoraGolds.Remove(amount);
                    else if (!gold && pandoraExps.Contains(amount))
                        pandoraExps.Remove(amount);
                }
                foreach (int v in pandoraExps)
                    settings.Add(new("6 0 1 " + v, true));
                foreach (int v in pandoraGolds)
                    settings.Add(new("6 0 2 " + v, true));
                duplicates.Clear();
                //Seer's Hut (experience)       +83 n 1 5000 2000 10 
                //Seer's Hut (experience)       +83 n 1 20000 12000 10 
                //exp   = 5000, 10000, 15000, 20000
                //value = 2000,  5333,  8666, 12000 
                //Seer's Hut (gold)             +83 n 2 5000 2000 10 
                //Seer's Hut (gold)             +83 n 2 20000 12000 10 
                //gold  = 5000, 10000, 15000, 20000
                //value = 2000,  5333,  8666, 12000 
                int seersHuts = Program.GaussianOEIntWithMax(26, .26, .13, 65, 6);
                double seerFreq = (10 * 4 + 10 * 4) / (double)seersHuts;
                HashSet<int> seerExps = new() { 5000, 10000, 15000, 20000 };
                HashSet<int> seerGolds = new() { 5000, 10000, 15000, 20000 };
                for (int a = 0; a < seersHuts; a++)
                {
                    bool gold = Program.rand.Bool();
                    int amount = Program.rand.GaussianOEInt(gold ? 11700 : 10400, 1, .5, 3900);
                    if (duplicates.Contains(amount * (gold ? -1 : 1)))
                    {
                        a--;
                        continue;
                    }
                    duplicates.Add(amount * (gold ? -1 : 1));
                    int value = Program.rand.Round((amount - 2100) * .65 * (gold ? 1 : 1.3));
                    settings.Add(new(string.Format("83 n {0} {1}", gold ? "2" : "1", amount), value: value, frequency: Program.rand.Round(seerFreq)));
                    if (gold && seerGolds.Contains(amount))
                        seerGolds.Remove(amount);
                    else if (!gold && seerExps.Contains(amount))
                        seerExps.Remove(amount);
                }
                foreach (int v in seerExps)
                    settings.Add(new("83 n 1 " + v, true));
                foreach (int v in seerGolds)
                    settings.Add(new("83 n 2 " + v, true));
                duplicates.Clear();

                // creature banks - the RMG loves to spam these out, make a little less frequent  

                int CreatureBank(int initial) => Range(Range(1, initial * .26), Math.Sqrt(100 * initial) * Program.rand.Range(1, 1.69));
                //Cyclops Stockpile             +16 0 3000 100 
                settings.Add(new("16 0", frequency: CreatureBank(100)));
                //Dwarven Treasury              +16 1 2000 100 
                settings.Add(new("16 1", frequency: CreatureBank(100)));
                //Imp Cache                     +16 3 5000 100 
                settings.Add(new("16 3", frequency: CreatureBank(100)));
                //Medusa Stores                 +16 4 1500 100 
                settings.Add(new("16 4", frequency: CreatureBank(100)));
                //Naga Bank                     +16 5 3000 100 
                settings.Add(new("16 5", frequency: CreatureBank(100)));
                //Beholder's Sanctuary          +16 21 2500 100 
                settings.Add(new("16 21", frequency: CreatureBank(100)));
                //Temple of the Sea             +16 22 10000 100 
                settings.Add(new("16 22", frequency: CreatureBank(100)));
                //Mansion                       +16 24 5000 50 
                settings.Add(new("16 24", frequency: CreatureBank(50)));
                //Spit                          +16 25 1500 100 
                // enable
                settings.Add(new("16 25", frequency: CreatureBank(90)));
                //Black Tower                   +16 27 1500 100 
                settings.Add(new("16 27", frequency: CreatureBank(100)));
                //Churchyard                    +16 29 1500 100 
                settings.Add(new("16 29", frequency: CreatureBank(100)));
                //Ruins                         +16 32 1000 100 
                settings.Add(new("16 32", frequency: CreatureBank(100)));
                //Derelict Ship                 +24 0 4000 20 
                settings.Add(new("24 0", frequency: CreatureBank(20)));
                //Dragon Utopia                 +25 0 10000 100 
                // looks silly when you have many of these close together, so cap them 
                settings.Add(new("25 0", frequency: CreatureBank(110), maxOnMap: Max(), maxPerZone: 1));
                //Crypt                         +84 0 1000 100 
                settings.Add(new("84 0", frequency: CreatureBank(100)));
                //Shipwreck                     +85 0 2000 100 
                settings.Add(new("85 0", frequency: CreatureBank(100)));

                // creature banks that give creature rewards - make either more or less frequent 

                int CreatureBankCreature(int initial) => Weighted(1, Program.rand.Round(Math.Sqrt(100 * initial) * 2.1));
                //Griffin Conservatory          +16 2 2000 100 
                settings.Add(new("16 2", frequency: CreatureBankCreature(100)));
                //Dragon Fly Hive               +16 6 9000 100 
                settings.Add(new("16 6", frequency: CreatureBankCreature(100)));
                //Pirate Cavern                 +16 23 3500 100 
                // enable 
                settings.Add(new("16 23", frequency: CreatureBankCreature(90)));
                //Red Tower                     +16 26 4000 20 
                settings.Add(new("16 26", frequency: CreatureBankCreature(20)));
                //Ivory Tower                   +16 28 7000 100 
                // enable 
                settings.Add(new("16 28", frequency: CreatureBankCreature(90)));
                //Experimental Shop             +16 30 3500 80 
                settings.Add(new("16 30", frequency: CreatureBankCreature(80)));
                //Wolf Raider Picket            +16 31 9500 70 
                settings.Add(new("16 31", frequency: CreatureBankCreature(70)));

                return Output(settings);
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

            public static string Output(IEnumerable<ObjectSetting> settings)
            {
                if (!settings.Any())
                    return null;
                return settings.OrderBy(x => x).Select(s => s.Output()).Aggregate((a, b) => a + " " + b);
            }
            private string Output()
            {
                if (disable)
                    return "-" + id;
                static string Map(int? v) => v.HasValue ? v.ToString() : "d";
                return string.Format("+{0} {1} {2} {3} {4}", id, Map(value), Map(frequency), Map(maxOnMap), Map(maxPerZone));
            }

            public int CompareTo(ObjectSetting other)
            {
                static string[] Split(ObjectSetting setting)
                {
                    const char split = ' ';
                    string output = setting.Output();
                    output = output.Substring(1) + split + output[0].ToString();
                    return output.Split(split);
                }
                string[] a = Split(this);
                string[] b = Split(other);
                for (int c = 0; c < a.Length && c < b.Length; c++)
                {
                    string str1 = a[c];
                    string str2 = b[c];
                    bool parse1 = int.TryParse(str1, out int int1);
                    bool parse2 = int.TryParse(str2, out int int2);
                    int retVal = int1.CompareTo(int2);
                    if (parse1 && parse2 && retVal != 0)
                        return retVal;
                    retVal = str1.CompareTo(str2);
                    if (retVal != 0)
                        return retVal;
                }
                return 0;
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
