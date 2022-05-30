﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HOMM3
{
    public class Zone
    {
        private static int counter;

        public readonly Player player;
        public readonly bool start, extra;
        private readonly Dictionary<Zone, List<Connections>> connections;
        private int woodOre, resource, gold;
        private int value;
        private Monsters.Str monsterStr;
        private double disposition;
        private bool moneyOnly;
        private double joinPct;
        public int WoodOreMines => woodOre;
        public int ResourceMines => resource;
        public int GoldMines => gold;
        public double NumTowns => (start ? 1 : 0) + neutral_towns.NumTowns();

        //output
        public readonly int Id = -1;
        private readonly Type type;
        private readonly Restrictions restrictions;
        private readonly Player_towns player_Towns;
        private readonly Neutral_towns neutral_towns;
        private readonly Town_types town_Types;
        private readonly Minimum_mines minimum_mines;
        private readonly Mine_Density mine_Density;
        private readonly Terrain terrain;
        private readonly Monsters monsters;
        private readonly Treasure treasure;
        private readonly Options options;

        public Zone(Player player, bool start, bool extra, double avgSize)
        {
            this.player = player;
            this.start = start;
            this.extra = extra;
            this.connections = new();

            Id = ++counter;

            //place towns initially so we can get an accurate number for mine generation
            type = new(start, player);
            neutral_towns = new(start, type.Size(avgSize), avgSize);

            restrictions = new();
            player_Towns = new();
            town_Types = new();
            minimum_mines = new();
            mine_Density = new();
            terrain = new();
            monsters = new();
            treasure = new();
            options = new();
        }
        public static void AddConnection(Zone z1, Zone z2, Connections connection)
        {
            z1.AddConnection(z2, connection);
            z2.AddConnection(z1, connection);
        }
        private void AddConnection(Zone z1, Connections connection)
        {
            connections.TryGetValue(z1, out List<Connections> list);
            if (list == null)
                connections.Add(z1, list = new());
            list.Add(connection);
        }

        public static List<Zone> InitZones(Player[] players, double size)
        {
            List<Zone> zones = new();
            // include count of doubled-up AIs
            int playerCount = players.Length;
            double avgNumZones = size / (.39 * Math.Pow(size + 6.5, .39));
            int numZones = Program.rand.GaussianOEInt(avgNumZones + playerCount, .21, .13, playerCount);
            double avgSize = 1296 * size / (double)numZones;
            while (numZones > 0)
            {
                bool extra = numZones < playerCount;
                foreach (int p in Program.rand.Iterate(playerCount))
                    if (!extra || !players[p].AIstrong || Program.rand.Bool(.13))
                    {
                        Zone zone = new(players[p], !players[p].Zones.Any(), extra, avgSize);
                        zones.Add(zone);
                        players[p].AddZone(zone);
                        if (--numZones == 0)
                            break;
                    }
            }

            int min = players.Min(p => p.Zones.Count);
            int max = players.Max(p => p.Zones.Count);
            for (int a = 0; a < max; a++)
            {
                //minimum value should be high enough that it's likely to be somewhat greater than 24960, even with a number of mines
                int minValue = 39000;
                int value;
                Zone.Monsters.Str monsters;
                double disposition;
                bool moneyOnly;
                double joinPct = 1.5;

                if (a == 0)
                {
                    //home zones
                    value = Program.rand.GaussianOEInt(91000, .169, .065, minValue);
                    monsters = Program.rand.Bool(.65) ? Zone.Monsters.Str.avg : Zone.Monsters.Str.weak;
                    disposition = Program.rand.GaussianCapped(5.2, .169, .91);
                    moneyOnly = Program.rand.Bool(.21);
                    joinPct = Program.rand.GaussianCapped(joinPct, .21);
                }
                else if (a >= min)
                {
                    //extra zones
                    value = Program.rand.GaussianOEInt(78000, .21, .078, minValue);
                    monsters = Program.rand.Bool(.39) ? Zone.Monsters.Str.avg : Monsters.Str.strong;
                    disposition = Program.rand.Range(6.5, 7.8);
                    moneyOnly = Program.rand.Bool();
                    joinPct = moneyOnly ? Program.rand.GaussianCapped(joinPct, .39) : 0;
                }
                else
                {
                    //standard zones
                    value = Program.rand.GaussianOEInt(104000, .26, .091, minValue);
                    monsters = (Zone.Monsters.Str)Program.rand.Next(4);
                    disposition = .91 + Program.rand.Weighted(6.5, .78);
                    moneyOnly = Program.rand.Bool(.26);
                    joinPct = Program.rand.GaussianCapped(joinPct, .26);
                }

                if (monsters == Zone.Monsters.Str.none)
                    if (Program.rand.Bool(.91))
                        monsters = Program.rand.Bool() ? Zone.Monsters.Str.weak : Zone.Monsters.Str.avg;
                    else
                        value = Program.rand.Round(value / 2.1);

                foreach (Player player in Program.rand.Iterate(players))
                    if (a < player.Zones.Count)
                        player.Zones[a].SetValues(value, monsters, disposition, moneyOnly, joinPct);
            }

            return zones;
        }

        private void SetValues(int value, Monsters.Str monsterStr, double disposition, bool moneyOnly, double joinPct)
        {
            this.value = value;
            this.monsterStr = monsterStr;
            this.disposition = disposition;
            this.moneyOnly = moneyOnly;
            this.joinPct = joinPct;
        }

        public void AddMines(int woodOre, int resource, int gold)
        {
            Mines(woodOre, resource, gold, 1);
        }
        internal void RemoveMines(int woodOre, int resource, int gold)
        {
            Mines(woodOre, resource, gold, -1);
        }
        private void Mines(int woodOre, int resource, int gold, int mult)
        {
            if (woodOre < 0 || resource < 0 || gold < 0)
                throw new Exception();
            this.woodOre += woodOre * mult;
            this.resource += resource * mult;
            this.gold += gold * mult;
            if (this.woodOre < 0 || this.resource < 0 || this.gold < 0)
                throw new Exception();
        }

        internal void Generate(List<Connections> allConnections)
        {
            double mineValue = woodOre * 1500 + resource * 3500 + gold * 7000;

            bool town = player_Towns.Generate(start, player);
            town |= neutral_towns.Generate();

            type.Generate(start, town, mineValue);
            minimum_mines.Generate(woodOre, resource, gold);
            mine_Density.Generate();
            terrain.Generate(town);
            monsters.Generate(monsterStr, town, mineValue, resource > 0, gold > 0);
            bool neutralDwellings = options.Generate(start, town, disposition, joinPct, moneyOnly, mineValue);
            treasure.Generate(start, player, value, mineValue, monsterStr, neutralDwellings);

            restrictions.Generate();
            town_Types.Generate();

            foreach (var pair in Program.rand.Iterate(connections))
                if (pair.Value.Any())
                {
                    Connections.Generate(allConnections, pair.Value);
                    pair.Key.connections[this].Clear();
                }
        }

        public class Type
        {
            private static readonly double sizeDev;
            private static readonly double junction;
            private static int playerSize = -1;
            static Type()
            {
                sizeDev = Program.rand.GaussianCapped(.26, .21);
                //junction zones are a little wack, so make them fairly unlikely 
                junction = Program.rand.Weighted(.39, .104);
            }

            private readonly string human_start;
            private readonly string computer_start;
            private string Treasure;
            private string Junction;
            private readonly int Base_Size = -1;

            public Type(bool start, Player player)
            {
                Base_Size = Program.rand.GaussianCappedInt(100, sizeDev, 1);

                if (start)
                {
                    if (playerSize == -1)
                        playerSize = Base_Size;
                    Base_Size = Program.rand.GaussianCappedInt(playerSize, .026, 1);

                    if (player.Human)
                        human_start = "x";
                    else
                        computer_start = "x";
                }
            }
            public double Size(double avgSize)
            {
                double size = Base_Size * avgSize / 100.0;
                //if (Junction == "x")
                //    size /= 2.1;
                return size;
            }
            public void Generate(bool start, bool town, double mineValue)
            {
                if (!start)
                {
                    if (Program.rand.Bool(junction * (town ? .52 : 1) / (1 + mineValue / 7800.0)))
                        Junction = "x";
                    else
                        Treasure = "x";
                }
                //return Size(avgSize);
            }

            //public enum T
            //{
            //    Human,
            //    Computer,
            //    Treasure,
            //    Junction,
            //}

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
            private int Minimum_human_positions = -1;
            private int Maximum_human_positions = -1;
            private int Minimum_total_positions = -1;
            private int Maximum_total_positions = -1;

            public void Generate()
            {
                Minimum_human_positions = Maximum_human_positions = 1;
                Minimum_total_positions = Maximum_total_positions = Program.NumPlayers;
            }

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
            //private static readonly bool castle;
            //static Player_towns()
            //{
            //    castle = Program.rand.Bool(.78);
            //}

            private int Ownership = -1;
            private int Minimum_towns = -1;
            private int Minimum_castles = -1;
            private readonly int Town_Density = -1;
            private readonly int Castle_Density = -1;

            public bool Generate(bool start, Player player)
            {
                if (start)
                {
                    Ownership = player.ID;
                    if (player.Human)
                        Minimum_towns = 1;
                    else
                        Minimum_castles = 1;

                    return true;
                }
                return false;
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

            private readonly int Minimum_towns = 0;
            private readonly int Minimum_castles = 0;
            private readonly int Town_Density = -1;
            private readonly int Castle_Density = -1;
            private string Towns_are_of_same_type;
            public Neutral_towns(bool start, double size, double avgSize)
            {
                if (!start)
                    Minimum_towns = Program.rand.GaussianOEInt(Math.Pow(size * size / avgSize / 1300, .65) * .65, .26, .13);
                int max = size > Program.rand.Gaussian(1300, .13) ? 2 : 1;
                if (size < 390)
                    max = 0;
                if (Minimum_towns > max)
                    Minimum_towns = max;
                if (Minimum_towns > 0 && Program.rand.Bool(.104))
                {
                    Minimum_towns--;
                    Minimum_castles = 1;
                }
            }
            public double NumTowns()
            {
                return Minimum_towns + Minimum_castles * 1.3;
            }
            public bool Generate()
            {
                if (Program.rand.Bool(same))
                    Towns_are_of_same_type = "x";
                return NumTowns() > 0;
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
            public void Generate()
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
            private static readonly List<int> choices;
            private static bool woodOre;
            static Minimum_mines()
            {
                choices = new();
                woodOre = Program.rand.Bool();
            }

            private int Wood = -1;
            private int Mercury = -1;
            private int Ore = -1;
            private int Sulfur = -1;
            private int Crystal = -1;
            private int Gems = -1;
            private int Gold = -1;
            public void Generate(int woodOre, int resource, int gold)
            {
                Wood = Mercury = Ore = Sulfur = Crystal = Gems = Gold = 0;
                for (int a = 0; a < woodOre; a++)
                {
                    if (Minimum_mines.woodOre)
                        Wood++;
                    else
                        Ore++;
                    Minimum_mines.woodOre = !Minimum_mines.woodOre;
                }
                int previous = -1;
                for (int b = 0; b < resource; b++)
                {
                    if (!choices.Any())
                        do
                            choices.AddRange(Enumerable.Range(0, 4));
                        while (Program.rand.Bool());
                    int choice;
                    do
                        choice = Program.rand.SelectValue(choices);
                    while (previous == choice && Program.rand.Bool());
                    previous = choice;
                    choices.Remove(choice);
                    switch (choice)
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
                        default: throw new Exception();
                    }
                }
                Gold = gold;
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
            private readonly int Wood = -1;
            private readonly int Mercury = -1;
            private readonly int Ore = -1;
            private readonly int Sulfur = -1;
            private readonly int Crystal = -1;
            private readonly int Gems = -1;
            private readonly int Gold = -1;
            public void Generate()
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
            private string Match_to_town;
            private string Dirt;
            private string Sand;
            private string Grass;
            private string Snow;
            private string Swamp;
            private string Rough;
            private string Cave;
            private string Lava;
            private string Highlands;
            private string Wasteland;

            public void Generate(bool town)
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
                match = Program.rand.GaussianOE(.078, .39, .13);
                allOne = Program.rand.GaussianOE(.065, .39, .13);
                static double p() => Program.rand.GaussianOE(.169, .39, .13, .052);
                neutral = p();
                prob = p();
                const double cap = .91;
                if (allOne > cap || neutral > cap || prob > cap)
                    Init();
            }

            private string Strength;
            private string Match_to_town;
            private string Neutral;
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

            public void Generate(Monsters.Str str, bool town, double mineValue, bool hasResource, bool hasGold)
            {
                if (mineValue > 3000)
                {
                    //chance to increase strength based on number/value of mines
                    int newStr = (int)str + Program.rand.GaussianCappedInt((mineValue - 2500) / 6500.0, .26);
                    if (newStr > (int)Monsters.Str.strong)
                        newStr = (int)Monsters.Str.strong;
                    str = (Monsters.Str)newStr;
                }

                bool canMatchTown = true;
                bool any = false;
                if (hasResource)
                {
                    // if there is a resource mine, strength must be at least avg or mine is ungarded
                    if (str <= Str.avg)
                    {
                        str = Str.avg;

                        // Neutral, Castle, and Rampart level 1 creatures are too strong to be generated to guard resource mines with avg strength, so force some other type
                        canMatchTown = false;
                        any = true;
                        // force a working type
                        Switch(Program.rand.RangeInt(2, 9));
                        // chance to add another type to keep balanced
                        if (Program.rand.Next(4) == 0)
                            Switch(Program.rand.RangeInt(0, 1));
                    }
                }
                else if (hasGold)
                {
                    // if there is a gold mine, force at least weak monsters
                    if (str < Str.weak)
                        str = Str.weak;
                }

                Strength = str.ToString();

                bool n = Program.rand.Bool(neutral);
                if (town && canMatchTown && Program.rand.Bool(match))
                {
                    Match_to_town = "x";
                }
                else if (Program.rand.Bool(allOne))
                {
                    if (!any)
                        if (n)
                            Neutral = "x";
                        else
                            Switch(Program.rand.Next(10));
                }
                else
                {
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
                switch (a)
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
                    default: throw new Exception();
                }
            }

            public enum Str
            {
                none = 0,
                weak = 1,
                avg = 2,
                strong = 3,
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

            internal void Generate(bool start, Player player, double value, double mineValue, Monsters.Str monsterStr, bool neutralDwellings)
            {
                value -= mineValue;

                // 55 — poor zone, white.       Content types: (  500– 3000, 9), ( 3000– 6000, 6), (10000–15000, 1).
                //133 — rich zone, silver.      Content types: ( 3000– 6000, 9), (10000–15000, 6), (15000–20000, 1).
                //242 — vastly rich zone, gold. Content types: (10000–15000, 9), (15000–20000, 6), (20000–30000, 1).

                double min1 = Program.rand.Range(101, 520);
                double min2 = Program.rand.Range(390, 3900);
                double nMin = Program.rand.Range(Program.rand.Bool() ? min1 : (Program.rand.Bool() ? 30001 : 39338), Program.rand.Range(43905, 49795));
                double[,,] ranges = new double[5, 3, 2] {
                    //for starting zones, we can guarantee unguarded resources (items ranging 750-2000)
                    { {  min1,  3900 }, {  2001,  2999 }, {   101,  6500 } },
                    //main tiers
                    { {  min1,  6500 }, {  min2,  9100 }, {  5200, 13000 } },
                    { { 6500,  10400 }, {  9100, 14300 }, { 13000, 16900 } },
                    { { 10400, 15600 }, { 14300, 21000 }, { 16900, 30001 } }, 
                    //if forcing neutral dwellings, there is a chance to use an alternate 3rd tier that allows for the neutral dragon dwellings
                    // these values are 1-sigma confidence level based on: Crystal Cavern = 39338, Frozen Cliffs = 78845
                    { {  nMin, 49795 }, { 43905, 99804 }, { 87997, 99804 } },
                };

                bool neutralDragons = !start && neutralDwellings && monsterStr != Monsters.Str.none && Program.rand.Bool();
                int[] order;
                if (neutralDragons)
                    order = new int[3] { 4, 2, 1 };
                else if (start && player.AIprimary)
                    // give the AI primary starting location unguarded resources
                    order = new int[3] { 0, 3, 2 };
                else
                    order = new int[3] { 3, 2, 1 }; ;

                int div = 3;
                foreach (int tier in order)
                {
                    double GetDensity(double l, double h) => value / (double)div * 2 / (l + h);

                    double lowAvg, highAvg;
                    if (start && player.Human && tier == 1)
                    {
                        //for the human start, make unguarded resources possible but infrequent
                        lowAvg = 2600;
                        highAvg = 9100;
                    }
                    else
                    {
                        int selection = Program.rand.Next(3);
                        do
                        {
                            lowAvg = ranges[tier, selection, 0];
                            highAvg = ranges[tier, selection, 1];
                            selection--;
                        }
                        while (!neutralDragons && selection >= 0 && GetDensity(lowAvg, highAvg) < 1);
                    }

                    int low = Program.rand.GaussianCappedInt(lowAvg, .104, 101);
                    int high = Program.rand.GaussianCappedInt(Math.Max(highAvg, low), .21, low);

                    int density;
                    if (neutralDragons)
                    {
                        value *= 2 / 3.0;
                        density = 1;// + Program.rand.Round(value / (double)(low + high));
                    }
                    else
                    {
                        double densityAvg = GetDensity(low, high);
                        density = Program.rand.GaussianCappedInt(Math.Max(densityAvg, 1), .091, 1);
                        value -= (low + high) / 2.0 * density;
                    }

                    SetVals(tier, low, high, density);

                    div--;
                    neutralDragons = false;
                }
            }
            private void SetVals(int a, int low, int high, int density)
            {
                switch (a)
                {
                    case 2:
                        Low = low;
                        High = high;
                        Density = density;
                        break;
                    case 0:
                    case 1:
                        Low_2 = low;
                        High_2 = high;
                        Density_2 = density;
                        break;
                    case 3:
                    case 4:
                        Low_3 = low;
                        High_3 = high;
                        Density_3 = density;
                        break;
                    default: throw new Exception();
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
            private static double neutral;
            private static double road;
            private static double roadTown;
            static Options()
            {
                Init();
            }
            private static void Init()
            {
                neutral = Program.rand.GaussianOE(.39, .26, .13);
                road = Program.rand.GaussianOE(.21, .39, .26);
                roadTown = Program.rand.Range(0, 1);
                const double cap = .91;
                if (neutral > cap || road > cap)
                    Init();
            }

            private readonly string Placement;
            private readonly string Objects;
            private readonly string Minimum_objects;
            private readonly string Image_settings;
            private string Force_neutral_creatures;
            // Allow non-coherent road
            private string Allow_non_coherent_road;
            private readonly string Zone_repulsion;
            private readonly string Town_Hint;
            //  Monsters disposition(standard)
            private int Monsters_disposition_standard = -1;
            //  Monsters disposition(custom)
            private int Monsters_disposition_custom = -1;
            private int Monsters_joining_percentage = -1;
            private string Monsters_join_only_for_money;

            public bool Generate(bool start, bool town, double disposition, double joinPct, bool moneyOnly, double mineValue)
            {
                mineValue /= 3900.0;
                disposition = Math.Min(disposition + mineValue, 10 - (10 - disposition) * (10 - mineValue) / 10.0);

                if (!start && Program.rand.Bool(neutral * (town ? .21 : 1)))
                    Force_neutral_creatures = "x";
                if (Program.rand.Bool(road * (town ? roadTown : 1)))
                    Allow_non_coherent_road = "x";

                if (SetDisposition(disposition) == 0)
                    moneyOnly = true;
                Monsters_joining_percentage = Program.rand.Round(joinPct);
                if (moneyOnly)
                    Monsters_join_only_for_money = "x";

                return Force_neutral_creatures != null;
            }
            private double SetDisposition(double disposition)
            {
                //1: 1-7  (4)
                //2: 1-10 (5.5)
                //3: 4-10 (7)
                //0: 0 (always)
                //5: custom 1-9
                //4: 10 (never) 

                int min = Math.Min((int)disposition, 3);
                min = Program.rand.RangeInt(disposition > 1 ? 1 : 0, min);
                int max = Math.Max(Math.Min((int)disposition + 1, 10), 8);
                max = Program.rand.RangeInt(max, disposition < 9 ? 9 : 10);
                double[] ranges = new double[] { min, 4, 5.5, 7, max };

                double result = double.NaN;
                if (disposition > min && disposition < max && Program.rand.Bool(.91))
                {
                    for (int a = 1; a < ranges.Length ? true : throw new Exception(); a++)
                        if (disposition < ranges[a])
                        {
                            double range = ranges[a] - ranges[a - 1];
                            double chance = disposition - ranges[a - 1];
                            if (Program.rand.Bool(chance / range))
                                result = ranges[a];
                            else
                                result = ranges[a - 1];
                            break;
                        }
                }
                else
                {
                    result = Math.Min(Program.rand.GaussianCappedInt(disposition, .078), 10);
                }

                bool useRange = Program.rand.Bool(.91);
                if (result == 0)
                {
                    Monsters_disposition_standard = 0;
                    Monsters_disposition_custom = -1;
                }
                else if (useRange && result == 4)
                {
                    Monsters_disposition_standard = 1;
                    Monsters_disposition_custom = -1;
                }
                else if (result == 5.5)
                {
                    Monsters_disposition_standard = 2;
                    Monsters_disposition_custom = -1;
                }
                else if (useRange && result == 7)
                {
                    Monsters_disposition_standard = 3;
                    Monsters_disposition_custom = -1;
                }
                else if (result == 10)
                {
                    Monsters_disposition_standard = 4;
                    Monsters_disposition_custom = -1;
                }
                else
                {
                    if (result > 10 || (result != (int)result && result != 5.5))
                        throw new Exception();
                    Monsters_disposition_standard = 5;
                    Monsters_disposition_custom = Program.rand.Round(result);
                }

                return result;
            }

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
