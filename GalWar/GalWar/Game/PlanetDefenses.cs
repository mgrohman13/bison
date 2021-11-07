using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    [Serializable]
    class PlanetDefenses
    {
        private Player owner;

        private byte _att, _def;
        private ushort _hp;

        public PlanetDefenses(Player owner, List<ShipDesign> designs)
        {
            this._att = 1;
            this._def = 1;
            this._hp = 1;

            this.owner = owner;
            foreach (ShipDesign design in designs)
                SetPlanetDefense(design);
        }

        internal int Att
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
        internal int Def
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
        internal int HP
        {
            get
            {
                return this._hp;
            }
            private set
            {
                checked
                {
                    this._hp = (ushort)value;
                }
            }
        }

        internal void SetPlanetDefense(ShipDesign design)
        {
            double str = ShipDesign.GetPlanetDefenseStrength(Att, Def) * HP;

            int att = GetPDStat(Att, design.Att);
            int def = GetPDStat(Def, design.Def);
            str = GetPDStat(str, ShipDesign.GetPlanetDefenseStrength(design.Att, design.Def) * design.HP);

            int min = Math.Max(att, def);
            if (min * Consts.PlanetDefenseStatRatio > 1)
                min = Game.Random.GaussianCappedInt(min * Consts.PlanetDefenseStatRatio, Consts.PlanetDefenseRndm, 1);
            if (att < min)
                att = min;
            if (def < min)
                def = min;

            this.Att = att;
            this.Def = def;
            this.HP = Game.Random.Round(str / ShipDesign.GetPlanetDefenseStrength(Att, Def)) + 1;
        }
        private int GetPDStat(double cur, double add)
        {
            double count = Math.Sqrt(owner.GetDesigns().Count + 1);
            return Math.Max(GetPDStat((count * cur + add) / (count + 1)), GetPDStat(add));
        }
        private static int GetPDStat(double stat)
        {
            return Game.Random.GaussianOEInt(stat + 1, Consts.PlanetDefenseRndm, Consts.PlanetDefenseRndm, 1);
        }
    }
}
