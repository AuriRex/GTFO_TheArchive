using Gear;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Linq;
using TheArchive.Utilities;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class GameSettingsPatches
    {

        [HarmonyPatch(typeof(Il2CppSystem.IO.Path), nameof(Il2CppSystem.IO.Path.Combine), new Type[] { typeof(string), typeof(string) })]
        public static class TestPatch
        {
            public static bool Prefix(ref string __result, ref string path1, ref string path2)
            {
                switch(path2)
                {
                    case "GTFO_Settings.txt":
                        __result = LocalFiles.SettingsPath;
                        return false;
                    case "GTFO_Favorites.txt":
                        __result = LocalFiles.FavoritesPath;
                        return false;
                    case "GTFO_BotFavorites.txt":
                        __result = LocalFiles.BotFavoritesPath;
                        return false;
                    default:
                        return true;
                }
            }
        }

    }
}
