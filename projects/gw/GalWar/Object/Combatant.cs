using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public abstract class Combatant : PopCarrier
    {
        #region fields and constructors

        private byte _att, _def;
        private ushort _hp;

        protected Combatant(Tile tile, int att, int def, int hp, int population, double soldiers)
            : base(tile, population, soldiers)
        {
            checked
            {
                this._att = (byte)att;
                this._def = (byte)def;
                this._hp = (ushort)hp;
            }
        }

        public int Att
        {
            get
            {
                return this._att;
            }
            protected set
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
            protected set
            {
                checked
                {
                    this._def = (byte)value;
                }
            }
        }

        public int HP
        {
            get
            {
                return this._hp;
            }
            protected set
            {
                checked
                {
                    if (this.HP > value)
                        OnDamaged(this.HP - value);

                    SetHP(value);
                }
            }
        }
        protected void SetHP(int value)
        {
            checked
            {
                this._hp = (ushort)value;
            }
        }
        protected virtual void OnDamaged(int damage)
        {
        }

        #endregion //fields and constructors

        #region abstract

        internal abstract double GetExpForDamage(double damage);

        protected abstract double GetKillExp();

        internal abstract void AddExperience(double rawExp, double valueExp);

        #endregion //abstract

        #region protected

        protected double Combat(IEventHandler handler, Combatant defender)
        {
            handler.OnCombat(this, defender, int.MinValue, int.MinValue);

            double pct = 0, rawExp = 0, valueExp = 0;

            int round = -1, rounds = this.Att;
            while (++round < rounds && this.HP > 0 && defender.HP > 0)
            {
                if (pct != 0)
                    throw new Exception();

                int attack = Game.Random.RangeInt(0, this.Att), defense = Game.Random.RangeInt(0, defender.Def),
                        damage = attack - defense;
                if (damage > 0)
                    pct = this.Damage(defender, damage, ref rawExp, ref valueExp);
                else if (damage < 0)
                    pct = defender.Damage(this, -damage, ref rawExp, ref valueExp);

                //a small constant exp is gained every round
                rawExp += this.GetExpForDamage(Consts.ExperienceConstDmgAmt) + defender.GetExpForDamage(Consts.ExperienceConstDmgAmt);

                handler.OnCombat(this, defender, attack, defense);
            }

            //add kill exp before destroying so the player gets paid off for the exp
            rawExp += this.AddKillExp();
            rawExp += defender.AddKillExp();

            //exp is always added in equal amount to both ships
            this.AddExperience(rawExp, valueExp);
            defender.AddExperience(rawExp, valueExp);

            CheckDestroy(this);
            CheckDestroy(defender);

            //get partial upkeep back for overkill
            return ( rounds - round + pct ) / (double)rounds;
        }

        private static void CheckDestroy(Combatant combatant)
        {
            Ship ship = combatant as Ship;
            if (ship != null && ship.HP == 0)
                ship.Destroy(true, true);
        }

        private double AddKillExp()
        {
            if (this.HP == 0)
                return this.GetKillExp();
            return 0;
        }

        private double Damage(Combatant combatant, int damage, ref double rawExp, ref double valueExp)
        {
            double retVal = 0;
            int startHP = combatant.HP;
            if (damage > startHP)
            {
                retVal = 1 - ( startHP / (double)damage );
                damage = startHP;
            }

            combatant.Damage(damage, ref rawExp, ref valueExp);

            return retVal;
        }
        internal void Damage(int damage, ref double rawExp, ref double valueExp)
        {
            if (damage > HP)
                throw new Exception();
            int pop = this.Population;

            HP -= damage;

            rawExp += GetExpForDamage(damage);
            valueExp += ( pop - this.Population ) * Consts.TroopExperienceMult;
        }

        #endregion //protected
    }
}
