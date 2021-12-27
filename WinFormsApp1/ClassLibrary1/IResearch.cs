using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    interface IResearch
    {
        Game Game { get; }
        int GetLevel();
        Research.Type GetType();
        int GetMinCost();
        int GetMaxCost();
        bool MakeType(Research.Type type);
        double GetMult(Research.Type type, double pow);
    }
}
