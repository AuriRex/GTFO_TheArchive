using GameEvent;
using SNetwork;
using System;
using TheArchive.Core;
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

        // Disables Steam rich presence
        [BindPatchToSetting(nameof(ArchiveSettings.DisableSteamRichPresence), "SteamRichPresence")]
        [ArchivePatch(typeof(SNet_Core_STEAM), "SetFriendsData", new Type[] { typeof(FriendsDataType), typeof(string) })]
        internal static class SNet_Core_STEAM_SetFriendsDataPatch
        {
            public static void Prefix(ref string data) => data = "";
        }

    }
}
