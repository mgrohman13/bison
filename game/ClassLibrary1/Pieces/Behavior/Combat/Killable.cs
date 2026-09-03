using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;
using Values = ClassLibrary1.Pieces.Behavior.Combat.IKillable.Values;

namespace ClassLibrary1.Pieces.Behavior.Combat
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Killable : IKillable, IDeserializationCallback
    {
        private readonly Piece _piece;
        private readonly Defense _hits;
        private readonly List<Defense> _defenses;

        private double _resilience;
        private bool _defended, _resetDefended;

        public Piece Piece => _piece;
        public Defense Hits => _hits;
        public IReadOnlyList<Defense> Protection => CombatTypes.OrderDef(_defenses);
        public double Resilience => _resilience;

        //public int HitsCur => Hits.DefenseCur;
        //public int HitsMax => Hits.DefenseMax;
        //public double TotalDefenseCur => Consts.SumStats(TotalDefenses.Select(d => d.DefenseCur));
        //public double TotalDefenseMax => Consts.SumStats(TotalDefenses.Select(d => d.DefenseCur)); 

        public bool Defended => _defended;
        public bool Dead => Hits.Dead;

        public Killable(Piece piece, Values hits, double resilience)
            : this(piece, [hits], resilience)
        {
        }
        public Killable(Piece piece, Values hits, IEnumerable<Values> defenses, double resilience)
            : this(piece, defenses.Concat([hits]), resilience)
        {
        }
        public Killable(Piece piece, IEnumerable<Values> values, double resilience)
        {
            Values hits = GetHits(values);

            _piece = piece;
            _hits = new(piece, hits);
            _defenses = [.. GetOther(values).Select(v => new Defense(piece, v))];

            _resilience = resilience;

            ResetFlags();

            OnDeserialization(this);
        }
        private void ResetFlags()
        {
            _defended = true;
            _resetDefended = false;
        }

        T IBehavior.GetBehavior<T>()
        {
            return _piece.GetBehavior<T>();
        }

        void IKillable.SetHits(int cur, int max) => Hits.SetHits(cur, max);
        void IKillable.Upgrade(IReadOnlyList<Values> values, double resilience, bool resetFlags, IReadOnlyList<int> setCur)
        {
            double energy = 0, mass = 0;
            foreach (var cur in Game.Rand.Iterate(Protection.Where(d1 => !values.Any(d2 => d1.Type == d2.Type))))
            {
                _defenses.Remove(cur);

                double costMult = CombatTypes.GetRegenCostMult(Piece.Game.Consts, cur.Type, Piece.HasBehavior<IAttacker>(), out bool isMass);
                double cost = Consts.StatValue(cur.DefenseCur) * costMult;
                if (isMass)
                    mass += cost;
                else
                    energy += cost;
            }
            if (setCur == null)
                Piece.Side.AddResources(energy, mass);

            for (int a = 0; a < values.Count; a++)
            {
                var upg = values[a];
                int? curDef = setCur?[a];
                if (upg.Type == CombatTypes.DefenseType.Hits)
                {
                    _hits.Upgrade(upg, curDef);
                }
                else
                {
                    var cur = Protection.SingleOrDefault(d => d.Type == upg.Type);
                    if (cur == null)
                        _defenses.Add(new(Piece, upg, curDef));
                    else
                        cur.Upgrade(upg, curDef);
                }
            }

            _resilience = resilience;

            if (resetFlags)
                ResetFlags();
        }

        private static Values GetHits(IEnumerable<Values> values) =>
            values.Single(d => d.Type == CombatTypes.DefenseType.Hits);
        private static IEnumerable<Values> GetOther(IEnumerable<Values> values) =>
            values.Where(d => d.Type != CombatTypes.DefenseType.Hits);
        private IEnumerable<Defense> IterateDefenses() =>
            Game.Rand.Iterate(((IKillable)this).AllDefenses);

        void IKillable.OnAttacked()
        {
            _defended = true;
            _resetDefended = false;
        }

        void IKillable.GetHitsRepair(out double hitsInc, out double massCost)
        {
            Hits.Repair(false, out hitsInc, out massCost);
        }
        bool IKillable.IsRepairing()
        {
            ((IKillable)this).GetHitsRepair(out double hitsInc, out _);
            var armor = Protection.SingleOrDefault(d => d.Type == CombatTypes.DefenseType.Armor && d.DefenseCur < d.DefenseMax);
            if (armor != null)
                hitsInc += armor.GetRegen();
            return hitsInc > 0;
        }

        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            foreach (Defense defense in IterateDefenses())
                defense.GetUpkeep(ref energyUpk, ref massUpk);
        }
        void IBehavior.StartTurn()
        {
            //base.StartTurn();

            foreach (Defense defense in IterateDefenses())
                defense.StartTurn();

            if (_resetDefended)
                _defended = false;
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            foreach (Defense defense in IterateDefenses())
                defense.EndTurn(ref energyUpk, ref massUpk);

            _resetDefended = true;
        }
        double IBehavior.Die()
        {
            double treasure = 0;
            foreach (Defense defense in IterateDefenses())
                treasure += defense.Die();
            return treasure;
        }

        [NonSerialized]
        private Events _event = new();
        public Events Event => _event;

        public class Events
        {
            public delegate void DamagedEventHandler(object sender, DamagedEventArgs e);
            public event DamagedEventHandler DamagedEvent;
            internal void RaiseDamagedEvent(Attack attack, Defense defense, Tile defTile) =>
                DamagedEvent?.Invoke(this, new DamagedEventArgs(attack, defense, defTile));
        }
        public class DamagedEventArgs(Attack attack, Defense defense, Map.Map.Tile defTile)
        {
            public readonly Attack Attack = attack;
            public readonly Defense Defense = defense;
            public readonly Tile DefTile = defTile;
        }
        void IKillable.RaiseDamagedEvent(Attack attack, Defense defense, Tile defTile)
            => Event.RaiseDamagedEvent(attack, defense, defTile);

        public void OnDeserialization(object sender)
        {
            //base.OnDeserialization(sender);
            _event ??= new();
        }
    }
}
