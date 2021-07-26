using System;
using System.IO;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using UWE;
using Oculus.Newtonsoft.Json;

namespace LeaviathanProwl.Patches
{
    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch(nameof(SaveLoadManager.SaveToTemporaryStorageAsync))]
    [HarmonyPatch(new Type[] { typeof(Texture2D) })]
    class SaveLoadManager_SaveToTemporaryStorageAsync
    {
        public static void Postfix()
        {
            Logger.LogInfo("SaveLoadManager_SaveToTemporaryStorageAsync");

            string path = SaveLoadManager_ClearSlotAsync.GetFilePath();
            File.Create(path).Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(LeviathanPool.instance, Formatting.Indented));

            Logger.LogAllFields(LeviathanPool.instance);
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch(nameof(SaveLoadManager.LoadSlotsAsync))]
    [HarmonyPatch(new Type[] { })]
    class SaveLoadManager_LoadSlotsAsync
    {
        public static void Postfix()
        {
            Logger.LogInfo("SaveLoadManager_LoadSlotsAsync");

            string path = SaveLoadManager_ClearSlotAsync.GetFilePath();
            if (File.Exists(path))
                LeviathanPool.instance = JsonConvert.DeserializeObject<LeviathanPool>(File.ReadAllText(path));

            Logger.LogAllFields(LeviathanPool.instance);
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch(nameof(SaveLoadManager.LoadAsync))]
    [HarmonyPatch(new Type[] { })]
    class SaveLoadManager_LoadAsync
    {
        public static void Postfix()
        {
            Logger.LogInfo("SaveLoadManager_LoadAsync");

            SaveLoadManager_LoadSlotsAsync.Postfix();
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch(nameof(SaveLoadManager.ClearSlotAsync))]
    [HarmonyPatch(new Type[] { typeof(string) })]
    class SaveLoadManager_ClearSlotAsync
    {
        public static void Postfix(string slotName)
        {
            Logger.LogInfo("SaveLoadManager_ClearSlotAsync");

            string path = GetFilePath(slotName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public static string GetFilePath()
        {
            return GetFilePath(SaveLoadManager.main.GetCurrentSlot());
        }

        public static string GetFilePath(string slotName)
        {
            string path = Config.path + "save_" + slotName + ".json";
            Logger.LogInfo(path);
            return path;
        }
    }
}
