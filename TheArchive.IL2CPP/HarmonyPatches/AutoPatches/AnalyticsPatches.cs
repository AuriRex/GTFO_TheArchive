using GameEvent;
using HarmonyLib;
using SNetwork;
using System;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class AnalyticsPatches
    {

        // Disables analytic reporting
        [HarmonyPatch(typeof(AnalyticsManager), "OnGameEvent", typeof(GameEventData))]

        internal static class AnalyticsManager_OnGameEventPatch
        {
            public static bool Prefix() => false;
        }

        // Disables Steam rich presence
        [HarmonyPatch(typeof(SNet_Core_STEAM), "SetFriendsData", new Type[] { typeof(FriendsDataType), typeof(string) })]
        internal static class SNet_Core_STEAM_SetFriendsDataPatch
        {
            public static void Prefix(ref string data) => data = "";
        }

    }
}
