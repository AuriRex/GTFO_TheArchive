using CellMenu;
using TheArchive.Core;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.AutoSkipIntro))]
    public class AutoIntroPatches
    {
        [ArchivePatch(typeof(CM_PageIntro), nameof(CM_PageIntro.Update))]
        internal class CM_PageIntro_UpdatePatch
        {
            private static bool _injectPressed = false;
            public static void Postfix(CM_PageIntro __instance)
            {
                if(__instance.m_startupLoaded
                    && __instance.m_enemiesLoaded
                    && __instance.m_sharedLoaded
                    && ItemSpawnManager.ReadyToSpawnItems
                    && RundownManager.RundownProgressionReady
                    && !_injectPressed)
                {
                    ArchiveLogger.Notice("Automatically pressing the Inject button ...");
                    __instance.EXT_PressInject(-1);
                    _injectPressed = true;
                }
            }
        }

    }
}
