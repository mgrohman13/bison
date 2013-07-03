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

        private readonly uint[] _divisions;
        private readonly byte[,] _marks;

        [NonSerialized]
        private bool _setup;
        [NonSerialized]
        private int _total, _count;

        internal ShipNames(int numPlayers)
        {
            checked
            {
                this._marks = new byte[numPlayers, (int)ShipClass.MAX];
                this._divisions = new uint[length - 1];
                for (int a = 2 ; a < length ; ++a)
                    this.Divisions(a, int.MaxValue);

                this._setup = true;
                this._total = 0;
                this._count = 0;
            }
        }

        private int Divisions(int idx)
        {
            checked
            {
                return (int)this._divisions[idx - 1];
            }
        }
        private void Divisions(int idx, int value)
        {
            checked
            {
                this._divisions[idx - 1] = (uint)value;
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
        private int total
        {
            get
            {
                return this._total;
            }
            set
            {
                checked
                {
                    this._total = value;
                }
            }
        }
        private int count
        {
            get
            {
                return this._count;
            }
            set
            {
                checked
                {
                    this._count = value;
                }
            }
        }

        #endregion //fields and constructors

        #region logic

        private ShipClass DoSetup(ShipClass[] type, int value)
        {
            //during setup phase, the first name is always used, and the average cost calculated
            this.total += value;
            ++this.count;
            return type[0];
        }

        internal void EndSetup()
        {
            this.setup = false;
            //first division is set using the average
            SetDivision(1, this.total / (double)this.count);
        }

        internal ShipClass GetName(ShipDesign design, double attDefStr, double transStr, double speedStr, bool anomalyShip)
        {
            return GetNameType(design, attDefStr, transStr, speedStr, anomalyShip);
        }

        internal int GetMark(Player player, int name)
        {
            int value = this.Marks(player.ID, name) + 1;
            this.Marks(player.ID, name, value);
            return value;
        }

        private ShipClass GetNameType(ShipDesign design, double attDefStr, double transStr, double speedStr, bool anomalyShip)
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

            int value = RandValue(ShipDesign.GetTotCost(design.Att, design.Def, design.HP, design.Speed, design.Trans, design.Colony, design.BombardDamage, 0), 1);

            if (this.setup)
                return DoSetup(type, value);

            return GetName(type, value);
        }

        private ShipClass GetName(ShipClass[] type, int value)
        {
            //find the highest division it matches
            for (int i = length ; --i > 0 ; )
                if (value > this.Divisions(i))
                {
                    //check if this is a breakthrough design, and if so use its value for the next division
                    int next = i + 1;
                    if (next < length && this.Divisions(next) == int.MaxValue)
                        SetDivision(next, value);
                    return type[i];
                }
            return type[0];
        }

        private void SetDivision(int index, double value)
        {
            this.Divisions(index, RandValue(value * 3.9, Game.Random.Round(value * 3) + 13));
        }

        private static double RandMult(double mult)
        {
            return Game.Random.Gaussian(mult, .03);
        }

        private static int RandValue(double value, int min)
        {
            if (value > min)
                return Game.Random.GaussianCappedInt(value, .06, min);
            else
                return min;
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

        #region new

        //private Tier[] t = new Tier[length - 1];

        //private ShipClass GetName(int numPlayers, ShipClass[] type, double value)
        //{
        //    int retVal = length - 1;

        //    const double mult = Math.PI;

        //    double avg = double.NaN;
        //    for (int i = 0 ; i < retVal ; ++i)
        //    {
        //        if (t[i] == null)
        //            t[i] = new Tier();

        //        if (t[i].Count < numPlayers * 3)
        //        {
        //            retVal = i;
        //            break;
        //        }

        //        if (double.IsNaN(avg))
        //        {
        //            avg = t[i].Avg;
        //        }
        //        else
        //        {
        //            avg = ( avg + t[i].Avg ) / 2.0;
        //            if (avg < t[i].Min)
        //                avg = t[i].Min;
        //            else if (avg > t[i].Max)
        //                avg = t[i].Max;
        //        }

        //        avg *= mult;
        //        if (t[i + 1] != null && avg > t[i].Min)
        //            avg = t[i].Min;

        //        if (value < avg)
        //        {
        //            retVal = i;
        //            break;
        //        }
        //    }

        //    t[retVal].AddShip(value);
        //    return type[retVal];
        //}


        //private class Tier
        //{
        //    private float _min, _max, _total;
        //    private byte _count;

        //    public Tier()
        //    {
        //        checked
        //        {
        //            this._min = float.MaxValue;
        //            this._max = float.MinValue;

        //            this._total = 0;
        //            this._count = 0;
        //        }
        //    }

        //    public double Min
        //    {
        //        get
        //        {
        //            return this._min;
        //        }
        //        private set
        //        {
        //            checked
        //            {
        //                this._min = (float)value;
        //            }
        //        }
        //    }
        //    public double Max
        //    {
        //        get
        //        {
        //            return this._max;
        //        }
        //        private set
        //        {
        //            checked
        //            {
        //                this._max = (float)value;
        //            }
        //        }
        //    }
        //    public int Count
        //    {
        //        get
        //        {
        //            return this._count;
        //        }
        //        private set
        //        {
        //            checked
        //            {
        //                this._count = (byte)value;
        //            }
        //        }
        //    }

        //    private double total
        //    {
        //        get
        //        {
        //            return this._total;
        //        }
        //        set
        //        {
        //            checked
        //            {
        //                this._total = (float)value;
        //            }
        //        }
        //    }

        //    public double Avg
        //    {
        //        get
        //        {
        //            return ( this.total / (double)this.count );
        //        }
        //    }

        //    public void AddShip(double value)
        //    {
        //        Min = Math.Min(Min, value);
        //        Max = Math.Max(Max, value);
        //        this.total += value;
        //        ++this.count;
        //    }
        //}

        #endregion //new
    }
}
