using System;
using TheArchive.Core.Core;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableLocalProgressionPatches), "LocalProgression")]
    public class PlayfabMatchmakingManagerPatches
    {


        [ArchivePatch(typeof(PlayfabMatchmakingManager), nameof(PlayfabMatchmakingManager.Setup))]
        public class PlayfabMatchmakingManager_SetupPatch
        {

            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.Red, $"Canceled {nameof(PlayfabMatchmakingManager)}.{nameof(PlayfabMatchmakingManager.Setup)}().");
                return false;
            }

        }

    }
}
