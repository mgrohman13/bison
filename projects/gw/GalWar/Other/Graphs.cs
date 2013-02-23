using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GalWar
{
    [Serializable]
    public class Graphs
    {
        private readonly List<Dictionary<byte, Dictionary<byte, float>>> data;
        private readonly Dictionary<byte, Dictionary<byte, float>> turnVals;

        private readonly Player[] players;

        internal Graphs(Game game)
        {
            this.data = new List<Dictionary<byte, Dictionary<byte, float>>>();
            this.turnVals = new Dictionary<byte, Dictionary<byte, float>>();

            ReadOnlyCollection<Player> players = game.GetPlayers();
            this.players = new Player[players.Count];
            players.CopyTo(this.players, 0);
        }

        internal void StartTurn(Player player)
        {
            float f, quality, pd, armada, damaged;
            LoopColonies(player, out f, out quality, out pd);
            LoopShips(player, out armada, out damaged, out f);

            AddNested(turnVals, GraphType.Quality, player, quality);
            AddNested(turnVals, GraphType.Armada, player, pd + armada);
            AddNested(turnVals, GraphType.ArmadaDamaged, player, pd + damaged);
        }

        private static void AddNested(Dictionary<byte, Dictionary<byte, float>> dictionary, GraphType graphType, Player player, float amount)
        {
            Dictionary<byte, float> playerVals;
            if (!dictionary.TryGetValue((byte)graphType, out playerVals))
            {
                playerVals = new Dictionary<byte, float>();
                dictionary.Add((byte)graphType, playerVals);
            }
            playerVals.Add((byte)player.ID, amount);
        }

        internal void EndTurn(Player player)
        {
            float f, quality, pd, armada, damaged;
            LoopColonies(player, out f, out quality, out pd);
            LoopShips(player, out armada, out damaged, out f);

            //values subject to major fluctuation depending on who moved last get averaged out to eliminate the effect of turn order
            EndTurn(GraphType.Quality, player, quality);
            EndTurn(GraphType.Armada, player, pd + armada);
            EndTurn(GraphType.ArmadaDamaged, player, pd + damaged);
        }

        private void EndTurn(GraphType graphType, Player player, float amount)
        {
            turnVals[(byte)graphType][(byte)player.ID] = ( turnVals[(byte)graphType][(byte)player.ID] + amount ) / 2f;
        }

        internal void Increment(Game game)
        {
            ReadOnlyCollection<Player> players = game.GetPlayers();
            Dictionary<Player, double> research = game.GetResearch();

            Dictionary<byte, Dictionary<byte, float>> playerGraphs = new Dictionary<byte, Dictionary<byte, float>>();

            foreach (Player player in this.players)
                if (players.IndexOf(player) > -1)
                {
                    Add(playerGraphs, GraphType.Quality, player, turnVals[(byte)GraphType.Quality][(byte)player.ID]);
                    Add(playerGraphs, GraphType.Armada, player, turnVals[(byte)GraphType.Armada][(byte)player.ID]);
                    Add(playerGraphs, GraphType.ArmadaDamaged, player, turnVals[(byte)GraphType.ArmadaDamaged][(byte)player.ID]);

                    Add(playerGraphs, GraphType.Research, player, (float)research[player]);
                    Add(playerGraphs, GraphType.TotalIncome, player, (float)player.IncomeTotal);

                    float pop, trans, f1, f2, f3, f4;
                    LoopColonies(player, out pop, out f1, out f4);
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

        private void Add(Dictionary<byte, Dictionary<byte, float>> playerGraphs, GraphType graphType, Player player, float value)
        {
            float last = -2;
            for (int x = this.data.Count ; --x > -1 ; )
            {
                Dictionary<byte, float> prevGraphs;
                if (this.data[x].TryGetValue((byte)graphType, out prevGraphs))
                {
                    float lastVal;
                    if (prevGraphs.TryGetValue((byte)player.ID, out lastVal))
                    {
                        last = lastVal;
                        break;
                    }
                }
            }

            if (last < -1 || Math.Abs(last - value) / ( last + value ) > 0.0001)
                AddNested(playerGraphs, graphType, player, value);
        }

        private static void LoopColonies(Player player, out float pop, out float quality, out float armada)
        {
            pop = 0;
            quality = 0;
            armada = 0;
            foreach (Colony colony in player.GetColonies())
            {
                armada += (float)( colony.PlanetDefenseStrength / 2.1 );
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
                Dictionary<byte, float> graphs = null;
                data[a].TryGetValue((byte)type, out graphs);

                for (int b = 0 ; b < this.players.Length ; ++b)
                {
                    float value;
                    if (graphs == null || !graphs.TryGetValue((byte)playerIndexes[b].ID, out value))
                        value = retVal[a - 1, b, 1];

                    retVal[a, b, 0] = a;
                    retVal[a, b, 1] = value;
                }
            }

            return retVal;
        }

        public enum GraphType
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
