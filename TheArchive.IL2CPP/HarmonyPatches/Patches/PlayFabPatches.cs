using System;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class PlayFabPatches
    {
        // We can use RundownFlags.All here as all IL2CPP versions are R4 or up only
        [ArchivePatch(typeof(PlayFabManager), "TryGetRundownTimerData", Utils.RundownFlags.All)]
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

    }
}
