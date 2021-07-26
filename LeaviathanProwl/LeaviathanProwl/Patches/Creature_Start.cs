using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Harmony;
using UnityEngine;
using UWE;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch(nameof(Creature.Start))]
    class Creature_Start
    {
        public static void Postfix(Creature __instance)
        {
            //TODO: inspect leviathan numbers and make less but comparable
            //health modification from QCreatureConfig mod
            if (__instance is ReaperLeviathan || __instance is GhostLeviathan)
                LeviathanPool.instance.Add(__instance);
            else
            {
                //TODO:
                Vector3 player = Player.main.transform.position;
                Vector3 start = __instance.transform.position;

                string b = LargeWorld.main.GetBiome(__instance.transform.position);

                TechType tech = LeviathanPool.instance.DoSpawn(BiomeType.Unassigned, false);
                if (tech != TechType.None)
                {
                    Logger.LogInfo("try instantiate {0}", tech);
                    CellManager_GetPrefabForSlot.FindCreatureIds();
                    if (PrefabDatabase.TryGetPrefab(CellManager_GetPrefabForSlot.leviathanIds[tech], out GameObject prefab))
                    {
                        GameObject gameObject = UWE.Utils.InstantiateDeactivated(prefab, __instance.transform.localPosition, __instance.transform.localRotation);
                        gameObject.transform.SetParent(__instance.transform.parent, false);
                        gameObject.transform.localScale = __instance.transform.localScale;
                        gameObject.SetActive(true);
                        Logger.LogInfo("instantiate {0} {1} {2}", gameObject, gameObject.GetComponent<Creature>(), Vector3.Distance(player, start));
                    }
                }


            }

            if (__instance is ReaperLeviathan || __instance is GhostLeviathan || __instance is CrabSnake || __instance is CrabSquid)
                __instance.liveMixin.data.maxHealth = 100000;
            else if (__instance is SeaDragon)
                __instance.liveMixin.data.maxHealth = 1000000;
        }
    }
}
