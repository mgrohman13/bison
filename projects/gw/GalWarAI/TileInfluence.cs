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
            influence.Add(InfluenceType.DeathStar, new Influence());
            influence.Add(InfluenceType.Population, new Influence());
            influence.Add(InfluenceType.Armada, new Influence());
            influence.Add(InfluenceType.Transport, new Influence());
            influence.Add(InfluenceType.Quality, new Influence());

            foreach (Player p in game.GetPlayers())
                foreach (Ship s in p.GetShips())
                {
                    double mod = GetMod(tile, s.Tile, s.MaxSpeed);
                    double str = s.GetStrength() * mod;
                    double trans = s.Population * mod;
                    double ds = s.BombardDamage * s.MaxSpeed * mod;
                    influence[InfluenceType.Armada].Add(p, str);
                    influence[InfluenceType.Transport].Add(p, trans);
                    influence[InfluenceType.DeathStar].Add(p, ds);
                }
            foreach (Planet p in game.GetPlanets())
                if (p.Colony != null)
                {
                    double mod = GetMod(tile, p.Tile, null);
                    double prod = p.Colony.Population * mod;
                    double quality = p.Quality * mod;
                    influence[InfluenceType.Population].Add(p.Colony.Player, prod);
                    influence[InfluenceType.Quality].Add(p.Colony.Player, quality);
                }
        }

        private double GetMod(Tile t1, Tile t2, int? speed)
        {
            double dist = Math.Pow(Tile.GetDistance(t1, t2), 1.69);
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

        public double GetTotalEnemyInfluence(params Tuple<InfluenceType, double>[] types)
        {
            double tot = 0;
            foreach (Tuple<InfluenceType, double> tuple in types)
                tot += influence[tuple.Item1].GetTotalEnemyInfluence() * tuple.Item2;
            return tot;
        }

        public class Influence
        {
            private SortedDictionary<Player, double> values;
            private Dictionary<Player, int> ranks;

            public Influence()
            {
                values = new SortedDictionary<Player, double>();
            }

            public KeyValuePair<Player, double> GetRank(int rank)
            {
                foreach (KeyValuePair<Player, double> pair in values)
                    if (--rank < 0)
                        return pair;
                throw new Exception();
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

            public double GetTotalEnemyInfluence()
            {
                double tot = 0;
                foreach (KeyValuePair<Player, double> pair in values)
                    if (!pair.Key.IsTurn)
                        tot += pair.Value;
                return tot;
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
            Quality,
            Population,
            Armada,
            Transport,
            DeathStar,
        }
    }
}
