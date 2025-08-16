using AK;
using GameData;
using LevelGeneration;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Audio;

[RundownConstraint(RundownFlags.RundownFive, RundownFlags.Latest)]
public class DisableArtifactSoundLoop : Feature
{
    public override string Name => "Disable Artifact Sound Loop";

    public override FeatureGroup Group => FeatureGroups.Audio;

    public override string Description => "Removes the Artifacts idle audio loop.";

#if IL2CPP
    public override void OnEnable()
    {
        if (!DataBlocksReady)
            return;

        foreach(var iil in GetAllArtifacts())
        {
            var artifact = iil.TryCastTo<ArtifactPickup_Core>();

            artifact.m_sound.Recycle();
        }
    }

    public override void OnDisable()
    {
        var soundId = SoundEventCache.Resolve(nameof(EVENTS.ARTIFACT_EMITTER_LOOP));

        foreach (var iil in GetAllArtifacts())
        {
            var artifact = iil.TryCastTo<ArtifactPickup_Core>();

            artifact.m_sound = new CellSoundPlayer(artifact.transform.position);
            artifact.m_sound.Post(soundId, true);
        }
    }

    /// <summary>
    /// Cast to <seealso cref="ArtifactPickup_Core"/> 
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<ItemInLevel> GetAllArtifacts()
    {
        foreach(var zone in SharedUtils.GetAllZones())
        {
            foreach(var area in zone.m_areas)
            {
                foreach (var artifact in area.GetComponentsInChildren<ArtifactPickup_Core>())
                    yield return artifact;
            }
        }
    }

    [ArchivePatch(typeof(ArtifactPickup_Core), nameof(ArtifactPickup_Core.Setup), new System.Type[] { typeof(ArtifactCategory) })]
    internal static class ArtifactPickup_Core_Setup_Patch
    {
        public static void Postfix(ArtifactPickup_Core __instance)
        {
            __instance.m_sound.Recycle();
        }
    }
#endif
}