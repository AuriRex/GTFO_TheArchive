using CellMenu;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL
{
    public class IntroSkip : Feature
    {
        public override string Name => "Skip Intro";

        public override string Group => FeatureGroups.QualityOfLife;

        public new static IArchiveLogger FeatureLogger { get; set; }

        [ArchivePatch(typeof(CM_PageIntro), "Update")]
        internal static class CM_PageIntro_UpdatePatch
        {
#if MONO
            private static PropertyAccessor<RundownManager, bool> A_RundownManager_RundownProgressionReady;

            [RundownConstraint(Utils.RundownFlags.RundownFour)]
            public static void Init()
            {
                A_RundownManager_RundownProgressionReady = PropertyAccessor<RundownManager, bool>.GetAccessor("RundownProgressionReady");
            }
#endif

            private static bool _injectPressed = false;

#if IL2CPP
            [IsPostfix, RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.Latest)]
            public static void Postfix(CM_PageIntro __instance)
            {
                CheckAndSkipIfReady(__instance, __instance.m_startupLoaded, __instance.m_enemiesLoaded, __instance.m_sharedLoaded);
            }
#endif

#if MONO
            [IsPostfix, RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFour)]
#endif
            public static void CheckAndSkipIfReady(CM_PageIntro __instance, bool ___m_startupLoaded, bool ___m_enemiesLoaded, bool ___m_sharedLoaded)
            {
                if (___m_startupLoaded
                    && ___m_enemiesLoaded
                    && ___m_sharedLoaded
                    && ItemSpawnManager.ReadyToSpawnItems
                    && IsProgressionFileReady()
                    && !_injectPressed)
                {
                    FeatureLogger.Notice("Automatically pressing the Inject button ...");
                    __instance.EXT_PressInject(-1);
                    _injectPressed = true;
                }
            }

            public static bool IsProgressionFileReady()
            {
#if MONO
                if(BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownFour))
                    return A_RundownManager_RundownProgressionReady.Get(null);
                return R3BelowIsProgressionFileReady();
#else
                return RundownManager.RundownProgressionReady;
#endif
            }

#if MONO
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static bool R3BelowIsProgressionFileReady()
            {
                return RundownManager.PlayerRundownProgressionFileReady;
            }
#endif
        }
    }
}
