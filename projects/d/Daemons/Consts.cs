using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daemons
{
    public class Consts
    {
        public const double WinPoints = 78;

        public const int DaemonSouls = 666;

        public const double IndySoulMult = 1.6;
        public const int ArcherHits = 20;
        public const int ArcherDamage = 12;
        public const int InfantryHits = 25;
        public const int InfantryDamage = 8;
        public const int KnightHits = 30;
        public const int KnightDamage = 11;
        public const int IndyHits = 35;
        public const int IndyDamage = 10;

        public const double ProdRand = .117;
        public const double IndyRand = .39;
        public const double SoulRand = .078;

        public const double DmgPos = 1.17;
        public const double DmgNeg = .78;
        public const double DmgIndy = 1.03;
        public const double DmgDaemon = .97;

        public const double MoraleTurnPower = .39;
        public const double MoraleDaemonGain = 1.3;
        public const double MoraleMax = 1 - .0091;
        public const double MoraleCritical = .00117;

        public const double NoReserveBattles = .26;

        public static bool MoveLeft(Unit unit)
        {
            return ( unit.Movement > 0 && ( ( unit.Movement - 1 ) * unit.Regen + unit.Hits ) >= unit.HitsMax );
        }

        public static double GetMoraleTurns(double current, double target)
        {
            return Math.Log(Math.Log(target) / Math.Log(current)) / Math.Log(MoraleTurnPower);
        }

        public static double GetStrength(UnitType type, int hits, double damage)
        {
            return Math.Pow(hits * Math.Pow(damage +
                    ( type == UnitType.Daemon ? damage / 13.0 : type == UnitType.Archer ? damage / 39.0 : 0 ),
                    24 / 25.0) / 7.0, 2 / 3.0);
        }
    }
}
