using System;
using System.Collections.Generic;
using System.Linq;

namespace CityWar
{
    [Serializable]
    public abstract class Piece
    {
        #region fields and constructors

        public readonly Abilities Ability;
        public readonly int MaxMove;

        private readonly string name;

        protected Tile tile;
        protected Player owner;
        protected int movement;
        protected int group;

        protected Piece(int maxMove, Player owner, Tile tile, string name)
            : this(maxMove, owner, tile, name, Abilities.None)
        {
        }
        protected Piece(int maxMove, Player owner, Tile tile, string name, Abilities ability)
        {
            this.Ability = ability;
            this.MaxMove = maxMove;

            this.name = name;

            this.tile = tile;
            this.owner = owner;
            this.movement = 0;
            this.group = Game.NewGroup();
        }

        #endregion //fields and constructors

        #region public methods and properties

        public bool CanMove(Tile t)
        {
            if (tile.IsNeighbor(t))
            {
                Player occupying;
                if (!t.OccupiedByUnit(out occupying) || ( owner == occupying ))
                    return CanMoveChild(t);
            }
            return false;
        }

        public int Group
        {
            get
            {
                return group;
            }
            internal set
            {
                group = value;
            }
        }

        public Tile Tile
        {
            get
            {
                return tile;
            }
        }

        public int Movement
        {
            get
            {
                return movement;
            }
        }

        public Player Owner
        {
            get
            {
                return owner;
            }
        }

        private List<Tile> path = null;
        public List<Tile> Path
        {
            get
            {
                return ( this.path == null ? null : this.path.ToList() );
            }
            internal set
            {
                this.path = value;
            }
        }

        public override string ToString()
        {
            return name;
        }

        #endregion //public methods and properties

        #region moving

        internal bool Move(Tile t, bool gamble, out bool canUndo)
        {
            if (CanMove(t))
            {
                if (Path != null && ( Path.Count < 2 || Path[1] != t ))
                    Path = null;
                return DoMove(t, gamble, out canUndo);
            }
            canUndo = true;
            return false;
        }

        internal static Dictionary<Piece, bool> GroupMove(List<Piece> pieces, Tile t, bool gamble)
        {
            Dictionary<Piece, bool> undoPieces = new Dictionary<Piece, bool>(pieces.Count);

            if (pieces.Count < 2)
            {
                Piece move = pieces[0];
                bool canUndo;
                move.Move(t, gamble, out canUndo);
                undoPieces.Add(move, canUndo);
                return undoPieces;
            }

            List<Unit> units = null;
            bool anyUnits = false, unitsMoved = false;
            //move units as a group so they either all make it or all dont
            if (!t.HasCity() && ( t.Terrain == Terrain.Forest || t.Terrain == Terrain.Mountain ))
            {
                //collect units that may not make it
                units = new List<Unit>();
                foreach (Piece p in pieces)
                {
                    Unit u = p as Unit;
                    if (u != null && ( u.Type == UnitType.Amphibious || u.Type == UnitType.Ground ))
                        units.Add(u);
                }

                //move them, if any
                anyUnits = units.Count > 0;
                unitsMoved = ( anyUnits && Unit.UnitGroupMove(units, t, undoPieces, gamble) );
            }

            bool any = unitsMoved;
            //only move everyone else if the units made it, or there were none
            if (!anyUnits || unitsMoved)
                foreach (Piece p in Game.Random.Iterate<Piece>(pieces))
                    //move anyone that was not moved previously
                    if (units == null || !units.Contains(p as Unit))
                    {
                        bool canUndo;
                        p.Move(t, gamble, out canUndo);
                        undoPieces.Add(p, canUndo);
                    }

            return undoPieces;
        }

        internal void UndoMove(Tile from, int oldMove)
        {
            if (oldMove < 1)
                throw new Exception();

            movement = oldMove;

            if (tile != from)
            {
                tile.Remove(this);
                tile = from;
                tile.Add(this);
            }
            else
            {
                throw new Exception();
            }
        }

        #endregion //moving

        #region abstract members

        protected abstract bool CanMoveChild(Tile t);
        protected abstract bool DoMove(Tile t, bool gamble, out bool canUndo);
        internal abstract void ResetMove();
        internal abstract double Heal();
        internal abstract void UndoHeal(double v);

        #endregion //abstract members
    }

    [Serializable]
    public enum Abilities
    {
        AircraftCarrier,
        Aircraft,
        None
    }
}
