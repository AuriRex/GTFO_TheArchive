using Enemies;
using GameData;
using IRF;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Interfaces;
using UnityEngine;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
public class DecayIRFNREFix : Feature
{
    public override string Name => "Decay IRF NRE Fix";

    public override Core.FeaturesAPI.Groups.GroupBase Group => GroupManager.Fixes;

    public override string Description => "Fixes enemies with invalid IRFs spamming the console on death.\n\n<i>Specifically 'tank_boss'</i>";
    
    public new static IArchiveLogger FeatureLogger { get; set; }
    
    [ArchivePatch(typeof(EnemyPrefabManager), nameof(EnemyPrefabManager.BuildEnemyPrefab))]
    public static class EnemyPrefabManager__BuildEnemyPrefab__Patch
    {
        public static void Postfix(EnemyDataBlock data, ref GameObject __result)
        {
            var generatedEnemyPrefab = __result;
        
            var irfs = generatedEnemyPrefab.GetComponentsInChildren<InstancedRenderFeature>(includeInactive: true);

            foreach (var irf in irfs)
            {
                if (irf.Descriptor != null)
                    continue;
            
                FeatureLogger.Debug($"Found invalid IRF on enemy '{data.name}' (pID: {data.persistentID}), removing ...");
                UnityEngine.Object.Destroy(irf);
            }
        }
    }
}