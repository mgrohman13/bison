using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    class Zone
    {
        private static int counter;
        private readonly bool player;

        public readonly int Id = -1;
        public readonly Type type;
        public readonly Restrictions restrictions;
        public readonly Player_towns player_Towns;
        public readonly Neutral_towns neutral_towns;
        public readonly Town_types town_Types;
        public Minimum_mines minimum_mines;
        public Mine_Density mine_Density;
        public readonly Terrain terrain;
        public readonly Monsters monsters;
        public readonly Treasure treasure;
        public readonly Options options;
        public Zone(Type.T t, Monsters.Str str, int value)
        {
            Id = ++counter;
            type = new(t);
            restrictions = new();
            player = t == Type.T.Human || t == Type.T.Computer;
            player_Towns = new(player);
            neutral_towns = new(player, type.size);
            bool town = player_Towns.Minimum_castles > 0 || player_Towns.Minimum_towns > 0 || neutral_towns.Minimum_castles > 0 || neutral_towns.Minimum_towns > 0;
            town_Types = new();
            terrain = new(town);
            monsters = new(str, town);
            treasure = new(value);
            options = new();

            //if (player)
            //{
            //    minimum_mines = new(player);
            //    mine_Density = new(player);
            //}
        }

        public void SetMines(int wo, int mscg, int g)
        {
            minimum_mines = new(player, wo, mscg, g);
            mine_Density = new(player, wo, mscg, g);

            double value = wo * 1500 + mscg * 3500 + g * 7000;

            if (value > 3333)
                monsters.SetMin(value / 7000);

            treasure.IncValue(value);
        }
        public class Type
        {
            private static readonly double sizeDev;
            private static int playerSize;
            static Type()
            {
                sizeDev = Program.rand.GaussianCapped(.26, .21);
            }

            public readonly string human_start;
            public readonly string computer_start;
            public readonly string Treasure;
            public readonly string Junction;
            private readonly int Base_Size = -1;
            public readonly double size;

            public Type(T type)
            {
                Base_Size = Program.rand.GaussianCappedInt(100, sizeDev, 1);
                switch (type)
                {
                    case T.Human:
                        if (playerSize == 0)
                            playerSize = Base_Size;
                        Base_Size = playerSize;
                        human_start = "x";
                        break;
                    case T.Computer:
                        computer_start = "x";
                        break;
                    case T.Treasure:
                        Treasure = "x";
                        break;
                    case T.Junction:
                        Junction = "x";
                        break;
                }
                size = Base_Size * Program.zoneSize / 100.0;
            }

            public enum T
            {
                Human,
                Computer,
                Treasure,
                Junction,
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "human start");
                Program.Output(output, x++, y, "computer start");
                Program.Output(output, x++, y, "Treasure");
                Program.Output(output, x++, y, "Junction");
                Program.Output(output, x++, y, "Base Size");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.type.human_start);
                    Program.Output(output, x++, y, zone.type.computer_start);
                    Program.Output(output, x++, y, zone.type.Treasure);
                    Program.Output(output, x++, y, zone.type.Junction);
                    Program.Output(output, x++, y, zone.type.Base_Size);
                }
            }
        }
        public class Restrictions
        {
            public readonly string Minimum_human_positions;
            public readonly string Maximum_human_positions;
            public readonly string Minimum_total_positions;
            public readonly string Maximum_total_positions;

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Minimum human positions");
                Program.Output(output, x++, y, "Maximum human positions");
                Program.Output(output, x++, y, "Minimum total positions");
                Program.Output(output, x++, y, "Maximum total positions");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.restrictions.Minimum_human_positions);
                    Program.Output(output, x++, y, zone.restrictions.Maximum_human_positions);
                    Program.Output(output, x++, y, zone.restrictions.Minimum_total_positions);
                    Program.Output(output, x++, y, zone.restrictions.Maximum_total_positions);
                }
            }
        }
        public class Player_towns
        {
            private static readonly bool castle;
            private static int counter;
            static Player_towns()
            {
                castle = Program.rand.Bool(.78);
            }

            public readonly int Ownership = -1;
            public readonly int Minimum_towns = -1;
            public readonly int Minimum_castles = -1;
            public readonly int Town_Density = -1;
            public readonly int Castle_Density = -1;
            public Player_towns(bool player)
            {
                if (player)
                {
                    Ownership = ++counter;
                    if (castle)
                        Minimum_castles = 1;
                    else
                        Minimum_towns = 1;
                }
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Ownership");
                Program.Output(output, x++, y, "Minimum towns");
                Program.Output(output, x++, y, "Minimum castles");
                Program.Output(output, x++, y, "Town Density");
                Program.Output(output, x++, y, "Castle Density");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.player_Towns.Ownership);
                    Program.Output(output, x++, y, zone.player_Towns.Minimum_towns);
                    Program.Output(output, x++, y, zone.player_Towns.Minimum_castles);
                    Program.Output(output, x++, y, zone.player_Towns.Town_Density);
                    Program.Output(output, x++, y, zone.player_Towns.Castle_Density);
                }
            }
        }
        public class Neutral_towns
        {
            private static double same;
            static Neutral_towns()
            {
                Init();
            }
            private static void Init()
            {
                static double p() => Program.rand.GaussianOE(1.04, .39, .13, .026);
                same = p();
                const double cap = .91;
                if (same > cap)
                    Init();
            }

            public readonly int Minimum_towns = -1;
            public readonly int Minimum_castles = -1;
            public readonly int Town_Density = -1;
            public readonly int Castle_Density = -1;
            public readonly string Towns_are_of_same_type;
            public Neutral_towns(bool player, double size)
            {
                if (!player)
                    Minimum_towns = Program.rand.GaussianOEInt(Math.Pow(size / Program.zoneSize, .78) * .65, .26, .13);
                int max = size > Program.zoneSize ? 2 : 1;
                if (Minimum_towns > max)
                    Minimum_towns = max;
                if (Program.rand.Bool(same))
                    Towns_are_of_same_type = "x";
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Minimum towns");
                Program.Output(output, x++, y, "Minimum castles");
                Program.Output(output, x++, y, "Town Density");
                Program.Output(output, x++, y, "Castle Density");
                Program.Output(output, x++, y, "Towns are of same type");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.neutral_towns.Minimum_towns);
                    Program.Output(output, x++, y, zone.neutral_towns.Minimum_castles);
                    Program.Output(output, x++, y, zone.neutral_towns.Town_Density);
                    Program.Output(output, x++, y, zone.neutral_towns.Castle_Density);
                    Program.Output(output, x++, y, zone.neutral_towns.Towns_are_of_same_type);
                }
            }
        }
        public class Town_types
        {
            public readonly string Castle;
            public readonly string Rampart;
            public readonly string Tower;
            public readonly string Inferno;
            public readonly string Necropolis;
            public readonly string Dungeon;
            public readonly string Stronghold;
            public readonly string Fortress;
            public readonly string Conflux;
            public readonly string Cove;
            public Town_types()
            {
                Castle = Rampart = Tower = Inferno = Necropolis = Dungeon = Stronghold = Fortress = Conflux = Cove = "x";
            }
            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Castle");
                Program.Output(output, x++, y, "Rampart");
                Program.Output(output, x++, y, "Tower");
                Program.Output(output, x++, y, "Inferno");
                Program.Output(output, x++, y, "Necropolis");
                Program.Output(output, x++, y, "Dungeon");
                Program.Output(output, x++, y, "Stronghold");
                Program.Output(output, x++, y, "Fortress");
                Program.Output(output, x++, y, "Conflux");
                Program.Output(output, x++, y, "Cove");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.town_Types.Castle);
                    Program.Output(output, x++, y, zone.town_Types.Rampart);
                    Program.Output(output, x++, y, zone.town_Types.Tower);
                    Program.Output(output, x++, y, zone.town_Types.Inferno);
                    Program.Output(output, x++, y, zone.town_Types.Necropolis);
                    Program.Output(output, x++, y, zone.town_Types.Dungeon);
                    Program.Output(output, x++, y, zone.town_Types.Stronghold);
                    Program.Output(output, x++, y, zone.town_Types.Fortress);
                    Program.Output(output, x++, y, zone.town_Types.Conflux);
                    Program.Output(output, x++, y, zone.town_Types.Cove);
                }
            }
        }
        public class Minimum_mines
        {
            public readonly int Wood = -1;
            public readonly int Mercury = -1;
            public readonly int Ore = -1;
            public readonly int Sulfur = -1;
            public readonly int Crystal = -1;
            public readonly int Gems = -1;
            public readonly int Gold = -1;
            public Minimum_mines(bool player)
            {
                if (player)
                    Wood = Ore = 1;
            }
            public Minimum_mines(bool player, int wo, int mscg, int g)
            {
                Wood = Mercury = Ore = Sulfur = Crystal = Gems = Gold = 0;
                if (player)
                    Wood = Ore = 1;
                for (int a = 0; a < wo; a++)
                    if (Program.rand.Bool())
                        Wood++;
                    else
                        Ore++;
                for (int b = 0; b < mscg; b++)
                    switch (Program.rand.Next(4))
                    {
                        case 0:
                            Mercury++;
                            break;
                        case 1:
                            Sulfur++;
                            break;
                        case 2:
                            Crystal++;
                            break;
                        case 3:
                            Gems++;
                            break;
                    }
                Gold = g;
            }
            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Wood");
                Program.Output(output, x++, y, "Mercury");
                Program.Output(output, x++, y, "Ore");
                Program.Output(output, x++, y, "Sulfur");
                Program.Output(output, x++, y, "Crystal");
                Program.Output(output, x++, y, "Gems");
                Program.Output(output, x++, y, "Gold");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.minimum_mines.Wood);
                    Program.Output(output, x++, y, zone.minimum_mines.Mercury);
                    Program.Output(output, x++, y, zone.minimum_mines.Ore);
                    Program.Output(output, x++, y, zone.minimum_mines.Sulfur);
                    Program.Output(output, x++, y, zone.minimum_mines.Crystal);
                    Program.Output(output, x++, y, zone.minimum_mines.Gems);
                    Program.Output(output, x++, y, zone.minimum_mines.Gold);
                }
            }
        }
        public class Mine_Density
        {
            public readonly int Wood = -1;
            public readonly int Mercury = -1;
            public readonly int Ore = -1;
            public readonly int Sulfur = -1;
            public readonly int Crystal = -1;
            public readonly int Gems = -1;
            public readonly int Gold = -1;
            public Mine_Density(bool player)
            {
            }
            public Mine_Density(bool player, int wo, int mscg, int g)
            {
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Wood");
                Program.Output(output, x++, y, "Mercury");
                Program.Output(output, x++, y, "Ore");
                Program.Output(output, x++, y, "Sulfur");
                Program.Output(output, x++, y, "Crystal");
                Program.Output(output, x++, y, "Gems");
                Program.Output(output, x++, y, "Gold");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.mine_Density.Wood);
                    Program.Output(output, x++, y, zone.mine_Density.Mercury);
                    Program.Output(output, x++, y, zone.mine_Density.Ore);
                    Program.Output(output, x++, y, zone.mine_Density.Sulfur);
                    Program.Output(output, x++, y, zone.mine_Density.Crystal);
                    Program.Output(output, x++, y, zone.mine_Density.Gems);
                    Program.Output(output, x++, y, zone.mine_Density.Gold);
                }
            }
        }
        public class Terrain
        {
            public readonly string Match_to_town;
            public readonly string Dirt;
            public readonly string Sand;
            public readonly string Grass;
            public readonly string Snow;
            public readonly string Swamp;
            public readonly string Rough;
            public readonly string Cave;
            public readonly string Lava;
            public readonly string Highlands;
            public readonly string Wasteland;
            public Terrain(bool town)
            {
                if (town)
                    Match_to_town = "x";
                Dirt = Sand = Grass = Snow = Swamp = Rough = Cave = Lava = Highlands = Wasteland = "x";
            }
            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Match to town");
                Program.Output(output, x++, y, "Dirt");
                Program.Output(output, x++, y, "Sand");
                Program.Output(output, x++, y, "Grass");
                Program.Output(output, x++, y, "Snow");
                Program.Output(output, x++, y, "Swamp");
                Program.Output(output, x++, y, "Rough");
                Program.Output(output, x++, y, "Cave");
                Program.Output(output, x++, y, "Lava");
                Program.Output(output, x++, y, "Highlands");
                Program.Output(output, x++, y, "Wasteland");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.terrain.Match_to_town);
                    Program.Output(output, x++, y, zone.terrain.Dirt);
                    Program.Output(output, x++, y, zone.terrain.Sand);
                    Program.Output(output, x++, y, zone.terrain.Grass);
                    Program.Output(output, x++, y, zone.terrain.Snow);
                    Program.Output(output, x++, y, zone.terrain.Swamp);
                    Program.Output(output, x++, y, zone.terrain.Rough);
                    Program.Output(output, x++, y, zone.terrain.Cave);
                    Program.Output(output, x++, y, zone.terrain.Lava);
                    Program.Output(output, x++, y, zone.terrain.Highlands);
                    Program.Output(output, x++, y, zone.terrain.Wasteland);
                }
            }
        }
        public class Monsters
        {
            private static double match;
            private static double allOne;
            private static double neutral;
            private static double prob;
            static Monsters()
            {
                Init();
            }
            private static void Init()
            {
                match = Program.rand.GaussianOE(.169, .39, .13);
                allOne = Program.rand.GaussianOE(.065, .39, .13);
                static double p() => Program.rand.GaussianOE(.104, .39, .13, .026);
                neutral = p();
                prob = p();
                const double cap = .91;
                if (allOne > cap || neutral > cap || prob > cap)
                    Init();
            }

            private string Strength;
            public readonly string Match_to_town;
            public readonly string Neutral;
            private string Castle;
            private string Rampart;
            private string Tower;
            private string Inferno;
            private string Necropolis;
            private string Dungeon;
            private string Stronghold;
            private string Fortress;
            private string Conflux;
            private string Cove;
            public Monsters(Monsters.Str str, bool town)
            {
                Strength = str.ToString();

                bool n = Program.rand.Bool(neutral);
                if (town && Program.rand.Bool(match))
                    Match_to_town = "x";
                else if (Program.rand.Bool(allOne))
                {
                    if (n)
                        Neutral = "x";
                    else
                        Switch(Program.rand.Next(10));
                }
                else
                {
                    bool any = false;
                    for (int a = 0; a < 10; a++)
                        if (Program.rand.Bool(prob))
                        {
                            any = true;
                            Switch(a);
                        }
                    if (n || !any)
                        Neutral = "x";
                }
            }
            private void Switch(int a)
            {
                switch (Program.rand.Next(10))
                {
                    case 0:
                        Castle = "x";
                        break;
                    case 1:
                        Rampart = "x";
                        break;
                    case 2:
                        Tower = "x";
                        break;
                    case 3:
                        Inferno = "x";
                        break;
                    case 4:
                        Necropolis = "x";
                        break;
                    case 5:
                        Dungeon = "x";
                        break;
                    case 6:
                        Stronghold = "x";
                        break;
                    case 7:
                        Fortress = "x";
                        break;
                    case 8:
                        Conflux = "x";
                        break;
                    case 9:
                        Cove = "x";
                        break;
                }
            }

            internal void SetMin(double high)
            {
                if (Strength != Str.strong.ToString() && Program.rand.Gaussian(high, .26) > 1)
                    Strength = Str.strong.ToString();
                else if (Strength == Str.none.ToString())
                    Strength = Str.weak.ToString();
            }

            public enum Str
            {
                strong,
                none,
                weak,
                avg,
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Strength");
                Program.Output(output, x++, y, "Match to town");
                Program.Output(output, x++, y, "Neutral");
                Program.Output(output, x++, y, "Castle");
                Program.Output(output, x++, y, "Rampart");
                Program.Output(output, x++, y, "Tower");
                Program.Output(output, x++, y, "Inferno");
                Program.Output(output, x++, y, "Necropolis");
                Program.Output(output, x++, y, "Dungeon");
                Program.Output(output, x++, y, "Stronghold");
                Program.Output(output, x++, y, "Fortress");
                Program.Output(output, x++, y, "Conflux");
                Program.Output(output, x++, y, "Cove");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.monsters.Strength);
                    Program.Output(output, x++, y, zone.monsters.Match_to_town);
                    Program.Output(output, x++, y, zone.monsters.Neutral);
                    Program.Output(output, x++, y, zone.monsters.Castle);
                    Program.Output(output, x++, y, zone.monsters.Rampart);
                    Program.Output(output, x++, y, zone.monsters.Tower);
                    Program.Output(output, x++, y, zone.monsters.Inferno);
                    Program.Output(output, x++, y, zone.monsters.Necropolis);
                    Program.Output(output, x++, y, zone.monsters.Dungeon);
                    Program.Output(output, x++, y, zone.monsters.Stronghold);
                    Program.Output(output, x++, y, zone.monsters.Fortress);
                    Program.Output(output, x++, y, zone.monsters.Conflux);
                    Program.Output(output, x++, y, zone.monsters.Cove);
                }
            }
        }
        public class Treasure
        {
            private int Low = -1;
            private int High = -1;
            private int Density = -1;
            //Low
            private int Low_2 = -1;
            //High
            private int High_2 = -1;
            //Density
            private int Density_2 = -1;
            //Low
            private int Low_3 = -1;
            //High
            private int High_3 = -1;
            //Density
            private int Density_3 = -1;

            // 55 — poor zone, white. Content types:      (  500– 3000, 9), ( 3000– 6000, 6), (10000–15000, 1).
            //133 — rich zone, silver. Content types:     ( 3000– 6000, 9), (10000–15000, 6), (15000–20000, 1).
            //242 — vastly rich zone, gold.Content types: (10000–15000, 9), (15000–20000, 6), (20000–30000, 1).

            public Treasure(double value)
            {
                double count = 3;
                foreach (int a in Enumerable.Repeat(2, 1).Concat(Program.rand.Iterate(2)))
                {
                    double pct, l, h;
                    switch (a)
                    {
                        case 0:
                            pct = 351.1655590906;
                            l = 4500;
                            h = 8000;
                            break;
                        case 1:
                            pct = 495.1957493211;
                            l = 9333.3333333333;
                            h = 13666.6666666667;
                            break;
                        case 2:
                            pct = 153.6386915883;
                            l = 15000;
                            h = 21666.6666666667;
                            break;
                        default: throw new Exception();
                    }
                    int low = Program.rand.GaussianCappedInt(l, .13, 1);
                    int high = Program.rand.GaussianCappedInt(Math.Max(h, low), .13, low);
                    double v = value * pct / 1000.0 * 3 / count;
                    v = 2 * v / (double)(low + high);
                    int density = Program.rand.GaussianCappedInt(v, .091);
                    value -= (low + high) / 2.0 * density;
                    count--;
                    SetVals(a, low, high, density);
                    if (value <= 0)
                        break;
                }
            }

            public void IncValue(double value)
            {
                if (value > 0)
                {
                    int sel = Program.rand.Next(3);
                    GetVals(sel, out int low, out int high, out int density);
                    value += (low + high) / 2.0 * density;
                    low = Program.rand.GaussianCappedInt(1500, .13);
                    high = Program.rand.GaussianCappedInt(Math.Max(7000, low), .13, low);
                    density = Program.rand.GaussianCappedInt(2.0 * value / (low + high), .052);
                    SetVals(sel, low, high, density);
                }
            }
            private void GetVals(int a, out int low, out int high, out int density)
            {
                switch (a)
                {
                    case 0:
                        low = Low;
                        high = High;
                        density = Density;
                        break;
                    case 1:
                        low = Low_2;
                        high = High_2;
                        density = Density_2;
                        break;
                    case 2:
                        low = Low_3;
                        high = High_3;
                        density = Density_3;
                        break;
                    default: throw new Exception();
                }
            }
            private void SetVals(int a, int low, int high, int density)
            {
                switch (a)
                {
                    case 0:
                        Low = low;
                        High = high;
                        Density = density;
                        break;
                    case 1:
                        Low_2 = low;
                        High_2 = high;
                        Density_2 = density;
                        break;
                    case 2:
                        Low_3 = low;
                        High_3 = high;
                        Density_3 = density;
                        break;
                }
            }

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Low");
                Program.Output(output, x++, y, "High");
                Program.Output(output, x++, y, "Density");
                Program.Output(output, x++, y, "Low");
                Program.Output(output, x++, y, "High");
                Program.Output(output, x++, y, "Density");
                Program.Output(output, x++, y, "Low");
                Program.Output(output, x++, y, "High");
                Program.Output(output, x++, y, "Density");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.treasure.Low);
                    Program.Output(output, x++, y, zone.treasure.High);
                    Program.Output(output, x++, y, zone.treasure.Density);
                    Program.Output(output, x++, y, zone.treasure.Low_2);
                    Program.Output(output, x++, y, zone.treasure.High_2);
                    Program.Output(output, x++, y, zone.treasure.Density_2);
                    Program.Output(output, x++, y, zone.treasure.Low_3);
                    Program.Output(output, x++, y, zone.treasure.High_3);
                    Program.Output(output, x++, y, zone.treasure.Density_3);
                }
            }
        }
        public class Options
        {
            public readonly string Placement;
            public readonly string Objects;
            public readonly string Minimum_objects;
            public readonly string Image_settings;
            public readonly string Force_neutral_creatures;
            // Allow non-coherent road
            public readonly string Allow_non_coherent_road;
            public readonly string Zone_repulsion;
            public readonly string Town_Hint;
            //  Monsters disposition(standard)
            public readonly string Monsters_disposition_standard;
            //  Monsters disposition(custom)
            public readonly string Monsters_disposition_custom;
            public readonly string Monsters_joining_percentage;
            public readonly string Monsters_join_only_for_money;

            public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
            {
                int sx = x;
                Program.Output(output, x++, y, "Placement");
                Program.Output(output, x++, y, "Objects");
                Program.Output(output, x++, y, "Minimum objects");
                Program.Output(output, x++, y, "Image settings");
                Program.Output(output, x++, y, "Force neutral creatures");
                Program.Output(output, x++, y, "Allow non-coherent road");
                Program.Output(output, x++, y, "Zone repulsion");
                Program.Output(output, x++, y, "Town Hint");
                Program.Output(output, x++, y, "Monsters disposition(standard)");
                Program.Output(output, x++, y, "Monsters disposition(custom)");
                Program.Output(output, x++, y, "Monsters joining percentage");
                Program.Output(output, x++, y, "Monsters join only for money");
                foreach (Zone zone in zones)
                {
                    x = sx;
                    y++;
                    Program.Output(output, x++, y, zone.options.Placement);
                    Program.Output(output, x++, y, zone.options.Objects);
                    Program.Output(output, x++, y, zone.options.Minimum_objects);
                    Program.Output(output, x++, y, zone.options.Image_settings);
                    Program.Output(output, x++, y, zone.options.Force_neutral_creatures);
                    Program.Output(output, x++, y, zone.options.Allow_non_coherent_road);
                    Program.Output(output, x++, y, zone.options.Zone_repulsion);
                    Program.Output(output, x++, y, zone.options.Town_Hint);
                    Program.Output(output, x++, y, zone.options.Monsters_disposition_standard);
                    Program.Output(output, x++, y, zone.options.Monsters_disposition_custom);
                    Program.Output(output, x++, y, zone.options.Monsters_joining_percentage);
                    Program.Output(output, x++, y, zone.options.Monsters_join_only_for_money);
                }
            }
        }

        public static void Output(List<List<string>> output, ref int x, ref int y, Zone[] zones)
        {
            int sx = x, sy = y;

            y++;
            Program.Output(output, x, y++, "Id");
            foreach (Zone zone in zones)
                Program.Output(output, x, y++, zone.Id);

            x = sx + 1;
            y = sy;
            Program.Output(output, x, y++, "Type");
            Type.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Restrictions");
            Restrictions.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Player towns");
            Player_towns.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Neutral towns");
            Neutral_towns.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Town types");
            Town_types.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Minimum mines");
            Minimum_mines.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Mine Density");
            Mine_Density.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Terrain");
            Terrain.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Monsters");
            Monsters.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Treasure");
            Treasure.Output(output, ref x, ref y, zones);

            y = sy;
            Program.Output(output, x, y++, "Options");
            Options.Output(output, ref x, ref y, zones);
        }
    }
}
