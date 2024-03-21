using System;
using System.Collections.Generic;

namespace ClassLibrary1.Pieces
{
    public interface IKillable : IBehavior
    {
        public double Resilience { get; }


        //public int HitsCur { get; }
        //public int HitsMax { get; }

        //public double Armor { get; }

        //public double ShieldCur { get; }
        //public double ShieldInc { get; }
        //public double ShieldIncBase { get; }
        //public int ShieldMax { get; }
        //public int ShieldLimit { get; }
        public int DefenseCur { get; }
        public int DefenseMax { get; }
        public bool Dead { get; }

        //public IReadOnlyCollection<Defense> Defenses { get; }

        internal void Upgrade(Values values);
        internal void Damage(int damage);//, double shieldDmg);
        internal void Repair(bool doRepair, out double hitsInc, out double massCost);
        //public double GetInc();

        //public interface IDefense 
        //{ 
        //}

        [Serializable]
        public readonly struct Values
        {
            private readonly int _defense;
            //, _shieldMax, _shieldLimit;
            private readonly double _resilience;//, _armor, _shieldInc;
            //public Values(int hitsMax, double resilience) : this(hitsMax, resilience, 0, 0, 0, 0) { }
            ////public Values(double hitsMax, double resilience, double armor) : this(hitsMax, resilience, armor, 0, 0, 0) { }
            ////public Values(double hitsMax, double resilience, double shieldInc, double shieldMax, double shieldLimit) : this(hitsMax, resilience, 0, shieldInc, shieldMax, shieldLimit) { }
            public Values(int defense, double resilience)//int hitsMax, double armor, double shieldInc, int shieldMax, int shieldLimit)
            {
                if (defense < 1)
                    defense = 1;
                this._defense = defense;
                //if (shieldInc <= 0 || shieldMax <= 0 || shieldLimit <= 0)
                //    shieldInc = shieldMax = shieldLimit = 0;
                //if (armor >= 1 || armor < 0)
                //    throw new Exception();
                //this._hitsMax = hitsMax;
                this._resilience = resilience;
                //this._armor = armor;
                //this._shieldInc = shieldInc;
                //this._shieldMax = shieldMax;
                //this._shieldLimit = shieldLimit;
            }
            //public int HitsMax => _hitsMax;
            public double Resilience => _resilience;
            //public double Armor => _armor;
            //public double ShieldInc => _shieldInc;
            //public int ShieldMax => _shieldMax;
            public int Defense => _defense;
        }

        public interface IRepairable
        {
            internal double RepairCost { get; }
            internal bool AutoRepair { get; }
        }
    }
}
