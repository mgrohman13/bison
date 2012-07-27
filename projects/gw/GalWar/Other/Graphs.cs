using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class Graphs
    {
        private readonly List<Dictionary<GraphType, Dictionary<Player, float>>> data;
        private readonly Dictionary<GraphType, Dictionary<Player, float>> turnVals;

        private readonly Player[] players;

        internal Graphs(Game game)
        {
            this.data = new List<Dictionary<GraphType, Dictionary<Player, float>>>();
            this.turnVals = new Dictionary<GraphType, Dictionary<Player, float>>();
            this.players = game.GetPlayers();
        }

        internal void StartTurn(Player player)
        {
            float f, quality, armada, damaged;
            LoopColonies(player, out f, out quality);
            LoopShips(player, out armada, out damaged, out f);

            AddNested(turnVals, GraphType.Quality, player, quality);
            AddNested(turnVals, GraphType.Armada, player, armada);
            AddNested(turnVals, GraphType.ArmadaDamaged, player, damaged);
        }

        private static void AddNested(Dictionary<GraphType, Dictionary<Player, float>> dictionary, GraphType graphType, Player player, float amount)
        {
            Dictionary<Player, float> playerVals;
            if (!dictionary.TryGetValue(graphType, out playerVals))
            {
                playerVals = new Dictionary<Player, float>();
                dictionary.Add(graphType, playerVals);
            }
            playerVals.Add(player, amount);
        }

        internal void EndTurn(Player player)
        {
            float f, quality, armada, damaged;
            LoopColonies(player, out f, out quality);
            LoopShips(player, out armada, out damaged, out f);

            //values subject to major fluctuation depending on who moved last get averaged out to eliminate the effect of turn order
            EndTurn(GraphType.Quality, player, quality);
            EndTurn(GraphType.Armada, player, armada);
            EndTurn(GraphType.ArmadaDamaged, player, damaged);
        }

        private void EndTurn(GraphType graphType, Player player, float amount)
        {
            turnVals[graphType][player] = ( turnVals[graphType][player] + amount ) / 2f;
        }

        internal void Increment(Game game)
        {
            Player[] players = game.GetPlayers();
            Dictionary<Player, double> research = game.GetResearch();

            Dictionary<GraphType, Dictionary<Player, float>> playerGraphs = new Dictionary<GraphType, Dictionary<Player, float>>();

            foreach (Player player in this.players)
                if (Array.IndexOf(players, player) > -1)
                {
                    Add(playerGraphs, GraphType.Quality, player, turnVals[GraphType.Quality][player]);
                    Add(playerGraphs, GraphType.Armada, player, turnVals[GraphType.Armada][player]);
                    Add(playerGraphs, GraphType.ArmadaDamaged, player, turnVals[GraphType.ArmadaDamaged][player]);

                    Add(playerGraphs, GraphType.Research, player, (float)research[player]);
                    Add(playerGraphs, GraphType.TotalIncome, player, (float)player.IncomeTotal);

                    float pop, trans, f1, f2, f3;
                    LoopColonies(player, out pop, out f1);
                    LoopShips(player, out f2, out f3, out trans);

                    Add(playerGraphs, GraphType.Population, player, pop);
                    Add(playerGraphs, GraphType.PopulationTrans, player, pop + trans);
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

            turnVals.Clear();
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
                AddNested(playerGraphs, graphType, player, value);
        }

        private static void LoopColonies(Player player, out float pop, out float quality)
        {
            pop = 0;
            quality = 0;
            foreach (Colony colony in player.GetColonies())
            {
                pop += colony.Population;
                quality += (float)colony.Planet.PlanetValue;
            }
        }

        private static void LoopShips(Player player, out float armada, out float damaged, out float trans)
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

        //[turn,playerIndex,(0=turn,1=value)]
        public float[, ,] Get(GraphType type, out Dictionary<int, Player> playerIndexes)
        {
            float[, ,] retVal = new float[this.data.Count, this.players.Length, 2];
            //tells the caller which players are at which indices
            playerIndexes = new Dictionary<int, Player>();
            for (int b = 0 ; b < this.players.Length ; ++b)
                playerIndexes.Add(b, this.players[b]);

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
