using HarmonyLib;
using System;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    internal class UnityLoggerPatches
    {

        //Log
        [HarmonyPatch(typeof(Debug), nameof(Debug.Log), new Type[] { typeof(Il2CppSystem.Object) })]
        internal static class Debug_LogPatch
        {
            public static void Prefix(Il2CppSystem.Object message)
            {
                string msg = Il2CppSystem.String.Concat(message);
                GTFOLogger.Log(msg);
            }
        }
        //LogWarning
        [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), new Type[] { typeof(Il2CppSystem.Object) })]
        internal static class Debug_LogWarningPatch
        {
            public static void Prefix(Il2CppSystem.Object message)
            {
                string msg = Il2CppSystem.String.Concat(message);
                GTFOLogger.Warn(msg);
            }
        }
        //LogError
        [HarmonyPatch(typeof(Debug), nameof(Debug.LogError), new Type[] { typeof(Il2CppSystem.Object) })]
        internal static class Debug_LogErrorPatch
        {
            public static void Prefix(Il2CppSystem.Object message)
            {
                string msg = Il2CppSystem.String.Concat(message);
                GTFOLogger.Error(msg);
            }
        }

    }
}
