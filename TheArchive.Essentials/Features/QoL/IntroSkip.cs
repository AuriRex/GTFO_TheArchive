using CellMenu;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL;

public class IntroSkip : Feature
{
    public override string Name => "Skip Intro";

    public override FeatureGroup Group => FeatureGroups.QualityOfLife;

    public override string Description => "Automatically presses inject at the start of the game";

    public new static IArchiveLogger FeatureLogger { get; set; }


#if MONO
        private static PropertyAccessor<RundownManager, bool> A_RundownManager_RundownProgressionReady;

        public override void Init()
        {
            if (Is.R4OrLater)
                A_RundownManager_RundownProgressionReady = PropertyAccessor<RundownManager, bool>.GetAccessor("RundownProgressionReady");
        }
#endif

    public static bool IsProgressionFileReady()
    {
#if MONO
            if(Is.R4OrLater)
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

    [ArchivePatch(typeof(CM_PageIntro), UnityMessages.Update)]
    internal static class CM_PageIntro_Update_Patch
    {
        private static bool _injectPressed = false;

        private static MethodAccessor<CM_PageIntro> _onSkip;

        public static void Init()
        {
            _onSkip = MethodAccessor<CM_PageIntro>.GetAccessor("OnSkip");
        }

#if IL2CPP
        [IsPostfix, RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.Latest)]
        public static void PostfixR4OrLater(CM_PageIntro __instance)
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
                _onSkip.Invoke(__instance);
                __instance.EXT_PressInject(-1);
                _injectPressed = true;
            }
        }
    }
}