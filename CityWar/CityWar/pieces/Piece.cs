using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CityWar
{
    [Serializable]
    public abstract class Piece
    {
        #region fields and constructors

        protected EnumFlags<Ability> abilities;
        public readonly int MaxMove;

        private readonly string name;

        protected Tile tile;
        protected Player owner;
        protected int movement;
        protected int group;

        protected Piece(int maxMove, Player owner, Tile tile, string name, Ability ability)
            : this(maxMove, owner, tile, name, new EnumFlags<Ability>(ability))
        {
        }
        protected Piece(int maxMove, Player owner, Tile tile, string name, EnumFlags<Ability> abilities)
        {
            this.abilities = abilities;
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
                if (!t.OccupiedByUnit(out Player occupying) || (owner == occupying))
                    return CanMoveChild(t);
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
                if (path != null && path[path.Count - 1] == tile)
                    path = null;
                return this.path?.ToList();
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

        public Boolean IsAir()
        {
            return IsAbility(Ability.Aircraft);
        }

        public bool IsAbility(Ability ability)
        {
            return abilities.Contains(ability);
        }

        public EnumFlags<Ability> Abilities
        {
            get
            {
                return new EnumFlags<Ability>(abilities);
            }
        }

        #endregion //public methods and properties

        #region moving

        internal bool Move(Tile t, bool gamble, out bool canUndo)
        {
            if (CanMove(t))
            {
                if (Path != null && (Path.Count < 2 || Path[1] != t))
                    Path = null;
                return DoMove(t, gamble, out canUndo);
            }
            canUndo = true;
            return false;
        }

        internal static Dictionary<Piece, bool> GroupMove(List<Piece> pieces, Tile t, bool gamble)
        {
            Dictionary<Piece, bool> undoPieces = new(pieces.Count);

            if (pieces.Count < 2)
            {
                Piece move = pieces[0];
                move.Move(t, gamble, out bool canUndo);
                undoPieces.Add(move, canUndo);
                return undoPieces;
            }

            List<Unit> units = null;
            bool anyUnits = false, unitsMoved = false;
            //move units as a group so they either all make it or all dont
            if (!t.HasCity() && (t.Terrain == Terrain.Forest || t.Terrain == Terrain.Mountain))
            {
                //collect units that may not make it
                units = new List<Unit>();
                foreach (Piece p in pieces)
                    if (p is Unit u && (u.Type == UnitType.Amphibious || u.Type == UnitType.Ground))
                        units.Add(u);

                //move them, if any
                anyUnits = units.Count > 0;
                unitsMoved = (anyUnits && Unit.UnitGroupMove(units, t, undoPieces, gamble));
            }

            //only move everyone else if the units made it, or there were none
            if (!anyUnits || unitsMoved)
                foreach (Piece p in Game.Random.Iterate<Piece>(pieces))
                    //move anyone that was not moved previously
                    if (units == null || !units.Contains(p as Unit))
                    {
                        p.Move(t, gamble, out bool canUndo);
                        undoPieces.Add(p, canUndo);
                    }

            return undoPieces;
        }

        internal void UndoMove(Tile from, int oldMove)
        {
            if (oldMove < 1)
                throw new Exception();

            if (IsAir())
                ((Unit)this).Fuel += oldMove - movement;
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
    public enum Ability
    {
        None = 0x0,
        AircraftCarrier = 0x1,
        Aircraft = 0x2,
        Shield = 0x4,
        Regen = 0x8,
        Submerged = 0x10,
    }
}
