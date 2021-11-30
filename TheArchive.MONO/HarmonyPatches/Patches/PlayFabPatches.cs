using System;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class PlayFabPatches
    {
        [ArchivePatch(typeof(PlayFabManager), "TryGetRundownTimerData", Utils.RundownFlags.RundownTwo | Utils.RundownFlags.RundownThree)]
        internal static class PlayFabManager_TryGetRundownTimerDataPatch
        {
            public static bool Prefix(ref bool __result, out RundownTimerData data)
            {
                data = new RundownTimerData();
                data.ShowScrambledTimer = true;
                data.ShowCountdownTimer = true;
                DateTime theDate = DateTime.Today.AddDays(5);
                data.UTC_Target_Day = theDate.Day;
                data.UTC_Target_Hour = theDate.Hour;
                data.UTC_Target_Minute = theDate.Minute;
                data.UTC_Target_Month = theDate.Month;
                data.UTC_Target_Year = theDate.Year;

                __result = true;
                return false;
            }
        }

        [ArchivePatch(typeof(PlayfabMatchmakingManager), "CancelAllTicketsForLocalPlayer", Utils.RundownFlags.RundownTwo, Utils.RundownFlags.RundownThree)]
        internal static class PlayfabMatchmakingManager_CancelAllTicketsForLocalPlayerPatch
        {
            public static bool Prefix()
            {
                return false;
            }
        }

    }
}
