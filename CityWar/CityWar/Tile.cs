using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CityWar
{
    [Serializable]
    public class Tile
    {
        #region fields and constructors

        [NonSerialized]
        private int group;

        public readonly Game Game;
        public readonly int X, Y;

        private readonly List<Piece> pieces;

        private Terrain terrain;
        private Treasure treasure;

        internal Tile(Game game, int x, int y)
            : this(game, x, y, (Terrain)Game.Random.Next(4))
        {
        }
        internal Tile(Game game, int x, int y, Terrain terrain)
        {
            this.group = Game.NewGroup();

            this.Game = game;
            this.X = x;
            this.Y = y;

            this.pieces = new List<Piece>();

            this.terrain = terrain;
            this.treasure = null;
        }

        #endregion //fields and constructors

        #region public methods and properties

        public int CurrentGroup
        {
            get
            {
                return group;
            }
            set
            {
                group = value;
            }
        }
        public Treasure Treasure
        {
            get
            {
                return treasure;
            }
        }
        public Terrain Terrain
        {
            get
            {
                return terrain;
            }
            internal set
            {
                if (terrain != value)
                {
                    foreach (Piece p in this.pieces)
                        if (p is Relic relic)
                            relic.ChangedTerrain(value);
                    terrain = value;
                }
            }
        }

        public bool IsNeighbor(int x, int y)
        {
            Tile neighbor = Game.GetTile(x, y);
            return (neighbor != null && IsNeighbor(neighbor));
        }
        public bool IsNeighbor(Tile t)
        {
            return GetNeighbors().Contains(t);
        }
        public IEnumerable<Tile> GetNeighbors(bool includeThis = false, bool includeNull = false)
        {
            IEnumerable<Tile> neighbors = Enumerable.Empty<Tile>().Concat(Enumerable.Range(0, 6).Select(dir => GetTileIn(dir)));
            if (!includeNull)
                neighbors = neighbors.Where(tile => tile != null);
            if (includeThis)
                neighbors = neighbors.Concat(new[] { this });
            return neighbors;
        }

        internal Tile GetTileIn(int direction)
        {
            int x = X, y = Y;
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

            if (x < 0 || x >= Game.Diameter || y < 0 || y >= Game.Diameter)
                return null;
            else
                return Game.GetTile(x, y);
        }

        public bool HasWizard()
        {
            return Has<Wizard>(Treasure.TreasureType.Wizard);
        }
        public bool HasCity()
        {
            return Has<City>(Treasure.TreasureType.City);
        }
        private bool Has<T>(Treasure.TreasureType type) where T : Piece
        {
            return (treasure != null && treasure.Type == type) || pieces.OfType<T>().Any();
        }

        public Image GetPieceImage()
        {
            Piece centerPiece = this.CenterPiece;

            if (centerPiece == null)
            {
                return null;
            }
            else
            {
                string pieceName = centerPiece.ToString();
                //a dot is placed on the image for a capturable if there are any units with it
                if ((centerPiece is Capturable) && this.OccupiedByUnit())
                    pieceName += "Unit";
                return centerPiece.Owner.GetPic(pieceName);
            }
        }

        public Piece[] GetAllPieces()
        {
            return pieces.ToArray();
        }
        public Piece[] GetSelectedPieces()
        {
            return Tile.FindAllPieces(pieces, IsSelected);
        }
        public Unit[] GetSelectedUnits()
        {
            return Tile.FindAllUnits(pieces, IsSelected);
        }
        private bool IsSelected(Piece p)
        {
            return (p.Group == CurrentGroup && p.Owner == Game.CurrentPlayer);
        }
        public Unit[] GetAllUnits()
        {
            return Tile.FindAllUnits(pieces, unit => true);
        }
        public Piece[] FindAllPieces(Predicate<Piece> match)
        {
            return Tile.FindAllPieces(pieces, match);
        }
        public Unit[] FindAllUnits(Predicate<Unit> match)
        {
            return Tile.FindAllUnits(pieces, match);
        }
        public static Piece[] FindAllPieces(IEnumerable<Piece> pieces, Predicate<Piece> match)
        {
            List<Piece> result = new();
            foreach (Piece p in pieces)
            {
                if (match(p))
                    result.Add(p);
            }
            return result.ToArray();
        }
        public static Unit[] FindAllUnits(IEnumerable<Piece> pieces, Predicate<Unit> match)
        {
            List<Unit> result = new();
            foreach (Piece p in pieces)
            {
                if (p is Unit u && match(u))
                    result.Add(u);
            }
            return result.ToArray();
        }

        public bool Occupied()
        {
            return Occupied(false, out _);
        }
        public bool Occupied(out Player occupying)
        {
            return Occupied(false, out occupying);
        }
        public bool OccupiedByUnit()
        {
            return Occupied(true, out _);
        }
        public bool OccupiedByUnit(out Player occupying)
        {
            return Occupied(true, out occupying);
        }
        private bool Occupied(bool unit, out Player occupying)
        {
            foreach (Piece p in pieces)
                if (!unit || p is Unit)
                {
                    occupying = p.Owner;
                    return true;
                }

            occupying = null;
            return false;
        }

        public void Group()
        {
            Occupied(out Player player);
            if (Game.CurrentPlayer != player)
                return;

            CurrentGroup = Game.NewGroup();
            foreach (Piece p in pieces)
                p.Group = CurrentGroup;
        }
        public void Ungroup()
        {
            Occupied(out Player player);
            if (Game.CurrentPlayer != player)
                return;

            int newGroup = CurrentGroup;
            foreach (Piece p in pieces)
                if (p.Group == CurrentGroup)
                {
                    p.Group = Game.NewGroup();
                    //select the first piece
                    if (newGroup == CurrentGroup && p.Movement > 0)
                        newGroup = p.Group;
                }
            CurrentGroup = newGroup;
        }

        public void ClickOn(Piece p, bool shift, bool ctrl, bool alt)
        {
            int count = pieces.Count;

            if (shift)
            {
                //find the first selected piece
                int first = -1;
                int a = -1;
                while (++a < count)
                    if (pieces[a].Group == CurrentGroup)
                    {
                        first = a;
                        break;
                    }

                //find the last selected piece
                int last = -1;
                for (; a < count; ++a)
                    if (pieces[a].Group == CurrentGroup)
                        last = a;

                //get the clicked on index
                int index = pieces.IndexOf(p);

                int min = Math.Min(first, index);
                int max = Math.Max(last, index);

                if (first == -1)
                {
                    min = index;
                    max = index;
                }

                //select all pieces in between
                CurrentGroup = Game.NewGroup();
                for (a = min; a <= max; ++a)
                    pieces[a].Group = CurrentGroup;
            }
            else if (ctrl)
            {
                if (p.Group == CurrentGroup)
                    p.Group = Game.NewGroup();
                else
                    p.Group = CurrentGroup;
            }
            else if (alt)
            {
                CurrentGroup = Game.NewGroup();
                p.Group = CurrentGroup;
            }
            else
            {
                this.CurrentGroup = p.Group;
            }
        }

        public void Select()
        {
            Piece select = null;
            foreach (Piece p in pieces)
                if (p.Movement > 0)
                {
                    if (CurrentGroup == p.Group)
                        return;
                    else if (select == null)
                        select = p;
                }

            if (select != null)
            {
                CurrentGroup = select.Group;
            }
        }

        public override string ToString()
        {
            return Terrain.ToString();
        }
        public override int GetHashCode()
        {
            return X * Game.Diameter + Y;
        }

        #endregion //public methods and properties

        #region internal methods

        internal int GetArmorBonus(UnitType unitType)
        {
            int armor = 0;

            bool city = false;
            foreach (Piece p in pieces)
                if (p is Wizard)
                    armor += 1;
                else if (p is City)
                    city = true;

            if (unitType == UnitType.Water)
            {
                if (Terrain != Terrain.Water)
                    armor -= 1;
            }
            else if (city)
            {
                armor += 1;
            }
            else if (unitType == UnitType.Ground || unitType == UnitType.Amphibious)
            {
                if (Terrain == Terrain.Forest)
                    armor += 1;
                else if (Terrain == Terrain.Mountain)
                    armor += 2;
                else if (Terrain == Terrain.Water)
                    if (unitType == UnitType.Amphibious)
                        armor += 1;
                    else
                        armor -= 1;
            }

            return armor;
        }

        internal void Remove(Piece p)
        {
            pieces.Remove(p);
            hasCenterPiece = false;
        }

        internal bool Add(Piece p)
        {
            SortedInsert(pieces, p, ComparePieces);
            hasCenterPiece = false;

            if (treasure != null && treasure.MoveTo(this, p))
            {
                treasure = null;
                return false;
            }
            return true;
        }

        internal bool CollectTreasure(Unit unit)
        {
            if (treasure.Collect(this, unit, out bool canUndo))
                treasure = null;
            return canUndo;
        }
        internal bool UndoCollectTreasure(Unit unit, Treasure treasure)
        {
            if (this.treasure == null)
                if (Treasure.CanCreate(this, treasure.Type))
                    this.treasure = treasure;
                else
                    return false;
            else if (this.treasure != treasure)
                return false;
            return treasure.UndoCollect(unit);
        }

        internal bool CheckCapture(Player owner)
        {
            bool canUndo = true;
            foreach (Piece p in pieces.ToArray())
                if (p.Owner != owner)
                {
                    ((Capturable)p).Capture(owner);
                    canUndo = false;
                }
            return canUndo;
        }

        internal void CreateTreasure(Treasure.TreasureType type)
        {
            if (treasure != null && (treasure.Type == type || Game.Random.Bool()))
                treasure.AddTo();
            else if (treasure != null)
                ;
            if (treasure == null && Treasure.CanCreate(this, type))
                treasure = new Treasure(this, type);
        }

        public bool HasCarrier()
        {
            return pieces.Any(p => p.IsAbility(Ability.AircraftCarrier));
        }

        public Dictionary<Piece, bool> MoveSelectedPieces(Tile tile, bool gamble)
        {
            if (CanMove(tile))
            {
                Piece[] selectedPieces = this.GetSelectedPieces();
                //only move pieces with move left
                List<Piece> pieces = new(selectedPieces.Length);
                foreach (Piece cur in selectedPieces)
                    if (cur.Movement > 0)
                        pieces.Add(cur);

                if (pieces.Count > 0)
                    return Piece.GroupMove(pieces, tile, gamble);
            }
            return null;
        }
        public Dictionary<Piece, bool> MovePiece(Piece p, Tile tile, bool gamble)
        {
            if (p.Group == group && p.Movement > 0 && CanMove(tile))
                if (p.Move(tile, gamble, out bool canUndo) || p.Movement == 0)
                {
                    //just move a single piece
                    Dictionary<Piece, bool> undoPieces = new(1);
                    undoPieces.Add(p, canUndo);
                    return undoPieces;
                }
            return null;
        }
        private bool CanMove(Tile tile)
        {
            return (IsNeighbor(tile) && Occupied(out Player thisP) &&
                    (!tile.OccupiedByUnit(out Player tileP) || thisP == tileP) && this.GetSelectedPieces().Length > 0);
        }

        internal void Reset()
        {
            if (treasure != null)
                treasure.Reset();
            foreach (Capturable c in this.pieces.OfType<Capturable>())
                c.EarnedIncome = false;
        }

        internal bool MatchesTerrain(CostType costType)
        {
            return MatchesTerrain(costType, this.Terrain);
        }
        internal static bool MatchesTerrain(CostType costType, Terrain terrain)
        {
            return ((terrain == Terrain.Forest && costType == CostType.Nature)
                    || (terrain == Terrain.Mountain && costType == CostType.Earth)
                    || (terrain == Terrain.Plains && costType == CostType.Air)
                    || (terrain == Terrain.Water && costType == CostType.Water));
        }

        #endregion //internal methods

        #region piece sorting

        [NonSerialized]
        private Piece _centerPiece;
        [NonSerialized]
        internal bool hasCenterPiece;
        //this is the piece that is drawn on the map for the tile
        private Piece CenterPiece
        {
            get
            {
                if (!hasCenterPiece)
                {
                    _centerPiece = FindCenterPiece(pieces);
                    hasCenterPiece = true;
                }

                return _centerPiece;
            }
        }
        internal static Piece FindCenterPiece(List<Piece> pieces)
        {
            //wizard first
            Piece city = null, relic = null;
            bool hasPortal = false;
            foreach (Piece p in Game.Random.Iterate(pieces))
                if (p is Wizard)
                    return p;
                else if (p is City)
                    city = p;
                else if (p is Relic)
                    relic = p;
                else if (p is Portal)
                    hasPortal = true;

            //then portal
            double cost = -1;
            if (hasPortal)
            {
                Portal highest = null;
                foreach (Piece p in Game.Random.Iterate(pieces))
                    if (p is Portal portal)
                    {
                        int portalCost = portal.Cost;
                        if (portalCost > cost)
                        {
                            cost = portalCost;
                            highest = portal;
                        }
                    }
                return highest;
            }

            //then city or relic
            if (city != null)
                return city;
            if (relic != null)
                return relic;

            //unit last
            bool all = false;
            cost = -1;
            Unit highestUnit = null;
            foreach (Piece p in Game.Random.Iterate(pieces))
                if (p is Unit unit)
                {
                    //unit type all takes precedence
                    if (unit.Type == UnitType.Immobile)
                    {
                        if (!all)
                        {
                            all = true;
                            cost = -1;
                            highestUnit = null;
                        }
                    }
                    else if (all)
                        continue;

                    //find the unit with the highest cost
                    double tempCost = unit.RandedCost;
                    if (unit.IsThree)
                        tempCost *= unit.Attacks.Length / 3.0;
                    if (tempCost > cost)
                    {
                        cost = tempCost;
                        highestUnit = unit;
                    }
                }

            return highestUnit;
        }

        internal void AdjustPiece(Piece p)
        {
            this.pieces.Remove(p);
            SortedInsert(this.pieces, p, ComparePieces);
        }

        public static void SortedInsert(List<Piece> pieces, Piece p, Comparison<Piece> compare)
        {
            //this is based off of a binary search algorithm
            int count = pieces.Count;
            int min = 0, max = count;
            int index = -1, compValue = -1;
            while (min < max)
            {
                index = (max - min) / 2 + min;
                compValue = compare(p, pieces[index]);
                if (compValue < 0)
                {
                    max = index - 1;
                }
                else if (compValue > 0)
                {
                    min = index + 1;
                }
                else
                {
                    //we found a piece that is comparatively equal to the new one
                    pieces.Insert(index, p);
                    return;
                }
            }
            if (min < count)
            {
                //if min was the index in the last iteration we already have the compare result and dont need to call again
                if ((min == index ? compValue : compare(p, pieces[min])) > 0)
                    ++min;
                pieces.Insert(min, p);
            }
            else
                pieces.Add(p);
        }

        public static int ComparePieces(Piece p1, Piece p2)
        {
            Unit u1 = p1 as Unit, u2 = p2 as Unit;
            int value = GetTypeValue(p2, u2) - GetTypeValue(p1, u1);
            //several levels of resolving ties
            if (value == 0 && u1 != null)
            {
                value = u2.MaxMove - u1.MaxMove;
                if (value == 0)
                {
                    value = Math.Sign(u2.RandedCost - u1.RandedCost);
                    if (value == 0)
                    {
                        value = Math.Sign(u2.GetHealthPct() - u1.GetHealthPct());
                        if (value == 0)
                            value = p1.Owner.GetPieces().IndexOf(p1) - p2.Owner.GetPieces().IndexOf(p2);
                    }
                }
            }
            return value;
        }
        private static int GetTypeValue(Piece piece, Unit u)
        {
            if (u != null)
            {
                //higher values show up higher in the list
                if (u.Type == UnitType.Immobile)
                    return 7;
                foreach (Ability a in u.Abilities)
                    switch (a)
                    {
                        case Ability.Aircraft:
                            return 5;
                        case Ability.AircraftCarrier:
                            return 6;
                    }
                switch (u.Type)
                {
                    case UnitType.Air:
                        return 4;
                    case UnitType.Ground:
                        return 3;
                    case UnitType.Amphibious:
                        return 2;
                    case UnitType.Water:
                        return 1;
                }
            }

            //give ample padding to make sure wizards are always above portals, etc.
            if (piece is Relic r)
                return 100000000 + r.CanBuildCount;
            if (piece is City c)
                return 400000000 + c.CanBuildCount;
            if (piece is Portal p)
                return 700000000 + p.Cost;
            if (piece is Wizard w)
                return 1000000000 + w.CanBuildCount;

            throw new Exception();
        }

        #endregion //piece sorting

        #region check aircraft

        //if a carrier is the piece being moved, these variables store information about the move
        public static Piece MovedCarrier;
        public static Tile MovedToTile;
        public static int MoveModifier;

        public bool CheckAircraft(int moveMod, int fuelMod, Piece p, ref Dictionary<Tile, Tile> CanGetCarrier)
        {
            //returning true means the aircraft will die
            if (p.IsAir())
            {
                int fuel = ((Unit)p).Fuel - fuelMod;
                if (fuel < 0)
                    fuel = 0;
                int move = p.Movement - moveMod;
                if (move < 0)
                    move = 0;
                return (CheckAircraft(fuel, move, p.MaxMove, p.Owner, ref CanGetCarrier));
            }
            return false;
        }

        private bool CheckAircraft(int fuel, int move, int maxMove, Player owner, ref Dictionary<Tile, Tile> CanGetCarrier)
        {
            //collect all tiles that contain or can be reached by friendly carriers this turn
            if (CanGetCarrier == null)
            {
                int turns = (fuel - move + maxMove) / (maxMove + 1);
                CanGetCarrier = new Dictionary<Tile, Tile>();
                foreach (Piece piece in owner.GetPieces())
                    if (piece.IsAbility(Ability.AircraftCarrier))
                    {
                        int carrierMove = piece.Movement + piece.MaxMove * turns;
                        if (piece == MovedCarrier)
                            AddTilesInRange(CanGetCarrier, piece, MovedToTile, carrierMove - MoveModifier);
                        else
                            AddTilesInRange(CanGetCarrier, piece, piece.Tile, carrierMove);
                    }
            }

            //have to make a copy so it can be used in the anonymous method
            Dictionary<Tile, Tile> CanGetCarrierCopy = new(CanGetCarrier);

            //find the distance to the nearest tile where the aircraft can land
            //check if a friendly carrier can move into the tile this turn
            int distance = FindDistance(tile => CanGetCarrierCopy.ContainsKey(tile), fuel);
            if (distance > move)
                fuel--;
            fuel -= (distance - move - 1) / maxMove;
            return (fuel < distance);
        }
        private void AddTilesInRange(Dictionary<Tile, Tile> CanGetCarrier, Piece piece, Tile carrierTile, int move)
        {
            if (--move > -1)
            {
                foreach (Tile neightbor in carrierTile.GetNeighbors())
                    if (neightbor != null)
                    {
                        int newMove = move;
                        UnitType carrierType = (piece is Unit unit ? unit.Type : UnitType.Immobile);
                        Terrain terrain = neightbor.Terrain;
                        //check if the carrier can move onto the tile
                        if (!(neightbor.OccupiedByUnit(out Player occupying) && occupying != piece.Owner) &&
                            (carrierType == UnitType.Air || carrierType == UnitType.Immobile ||
                            ((carrierType == UnitType.Water || carrierType == UnitType.Amphibious) && terrain == Terrain.Water) ||
                            ((carrierType == UnitType.Ground || carrierType == UnitType.Amphibious) && (terrain == Terrain.Plains ||
                            (--newMove > -1 && (terrain == Terrain.Forest ||
                            (--newMove > -1 && terrain == Terrain.Mountain)))))))
                            //if so, add the tile and check its neighbors
                            AddTilesInRange(CanGetCarrier, piece, neightbor, newMove);
                    }
            }

            //the carrier is either already here or can move here, so add the tile
            if (!CanGetCarrier.ContainsKey(carrierTile))
                CanGetCarrier.Add(carrierTile, null);
        }

        #endregion //check aircraft

        #region find distance

        internal int FindDistance(Predicate<Tile> match)
        {
            return FindDistance(match, int.MaxValue);
        }
        private int FindDistance(Predicate<Tile> match, int stopAtDist)
        {
            if (match(this))
                return 0;

            Dictionary<Tile, int> distances = new() { { this, 0 } };
            int dist = 0;
            while (dist < stopAtDist)
            {
                //only check the tiles that were picked up at the previous distance and dont rematch tiles or match null tiles
                List<Tile> newTiles = distances.Where(pair => pair.Value == dist).SelectMany(pair => pair.Key.GetNeighbors())
                        .Where(tile => tile != null && !distances.ContainsKey(tile)).Distinct().ToList();
                ++dist;
                //check new tiles for a match
                foreach (Tile tile in newTiles)
                    if (match(tile))
                        return dist;
                    else
                        distances.Add(tile, dist);
            }

            return int.MaxValue;
        }

        #endregion //find distance
    }

    [Serializable]
    public enum Terrain
    {
        Plains,
        Mountain,
        Water,
        Forest
    }
}
