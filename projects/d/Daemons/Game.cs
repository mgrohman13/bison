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

        private readonly Tile[,] map;
        private readonly List<ProductionCenter> production;

        private int currentPlayer, turn;
        private Player[] players;
        private readonly Player independent;

        private readonly Dictionary<Player, int> won, lost;

        [NonSerialized]
        private int _width, _height;
        [NonSerialized]
        private bool independentsTurn;
        [NonSerialized]
        public string _log;

        public Game(Player[] newPlayers, int newWidth, int newHeight)
        {
            //map
            this._width = newWidth;
            this._height = newHeight;
            this.map = new Tile[width, height];
            for (int x = 0 ; x < width ; x++)
                for (int y = 0 ; y < height ; y++)
                    map[x, y] = new Tile(this, x, y);
            CreateNeighborReferences();

            //production centers
            const float rand = .078f;
            const int min = 1;
            this.production = new List<ProductionCenter>();
            int[] num = new int[3];
            num[0] = Random.GaussianCappedInt(newPlayers.Length / 2f, rand, min);
            num[1] = Random.GaussianCappedInt(newPlayers.Length, rand, min);
            num[2] = Random.GaussianCappedInt(3f * newPlayers.Length / 2f, rand, min);
            for (int a = 0 ; a < num.Length ; a++)
                for (int b = 0 ; b < num[a] ; b++)
                    production.Add(new ProductionCenter(GetRandomTile(), a));

            //players/indy
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

            //convert some indy start units to special
            Dictionary<UnitType, int> startUnits = new Dictionary<UnitType, int>();
            AddIndyStart(startUnits, UnitType.Daemon, .13f);
            AddIndyStart(startUnits, UnitType.Knight, .6f);
            AddIndyStart(startUnits, UnitType.Archer, 1.2f);
            AddIndyStart(startUnits, UnitType.Infantry, 1.8f);
            int count = independent.Units.Count;
            while (startUnits.Count > 0 && count > 0)
            {
                Tile tile = map[Random.Next(width), Random.Next(height)];
                List<Unit> units = tile.GetUnits(independent);
                if (units.Count > 0)
                {
                    Unit unit = units[Random.Next(units.Count)];
                    UnitType unitType = Random.SelectValue(startUnits);
                    startUnits[unitType]--;
                    if (startUnits[unitType] == 0)
                        startUnits.Remove(unitType);
                    new Unit(unitType, unit.Tile, unit.Owner);
                    unit.Tile.Remove(unit);
                    unit.Owner.Remove(unit);
                    --count;
                }
            }

            this.won = new Dictionary<Player, int>();
            this.lost = new Dictionary<Player, int>();
            this.CombatLog = null;

            //player start units
            startUnits.Clear();
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

            independent.ResetMoves();
            foreach (Player player in players)
                player.ResetMoves();
            this.currentPlayer = 0;
        }
        private void AddIndyStart(Dictionary<UnitType, int> startUnits, UnitType unitType, float amt)
        {
            int intAmt = Random.GaussianCappedInt(amt, .52f, 0);
            if (intAmt > 0)
                startUnits.Add(unitType, intAmt);
        }
        internal void CreateNeighborReferences()
        {
            Tile.CreateNeighborReferences(map, width, height);
        }

        private int width
        {
            get
            {
                if (_width == default(int))
                    _width = map.GetLength(0);
                return _width;
            }
        }
        private int height
        {
            get
            {
                if (_height == default(int))
                    _height = map.GetLength(1);
                return _height;
            }
        }

        public string CombatLog
        {
            get
            {
                if (_log == default(string))
                    _log = string.Empty;
                return _log;
            }
            private set
            {
                if (_log == default(string))
                    _log = string.Empty;
                _log = value;
            }
        }
        public string Turn
        {
            get
            {
                return turn.ToString();
            }
        }

        public IEnumerable<KeyValuePair<Player, int>> GetWinners()
        {
            return won.OrderBy((player) => player.Value);
        }
        public IEnumerable<Player> GetLosers()
        {
            return lost.Keys.OrderBy((player) => player.Score).Reverse();
        }
        public IEnumerable<KeyValuePair<Player, int>> GetResult()
        {
            if (players.Length != 1)
                throw new Exception();

            IDictionary<Player, int> results = new Dictionary<Player, int>();

            int points = 0, min = int.MaxValue;
            foreach (Player player in GetLosers().Reverse())
                AddResult(results, player, -39.0 / lost[player], ref points, ref min);
            foreach (var player in GetWinners().Reverse())
                AddResult(results, player.Key, 130.0 / player.Value, ref points, ref min);

            foreach (var pair in results.ToArray())
                results[pair.Key] = pair.Value - min;

            return results.OrderByDescending((p) => p.Value);
        }
        private static void AddResult(IDictionary<Player, int> results, Player player, double add, ref int points, ref int min)
        {
            int newPoints = points + Game.Random.Round(add);
            results.Add(player, newPoints);
            points += 2;
            min = Math.Min(min, newPoints);
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

        internal void Log(String message)
        {
            CombatLog = message + "\r\n" + CombatLog;
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
            ProcessBattles();

            if (players.Length == 1)
            {
                RemovePlayer(players[0], true);
            }
            else
            {
                GetCurrentPlayer().ResetMoves();

                if (Random.Bool(GetWinPct(GetCurrentPlayer())))
                {
                    RemovePlayer(GetCurrentPlayer(), true);
                    if (players.Length == 1)
                        RemovePlayer(players[0], false);
                }
                else
                {
                    currentPlayer++;
                    StartTurn();
                }
            }

            AutoSave();
        }
        public double GetWinPct(Player player)
        {
            double count = 0;
            double total = players.Union(new[] { independent }).OrderByDescending((p) => p.GetStrength())
                    .Aggregate<Player, double>(0, (t, p) => t + ( p == player ? 0 : p.GetStrength() / ++count ));
            double str = player.GetStrength();
            if (str > total * 1.3)
                return Math.Pow(( str - total * 1.3 ) / ( str - total ), 2.6);
            return 0;
        }
        public bool HasWinner()
        {
            return players.Any((p) => ( GetWinPct(p) > 0 ));
        }
        private void StartTurn()
        {
            CheckTurnInc();
            foreach (ProductionCenter pc in production)
                pc.Reset(GetCurrentPlayer());
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

            ChangeMoveOrder();
            ChangeMap();
        }

        private void IndependentsTurn()
        {
            independentsTurn = true;

            ProcessBattles();
            //convert any leftover souls to arrows for archers to fire
            independent.IndyArrows(true);

            //move single-movement units
            Player p;
            Dictionary<Tile, Tile> moved = new Dictionary<Tile, Tile>();
            foreach (Tile from in map)
                if (from.Occupied(out p) && p.Independent)
                {
                    int x = from.X, y = from.Y;
                    MoveRand(ref x, ref y);
                    Tile to = GetTile(x, y);
                    if (MoveIndy(from, to, null))
                        moved.Add(from, to);
                }

            //move special-movement units
            foreach (Tile from in Game.Random.Iterate<Tile>(map.Cast<Tile>()))
            {
                IEnumerable<Unit> units = from.GetUnits(independent, true, true);
                if (units.Any())
                {
                    //archers that stay put shoot if they can
                    IEnumerable<Unit> archers = units.Where((u) => ( u.Type == UnitType.Archer ));
                    if (archers.Any())
                    {
                        foreach (Tile target in Game.Random.Iterate(from.GetSideNeighbors()))
                            Unit.Fire(archers, target);
                        foreach (Tile target in Game.Random.Iterate(from.GetCornerNeighbors()))
                            Unit.Fire(archers, target);
                    }

                    //knights and daemons move separately
                    IEnumerable<Unit> daemons = units.Where((u) => ( u.Type == UnitType.Daemon ));
                    IEnumerable<Unit> knights = units.Where((u) => ( u.Type == UnitType.Knight ));
                    bool anyDaemons = daemons.Any();
                    if (anyDaemons || knights.Any())
                    {
                        //move in 2 steps
                        int x = from.X, y = from.Y;
                        MoveRand(ref x, ref y);
                        int x1 = x, y1 = y;
                        MoveRand(ref x, ref y);
                        Tile to = GetTile(x, y);

                        //check where regular units moved
                        Tile follow;
                        moved.TryGetValue(from, out follow);

                        //make sure they don't just move forward then back, and that if there are any daemons they can make the move
                        if (from == to || ( anyDaemons && !from.IsNeighbor(to) ))
                        {
                            //no actual movement; follow regular units if possible
                            MoveIndy(from, follow, UnitType.Daemon);
                            MoveIndy(from, follow, UnitType.Knight);
                        }
                        else
                        {
                            //first follow regular units unless knights and daemons will move to an already occupied tile
                            if (follow != null && !to.GetUnits(independent).Any((u) => ( !u.Healed
                                    || ( u.Type != UnitType.Daemon && u.Type != UnitType.Knight ) )))
                            {
                                MoveIndy(from, follow, UnitType.Daemon);
                                MoveIndy(from, follow, UnitType.Knight);
                            }
                            else
                            {
                                MoveIndy(from, to, UnitType.Daemon);
                                MoveIndy(from, GetTile(x1, y1), UnitType.Knight);
                                MoveIndy(from, to, UnitType.Knight);
                            }
                        }
                    }
                }
            }

            ProcessBattles();
            //convert arrows to souls to create new units
            independent.IndyArrows(false);

            independent.AddSouls(Random.GaussianOEInt(IndyProd(), .26f, .52 * turn / ( 7.8 + turn )));
            int amt = independent.RoundSouls();
            if (amt > 0)
            {
                independent.AddSouls(-666 * amt);
                Tile tile = GetTile(independent);
                for (int a = 0 ; a < amt ; a++)
                {
                    UnitType type = UnitType.Indy;
                    List<Unit> existing = tile.GetUnits(independent);
                    if (existing.Count > 0)
                        type = existing[Random.Next(existing.Count)].Type;
                    new Unit(type, tile, independent);
                }
            }

            independent.ResetMoves();

            independentsTurn = false;
        }
        public double IndyProd()
        {
            return production.Count * ( 5.2 * turn + 39 );
        }
        private bool MoveIndy(Tile from, Tile to, UnitType? special)
        {
            bool any = false;
            if (from != to && to != null)
                foreach (Unit unit in from.GetUnits(independent))
                    if (unit.Healed && ( special.HasValue ? ( unit.Type == special.Value ) :
                            ( unit.Type != UnitType.Daemon && unit.Type != UnitType.Knight ) ))
                        any |= unit.Move(to);
            return any;
        }

        private void ChangeMap()
        {
            if (Random.Bool())
                for (int a = 0 ; a < production.Count ; a++)
                    if (Random.Bool())
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
            if (players.Length > 1)
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

        internal void RemovePlayer(Player player, bool win)
        {
            if (win)
                won.Add(player, turn);
            else
                lost.Add(player, turn);

            if (this.players.Length > 1)
            {
                player.Won(independent);
                for (int x = 0 ; x < width ; ++x)
                    for (int y = 0 ; y < height ; ++y)
                        foreach (Unit u in map[x, y].GetUnits(player))
                            u.Won(independent);

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
                    float avg = count / ( 1f + players.Length );
                    int remove = Random.GaussianCappedInt(avg, .13f);
                    for (int b = 0 ; b < remove ; b++)
                    {
                        ProductionCenter pc = GetRandom(production);
                        if (pc.type == type)
                            production.Remove(pc);
                        else
                            --b;
                    }

                    foreach (ProductionCenter pc in this.production)
                        pc.Reset(player);
                }

                //ensure we still have the correct current player
                if (currentPlayer > removedIndex)
                    --currentPlayer;
                else if (currentPlayer == removedIndex)
                    StartTurn();
            }
            else
            {
                currentPlayer = 0;
            }
        }

        public List<ProductionCenter> GetProduction(int x, int y)
        {
            return GetProduction(x, y, false);
        }

        public List<ProductionCenter> GetProduction(int x, int y, bool unused)
        {
            List<ProductionCenter> result = new List<ProductionCenter>();
            foreach (ProductionCenter pc in production)
                if (pc.x == x && pc.y == y && ( !unused || !pc.Used ))
                    result.Add(pc);
            return result;
        }

        public double GetProduction(Player p)
        {
            double total = 0;
            foreach (ProductionCenter pc in production)
                if (pc.Owner == p)
                    total += pc.GetValue();
            return total;
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
