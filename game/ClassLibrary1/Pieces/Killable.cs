using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        public Piece Piece => _piece;
        public Defense Hits => _hits;
        public IEnumerable<Defense> Defenses => _defenses.AsReadOnly();
        public double Resilience => _resilience;
        //public int DefenseCur => Hits.DefenseCur;
        //public int DefenseMax => Hits.DefenseMax;
        //public double TotalDefenseCur2 => Consts.SumStats(TotalDefenses.Select(d => d.DefenseCur));
        //public double TotalDefenseMax2 => Consts.SumStats(TotalDefenses.Select(d => d.DefenseCur));
        //public double TotalDefenseCurValue2 => Consts.StatValue(TotalDefenseCur2);
        //public double TotalDefenseMaxValue2 => Consts.StatValue(TotalDefenseMax2);

        public bool Defended => ((IKillable)this).TotalDefenses.Any(d => d.Defended);
        public bool Dead => Hits.Dead;

        public Killable(Piece piece, Values hits, double resilience)
            : this(piece, new[] { hits }, resilience)
        {
        }
        public Killable(Piece piece, Values hits, IEnumerable<Values> defenses, double resilience)
            : this(piece, defenses.Concat(new[] { hits }), resilience)
        {
        }
        public Killable(Piece piece, IEnumerable<Values> defenses, double resilience)
        {
            Values hits = GetHits(defenses);

            this._piece = piece;
            this._hits = new(piece, hits);
            this._defenses = defenses.Except(new[] { hits }).Select(v => new Defense(piece, v)).ToList();
            this._resilience = resilience;

            OnDeserialization(this);
        }

        T IBehavior.GetBehavior<T>()
        {
            return _piece.GetBehavior<T>();
        }

        void IKillable.Upgrade(IEnumerable<Values> values, double resilience)
        {
            Values hits = GetHits(values);
            Values[] defenses = values.Where(d => d.Type != CombatTypes.DefenseType.Hits).ToArray();
            //if (_defenses.Count != defenses.Length)
            //    throw new Exception();
            _hits.Upgrade(hits);
            for (int a = 0; a < defenses.Length; a++)
                if (a < _defenses.Count)
                    _defenses[a].Upgrade(defenses[a]);
                else
                    _defenses.Add(new(Piece, defenses[a]));
            _resilience = resilience;
        }
        private static Values GetHits(IEnumerable<Values> defenses)
        {
            return defenses.Where(d => d.Type == CombatTypes.DefenseType.Hits).Single();
        }

        //void IKillable.Damage(int damage,bool splashDamage)
        //{
        //    Hits.Damage(damage);
        //}

        void IKillable.GetHitsRepair(out double hitsInc, out double massCost)
        {
            Hits.Repair(false, out hitsInc, out massCost);
        }

        //public double GetInc()
        //{
        //    return IncShield(false);
        //}
        //private double IncShield(bool doEndTurn)
        //{
        //    double shieldInc = Consts.IncValueWithMaxLimit(ShieldCur, ShieldInc, Consts.HitsIncDev, ShieldMax, ShieldLimit, Consts.ShieldLimitPow, doEndTurn);
        //    if (doEndTurn)
        //        this._shieldCur += shieldInc;
        //    return shieldInc;
        //}
        void IBehavior.GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            foreach (Defense defense in ((IKillable)this).TotalDefenses)
                defense.GetUpkeep(ref energyUpk, ref massUpk);
        }
        void IBehavior.StartTurn()
        {
            foreach (Defense defense in Game.Rand.Iterate(((IKillable)this).TotalDefenses))
                defense.StartTurn();
        }
        void IBehavior.EndTurn(ref double energyUpk, ref double massUpk)
        {
            foreach (Defense defense in Game.Rand.Iterate(((IKillable)this).TotalDefenses))
                defense.EndTurn(ref energyUpk, ref massUpk);
        }

        [NonSerialized]
        private Events _event = new();
        public Events Event => _event;

        public class Events
        {
            public delegate void DamagedEventHandler(object sender, DamagedEventArgs e);
            public event DamagedEventHandler DamagedEvent;
            internal void RaiseDamagedEvent(Attack attack, Defense defense) =>
                DamagedEvent?.Invoke(this, new DamagedEventArgs(attack, defense));
        }
        public class DamagedEventArgs
        {
            public readonly Attack Attack;
            public readonly Defense Defense;
            public DamagedEventArgs(Attack attack, Defense defense)
            {
                this.Attack = attack;
                this.Defense = defense;
            }
        }
        void IKillable.RaiseDamagedEvent(Attack attack, Defense defense) => Event.RaiseDamagedEvent(attack, defense);

        public void OnDeserialization(object sender)
        {
            _event ??= new();
        }
    }
}
