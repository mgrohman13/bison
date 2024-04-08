using System;
using System.Collections.Generic;
using System.Linq;
using static ClassLibrary1.Pieces.Killable;

using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;

namespace ClassLibrary1.Pieces
{
    public interface IKillable : IBehavior
    {
        //public int HitsCur { get; }
        //public int HitsMax { get; }

        //public double Armor { get; }

        //public double ShieldCur { get; }
        //public double ShieldInc { get; }
        //public double ShieldIncBase { get; }
        //public int ShieldMax { get; }
        //public int ShieldLimit { get; }
        public Defense Hits { get; }
        public IReadOnlyCollection<Defense> Defenses { get; }
        public IReadOnlyCollection<Defense> TotalDefenses => new[] { Hits }.Concat(Defenses).ToList().AsReadOnly();
        public double Resilience { get; }
        public bool Defended { get; }
        //public int DefenseCur { get; }
        //public int DefenseMax { get; }
        //public double TotalDefenseCur2 { get; }
        //public double TotalDefenseMax2 { get; }
        //public double TotalDefenseCurValue2 { get; }
        //public double TotalDefenseMaxValue2 { get; }
        public bool Dead { get; }

        internal void Upgrade(IEnumerable<Values> defenses, double resilience);
        //internal void Damage(int damage, bool splashDamage); 
        internal void GetHitsRepair(out double hitsInc, out double massCost);
        //internal IEnumerable<Defense> GetDefenses(Attack attack) =>
        //    Defenses.Where(d => d.Type != CombatTypes.SkipsDefense(attack.Type));

        //public double GetInc();

        //public interface IDefense 
        //{ 
        //}


        public Events Event { get; }
        internal void RaiseDamagedEvent(Attack attack, Defense defense);


        [Serializable]
        public readonly struct Values
        {
            public readonly DefenseType Type;
            private readonly int _defense;
            //, _shieldMax, _shieldLimit;
            //private readonly double _resilience;//, _armor, _shieldInc;
            //public Values(int hitsMax, double resilience) : this(hitsMax, resilience, 0, 0, 0, 0) { }
            ////public Values(double hitsMax, double resilience, double armor) : this(hitsMax, resilience, armor, 0, 0, 0) { }
            ////public Values(double hitsMax, double resilience, double shieldInc, double shieldMax, double shieldLimit) : this(hitsMax, resilience, 0, shieldInc, shieldMax, shieldLimit) { }
            //public Values(int defense, double resilience)
            //    : this(DefenseType.Hits, defense, resilience)
            //{
            //}
            public Values(DefenseType type, int defense)//, double resilience)
            {
                this.Type = type;
                if (defense < 1)
                    defense = 1;
                this._defense = defense;
                //if (shieldInc <= 0 || shieldMax <= 0 || shieldLimit <= 0)
                //    shieldInc = shieldMax = shieldLimit = 0;
                //if (armor >= 1 || armor < 0)
                //    throw new Exception();
                //this._hitsMax = hitsMax;
                //this._resilience = resilience;
                //this._armor = armor;
                //this._shieldInc = shieldInc;
                //this._shieldMax = shieldMax;
                //this._shieldLimit = shieldLimit;
            }
            //public int HitsMax => _hitsMax;
            //public double Resilience => _resilience;
            //public double Armor => _armor;
            //public double ShieldInc => _shieldInc;
            //public int ShieldMax => _shieldMax;
            public int Defense => _defense;
        }

        public interface IRepairable
        {
            internal double RepairCost { get; }
            internal bool AutoRepair { get; }

            bool CanRepair();
        }
    }
}
