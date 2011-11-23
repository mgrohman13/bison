using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using MattUtil;

namespace CityWar
{
    [Serializable]
    public class Game
    {
        #region fields
        public static MattUtil.MTRandom Random;
        static Game()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();

            InitRaces();
        }

        public static string Path = "..\\..\\..\\";
        public static Dictionary<string, string[]> Races;

        private int turn, width, height, currentPlayer, startWork;
        private Player[] players, defeatedPlayers;
        private Dictionary<Player, int> winningPlayers;
        private Tile[,] map;
        private Dictionary<string, int> unitsHave;

        #endregion //fields

        #region public commands
        public void AutoSave()
        {
            TBSUtil.SaveGame(this, Path + "Saves/auto", turn + "-" + currentPlayer + ".cws");
        }
        public void SaveGame(string filePath)
        {
            TBSUtil.SaveGame(this, filePath);
        }
        public static Game LoadGame(string filePath)
        {
            return TBSUtil.LoadGame<Game>(filePath);
        }

        public static Game StartNewGame(Player[] newPlayers, int width, int height)
        {
            Game game = new Game();

            int numPlayers = newPlayers.Length;

            //ensure the map is big enough for all the players
            const int HexesPerPlayer = 13;
            if (numPlayers * HexesPerPlayer > width * height)
                throw new ArgumentOutOfRangeException(string.Format(
                    "Map is too small for that many players.  Must be at least {0} hexes.",
                    numPlayers * HexesPerPlayer), new Exception());

            //initialize variables
            ClearUndos();
            game.turn = 1;
            game.Width = width;
            game.Height = height;
            game.defeatedPlayers = new Player[0];
            game.winningPlayers = new Dictionary<Player, int>();

            game.CreateMap(width, height);

            UnitSchema us;
            int numUnits;
            InitRaces(out us, out numUnits, out game.unitsHave);

            //pick 3 random starting units

            Dictionary<string, string>[] startUnits = new Dictionary<string, string>[3];

            for (int a = -1 ; ++a < 3 ; )
            {
                UnitSchema.UnitRow row = ( (UnitSchema.UnitRow)us.Unit.Rows[Random.Next(numUnits)] );
                //this has to come after unitsHave is set to cost but before actualy initialized
                startUnits[a] = game.GetForRaces(row.Name);
            }
            double totalStartCost = 0;
            foreach (Player player in newPlayers)
                foreach (Dictionary<string, string> dict in startUnits)
                    totalStartCost += Unit.CreateTempUnit(dict[player.Race]).BaseCost;
            totalStartCost /= newPlayers.Length;

            foreach (string[] race in Races.Values)
                foreach (string name in race)
                    if (game.unitsHave[name] > 0)
                        game.unitsHave[name] = game.GetInitUnitsHave(game.unitsHave[name]);

            //initialize the players, half with cities and half with wizards
            bool city = Random.Bool();
            foreach (Player current in Random.Iterate<Player>(newPlayers))
            {
                string[] raceUnits = new string[3];
                for (int a = -1 ; ++a < 3 ; )
                    raceUnits[a] = startUnits[a][current.Race];
                current.NewPlayer(game, city = !city, raceUnits, totalStartCost);
            }

            //randomize the turn order
            game.currentPlayer = -1;
            game.players = new Player[numPlayers];
            foreach (Player current in Random.Iterate<Player>(newPlayers))
            {
                game.players[++game.currentPlayer] = current;
                //players moving later in the turn order receive compensation
                game.AddMoveOrderDiff(current, game.currentPlayer);
                current.EndTurn();
            }

            //create wizard points and possibly some starting city spots
            int wizspots = 1 + Random.GaussianCappedInt(width * height / 66.6f, .09f);
            for (int a = -1 ; ++a < wizspots ; )
                game.CreateWizardPts();
            for (int a = -1 ; ++a < numPlayers ; )
                game.CreateCitySpot();

            //	Start the game!
            Player.SubtractCommonUpkeep(game.players);
            game.currentPlayer = 0;
            game.startWork = game.players[0].StartTurn();

            return game;
        }
        private int GetInitUnitsHave(int needed)
        {
            //~1/28 will be outside [-needed,needed]
            return Random.GaussianInt(needed / 2.1f);
        }
        private static void InitRaces()
        {
            UnitSchema us;
            int numUnits;
            Dictionary<string, int> unitsHave;
            InitRaces(out us, out numUnits, out unitsHave);
        }
        private static void InitRaces(out UnitSchema us, out int numUnits, out Dictionary<string, int> unitsHave)
        {
            //initialize units
            us = UnitTypes.GetSchema();
            numUnits = us.Unit.Rows.Count;
            Dictionary<string, List<string>> tempRaces = new Dictionary<string, List<string>>();
            unitsHave = new Dictionary<string, int>(numUnits);
            for (int a = -1 ; ++a < numUnits ; )
            {
                UnitSchema.UnitRow row = ( (UnitSchema.UnitRow)us.Unit.Rows[a] );
                string race = row.Race;
                string name = row.Name;
                if (!tempRaces.ContainsKey(race))
                    tempRaces.Add(race, new List<string>());
                tempRaces[race].Add(name);
                unitsHave.Add(name, row.Cost + row.People);
            }

            Races = new Dictionary<string, string[]>();
            foreach (string key in tempRaces.Keys)
                Races.Add(key, tempRaces[key].ToArray());
        }

        private delegate Piece UndoDelegate(object[] args);
        private static Stack<UndoDelegate> UndoCommands = new Stack<UndoDelegate>();
        private static Stack<object[]> UndoArgs = new Stack<object[]>();
        private static Dictionary<Unit, List<Tile>> UnitTiles = new Dictionary<Unit, List<Tile>>();

        private static void ClearUndos()
        {
            UndoCommands.Clear();
            UndoArgs.Clear();
            UnitTiles.Clear();
        }

        public static bool CanUndoCommand()
        {
            return ( UndoCommands.Count > 0 );
        }
        public static Tile UndoCommand()
        {
            //undo the command
            Piece piece = UndoCommands.Pop()(UndoArgs.Pop());

            //tell the caller which tile was relevant to the undo, if any
            if (piece == null)
                return null;
            Tile retVal = piece.Tile;
            retVal.CurrentGroup = piece.Group;
            return retVal;
        }

        public void EndTurn()
        {
            EndTurn(false);
            AutoSave();
        }
        private void EndTurn(bool currentDead)
        {
            if (players.Length > 0)
            {
                //cant undo anything after ending turn
                ClearUndos();
                if (!currentDead)
                {
                    Player player = players[currentPlayer];
                    player.EndTurn();
                    //might lose the game during EndTurn
                    if (player == players[currentPlayer])
                        ++currentPlayer;
                }

                if (players.Length > 0)
                {
                    if (currentPlayer >= players.Length)
                    {
                        //a new round of turns
                        IncrementTurn();
                        currentPlayer = 0;
                    }

                    if (players.Length > 0)
                    {
                        //keep track of the amount of work the player started his turn with to determine when healing units can be undone
                        startWork = players[currentPlayer].StartTurn();
                    }
                }
            }
        }

        public int AttackUnit(Battle b, Attack attack, Unit target)
        {
            int retVal = attack.AttackUnit(target);

            if (retVal > -1)
            {
                if (target.Dead)
                    b.defenders.Remove(target);

                if (attack.Owner.Owner == CurrentPlayer)
                {
                    AddUnitTile(attack.Owner, attack.Owner.Tile);

                    List<Piece> removePieces = new List<Piece>();
                    removePieces.Add(attack.Owner);
                    foreach (Piece p in attack.Owner.Tile.GetAllPieces())
                        if (p is Wizard)
                            removePieces.Add(p);
                    RemoveUndosForPieces(removePieces);
                }
            }

            return retVal;
        }

        public Battle StartBattle(Tile attacker, Tile defender)
        {
            Player player;
            attacker.OccupiedByUnit(out player);
            if (player != players[currentPlayer])
                return null;
            return Unit.StartBattle(attacker.GetSelectedUnits(), defender);
        }

        public bool EndBattle(Battle b)
        {
            if (b.canRetalliate)
            {
                b.StartRetalliation();

                //consider the battle over if no one can retalliate
                if (b.attackers.Count == 0)
                {
                    Unit.EndBattle(b);
                    return false;
                }

                return true;
            }

            Unit.EndBattle(b);
            return false;
        }

        public void ChangeTerrain(Wizard wizard, Terrain terrain)
        {
            int movement = wizard.Movement;
            if (movement > 0)
            {
                if (wizard.Owner != players[currentPlayer])
                    return;

                Terrain oldTerrain = wizard.Tile.Terrain;
                if (oldTerrain == terrain)
                {
                    //trying to change to the same terrain counts as a heal
                    HealPieces(new Piece[] { wizard });
                }
                else
                {
                    wizard.ChangeTerrain(terrain);
                    UndoCommands.Push(UndoChangeTerrain);
                    UndoArgs.Push(new object[] { wizard, movement, oldTerrain });
                }
            }
        }
        private static Piece UndoChangeTerrain(object[] args)
        {
            Wizard wizard = (Wizard)args[0];
            int oldMove = (int)args[1];
            Terrain oldTerrain = (Terrain)args[2];

            if (oldMove > -1)
            {
                if (oldTerrain == wizard.Tile.Terrain)
                    throw new Exception();
                else
                    wizard.UndoChangeTerrain(oldMove, oldTerrain);

                return wizard;
            }
            else
            {
                return null;
            }
        }

        public void BuildPiece(Capturable capt, string pieceName)
        {
            if (capt.Owner != players[currentPlayer])
                return;

            bool canUndo;
            Piece piece = capt.BuildPiece(pieceName, out canUndo);
            if (piece != null)
                if (canUndo)
                {
                    UndoCommands.Push(UndoBuildPiece);
                    UndoArgs.Push(new object[] { capt, piece });
                }
                else
                {
                    RemoveUndosForPiece(capt);
                }
        }
        private Piece UndoBuildPiece(object[] args)
        {
            Capturable capt = (Capturable)args[0];
            Piece piece = (Piece)args[1];

            capt.UndoBuildPiece(piece);

            return capt;
        }

        public bool MovePieces(Tile from, int x, int y, bool group)
        {
            Player player;
            from.Occupied(out player);
            if (player != players[currentPlayer])
                return false;

            if (group)
            {
                return MovePiecesHelper(from, x, y, true);
            }
            else
            {
                Piece[] pieces = from.FindAllPieces(delegate(Piece p)
                {
                    return p.Group == from.CurrentGroup && p.Movement > 0;
                });
                bool any = false;
                //call the helper once for each piece as they will be moved individually
                for (int amount = pieces.Length ; --amount >= 0 ; )
                    if (MovePiecesHelper(from, x, y, false))
                        any = true;

                if (any)
                    RegroupMoved(from, x, y, pieces);

                return any;
            }
        }
        private bool MovePiecesHelper(Tile from, int x, int y, bool group)
        {
            Player player;
            from.Occupied(out player);
            if (player != players[currentPlayer])
                return false;

            Tile to = map[x, y];

            Dictionary<Piece, int> oldMoves = new Dictionary<Piece, int>();
            foreach (Piece p in from.GetSelectedPieces())
                oldMoves.Add(p, p.Movement);
            //passing a group of false will just move a single random unit
            Dictionary<Piece, bool> undoPieces = from.MovePieces(to, group);

            bool any = false;
            if (undoPieces != null)
            {
                foreach (Piece p in undoPieces.Keys)
                {
                    Unit u = p as Unit;
                    if (u != null && u.Type != UnitType.Air)
                        AddUnitTile(u, to);
                    if (from != p.Tile)
                        any = true;
                }

                //non group move regrouping is done higher up in MovePieces
                if (any && group)
                    RegroupMoved(from, x, y, undoPieces.Keys);

                foreach (Piece p in undoPieces.Keys)
                {
                    if (undoPieces[p])
                    {
                        if (from != p.Tile)
                        {
                            UndoCommands.Push(UndoMovePieces);
                            UndoArgs.Push(new object[] { from, p, oldMoves[p], p.Movement });
                        }
                    }
                    else if (oldMoves[p] != p.Movement)
                    {
                        RemoveUndosForPiece(p);
                    }
                }
            }

            return any;
        }
        private void RegroupMoved(Tile from, int x, int y, IEnumerable<Piece> pieces)
        {
            int newGroup = NewGroup();
            foreach (Piece piece in pieces)
                if (piece.Tile != from)
                    piece.Group = newGroup;
            map[x, y].CurrentGroup = newGroup;
        }
        private Piece UndoMovePieces(object[] args)
        {
            return UndoMovePieceSetGroup(args, NewGroup());
        }
        private Piece UndoMovePieceSetGroup(object[] args, int group)
        {
            Tile from = (Tile)args[0];
            Piece p = (Piece)args[1];
            int oldMove = (int)args[2];
            int move = (int)args[3];

            if (move != p.Movement)
                throw new Exception();

            Tile movedTile = p.Tile;
            p.UndoMove(from, oldMove);
            p.Group = group;

            //undo all pieces that moved from the same tile to the same tile at once
            if (UndoCommands.Count > 0 && UndoCommands.Peek() == UndoMovePieces
                && UndoArgs.Peek()[0] == from && ( (Piece)UndoArgs.Peek()[1] ).Tile == movedTile)
            {
                UndoCommands.Pop();
                return UndoMovePieceSetGroup(UndoArgs.Pop(), group);
            }
            return p;
        }

        public void HealPieces(Piece[] selPieces)
        {
            int length = selPieces.Length;
            if (length < 1)
                return;
            for (int i = 0 ; i < length ; ++i)
                if (selPieces[i].Owner != players[currentPlayer])
                    return;

            bool any = false;
            Dictionary<Piece, double> undoInfo = new Dictionary<Piece, double>();
            for (int i = 0 ; i < length ; ++i)
            {
                Piece curPiece = selPieces[i];
                double info = curPiece.Heal();
                //weed out futile calls
                if (info > -1)
                {
                    if (!any)
                        any = true;
                    undoInfo.Add(curPiece, info);
                }
            }

            if (any)
            {
                UndoCommands.Push(UndoHealPieces);
                UndoArgs.Push(new object[] { undoInfo });
            }
        }
        private Piece UndoHealPieces(object[] args)
        {
            Dictionary<Piece, double> undoInfo = (Dictionary<Piece, double>)args[0];

            bool wizCheck = false;
            List<Piece> dead = new List<Piece>();

            Piece piece = null;
            foreach (Piece p in undoInfo.Keys)
            {
                p.UndoHeal(undoInfo[p]);

                Unit u;
                if (( u = p as Unit ) != null && u.Dead)
                {
                    dead.Add(p);
                }
                else
                {
                    if (p is Wizard)
                        wizCheck = true;
                    piece = p;
                }
            }

            if (dead.Count > 0)
            {
                RemoveUndosForPieces(dead);
                foreach (Piece p in dead)
                    undoInfo.Remove(p);
            }

            if (undoInfo.Count > 0)
            {
                if (wizCheck)
                {
                    int element;
                    switch (piece.Tile.Terrain)
                    {
                    case Terrain.Forest:
                        element = piece.Owner.Nature;
                        break;
                    case Terrain.Mountain:
                        element = piece.Owner.Earth;
                        break;
                    case Terrain.Plains:
                        element = piece.Owner.Air;
                        break;
                    case Terrain.Water:
                        element = piece.Owner.Water;
                        break;
                    default:
                        throw new Exception();
                    }
                    wizCheck = ( element < 0 );
                }

                if (wizCheck || piece.Owner.Work < startWork)
                {
                    Piece[] units = new Piece[undoInfo.Keys.Count];
                    undoInfo.Keys.CopyTo(units, 0);
                    int stack = UndoCommands.Count;
                    HealPieces(units);
                    RemoveUndos(stack);
                }
            }
            return piece;
        }

        public void ConvertCity(Piece p)
        {
            if (players[currentPlayer] != p.Owner)
                return;

            if (p.GetCity())
            {
                UndoCommands.Push(UndoConvertCity);
                UndoArgs.Push(new object[] { p });
            }
        }
        private Piece UndoConvertCity(object[] args)
        {
            Piece piece = (Piece)args[0];

            piece.UndoGetCity();

            if (piece.Owner.Population < 0)
            {
                int stack = UndoCommands.Count;
                ConvertCity(piece);
                RemoveUndos(stack);
            }
            return piece;
        }

        public void DisbandUnits(Unit[] units)
        {
            if (units.Length < 1)
                return;

            foreach (Unit unit in units)
                if (unit.Owner != players[currentPlayer])
                    return;

            Dictionary<Unit, Stack<double>> undoInfo = new Dictionary<Unit, Stack<double>>();
            foreach (Unit unit in units)
                undoInfo.Add(unit, unit.Disband());

            UndoCommands.Push(UndoDisbandUnits);
            UndoArgs.Push(new object[] { undoInfo });
        }
        private Piece UndoDisbandUnits(object[] args)
        {
            Dictionary<Unit, Stack<double>> undoInfo = (Dictionary<Unit, Stack<double>>)args[0];

            Piece piece = null;
            Player player = null;
            bool redoDisband = false;
            foreach (Unit u in undoInfo.Keys)
            {
                u.UndoDisband(undoInfo[u]);
                if (u.Dead)
                    redoDisband = true;
                piece = u;
                player = u.Owner;
            }

            if (redoDisband || player.Death < 0)
            {
                Unit[] units = new Unit[undoInfo.Keys.Count];
                undoInfo.Keys.CopyTo(units, 0);
                int stack = UndoCommands.Count;
                DisbandUnits(units);
                RemoveUndos(stack);
            }
            return piece;
        }

        private static void AddUnitTile(Unit u, Tile t)
        {
            if (!UnitTiles.ContainsKey(u))
                UnitTiles.Add(u, new List<Tile>());
            UnitTiles[u].Add(t);
        }
        private static void RemoveUndos(int stack)
        {
            List<object> args = new List<object>();
            while (UndoCommands.Count > stack)
            {
                UndoCommands.Pop();
                args.AddRange(UndoArgs.Pop());
            }
            RemoveUndosForPiecesInArgs(args);
        }
        private static void RemoveUndosForPiece(Piece piece)
        {
            RemoveUndosForPiecesInArgs(Enumerate(piece));
        }
        private static IEnumerable<object> Enumerate(Piece piece)
        {
            yield return piece;
        }
        private static void RemoveUndosForPieces(IEnumerable<Piece> pieces)
        {
            RemoveUndosForPiecesInArgs(Enumerate(pieces));
        }
        private static IEnumerable<object> Enumerate(IEnumerable<Piece> pieces)
        {
            foreach (Piece piece in pieces)
                yield return piece;
        }
        private static void RemoveUndosForPiecesInArgs(IEnumerable<object> pieces)
        {
            List<object> removeArgs = new List<object>();

            //cant undo any previous commands involving this unit
            List<UndoDelegate> newCommands = new List<UndoDelegate>();
            List<object[]> newArgs = new List<object[]>();
            while (UndoArgs.Count > 0)
            {
                UndoDelegate undo = UndoCommands.Pop();
                object[] args = UndoArgs.Pop();
                if (!IsUndoTerrain(undo, args, pieces) && !ContainsPiece(args, pieces))
                {
                    newCommands.Add(undo);
                    newArgs.Add(args);
                }
                else
                {
                    removeArgs.AddRange(args);
                }
            }

            Stack<UndoDelegate> newCommandStack = new Stack<UndoDelegate>();
            Stack<object[]> newArgStack = new Stack<object[]>();
            for (int i = newCommands.Count ; --i > -1 ; )
            {
                newCommandStack.Push(newCommands[i]);
                newArgStack.Push(newArgs[i]);
            }
            UndoCommands = newCommandStack;
            UndoArgs = newArgStack;

            if (removeArgs.Count > 0)
                RemoveUndosForPiecesInArgs(removeArgs);
        }
        private static bool IsUndoTerrain(UndoDelegate undo, object[] args, IEnumerable<object> pieces)
        {
            if (undo == UndoChangeTerrain)
            {
                Tile tile = ( (Wizard)args[0] ).Tile;
                foreach (object piece in pieces)
                {
                    Unit u = ( piece as Unit );
                    if (u != null && UnitTiles.ContainsKey(u) && UnitTiles[u].Contains(tile))
                        return true;
                }
            }
            return false;
        }
        private static bool ContainsPiece(object arg, IEnumerable<object> pieces)
        {
            foreach (Piece p1 in FindAllPieces(arg))
                foreach (Piece p2 in FindAllPieces(pieces))
                    if (p1 == p2)
                        return true;
            return false;
        }
        private static IEnumerable<Piece> FindAllPieces(object arg)
        {
            Piece p;
            System.Collections.IDictionary dictionary;
            System.Collections.IEnumerable enumberable;
            if (( p = arg as Piece ) != null)
                yield return p;
            else if (( dictionary = arg as System.Collections.IDictionary ) != null)
            {
                foreach (Piece piece in FindAllPieces(dictionary.Keys))
                    yield return piece;
                foreach (Piece piece in FindAllPieces(dictionary.Values))
                    yield return piece;
            }
            else if (( enumberable = arg as System.Collections.IEnumerable ) != null)
            {
                foreach (object obj in enumberable)
                    foreach (Piece piece in FindAllPieces(obj))
                        yield return piece;
            }
        }
        #endregion //public commands

        #region public methods and properties
        public int Turn
        {
            get
            {
                return turn;
            }
        }

        public void ResetPics(float zoom)
        {
            Player.ResetPics(this.players, zoom);
        }

        public Player[] GetDefeatedPlayers()
        {
            return (Player[])defeatedPlayers.Clone();
        }

        public Dictionary<Player, int> GetWon()
        {
            Dictionary<Player, int> r = new Dictionary<Player, int>();
            foreach (KeyValuePair<Player, int> p in winningPlayers)
                r.Add(p.Key, p.Value);
            return r;
        }

        public int GetUnitHas(string name)
        {
            return unitsHave[name];
        }

        public int GetUnitNeeds(string name)
        {
            return Unit.CreateTempUnit(name).BaseCost;
        }

        public static int NewGroup()
        {
            return (int)Game.Random.NextUInt();
        }

        public int Width
        {
            get
            {
                return width;
            }
            private set
            {
                width = value;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
            private set
            {
                height = value;
            }
        }

        public Tile GetTile(int x, int y)
        {
            return map[x, y];
        }

        public Player[] GetPlayers()
        {
            return (Player[])players.Clone();
        }

        public Player CurrentPlayer
        {
            get
            {
                if (players.Length < 1)
                    return defeatedPlayers[0];
                return players[currentPlayer];
            }
        }
        #endregion //public methods and properties

        #region internal methods
        internal void CreateWizardPts()
        {
            Tile t;

            //keep going until a valid tile is found
            while (true)
            {
                t = RandomTile();
                if (( t.WizardPoints > 0 || t.HasWizard() ))
                    continue;

                //check neighbors
                bool can = true;
                for (int a = -1 ; ++a < 6 ; )
                {
                    Tile neighbor = t.GetNeighbor(a);
                    if (neighbor != null && ( neighbor.WizardPoints > 0 || neighbor.HasWizard() ))
                    {
                        can = false;
                        break;
                    }
                }
                if (can)
                    break;
            }

            t.MakeWizPts();
        }

        internal Tile RandomTile()
        {
            return map[Random.Next(Width), Random.Next(Height)];
        }

        private void WinGame(Player win)
        {
            if (win != null)
            {
                winningPlayers.Add(win, turn - 1);
                RemovePlayer(win);

                if (players.Length == 1)
                    players[0].KillPlayer();
                else if (players.Length > 1)
                    foreach (Piece piece in win.GetPieces())
                    {
                        piece.Tile.Remove(piece);
                        win.Remove(piece, true);
                    }
            }
        }
        internal void DefeatPlayer(Player player)
        {
            //add the losing player to the defeatedPlayers array
            Player[] newLost = new Player[defeatedPlayers.Length + 1];
            newLost[0] = player;
            for (int i = 1 ; i < newLost.Length ; ++i)
                newLost[i] = defeatedPlayers[i - 1];
            defeatedPlayers = newLost;

            RemovePlayer(player);
        }
        private void RemovePlayer(Player player)
        {
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

            //ensure we still have the correct current player
            if (currentPlayer > removedIndex)
                --currentPlayer;
            else if (currentPlayer == removedIndex)
                EndTurn(true);
        }
        #endregion //internal methods

        #region increment turn
        private void IncrementTurn()
        {
            ++turn;

            //initialize variables
            Player deadPlayer = null, win;
            Dictionary<Player, double> deadPlayers = null;
            bool noCapts = false;
            int[,] counts = GetPlayerCounts(ref deadPlayer, ref deadPlayers, ref noCapts, out win);

            //these must happen in this order
            FreeUnit(noCapts, ref counts);
            RemoveCapturables(noCapts, counts, ref deadPlayer, ref deadPlayers);
            KillPlayers(deadPlayer, deadPlayers, win);
            WinGame(win);

            if (players.Length > 0)
            {
                //the order of these should be irrelevant
                ResetTiles();
                ChangeMap();
                CreateCitySpot();
                ChangeMoveOrder();
                Player.SubtractCommonUpkeep(this.players);
            }
        }
        private int[,] GetPlayerCounts(ref Player deadPlayer, ref Dictionary<Player, double> deadPlayers, ref bool noCapts, out Player win)
        {
            win = null;


            int numPlayers = players.Length;
            double[] values = new double[numPlayers];
            int[,] counts = new int[numPlayers, 5];
            //count the number of different types of pieces for each player
            for (int i = 0 ; i < numPlayers ; ++i)
            {
                values[i] = players[i].GetArmyStrength() + players[i].GetTotalResources();

                int wizards, portals, cities, relics, units;
                players[i].GetCounts(out wizards, out portals, out cities, out relics, out units);

                if (relics < 1 && cities < 1 && portals < 1 && wizards < 1)
                {
                    noCapts = true;
                    //if a player has no capturables, remove one of their remaining units
                    if (units <= players[i].RemoveUnit())
                        AddDeadPlayer(ref deadPlayer, ref deadPlayers, players[i]);
                }

                counts[i, 0] = relics;
                counts[i, 1] = cities;
                counts[i, 2] = portals;
                counts[i, 3] = wizards;
                counts[i, 4] = units;
            }

            for (int a = 0 ; a < numPlayers ; ++a)
            {
                for (int b = 0 ; b < numPlayers ; ++b)
                    if (a != b)
                    {
                        for (int c = 0 ; c < 4 ; ++c)
                            if (counts[a, c] <= counts[b, c])
                                goto next;
                        if (values[a] <= values[b])
                            goto next;
                    }
                win = players[a];
                break;
next:
                {
                }
            }

            return counts;
        }
        private void AddDeadPlayer(ref Player deadPlayer, ref Dictionary<Player, double> deadPlayers, Player player)
        {
            //dont initialize and use the list until its needed
            if (deadPlayer == null)
            {
                deadPlayer = player;
            }
            else
            {
                if (deadPlayers == null)
                {
                    deadPlayers = new Dictionary<Player, double>();
                    deadPlayers.Add(deadPlayer, deadPlayer.GetTotalResources());
                }

                deadPlayers.Add(player, player.GetTotalResources());
            }
        }

        private void FreeUnit(bool noCapts, ref int[,] counts)
        {
            List<string> units = new List<string>();
            foreach (string race in Races.Keys)
            {
                int baseCost;
                string addUnit;
                do
                    addUnit = Races[race][Random.Next(Races[race].Length)];
                while (unitsHave[addUnit] / 3.0f > Random.Gaussian(baseCost = Unit.CreateTempUnit(addUnit).BaseCost));
                unitsHave[addUnit] += Random.GaussianInt(66.6f, 1f);
                //dont place free units when someone has no capturables
                if (!noCapts && unitsHave[addUnit] >= baseCost)
                    units.Add(addUnit);
            }
            if (units.Count > 0)
            {
                Dictionary<string, string> forRaces = GetForRaces(units[Random.Next(units.Count)]);
                foreach (string unit in forRaces.Values)
                    unitsHave[unit] -= Unit.CreateTempUnit(unit).BaseCost;
                double avg = 0;
                foreach (Player player in players)
                    avg += Unit.CreateTempUnit(forRaces[player.Race]).BaseCost;
                avg /= players.Length;
                foreach (Player p in players)
                    p.FreeUnit(forRaces[p.Race], avg);
                int numPlayers = players.Length;
                for (int i = 0 ; i < numPlayers ; ++i)
                    ++counts[i, 4];
            }
        }

        private Dictionary<string, string> GetForRaces(string baseUnit)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            Unit unit = Unit.CreateTempUnit(baseUnit);
            double targetCost = unit.BaseCost;

            retVal.Add(unit.Race, baseUnit);

            double numUnits = 0;
            Dictionary<string, int> raceTotals = new Dictionary<string, int>();
            double avgRaceTotal = 0;
            foreach (string race in Races.Keys)
            {
                int total = 0;
                foreach (string raceUnit in Races[race])
                {
                    ++numUnits;
                    total += unitsHave[raceUnit];
                }
                raceTotals.Add(race, total);
                avgRaceTotal += total;
            }
            numUnits /= Races.Count;
            avgRaceTotal /= Races.Count;

            foreach (string race in Races.Keys)
                if (unit.Race != race)
                {
                    double target = targetCost + ( raceTotals[race] - avgRaceTotal ) / numUnits;
                    if (target < targetCost / 2.0)
                        target = targetCost / 2.0;
                    else if (target > targetCost * 3 / 2.0)
                        target = targetCost * 3 / 2.0;
                    retVal.Add(race, Random.SelectValue<string>(Races[race], delegate(string raceUnit)
                    {
                        double baseCost = Unit.CreateTempUnit(raceUnit).BaseCost;

                        double chance = Math.Abs(target - baseCost) / target;
                        const double offset1 = .03;
                        chance = 1 / ( chance + offset1 );

                        const double offset2 = .003, offset3 = .3;
                        double mult = offset2;
                        double pct = unitsHave[raceUnit] / baseCost;
                        if (pct > 0)
                            mult += offset3 + pct;
                        else
                            mult /= 1 - 6.66 * pct;
                        chance *= mult;

                        return Random.Round(chance * ( int.MaxValue * ( offset1 / ( offset2 + offset3 + 1 ) / Races[race].Length ) - 1 ));
                    }));
                }

            return retVal;
        }

        private void RemoveCapturables(bool noCapts, int[,] counts, ref Player deadPlayer, ref Dictionary<Player, double> deadPlayers)
        {
            if (!noCapts)
            {
                int numPlayers = players.Length;
                //check for a capturable type to remove
                for (int i = -1 ; ++i < 4 ; )
                {
                    Type type = null;
                    //this is the order they are removed; relics first, wizards last
                    switch (i)
                    {
                    case 0:
                        type = typeof(Relic);
                        break;
                    case 1:
                        type = typeof(City);
                        break;
                    case 2:
                        type = typeof(Portal);
                        break;
                    case 3:
                        type = typeof(Wizard);
                        break;
                    }

                    bool canRemove = true;
                    for (int j = 0 ; j < numPlayers ; ++j)
                        if (counts[j, i] < 1)
                        {
                            canRemove = false;
                            break;
                        }

                    if (canRemove)
                    {
                        RemoveCapturables(type);

                        //check if any players died
                        for (int a = 0 ; a < numPlayers ; ++a)
                        {
                            bool any = false;
                            for (int b = 0 ; b < 5 ; ++b)
                                if (counts[a, b] > 0)
                                {
                                    any = true;
                                    break;
                                }
                            if (!any)
                                AddDeadPlayer(ref deadPlayer, ref deadPlayers, players[a]);
                        }

                        //only remove one each round of turns
                        break;
                    }
                }
            }
        }
        private void RemoveCapturables(Type type)
        {
            //const double portalAvg = Player.WizardCost;
            double portalAvg = 0;
            if (type == typeof(Portal))
            {
                //account for partially finished units
                foreach (string[] units in Races.Values)
                    foreach (string unit in units)
                    {
                        Unit u = Unit.CreateTempUnit(unit);
                        if (u.costType != CostType.Production)
                            portalAvg += u.BaseCost / 2.6 * Portal.WorkPct;
                    }
                portalAvg /= Races.Count * 5.0;

                portalAvg += Portal.AvgPortalCost;
            }

            foreach (Player p in players)
                p.RemoveCapturable(type, portalAvg);
        }

        private void KillPlayers(Player deadPlayer, Dictionary<Player, double> deadPlayers, Player win)
        {
            //kill off any players that died during IncrementTurn
            if (deadPlayers != null)
            {
                //there are multiple dead players, so kill them off in order based on the amount of resources they died with
                for (int j = deadPlayers.Count ; --j > -1 ; )
                {
                    //the odds of having a tie condition are absurdly low,
                    //but we need to account for it nontheless and break it randomly
                    int tieCount = 1;
                    Player loser = null;
                    double min = double.MaxValue;
                    foreach (Player p in deadPlayers.Keys)
                        if (deadPlayers[p] < min || ( deadPlayers[p] == min && Random.Next(++tieCount) == 0 ))
                        {
                            if (deadPlayers[p] < min)
                                tieCount = 1;
                            loser = p;
                            min = deadPlayers[p];
                        }

                    deadPlayers.Remove(loser);
                    loser.KillPlayer();
                }
            }
            else if (deadPlayer != null)
            {
                deadPlayer.KillPlayer();
            }

            //if theres only one player left, end the game
            if (players.Length == 1 && players[0] != win)
                WinGame(players[0]);
        }

        private void ResetTiles()
        {
            foreach (Tile t in map)
                t.Reset();
        }

        private void ChangeMap()
        {
            if (Random.Bool(2 / 3f))
            {
                int amt = Random.OEInt(Width * Height / 39.0);
                if (amt > 0)
                {
                    Tile tile = RandomTile();

                    Terrain terrain;
                    switch (Random.Next(6))
                    {
                    case 0:
                        terrain = Terrain.Forest;
                        break;
                    case 1:
                        terrain = Terrain.Mountain;
                        break;
                    case 2:
                        terrain = Terrain.Plains;
                        break;
                    case 3:
                        terrain = Terrain.Water;
                        break;
                    default:
                        terrain = tile.Terrain;
                        break;
                    }

                    for (int a = 0 ; a < amt ; ++a)
                    {
                        tile.Terrain = terrain;

                        Tile nextTile;
                        do
                        {
                            nextTile = tile.GetNeighbor(Random.Next(6));
                        }
                        while (nextTile == null);
                        tile = nextTile;
                    }
                }
            }
        }

        private void CreateCitySpot()
        {
            int amt = Random.OEInt(Width * Height / 666.0);
            while (--amt > -1)
            {
                Tile tile = RandomTile();

                //if it cant be placed, dont even try again with another tile
                if (!( tile.HasCity() ))
                {
                    bool can = true;
                    for (int a = -1 ; ++a < 6 ; )
                    {
                        Tile neighbor = tile.GetNeighbor(a);
                        if (neighbor != null && ( neighbor.HasCity() ))
                        {
                            can = false;
                            break;
                        }
                    }
                    if (can)
                        tile.MakeCitySpot(Random.GaussianCappedInt(6f, .26f, 1) + Random.OEInt(1.3));
                }
            }
        }

        private void ChangeMoveOrder()
        {
            //a lower shuffleValue makes the move order change faster
            const double shuffleValue = 0.169;
            Dictionary<Player, int> bonus = MattUtil.TBSUtil.RandMoveOrder<Player>(Random, players, shuffleValue);
            foreach (KeyValuePair<Player, int> pair in bonus)
                AddMoveOrderDiff(pair.Key, pair.Value);
        }
        private void AddMoveOrderDiff(Player player, int diff)
        {
            //total difference between first and last moving player is worth 300 resources
            double amount = 3 * diff / ( players.Length - 1.0 );
            player.CollectWizardPts(amount);
            player.AddWork(50 * amount * Player.WorkMult);
        }
        #endregion //increment turn

        #region create map
        private void CreateMap(int width, int height)//, int numPlayers)
        {
            map = new Tile[width, height];
            foreach (int coord in Random.Iterate(width * height))
            {
                int x = coord / height;
                int y = coord % height;
                map[x, y] = CreateTile(x, y);
            }

            for (int x = -1 ; ++x < width ; )
                for (int y = -1 ; ++y < height ; )
                    map[x, y].SetupNeighbors();
        }
        private Tile CreateTile(int x, int y)
        {
            Tile tile = null;
            //try three times to find a neighbor that has already been initialized
            for (int i = 0 ; i < 3 ; ++i)
            {
                tile = GetTileIn(x, y, Random.Next(6));
                if (tile != null)
                    break;
            }

            Tile result;
            if (tile == null)
                result = new Tile(this, x, y);
            else
                result = new Tile(this, x, y, tile.Terrain);

            return result;
        }
        internal Tile GetTileIn(int x, int y, int direction)
        {
            //this methoid is called to set up the neighbors array
            bool odd = y % 2 > 0;
            switch (direction)
            {
            case 0:
                if (odd)
                    --x;
                --y;
                break;
            case 1:
                if (!odd)
                    ++x;
                --y;
                break;
            case 2:
                --x;
                break;
            case 3:
                ++x;
                break;
            case 4:
                if (odd)
                    --x;
                ++y;
                break;
            case 5:
                if (!odd)
                    ++x;
                ++y;
                break;
            default:
                throw new Exception();
            }

            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            else
                return map[x, y];
        }
        #endregion //create map
    }
}
