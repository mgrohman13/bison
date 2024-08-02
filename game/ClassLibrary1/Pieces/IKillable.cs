using System;
using System.Collections.Generic;
using System.Linq;
using static ClassLibrary1.Pieces.Killable;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces
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
        internal void Upgrade(IEnumerable<Values> defenses, double resilience);
        internal void OnAttacked();
        internal void GetHitsRepair(out double hitsInc, out double massCost);

        public Events Event { get; }
        internal void RaiseDamagedEvent(Attack attack, Defense defense, Tile defTile);

        [Serializable]
        public readonly struct Values
        {
            public readonly DefenseType Type;
            private readonly int _defense;

            public Values(DefenseType type, int defense)
            {
                this.Type = type;
                if (defense < 1)
                    defense = 1;
                this._defense = defense;
            }

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
