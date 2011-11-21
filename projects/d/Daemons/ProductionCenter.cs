using System;
using System.Collections.Generic;
using System.Text;

namespace Daemons
{
    [Serializable]
    public class ProductionCenter
    {
        internal int x;
        internal int y;
        internal readonly ProductionType type;
        internal bool used = false;

        public ProductionCenter(Tile tile, int a)
        {
            this.x = tile.X;
            this.y = tile.Y;

            switch (a)
            {
            case 0:
                this.type = ProductionType.Knight;
                break;
            case 1:
                this.type = ProductionType.Archer;
                break;
            case 2:
                this.type = ProductionType.Infantry;
                break;

            default:
                throw new Exception("");
            }
        }

        public bool Used
        {
            get
            {
                return used;
            }
        }

        public ProductionType Type
        {
            get
            {
                return type;
            }
        }

        internal void Use(Player owner)
        {
            this.used = true;

            UnitType unitType;
            switch (this.type)
            {
            case ProductionType.Archer:
                unitType = UnitType.Archer;
                break;
            case ProductionType.Infantry:
                unitType = UnitType.Infantry;
                break;
            case ProductionType.Knight:
                unitType = UnitType.Knight;
                break;
            default:
                throw new Exception("die");
            }

            new Unit(unitType, owner.Game.GetTile(this.x, this.y), owner);
        }

        public override string ToString()
        {
            return type.ToString();
        }
    }

    public enum ProductionType
    {
        Infantry,
        Archer,
        Knight
    }
}
