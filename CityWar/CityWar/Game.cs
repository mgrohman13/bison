using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using MattUtil;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

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

        public static readonly UnitTypes UnitTypes = new();
        public readonly int Diameter;

        private readonly Tile[,] map;
        private readonly Dictionary<Player, int> winningPlayers, defeatedPlayers;
        private readonly Dictionary<string, int> freeUnits;

        private Player[] players;
        private int turn, currentPlayer;

        private delegate Piece UndoDelegate(object[] args);
        private Stack<UndoDelegate> UndoCommands = new();
        private Stack<object[]> UndoArgs = new();
        private readonly Dictionary<Unit, List<Tile>> UnitTiles = new();
        private readonly Dictionary<Tile, List<Unit>> TileUnits = new();
        private readonly Dictionary<Piece, List<Piece>> AirHeal = new();

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
            //this.UnitTypes = new();
            this.Diameter = radius * 2 - 1;

            this.map = new Tile[Diameter, Diameter];
            this.winningPlayers = new();
            this.defeatedPlayers = new();

            int numPlayers = newPlayers.Length;
            this.players = new Player[numPlayers];

            this.turn = 1;
            this.currentPlayer = -1;

            this.freeUnits = InitRaces();

            //ensure the map is big enough for all the players
            const int HexesPerPlayer = 13;
            if (numPlayers * HexesPerPlayer > MapSize)
                throw new ArgumentOutOfRangeException("newPlayers", numPlayers, string.Format(
                        "Map is too small for that many players.  Must be at least {0} hexes.",
                        numPlayers * HexesPerPlayer));

            ClearUndos();

            CreateMap(radius);

            List<string> races = new();
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
                    dict => GetUnitNeeds(dict[player.Race])));

            foreach (string[] race in Races.Values)
                foreach (string name in race)
                    freeUnits[name] = GetInitUnitsHave(name);

            //initialize the players, half with cities and half with wizards
            bool city = Random.Bool();
            int addWork = 0;
            foreach (Player current in Random.Iterate(newPlayers))
            {
                string[] raceUnits = new string[3];
                for (int a = 0; a < 3; ++a)
                    raceUnits[a] = startUnits[a][current.Race];
                current.NewPlayer(this, city = !city, raceUnits, totalStartCost);
                addWork = Math.Max(addWork, (int)Math.Ceiling(current.GetTurnUpkeep()) - current.Work);
            }

            //randomize the turn order          
            foreach (Player current in Random.Iterate(newPlayers))
            {
                players[++currentPlayer] = current;
                current.SetStartOrder(currentPlayer);
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
                CreateTreasure();

            ResetTiles();

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
            int needed = GetUnitNeeds(name);
            //~1/28 will be outside [-needed,needed]
            return Random.GaussianInt(needed / 2.1);
        }
        private Dictionary<string, int> InitRaces()
        {
            //initialize units
            UnitSchema us = UnitTypes.GetSchema();
            int numUnits = us.Unit.Rows.Count;
            Dictionary<string, List<string>> tempRaces = new();
            Dictionary<string, int> unitsHave = new(numUnits);
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
            AirHeal.Clear();
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
                    || piece.Owner.Magic < 0 || piece.Owner.Relic < 0 || piece.Owner.Population < 0 || piece.Owner.GetTurnUpkeep() < 0 || piece.Owner.Work < 0)
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
                    currentPlayer = -1;
                    //a new round of turns
                    IncrementTurn();
                    currentPlayer = 0;
                }

                CurrentPlayer.StartTurn();
            }
        }

        public int AttackUnit(Battle b, Attack attack, Unit target, out double relic, out Tuple<Unit, int, int, double> splash)
        {
            splash = null;
            int retVal = attack.AttackUnit(target, attack.Owner.Owner == target.Owner.Game.CurrentPlayer, out relic);
            if (target.Dead)
                b.defenders.Remove(target);

            if (retVal > -1)
            {
                if (attack.Special == Attack.SpecialType.Splash)
                {
                    attack.Used = false;
                    var targets = b.defenders.Where(u => attack.CanAttack(u));
                    if (targets.Any())
                    {
                        double count = targets.Sum(u => u.IsThree ? Math.Sqrt(u.Attacks.Length) : 1);
                        double hp = targets.Sum(u => Math.Sqrt(u.Hits * u.MaxHits));
                        double chance = count / (3.9 + count) * hp / (130 + hp);
                        targets = targets.Where(u => u != target);
                        if (targets.Any() && Random.Bool(chance))
                        {
                            Unit splashTarget = Random.SelectValue(targets);
                            int oldHits = splashTarget.Hits;
                            int splashDmg = attack.AttackUnit(splashTarget, false, out double splashRelic);
                            if (splashDmg > 0)
                                splash = new Tuple<Unit, int, int, double>(splashTarget, splashDmg, oldHits, splashRelic);
                            if (splashTarget.Dead)
                                b.defenders.Remove(splashTarget);
                        }
                    }
                    attack.Used = true;
                }

                if (attack.Owner.Owner == CurrentPlayer)
                {
                    AddUnitTile(attack.Owner, attack.Owner.Tile);

                    List<Piece> removePieces = new() { attack.Owner, };
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
            if (target.OccupiedByUnit(out Player enemy) && enemy != CurrentPlayer)
            {
                HashSet<Unit> defenders = new();

                ////'Immobile' units defend first, by themselves
                //defenders.UnionWith(target.FindAllUnits(defender => defender.Type == UnitType.Immobile));
                //if (defenders.Count > 0)
                //{
                //    //attack with either just the selected units, or all within range, but do not add any additional defenders
                //    if (selected == null)
                //        selected = GetAttackers(target);
                //}
                //else
                if (selected == null)
                {
                    //collect all battle units recursively, starting with attackers that can target any units in the target tile
                    Dictionary<Unit, int> attackers = new();
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

                    //then collect defenders that can retaliate
                    foreach (Unit attacker in selected)
                    {
                        //find the minimum length attack the attacker might use
                        int length = int.MaxValue;
                        foreach (Unit defender in defenders)
                            if (CanTarget(attacker, defender, out int minLength))
                                length = Math.Min(length, minLength);
                        //add defenders that can retaliate at that length
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
            //find all adjacent attackers that might be able to participate
            foreach (Unit attacker in GetAttackers(defender.Tile))
                //check if the potential attacker can target the defender and is not already in the battle with an equal or shorter length weapon
                if (CanTarget(attacker, defender, out int minLength) && !(attackers.TryGetValue(attacker, out int hasLength) && hasLength <= minLength))
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
            return (attacker.Owner == CurrentPlayer && attacker.Movement > 0 && (!attacker.IsAir() || attacker.Fuel > 0));
        }
        private void AddDefenders(HashSet<Unit> defenders, Player enemy, Unit attacker, int length = int.MaxValue, Dictionary<Unit, int> attackers = null)
        {
            //find all adjacent defenders that can either retaliate against or be targeted by this attacker
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
            return CanTarget(unit, target, out _, length);
        }
        private static bool CanTarget(Unit unit, Unit target, out int minLength, int length = int.MinValue)
        {
            if (target.Type == UnitType.Immobile)
            {
                //immobile units can always be targeted but dont reduce the attacker's length 
                minLength = int.MaxValue;
                return true;
            }

            minLength = unit.Attacks.Where(attack => attack.Length >= length && attack.CanTarget(target))
                    .Select(attack => attack.Length).DefaultIfEmpty(int.MaxValue).Min();
            return (minLength != int.MaxValue);
        }

        public bool EndBattle(Battle b)
        {
            if (b.canRetaliate)
            {
                b.StartRetaliation();

                //consider the battle over if no one can retaliate
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
                    HealPieces(new[] { wizard });
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

            Piece piece = capt.BuildPiece(pieceName, out bool canUndo);
            if (piece != null)
                if (canUndo)
                {
                    Unit u = (piece as Unit);
                    if (u != null)
                        AddTileUnit(u.Tile, u);

                    UndoCommands.Push(UndoBuildPiece);
                    UndoArgs.Push(new[] { capt, piece });
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
            from.Occupied(out Player player);
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
            from.Occupied(out Player player);
            if (player != players[currentPlayer])
                return false;

            Tile to = map[x, y];

            Dictionary<Piece, int> oldMoves = new();
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
                    if (p is Unit u && u.Type != UnitType.Air)
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
                            if (p is Unit u)
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
            Dictionary<Piece, double> undoInfo = new();
            for (int i = 0; i < length; ++i)
            {
                Piece curPiece = selPieces[i];
                double info = curPiece.Heal();
                //weed out futile calls
                if (info > -1)
                {
                    any = true;
                    undoInfo.Add(curPiece, info);

                    //if this unit is an aircraft relying on a movable carrier to heal, block undoing the carrier if this unit ever can't undo
                    if (info < 1 && curPiece.IsAir())
                    {
                        IEnumerable<Piece> carriers = curPiece.Tile.FindAllPieces(p => p.IsAbility(Ability.AircraftCarrier));
                        if (carriers.Any())
                        {
                            AirHeal.TryGetValue(curPiece, out List<Piece> existing);
                            existing ??= new List<Piece>();
                            if (!carriers.Any(c =>
                                    c.MaxMove == 0 || existing.Contains(c)))
                            {
                                var priority = carriers.Where(c => AirHeal.Values.SelectMany(v => v).Contains(c));
                                if (carriers.Count() > 1)
                                    ;
                                if (priority.Any())
                                    carriers = priority;
                                if (carriers.Count() > 1)
                                    ;
                                AddAirHeal(curPiece, Random.SelectValue(carriers));
                            }
                        }
                        else
                            ;
                    }
                }
            }

            if (any)
            {
                UndoCommands.Push(UndoHealPieces);
                UndoArgs.Push(new[] { undoInfo });
            }
        }
        private Piece UndoHealPieces(object[] args)
        {
            Dictionary<Piece, double> undoInfo = (Dictionary<Piece, double>)args[0];

            bool wizCheck = false;
            List<Piece> dead = new();

            Piece piece = null;
            foreach (Piece p in undoInfo.Keys)
            {
                p.UndoHeal(undoInfo[p]);

                if (p is Unit u && u.Dead)
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
                    var element = piece.Tile.Terrain switch
                    {
                        Terrain.Forest => piece.Owner.Nature,
                        Terrain.Mountain => piece.Owner.Earth,
                        Terrain.Plains => piece.Owner.Air,
                        Terrain.Water => piece.Owner.Water,
                        _ => throw new Exception(),
                    };
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

        public void CollectTreasure(Unit unit)
        {
            if (players[currentPlayer] != unit.Owner)
                return;

            Treasure treasure = unit.Tile.Treasure;
            if (unit.CollectTreasure())
            {
                UndoCommands.Push(UndoCollectTreasure);
                UndoArgs.Push(new object[] { unit, treasure });
            }
        }
        private Piece UndoCollectTreasure(object[] args)
        {
            Unit unit = (Unit)args[0];
            Treasure treasure = (Treasure)args[1];

            if (unit.UndoCollectTreasure(treasure))
            {
                int stack = UndoCommands.Count;
                CollectTreasure(unit);
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

            Dictionary<Unit, Stack<double>> undoInfo = new();
            foreach (Unit unit in units)
                undoInfo.Add(unit, unit.Disband());

            UndoCommands.Push(UndoDisbandUnits);
            UndoArgs.Push(new[] { undoInfo });
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
            AddDict(UnitTiles, u, t);
        }
        private void AddTileUnit(Tile t, Unit u)
        {
            AddDict(TileUnits, t, u);
        }
        private void AddAirHeal(Piece air, Piece carrier)
        {
            AddDict(AirHeal, air, carrier);
        }
        private static void AddDict<K, V>(Dictionary<K, List<V>> dict, K key, V value)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, new List<V>());
            dict[key].Add(value);
        }
        private void RemoveUndosForTile(Tile tile)
        {
            if (TileUnits.TryGetValue(tile, out List<Unit> units))
                RemoveUndosForPieces(units);
        }
        private void RemoveUndos(int stack)
        {
            List<object> args = new();
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
            List<object> removeArgs = new();

            //cant undo any previous commands involving this unit
            List<UndoDelegate> newCommands = new();
            List<object[]> newArgs = new();
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

            Stack<UndoDelegate> newCommandStack = new();
            Stack<object[]> newArgStack = new();
            for (int i = newCommands.Count; --i > -1;)
            {
                newCommandStack.Push(newCommands[i]);
                newArgStack.Push(newArgs[i]);
            }
            UndoCommands = newCommandStack;
            UndoArgs = newArgStack;

            if (removeArgs.Count > 0)
                RemoveUndosForPiecesInArgs(removeArgs);

            var carriers = pieces.OfType<Piece>().Where(AirHeal.ContainsKey).SelectMany(p => AirHeal[p]).Distinct();
            if (carriers.Any())
                RemoveUndosForPiecesInArgs(carriers);
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
            if (arg is Piece p)
            {
                yield return p;
            }
            else if (arg is System.Collections.IDictionary dictionary)
            {
                foreach (Piece piece in FindAllPieces(dictionary.Keys))
                    yield return piece;
                foreach (Piece piece in FindAllPieces(dictionary.Values))
                    yield return piece;
            }
            else if (arg is System.Collections.IEnumerable enumberable)
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
            if (Math.Abs(ImageUtil.Zoom - zoom) > 1)
            {
                ImageUtil.Zoom = zoom;
                Player.ResetPics(this.players);
                Treasure.ResetPics();
            }
        }

        public ReadOnlyDictionary<Player, int> GetWon()
        {
            return new(winningPlayers);
        }
        public ReadOnlyDictionary<Player, int> GetLost()
        {
            return new(defeatedPlayers);
        }

        public int GetUnitHas(string name)
        {
            return freeUnits[name];
        }

        public int GetUnitNeeds(string name)
        {
            return Unit.CreateTempUnit(name).BaseTotalCost;
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
                    double score = winningPlayers.Values.Min();
                    return winningPlayers.Single(p => p.Value == score).Key;
                }
                if (currentPlayer == -1)
                    return null;
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
                int count = map.OfType<Tile>().Count(t => t.Treasure != null && t.Treasure.Type == Treasure.TreasureType.Wizard);
                double target = NumWizSpots();
                //if (count < target)
                //    target += Math.Pow(1 / (count + 1.0), 2.6); 
                double avg = Math.Pow((target + 1) / (count + 2.0), 1.3);
                create = Game.Random.GaussianOEInt(avg, .26, .13, count == 0 ? 1 : 0);
            }
            for (int a = 0; a < create; a++)
            {
                Tile tile;
                do
                    tile = RandomTile(neighbor => neighbor == null || !neighbor.HasWizard());
                while (tile.Treasure != null);
                tile.CreateTreasure(Treasure.TreasureType.Wizard);
                if (tile.Treasure == null)
                    throw new Exception();
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
            //when the last piece of a winning player is removed we will end up here
            if (!winningPlayers.ContainsKey(player))
                AddPlayer(defeatedPlayers, player);
        }

        private void AddPlayer(Dictionary<Player, int> dict, Player player)
        {
            RemovePlayer(player);
            dict.Add(player, turn);
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
            //priority by which capturables are removed
            var capts = new[] { typeof(Relic), typeof(City), typeof(Portal), typeof(Wizard) };

            var counts = GetPlayerCounts(capts);
            Player win = GetWinner(counts);
            bool removed = false;

            //these must happen in this order
            if (win == null)
                removed = RemoveUnits(counts);
            FreeUnit();
            if (win == null && !removed)
                RemoveCapturables(capts, counts);
            WinGame(win);

            if (players.Length > 0)
            {
                //the order of these should be irrelevant
                ResetTiles();
                ChangeMap();
                CreateTreasure();
                ChangeMoveOrder();
                Player.SubtractCommonUpkeep(this.players);
            }

            ++turn;
        }

        private Dictionary<Type, Dictionary<Player, int>> GetPlayerCounts(IEnumerable<Type> capts)
        {
            Dictionary<Type, Dictionary<Player, int>> counts = new();
            foreach (Type type in capts)
                counts.Add(type, new Dictionary<Player, int>());

            foreach (Player player in this.players)
            {
                player.GetCounts(out int wizards, out int portals, out int cities, out int relics, out _);

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

        private bool RemoveUnits(Dictionary<Type, Dictionary<Player, int>> counts)
        {
            //remove resources/units for players with no capturables
            bool removed = false;
            foreach (Player player in this.players)
                if (counts.All(pair => pair.Value[player] == 0))
                {
                    removed = true;
                    player.RemoveUnit();
                }
            return removed;
        }

        private void FreeUnit()
        {
            Dictionary<string, string> units = new();
            foreach (string race in Races.Keys)
            {
                int baseCost;
                string addUnit;
                do
                    addUnit = Random.SelectValue(Races[race]);
                while (freeUnits[addUnit] / 2.6f > Random.Gaussian(baseCost = GetUnitNeeds(addUnit)));
                freeUnits[addUnit] += Random.GaussianOEInt(65, 1, .21);
                if (freeUnits[addUnit] >= baseCost)
                    units.Add(race, addUnit);
            }
            //dont place free units when someone has no capturables
            if (units.Count > 0 && players.All(p => p.GetPieces().OfType<Capturable>().Any()))
            {
                if (units.Count < Races.Count)
                {
                    Dictionary<string, string> forRaces = GetForRaces(Random.SelectValue(units.Values));
                    foreach (string race in Races.Keys)
                        if (!units.ContainsKey(race))
                            units.Add(race, forRaces[race]);
                }
                foreach (string unit in units.Values)
                    freeUnits[unit] -= GetUnitNeeds(unit);
                double avg = players.Average(player => GetUnitNeeds(units[player.Race]));
                foreach (Player player in players)
                    player.FreeUnit(units[player.Race], avg);
            }
        }
        private Dictionary<string, string> GetForRaces(string targetName)
        {
            Unit targetUnit = Unit.CreateTempUnit(targetName);
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
                        target = minTarget / (Math.Pow(1 + 5.2 * (minTarget - target) / minTarget, .52));
                    if (isHigh)
                        target = ReverseTarget();

                    IDictionary<string, int> dict = race.Value.ToDictionary(name => name, name =>
                    {
                        double baseCost = GetUnitNeeds(name);

                        double chance = Math.Abs(target - baseCost) / target;
                        chance = 1 / (.039 + chance);
                        chance *= chance;

                        double pct = freeUnits[name] / baseCost;
                        if (pct >= 1)
                            pct *= 1.3 * pct;
                        if (pct <= 0)
                            chance *= .052 / (1 - 6.5 * pct);
                        else
                            chance *= .26 + pct;

                        return Random.Round(chance * short.MaxValue);
                    });
                    if (dict.Values.Any(v => v > 0))
                        return Random.SelectValue(dict);
                    else
                        return Random.SelectValue(race.Value);
                }
            });
        }

        private void RemoveCapturables(IEnumerable<Type> capts, Dictionary<Type, Dictionary<Player, int>> counts)
        {
            foreach (Type type in capts)
                if (counts[type].All(pair => pair.Value > 0))
                {
                    RemoveCapturables(type);
                    break;
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
                        .Where(unit => unit.CostType != CostType.Production).Sum(unit => unit.BaseTotalCost);
                portalAvg *= .39 * Portal.ValuePct / (double)Races.Count / 5.0;

                portalAvg += Portal.CostTotalAvg;
            }

            foreach (Player player in players)
                player.RemoveCapturable(type, portalAvg);
        }

        private void WinGame(Player win)
        {
            //a single remaining player automatically wins
            if (players.Length == 1)
                win = players[0];

            if (win != null)
            {
                AddPlayer(winningPlayers, win);

                if (players.Length == 1)
                    //if a single player is left, they lose
                    players[0].KillPlayer();
                else if (players.Length > 1)
                    //otherwise, remove all winning pieces from the game
                    foreach (Piece piece in Random.Iterate(win.GetPieces()))
                    {
                        piece.Tile.Remove(piece);
                        win.Remove(piece);
                        if (Game.Random.Bool())
                            if (piece is City)
                                piece.Tile.CreateTreasure(Treasure.TreasureType.City);
                            else if (piece is Wizard)
                                piece.Tile.CreateTreasure(Game.Random.Bool(.78) ? Treasure.TreasureType.Magic : Treasure.TreasureType.Wizard);
                            else if (piece is Portal && Game.Random.Bool(.91))
                                piece.Tile.CreateTreasure(Game.Random.Bool(.39) ? Treasure.TreasureType.Magic : Treasure.TreasureType.Wizard);
                            else if (piece is Relic && Game.Random.Bool(.65))
                                piece.Tile.CreateTreasure(Treasure.TreasureType.Relic);
                            else if (piece is Unit unit && Game.Random.Bool(unit.RandedCost / (unit.RandedCost + 1690)))
                                piece.Tile.CreateTreasure(Treasure.TreasureType.Unit);
                            else throw new Exception();
                    }
            }
        }

        private void ResetTiles()
        {
            foreach (Tile t in map)
                t?.Reset();
        }

        private void ChangeMap()
        {
            if (Random.Bool(.52))
            {
                int amt = Random.OEInt(MapSize / 26.0);
                if (amt > 0)
                {
                    Tile tile = RandomTile();
                    var terrain = Random.Next(6) switch
                    {
                        0 => Terrain.Forest,
                        1 => Terrain.Mountain,
                        2 => Terrain.Plains,
                        3 => Terrain.Water,
                        _ => tile.Terrain,
                    };
                    for (int a = 0; a < amt; ++a)
                    {
                        tile.Terrain = terrain;
                        tile = Random.SelectValue(tile.GetNeighbors());
                    }
                }
            }
        }

        private void CreateTreasure()
        {
            //cities cannot be on the edge so subtract 2 from diameter, also scale only with sqrt
            int cities = Random.OEInt(Math.Sqrt(GetMapSize(Diameter - 2) / 910.0));
            for (int a = 0; a < cities; ++a)
            {
                //select a tile not on the map edge
                Tile tile = RandomTile(neighbor => neighbor != null);
                //don't try again if it is on or next to an existing city 
                tile.CreateTreasure(Treasure.TreasureType.City);
            }

            int other = Random.OEInt(Math.Sqrt(GetMapSize(Diameter) / 910.0));
            for (int b = 0; b < other; ++b)
            {
                Tile tile = RandomTile();
                if (!tile.Occupied())
                {
                    Treasure.TreasureType type = Random.SelectValue(new[] {
                        Treasure.TreasureType.Magic, Treasure.TreasureType.Relic, Treasure.TreasureType.Unit });
                    tile.CreateTreasure(type);
                }
            }
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
                int pts = Random.RangeInt(0, Random.Round(amount / 50.0));
                player.CollectResources(false, pts);
                amount -= pts * 50;
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
            InitRaces();
        }

        #endregion
    }
}
