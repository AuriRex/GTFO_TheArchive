using System;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Dev;

[EnableFeatureByDefault, HideInModSettings]
#if BepInEx
[ForceDisable("Not needed as BepInEx already redirects unity debug logs itself.")]
#endif
internal class GameDebugLogRedirect : Feature
{
    public override string Name => "Game Logs Redirect";

    public override GroupBase Group => GroupManager.Dev;

    public override string Description => "Prints Unity debug logs into the <i>MelonLoader</i> console.";

    //Log
#if IL2CPP
    [ArchivePatch(typeof(Debug), nameof(Debug.Log), new Type[] { typeof(Il2CppSystem.Object) })]
#else
    [ArchivePatch(typeof(Debug), nameof(Debug.Log), new Type[] { typeof(object) })]
#endif
    internal static class Debug__Log__Patch
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
    internal static class Debug__LogWarning__Patch
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
    internal static class Debug__LogError__Patch
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