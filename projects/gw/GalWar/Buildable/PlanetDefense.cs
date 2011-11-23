using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class PlanetDefense : Buildable
    {
        public readonly Player Player;

        private byte _att;
        private byte _def;

        internal PlanetDefense(PlanetDefense clone)
        {
            this.Player = clone.Player;
            this.Att = clone.Att;
            this.Def = clone.Def;
        }

        internal PlanetDefense(Player player, List<ShipDesign> designs)
        {
            this.Player = player;

            this.Att = 1;
            this.Def = 1;
            foreach (ShipDesign design in designs)
                GetStats(design);
        }

        internal void GetStats(ShipDesign design)
        {
            this.Att = GetStat(this.Att, design.Att);
            this.Def = GetStat(this.Def, design.Def);
        }

        private int GetStat(int cur, int add)
        {
            return Math.Max(GetStat(( cur + add * Consts.PlanetDefensesRndm ) / ( 1 + Consts.PlanetDefensesRndm )), GetStat(add));
        }

        private static int GetStat(double stat)
        {
            return Game.Random.GaussianCappedInt(stat, Consts.PlanetDefensesRndm, 1);
        }

        public override int Cost
        {
            get
            {
                return 0;
            }
        }

        internal override bool NeedsTile
        {
            get
            {
                return false;
            }
        }

        internal override bool Multiple
        {
            get
            {
                return false;
            }
        }

        public int Att
        {
            get
            {
                return this._att;
            }
            private set
            {
                checked
                {
                    this._att = (byte)value;
                }
            }
        }

        public int Def
        {
            get
            {
                return this._def;
            }
            private set
            {
                checked
                {
                    this._def = (byte)value;
                }
            }
        }

        public double HPCost
        {
            get
            {
                return ShipDesign.GetPlanetDefenseCost(this.Att, this.Def, this.Player.LastResearched);
            }
        }

        internal override void Build(Colony colony, Tile tile, IEventHandler handler)
        {
            colony.BuildPlanetDefense();
        }

        internal override bool CanBeBuiltBy(Colony colony)
        {
            return colony.Player.PlanetDefense == this;
        }

        public override string GetProdText(string curProd)
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return "Defense";
        }
    }
}
