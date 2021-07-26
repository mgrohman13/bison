using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalWar
{
    public interface IShipStats
    {
        double Cost
        {
            get;
        }
        int Upkeep
        {
            get;
        }
        bool Colony
        {
            get;
        }
        int CurTrans
        {
            get;
        }
        int MaxTrans
        {
            get;
        }
        double BombardDamage
        {
            get;
        }
        int CurSpeed
        {
            get;
        }
        int MaxSpeed
        {
            get;
        }
        int Att
        {
            get;
        }
        int Def
        {
            get;
        }
        int CurHP
        {
            get;
        }
        int MaxHP
        {
            get;
        }
        double GetUpkeepPayoff(Game game);
    }
}
