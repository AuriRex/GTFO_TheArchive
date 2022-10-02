using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Dev
{
    [EnableFeatureByDefault, HideInModSettings]
#if BepInEx
    [ForceDisable]
#endif
    public class GameDebugLogRedirect : Feature
    {
        public override string Name => "Game Logs Redirect";

        public override string Group => FeatureGroups.Dev;

        //Log
#if IL2CPP
        [ArchivePatch(typeof(Debug), nameof(Debug.Log), new Type[] { typeof(Il2CppSystem.Object) })]
#else
        [ArchivePatch(typeof(Debug), nameof(Debug.Log), new Type[] { typeof(object) })]
#endif
        internal static class Debug_LogPatch
        {
#if IL2CPP
            public static void Prefix(Il2CppSystem.Object message)
#else
            public static void Prefix(object message)
#endif
            {
                GTFOLogger.Log(message.ToString());
            }
        }

        //LogWarning
#if IL2CPP
        [ArchivePatch(typeof(Debug), nameof(Debug.LogWarning), new Type[] { typeof(Il2CppSystem.Object) })]
#else
        [ArchivePatch(typeof(Debug), nameof(Debug.LogWarning), new Type[] { typeof(object) })]
#endif
        internal static class Debug_LogWarningPatch
        {
#if IL2CPP
            public static void Prefix(Il2CppSystem.Object message)
#else
            public static void Prefix(object message)
#endif
            {
                GTFOLogger.Warn(message.ToString());
            }
        }

        //LogError
#if IL2CPP
        [ArchivePatch(typeof(Debug), nameof(Debug.LogError), new Type[] { typeof(Il2CppSystem.Object) })]
#else
        [ArchivePatch(typeof(Debug), nameof(Debug.LogError), new Type[] { typeof(object) })]
#endif
        internal static class Debug_LogErrorPatch
        {
#if IL2CPP
            public static void Prefix(Il2CppSystem.Object message)
#else
            public static void Prefix(object message)
#endif
            {
                GTFOLogger.Error(message.ToString());
            }
        }
    }
}
