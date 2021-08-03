using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using MattUtil;
using System.Runtime.Serialization;

namespace CityWar
{
    [Serializable]
    public class Game : IDeserializationCallback
    {
        #region fields

        public static readonly MattUtil.MTRandom Random;
        static Game()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();
        }

        public static readonly string ResourcePath = "..\\..\\..\\";

        public static string AutoSavePath = "..\\..\\..\\";
        public static Dictionary<string, string[]> Races;

        public readonly UnitTypes UnitTypes;
        public readonly int Diameter;

        private readonly Tile[,] map;
        private readonly Dictionary<int, Player> winningPlayers, defeatedPlayers;
        private readonly Dictionary<string, int> freeUnits;

        private Player[] players;
        private int turn, currentPlayer;

        private delegate Piece UndoDelegate(object[] args);
        private Stack<UndoDelegate> UndoCommands = new Stack<UndoDelegate>();
        private Stack<object[]> UndoArgs = new Stack<object[]>();
        private readonly Dictionary<Unit, List<Tile>> UnitTiles = new Dictionary<Unit, List<Tile>>();
        private readonly Dictionary<Tile, List<Unit>> TileUnits = new Dictionary<Tile, List<Unit>>();

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

        public Game(Player[] newPlayers, int radius)
        {
            this.UnitTypes = new UnitTypes();
            this.Diameter = radius * 2 - 1;

            this.map = new Tile[Diameter, Diameter];
            this.winningPlayers = new Dictionary<int, Player>();
            this.defeatedPlayers = new Dictionary<int, Player>();

            int numPlayers = newPlayers.Length;
            this.players = new Player[numPlayers];

            this.turn = 1;
            this.currentPlayer = -1;

            UnitSchema us;
            int numUnits;
            this.freeUnits = InitRaces(out us, out numUnits);

            //ensure the map is big enough for all the players
            const int HexesPerPlayer = 13;
            if (numPlayers * HexesPerPlayer > MapSize)
                throw new ArgumentOutOfRangeException("newPlayers", numPlayers, string.Format(
                        "Map is too small for that many players.  Must be at least {0} hexes.",
                        numPlayers * HexesPerPlayer));

            ClearUndos();

            CreateMap(radius);

            List<string> races = new List<string>();
            //pick 3 random starting units
            Dictionary<string, string>[] startUnits = new Dictionary<string, string>[3];
            for (int a = 0; a < 3; ++a)
            {
                if (!races.Any())
                    races = Races.Keys.ToList();
                string race = Random.SelectValue(races);
                races.Remove(race);
                string unit = Random.SelectValue(Races[race]);
                //this should come after freeUnits is set to 0 but before actualy initialized
                startUnits[a] = GetForRaces(unit);
            }
            double totalStartCost = newPlayers.Average(player => startUnits.Sum(
                    dict => Unit.CreateTempUnit(this, dict[player.Race]).BaseTotalCost));

            foreach (string[] race in Races.Values)
                foreach (string name in race)
                    freeUnits[name] = GetInitUnitsHave(name);

            //initialize the players, half with cities and half with wizards
            bool city = Random.Bool();
            IEnumerable<Player> randOrder = Random.Iterate<Player>(newPlayers);
            int addWork = 0;
            foreach (Player current in randOrder)
            {
                string[] raceUnits = new string[3];
                for (int a = 0; a < 3; ++a)
                    raceUnits[a] = startUnits[a][current.Race];
                current.NewPlayer(this, city = !city, raceUnits, totalStartCost);
                addWork = Math.Max(addWork, (int)Math.Ceiling(current.GetTurnUpkeep()) - current.Work);
            }

            //randomize the turn order          
            foreach (Player current in randOrder)
            {
                players[++currentPlayer] = current;
                //players moving later in the turn order receive compensation
                AddMoveOrderDiff(current, currentPlayer);
                current.AddWork(addWork);
                current.EndTurn();
            }

            //create wizard points and possibly some starting city spots
            double avg = NumWizSpots();
            int wizspots = Random.GaussianCappedInt(avg, .117, 1);
            for (int a = 0; a < wizspots; ++a)
                CreateWizardPts(true);
            for (int a = 0; a < numPlayers; ++a)
                CreateCitySpot();

            //	Start the game!
            Player.SubtractCommonUpkeep(players);
            currentPlayer = 0;
            players[0].StartTurn();
        }
        private double NumWizSpots()
        {
            return 1 + MapSize / 78.0;
        }

        private int GetInitUnitsHave(string name)
        {
            int needed = Unit.CreateTempUnit(this, name).BaseTotalCost;
            //~1/28 will be outside [-needed,needed]
            return Random.GaussianInt(needed / 2.1);
        }
        private Dictionary<string, int> InitRaces(out UnitSchema us, out int numUnits)
        {
            //initialize units
            us = UnitTypes.GetSchema();
            numUnits = us.Unit.Rows.Count;
            Dictionary<string, List<string>> tempRaces = new Dictionary<string, List<string>>();
            var unitsHave = new Dictionary<string, int>(numUnits);
            for (int a = -1; ++a < numUnits;)
            {
                UnitSchema.UnitRow row = ((UnitSchema.UnitRow)us.Unit.Rows[a]);
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

            return unitsHave;
        }

        private void ClearUndos()
        {
            UndoCommands.Clear();
            UndoArgs.Clear();
            UnitTiles.Clear();
            TileUnits.Clear();
        }

        public bool CanUndoCommand()
        {
            return (UndoCommands.Count > 0);
        }
        public Tile UndoCommand()
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
                        return null;

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
                if (CanTarget(attacker, defender, out minLength) && !(attackers.TryGetValue(attacker, out hasLength) && hasLength <= minLength))
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
            return (attacker.Owner == CurrentPlayer && attacker.Movement > 0);
        }
        private void AddDefenders(HashSet<Unit> defenders, Player enemy, Unit attacker, int length = int.MaxValue, Dictionary<Unit, int> attackers = null)
        {
            //find all adjacent defenders that can either retalliate against or be targeted by this attacker
            foreach (Unit defender in FindNeighborUnits(attacker.Tile, defender =>
                    defender.Owner == enemy && (CanTarget(defender, attacker, length) || CanTarget(attacker, defender)) && !defenders.Contains(defender)))
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
            return (minLength != int.MaxValue);
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
                    Unit u = (piece as Unit);
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
                && UndoArgs.Peek()[0] == from && ((Piece)UndoArgs.Peek()[1]).Tile == movedTile)
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
            for (int i = 0; i < length; ++i)
                if (selPieces[i].Owner != players[currentPlayer])
                    return;

            bool any = false;
            Dictionary<Piece, double> undoInfo = new Dictionary<Piece, double>();
            for (int i = 0; i < length; ++i)
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
                if ((u = p as Unit) != null && u.Dead)
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
                    wizCheck = (element < 0);
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

        private void AddUnitTile(Unit u, Tile t)
        {
            if (!UnitTiles.ContainsKey(u))
                UnitTiles.Add(u, new List<Tile>());
            UnitTiles[u].Add(t);
        }
        private void AddTileUnit(Tile t, Unit u)
        {
            if (!TileUnits.ContainsKey(t))
                TileUnits.Add(t, new List<Unit>());
            TileUnits[t].Add(u);
        }
        private void RemoveUndosForTile(Tile tile)
        {
            List<Unit> units;
            if (TileUnits.TryGetValue(tile, out units))
                RemoveUndosForPieces(units);
        }
        private void RemoveUndos(int stack)
        {
            List<object> args = new List<object>();
            while (UndoCommands.Count > stack)
            {
                UndoCommands.Pop();
                args.AddRange(UndoArgs.Pop());
            }
            RemoveUndosForPiecesInArgs(args);
        }
        private void RemoveUndosForPiece(Piece piece)
        {
            RemoveUndosForPiecesInArgs(Enumerate(piece));
        }
        private static IEnumerable<object> Enumerate(Piece piece)
        {
            yield return piece;
        }
        private void RemoveUndosForPieces(IEnumerable<Piece> pieces)
        {
            RemoveUndosForPiecesInArgs(Enumerate(pieces));
        }
        private static IEnumerable<object> Enumerate(IEnumerable<Piece> pieces)
        {
            foreach (Piece piece in pieces)
                yield return piece;
        }
        private void RemoveUndosForPiecesInArgs(IEnumerable<object> pieces)
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
                        RemoveUndosForTile(((Wizard)args[0]).Tile);
                }
            }

            Stack<UndoDelegate> newCommandStack = new Stack<UndoDelegate>();
            Stack<object[]> newArgStack = new Stack<object[]>();
            for (int i = newCommands.Count; --i > -1;)
            {
                newCommandStack.Push(newCommands[i]);
                newArgStack.Push(newArgs[i]);
            }
            UndoCommands = newCommandStack;
            UndoArgs = newArgStack;

            if (removeArgs.Count > 0)
                RemoveUndosForPiecesInArgs(removeArgs);
        }
        private bool IsUndoTerrain(UndoDelegate undo, object[] args, IEnumerable<object> pieces)
        {
            if (undo == UndoChangeTerrain)
            {
                Tile tile = ((Wizard)args[0]).Tile;
                foreach (object piece in pieces)
                {
                    Unit u = (piece as Unit);
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
            if ((p = arg as Piece) != null)
            {
                yield return p;
            }
            else if ((dictionary = arg as System.Collections.IDictionary) != null)
            {
                foreach (Piece piece in FindAllPieces(dictionary.Keys))
                    yield return piece;
                foreach (Piece piece in FindAllPieces(dictionary.Values))
                    yield return piece;
            }
            else if ((enumberable = arg as System.Collections.IEnumerable) != null)
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
            return freeUnits[name];
        }

        public int GetUnitNeeds(string name)
        {
            return Unit.CreateTempUnit(this, name).BaseTotalCost;
        }

        public static int NewGroup()
        {
            return (int)Random.NextUInt();
        }

        public int MapSize
        {
            get
            {
                return GetMapSize(Diameter);
            }
        }
        private static int GetMapSize(int Diameter)
        {
            double numHexes = GetNumHexes((Diameter + 1) / 2);
            int mapSize = (int)numHexes;
            if (numHexes != mapSize)
                throw new Exception();
            return mapSize;
        }
        public static double GetNumHexes(double radius)
        {
            return 3 * radius * (radius - 1) + 1;
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= Diameter || y < 0 || y >= Diameter)
                return null;
            else
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
                {
                    int max = winningPlayers.Concat(defeatedPlayers).Max(pair => pair.Key);
                    if (winningPlayers.ContainsKey(max))
                        return winningPlayers[max];
                    return defeatedPlayers[max];
                }
                return players[currentPlayer];
            }
        }

        #endregion //public methods and properties

        #region internal methods

        internal void CreateWizardPts(bool alwaysOne = false)
        {
            int create = 1;
            if (!alwaysOne)
            {
                int count = map.OfType<Tile>().Count(t => t.WizardPoints > 0);
                double target = NumWizSpots();
                //if (count < target)
                //    target += Math.Pow(1 / (count + 1.0), 2.6); 
                double avg = Math.Pow((target + 1) / (count + 2.0), 1.3);
                create = Game.Random.GaussianOEInt(avg, .26, .13, count == 0 ? 1 : 0);
            }
            for (int a = 0; a < create; a++)
            {
                Tile tile = RandomTile(neighbor => neighbor == null || (neighbor.WizardPoints < 1 && !neighbor.HasWizard()));
                tile.MakeWizPts();
            }
        }

        internal Tile RandomTile(Func<Tile, bool> ValidNeighbor = null)
        {
            while (true)
            {
                Tile tile = map[Random.Next(Diameter), Random.Next(Diameter)];
                if (tile != null && (ValidNeighbor == null || tile.GetNeighbors(true, true).All(ValidNeighbor)))
                    return tile;
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
            for (int a = 0, b = -1; a < newPlayers.Length; ++a)
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
            var dead = new List<Player>();
            //priority by which capturables are removed
            var capts = new[] { typeof(Relic), typeof(City), typeof(Portal), typeof(Wizard) };

            var counts = GetPlayerCounts(capts);
            var win = GetWinner(counts);

            //these must happen in this order
            var removed = RemoveUnits(dead, counts);
            FreeUnit(removed);
            RemoveCapturables(dead, capts, counts, win, removed);
            KillPlayers(dead);
            WinGame(win);

            if (players.Length > 0)
            {
                //the order of these should be irrelevant
                ResetTiles();
                ChangeMap();
                CreateCitySpot();
                ChangeMoveOrder();
                Player.SubtractCommonUpkeep(this.players);
                foreach (Capturable c in this.players.SelectMany<Player, Piece>(p => p.GetPieces()).OfType<Capturable>())
                    c.EarnedIncome = false;
            }

            ++turn;
        }

        private Dictionary<Type, Dictionary<Player, int>> GetPlayerCounts(IEnumerable<Type> capts)
        {
            var counts = new Dictionary<Type, Dictionary<Player, int>>();
            foreach (Type type in capts)
                counts.Add(type, new Dictionary<Player, int>());

            foreach (Player player in this.players)
            {
                int wizards, portals, cities, relics, units;
                player.GetCounts(out wizards, out portals, out cities, out relics, out units);

                counts[typeof(Relic)].Add(player, relics);
                counts[typeof(City)].Add(player, cities);
                counts[typeof(Portal)].Add(player, portals);
                counts[typeof(Wizard)].Add(player, wizards);
            }

            return counts;
        }
        private Player GetWinner(Dictionary<Type, Dictionary<Player, int>> counts)
        {
            //a player whens the game when, for all capturable types, they have more than the next highest player
            foreach (Player player in this.players)
                if (counts.Values.All(dict => dict[player] > dict.Where(pair => player != pair.Key).Max(pair => pair.Value)))
                    return player;
            return null;
        }

        private bool RemoveUnits(List<Player> dead, Dictionary<Type, Dictionary<Player, int>> counts)
        {
            //remove resources/units for players with no capturables
            bool removed = false;
            foreach (Player player in this.players)
                if (counts.All(pair => pair.Value[player] == 0))
                {
                    removed = true;
                    player.RemoveUnit();
                    if (player.Dead)
                        dead.Add(player);
                }
            return removed;
        }

        private void FreeUnit(bool removed)
        {
            Dictionary<string, string> units = new Dictionary<string, string>();
            foreach (string race in Races.Keys)
            {
                int baseCost;
                string addUnit;
                do
                    addUnit = Random.SelectValue(Races[race]);
                while (freeUnits[addUnit] / 2.6f > Random.Gaussian(baseCost = Unit.CreateTempUnit(this, addUnit).BaseTotalCost));
                freeUnits[addUnit] += Random.GaussianOEInt(65, 1, .21);
                if (freeUnits[addUnit] >= baseCost)
                    units.Add(race, addUnit);
            }
            //dont place free units when someone has no capturables
            if (!removed && units.Count > 0)
            {
                if (units.Count < Races.Count)
                {
                    Dictionary<string, string> forRaces = GetForRaces(Random.SelectValue(units.Values));
                    foreach (string race in Races.Keys)
                        if (!units.ContainsKey(race))
                            units.Add(race, forRaces[race]);
                }
                foreach (string unit in units.Values)
                    freeUnits[unit] -= Unit.CreateTempUnit(this, unit).BaseTotalCost;
                double avg = players.Average(player => Unit.CreateTempUnit(this, units[player.Race]).BaseTotalCost);
                foreach (Player player in players)
                    player.FreeUnit(units[player.Race], avg);
            }
        }
        private Dictionary<string, string> GetForRaces(string targetName)
        {
            Unit targetUnit = Unit.CreateTempUnit(this, targetName);
            double avgRaceTotal = freeUnits.Values.Sum() / (double)Races.Count;

            return Races.ToDictionary(race => race.Key, race =>
            {
                if (targetUnit.Race == race.Key)
                {
                    return targetName;
                }
                else
                {
                    double raceTotal = race.Value.Sum(name => freeUnits[name]);
                    double target = targetUnit.BaseTotalCost + (raceTotal - avgRaceTotal) / (double)race.Value.Length;
                    double minTarget = targetUnit.BaseTotalCost / 1.69;

                    bool isHigh = (target > targetUnit.BaseTotalCost);
                    Func<double> ReverseTarget = (() => 2 * targetUnit.BaseTotalCost - target);
                    if (isHigh)
                        target = ReverseTarget();
                    if (target < minTarget)
                        target = minTarget / (Math.Pow(1 + minTarget - target, .52));
                    if (isHigh)
                        target = ReverseTarget();

                    IDictionary<string, int> dict = null;
                    try
                    {
                        dict = race.Value.ToDictionary(name => name, name =>
                        {
                            double baseCost = Unit.CreateTempUnit(this, name).BaseTotalCost;

                            double chance = Math.Abs(target - baseCost) / target;
                            chance = 1 / (.039 + chance);
                            chance *= chance;

                            double pct = freeUnits[name] / baseCost;
                            if (pct >= 1)
                                pct *= 1.3 * pct;
                            if (pct < 0)
                                chance *= .052 / (1 - 6.5 * pct);
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

        private void RemoveCapturables(List<Player> dead, IEnumerable<Type> capts,
                Dictionary<Type, Dictionary<Player, int>> counts, Player win, bool removed)
        {
            if (win == null && !removed)
                foreach (Type type in capts)
                    if (counts[type].All(pair => pair.Value > 0))
                    {
                        RemoveCapturables(dead, type);
                        break;
                    }
        }
        private void RemoveCapturables(List<Player> dead, Type type)
        {
            //const double portalAvg = Player.WizardCost;
            double portalAvg = double.NaN;
            if (type == typeof(Portal))
            {
                //account for partially finished units
                portalAvg = Races.Values.SelectMany(units => units).Select(unit => Unit.CreateTempUnit(this, unit))
                        .Where(unit => unit.CostType != CostType.Production).Sum(unit => unit.BaseTotalCost);
                portalAvg *= .39 * Portal.ValuePct / (double)Races.Count / 5.0;

                portalAvg += Portal.AvgPortalCost;
            }

            foreach (Player player in players)
            {
                player.RemoveCapturable(type, portalAvg);
                if (player.Dead)
                    dead.Add(player);
            }
        }

        private void KillPlayers(List<Player> dead)
        {
            while (dead.Count > 0)
            {
                //if there are multiple dead players, kill them off in order based on the amount of resources they died with
                Player loser = null;
                double min = double.MaxValue;
                foreach (Player player in Random.Iterate(dead))
                {
                    double value = player.GetTotalResources();
                    if (value < min)
                    {
                        loser = player;
                        min = value;
                    }
                }

                dead.Remove(loser);
                loser.KillPlayer();
            }
        }

        private void WinGame(Player win)
        {
            //a single remaining player automatically wins
            if (players.Length == 1)
                win = players[0];

            if (win != null)
            {
                AddPlayer(winningPlayers, turn, win);
                RemovePlayer(win);

                if (players.Length == 1)
                    //if a single player if left, they lose
                    players[0].KillPlayer();
                else if (players.Length > 1)
                    //otherwise, remove all winning pieces from the game
                    foreach (Piece piece in win.GetPieces().ToList())
                    {
                        piece.Tile.Remove(piece);
                        win.Remove(piece, true);
                        if (piece is City && Game.Random.Bool())
                            MakeCitySpot(piece.Tile);
                    }
            }
        }

        private void ResetTiles()
        {
            foreach (Tile t in map)
                if (t != null)
                    t.Reset();
        }

        private void ChangeMap()
        {
            if (Random.Bool(.52))
            {
                int amt = Random.OEInt(MapSize / 26.0);
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

                    for (int a = 0; a < amt; ++a)
                    {
                        tile.Terrain = terrain;
                        tile = Random.SelectValue(tile.GetNeighbors());
                    }
                }
            }
        }

        private void CreateCitySpot()
        {
            int amt = Random.OEInt(Math.Sqrt(GetMapSize(Diameter - 2) / 910.0));
            for (int a = 0; a < amt; ++a)
            {
                //select a tile not on the map edge
                Tile tile = RandomTile(neighbor => neighbor != null);
                //don't try again if it is on or next to an existing city
                if (tile.GetNeighbors(true).All(neighbor => !neighbor.HasCity()))
                    MakeCitySpot(tile);
            }
        }

        private static void MakeCitySpot(Tile tile)
        {
            tile.MakeCitySpot(Random.GaussianOEInt(7.8, .39, .169, 1));
        }

        private void ChangeMoveOrder()
        {
            //a lower shuffleValue makes the move order change faster
            const double shuffleValue = .169;
            Dictionary<Player, int> bonus = MattUtil.TBSUtil.RandMoveOrder<Player>(Random, players, shuffleValue, (a, b) => true);
            foreach (KeyValuePair<Player, int> pair in bonus)
                AddMoveOrderDiff(pair.Key, pair.Value);
        }
        private void AddMoveOrderDiff(Player player, double diff)
        {
            //total difference between first and last moving player is worth 260 resources
            double amount = diff * 260.0 / (players.Length - 1.0);
            if (amount > 0)
            {
                double pts = Random.DoubleHalf(amount);
                player.CollectWizardPts(pts / 50.0);
                amount -= pts;
            }
            player.BalanceForUnit(amount, 0);
        }

        #endregion //increment turn

        #region create map

        private void CreateMap(int radius)
        {
            foreach (Point coord in Random.Iterate(Diameter, Diameter))
            {
                int x = coord.X, y = coord.Y;
                int nullTiles;
                if (y < Diameter / 2)
                    nullTiles = Diameter - radius - y;
                else
                    nullTiles = y - Diameter / 2;

                int compX = x - (radius % 2 == 1 && y % 2 == 1 ? 1 : 0);
                if (compX >= nullTiles / 2 && compX < Diameter - (nullTiles + 1) / 2)
                {
                    this.map[x, y] = new Tile(this, x, y);
                    this.map[x, y] = CreateTile(coord.X, coord.Y);
                }
            }
        }
        private Tile CreateTile(int x, int y)
        {
            Tile tile = null;
            //try three times to find a neighbor that has already been initialized
            for (int i = 0; i < 3; ++i)
            {
                tile = GetTile(x, y).GetTileIn(Random.Next(6));
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

        #endregion //create map

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            UnitSchema us;
            int numUnits;
            InitRaces(out us, out numUnits);
        }

        #endregion
    }
}
