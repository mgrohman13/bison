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
        private Zone home;
        public Zone Home => home;
        private readonly List<Zone> zones;
        public ReadOnlyCollection<Zone> Zones => zones.AsReadOnly();
        private Player paired;
        public readonly bool AI;
        public Player(bool AI)
        {
            zones = new();
            this.AI = AI;
        }

        public void AddZone(Zone zone)
        {
            if (home == null)
                home = zone;
            zones.Add(zone);
        }
        public static void SetPair(Player p1, Player p2)
        {
            p1.paired = p2;
            p2.paired = p1;
        }

        public static void InitMines(Player[] players, bool pairPlayers, double size)
        {
            double zonesPerPlayer = players.Average(p => p.zones.Count);
            double sizeMult = Math.Pow(size / 13, .39);
            double avgMines = (2.1 + .13 * Math.Sqrt(zonesPerPlayer)) * sizeMult;
            int homeWoodOre = 1 + Program.rand.Round((pairPlayers ? .39 : .65) * Math.Sqrt(sizeMult));
            int woodOre = Program.rand.GaussianOEInt(avgMines / 7.8, .39, .13);
            if (homeWoodOre == 1 && woodOre == 0)
                woodOre = Program.rand.Round(pairPlayers ? .13 : .65);
            int resource = Program.rand.GaussianOEInt(avgMines, .13, .13, 1);
            int gold = Program.rand.GaussianOEInt(.52 + avgMines / 5.2, .26, .13);

            if (pairPlayers)
            {
                if (homeWoodOre == 2 || woodOre > 1)
                {
                    woodOre -= Program.rand.RangeInt(0, homeWoodOre + woodOre > 2 ? 2 : 1);
                    if (woodOre < 0)
                        woodOre = 0;
                }
                resource = Program.rand.Round((resource + 1) / 2.0);
                gold = Program.rand.Round(gold / 2.0);
            }

            foreach (Player player in Program.rand.Iterate(players))
            {
                IEnumerable<Zone> zones = player.zones.Where(z => z != player.home);

                int a = woodOre, b = resource, c = gold;
                double div = Math.Sqrt(zones.Count() + .39);
                foreach (Zone z in Program.rand.Iterate(zones))
                {
                    int d = Program.rand.Round(a / div);
                    int e;
                    do
                        e = Program.rand.GaussianOEInt(b / div, .13, .13);
                    while (e > b);
                    int f = Program.rand.Round(c / div);
                    z.AddMines(d, e, f);
                    a -= d;
                    b -= e;
                    c -= f;
                }
                player.home.AddMines(homeWoodOre + a, b, c);
            }
        }

        public static void Generate(Player[] players, List<Connections> connections, double zoneSize)
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
                foreach (Zone zone in zones)
                    zone.Generate(connections, players.Length, zoneSize);
            }
        }
    }
}
