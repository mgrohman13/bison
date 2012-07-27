using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    class TileInfluence
    {
        internal static void ClearCache()
        {
            //TODO influence caching? - maybe be more intelligent with distances to avoid looping so much?
        }

        public readonly Tile tile;

        private Dictionary<InfluenceType, Influence> influence;

        private TileInfluence(Game game, GalWarAI ai, Tile tile)
        {
            this.tile = tile;

            influence = new Dictionary<InfluenceType, Influence>();
            influence.Add(InfluenceType.DeathStar, new Influence());
            influence.Add(InfluenceType.Population, new Influence());
            influence.Add(InfluenceType.Armada, new Influence());
            influence.Add(InfluenceType.Transport, new Influence());
            influence.Add(InfluenceType.Quality, new Influence());

            foreach (Ship s in ai.LoopShips())
            {
                double mod = GetMod(tile, s.Tile, s.MaxSpeed);
                double str = s.GetStrength() * mod;
                double trans = s.Population * mod;
                double ds = s.BombardDamage * s.MaxSpeed * mod;
                influence[InfluenceType.Armada].Add(s.Player, str);
                influence[InfluenceType.Transport].Add(s.Player, trans);
                influence[InfluenceType.DeathStar].Add(s.Player, ds);
            }
            foreach (Colony c in ai.LoopColonies())
            {
                double mod = GetMod(tile, c.Tile, null);
                double prod = c.Population * mod;
                double quality = c.Planet.Quality * mod;
                influence[InfluenceType.Population].Add(c.Player, prod);
                influence[InfluenceType.Quality].Add(c.Player, quality);
            }
        }

        public static TileInfluence GetInfluence(Game game, GalWarAI ai, Tile tile)
        {
            return new TileInfluence(game, ai, tile);
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
            private Dictionary<Player, double> values;
            private Dictionary<Player, int> ranks;

            public Influence()
            {
                values = new Dictionary<Player, double>();
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
                    //TODO: wont work - need sorting by value
                    foreach (Player p in values.Keys)
                        ranks[p] = ++rank;
                }
                return ranks[player];
            }

            public double GetValue(Player player)
            {
                double val;
                values.TryGetValue(player, out val);
                return val;
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
