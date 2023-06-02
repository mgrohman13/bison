using HarmonyLib;
using LevelGeneration;
using MattUtil;
using System;
using System.Reflection;
using UnityEngine;
using WarpipsReplayability.Mod;

namespace WarpipsReplayability.Patches
{
    [HarmonyPatch(typeof(DifficultyBar))]
    [HarmonyPatch(nameof(DifficultyBar.BuildDifficultyBar))]
    internal class DifficultyBar_BuildDifficultyBar
    {
        static DifficultyBar_BuildDifficultyBar()
        {
            DisplayThreshold = float.NaN;
        }
        public static float DisplayThreshold
        {
            get;
            set;
        }

        private static readonly FieldInfo _displayThreshold = AccessTools.Field(typeof(SpawnWaveProfile), "displayThreshold");

        public static bool Prefix(SpawnWaveProfile waveProfile, Texture2D ___barTexture, Transform ___warningHolder, ref bool ___ignoreCycleDifficulty)//, ref List<GameObject> ___bombIndicatorPrefabs)
        {
            bool showBar = true;
            try
            {
                Plugin.Log.LogDebug("DifficultyBar_BuildDifficultyBar Prefix");

                ////copy the HotbarDifficultyIndicatorController bombIndicatorPrefabs to OperationDetailsController
                ////this way you can see bombs on the island map
                //ManageBombIndicatorPrefabs(ref ___bombIndicatorPrefabs);

                //if (___bombIndicatorPrefabs == null)
                ___ignoreCycleDifficulty = true;

                showBar = Operations.ShowEnemies();
                if (showBar && waveProfile.DisplayThreshold != DisplayThreshold)
                {
                    Plugin.Log.LogInfo($"displayThreshold: {waveProfile.DisplayThreshold} -> {DisplayThreshold}");
                    //normalize the display threshold so the relative difficulty of different missions is more apparent 
                    _displayThreshold.SetValue(waveProfile, DisplayThreshold);
                }
                else
                {
                    for (int i = 0; i < ___barTexture.width; i++)
                        ___barTexture.SetPixel(i, 0, Color.black);
                    ___barTexture.Apply();
                    foreach (var warning in ___warningHolder)
                        DifficultyBar.Destroy(((Transform)warning).gameObject);

                    Plugin.Log.LogDebug("hiding difficulty bar");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
            return showBar;
        }

        public static void Postfix(SpawnWaveProfile waveProfile, Texture2D ___barTexture)
        {
            try
            {
                Plugin.Log.LogDebug("DifficultyBar_BuildDifficultyBar Postfix");

                if (Operations.ShowEnemies())
                {
                    MTRandom deterministic = new(SpawnWaveProfile_ReturnAllWarningMessageTimings.GenerateSeed(waveProfile));
                    float mult = ___barTexture.width / waveProfile.RoundDuration;
                    for (int a = 1; a < (int)Math.Round(waveProfile.RoundDuration); a++)
                    {
                        int b = deterministic.Round(a * mult);
                        ___barTexture.SetPixel(b, 0, Color.white);
                    }
                    ___barTexture.Apply();
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        //private static List<GameObject> bombIndicatorPrefabs;
        //private static void ManageBombIndicatorPrefabs(ref List<GameObject> ___bombIndicatorPrefabs)
        //{
        //    List<GameObject> bombs = ___bombIndicatorPrefabs;
        //    if (bombIndicatorPrefabs == null && bombs != null && bombs.Any() && bombs.All(b => b != null))
        //    {
        //        Plugin.Log.LogInfo("storing off bombIndicatorPrefabs");
        //        Plugin.Log.LogInfo($"{SpawnWaveProfile.BombTimings.Aggregate("", (a, b) => a + " " + b)}");
        //        bombIndicatorPrefabs = bombs.ToList();
        //    }
        //    else if (bombIndicatorPrefabs != null)
        //    {
        //        Plugin.Log.LogInfo("placing bombIndicatorPrefabs");
        //        bombs = bombIndicatorPrefabs.Select(b => GameObject.Instantiate(b)).ToList();
        //        foreach (var b in bombs)
        //        {
        //            const float scale = -1 / 3f;
        //            //clone transform?? fix every call?
        //            b.transform.localScale += new Vector3(scale, scale, scale);
        //            Plugin.Log.LogInfo(b.transform.GetInstanceID());
        //        }
        //        ___bombIndicatorPrefabs = bombs;
        //    }
        //} 
    }
}
