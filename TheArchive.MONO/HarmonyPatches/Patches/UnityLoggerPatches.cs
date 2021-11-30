using System;
using TheArchive.Core.Core;
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
            static string lastMessage = string.Empty;
            static int lastTime = 0;
            public static void Prefix(string message)
            {
                int newTime = Time.frameCount;
                if (message.Equals(lastMessage) && lastTime == newTime) return;
                lastTime = Time.frameCount;
                lastMessage = message;
                GTFOLogger.Log(message);
            }
        }
        //LogWarning
        [ArchivePatch(typeof(Debug), nameof(Debug.LogWarning), new Type[] { typeof(object) })]
        internal static class Debug_LogWarningPatch
        {
            static string lastMessage = string.Empty;
            static int lastTime = 0;
            public static void Prefix(string message)
            {
                int newTime = Time.frameCount;
                if (message.Equals(lastMessage) && lastTime == newTime) return;
                lastTime = Time.frameCount;
                lastMessage = message;
                GTFOLogger.Warn(message);
            }
        }
        //LogError
        [ArchivePatch(typeof(Debug), nameof(Debug.LogError), new Type[] { typeof(object) })]
        internal static class Debug_LogErrorPatch
        {
            static string lastMessage = string.Empty;
            static int lastTime = 0;
            public static void Prefix(string message)
            {
                int newTime = Time.frameCount;
                if (message.Equals(lastMessage) && lastTime == newTime) return;
                lastTime = Time.frameCount;
                lastMessage = message;
                GTFOLogger.Error(message);
            }
        }
    }
}
