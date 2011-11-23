using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MattUtil;

namespace Daemons
{
    [Serializable]
    public class Game
    {
        public static MattUtil.MTRandom Random;

        static Game()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();
        }

        private readonly int width, height;
        private readonly Tile[,] map;
        private readonly List<ProductionCenter> production;

        private int currentPlayer, turn;
        private Player[] players;
        private readonly Player independent;
        private bool independentsTurn;

        private readonly List<Player> lost;
        public string log;

        public Game(Player[] newPlayers, int newWidth, int newHeight)
        {
            this.width = newWidth;
            this.height = newHeight;

            this.map = new Tile[width, height];
            for (int x = 0 ; x < width ; x++)
                for (int y = 0 ; y < height ; y++)
                    map[x, y] = new Tile(this, x, y);
            Tile.CreateNeighborReferences(map, width, height);

            const float rand = .0666f;
            const int min = 1;
            this.production = new List<ProductionCenter>();
            int[] num = new int[3];
            num[0] = Random.GaussianCappedInt(newPlayers.Length / 2f, rand, min);
            num[1] = Random.GaussianCappedInt(newPlayers.Length, rand, min);
            num[2] = Random.GaussianCappedInt(3f * newPlayers.Length / 2f, rand, min);
            for (int a = 0 ; a < num.Length ; a++)
                for (int b = 0 ; b < num[a] ; b++)
                    production.Add(new ProductionCenter(GetRandomTile(), a));

            this.currentPlayer = -1;
            this.players = new Player[newPlayers.Length];
            this.independent = new Player(this, Color.DarkGray, "Independents", true);
            this.independentsTurn = false;
            int index = -1;
            int lastSouls = 0;
            float addSouls, addArrows;
            GetMoveDiff(out addSouls, out addArrows);
            this.turn = 6;
            foreach (Player player in Random.Iterate<Player>(newPlayers))
            {
                int souls = lastSouls + RandSouls(addSouls);
                lastSouls = souls;
                players[++index] = new Player(this, player.Color, player.Name, souls);
                for (int b = -1 ; b < index ; ++b)
                    players[index].MakeArrow(addArrows);
                IndependentsTurn();
            }
            this.turn = 1;

            this.lost = new List<Player>();
            this.log = string.Empty;

            Dictionary<UnitType, int> startUnits = new Dictionary<UnitType, int>();
            startUnits.Add(UnitType.Knight, 1);
            startUnits.Add(UnitType.Archer, 2);
            startUnits.Add(UnitType.Infantry, 3);
            for (int a = 6 ; a < 13 ; ++a)
            {
                UnitType unitType = Random.SelectValue<UnitType>(startUnits);
                ++startUnits[unitType];
            }
            for (int a = 0 ; a < 13 ; ++a)
            {
                UnitType unitType = Random.SelectValue<UnitType>(startUnits);
                --startUnits[unitType];
                foreach (Player player in Random.Iterate<Player>(players))
                    AddUnit(player, unitType);
            }

            foreach (Player player in players)
                player.ResetMoves();
            this.currentPlayer = 0;
        }

        public string Turn
        {
            get
            {
                return turn.ToString();
            }
        }

        public List<Player> GetLost()
        {
            return lost;
        }

        public Player[] GetPlayers()
        {
            return players;
        }

        public Player GetIndependent()
        {
            return independent;
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public Tile GetTile(int x, int y)
        {
            return map[x, y];
        }

        public Player GetCurrentPlayer()
        {
            if (independentsTurn)
                return independent;
            return players[currentPlayer];
        }

        public void Log(String message)
        {
            log = message + "\r\n" + log;
        }

        private void AddUnit(Player player, UnitType type)
        {
            new Unit(type, GetTile(player), player);
        }

        private Tile GetTile(Player player)
        {
            Tile tile;
            Player occupying;
            do
            {
                tile = GetRandomTile();
            } while (tile.Occupied(out occupying) && occupying != player);
            return tile;
        }

        internal Tile GetRandomTile()
        {
            return GetTile(Random.Next(width), Random.Next(height));
        }

        public void EndTurn()
        {
            if (players.Length == 1)
            {
                currentPlayer = 0;
                Player winner = players[currentPlayer];
                if (lost[0] != winner)
                    lost.Insert(0, winner);
            }
            else
            {
                ProcessBattles();
                players[currentPlayer].ResetMoves();

                currentPlayer++;
                CheckTurnInc();
            }

            AutoSave();
        }

        private void ProcessBattles()
        {
            bool any = true;
            while (any)
            {
                any = false;
                foreach (Tile tile in Random.Iterate<Tile>(map.Cast<Tile>()))
                    any |= tile.FightBattle();
            }
        }

        private void CheckTurnInc()
        {
            if (currentPlayer >= players.Length)
            {
                currentPlayer = -1;
                IncTurn();
                currentPlayer = 0;
            }
        }

        private void IncTurn()
        {
            IndependentsTurn();

            turn++;
            foreach (ProductionCenter pc in production)
                pc.used = false;

            ChangeMoveOrder();
            ChangeMap();
        }

        private void IndependentsTurn()
        {
            independentsTurn = true;

            foreach (Tile t in map)
            {
                int x = t.X, y = t.Y;
                MoveRand(ref x, ref y);
                Tile moveTo = GetTile(x, y);
                foreach (Unit unit in t.GetUnits(independent))
                    if (unit.Healed && unit.Movement > 0)
                        unit.Move(moveTo);
            }

            float mult = production.Count;
            independent.AddSouls(Random.GaussianInt(2.6f * mult * ( turn + 16.9f ), .13f));
            independent.AddSouls(Random.OEInt(3.0 * mult * turn));

            int amt = independent.RoundSouls();
            if (amt > 0)
            {
                independent.AddSouls(-666 * amt);
                Tile tile = GetTile(independent);
                for (int a = 0 ; a < amt ; a++)
                    new Unit(UnitType.Indy, tile, independent);
            }

            ProcessBattles();
            independent.ResetMoves();

            independentsTurn = false;
        }

        private void ChangeMap()
        {
            for (int a = 0 ; a < production.Count ; a++)
                if (Random.Bool(.3f))
                    MoveRand(ref production[a].x, ref production[a].y);
        }

        public void MoveRand(ref int x, ref int y)
        {
            switch (Random.Next(7))
            {
            case 0:
                x--;
                break;
            case 1:
                x++;
                break;
            case 2:
                y--;
                break;
            case 3:
                y++;
                break;
            default:
                return;
            }

            bool retry = true;
            if (x < 0)
                x = 0;
            else if (x >= width)
                x = width - 1;
            else if (y < 0)
                y = 0;
            else if (y >= height)
                y = height - 1;
            else
                retry = false;
            if (retry)
                MoveRand(ref x, ref y);
        }

        private void ChangeMoveOrder()
        {
            foreach (KeyValuePair<Player, int> pair in MattUtil.TBSUtil.RandMoveOrder<Player>(Random, players, .169))
            {
                float souls, arrows;
                GetMoveDiff(out souls, out arrows);
                pair.Key.AddSouls(RandSouls(souls));
                pair.Key.MakeArrow(arrows);
            }
        }

        private void GetMoveDiff(out float souls, out float arrows)
        {
            float div = players.Length - 1f;
            souls = 666f / div;
            arrows = 16.9f / div;
        }

        private static int RandSouls(float addSouls)
        {
            return Random.GaussianCappedInt(addSouls, .09f, Random.Round(.78f * addSouls));
        }

        internal void RemovePlayer(Player player)
        {
            //add the losing player to the lost collection
            lost.Insert(0, player);

            //remove from the players array
            int removedIndex = players.Length - 1;
            Player[] newPlayers = new Player[players.Length - 1];
            for (int a = 0, b = -1 ; a < newPlayers.Length ; ++a)
                if (players[++b] == player)
                {
                    --a;
                    removedIndex = b;
                }
                else
                {
                    newPlayers[a] = players[b];
                }
            players = newPlayers;

            //remove the dead players portion of the production centers
            for (int a = 0 ; a < 3 ; a++)
            {
                ProductionType type;
                switch (a)
                {
                case 0:
                    type = ProductionType.Knight;
                    break;
                case 1:
                    type = ProductionType.Archer;
                    break;
                case 2:
                    type = ProductionType.Infantry;
                    break;
                default:
                    throw new Exception();
                }

                int count = 0;
                foreach (ProductionCenter pc in production)
                    if (pc.type == type)
                        ++count;
                float avg = count / ( players.Length + 1f );
                int remove = Random.GaussianCappedInt(avg, .13f);
                for (int b = 0 ; b < remove ; b++)
                {
                    ProductionCenter pc = GetRandom(production);
                    if (pc.type == type)
                        production.Remove(pc);
                    else
                        --b;
                }
            }

            //ensure we still have the correct current player
            if (currentPlayer > removedIndex)
                --currentPlayer;
            else if (currentPlayer == removedIndex)
                CheckTurnInc();
        }

        public List<ProductionCenter> GetProduction(int x, int y)
        {
            return GetProduction(x, y, false);
        }

        public List<ProductionCenter> GetProduction(int x, int y, bool unused)
        {
            List<ProductionCenter> result = new List<ProductionCenter>();
            foreach (ProductionCenter pc in production)
                if (pc.x == x && pc.y == y && ( !unused || !pc.used ))
                    result.Add(pc);
            return result;
        }

        internal static T GetRandom<T>(List<T> list)
        {
            return list[Random.Next(list.Count)];
        }

        public void AutoSave()
        {
            TBSUtil.SaveGame(this, "../../../auto", turn + "-" + currentPlayer + ".dae");
        }
        public void SaveGame(string filePath)
        {
            TBSUtil.SaveGame(this, filePath);
        }
        public static Game LoadGame(string filePath)
        {
            return TBSUtil.LoadGame<Game>(filePath);
        }
    }
}
