using System;
using System.Collections.Generic;
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
            : this(game, x, y, (Terrain)Game.Random.NextBits(2))
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
                terrain = value;
            }
        }

        public bool IsNeighbor(int x, int y)
        {
            return IsNeighbor(game.GetTile(x, y));
        }
        public bool IsNeighbor(Tile t)
        {
            for (int a = -1 ; ++a < 6 ; )
                if (neighbors[a] == t)
                    return true;

            return false;
        }
        public Tile GetNeighbor(int direction)
        {
            if (direction < 0 || direction > 5)
                return null;
            else
                return neighbors[direction];
        }

        public bool HasWizard()
        {
            foreach (Piece p in pieces)
                if (p is Wizard)
                    return true;

            return false;
        }

        public bool HasCity()
        {
            if (cityTime > 0)
                return true;

            foreach (Piece p in pieces)
                if (p is City)
                    return true;

            return false;
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
                string pieceName = centerPiece.Name;
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
            return Tile.FindAllPieces(pieces, delegate(Piece p)
                {
                    return ( p.Group == CurrentGroup );
                });
        }
        public Unit[] GetSelectedUnits()
        {
            return Tile.FindAllUnits(pieces, delegate(Unit unit)
                {
                    return ( unit.Group == CurrentGroup );
                });
        }
        public Unit[] GetAllUnits()
        {
            return Tile.FindAllUnits(pieces, delegate(Unit unit)
            {
                return true;
            });
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
            return terrain.ToString();
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
                else if (!city && ( p is City ))
                    city = true;

            if (unitType == UnitType.Water)
            {
                if (terrain != Terrain.Water)
                    armor -= 1;
            }
            else if (city)
            {
                armor += 1;
            }
            else if (unitType == UnitType.Ground || unitType == UnitType.Amphibious)
            {
                if (terrain == Terrain.Forest)
                    armor += 1;
                else if (terrain == Terrain.Mountain)
                    armor += 2;
                else if (terrain == Terrain.Water)
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

        internal bool CaptureCity(Piece piece)
        {
            --cityTime;
            madeCity = true;

            Player p = piece.Owner;
            //get people based on the number of turns remaining
            p.Spend(0, CostType.Production, Game.Random.Round(-Math.Sqrt(cityTime * 210)));
            //get a little bit of work as if the unit partially rested
            p.AddWork(GetCaptureCityWork(piece));

            if (cityTime == 0)
            {
                //get the city
                p.AddUpkeep(390);
                new City(p, this);

                cityTime = -1;
                return false;
            }
            return true;
        }
        internal void UndoCaptureCity(Piece piece)
        {
            Player p = piece.Owner;

            if (cityTime == -1)
            {
                p.AddUpkeep(-390);
                foreach (Piece city in pieces)
                    if (city is City)
                    {
                        Remove(city);
                        p.Remove(city, false);
                        break;
                    }

                cityTime = 0;
            }

            p.Spend(0, CostType.Production, Game.Random.Round(Math.Sqrt(cityTime * 210)));
            p.AddWork(-GetCaptureCityWork(piece));

            ++cityTime;
            madeCity = false;
        }
        private double GetCaptureCityWork(Piece p)
        {
            Unit u = p as Unit;
            return ( u == null ? Player.WorkMult * 10 : u.WorkRegen ) * p.MaxMove * .5;
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
            this.wizardPoints = FindDistance(delegate(Tile tile)
                {
                    return ( tile.HasWizard() || tile.wizardPoints > 0 );
                }) - 1;
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

        internal Dictionary<Piece, bool> MovePieces(Tile tile, bool group)
        {
            Piece[] selectedPieces;

            Player thisP, tileP;
            //check that there are any pieces selected and that the destination tile 
            //is a neighbor and is not occupied by an enemy unit
            if (!IsNeighbor(tile) || !Occupied(out thisP) || ( tile.OccupiedByUnit(out tileP) && thisP != tileP )
                || ( (Piece[])( selectedPieces = this.GetSelectedPieces() ) ).Length < 1)
                return null;

            if (group)
            {
                List<Piece> pieces = new List<Piece>(selectedPieces.Length);
                foreach (Piece cur in selectedPieces)
                    if (cur.Movement > 0)
                        pieces.Add(cur);

                //check if no one has any move left
                if (pieces.Count < 1)
                    return null;

                //only move pieces with move left
                return Piece.GroupMove(pieces, tile);
            }
            else
            {
                foreach (Piece p in Game.Random.Iterate<Piece>(pieces))
                    if (p.Group == currentGroup && p.Movement > 0)
                    {
                        bool canUndo;
                        if (p.Move(tile, out canUndo) || p.Movement == 0)
                        {
                            //just move a single piece
                            Dictionary<Piece, bool> undoPieces = new Dictionary<Piece, bool>(1);
                            undoPieces.Add(p, canUndo);
                            return undoPieces;
                        }
                    }
            }
            return null;
        }

        internal void Reset()
        {
            madeCity = false;
        }
        #endregion //internal methods

        #region piece sorting
        private Piece _centerPiece = null;
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

        private static int ComparePieces(Piece p1, Piece p2)
        {
            Unit u1 = p1 as Unit, u2 = p2 as Unit;
            int value = GetTypeValue(p2, u2) - GetTypeValue(p1, u1);
            //several levels of resolving ties
            if (value == 0 && u1 != null)
            {
                value = u2.MaxMove - u1.MaxMove;
                if (value == 0)
                {
                    value = (int)Math.Ceiling(u2.RandedCost - u1.RandedCost);
                    if (value == 0)
                        value = (int)Math.Ceiling(u2.GetHealthPct() - u1.GetHealthPct());
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
                        Unit unit = piece as Unit;
                        if (piece == MovedCarrier)
                            AddTilesInRange(CanGetCarrier, unit, MovedToTile, piece.Movement - MoveModifier);
                        else
                            AddTilesInRange(CanGetCarrier, unit, piece.Tile, piece.Movement);
                    }
            }

            //have to make a copy so it can be used in the anonymous method
            Dictionary<Tile, Tile> CanGetCarrierCopy = new Dictionary<Tile, Tile>(CanGetCarrier);

            //find the distance to the nearest tile where the aircraft can land
            return ( move < FindDistance(delegate(Tile tile)
                {
                    //check if a friendly carrier can move into the tile this turn
                    if (CanGetCarrierCopy.ContainsKey(tile))
                    {
                        return true;
                    }
                    else
                    {
                        //check to see if there is an enemy carrier but no defending units
                        List<Piece> tilePieces = tile.pieces;
                        int count = tilePieces.Count;
                        if (count > 0)
                        {
                            Player occupying = tilePieces[0].Owner;
                            if (occupying != owner)
                            {
                                bool hasCarrier = false;
                                foreach (Piece p in tilePieces)
                                {
                                    if (p is Unit)
                                        return false;
                                    else if (p.Abilty == Abilities.AircraftCarrier)
                                        hasCarrier = true;
                                }
                                return hasCarrier;
                            }
                        }
                        return false;
                    }
                }, move) );
        }
        private void AddTilesInRange(Dictionary<Tile, Tile> CanGetCarrier, Unit unit, Tile tile, int move)
        {
            if (--move > -1)
            {
                foreach (Tile t in tile.neighbors)
                    if (t != null)
                    {
                        int newMove = move;
                        Player occupying;
                        UnitType carrierType = unit == null ? UnitType.Air : unit.Type;
                        Terrain terrain = t.Terrain;
                        //check if the carrier can move onto the tile
                        if (!( tile.OccupiedByUnit(out occupying) && occupying != unit.Owner ) &&
                            ( carrierType == UnitType.Air || carrierType == UnitType.Immobile ||
                            ( ( carrierType == UnitType.Water || carrierType == UnitType.Amphibious ) && terrain == Terrain.Water ) ||
                            ( ( carrierType == UnitType.Ground || carrierType == UnitType.Amphibious ) && ( terrain == Terrain.Plains ||
                            ( --newMove > -1 && ( terrain == Terrain.Forest ||
                            ( --newMove > -1 && terrain == Terrain.Mountain ) ) ) ) ) ))
                            //if so, add the tile and check its neighbors
                            AddTilesInRange(CanGetCarrier, unit, t, newMove);
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
