using Gear;
using HarmonyLib;
using System;
using System.IO;
using TheArchive.Utilities;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    class CELLSettingsPatches
    {
        [HarmonyPatch(typeof(CellSettingsManager))]
        [HarmonyPatch("SettingsPath", MethodType.Getter)]
        public static class CellSettingsManager_SettingsPathPatch
        {
            public static void Postfix(ref string __result)
            {
                if (File.Exists(__result) && !File.Exists(LocalFiles.SettingsPath))
                {
                    try
                    {
                        ArchiveLogger.Notice($"Local Settings files doesn't exist, copying from official location.");
                        File.Copy(__result, LocalFiles.SettingsPath);
                    }
                    catch (Exception ex)
                    {
                        ArchiveLogger.Error($"This shouldn't have happened: {ex.Message}");
                        ArchiveLogger.Error($"{ex.StackTrace}");
                    }
                }

                __result = LocalFiles.SettingsPath;
            }
        }

        [HarmonyPatch(typeof(GearManager))]
        [HarmonyPatch("FavoritesPath", MethodType.Getter)]
        public static class GearManager_FavoritesPathPatch
        {
            public static void Postfix(GearManager __instance, ref string __result)
            {
                if (File.Exists(__result) && !File.Exists(LocalFiles.FavoritesPath))
                {
                    try
                    {
                        ArchiveLogger.Notice($"Local Favorites files doesn't exist, copying from official location.");
                        File.Copy(__result, LocalFiles.FavoritesPath);
                    }
                    catch(Exception ex)
                    {
                        ArchiveLogger.Error($"This shouldn't have happened: {ex.Message}");
                        ArchiveLogger.Error($"{ex.StackTrace}");
                    }
                }

                __result = LocalFiles.FavoritesPath;
            }
        }

    }
}
