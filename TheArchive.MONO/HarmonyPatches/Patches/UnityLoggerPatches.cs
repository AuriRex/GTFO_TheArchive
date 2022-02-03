using System;
using TheArchive.Core;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.RedirectUnityDebugLogs), "UnityDebug")]
    public class UnityLoggerPatches
    {
        //Log
        [ArchivePatch(typeof(Debug), nameof(Debug.Log), new Type[] { typeof(object) })]
        internal static class Debug_LogPatch
        {
            public static void Prefix(string message)
            {
                GTFOLogger.Log(message);
            }
        }
        //LogWarning
        [ArchivePatch(typeof(Debug), nameof(Debug.LogWarning), new Type[] { typeof(object) })]
        internal static class Debug_LogWarningPatch
        {
            public static void Prefix(string message)
            {
                GTFOLogger.Warn(message);
            }
        }
        //LogError
        [ArchivePatch(typeof(Debug), nameof(Debug.LogError), new Type[] { typeof(object) })]
        internal static class Debug_LogErrorPatch
        {
            public static void Prefix(string message)
            {
                GTFOLogger.Error(message);
            }
        }
    }
}
