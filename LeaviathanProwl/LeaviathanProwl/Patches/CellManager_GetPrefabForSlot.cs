using System;
using System.IO;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using UWE;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(CellManager))]
    [HarmonyPatch(nameof(CellManager.GetPrefabForSlot))]
    class CellManager_GetPrefabForSlot
    {
        public static Dictionary<TechType, string> leviathanIds = null;

        public static void Postfix(IEntitySlot slot, ref EntitySlot.Filler __result)
        {
            if (slot.IsCreatureSlot())
            {
                //TechType.None;// LeviathanPool.instance.DoSpawn(slot.GetBiomeType());
                TechType tech = LeviathanPool.instance.DoSpawn(slot.GetBiomeType(), true);
                if (tech != TechType.None)
                {
                    Logger.LogAllFields(slot);
                    FindCreatureIds();
                    __result = new EntitySlot.Filler
                    {
                        classId = leviathanIds[tech],
                        count = 1,
                    };
                }
            }
        }

        public static void FindCreatureIds()
        {
            if (leviathanIds == null)
            {
                leviathanIds = new Dictionary<TechType, string>();
                foreach (var pair in WorldEntityDatabase.main.infos)
                    foreach (TechType tech in new TechType[] { TechType.ReaperLeviathan, TechType.GhostLeviathan, TechType.GhostLeviathanJuvenile })
                        if (tech == pair.Value.techType)
                        {
                            leviathanIds.Add(tech, pair.Value.classId);
                            if (leviathanIds.Count == 3)
                                return;
                            else
                                break;
                        }
            }
        }
    }
}
