using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1.Pieces
{
    public interface IKillable : IBehavior
    {
        public int HitsCur { get; }
        public int HitsMax { get; }
        public double Resilience { get; }
        public double Armor { get; }
        public double ShieldCur { get; }
        public double ShieldInc { get; }
        public double ShieldIncBase { get; }
        public int ShieldMax { get; }
        public int ShieldLimit { get; }
        public bool Dead { get; }

        internal void Upgrade(Values values);
        internal void Damage(int damage, double shieldDmg);
        internal void Repair(bool doRepair, out double hitsInc, out double massCost);
        public double GetInc();

        [Serializable]
        public struct Values
        {
            private readonly int _hitsMax, _shieldMax, _shieldLimit;
            private readonly double _resilience, _armor, _shieldInc;
            public Values(int hitsMax, double resilience) : this(hitsMax, resilience, 0, 0, 0, 0) { }
            //public Values(double hitsMax, double resilience, double armor) : this(hitsMax, resilience, armor, 0, 0, 0) { }
            //public Values(double hitsMax, double resilience, double shieldInc, double shieldMax, double shieldLimit) : this(hitsMax, resilience, 0, shieldInc, shieldMax, shieldLimit) { }
            public Values(int hitsMax, double resilience, double armor, double shieldInc, int shieldMax, int shieldLimit)
            {
                if (shieldInc <= 0 || shieldMax <= 0 || shieldLimit <= 0)
                    shieldInc = shieldMax = shieldLimit = 0;
                this._hitsMax = hitsMax;
                this._resilience = resilience;
                this._armor = armor;
                this._shieldInc = shieldInc;
                this._shieldMax = shieldMax;
                this._shieldLimit = shieldLimit;
            }
            public int HitsMax => _hitsMax;
            public double Resilience => _resilience;
            public double Armor => _armor;
            public double ShieldInc => _shieldInc;
            public int ShieldMax => _shieldMax;
            public int ShieldLimit => _shieldLimit;
        }

        public interface IRepairable
        {
            internal double RepairCost { get; }
            internal bool AutoRepair { get; }
        }
    }
}
