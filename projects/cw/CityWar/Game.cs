using System;
using System.Collections.Generic;
using System.Linq;
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
        public static readonly MattUtil.MTRandom Random;
        static Game()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();

            InitRaces();
        }

        public static string Path = "..\\..\\..\\";
        public static string AutoSavePath = "..\\..\\..\\";
        public static Dictionary<string, string[]> Races;

        private int turn, width, height, currentPlayer;
        private Player[] players;
        private Dictionary<int, Player> winningPlayers, defeatedPlayers;
        private Tile[,] map;
        private Dictionary<string, int> unitsHave;

        #endregion //fields

        #region public commands
        public void AutoSave()
        {
            TBSUtil.SaveGame(this, AutoSavePath + "auto", turn + "-" + currentPlayer + ".cws");
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
            game.defeatedPlayers = new Dictionary<int, Player>();
            game.winningPlayers = new Dictionary<int, Player>();

            game.CreateMap(width, height);

            UnitSchema us;
            int numUnits;
            InitRaces(out us, out numUnits, out game.unitsHave);

            //pick 3 random starting units
            Dictionary<string, string>[] startUnits = new Dictionary<string, string>[3];
            for (int a = -1 ; ++a < 3 ; )
            {
                UnitSchema.UnitRow row = ( (UnitSchema.UnitRow)us.Unit.Rows[Random.Next(numUnits)] );
                //this has to come after unitsHave is set to 0 but before actualy initialized
                startUnits[a] = game.GetForRaces(row.Name);
            }
            double totalStartCost = newPlayers.Average(player => startUnits.Sum(dict => Unit.CreateTempUnit(dict[player.Race]).BaseCost));

            foreach (string[] race in Races.Values)
                foreach (string name in race)
                    game.unitsHave[name] = game.GetInitUnitsHave(name);

            //initialize the players, half with cities and half with wizards
            bool city = Random.Bool();
            IEnumerable<Player> randOrder = Random.Iterate<Player>(newPlayers);
            int addWork = 0;
            foreach (Player current in randOrder)
            {
                string[] raceUnits = new string[3];
                for (int a = -1 ; ++a < 3 ; )
                    raceUnits[a] = startUnits[a][current.Race];
                current.NewPlayer(game, city = !city, raceUnits, totalStartCost);
                addWork = Math.Max(addWork, (int)Math.Ceiling(current.GetTurnUpkeep()) - current.Work);
            }

            //randomize the turn order
            game.currentPlayer = -1;
            game.players = new Player[numPlayers];
            foreach (Player current in randOrder)
            {
                game.players[++game.currentPlayer] = current;
                //players moving later in the turn order receive compensation
                game.AddMoveOrderDiff(current, game.currentPlayer);
                current.AddWork(addWork);
                current.EndTurn();
            }

            //create wizard points and possibly some starting city spots
            double avg = width * height / 65.0;
            int wizspots = 1 + Random.GaussianCappedInt(avg, .052, (int)( avg / 1.3 ));
            for (int a = -1 ; ++a < wizspots ; )
                game.CreateWizardPts();
            for (int a = -1 ; ++a < numPlayers ; )
                game.CreateCitySpot();

            //	Start the game!
            Player.SubtractCommonUpkeep(game.players);
            game.currentPlayer = 0;
            game.players[0].StartTurn();

            return game;
        }
        private int GetInitUnitsHave(string name)
        {
            int needed = Unit.CreateTempUnit(name).BaseCost;
            //~1/28 will be outside [-needed,needed]
            return Random.GaussianInt(needed / 2.1);
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
                unitsHave.Add(name, 0);
            }

            Races = new Dictionary<string, string[]>();
            foreach (string key in tempRaces.Keys)
                Races.Add(key, tempRaces[key].ToArray());
        }

        private delegate Piece UndoDelegate(object[] args);
        private static Stack<UndoDelegate> UndoCommands = new Stack<UndoDelegate>();
        private static Stack<object[]> UndoArgs = new Stack<object[]>();
        private static Dictionary<Unit, List<Tile>> UnitTiles = new Dictionary<Unit, List<Tile>>();
        private static Dictionary<Tile, List<Unit>> TileUnits = new Dictionary<Tile, List<Unit>>();

        private static void ClearUndos()
        {
            UndoCommands.Clear();
            UndoArgs.Clear();
            UnitTiles.Clear();
            TileUnits.Clear();
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

            if (piece.Owner.Air < 0 || piece.Owner.Death < 0 || piece.Owner.Earth < 0 || piece.Owner.Nature < 0 || piece.Owner.Production < 0 || piece.Owner.Water < 0
                    || piece.Owner.Magic < 0 || piece.Owner.relic < 0 || piece.Owner.Population < 0 || piece.Owner.GetTurnUpkeep() < 0 || piece.Owner.Work < 0)
            {
            }
            piece.Owner.CheckNegativeResources();

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

                if (players.Length > 0 && currentPlayer >= players.Length)
                {
                    //a new round of turns
                    IncrementTurn();
                    currentPlayer = 0;
                }

                CurrentPlayer.StartTurn();
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

        public Battle StartBattle(Tile target, IEnumerable<Unit> selected)
        {
            Player enemy;
            if (target.OccupiedByUnit(out enemy) && enemy != CurrentPlayer)
            {
                HashSet<Unit> defenders = new HashSet<Unit>();

                //'Immobile' units defend first, by themselves
                defenders.UnionWith(target.FindAllUnits(defender => defender.Type == UnitType.Immobile));
                if (defenders.Count > 0)
                {
                    //attack with either just the selected units, or all within range, but do not add any additional defenders
                    if (selected == null)
                        selected = GetAttackers(target);
                }
                else if (selected == null)
                {
                    //collect all battle units recursively, starting with attackers that can target any units in the target tile
                    Dictionary<Unit, int> attackers = new Dictionary<Unit, int>();
                    foreach (Unit defender in target.GetAllUnits())
                        AddAttackers(defenders, enemy, defender, attackers);
                    selected = attackers.Keys;
                }
                else
                {
                    //limit attackers to just the selected units, but still include all relevant defenders

                    //first collect defenders that can be targeted by the selected units
                    foreach (Unit attacker in selected)
                        AddDefenders(defenders, enemy, attacker);

                    //then collect defenders that can retalliate
                    foreach (Unit attacker in selected)
                    {
                        //find the minimum length attack the attacker might use
                        int length = int.MaxValue, minLength;
                        foreach (Unit defender in defenders)
                            if (CanTarget(attacker, defender, out minLength))
                                length = Math.Min(length, minLength);
                        //add defenders that can retalliate at that length
                        if (length != int.MaxValue)
                            AddDefenders(defenders, enemy, attacker, length);
                    }
                }

                if (selected.Any() && defenders.Any())
                {
                    if (!selected.All(CanStartBattle))
                        throw new Exception();

                    return Unit.StartBattle(selected, defenders);
                }
            }
            return null;
        }
        private void AddAttackers(HashSet<Unit> defenders, Player enemy, Unit defender, Dictionary<Unit, int> attackers)
        {
            int minLength, hasLength;
            //find all adjacent attackers that might be able to participate
            foreach (Unit attacker in GetAttackers(defender.Tile))
                //check if the potential attacker can target the defender and is not already in the battle with an equal or shorter length weapon
                if (CanTarget(attacker, defender, out minLength) && !( attackers.TryGetValue(attacker, out hasLength) && hasLength <= minLength ))
                {
                    //add the found attacker to the battle with the minimum length weapon it might use
                    attackers[attacker] = minLength;
                    //collect additional adjacent defenders
                    AddDefenders(defenders, enemy, attacker, minLength, attackers);
                }
        }
        private IEnumerable<Unit> GetAttackers(Tile target)
        {
            return FindNeighborUnits(target, CanStartBattle);
        }
        private bool CanStartBattle(Unit attacker)
        {
            return ( attacker.Owner == CurrentPlayer && attacker.Movement > 0 );
        }
        private void AddDefenders(HashSet<Unit> defenders, Player enemy, Unit attacker, int length = int.MaxValue, Dictionary<Unit, int> attackers = null)
        {
            //find all adjacent defenders that can either retalliate against or be targeted by this attacker
            foreach (Unit defender in FindNeighborUnits(attacker.Tile, defender =>
                    defender.Owner == enemy && ( CanTarget(defender, attacker, length) || CanTarget(attacker, defender) ) && !defenders.Contains(defender)))
            {
                //add the found defender to the battle
                defenders.Add(defender);

                //collect additional adjacent attackers
                if (attackers != null)
                    AddAttackers(defenders, enemy, defender, attackers);
            }
        }
        private static IEnumerable<Unit> FindNeighborUnits(Tile tile, Predicate<Unit> match)
        {
            return tile.GetNeighbors().SelectMany(neighbor => neighbor.FindAllUnits(match));
        }
        private static bool CanTarget(Unit unit, Unit target, int length = int.MinValue)
        {
            int minLength;
            return CanTarget(unit, target, out minLength, length);
        }
        private static bool CanTarget(Unit unit, Unit target, out int minLength, int length = int.MinValue)
        {
            minLength = unit.Attacks.Where(attack => attack.Length >= length && attack.CanTarget(target))
                    .Select(attack => attack.Length).DefaultIfEmpty(int.MaxValue).Min();
            return ( minLength != int.MaxValue );
        }

        public bool EndBattle(Battle b)
        {
            if (b.canRetalliate)
            {
                b.StartRetalliation();

                //consider the battle over if no one can retalliate
                if (b.attackers.Count > 0)
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

                    bool canUndo = !wizard.Tile.GetAllPieces().OfType<Relic>().Any();
                    if (canUndo)
                    {
                        UndoCommands.Push(UndoChangeTerrain);
                        UndoArgs.Push(new object[] { wizard, movement, oldTerrain });
                    }
                    else
                    {
                        //if you cannot undo changing the terrain, you can no longer undo any pieces that were built or moved onto the tile
                        RemoveUndosForTile(wizard.Tile);
                    }
                }
            }
        }

        private static Piece UndoChangeTerrain(object[] args)
        {
            Wizard wizard = (Wizard)args[0];
            int oldMove = (int)args[1];
            Terrain oldTerrain = (Terrain)args[2];

            if (oldMove < 1 || oldTerrain == wizard.Tile.Terrain)
                throw new Exception();

            wizard.UndoChangeTerrain(oldMove, oldTerrain);
            return wizard;
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
                    Unit u = ( piece as Unit );
                    if (u != null)
                        AddTileUnit(u.Tile, u);

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

        public bool MovePieces(Tile from, int x, int y, bool group, bool gamble)
        {
            Player player;
            from.Occupied(out player);
            if (player != players[currentPlayer])
                return false;

            if (group)
            {
                return MovePiecesHelper(from, x, y, null, gamble);
            }
            else
            {
                Piece[] pieces = from.FindAllPieces(piece => piece.Group == from.CurrentGroup && piece.Movement > 0);
                bool any = false;
                //call the helper once for each piece as they will be moved individually
                foreach (Piece p in Random.Iterate(pieces))
                    if (MovePiecesHelper(from, x, y, p, gamble))
                        any = true;

                if (any)
                    RegroupMoved(from, x, y, pieces);

                return any;
            }
        }
        private bool MovePiecesHelper(Tile from, int x, int y, Piece singlePiece, bool gamble)
        {
            Player player;
            from.Occupied(out player);
            if (player != players[currentPlayer])
                return false;

            Tile to = map[x, y];

            Dictionary<Piece, int> oldMoves = new Dictionary<Piece, int>();
            foreach (Piece p in from.GetSelectedPieces())
                oldMoves.Add(p, p.Movement);
            Dictionary<Piece, bool> undoPieces;
            if (singlePiece == null)
                undoPieces = from.MoveSelectedPieces(to, gamble);
            else
                undoPieces = from.MovePiece(singlePiece, to, gamble);

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
                if (any && singlePiece == null)
                    RegroupMoved(from, x, y, undoPieces.Keys);

                foreach (Piece p in undoPieces.Keys)
                {
                    if (undoPieces[p])
                    {
                        if (from != p.Tile)
                        {
                            Unit u = p as Unit;
                            if (u != null)
                                AddTileUnit(u.Tile, u);

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

                if (wizCheck || piece.Owner.Work < 0)
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

        public void CaptureCity(Unit unit)
        {
            if (players[currentPlayer] != unit.Owner)
                return;

            if (unit.CaptureCity())
            {
                UndoCommands.Push(UndoCaptureCity);
                UndoArgs.Push(new object[] { unit });
            }
        }
        private Piece UndoCaptureCity(object[] args)
        {
            Unit unit = (Unit)args[0];

            unit.UndoCaptureCity();

            if (unit.Owner.Population < 0 || unit.Owner.Work < 0)
            {
                int stack = UndoCommands.Count;
                CaptureCity(unit);
                RemoveUndos(stack);
            }
            return unit;
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
        private static void AddTileUnit(Tile t, Unit u)
        {
            if (!TileUnits.ContainsKey(t))
                TileUnits.Add(t, new List<Unit>());
            TileUnits[t].Add(u);
        }
        private static void RemoveUndosForTile(Tile tile)
        {
            List<Unit> units;
            if (TileUnits.TryGetValue(tile, out units))
                RemoveUndosForPieces(units);
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
                bool undoTerrain = IsUndoTerrain(undo, args, pieces);
                if (!undoTerrain && !ContainsPiece(args, pieces))
                {
                    newCommands.Add(undo);
                    newArgs.Add(args);
                }
                else
                {
                    removeArgs.AddRange(args);
                    if (undoTerrain)
                        RemoveUndosForTile(( (Wizard)args[0] ).Tile);
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
            {
                yield return p;
            }
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

        public SortedList<int, Player> GetWon()
        {
            return GetSortedList(winningPlayers);
        }
        public SortedList<int, Player> GetLost()
        {
            return GetSortedList(defeatedPlayers);
        }
        private static SortedList<int, Player> GetSortedList(Dictionary<int, Player> dictionary)
        {
            SortedList<int, Player> retVal = new SortedList<int, Player>();
            foreach (var pair in dictionary)
                retVal.Add(pair.Key, pair.Value);
            return retVal;
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
            return Random.RangeInt(int.MinValue, int.MaxValue);
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
            Tile tile = RandomTile(neighbor => neighbor == null || ( neighbor.WizardPoints == 0 && !neighbor.HasWizard() ));
            tile.MakeWizPts();
        }

        internal Tile RandomTile(Func<Tile, bool> ValidNeighbor = null)
        {
            while (true)
            {
                Tile tile = map[Random.Next(Width), Random.Next(Height)];
                if (tile != null && ( ValidNeighbor == null || tile.GetNeighbors(true, true).All(ValidNeighbor) ))
                    return tile;
            }
        }

        private void WinGame(Player win)
        {
            if (win != null)
            {
                AddPlayer(winningPlayers, turn, win);
                RemovePlayer(win);

                if (players.Length == 1)
                {
                    players[0].KillPlayer();
                }
                else if (players.Length > 1)
                {
                    Piece[] temp = new Piece[win.GetPieces().Count];
                    win.GetPieces().CopyTo(temp, 0);
                    foreach (Piece piece in temp)
                    {
                        piece.Tile.Remove(piece);
                        win.Remove(piece, true);
                    }
                }
            }
        }

        internal void DefeatPlayer(Player player)
        {
            AddPlayer(defeatedPlayers, turn, player);
            RemovePlayer(player);
        }

        private static void AddPlayer(Dictionary<int, Player> dict, int turn, Player p)
        {
            AddPlayer(dict, turn, p, false);
        }
        private static void AddPlayer(Dictionary<int, Player> dict, int turn, Player p, bool lower)
        {
            Player low;
            if (dict.TryGetValue(turn, out low))
            {
                if (lower)
                {
                    Player temp = p;
                    p = low;
                    low = temp;
                }

                if (Random.Bool())
                {
                    dict[turn] = low;
                    AddPlayer(dict, turn + 1, p, true);
                }
                else
                {
                    dict[turn] = p;
                    AddPlayer(dict, turn - 1, low, false);
                }
            }
            else
            {
                dict.Add(turn, p);
            }
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
            //initialize variables
            Player deadPlayer = null, win;
            Dictionary<Player, double> deadPlayers = null;
            bool noCapts = false;
            int[,] counts = GetPlayerCounts(ref deadPlayer, ref deadPlayers, ref noCapts, out win);

            //these must happen in this order
            FreeUnit(noCapts, ref counts);
            if (win == null)
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

            ++turn;
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
                ;
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
                    addUnit = Random.SelectValue(Races[race]);
                while (unitsHave[addUnit] / 2.6f > Random.Gaussian(baseCost = Unit.CreateTempUnit(addUnit).BaseCost));
                unitsHave[addUnit] += Random.GaussianOEInt(65, 1, .21);
                if (unitsHave[addUnit] >= baseCost)
                    units.Add(addUnit);
            }
            //dont place free units when someone has no capturables
            if (!noCapts && units.Count > 0)
            {
                Dictionary<string, string> forRaces = GetForRaces(Random.SelectValue(units));
                foreach (string unit in forRaces.Values)
                    unitsHave[unit] -= Unit.CreateTempUnit(unit).BaseCost;
                double avg = players.Average(player => Unit.CreateTempUnit(forRaces[player.Race]).BaseCost);
                foreach (Player p in players)
                    p.FreeUnit(forRaces[p.Race], avg);
                for (int i = 0 ; i < players.Length ; ++i)
                    ++counts[i, 4];
            }
        }

        private Dictionary<string, string> GetForRaces(string targetName)
        {
            Unit targetUnit = Unit.CreateTempUnit(targetName);
            double avgRaceTotal = unitsHave.Values.Sum() / (double)Races.Count;

            return Races.ToDictionary(race => race.Key, race =>
            {
                if (targetUnit.Race == race.Key)
                {
                    return targetName;
                }
                else
                {
                    double raceTotal = race.Value.Sum(name => unitsHave[name]);
                    double target = targetUnit.BaseCost + ( raceTotal - avgRaceTotal ) / (double)race.Value.Length;
                    double minTarget = targetUnit.BaseCost / 1.69;

                    bool isHigh = ( target > targetUnit.BaseCost );
                    Func<double> ReverseTarget = ( () => 2 * targetUnit.BaseCost - target );
                    if (isHigh)
                        target = ReverseTarget();
                    if (target < minTarget)
                        target = minTarget / ( Math.Pow(1 + minTarget - target, .52) );
                    if (isHigh)
                        target = ReverseTarget();

                    IDictionary<string, int> dict = null;
                    try
                    {
                        dict = race.Value.ToDictionary(name => name, name =>
                        {
                            double baseCost = Unit.CreateTempUnit(name).BaseCost;

                            double chance = Math.Abs(target - baseCost) / target;
                            chance = 1 / ( .039 + chance );
                            chance *= chance;

                            double pct = unitsHave[name] / baseCost;
                            if (pct >= 1)
                                pct *= 1.3 * pct;
                            if (pct < 0)
                                chance *= .052 / ( 1 - 6.5 * pct );
                            else
                                chance *= .26 + pct;

                            return Random.Round(chance * short.MaxValue);
                        });
                        return Random.SelectValue(dict);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        return Random.SelectValue(race.Value);
                    }
                }
            });
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
                    //this is the order they are removed: relics first, wizards last
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
            double portalAvg = double.NaN;
            if (type == typeof(Portal))
            {
                //account for partially finished units
                portalAvg = Races.Values.SelectMany(units => units).Select(unit => Unit.CreateTempUnit(unit))
                        .Where(unit => unit.costType != CostType.Production).Sum(unit => unit.BaseCost);
                portalAvg *= Portal.StartAmt * Portal.ValuePct / (double)Races.Count / 5.0;

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
                    Player loser = null;
                    double min = double.MaxValue;
                    foreach (Player p in Random.Iterate(deadPlayers.Keys))
                        if (deadPlayers[p] < min)
                        {
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
                if (t != null)
                    t.Reset();
        }

        private void ChangeMap()
        {
            if (Random.Bool(.65))
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
                            nextTile = tile.GetNeighbor(Random.Next(6));
                        while (nextTile == null);
                        tile = nextTile;
                    }
                }
            }
        }

        private void CreateCitySpot()
        {
            int amt = Random.OEInt(Width * Height / 780.0);
            for (int a = 0 ; a < amt ; ++a)
            {
                //select a tile not on the map edge
                Tile tile = RandomTile(neighbor => neighbor != null);
                //don't try again if it is on or next to an existing city
                if (tile.GetNeighbors(true).All(neighbor => !neighbor.HasCity()))
                    tile.MakeCitySpot(Random.GaussianOEInt(7.8, .39, .169, 1));
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
            double amount = diff * 3.0 / ( players.Length - 1.0 );
            player.CollectWizardPts(amount);
            player.BalanceForUnit(50 * amount, 0);
        }
        #endregion //increment turn

        #region create map
        private void CreateMap(int width, int height)//, int numPlayers)
        {
            map = new Tile[width, height];

            foreach (Point coord in Random.Iterate(width, height))
                map[coord.X, coord.Y] = CreateTile(coord.X, coord.Y);

            for (int x = -1 ; ++x < width ; )
                for (int y = -1 ; ++y < height ; )
                    if (map[x, y] != null)
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
