using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    class TileInfluence
    {
        public readonly Tile tile;

        private Dictionary<InfluenceType, Influence> influence;

        public TileInfluence(Game game, Tile tile)
        {
            this.tile = tile;

            influence = new Dictionary<InfluenceType, Influence>();
            influence.Add(InfluenceType.DS, new Influence());
            influence.Add(InfluenceType.Prod, new Influence());
            influence.Add(InfluenceType.Str, new Influence());
            influence.Add(InfluenceType.Trans, new Influence());

            foreach (Player p in game.GetPlayers())
                foreach (Ship s in p.GetShips())
                {
                    double mod = GetMod(tile, s.Tile, s.MaxSpeed);
                    double str = s.GetStrength() * mod;
                    double trans = s.Population * mod;
                    double ds = s.BombardDamage * mod;
                    influence[InfluenceType.Str].Add(p, str);
                    influence[InfluenceType.Trans].Add(p, trans);
                    influence[InfluenceType.DS].Add(p, ds);
                }
            foreach (Planet p in game.GetPlanets())
                if (p.Colony != null)
                {
                    double mod = GetMod(tile, p.Tile, null);
                    double prod = p.Colony.Population * mod;
                    influence[InfluenceType.Prod].Add(p.Colony.Player, prod);
                }
        }

        private double GetMod(Tile t1, Tile t2, int? speed)
        {
            double dist = Math.Pow(Tile.GetDistance(t1, t2), 1.3);
            if (speed.HasValue)
                dist /= ( speed.Value + 1.0 );
            return 1 / dist;
        }

        public Influence GetInfluence(InfluenceType type)
        {
            return influence[type];
        }

        public double GetInfluence(InfluenceType type, Player player)
        {
            return influence[type].GetValue(player);
        }

        public class Influence
        {
            private SortedDictionary<Player, double> values;
            private Dictionary<Player, int> ranks;

            public Influence()
            {
                values = new SortedDictionary<Player, double>();
            }

            public int GetRank(Player player)
            {
                if (ranks == null)
                {
                    ranks = new Dictionary<Player, int>();
                    int rank = -1;
                    foreach (Player p in values.Keys)
                        ranks[p] = ++rank;
                }
                return ranks[player];
            }

            public double GetValue(Player player)
            {
                return values[player];
            }

            internal void Add(Player p, double amount)
            {
                double v;
                values.TryGetValue(p, out v);
                values[p] = amount + v;
            }
        }

        public enum InfluenceType
        {
            Str,
            Prod,
            Trans,
            DS,
        }
    }
}
