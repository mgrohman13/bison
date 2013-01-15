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

        protected Combatant(int att, int def, int hp, int population, double soldiers)
            : base(population, soldiers)
        {
            this.Att = att;
            this.Def = def;
            this.HP = hp;
        }

        #endregion //fields and constructors

        #region abstract

        internal abstract double GetExpForDamage(double damage);

        protected abstract double GetKillExp();

        internal abstract void AddExperience(double experience);

        internal abstract void AddCostExperience(double cost);

        #endregion //abstract

        #region protected

        protected double Combat(IEventHandler handler, Combatant defender)
        {
            handler.OnCombat(this, defender, int.MinValue, int.MinValue);

            double pct = 0, experience = 0, costExperience = 0;

            int round = -1, rounds = this.Att;
            while (++round < rounds && this.HP > 0 && defender.HP > 0)
            {
                if (pct != 0)
                    throw new Exception();

                int attack = Game.Random.RangeInt(0, this.Att), defense = Game.Random.RangeInt(0, defender.Def),
                        damage = attack - defense, pop = ( this.Population + defender.Population );
                if (damage > 0)
                    pct = this.Damage(defender, damage, ref experience);
                else if (damage < 0)
                    pct = defender.Damage(this, -damage, ref experience);

                //a small constant exp is gained every round
                experience += this.GetExpForDamage(Consts.ExperienceConstDmgAmt) + defender.GetExpForDamage(Consts.ExperienceConstDmgAmt);

                if (pop > 0)
                    costExperience += ( pop - ( this.Population + defender.Population ) ) * Consts.TroopExperienceMult;

                handler.OnCombat(this, defender, attack, defense);
            }

            //add kill exp before destroying so the player gets paid off for the exp
            experience += this.AddKillExp();
            experience += defender.AddKillExp();

            //exp is always added in equal amount to both ships
            this.AddExperience(experience);
            defender.AddExperience(experience);
            if (costExperience > 0)
            {
                this.AddCostExperience(costExperience);
                defender.AddCostExperience(costExperience);
            }

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
