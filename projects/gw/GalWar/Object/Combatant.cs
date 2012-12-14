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

        protected Combatant()
        {
            this._att = 1;
            this._def = 1;
            this._hp = 0;
        }

        #endregion //fields and constructors

        #region abstract

        protected abstract double GetExpForDamage(double damage);

        protected abstract double GetKillExp();

        protected abstract void AddExperience(double experience);

        #endregion //abstract

        #region protected

        protected double Combat(IEventHandler handler, Combatant defender)
        {
            handler.OnCombat(this, defender, 0, 0);

            double pct = 0;
            double experience = 0;

            int rounds = this.Att;
            int round = -1;
            while (++round < rounds && this.HP > 0 && defender.HP > 0)
            {
                if (pct != 0)
                    throw new Exception();

                int attack = Game.Random.RangeInt(0, this.Att), defense = Game.Random.RangeInt(0, defender.Def), damage = attack - defense;
                if (damage > 0)
                    pct = this.Damage(defender, damage, ref experience);
                else if (damage < 0)
                    pct = defender.Damage(this, -damage, ref experience);

                //a small constant exp is gained every round
                experience += ( this.GetExpForDamage(Consts.ExperienceConstDmgAmt)
                        + defender.GetExpForDamage(Consts.ExperienceConstDmgAmt) ) / 2.0;

                handler.OnCombat(this, defender, attack, defense);
            }

            //add kill exp before destroying so the player gets paid off for the exp
            experience += this.AddKillExp();
            experience += defender.AddKillExp();

            //exp is always added in equal amount to both ships
            this.AddExperience(experience);
            defender.AddExperience(experience);

            CheckDestroy(this);
            CheckDestroy(defender);

            //get partial upkeep back for overkill
            return ( rounds - round + pct ) / (double)rounds;
        }

        private static void CheckDestroy(Combatant combatant)
        {
            Ship ship = combatant as Ship;
            if (ship != null && ship.HP == 0)
                ship.Destroy(true);
        }

        private double AddKillExp()
        {
            if (this.HP == 0)
                return this.GetKillExp();
            return 0;
        }

        private double Damage(Combatant combatant, int damage, ref double experience)
        {
            double retVal = 0;
            int startHP = combatant.HP;
            if (damage > startHP)
            {
                retVal = 1 - ( startHP / (double)damage );
                damage = startHP;
            }

            combatant.HP -= damage;

            experience += combatant.GetExpForDamage(damage);

            return retVal;
        }

        #endregion //protected

        #region public

        public int Att
        {
            get
            {
                return this._att;
            }
            protected set
            {
                SetAtt(value);
            }
        }

        protected virtual void SetAtt(int value)
        {
            checked
            {
                this._att = (byte)value;
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
                SetDef(value);
            }
        }

        protected virtual void SetDef(int value)
        {
            checked
            {
                this._def = (byte)value;
            }
        }

        public int HP
        {
            get
            {
                return this._hp;
            }
            internal set
            {
                SetHP(value);
            }
        }

        protected virtual void SetHP(int value)
        {
            checked
            {
                this._hp = (ushort)value;
            }
        }

        #endregion //public
    }
}
