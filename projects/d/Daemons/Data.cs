using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;

namespace Daemons
{
    public static class Data
    {

        public static MattUtil.MTRandom Random = new MattUtil.MTRandom();

        static int width, height;
        static Tile[,] map;
        static List<ProductionCenter> production;

        static int currentPlayer, turn;
        static Player[] players;
        static Player independent;
        static bool independentsTurn;

        static List<Player> lost;
        public static string log;

        public static void StartNewGame(Player[] newPlayers, int width, int height)
        {
            Data.width = width;
            Data.height = height;

            map = new Tile[width, height];
            for (int x = 0 ; x < width ; x++)
                for (int y = 0 ; y < height ; y++)
                    map[x, y] = new Tile(x, y);
            Tile.CreateNeighborReferences(map, width, height);

            int[] num = new int[3];
            num[0] = Random.Round((double)newPlayers.Length / 2.0);
            num[1] = newPlayers.Length;
            num[2] = Random.Round(3.0 * (double)newPlayers.Length / 2.0);
            production = new List<ProductionCenter>();
            for (int a = 0 ; a < num.Length ; a++)
                for (int b = 0 ; b < num[a] ; b++)
                    production.Add(new ProductionCenter(Random.Next(width), Random.Next(height), a));

            currentPlayer = -1;
            turn = 6;
            players = new Player[newPlayers.Length];
            independent = new Player(Color.DarkGray, "Independents", true);
            independentsTurn = false;
            int c = -1;
            foreach (Player p in Random.Iterate<Player>(newPlayers))
            {
                players[++c] = new Player(p.Color, p.Name, false);
                IndyStuff();
            }
            turn = 0;

            lost = new List<Player>();
            log = string.Empty;

            int numKnight = 2, numArch = numKnight * 2, numInf = numKnight * 3;
            while (numKnight > 0 || numArch > 0 || numInf > 0)
            {
                int inf = Random.Round(Random.DoubleHalf(numInf) / 6.66);
                int arch = Random.Round(Random.DoubleHalf(numArch) / 6.66);
                int kni = Random.Round(Random.DoubleHalf(numKnight) / 6.66);
                numInf -= inf;
                numArch -= arch;
                numKnight -= kni;
                foreach (Player p in Random.Iterate<Player>(players))
                {
                    addUnits(width, height, p, 0, inf, UnitType.Infantry);
                    addUnits(width, height, p, 0, arch, UnitType.Archer);
                    addUnits(width, height, p, 0, kni, UnitType.Knight);
                }
            }

            Player[] old = (Player[])players.Clone();

            currentPlayer = players.Length - 1;
            EndTurn();

            bool changed = false;
            if (players.Length == old.Length)
                for (int i = 0 ; i < old.Length ; ++i)
                    if (old[i] != players[i])
                    {
                        changed = true;
                        break;
                    }
            if (!changed)
            {
                StartNewGame(newPlayers, width, height);
                return;
            }

            for (int i = 0 ; i < players.Length ; ++i)
            {
                players[i].AddSouls(Random.GaussianCappedInt(i * 100, .13, i * 78));
                players[i].MakeArrow(( i + 1 ) * 3.9);
            }
        }

        public static string Turn
        {
            get
            {
                return turn.ToString();
            }
        }

        public static List<Player> GetLost()
        {
            return lost;
        }

        public static Player[] GetPlayers()
        {
            return players;
        }

        public static Player GetIndy()
        {
            return independent;
        }

        public static int getWidth()
        {
            return width;
        }

        public static int getHeight()
        {
            return height;
        }

        public static Tile GetTile(int x, int y)
        {
            return map[x, y];
        }

        public static Player GetCurrentPlayer()
        {
            if (independentsTurn)
                return independent;
            return players[currentPlayer];
        }

        public static void Log(String message)
        {
            log = message + "\r\n" + log;
        }

        private static void addUnits(int width, int height, Player player, int count, int max, UnitType type)
        {
            while (count < max)
            {
                Tile t = map[Random.Next(width), Random.Next(height)];
                Player occP;
                if (!t.Occupied(out occP) || occP == player)
                {
                    ++count;
                    new Unit(type, t, player);
                }
            }
        }

        public static void EndTurn()
        {
            if (players.Length == 0)
                return;

            ProcessAllBattles();

            currentPlayer++;
            checkIncStuff();

            if (players.Length == 1)
            {
                currentPlayer = 0;
                Player lastPlayer = players[currentPlayer];
                RemovePlayer(lastPlayer);
                players = new Player[] { lastPlayer };
                return;
            }
        }

        private static void ProcessAllBattles()
        {
            bool any = true;
            while (any)
            {
                any = false;
                foreach (Tile tile in Random.Iterate<Tile>(map.Cast<Tile>()))
                    if (tile.FightBattle())
                        any = true;
            }
        }

        private static void checkIncStuff()
        {
            if (currentPlayer >= players.Length)
            {
                currentPlayer = -1;
                incStuff();
                currentPlayer = 0;
            }
        }

        private static void incStuff()
        {
            IndyStuff();

            turn++;

            foreach (Player p in players)
                p.ResetMoves();
            independent.ResetMoves();

            foreach (ProductionCenter pc in production)
                pc.used = false;

            ChangeOrder();
            changeMap();
        }

        private static void IndyStuff()
        {
            independentsTurn = true;

            foreach (Tile t in map)
            {
                int x = t.X, y = t.Y;
                randPoint(ref x, ref y);
                Tile moveTo = GetTile(x, y);
                foreach (Unit u in t.GetUnits(independent))
                    if (u.Healed && u.Movement > 0)
                        u.Move(moveTo);
            }

            double mult = production.Count;
            independent.AddSouls(Random.Gaussian(2.6 * mult * ( turn + 16.9 ), .13));
            independent.AddSouls(Random.OE(3.0 * mult * turn));

            Tile tile;
            Player occupied;
            do
            {
                tile = GetTile(Random.Next(width), Random.Next(height));
            } while (tile.Occupied(out occupied) && !occupied.Independent);
            int amt = Random.Round(independent.Souls / 666.0);
            independent.AddSouls(-666 * amt);
            for (int a = 0 ; a < amt ; a++)
                new Unit(UnitType.Indy, tile, independent);

            ProcessAllBattles();

            independentsTurn = false;
        }

        private static void changeMap()
        {
            for (int a = 0 ; a < production.Count ; a++)
                if (Random.Bool(.3))
                    randPoint(ref production[a].x, ref production[a].y);
        }

        public static void randPoint(ref int x, ref int y)
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
                randPoint(ref x, ref y);
        }

        private static void ChangeOrder()
        {
            int num = players.Length + lost.Count;
            int index = Random.Next(num);

            if (index == 0)
            {
                players[0].AddSouls(Random.GaussianCapped(100, .13, 78));
                players[0].MakeArrow(3.9);
            }
            else if (index < players.Length)
            {
                Player temp = players[index - 1];
                players[index - 1] = players[index];
                players[index] = temp;
            }
        }

        internal static void RemovePlayer(Player player)
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

            //remove the players portion of the production centers
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

                List<ProductionCenter> pcs = new List<ProductionCenter>();
                foreach (ProductionCenter pc in production)
                    if (pc.type == type)
                        pcs.Add(pc);
                double avg = pcs.Count / ( players.Length + 1.0 );
                int remove = Data.Random.GaussianCappedInt(avg, .13);
                for (int b = 0 ; b < remove ; b++)
                {
                    if (pcs.Count > 0)
                        production.Remove(pcs[Random.Next(pcs.Count)]);
                }
            }

            //ensure we still have the correct current player
            if (currentPlayer > removedIndex)
                --currentPlayer;
            else if (currentPlayer == removedIndex)
                EndTurn();
        }

        public static List<ProductionCenter> GetProduction(int x, int y, bool unused)
        {
            List<ProductionCenter> result = new List<ProductionCenter>();
            foreach (ProductionCenter pc in production)
                if (pc.x == x && pc.y == y && ( !unused || !pc.used ))
                    result.Add(pc);
            return result;
        }

        public static List<ProductionCenter> GetProduction(int x, int y)
        {
            return GetProduction(x, y, false);
        }
    }
}