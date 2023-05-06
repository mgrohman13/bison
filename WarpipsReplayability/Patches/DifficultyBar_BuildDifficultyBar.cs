using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool Prefix(Texture2D ___barTexture, Transform ___warningHolder, SpawnWaveProfile waveProfile, ref bool ___ignoreCycleDifficulty)//, ref List<GameObject> ___bombIndicatorPrefabs)
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
                if (showBar)
                {
                    //BuildDifficultyBar(waveProfile); 
                    Plugin.Log.LogInfo($"displayThreshold: {waveProfile.DisplayThreshold} -> {DisplayThreshold}");
                    //normalize the display threshold so the relative difficulty of different missions is more apparent 
                    AccessTools.Field(typeof(SpawnWaveProfile), "displayThreshold").SetValue(waveProfile, DisplayThreshold);
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

        //public static void BuildDifficultyBar(SpawnWaveProfile waveProfile)
        //{
        //}

        //public static void BuildDifficultyBar(SpawnWaveProfile waveProfile)
        //{
        //    float stripeX = 0f;
        //    bool flag = false;
        //    List<WarningStripe> list = new List<WarningStripe>();
        //    int width = this.barTexture.width;
        //    for (int i = 0; i < width; i++)
        //    {
        //        float timeScalar = Mathf.Clamp01(((float)i) / ((float)width));
        //        float num7 = waveProfile.ReturnDifficultyAtNormalizedTime(timeScalar);
        //        float displayThreshold = waveProfile.DisplayThreshold;
        //        if (!this.ignoreCycleDifficulty)
        //        {
        //            num7 = Mathf.Clamp01(num7 + this.missionManager.CycleDifficultyMultipler);
        //        }
        //        float time = num7 / displayThreshold;
        //        if ((num7 > displayThreshold) && !flag)
        //        {
        //            flag = true;
        //            stripeX = timeScalar;
        //        }
        //        else if (((num7 < displayThreshold) & flag) || (flag && (i == (width - 1))))
        //        {
        //            flag = false;
        //            list.Add(new WarningStripe(stripeX, timeScalar - stripeX));
        //        }
        //        this.barTexture.SetPixel(i, 0, this.textureGradiant.Evaluate(time));
        //    }
        //    this.barTexture.Apply();
        //    using (IEnumerator enumerator = this.warningHolder.GetEnumerator())
        //    {
        //        while (enumerator.MoveNext())
        //        {
        //            Destroy(((Transform)enumerator.Current).gameObject);
        //        }
        //    }
        //    int index = 0;
        //    foreach (WarningStripe stripe in list)
        //    {
        //        WaveWarningStripeController component = Instantiate<GameObject>(this.warningStripe, this.warningHolder).GetComponent<WaveWarningStripeController>();
        //        if (component)
        //        {
        //            component.InitializeStripe(index, this.holderRect.rect.width * stripe.stripeX, this.holderRect.rect.width * stripe.stripeWidth);
        //            index++;
        //        }
        //    }
        //    foreach (float num12 in waveProfile.ReturnAllWarningMessageTimings())
        //    {
        //        Utilities.SetPositionWithinBar(Instantiate<GameObject>(this.warningIndicatorPrefab, this.warningHolder).transform, this.startLerp, this.endLerp, num12);
        //    }
        //    if (!this.missionManager.RunningTutorial && (this.missionManager.CycleIndex == waveProfile.bombsOnCycle))
        //    {
        //        for (int j = 0; j < this.bombIndicatorPrefabs.Count; j++)
        //        {
        //            float num14 = SpawnWaveProfile.BombTimings[j];
        //            float t = Mathf.Min((float)1f, (float)((((waveProfile.RoundDuration * num14) * 60f) + 10f) / (waveProfile.RoundDuration * 60f)));
        //            Utilities.SetPositionWithinBar(Instantiate<GameObject>(this.bombIndicatorPrefabs[j], this.warningHolder).transform, this.startLerp, this.endLerp, t);
        //        }
        //    }
        //}
    }
}
