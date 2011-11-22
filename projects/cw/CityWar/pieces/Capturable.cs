using System;
using System.Collections.Generic;
using System.Text;

namespace CityWar
{
    [Serializable]
    public abstract class Capturable : Piece
    {
        #region fields and constructors
        protected Capturable(int maxMove, Player owner, Tile tile) : base(maxMove, owner, tile)
        {
        }
        #endregion //fields and constructors

        #region public methods and properties
        public bool CanBuild(string name)
        {
            if (!CapableBuild(name))
                return false;

            if (name == "Wizard")
            {
                return ( owner.Magic >= Player.WizardCost );
            }
            else if (name.EndsWith(" Portal"))
            {
                CostType poralType = (CostType)Enum.Parse(typeof(CostType), name.Split(' ')[0]);
                int magic, element;
                Player.SplitPortalCost(Owner.Race, poralType, out magic, out element);

                return ( owner.Magic >= magic ) && ( owner.GetResource(poralType.ToString()) >= element );
            }

            Unit unit = Unit.CreateTempUnit(name);
            return ( owner.Population >= unit.BasePplCost && owner.GetResource(unit.costType.ToString()) >= unit.BaseOtherCost );
        }
        #endregion //public methods and properties

        #region internal methods
        internal virtual void Capture(Player p)
        {
            while (movement > 0)
                Heal();
            owner.LostCapt(this, p);
            owner = p;
            owner.Add(this);
        }

        internal Piece BuildPiece(string name, out bool canUndo)
        {
            if (CanBuild(name))
            {
                if (name == "Wizard")
                {
                    owner.SpendMagic(Player.WizardCost);
                    return new Wizard(owner, tile, out canUndo);
                }
                else if (name.EndsWith(" Portal"))
                {
                    CostType poralType = (CostType)Enum.Parse(typeof(CostType), name.Split(' ')[0]);
                    int magic, element;
                    Player.SplitPortalCost(Owner.Race, poralType, out magic, out element);

                    owner.SpendMagic(magic);
                    owner.Spend(element, poralType, 0);
                    canUndo = false;
                    return new Portal(owner, tile, poralType);
                }

                Unit unit = Unit.NewUnit(name, tile, owner);
                owner.Spend(unit.BaseOtherCost, unit.costType, unit.BasePplCost);
                canUndo = true;
                return unit;
            }
            canUndo = false;
            return null;
        }
        internal void UndoBuildPiece(Piece piece)
        {
            Portal portal;
            if (piece is Wizard)
            {
                owner.SpendMagic(-Player.WizardCost);
            }
            else if (( portal = piece as Portal ) != null)
            {
                CostType poralType = portal.PortalType;
                int magic, element;
                Player.SplitPortalCost(Owner.Race, poralType, out magic, out element);

                owner.SpendMagic(-magic);
                owner.Spend(-element, poralType, 0);
            }
            else
            {
                Unit unit = ( (Unit)piece );
                owner.Spend(-unit.BaseOtherCost, unit.costType, -unit.BasePplCost);
                owner.Remove(unit, false);
                tile.Remove(unit);
            }

            piece.Tile.Remove(piece);
            piece.Owner.Remove(piece, false);
        }
        #endregion //internal methods

        #region abstract members
        public abstract bool CapableBuild(string name);
        protected bool raceCheck(string name)
        {
            if (name == "Wizard")
                return true;
            if (name.EndsWith(" Portal"))
                return true;
            return raceCheck(Unit.CreateTempUnit(name));
        }
        protected bool raceCheck(Unit temp)
        {
            return temp.Race == owner.Race;
        }
        #endregion //abstract members
    }
}
