using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Behavior.Combat
{
    public interface IKillable : IBehavior
    {
        public Defense Hits { get; }
        public IReadOnlyList<Defense> Protection { get; }
        public IReadOnlyList<Defense> AllDefenses => new[] { Hits }.Concat(Protection).ToList().AsReadOnly();
        public double Resilience { get; }
        public bool Defended { get; }

        //public int HitsCur { get; }
        //public int HitsMax { get; } 
        //public double TotalDefenseCur { get; }
        //public double TotalDefenseMax { get; } 

        public bool Dead { get; }

        internal void SetHits(int cur, int max);
        internal void Upgrade(IReadOnlyList<Values> values, double resilience, bool resetFlags = false, IReadOnlyList<int> setCur = null);
        internal void OnAttacked();
        internal void GetHitsRepair(out double hitsInc, out double massCost);
        internal bool IsRepairing();
        public Killable.Events Event { get; }
        internal void RaiseDamagedEvent(Attack attack, Defense defense, Tile defTile);

        public double CurDefenseValue => AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur));
        public double MaxDefenseValue => AllDefenses.Sum(d => Consts.StatValue(d.DefenseMax));

        [Serializable]
        [DataContract(IsReference = true)]
        public readonly struct Values
        {
            public readonly DefenseType Type;
            private readonly int _defense;

            public Values() : this(DefenseType.Hits, 1) { }

            public Values(DefenseType type, int defense)
            {
                Type = type;
                if (defense < 1)
                    defense = 1;
                _defense = defense;
            }
            public Values(Defense defense)
                : this(defense.Type, defense.DefenseMax)
            { }
            public Values(Values defense)
                : this(defense.Type, defense.Defense)
            { }

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
