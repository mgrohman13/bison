using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    internal class ShipNames
    {
        #region static

        private const int length = 6;

        private static readonly ShipClass[] attack;
        private static readonly ShipClass[] defense;
        private static readonly ShipClass[] speed;
        private static readonly ShipClass[] transport;
        private static readonly ShipClass[] deathStar;

        static ShipNames()
        {
            attack = new ShipClass[] { ShipClass.Destroyer, ShipClass.Cruiser, ShipClass.Battlecruiser, ShipClass.Battleship, ShipClass.Dreadnought, ShipClass.Excalibur };
            defense = new ShipClass[] { ShipClass.Warrior, ShipClass.Defender, ShipClass.Ironclad, ShipClass.Armor, ShipClass.Guardian, ShipClass.Avatar };
            speed = new ShipClass[] { ShipClass.Scout, ShipClass.Fighter, ShipClass.Corvette, ShipClass.Frigate, ShipClass.Ranger, ShipClass.Phoenix };
            transport = new ShipClass[] { ShipClass.Galley, ShipClass.Carrack, ShipClass.Galleon, ShipClass.Transport, ShipClass.Invader, ShipClass.Reaper };
            deathStar = new ShipClass[] { ShipClass.Catapult, ShipClass.Trebuchet, ShipClass.Cannon, ShipClass.Deathstar, ShipClass.Exterminator, ShipClass.Demon };

            if (attack.Length != length || defense.Length != length || speed.Length != length || transport.Length != length || deathStar.Length != length)
                throw new Exception();
        }

        internal static string GetName(int name, int mark)
        {
            return Game.CamelToSpaces(( (ShipClass)name ).ToString()) + " " + Game.NumberToRoman(mark);
        }

        #endregion static

        #region fields and constructors

        private readonly Tier[] _tiers;

        private readonly byte[,] _marks;

        [NonSerialized]
        private bool _setup;

        internal ShipNames(int numPlayers)
        {
            checked
            {
                this._tiers = new Tier[length];
                this._marks = new byte[numPlayers, (int)ShipClass.MAX];
                this._setup = true;

                this.tiers[0] = new Tier();
            }
        }

        private Tier[] tiers
        {
            get
            {
                return this._tiers;
            }
        }

        private int Marks(int idx1, int idx2)
        {
            checked
            {
                return this._marks[idx1, idx2];
            }
        }
        private void Marks(int idx1, int idx2, int value)
        {
            checked
            {
                this._marks[idx1, idx2] = (byte)value;
            }
        }

        private bool setup
        {
            get
            {
                return this._setup;
            }
            set
            {
                checked
                {
                    this._setup = value;
                }
            }
        }

        #endregion //fields and constructors

        #region logic

        internal void EndSetup()
        {
            this.setup = false;
        }

        internal ShipClass GetName(int numPlayers, ShipDesign design, double attDefStr, double transStr, double speedStr, bool anomalyShip)
        {
            return GetNameType(numPlayers, design, attDefStr, transStr, speedStr, anomalyShip);
        }

        internal int GetMark(Player player, int name)
        {
            int value = this.Marks(player.ID, name) + 1;
            this.Marks(player.ID, name, value);
            return value;
        }

        private ShipClass GetNameType(int numPlayers, ShipDesign design, double attDefStr, double transStr, double speedStr, bool anomalyShip)
        {
            if (anomalyShip)
                return ShipClass.Salvage;
            if (design.Colony)
                return ShipClass.Colony;

            //determine which array to use
            ShipClass[] type;
            if (design.Trans * design.Speed > RandMult(transStr * speedStr * .3))
                type = transport;
            else if (design.BombardDamage * design.Speed > RandMult(Consts.GetBombardDamage(attDefStr) * speedStr * ShipDesign.DeathStarAvg * .3))
                type = deathStar;
            else if (design.Speed > RandMult(speedStr * 1.3))
                type = speed;
            else if (design.Def > RandMult(design.Att * 1.3))
                type = defense;
            else
                type = attack;

            double value = ShipDesign.GetTotCost(design.Att, design.Def, design.HP, design.Speed, design.Trans, design.Colony, design.BombardDamage, 0);
            int retVal = GetName(numPlayers, type, RandValue(value));

            this.tiers[retVal].AddShip(RandValue(value));
            return type[retVal];
        }

        private int GetName(int numPlayers, ShipClass[] type, double value)
        {
            const double mult = Math.PI;

            if (setup)
                return 0;

            double avg = double.NaN;
            for (int a = 0 ; a < length ; ++a)
            {
                Tier tier = this.tiers[a];
                if (tier == null)
                    tier = this.tiers[a] = new Tier();

                if (tier.Count < numPlayers || value < tier.Max)
                    return a;

                if (double.IsNaN(avg))
                    avg = tier.Avg;
                else
                    avg = ( avg + tier.Avg ) / 2.0;
                avg *= mult;

                int b = a + 1;
                if (b < length)
                {
                    Tier next = this.tiers[b];
                    if (next != null && value > next.Min)
                        continue;
                }

                if (value < avg)
                    return a;
            }

            return length;
        }

        private static double RandMult(double mult)
        {
            return Game.Random.GaussianCapped(mult, .03, 0);
        }

        private static double RandValue(double value)
        {
            return Game.Random.GaussianCapped(value, .06, value / 1.3);
        }

        private class Tier
        {
            private float _min, _max, _total;
            private byte _count;

            public Tier()
            {
                checked
                {
                    this._min = float.MaxValue;
                    this._max = float.MinValue;

                    this._total = 0;
                    this._count = 0;
                }
            }

            public double Min
            {
                get
                {
                    return this._min;
                }
                private set
                {
                    checked
                    {
                        this._min = (float)value;
                    }
                }
            }
            public double Max
            {
                get
                {
                    return this._max;
                }
                private set
                {
                    checked
                    {
                        this._max = (float)value;
                    }
                }
            }
            public int Count
            {
                get
                {
                    return this._count;
                }
                private set
                {
                    checked
                    {
                        this._count = (byte)value;
                    }
                }
            }

            private double total
            {
                get
                {
                    return this._total;
                }
                set
                {
                    checked
                    {
                        this._total = (float)value;
                    }
                }
            }

            public double Avg
            {
                get
                {
                    return ( this.total / (double)this.Count );
                }
            }

            public void AddShip(double value)
            {
                Min = Math.Min(Min, value);
                Max = Math.Max(Max, value);
                this.total += value;
                ++this.Count;
            }
        }

        internal enum ShipClass
        {
            Salvage,
            Colony,
            Destroyer,
            Cruiser,
            Battlecruiser,
            Battleship,
            Dreadnought,
            Excalibur,
            Warrior,
            Defender,
            Ironclad,
            Armor,
            Guardian,
            Avatar,
            Scout,
            Fighter,
            Corvette,
            Frigate,
            Ranger,
            Phoenix,
            Galley,
            Carrack,
            Galleon,
            Transport,
            Invader,
            Reaper,
            Catapult,
            Trebuchet,
            Cannon,
            Deathstar,
            Exterminator,
            Demon,
            MAX
        }

        #endregion //logic
    }
}
