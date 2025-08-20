using Player;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Dev;

[HideInModSettings]
[DoNotSaveToConfig]
[AutomatedFeature]
internal class InternalUAudioListenerHelper : Feature
{
    public override string Name => nameof(InternalUAudioListenerHelper);

    public override FeatureGroup Group => FeatureGroups.Dev;

    public new static IArchiveLogger FeatureLogger { get; set; }

    public static void AddOrEnableAudioListener(GameObject go)
    {
        if (go == null) return;
        var audioListener = go.GetComponent<AudioListener>();
        if(audioListener == null)
        {
            go.AddComponent<AudioListener>();
            FeatureLogger.Success($"Added {nameof(AudioListener)} to local player! ({go.name})");
        }
    }

    public static void DestroyAudioListener(GameObject go)
    {
        if (go == null) return;
        var audioListener = go.GetComponent<AudioListener>();
        if(audioListener != null)
        {
            UnityEngine.Object.Destroy(audioListener);
            FeatureLogger.Fail($"Removed {nameof(AudioListener)} from local player! ({go.name})");
        }
    }

    public override void OnEnable()
    {
        var localPlayer = PlayerManager.GetLocalPlayerAgent();
        AddOrEnableAudioListener(localPlayer?.gameObject);
    }

    public override void OnDisable()
    {
        var localPlayer = PlayerManager.GetLocalPlayerAgent();
        DestroyAudioListener(localPlayer?.gameObject);
    }

    [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
    [ArchivePatch(typeof(PlayerAgent), nameof(PlayerAgent.Setup))]
    public static class PlayerAgent_Setup_Patch
    {
        public static void Postfix(PlayerAgent __instance)
        {
            AddOrEnableAudioListener(__instance?.gameObject);
        }
    }
#if IL2CPP
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    [ArchivePatch(null, nameof(LocalPlayerAgent.Setup))]
    public static class LocalPlayerAgent_Setup_Patch
    {
        public static Type Type() => typeof(LocalPlayerAgent);
        public static void Postfix(LocalPlayerAgent __instance)
        {
            AddOrEnableAudioListener(__instance?.gameObject);
        }
    }
#endif
}