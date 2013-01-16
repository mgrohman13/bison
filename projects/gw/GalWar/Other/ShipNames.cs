using System;
using System.Text.RegularExpressions;
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
        private static readonly ShipClass[] deathStar;

        static ShipNames()
        {
            //these arrays all need to be the same length
            attack = new ShipClass[] { ShipClass.Destroyer, ShipClass.Cruiser, ShipClass.Battlecruiser, ShipClass.Battleship, ShipClass.Dreadnought, ShipClass.Excalibur };
            defense = new ShipClass[] { ShipClass.Warrior, ShipClass.Defender, ShipClass.Ironclad, ShipClass.Armor, ShipClass.Guardian, ShipClass.Avatar };
            speed = new ShipClass[] { ShipClass.Scout, ShipClass.Fighter, ShipClass.Corvette, ShipClass.Frigate, ShipClass.Ranger, ShipClass.Phoenix };
            transport = new ShipClass[] { ShipClass.Galley, ShipClass.Carrack, ShipClass.Galleon, ShipClass.Transport, ShipClass.Invader, ShipClass.Reaper };
            deathStar = new ShipClass[] { ShipClass.Catapult, ShipClass.Trebuchet, ShipClass.Cannon, ShipClass.DeathStar, ShipClass.Exterminator, ShipClass.Demon };
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

        internal byte GetName(ShipDesign design, double attDefStr, double transStr, double speedStr, bool anomalyShip)
        {
            return (byte)GetNameType(design, attDefStr, transStr, speedStr, anomalyShip);
        }

        internal byte GetMark(Player player, byte name)
        {
            checked
            {
                return ++this._marks[player.ID, (int)name];
            }
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
            else if (design.BombardDamage * design.Speed > RandMult(Consts.GetBombardDamage(attDefStr) * speedStr * 26))
                type = deathStar;
            else if (design.Speed > RandMult(speedStr * 1.3))
                type = speed;
            else if (design.Def > RandMult(design.Att * 1.3))
                type = defense;
            else
                type = attack;

            int value = RandValue(ShipDesign.GetTotCost(design.Att, design.Def, design.HP, design.Speed, design.Trans, design.Colony, design.BombardDamage, 0), 1);

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
            Regex r = new Regex("(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])");
            return r.Replace(( (ShipClass)name ).ToString(), " ${x}") + " " + NumberToRoman(mark);
        }

        private static readonly byte[] values = new byte[] { 90, 50, 40, 10, 9, 5, 4, 1 };
        private static readonly string[] numerals = new string[] { "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

        private static string NumberToRoman(byte mark)
        {
            if (mark > 99)
                return mark.ToString();

            string result = string.Empty;
            for (int i = 0 ; i < values.Length ; ++i)
                while (mark >= values[i])
                {
                    mark -= values[i];
                    result += numerals[i];
                }
            return result;
        }

        private enum ShipClass : byte
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
            DeathStar,
            Exterminator,
            Demon,
            MAX
        }
    }
}
