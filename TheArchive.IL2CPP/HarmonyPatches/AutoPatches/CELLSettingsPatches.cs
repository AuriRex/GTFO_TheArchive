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
    class CELLSettingsPatches
    {
        // has been inlined into CellSettingsManager.Setup() in IL2CPP builds
        [HarmonyPatch(typeof(CellSettingsManager))]
        [HarmonyPatch("SettingsPath", MethodType.Getter)]
        public static class CellSettingsManager_SettingsPathPatch
        {
            public static bool Prefix(ref string __result)
            {
                __result = LocalFiles.SettingsPath;
                return false;
            }
        }

        // has been inlined into GearManager.Setup() in IL2CPP builds
        [HarmonyPatch(typeof(GearManager))]
        [HarmonyPatch("FavoritesPath", MethodType.Getter)]
        public static class GearManager_FavoritesPathPatch
        {
            public static bool Prefix(GearManager __instance, ref string __result)
            {
                __result = LocalFiles.FavoritesPath;
                return false;
            }
        }

        [HarmonyPatch(typeof(CellSettingsManager), nameof(CellSettingsManager.Setup))]
        public static class CellSettingsManager_SetupPatch
        {
            public static void Postfix()
            {
                CellSettingsManager.SettingsData = CellJSON.ReadFromDisk<CellSettingsData>(LocalFiles.SettingsPath, out var createdNew);
                var hasDirtySettings = createdNew;
                hasDirtySettings |= !createdNew && CellSettingsManager.SettingsData.CheckVersions();
                hasDirtySettings |= CellSettingsManager.SettingsData.KeyBindings.AddDefaultBindingsForUnboundActions();
                CellSettingsManager.HasDirtySettings = hasDirtySettings;
                if (hasDirtySettings)
                {
                    CellSettingsManager_TrySaveSettingsPatch.Prefix();
                }
            }
        }

        public static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[]
            {
                new ColorConverter(),
                new StringEnumConverter()
            }
        };

        //SaveFavoritesData
        [HarmonyPatch(typeof(GearManager), nameof(GearManager.SaveFavoritesData))]
        public static class GearManager_SaveFavoritesDataPatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.Red, $"Saving Favorites file to: {LocalFiles.FavoritesPath}");
                CellJSON.SaveToDisk(LocalFiles.FavoritesPath, GearManager.FavoritesData);
                return false;
            }
        }

        [HarmonyPatch(typeof(CellSettingsManager), nameof(CellSettingsManager.TrySaveSettings))]
        public static class CellSettingsManager_TrySaveSettingsPatch
        {
            public static bool Prefix()
            {
                if (ArchiveModule.CurrentRundownID == 0) return false;

                ArchiveLogger.Msg(ConsoleColor.Red, $"Saving Settings file to: {LocalFiles.SettingsPath}");

                // Reflection it is because VS doesn't let me call the method
                try
                {
                    typeof(CellJSON).GetMethods(AccessTools.all).First(mb => mb.Name == nameof(CellJSON.SaveToDisk) && mb.GetParameters().Length == 3).MakeGenericMethod(typeof(CellSettingsData)) .Invoke(null, new object[] { LocalFiles.SettingsPath, CellSettingsManager.SettingsData, null });
                    CellSettingsManager.HasDirtySettings = false;
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Error(ex.Message);
                    ArchiveLogger.Error(ex.StackTrace);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(GearManager), nameof(GearManager.Setup))]
        public static class GearManager_SetupPatch
        {
            public static void Postfix(GearManager __instance)
            {
                GearManager.FavoritesData = CellJSON.ReadFromDisk<GearFavoritesData>(LocalFiles.FavoritesPath, out var _);
            }
        }

    }
}
