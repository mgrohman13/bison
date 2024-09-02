using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;
using Values = ClassLibrary1.Pieces.IKillable.Values;

namespace ClassLibrary1.Pieces
{
    [Serializable]
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
            : this(piece, new[] { hits }, resilience)
        {
        }
        public Killable(Piece piece, Values hits, IEnumerable<Values> defenses, double resilience)
            : this(piece, defenses.Concat(new[] { hits }), resilience)
        {
        }
        public Killable(Piece piece, IEnumerable<Values> values, double resilience)
        {
            Values hits = GetHits(values);

            this._piece = piece;
            this._hits = new(piece, hits);
            this._defenses = GetOther(values).Select(v => new Defense(piece, v)).ToList();

            this._resilience = resilience;
            this._defended = true;
            this._resetDefended = false;

            OnDeserialization(this);
        }

        T IBehavior.GetBehavior<T>()
        {
            return _piece.GetBehavior<T>();
        }

        void IKillable.SetHits(int cur, int max) => Hits.SetHits(cur, max);
        void IKillable.Upgrade(IEnumerable<Values> values, double resilience)
        {
            Values hits = GetHits(values);
            Values[] defenses = GetOther(values).ToArray();

            _hits.Upgrade(hits);

            double energy = 0, mass = 0;
            foreach (var cur in Game.Rand.Iterate(this.Protection.Where(d1 => !defenses.Any(d2 => d1.Type == d2.Type))))
            {
                _defenses.Remove(cur);

                double costMult = CombatTypes.GetRegenCostMult(cur.Type, Piece.HasBehavior<IAttacker>(), out bool isMass);
                double cost = Consts.StatValue(cur.DefenseCur) * costMult;
                if (isMass)
                    mass += cost;
                else
                    energy += cost;
            }
            Piece.Side.Spend(Game.Rand.Round(-energy), Game.Rand.Round(-mass));

            foreach (var upg in defenses)
            {
                var cur = this.Protection.Where(d => d.Type == upg.Type).SingleOrDefault();
                if (cur == null)
                    _defenses.Add(new(Piece, upg));
                else
                    cur.Upgrade(upg);
            }

            _resilience = resilience;
        }
        private static Values GetHits(IEnumerable<Values> values) =>
            values.Where(d => d.Type == CombatTypes.DefenseType.Hits).Single();
        private static IEnumerable<Values> GetOther(IEnumerable<Values> values) =>
            values.Where(d => d.Type != CombatTypes.DefenseType.Hits);

        void IKillable.OnAttacked()
        {
            this._defended = true;
            this._resetDefended = false;
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
            foreach (Defense defense in ((IKillable)this).AllDefenses)
                defense.GetUpkeep(ref energyUpk, ref massUpk);
        }
        void IBehavior.StartTurn()
        {
            foreach (Defense defense in Game.Rand.Iterate(((IKillable)this).AllDefenses))
                defense.StartTurn();

            if (this._resetDefended)
                _defended = false;
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            foreach (Defense defense in Game.Rand.Iterate(((IKillable)this).AllDefenses))
                defense.EndTurn(ref energyUpk, ref massUpk);

            this._resetDefended = true;
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
        public class DamagedEventArgs
        {
            public readonly Attack Attack;
            public readonly Defense Defense;
            public readonly Tile DefTile;
            public DamagedEventArgs(Attack attack, Defense defense, Tile defTile)
            {
                this.Attack = attack;
                this.Defense = defense;
                this.DefTile = defTile;
            }
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
