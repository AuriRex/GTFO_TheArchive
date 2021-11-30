using Globals;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    internal class RandomInfoPatches
    {
        [HarmonyPatch(typeof(SNet_GlobalManager), "PostSetup")]
        internal static class SNet_GlobalManager_PostSetupPatch
        {
            public static void Prefix() => ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"SNet_GlobalManager - PostSetup running");
        }

        [HarmonyPatch(typeof(SNet_GlobalManager), "Setup")]
        internal static class SNet_GlobalManager_SetupPatch
        {
            public static void Prefix() => ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"SNet_GlobalManager - Setup running");
        }


        [HarmonyPatch(typeof(Global), "SetupManagers")]
        internal static class Global_SetupManagersPatch
        {
            public static void Prefix(Global __instance, Dictionary<string, GlobalManager> ___m_ManagersAlwaysLoaded, Dictionary<string, GlobalManager> ___m_ManagersNotLoadedInSlim)
            {
                foreach (KeyValuePair<string, GlobalManager> keyValuePair in ___m_ManagersAlwaysLoaded)
                {
                    ArchiveLogger.Msg(ConsoleColor.Yellow, $"alwayson: {keyValuePair.Key} - {keyValuePair.Value}");
                }

                foreach (KeyValuePair<string, GlobalManager> keyValuePair in ___m_ManagersNotLoadedInSlim)
                {
                    ArchiveLogger.Msg(ConsoleColor.Yellow, $"notinslim: {keyValuePair.Key} - {keyValuePair.Value}");
                }
            }
        }

    }
}
