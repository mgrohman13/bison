using System;
using System.Collections.Generic;

namespace CityWar
{
    [Serializable]
    public abstract class Piece
    {
        #region fields and constructors
        public readonly int MaxMove;

        protected Tile tile;
        protected Player owner;
        protected string name;
        protected int movement;
        protected Abilities ability = Abilities.None;
        protected int group;

        protected Piece(int maxMove, Player owner, Tile tile)
        {
            this.owner = owner;
            this.tile = tile;

            this.movement = 0;
            this.MaxMove = maxMove;

            group = Game.NewGroup();
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

        public Abilities Abilty
        {
            get
            {
                return ability;
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

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public override string ToString()
        {
            return name;
        }
        #endregion //public methods and properties

        #region internal methods
        internal bool GetCity()
        {
            if (movement < 1 || tile.CityTime < 0 || movement < MaxMove || tile.MadeCity)
                return false;

            movement = 0;
            return tile.CaptureCity(this);
        }
        internal void UndoGetCity()
        {
            movement = MaxMove;
            tile.UndoCaptureCity(this);
        }
        #endregion //internal methods

        #region moving
        internal bool Move(Tile t, out bool canUndo)
        {
            if (CanMove(t))
                return DoMove(t, out canUndo);
            canUndo = true;
            return false;
        }

        internal static Dictionary<Piece, bool> GroupMove(List<Piece> pieces, Tile t)
        {
            Dictionary<Piece, bool> undoPieces = new Dictionary<Piece, bool>(pieces.Count);

            if (pieces.Count < 2)
            {
                Piece move = pieces[0];
                bool canUndo;
                move.Move(t, out canUndo);
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
                unitsMoved = ( anyUnits && Unit.UnitGroupMove(units, t, undoPieces) );
            }

            bool any = unitsMoved;
            //only move everyone else if the units made it, or there were none
            if (!anyUnits || unitsMoved)
                foreach (Piece p in Game.Random.Iterate<Piece>(pieces))
                    //move anyone that was not moved previously
                    if (units == null || !units.Contains(p as Unit))
                    {
                        bool canUndo;
                        p.Move(t, out canUndo);
                        undoPieces.Add(p, canUndo);
                    }

            return undoPieces;
        }

        internal void UndoMove(Tile from, int oldMove)
        {
            if (oldMove < 1)
                throw new Exception();

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
            movement = oldMove;
        }
        #endregion //moving

        #region abstract members
        protected abstract bool CanMoveChild(Tile t);
        protected abstract bool DoMove(Tile t, out bool canUndo);
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
