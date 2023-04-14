using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HarmonyLib;

namespace WarpipsReplayability.Mod
{
    internal class Config
    {
        private const string configFile = "BepInEx/plugins/WarpipsReplayability.txt";

        static Config()
        {
            try
            {
                foreach (var row in File.ReadAllLines(configFile))
                    if (!row.StartsWith("#"))
                    {
                        string[] split = row.Split('=');
                        if (split.Length > 1)
                        {
                            string key = split[0];
                            string value = string.Join("=", split.Skip(1).ToArray());

                            switch (key)
                            {
                                case "PlayerLives":
                                    if (int.TryParse(value, out int playerLives))
                                        PlayerLives = playerLives;
                                    break;
                                case "FixArmsDealer":
                                    FixArmsDealer = value != "false";
                                    break;
                                case "RebalanceTech":
                                    RebalanceTech = value != "false";
                                    break;
                                case "DifficultMode":
                                    DifficultMode = value == "true";
                                    break;
                                default:
                                    Plugin.Log.LogError($"Unknown property: {key}={value}");
                                    break;
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
                Plugin.Log.LogError("Unable to read config file, using defaults");
            }

            Plugin.Log.LogInfo($@"settings:
                PlayerLives={PlayerLives}
                FixArmsDealer={FixArmsDealer}
                RebalanceTech={RebalanceTech}
                DifficultMode={DifficultMode}");
        }

        public static readonly int? PlayerLives = null;
        public static readonly bool FixArmsDealer = true;
        public static readonly bool RebalanceTech = true;
        public static readonly bool DifficultMode = false;
    }
}
