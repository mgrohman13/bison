using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    internal class ShipNames
    {
        private const int length = 6;

        private static readonly ShipClass[] attack;
        private static readonly ShipClass[] defense;
        private static readonly ShipClass[] speed;
        private static readonly ShipClass[] transport;

        static ShipNames()
        {
            //these arrays all need to be the same length
            attack = new ShipClass[] { ShipClass.Destroyer, ShipClass.Cruiser, ShipClass.Battlecruiser, ShipClass.Battleship, ShipClass.Dreadnought, ShipClass.Excalibur };
            defense = new ShipClass[] { ShipClass.Warrior, ShipClass.Defender, ShipClass.Ironclad, ShipClass.Armor, ShipClass.Guardian, ShipClass.Avatar };
            speed = new ShipClass[] { ShipClass.Scout, ShipClass.Fighter, ShipClass.Corvette, ShipClass.Frigate, ShipClass.Ranger, ShipClass.Phoenix };
            transport = new ShipClass[] { ShipClass.Galley, ShipClass.Carrack, ShipClass.Galleon, ShipClass.Transport, ShipClass.Invader, ShipClass.Reaper };
        }

        private int[] divisions;
        private byte[,] _marks;

        private bool _setup;
        private ushort _total;
        private byte _count;

        internal ShipNames(int numPlayers)
        {
            this.divisions = new int[length];
            for (int a = 2 ; a < length ; ++a)
                this.divisions[a] = int.MaxValue;

            this._marks = new byte[numPlayers, (int)ShipClass.MAX];

            this._setup = true;
            this._total = 0;
            this._count = 0;
        }

        private ShipClass DoSetup(ShipClass[] type, int value)
        {
            //during setup phase, the first name is always used, and the average cost calculated
            checked
            {
                this._total += (ushort)value;
                ++this._count;
            }
            return type[0];
        }

        internal void EndSetup()
        {
            this._setup = false;
            //first division is set using the average
            SetDivision(1, this._total / (double)this._count);
        }

        internal byte GetName(Player player, ShipDesign design, double transStr, double speedStr)
        {
            return (byte)GetNameType(design, transStr, speedStr);
        }

        internal byte GetMark(Player player, byte name)
        {
            checked
            {
                return ++this._marks[player.ID, (int)name];
            }
        }

        private ShipClass GetNameType(ShipDesign design, double transStr, double speedStr)
        {
            if (design.Colony)
                return ShipClass.Colony;

            //determine which array to use
            ShipClass[] type;
            if (design.Trans > RandMult(transStr * .3))
                type = transport;
            else if (design.Speed > RandMult(speedStr * 1.3))
                type = speed;
            else if (design.Def > RandMult(design.Att * 1.3))
                type = defense;
            else
                type = attack;

            int value = RandValue(ShipDesign.GetTotCost(design.Att, design.Def, design.HP, design.Speed, design.Trans, design.Colony, design.BombardDamageMult, 0), 1);

            if (this._setup)
                return DoSetup(type, value);

            return GetName(type, value);
        }

        private ShipClass GetName(ShipClass[] type, int value)
        {
            //find the highest division it matches
            for (int i = length ; --i > -1 ; )
                if (value > this.divisions[i])
                {
                    //check if this is a breakthrough design, and if so use its value for the next division
                    int next = i + 1;
                    if (next < length && this.divisions[next] == int.MaxValue)
                        SetDivision(next, value);
                    return type[i];
                }
            throw new Exception();
        }

        private void SetDivision(int index, double value)
        {
            this.divisions[index] = RandValue(value * 3.0, Game.Random.Round(value * 1.3) + 13);
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

        internal static string GetName(byte name, byte mark)
        {
            return ( (ShipClass)name ).ToString() + " " + NumberToRoman(mark);
        }

        private static readonly int[] values = new int[] { 90, 50, 40, 10, 9, 5, 4, 1 };
        private static readonly string[] numerals = new string[] { "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

        private static string NumberToRoman(int number)
        {
            if (number < 100)
            {
                string result = string.Empty;
                for (int i = 0 ; i < values.Length ; ++i)
                    while (number >= values[i])
                    {
                        number -= values[i];
                        result += numerals[i];
                    }
                return result;
            }
            else
            {
                return number.ToString();
            }
        }

        private enum ShipClass : byte
        {
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
            MAX
        }
    }
}
