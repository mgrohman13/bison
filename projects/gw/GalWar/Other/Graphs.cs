using System;
using System.Collections.Generic;
using System.Text;

namespace GalWar
{
    [Serializable]
    public class Graphs
    {

        private readonly List<Dictionary<GraphType, Dictionary<Player, float>>> data;

        private readonly Player[] players;

        internal Graphs(Game game)
        {
            this.data = new List<Dictionary<GraphType, Dictionary<Player, float>>>();
            this.players = game.GetPlayers().ToArray();
        }

        internal void Increment(Game game)
        {
            List<Player> players = game.GetPlayers();
            Dictionary<Player, double> research = game.GetResearch();

            Dictionary<GraphType, Dictionary<Player, float>> playerGraphs = new Dictionary<GraphType, Dictionary<Player, float>>();

            foreach (Player player in this.players)
                if (players.Contains(player))
                {
                    Add(playerGraphs, GraphType.Research, player, (float)research[player]);

                    //loop once through colonies
                    float pop, quality;
                    LoopColonies(player, out pop, out quality);

                    Add(playerGraphs, GraphType.Population, player, pop);
                    Add(playerGraphs, GraphType.Quality, player, quality);

                    //loop once through ships
                    float armada, damaged, trans;
                    LoopShips(player, out armada, out damaged, out trans);

                    Add(playerGraphs, GraphType.Armada, player, armada);
                    Add(playerGraphs, GraphType.ArmadaDamaged, player, damaged);

                    Add(playerGraphs, GraphType.PopulationTrans, player, pop + trans);

                    Add(playerGraphs, GraphType.TotalIncome, player, (float)player.IncomeTotal);
                }
                else
                {
                    Add(playerGraphs, GraphType.Research, player, 0);
                    Add(playerGraphs, GraphType.Population, player, 0);
                    Add(playerGraphs, GraphType.Quality, player, 0);
                    Add(playerGraphs, GraphType.Armada, player, 0);
                    Add(playerGraphs, GraphType.ArmadaDamaged, player, 0);
                    Add(playerGraphs, GraphType.PopulationTrans, player, 0);
                    Add(playerGraphs, GraphType.TotalIncome, player, 0);
                }

            data.Add(playerGraphs);
        }

        private void Add(Dictionary<GraphType, Dictionary<Player, float>> playerGraphs, GraphType graphType, Player player, float value)
        {
            float last = -2;
            for (int x = this.data.Count ; --x > -1 ; )
            {
                Dictionary<Player, float> prevGraphs;
                if (this.data[x].TryGetValue(graphType, out prevGraphs))
                {
                    float lastVal;
                    if (prevGraphs.TryGetValue(player, out lastVal))
                    {
                        last = lastVal;
                        break;
                    }
                }
            }

            if (last < -1 || Math.Abs(last - value) / ( last + value ) > 0.0001)
            {
                Dictionary<Player, float> graphs;
                if (!playerGraphs.TryGetValue(graphType, out graphs))
                {
                    graphs = new Dictionary<Player, float>();
                    playerGraphs.Add(graphType, graphs);
                }
                graphs.Add(player, value);
            }
        }

        private void LoopColonies(Player player, out float pop, out float quality)
        {
            pop = 0;
            quality = 0;
            foreach (Colony colony in player.GetColonies())
            {
                pop += colony.Population;
                quality += (float)colony.Planet.PlanetValue;
            }
        }

        private void LoopShips(Player player, out float armada, out float damaged, out float trans)
        {
            armada = 0;
            damaged = 0;
            trans = 0;
            foreach (Ship ship in player.GetShips())
            {
                float strength = (float)ship.GetStrength();
                armada += strength;
                damaged += strength * ship.HP / (float)ship.MaxHP;
                trans += ship.Population;
            }
        }

        public float[, ,] Get(GraphType type, out Dictionary<int, Player> playerIndexes)
        {
            float[, ,] retVal = new float[this.data.Count, this.players.Length, 2];
            //tells the caller which players are at which indices
            playerIndexes = new Dictionary<int, Player>();
            for (int b = 0 ; b < this.players.Length ; ++b)
                playerIndexes[b] = this.players[b];

            for (int a = 0 ; a < this.data.Count ; ++a)
            {
                Dictionary<Player, float> graphs = null;
                data[a].TryGetValue(type, out graphs);

                for (int b = 0 ; b < this.players.Length ; ++b)
                {
                    float value;
                    if (graphs == null || !graphs.TryGetValue(playerIndexes[b], out value))
                        value = retVal[a - 1, b, 1];

                    retVal[a, b, 0] = a;
                    retVal[a, b, 1] = value;
                }
            }

            return retVal;
        }

        public enum GraphType : byte
        {
            Population,
            PopulationTrans,
            Quality,
            Armada,
            ArmadaDamaged,
            Research,
            TotalIncome,
        }

    }
}
