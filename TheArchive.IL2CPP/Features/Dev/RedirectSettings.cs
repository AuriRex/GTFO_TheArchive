using Gear;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Dev
{
    [EnableFeatureByDefault, HideInModSettings]
    public class RedirectSettings : Feature
    {
        public override string Name => "Redirect Settings";

        public override bool RequiresRestart => true;

        public override string Group => FeatureGroups.Dev;

#if IL2CPP
        [ArchivePatch(typeof(Il2CppSystem.IO.Path), nameof(Il2CppSystem.IO.Path.Combine), new Type[] { typeof(string), typeof(string) })]
        public static class Il2CppSystem_IO_Path_Combine_Patch
        {
            public static bool Prefix(ref string __result, ref string path1, ref string path2)
            {
                switch (path2)
                {
                    case "GTFO_Settings.txt":
                        __result = LocalFiles.SettingsPath;
                        return ArchivePatch.SKIP_OG;
                    case "GTFO_Favorites.txt":
                        __result = LocalFiles.FavoritesPath;
                        return ArchivePatch.SKIP_OG;
                    case "GTFO_BotFavorites.txt":
                        __result = LocalFiles.BotFavoritesPath;
                        return ArchivePatch.SKIP_OG;
                    default:
                        return ArchivePatch.RUN_OG;
                }
            }
        }
#else
        [ArchivePatch(typeof(CellSettingsManager), "SettingsPath", patchMethodType: ArchivePatch.PatchMethodType.Getter)]
        public static class CellSettingsManager_SettingsPathPatch
        {
            public static void Postfix(ref string __result)
            {
                __result = LocalFiles.SettingsPath;
            }
        }

        [ArchivePatch(typeof(GearManager), "FavoritesPath", patchMethodType: ArchivePatch.PatchMethodType.Getter)]
        public static class GearManager_FavoritesPathPatch
        {
            public static void Postfix(ref string __result)
            {
                __result = LocalFiles.FavoritesPath;
            }
        }
#endif

    }
}
