using System;
using System.Collections.Generic;
using System.Text;

namespace Daemons
{
    [Serializable]
    public class ProductionCenter
    {
        public readonly ProductionType Type;

        private Player owner = null;

        private int x, y;
        private bool used = false;

        public ProductionCenter(Tile tile, int a)
        {
            this.x = tile.X;
            this.y = tile.Y;

            switch (a)
            {
            case 0:
                this.Type = ProductionType.Knight;
                break;
            case 1:
                this.Type = ProductionType.Archer;
                break;
            case 2:
                this.Type = ProductionType.Infantry;
                break;

            default:
                throw new Exception("stuff");
            }
        }

        public int X
        {
            get
            {
                return this.x;
            }
        }
        public int Y
        {
            get
            {
                return this.y;
            }
        }

        internal Player Owner
        {
            get
            {
                return this.owner;
            }
        }

        public bool Used
        {
            get
            {
                return this.used;
            }
        }

        internal void Move(int newX, int newY)
        {
            this.x = newX;
            this.y = newY;
        }

        internal void Use(Player p)
        {
            this.used = true;
            this.owner = p;

            UnitType unitType;
            switch (this.Type)
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

            new Unit(unitType, this.owner.Game.GetTile(this.x, this.y), this.owner);
        }

        internal void Reset(Player player)
        {
            if (this.owner == null || this.owner == player)
            {
                this.used = false;
                this.owner = null;
            }
        }

        public double GetValue()
        {
            UnitType unitType;
            int hits, damage;
            switch (this.Type)
            {
            case ProductionType.Archer:
                unitType = UnitType.Archer;
                hits = Consts.ArcherHits;
                damage = Consts.ArcherDamage;
                break;
            case ProductionType.Infantry:
                unitType = UnitType.Infantry;
                hits = Consts.InfantryHits;
                damage = Consts.InfantryDamage;
                break;
            case ProductionType.Knight:
                unitType = UnitType.Knight;
                hits = Consts.KnightHits;
                damage = Consts.KnightDamage;
                break;
            default:
                throw new Exception("meh");
            }
            return Consts.GetStrength(unitType, hits, damage);
        }

        public override string ToString()
        {
            return this.Type.ToString();
        }
    }

    public enum ProductionType
    {
        Infantry,
        Archer,
        Knight
    }
}
