using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    public class Player
    {
        private static int counter = 1;

        public readonly int ID;
        public readonly bool Human, AIstrong, AIprimary;
        private readonly List<Zone> zones;
        private Zone home;
        private Player paired;
        public ReadOnlyCollection<Zone> Zones => zones.AsReadOnly();
        public Zone Home => home;
        public Player Paired => paired;

        public Player(Player aiSecondaryOf)
            : this(false, true, false)
        {
            this.ID = aiSecondaryOf.ID;
            SetPair(this, aiSecondaryOf);
        }
        public Player(bool human, bool aiStrong)
            : this(human, aiStrong, !human)
        {
            this.ID = counter++;
        }
        private Player(bool human, bool aiStrong, bool aiPrimary)
        {
            if (human ? aiStrong || aiPrimary : !aiStrong && !aiPrimary)
                throw new Exception();

            this.Human = human;
            this.AIstrong = aiStrong;
            this.AIprimary = aiPrimary;

            zones = new();
        }

        public void AddZone(Zone zone)
        {
            if (home == null)
                home = zone;
            zones.Add(zone);
        }
        public static void SetPair(Player p1, Player p2)
        {
            if (p1 == p2)
                throw new Exception();
            p1.paired = p2;
            p2.paired = p1;
        }

        public static void InitMines(Player[] players, List<Zone> zones, double size)
        {
            double numZones = zones.Count;
            double zonesPerPlayer = numZones / (double)Program.NumPlayers;
            //s=1.368 m=1.631 l=1.866 xl=2.084  (1-level)
            double sizeMult = 1 + Math.Pow(size / 13, .39);
            double minesAvg = (.26 + .52 * Math.Sqrt(zonesPerPlayer)) * sizeMult;

            //player mines
            int playerWoodOre = Program.rand.GaussianOEInt(1 + (Program.PairPlayer ? .13 : .52), .13, .013);
            int pairedWoodOre = Program.PairPlayer ? Program.rand.GaussianOEInt(playerWoodOre == 0 ? .52 : .91, .13, .013) : 0;
            int pWoodOre = Math.Max(0, 2 - (playerWoodOre + pairedWoodOre) + Program.rand.OEInt(minesAvg / 9.1));
            int pResource = Program.rand.GaussianOEInt(minesAvg, .13, .065, 1);
            int pGold = Program.rand.GaussianOEInt(.52 + minesAvg / 3.9, .26, .065);
            int pairedResource = Program.rand.GaussianOEInt(.26, .13, .065), pairedGold = Program.rand.GaussianOEInt(.13, .26, .065);

            //paired balancing
            if (Program.PairPlayer && playerWoodOre > pairedWoodOre)
                if (pGold > 0 && Program.rand.Bool(.91))
                {
                    pGold--;
                    pairedGold++;
                }
                else
                {
                    int xfer = Program.rand.RangeInt(1, Program.rand.Round(Math.Pow(pResource, .65)));
                    pResource -= xfer;
                    pairedResource += xfer;
                }

            //ai wood/ore
            double numAIs = Program.NumPlayers - (Program.PairPlayer ? 2 : 1);
            var AIs = players.Where(p => !p.Human && (p.paired == null || !p.paired.Human));
            Dictionary<Player, int> aiHomeWoodOre = Program.rand.Iterate(AIs).ToDictionary(p => p, p => Program.rand.GaussianOEInt(p.AIstrong ? 1 : 2, .13, .013));
            int aiHomeWoodOreCount = aiHomeWoodOre.Values.Sum();
            int aiWoodOre = Math.Max(0, 1 + 2 * (int)numAIs - aiHomeWoodOreCount + Program.rand.OEInt(numAIs * minesAvg / 9.1));

            //ai resource mines
            double aiResourceAvg = Math.Max(1 + Program.NumPlayers * minesAvg - pResource - pairedResource, Program.NumPlayers);
            int aiResource = Program.rand.GaussianOEInt(aiResourceAvg, .13, .091, Program.NumPlayers);

            //balancing
            var playerZones = players.Where(p => p.Human || (p.paired != null && p.paired.Human)).SelectMany(p => p.zones);
            var aiZones = AIs.SelectMany(p => p.zones);
            if (playerZones.Intersect(aiZones).Any())
                throw new Exception();
            double aiGoldAvg = 1;

            //towns balancing
            double playerTowns = playerZones.Sum(z => z.NumTowns);
            double aiTowns = aiZones.Sum(z => z.NumTowns) / numAIs;
            aiGoldAvg += (playerTowns - aiTowns) * 2.1;
            //wood/ore balancing 
            aiGoldAvg += (playerWoodOre * 1.3 + pairedWoodOre / 1.3 + pWoodOre - (aiHomeWoodOreCount + aiWoodOre) / numAIs) / 2.1;
            //resource mine balancing
            aiGoldAvg += (pairedResource / 1.3 + pResource - aiResource / numAIs) / 1.69;
            //player gold mine balancing
            aiGoldAvg += pGold + pairedGold / 1.3;

            //balancing
            if (aiGoldAvg < 0)
            {
                int xferGold = (int)Math.Ceiling(-aiGoldAvg);
                aiGoldAvg += xferGold;
                int xferResource = Program.rand.RangeInt(0, xferGold);
                xferGold -= xferResource;
                xferResource *= 2;
                pGold += xferGold;
                pResource += xferResource;
            }
            double mult = Math.Sqrt(numAIs);
            aiGoldAvg *= mult;

            //ai gold mines
            int aiGold = aiGoldAvg > 1 ? Program.rand.Round(aiGoldAvg - 1) : 0;
            aiGoldAvg -= aiGold;
            aiGoldAvg *= 2;
            aiResource += Program.rand.Round(aiGoldAvg);
            if (aiGold == 0 && Program.rand.Bool())
            {
                aiGold = Program.rand.Round(mult);
                pGold++;
            }

            //place home mines
            Player human = players.Single(p => p.Human);
            human.home.AddMines(playerWoodOre, 0, 0);
            if (Program.PairPlayer)
                human.paired.home.AddMines(pairedWoodOre, pairedResource, pairedGold);
            foreach (var pair in Program.rand.Iterate(aiHomeWoodOre))
                pair.Key.home.AddMines(pair.Value, 0, 0);

            //place all mines
            HashSet<Player> place = players.ToHashSet();
            double strongWeight = 1.3;
            numAIs += (strongWeight - 1) * players.Count(p => p.AIstrong) / 2.0;
            while (place.Any())
            {
                Player p1 = Program.rand.SelectValue(place);
                Player p2 = p1.paired;
                place.Remove(p1);
                place.Remove(p2);
                IEnumerable<Zone> placeZones = p1.zones.Where(z => z != p1.home);
                if (p2 != null)
                    placeZones = placeZones.Concat(p2.zones.Where(z => z != p2.home));

                int totalWoodOre, totalResource, totalGold;
                if (p1.Human || (p2 != null && p2.Human))
                {
                    totalWoodOre = pWoodOre;
                    totalResource = pResource;
                    totalGold = pGold;
                }
                else
                {
                    double weight = p1.AIstrong ? strongWeight : 1;
                    totalWoodOre = Program.rand.Round(aiWoodOre * weight / numAIs);
                    totalResource = Program.rand.Round(aiResource * weight / numAIs);
                    totalGold = Program.rand.Round(aiGold * weight / numAIs);
                    numAIs -= weight;
                    aiWoodOre -= totalWoodOre;
                    aiResource -= totalResource;
                    aiGold -= totalGold;
                }

                //place most in non-home zones
                double div = Math.Sqrt(placeZones.Count() + .39);
                foreach (Zone z in Program.rand.Iterate(placeZones))
                {
                    int woodOre = Program.rand.Round(totalWoodOre / div);
                    int resource;
                    do
                        resource = Program.rand.GaussianOEInt(totalResource / div, .13, .13);
                    while (resource > totalResource);
                    int gold = Program.rand.Round(totalGold / div);
                    z.AddMines(woodOre, resource, gold);
                    totalWoodOre -= woodOre;
                    totalResource -= resource;
                    totalGold -= gold;
                }

                //place the remaining in the home zones
                if (p2 != null)
                {
                    int woodOre = Program.rand.Round(totalWoodOre / 2.0);
                    int resource = Program.rand.Round(totalResource / 2.0);
                    int gold = Program.rand.Round(totalGold / 2.0);
                    p2.home.AddMines(woodOre, resource, gold);
                    totalWoodOre -= woodOre;
                    totalResource -= resource;
                    totalGold -= gold;
                }
                p1.home.AddMines(totalWoodOre, totalResource, totalGold);
            }

            if (aiWoodOre != 0 || aiResource != 0 || aiGold != 0 || Math.Abs(numAIs) > .0000001)
                throw new Exception();

            bool any;
            do
            {
                int CountMines(Zone zone) => zone.WoodOreMines + zone.ResourceMines + zone.GoldMines;
                int CanRemove(Func<Zone, int> GetCount) => Program.rand.Bool(.91) ? Math.Max(0, Math.Min(zones.Min(GetCount), Program.rand.Round(zones.Average(GetCount)) - 1)) : 0;
                void ModRandomMine(Zone zone, Action<Zone> WoodOre, Action<Zone> Resource, Action<Zone> Gold)
                {
                    switch (Program.rand.SelectValue(new Dictionary<string, int>()
                        { { "wo", zone.WoodOreMines }, { "r", zone.ResourceMines }, { "g", zone.GoldMines }, }))
                    {
                        case "wo":
                            WoodOre(zone);
                            break;
                        case "r":
                            Resource(zone);
                            break;
                        case "g":
                            Gold(zone);
                            break;
                        default: throw new Exception();
                    }
                }

                //remove any excess mines in every zone 
                int removeWoodOre = CanRemove(z => z.WoodOreMines);
                int removeResource = CanRemove(z => z.ResourceMines);
                int removeGold = CanRemove(z => z.GoldMines);
                any = removeWoodOre > 0 || removeResource > 0 || removeGold > 0;
                if (any)
                    foreach (Zone zone in Program.rand.Iterate(zones))
                        zone.RemoveMines(removeWoodOre, removeResource, removeGold);
                while (CanRemove(CountMines) > 0)
                {
                    any = true;
                    foreach (Zone zone in Program.rand.Iterate(zones))
                        ModRandomMine(zone, wo =>
                        {
                            zone.RemoveMines(1, 0, 0);
                        }, r =>
                        {
                            zone.RemoveMines(0, 1, 0);
                            zone.AddMines(1, 0, 0);
                        }, g =>
                        {
                            zone.RemoveMines(0, 0, 1);
                            zone.AddMines(0, 1, 0);
                        });
                }

                //move mines from zones with a large number of them to a neighbor
                double minesPerZone = Math.PI * Math.Sqrt(zones.Average(CountMines));
                foreach (Zone zone1 in Program.rand.Iterate(zones))
                    if (Program.rand.GaussianOE(CountMines(zone1), .26, .065) > minesPerZone)
                    {
                        any = true;
                        Zone zone2 = Program.rand.SelectValue(zone1.Connections.Keys, z =>
                        {
                            double str = 3900 + zone1.Connections[z].Min(c => c.Strength);
                            str /= 3900;
                            double mines = .52 + CountMines(z);
                            return Program.rand.Round(ushort.MaxValue / str / str / mines);
                        });
                        ModRandomMine(zone1, wo =>
                        {
                            zone1.RemoveMines(1, 0, 0);
                            zone2.AddMines(1, 0, 0);
                        }, r =>
                        {
                            zone1.RemoveMines(0, 1, 0);
                            zone2.AddMines(0, 1, 0);
                        }, g =>
                        {
                            zone1.RemoveMines(0, 0, 1);
                            zone2.AddMines(0, 0, 1);
                        });
                    }
            } while (any);
        }

        public static void Generate(Player[] players, List<Connections> connections)
        {
            HashSet<Player> tempPlayers = players.ToHashSet();
            while (tempPlayers.Any())
            {
                Player player = Program.rand.SelectValue(tempPlayers);
                tempPlayers.Remove(player);
                IEnumerable<Zone> zones = Program.rand.Iterate(player.Zones);
                if (player.paired != null)
                {
                    //generate paired players together so mine type distributed is cumulative 
                    zones = zones.Concat(Program.rand.Iterate(player.paired.Zones));
                    tempPlayers.Remove(player.paired);
                }

                if (player.AIstrong)
                {
                    Zone primary = player.AIprimary ? player.home : player.paired.home;
                    Zone secondary = player.AIprimary ? player.paired.home : player.home;
                    //for players with 2 starting locations, generate the primary one first, since it will get unguarded resources and in-game is the one that starts with the hero
                    primary.Generate(connections);
                    secondary.Generate(connections);
                    zones = zones.Except(new Zone[] { primary, secondary });
                }

                foreach (Zone zone in zones)
                    zone.Generate(connections);
            }
        }
    }
}
