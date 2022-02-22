using GameEvent;
using SNetwork;
using System;
using TheArchive.Core;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class AnalyticsPatches
    {
        // Disables analytic reporting
        [BindPatchToSetting(nameof(ArchiveSettings.DisableGameAnalytics), "GameAnalytics")]
        [ArchivePatch(typeof(AnalyticsManager), "OnGameEvent", new Type[] { typeof(GameEventData) })]

        internal static class AnalyticsManager_OnGameEventPatch
        {
            public static bool Prefix() => false;
        }

    }
}
