using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace CityWar
{
    [Serializable]
    public class Tile
    {
        #region fields and constructors
        private readonly Game game;
        public readonly int x, y;

        private int currentGroup;
        private bool madeCity = false;
        private Terrain terrain;
        private List<Piece> pieces = new List<Piece>();
        private Tile[] neighbors = new Tile[6];
        private int wizardPoints = 0;
        private int cityTime = -1;

        internal Tile(Game game, int x, int y, Terrain terrain)
        {
            //mostly for validating the random terrain constructor
            if (terrain != CityWar.Terrain.Forest && terrain != CityWar.Terrain.Mountain &&
                    terrain != CityWar.Terrain.Plains && terrain != CityWar.Terrain.Water)
                throw new Exception();

            this.game = game;
            this.x = x;
            this.y = y;
            this.terrain = terrain;
        }
        internal Tile(Game game, int x, int y)
            : this(game, x, y, (Terrain)Game.Random.Next(4))
        {
        }
        #endregion //fields and constructors

        #region public methods and properties
        public int CurrentGroup
        {
            get
            {
                return currentGroup;
            }
            set
            {
                currentGroup = value;
            }
        }
        public bool MadeCity
        {
            get
            {
                return madeCity;
            }
        }
        public int WizardPoints
        {
            get
            {
                return wizardPoints;
            }
        }
        public int CityTime
        {
            get
            {
                return cityTime;
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
                        if (p is Relic)
                            ( (Relic)p ).ChangedTerrain(value);
                    terrain = value;
                }
            }
        }

        public bool IsNeighbor(int x, int y)
        {
            return IsNeighbor(game.GetTile(x, y));
        }
        public bool IsNeighbor(Tile t)
        {
            return neighbors.Contains(t);
        }
        public Tile GetNeighbor(int direction)
        {
            return neighbors[direction];
        }
        public IEnumerable<Tile> GetNeighbors(bool includeThis = false, bool includeNull = false)
        {
            IEnumerable<Tile> neighbors = Enumerable.Empty<Tile>().Concat(this.neighbors);
            if (!includeNull)
                neighbors = neighbors.Where(tile => tile != null);
            if (includeThis)
                neighbors = neighbors.Concat(new[] { this });
            return neighbors;
        }

        public bool HasWizard()
        {
            return pieces.OfType<Wizard>().Any();
        }
        public bool HasCity()
        {
            return ( cityTime > 0 || pieces.OfType<City>().Any() );
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
                if (( centerPiece is Capturable ) && this.OccupiedByUnit())
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
            return ( p.Group == CurrentGroup && p.Owner == game.CurrentPlayer );
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
            List<Piece> result = new List<Piece>();
            foreach (Piece p in pieces)
            {
                if (match(p))
                    result.Add(p);
            }
            return result.ToArray();
        }
        public static Unit[] FindAllUnits(IEnumerable<Piece> pieces, Predicate<Unit> match)
        {
            List<Unit> result = new List<Unit>();
            foreach (Piece p in pieces)
            {
                Unit u = p as Unit;
                if (u != null && match(u))
                    result.Add(u);
            }
            return result.ToArray();
        }

        public bool Occupied()
        {
            Player p;
            return Occupied(false, out p);
        }
        public bool Occupied(out Player occupying)
        {
            return Occupied(false, out occupying);
        }
        public bool OccupiedByUnit()
        {
            Player p;
            return Occupied(true, out p);
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
            Player player;
            Occupied(out player);
            if (game.CurrentPlayer != player)
                return;

            CurrentGroup = Game.NewGroup();
            foreach (Piece p in pieces)
                p.Group = CurrentGroup;
        }
        public void Ungroup()
        {
            Player player;
            Occupied(out player);
            if (game.CurrentPlayer != player)
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
                for ( ; a < count ; ++a)
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
                for (a = min ; a <= max ; ++a)
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
            return x * game.Height + y;
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
            bool canUndo;
            SortedInsert(pieces, p, ComparePieces);

            if (wizardPoints > 0 && ( p is Wizard ))
            {
                p.Owner.CollectWizardPts(wizardPoints);
                wizardPoints = 0;
                game.CreateWizardPts();
                canUndo = false;
            }

            hasCenterPiece = false;
            canUndo = true;
            return canUndo;
        }

        internal bool CaptureCity(Unit unit)
        {
            --cityTime;
            madeCity = true;

            Player player = unit.Owner;
            //get population based on the number of turns remaining
            player.Spend(0, CostType.Production, -GetCaptureCityPop());
            //get a little bit of work as if the unit partially rested
            player.AddWork(GetCaptureCityWork(unit));

            if (cityTime == 0)
            {
                //get the city
                player.AddUpkeep(390, .052);
                new City(player, this);

                cityTime = -1;
                return false;
            }
            return true;
        }
        internal void UndoCaptureCity(Unit unit)
        {
            Player player = unit.Owner;

            if (cityTime < 1)
                throw new Exception();

            player.Spend(0, CostType.Production, GetCaptureCityPop());
            player.AddWork(-GetCaptureCityWork(unit));

            ++cityTime;
            madeCity = false;
        }
        private int GetCaptureCityPop()
        {
            return Game.Random.GaussianCappedInt(Math.Sqrt(cityTime * 210), .065);
        }
        private static double GetCaptureCityWork(Unit u)
        {
            return u.WorkRegen * u.MaxMove * .52;
        }

        internal void SetupNeighbors()
        {
            for (int a = -1 ; ++a < neighbors.Length ; )
                neighbors[a] = game.GetTileIn(x, y, a);
        }

        internal bool CheckCapture(Player owner)
        {
            bool canUndo = true;
            foreach (Piece p in pieces.ToArray())
                if (p.Owner != owner)
                {
                    ( (Capturable)p ).Capture(owner);
                    canUndo = false;
                }
            return canUndo;
        }

        internal void MakeWizPts()
        {
            //the amount is one less than the distance to the nearest wizard or wizardpoints
            this.wizardPoints = FindDistance(tile => tile.HasWizard() || tile.wizardPoints > 0) - 1;
            //should not have tried to place too close to existing wizard points
            if (this.wizardPoints < 1)
                throw new Exception();
        }

        internal void MakeCitySpot(int time)
        {
            if (HasCity())
                throw new Exception();
            cityTime = time;
        }

        internal bool CollectWizPts(Player player)
        {
            if (this.WizardPoints > 0)
            {
                player.CollectWizardPts(this.WizardPoints);
                wizardPoints = 0;
                game.CreateWizardPts();
                return false;
            }
            return true;
        }

        internal bool HasCarrier()
        {
            foreach (Piece p in pieces)
                if (p.Abilty == Abilities.AircraftCarrier)
                    return true;

            return false;
        }

        public Dictionary<Piece, bool> MoveSelectedPieces(Tile tile, bool gamble)
        {
            if (CanMove(tile))
            {
                Piece[] selectedPieces = this.GetSelectedPieces();
                //only move pieces with move left
                List<Piece> pieces = new List<Piece>(selectedPieces.Length);
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
            if (p.Group == currentGroup && p.Movement > 0 && CanMove(tile))
            {
                bool canUndo;
                if (p.Move(tile, gamble, out canUndo) || p.Movement == 0)
                {
                    //just move a single piece
                    Dictionary<Piece, bool> undoPieces = new Dictionary<Piece, bool>(1);
                    undoPieces.Add(p, canUndo);
                    return undoPieces;
                }
            }
            return null;
        }
        private bool CanMove(Tile tile)
        {
            Player thisP, tileP;
            return ( IsNeighbor(tile) && Occupied(out thisP) &&
                    ( !tile.OccupiedByUnit(out tileP) || thisP == tileP ) && this.GetSelectedPieces().Length > 0 );
        }

        internal void Reset()
        {
            madeCity = false;
        }

        internal bool MatchesTerrain(CostType costType)
        {
            return MatchesTerrain(costType, this.Terrain);
        }
        internal static bool MatchesTerrain(CostType costType, Terrain terrain)
        {
            return ( ( terrain == Terrain.Forest && costType == CostType.Nature )
                    || ( terrain == Terrain.Mountain && costType == CostType.Earth )
                    || ( terrain == Terrain.Plains && costType == CostType.Air )
                    || ( terrain == Terrain.Water && costType == CostType.Water ) );
        }
        #endregion //internal methods

        #region piece sorting
        [NonSerialized]
        private Piece _centerPiece = null;
        [NonSerialized]
        internal bool hasCenterPiece = false;
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
            foreach (Piece p in pieces)
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
                foreach (Piece p in pieces)
                {
                    Portal portal;
                    if (( portal = p as Portal ) != null)
                    {
                        int portalCost = portal.PortalCost;
                        if (portalCost > cost)
                        {
                            cost = portalCost;
                            highest = portal;
                        }
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
            foreach (Piece p in pieces)
            {
                Unit unit;
                if (( unit = p as Unit ) != null)
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
                    if (unit.isThree)
                        tempCost *= unit.Attacks.Length / 3.0;
                    if (tempCost > cost)
                    {
                        cost = tempCost;
                        highestUnit = unit;
                    }
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
                index = ( max - min ) / 2 + min;
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
                if (( min == index ? compValue : compare(p, pieces[min]) ) > 0)
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
                switch (u.Abilty)
                {
                case Abilities.Aircraft:
                    return 6;
                case Abilities.AircraftCarrier:
                    return 5;
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
            Relic r;
            if (( r = piece as Relic ) != null)
            {
                return 100000000 + r.CanBuildCount;
            }
            City c;
            if (( c = piece as City ) != null)
            {
                return 400000000 + c.CanBuildCount;
            }
            Portal p;
            if (( p = piece as Portal ) != null)
            {
                return 700000000 + p.PortalCost;
            }
            Wizard w;
            if (( w = piece as Wizard ) != null)
            {
                return 1000000000 + w.CanBuildCount;
            }

            throw new Exception();
        }
        #endregion //piece sorting

        #region check aircraft
        //if a carrier is the piece being moved, these variables store information about the move
        public static Piece MovedCarrier;
        public static Tile MovedToTile;
        public static int MoveModifier;

        public bool CheckAircraft(int moveMod, Piece p, ref Dictionary<Tile, Tile> CanGetCarrier)
        {
            //returning true means the aircraft will die
            if (p.Abilty == Abilities.Aircraft)
            {
                int move = p.Movement - moveMod;
                if (move < 0)
                    move = 0;
                return ( CheckAircraft(move, p.Owner, ref CanGetCarrier) );
            }
            return false;
        }

        private bool CheckAircraft(int move, Player owner, ref Dictionary<Tile, Tile> CanGetCarrier)
        {
            //collect all tiles that contain or can be reached by friendly carriers this turn
            if (CanGetCarrier == null)
            {
                CanGetCarrier = new Dictionary<Tile, Tile>();
                foreach (Piece piece in owner.GetPieces())
                    if (piece.Abilty == Abilities.AircraftCarrier)
                    {
                        if (piece == MovedCarrier)
                            AddTilesInRange(CanGetCarrier, piece, MovedToTile, piece.Movement - MoveModifier);
                        else
                            AddTilesInRange(CanGetCarrier, piece, piece.Tile, piece.Movement);
                    }
            }

            //have to make a copy so it can be used in the anonymous method
            Dictionary<Tile, Tile> CanGetCarrierCopy = new Dictionary<Tile, Tile>(CanGetCarrier);

            //find the distance to the nearest tile where the aircraft can land
            //check if a friendly carrier can move into the tile this turn
            return ( move < FindDistance(tile => CanGetCarrierCopy.ContainsKey(tile), move) );
        }
        private void AddTilesInRange(Dictionary<Tile, Tile> CanGetCarrier, Piece piece, Tile tile, int move)
        {
            if (--move > -1)
            {
                foreach (Tile t in tile.neighbors)
                    if (t != null)
                    {
                        int newMove = move;
                        Player occupying;
                        UnitType carrierType = ( piece is Unit ? ( (Unit)piece ).Type : UnitType.Immobile );
                        Terrain terrain = t.Terrain;
                        //check if the carrier can move onto the tile
                        if (!( tile.OccupiedByUnit(out occupying) && occupying != piece.Owner ) &&
                            ( carrierType == UnitType.Air || carrierType == UnitType.Immobile ||
                            ( ( carrierType == UnitType.Water || carrierType == UnitType.Amphibious ) && terrain == Terrain.Water ) ||
                            ( ( carrierType == UnitType.Ground || carrierType == UnitType.Amphibious ) && ( terrain == Terrain.Plains ||
                            ( --newMove > -1 && ( terrain == Terrain.Forest ||
                            ( --newMove > -1 && terrain == Terrain.Mountain ) ) ) ) ) ))
                            //if so, add the tile and check its neighbors
                            AddTilesInRange(CanGetCarrier, piece, t, newMove);
                    }
            }

            //the carrier is either already here or can move here, so add the tile
            if (!CanGetCarrier.ContainsKey(tile))
                CanGetCarrier.Add(tile, null);
        }
        #endregion //check aircraft

        #region find distance
        private int FindDistance(Predicate<Tile> match)
        {
            return FindDistance(match, int.MaxValue);
        }
        private int FindDistance(Predicate<Tile> match, int stopAtDist)
        {
            if (match(this))
                return 0;
            if (stopAtDist < 1)
                return int.MaxValue;

            Dictionary<Tile, int> distances = new Dictionary<Tile, int>();
            distances.Add(this, 0);

            int dist = 0;
            while (dist < stopAtDist)
            {
                int checkDist = dist++;
                Tile[] keys = new Tile[distances.Count];
                distances.Keys.CopyTo(keys, 0);
                foreach (Tile t in keys)
                    //only check the tiles that were picked up at the previous distance
                    if (distances[t] == checkDist)
                        foreach (Tile tile in t.neighbors)
                            //dont rematch tiles or match null tiles
                            if (tile != null && !distances.ContainsKey(tile))
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
