using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Utilities;

namespace TheArchive.Features.Audio;

[RundownConstraint(Utils.RundownFlags.RundownFive, Utils.RundownFlags.Latest)]
public class DisableRespawnSackAudio : Feature
{
    public override string Name => "Disable Respawn Sack Audio";

    public override GroupBase Group => GroupManager.Audio;

    public override string Description => "Prevents Respawn Sacks from emitting audio.";

#if IL2CPP
    public override void OnEnable()
    {
        foreach (var ervw in GetAllEnemyRespawnerVisuals())
        {
            ervw.enemyRespawnerVisual.OnDestroy(); // Stops the sound and recycles it if it exists
        }
    }

    public override void OnDisable()
    {
        foreach (var ervw in GetAllEnemyRespawnerVisuals())
        {
            var erv = ervw.enemyRespawnerVisual;
            erv.OnDestroy(); // Stops the sound and recycles it if it exists
            erv.Start();
        }
    }

    // Exists so it doesn't blow up on R4 lol
    private class EnemyRespawnerVisualWrapper
    {
        public EnemyRespawnerVisual enemyRespawnerVisual;
    }

    private static IEnumerable<EnemyRespawnerVisualWrapper> GetAllEnemyRespawnerVisuals()
    {
        foreach (var zone in SharedUtils.GetAllZones())
        {
            foreach (var area in zone.m_areas)
            {
                foreach (var erv in area.GetComponentsInChildren<EnemyRespawnerVisual>())
                {
                    yield return new EnemyRespawnerVisualWrapper()
                    {
                        enemyRespawnerVisual = erv
                    };
                }
            }
        }
    }

    [ArchivePatch(typeof(EnemyRespawnerVisual), UnityMessages.Start)]
    internal static class EnemyRespawnerVisual_Start_Patch
    {
        public static bool Prefix()
        {
            return ArchivePatch.SKIP_OG;
        }
    }
#endif
}