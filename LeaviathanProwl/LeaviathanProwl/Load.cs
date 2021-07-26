using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Reflection;

namespace LeaviathanProwl
{
    public class Load
    {
        public static void LoadAndPatch()
        {
            Config.Load();

            var harmony = HarmonyInstance.Create("leaviathanprowl.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());


        }
    }
}
