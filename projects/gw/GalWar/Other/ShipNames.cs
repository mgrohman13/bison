using System;
using System.Collections.Generic;
using System.Linq;

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
            attack = new ShipClass[] { ShipClass.Destroyer, ShipClass.Cruiser, ShipClass.BattleCruiser, ShipClass.Battleship, ShipClass.Dreadnought, ShipClass.Excalibur };
            defense = new ShipClass[] { ShipClass.Warrior, ShipClass.Defender, ShipClass.Ironclad, ShipClass.Armor, ShipClass.Guardian, ShipClass.Avatar };
            speed = new ShipClass[] { ShipClass.Scout, ShipClass.Fighter, ShipClass.Corvette, ShipClass.Frigate, ShipClass.Ranger, ShipClass.Phoenix };
            transport = new ShipClass[] { ShipClass.Bireme, ShipClass.Carrack, ShipClass.Galleon, ShipClass.Transport, ShipClass.Arbiter, ShipClass.MotherShip };
            deathStar = new ShipClass[] { ShipClass.Catapult, ShipClass.Trebuchet, ShipClass.Cannon, ShipClass.DeathStar, ShipClass.Reaper, ShipClass.Demon };

            if (attack.Length != length || defense.Length != length || speed.Length != length || transport.Length != length || deathStar.Length != length)
                throw new Exception();
        }

        internal static int GetClassSort(int name, int mark)
        {
            return name * byte.MaxValue + mark;
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
            return this._marks[idx1, idx2];
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

        internal int GetMark(Player player, int name)
        {
            int value = this.Marks(player.ID, name) + 1;
            this.Marks(player.ID, name, value);
            return value;
        }

        internal ShipClass GetName(Game game, ShipDesign design, double attDefStr, double transStr, double speedStr, bool anomalyShip)
        {
            if (anomalyShip)
                return ShipClass.Salvage;
            if (design.Colony)
                return ShipClass.Colony;

            double avgDS = Consts.GetBombardDamage(attDefStr) * speedStr;

            //double value = ShipDesign.GetValue(design.Att, design.Def, design.HP, design.Speed, design.Trans, design.Colony, design.BombardDamage,
            //        RandValue((game.AvgResearch + design.Research) / 2.0));
            double value = ShipDesign.GetTotCost(design.Att, design.Def, design.HP, design.Speed, design.Trans, design.Colony, design.BombardDamage, 0);
            int tier = GetShipTier(game.GetPlayers().Count, RandValue(value));

            ShipClass shipClass;
            if (tier < length)
            {
                this.tiers[tier].AddShip(RandValue(value));

                //determine which array to use
                ShipClass[] type;
                if (design.Trans * design.Speed > RandMult(transStr * speedStr * .3, 0))
                    type = transport;
                else if (design.DeathStar && design.BombardDamage * design.Speed > RandMult(avgDS * ShipDesign.DeathStarAvg * .3, avgDS * ShipDesign.DeathStarMin))
                    type = deathStar;
                else if (design.Speed > RandMult(speedStr * 1.3, speedStr + .52))
                    type = speed;
                else if (design.Def > RandMult(design.Att * 1.3, design.Att))
                    type = defense;
                else
                    type = attack;
                shipClass = type[tier];
            }
            else
            {
                shipClass = ShipClass.Zeus;
            }
            return shipClass;
        }

        private int GetShipTier(int numPlayers, double value)
        {
            const double mult = 3;

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

            //this means we actually need a higher tier than we have
            return length;
        }

        private static double RandMult(double mult, double min)
        {
            if (mult > min)
                return Game.Random.GaussianCapped(mult, .039, min);
            return min;
        }

        private static double RandValue(double value)
        {
            return Game.Random.GaussianCapped(value, .052, value / 1.3);
        }

        [Serializable]
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
            Colony,

            Destroyer,
            Warrior,
            Scout,
            Bireme,
            Catapult,

            Cruiser,
            Defender,
            Fighter,
            Carrack,
            Trebuchet,

            BattleCruiser,
            Ironclad,
            Corvette,
            Galleon,
            Cannon,

            Battleship,
            Armor,
            Frigate,
            Transport,
            DeathStar,

            Dreadnought,
            Guardian,
            Ranger,
            Arbiter,
            Reaper,

            Excalibur,
            Avatar,
            Phoenix,
            MotherShip,
            Demon,

            Zeus,
            Salvage,

            MAX
        }

        #endregion //logic
    }
}
