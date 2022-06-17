using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;

namespace TheArchive.Features
{
    [EnableFeatureByDefault]
    public class RedirectSettings : Feature
    {
        public override string Name => "Redirect Settings";


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
#endif

    }
}
