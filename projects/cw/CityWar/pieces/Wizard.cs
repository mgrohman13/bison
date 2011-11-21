using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CityWar
{
    public class Wizard : Capturable
    {
        #region fields and constructors
        private List<string> units;

        internal Wizard(Player owner, Tile tile, out bool canUndo)
            : base(1, owner, tile)
        {
            name = "Wizard";

            InitUnits();

            canUndo = tile.Add(this);
            owner.Add(this);

            canUndo = false;
        }

        private void InitUnits()
        {
            units = new List<string>();
            foreach (string[] race in Game.Races.Values)
                foreach (string u in race)
                {
                    float chance;
                    switch (Unit.CreateTempUnit(u).costType)
                    {
                    case CostType.Death:
                        chance = .7f;
                        break;
                    default:
                        chance = .5f;
                        break;
                    case CostType.Production:
                        chance = 0f;
                        break;
                    }
                    if (Game.Random.Bool(chance))
                        units.Add(u);
                }
        }

        //constructor for loading games
        private Wizard(Player owner, int group, int movement, int maxMove, string name, Tile tile, Abilities ability, List<string> units)
            : base(group, movement, maxMove, name, tile, ability)
        {
            this.owner = owner;
            this.units = units;
        }
        #endregion //fields and constructors

        #region overrides
        public override bool CapableBuild(string name)
        {
            if (name == "Wizard")
                return true;

            if (name.EndsWith(" Portal"))
                return true;

            Unit unit = Unit.CreateTempUnit(name);
            if (!raceCheck(unit))
                return false;

            if (units.Contains(name))
                return true;
            CostType costType = unit.costType;
            return ( costType == CostType.Air && tile.Terrain == Terrain.Plains ||
                costType == CostType.Earth && tile.Terrain == Terrain.Mountain ||
                costType == CostType.Nature && tile.Terrain == Terrain.Forest ||
                costType == CostType.Water && tile.Terrain == Terrain.Water );
        }

        internal override double Heal()
        {
            if (movement > 0)
            {
                owner.AddMagic(tile.Terrain, 10);
                --movement;
                return 1;
            }
            return -1;
        }
        internal override void UndoHeal(double oldMove)
        {
            owner.AddMagic(tile.Terrain, -10);
            ++movement;
        }

        protected override bool CanMoveChild(Tile t)
        {
            Player p;
            return ( !( t.Occupied(out p) && p != owner ) );
        }

        protected override bool DoMove(Tile t, out bool canUndo)
        {
            if (movement > 0)
            {
                //cant undo if you collect any wizard points
                canUndo = t.CollectWizPts(owner);
                tile.Remove(this);
                t.Add(this);
                this.tile = t;
                tile.CurrentGroup = group;
                --movement;
                return true;
            }

            canUndo = true;
            return false;
        }

        internal override void ResetMove()
        {
            while (movement > 0)
            {
                Heal();
            }

            movement = MaxMove;
        }
        #endregion //overrides

        #region internal methods
        internal void ChangeTerrain(Terrain t)
        {
            if (movement > 0)
            {
                tile.Terrain = t;
                --movement;
            }
        }
        internal void UndoChangeTerrain(int oldMovement, Terrain t)
        {
            tile.Terrain = t;
            movement = oldMovement;
        }

        internal int CanBuildCount
        {
            get
            {
                return units.Count;
            }
        }
        #endregion //internal methods

        #region saving and loading
        internal override void SavePiece(BinaryWriter bw)
        {
            bw.Write("Wizard");

            bw.Write(units.Count);
            foreach (string s in units)
                bw.Write(s);

            SavePieceStuff(bw);
        }

        internal static Wizard LoadWizard(BinaryReader br, Player owner)
        {
            int unitCount = br.ReadInt32();
            List<string> units = new List<string>(unitCount);
            for (int a = -1 ; ++a < unitCount ; )
                units.Add(br.ReadString());

            int group, movement, maxMove;
            string name;
            Tile tile;
            Abilities ability;
            Piece.LoadPieceStuff(br, out group, out movement, out maxMove, out name, out tile, out ability);

            return new Wizard(owner, group, movement, maxMove, name, tile, ability, units);
        }
        #endregion //saving and loading
    }
}
