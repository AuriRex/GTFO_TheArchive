using CellMenu;
using TheArchive.Core;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.AutoSkipIntro))]
    public class AutoIntroPatches
    {
        [ArchivePatch(typeof(CM_PageIntro), "Update")]
        internal class CM_PageIntro_UpdatePatch
        {
            private static bool _injectPressed = false;
            public static void Postfix(CM_PageIntro __instance, bool ___m_startupLoaded, bool ___m_enemiesLoaded, bool ___m_sharedLoaded)
            {
                if (___m_startupLoaded
                    && ___m_enemiesLoaded
                    && ___m_sharedLoaded
                    && ItemSpawnManager.ReadyToSpawnItems
                    && RundownManager.PlayerRundownProgressionFileReady // probably gonna result in an exception on R4 mono builds :p
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
