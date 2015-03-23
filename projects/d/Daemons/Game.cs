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
        public static readonly MattUtil.MTRandom Random;
        static Game()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();
        }

        private readonly Tile[,] map;
        private readonly List<ProductionCenter> production;

        private List<Player> players;
        private readonly Player independent;

        private readonly Dictionary<Player, int> won, lost;

        private int currentPlayer, turn;

        [NonSerialized]
        private bool independentsTurn;
        [NonSerialized]
        public string _log;

        public Game(Player[] newPlayers, int newWidth, int newHeight)
        {
            //map
            this.map = new Tile[newWidth, newHeight];
            for (int x = 0 ; x < newWidth ; x++)
                for (int y = 0 ; y < newHeight ; y++)
                    this.map[x, y] = new Tile(this, x, y);
            CreateNeighborReferences();

            //production centers
            const int min = 1;
            this.production = new List<ProductionCenter>();
            int[] num = new[] { 
                Random.GaussianCappedInt(newPlayers.Length / 2.0, Consts.ProdRand , min),
                Random.GaussianCappedInt(newPlayers.Length, Consts.ProdRand, min),
                Random.GaussianCappedInt(newPlayers.Length * 3 / 2.0, Consts.ProdRand, min)
            };

            for (int a = 0 ; a < num.Length ; a++)
                for (int b = 0 ; b < num[a] ; b++)
                    this.production.Add(new ProductionCenter(GetRandomTile(), a));

            //players/indy
            this.currentPlayer = -1;
            this.players = newPlayers.Select<Player, Player>(player => null).ToList();
            this.independent = new Player(Color.DarkGray, "Independents", this, true);
            int index = -1;
            int lastSouls = 0;
            double addSouls, addArrows;
            GetMoveDiff(out addSouls, out addArrows);
            this.turn = 6;
            foreach (Player player in Random.Iterate<Player>(newPlayers))
            {
                int souls = lastSouls + RandSouls(addSouls);
                lastSouls = souls;
                this.players[++index] = new Player(player.Color, player.Name, this, false, souls);
                for (int b = -1 ; b < index ; ++b)
                    this.players[index].MakeArrow(addArrows);
                IndependentsTurn();
            }
            this.turn = 1;

            //convert some indy start units to special
            Dictionary<UnitType, int> startUnits = new Dictionary<UnitType, int>();
            AddIndyStart(startUnits, UnitType.Daemon, .169);
            AddIndyStart(startUnits, UnitType.Knight, .6);
            AddIndyStart(startUnits, UnitType.Archer, 1.2);
            AddIndyStart(startUnits, UnitType.Infantry, 1.8);
            while (startUnits.Count > 0 && this.independent.GetUnits().Any(unit => unit.Type == UnitType.Indy))
            {
                Tile tile = GetRandomTile();
                IEnumerable<Unit> units = tile.GetUnits(this.independent);
                if (units.Any())
                {
                    Unit unit = Random.SelectValue(units);
                    UnitType unitType = Random.SelectValue(startUnits);
                    if (--startUnits[unitType] == 0)
                        startUnits.Remove(unitType);
                    new Unit(unitType, unit.Tile, unit.Owner);
                    unit.Tile.Remove(unit);
                    unit.Owner.Remove(unit);
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
                ++startUnits[Random.SelectValue<UnitType>(startUnits)];
            for (int a = 0 ; a < 13 ; ++a)
            {
                UnitType unitType = Random.SelectValue<UnitType>(startUnits);
                --startUnits[unitType];
                foreach (Player player in Random.Iterate<Player>(this.players))
                    new Unit(unitType, GetTile(player), player);
            }

            this.independent.ResetMoves();
            foreach (Player player in this.players)
                player.ResetMoves();
            this.currentPlayer = 0;
        }
        private void AddIndyStart(Dictionary<UnitType, int> startUnits, UnitType unitType, double amt)
        {
            int intAmt = Random.GaussianCappedInt(amt, Consts.IndyRand);
            if (intAmt > 0)
                startUnits.Add(unitType, intAmt);
        }
        internal void CreateNeighborReferences()
        {
            Tile.CreateNeighborReferences(this.map, Width, Height);
        }

        public int Width
        {
            get
            {
                return this.map.GetLength(0);
            }
        }
        public int Height
        {
            get
            {
                return this.map.GetLength(1);
            }
        }

        public string CombatLog
        {
            get
            {
                string log = this._log;
                if (log == default(string))
                    log = string.Empty;
                return log;
            }
            private set
            {
                this._log = value;
            }
        }
        public string Turn
        {
            get
            {
                return this.turn.ToString();
            }
        }

        public IEnumerable<KeyValuePair<Player, int>> GetWinners()
        {
            return this.won.OrderBy(player => player.Value);
        }
        public IEnumerable<Player> GetLosers()
        {
            return this.lost.Keys.OrderByDescending(player => player.Score);
        }
        public IEnumerable<KeyValuePair<Player, int>> GetResult()
        {
            if (this.players.Count != 1)
                throw new Exception();

            IDictionary<Player, int> results = new Dictionary<Player, int>();

            int points = 0, min = int.MaxValue;
            foreach (Player player in GetLosers().Reverse())
                AddResult(results, player, -13.0 / this.lost[player], ref points, ref min);
            foreach (var player in GetWinners().Reverse())
                AddResult(results, player.Key, Consts.WinPoints / player.Value, ref points, ref min);

            return results.Select(pair => new KeyValuePair<Player, int>(pair.Key, pair.Value - min)).OrderByDescending(player => player.Value);
        }
        private static void AddResult(IDictionary<Player, int> results, Player player, double add, ref int points, ref int min)
        {
            int newPoints = points + Random.Round(add);
            results.Add(player, newPoints);
            points += 1;
            min = Math.Min(min, newPoints);
        }

        public IList<Player> GetPlayers()
        {
            return this.players.AsReadOnly();
        }

        public Player GetIndependent()
        {
            return this.independent;
        }

        public Tile GetTile(int x, int y)
        {
            return this.map[x, y];
        }

        public Player GetCurrentPlayer()
        {
            if (this.independentsTurn)
                return this.independent;
            return this.players[this.currentPlayer];
        }

        internal void Log(String message)
        {
            CombatLog = message + "\r\n" + CombatLog;
        }

        private Tile GetTile(Player player)
        {
            Tile tile;
            do
            {
                tile = GetRandomTile();
            } while (!tile.Unoccupied(player));
            return tile;
        }

        internal Tile GetRandomTile()
        {
            return GetTile(Random.Next(Width), Random.Next(Height));
        }

        public void EndTurn()
        {
            ProcessBattles();

            if (this.players.Count == 1)
            {
                RemovePlayer(this.players[0], true);
            }
            else
            {
                GetCurrentPlayer().ResetMoves();

                if (Random.Bool(GetWinPct(GetCurrentPlayer())))
                {
                    RemovePlayer(GetCurrentPlayer(), true);
                    if (this.players.Count == 1)
                        RemovePlayer(this.players[0], false);
                }
                else
                {
                    this.currentPlayer++;
                    StartTurn();
                }
            }

            AutoSave();
        }
        public double GetWinPct(Player curPlayer)
        {
            IEnumerable<double> strengths = this.players.Where(player => player != curPlayer)
                    .Concat(new[] { this.independent }).Select(player => player.GetStrength()).OrderByDescending(s => s);
            double total = 0, count = 0;
            foreach (double strength in strengths)
                total += ( strength / ++count );
            double str = curPlayer.GetStrength();
            if (str > total * 1.3)
                return Math.Pow(( str - total * 1.3 ) / ( str - total ), 2.6);
            return 0;
        }
        public bool HasWinner()
        {
            return this.players.Any(player => GetWinPct(player) > 0);
        }
        private void StartTurn()
        {
            CheckTurnInc();
            foreach (ProductionCenter pc in this.production)
                pc.Reset(GetCurrentPlayer());

            while (GetCurrentPlayer().Souls < 0)
            {
                int add = -players.Min(p => p.Souls);
                foreach (Player p in players)
                    p.AddSouls(add);
            }
            while (GetCurrentPlayer().Arrows < 0)
            {
                double add = -players.Min(p => p.Arrows);
                foreach (Player p in players)
                    p.MakeArrow(add);
            }
        }

        private void ProcessBattles()
        {
            bool any = true;
            while (any)
            {
                any = false;
                foreach (Tile tile in Random.Iterate<Tile>(this.map.Cast<Tile>()))
                    any |= tile.FightBattle();
            }
        }

        private void CheckTurnInc()
        {
            if (this.currentPlayer >= this.players.Count)
            {
                this.currentPlayer = -1;
                IncTurn();
                this.currentPlayer = 0;
            }
        }

        private void IncTurn()
        {
            IndependentsTurn();

            this.turn++;

            ChangeMoveOrder();
            ChangeMap();
        }

        private void IndependentsTurn()
        {
            this.independentsTurn = true;

            ProcessBattles();
            //convert any leftover souls to arrows for archers to fire
            this.independent.IndyArrows(true);

            //move single-movement units
            Dictionary<Tile, Tile> moved = new Dictionary<Tile, Tile>();
            foreach (Tile from in this.map)
            {
                int x = from.X, y = from.Y;
                MoveRand(ref x, ref y);
                Tile to = GetTile(x, y);
                if (MoveIndy(from, to, null))
                    moved.Add(from, to);
            }

            //move special-movement units
            foreach (Tile from in Random.Iterate<Tile>(this.map.Cast<Tile>()))
            {
                IEnumerable<Unit> units = from.GetUnits(this.independent, true, true);
                if (units.Any())
                {
                    //archers that stay put shoot if they can
                    IEnumerable<Unit> archers = units.Where(unit => unit.Type == UnitType.Archer);
                    if (archers.Any())
                    {
                        foreach (Tile target in Random.Iterate(from.GetSideNeighbors()))
                            Unit.Fire(archers, target);
                        foreach (Tile target in Random.Iterate(from.GetCornerNeighbors()))
                            Unit.Fire(archers, target);
                    }

                    //knights and daemons move separately
                    IEnumerable<Unit> daemons = units.Where(unit => unit.Type == UnitType.Daemon);
                    IEnumerable<Unit> knights = units.Where(unit => unit.Type == UnitType.Knight);
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
                        //first follow regular units unless knights and daemons will move to an already occupied tile
                        else if (follow != null && !to.GetUnits(this.independent).Any(unit => ( !unit.Healed
                                || ( unit.Type != UnitType.Daemon && unit.Type != UnitType.Knight ) )))
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

            ProcessBattles();
            //convert arrows to souls to create new units
            this.independent.IndyArrows(false);

            this.independent.AddSouls(Random.GaussianOEInt(IndyProd(), Consts.IndyRand, .52 * this.turn / ( 7.8 + this.turn )));
            int amt = this.independent.RoundSouls();
            if (amt > 0)
            {
                this.independent.AddSouls(-amt * Consts.DaemonSouls);
                Tile tile = GetTile(this.independent);
                for (int a = 0 ; a < amt ; a++)
                {
                    UnitType type = UnitType.Indy;
                    IEnumerable<Unit> existing = tile.GetUnits(this.independent);
                    if (existing.Any())
                        type = Random.SelectValue(existing).Type;
                    new Unit(type, tile, this.independent);
                }
            }

            this.independent.ResetMoves();

            this.independentsTurn = false;
        }
        public double IndyProd()
        {
            double prod = ( ( this.turn * 5.2 + 39 ) * this.production.Count );
            if (this.players.All(player => player != null))
            {
                double playerStr = this.players.Sum(player => player.GetStrength());
                prod *= Math.Sqrt(playerStr / ( this.independent.GetStrength() + 65 ) * .26);
            }
            return prod;
        }
        private bool MoveIndy(Tile from, Tile to, UnitType? special)
        {
            bool any = false;
            if (from != to && to != null)
                foreach (Unit unit in from.GetUnits(this.independent).Where(unit => unit.Healed &&
                        ( special.HasValue ? ( unit.Type == special.Value ) : ( unit.Type != UnitType.Daemon && unit.Type != UnitType.Knight ) )).ToList())
                    any |= unit.Move(to);
            return any;
        }

        private void ChangeMap()
        {
            if (Random.Bool())
                for (int a = 0 ; a < this.production.Count ; a++)
                    if (Random.Bool())
                    {
                        int x = this.production[a].X;
                        int y = this.production[a].Y;
                        MoveRand(ref x, ref y);
                        this.production[a].Move(x, y);
                    }
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
            else if (x >= Width)
                x = Width - 1;
            else if (y < 0)
                y = 0;
            else if (y >= Height)
                y = Height - 1;
            else
                retry = false;
            if (retry)
                MoveRand(ref x, ref y);
        }

        private void ChangeMoveOrder()
        {
            if (this.players.Count > 1)
                foreach (KeyValuePair<Player, int> pair in MattUtil.TBSUtil.RandMoveOrder<Player>(Random, this.players, .26))
                {
                    double souls, arrows;
                    GetMoveDiff(out souls, out arrows);
                    souls *= pair.Value;
                    arrows *= pair.Value;
                    pair.Key.AddSouls(souls);
                    pair.Key.MakeArrow(arrows);
                }
        }

        private void GetMoveDiff(out double souls, out double arrows)
        {
            double div = this.players.Count - 1;
            souls = Consts.DaemonSouls / div;
            arrows = 16.9 / div;
        }

        private static int RandSouls(double addSouls)
        {
            return Random.GaussianCappedInt(addSouls, Consts.SoulRand, Random.Round(.78 * addSouls));
        }

        internal void RemovePlayer(Player player, bool win)
        {
            if (win)
                this.won.Add(player, this.turn);
            else
                this.lost.Add(player, this.turn);

            if (this.players.Count > 1)
            {
                player.Won(independent);
                foreach (Unit u in player.GetUnits())
                    u.Won(independent);

                //remove from the players array
                int removedIndex = this.players.IndexOf(player);
                this.players.RemoveAt(removedIndex);

                //remove the dead players portion of the production centers
                foreach (ProductionType type in new[] { ProductionType.Infantry, ProductionType.Archer, ProductionType.Knight })
                {
                    int remove = Random.GaussianCappedInt(this.production.Count(prod => prod.Type == type) / ( 1.0 + this.players.Count ), Consts.ProdRand);
                    for (int b = 0 ; b < remove ; b++)
                        this.production.Remove(Random.SelectValue(this.production.Where(prod => prod.Type == type)));
                }
                //reset owned centers
                foreach (ProductionCenter pc in this.production)
                    pc.Reset(player);

                //ensure we still have the correct current player
                if (this.currentPlayer > removedIndex)
                    --this.currentPlayer;
                else if (this.currentPlayer == removedIndex)
                    StartTurn();
            }
            else
            {
                this.currentPlayer = 0;
            }
        }

        public IEnumerable<ProductionCenter> GetProduction(int x, int y, bool unused = false)
        {
            return this.production.Where(prod => ( prod.X == x && prod.Y == y && ( !unused || !prod.Used ) ));
        }

        public double GetProduction(Player p)
        {
            return this.production.Where(prod => prod.Owner == p).Sum(prod => prod.GetValue());
        }

        public void AutoSave()
        {
            TBSUtil.SaveGame(this, "../../../auto", this.turn + "-" + this.currentPlayer + ".dae");
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
