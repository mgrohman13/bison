using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using GameUI;
using System.Runtime.CompilerServices;
using LevelGeneration.WorldMap;
using LevelGeneration;
using WarpipsReplayability.Mod;
using DynamicEnums;
using UnityEngine;
using System.Reflection;
using System.Collections;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(DifficultyBar))]
    [HarmonyPatch(nameof(DifficultyBar.BuildDifficultyBar))]
    internal class DifficultyBar_BuildDifficultyBar
    {
        public static bool Prefix(Texture2D ___barTexture, Transform ___warningHolder)
        {
            bool showBar = true;
            try
            {
                Plugin.Log.LogDebug("DifficultyBar_BuildDifficultyBar Prefix");

                showBar = Operations.ShowDifficultyBar();

                if (!showBar)
                {
                    for (int i = 0; i < ___barTexture.width; i++)
                        ___barTexture.SetPixel(i, 0, Color.black);
                    ___barTexture.Apply();
                    foreach (var warning in ___warningHolder)
                        DifficultyBar.Destroy(((Transform)warning).gameObject);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
            return showBar;
        }
    }
}
